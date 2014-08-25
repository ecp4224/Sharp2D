﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Sharp2D.Core;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Interfaces;

namespace Sharp2D
{
    public class Screen : GameWindow
    {
        public static ScreenSettings DEFAULT_SETTINGS { 
            get 
            {
                ScreenSettings settings = new ScreenSettings(null);
                settings.LogicTickRate = 40; //25 ticks/second
                settings.MaxSkippedFrames = 5; //Skip a max of 5 draw frames
                settings.GameSize = new System.Drawing.Rectangle(0, 0, 1280, 720); //720p
                settings.WindowSize = settings.GameSize;
                settings.Fullscreen = false;
                settings.VSync = false;
                settings.Camera = new Sharp2D.Game.Worlds.GenericCamera() {
                    Z = 100f
                };
                settings.WindowTitle = "Sharp2D";
                settings.UseOpenTKLoop = true;
                settings.MaxFPS = -1; //Max FPS

                return settings;
            } 
        }

        public static Camera Camera
        {
            get
            {
                return Settings.Camera;
            }
            set
            {
                Settings.Camera = value;
            }
        }

        public static Thread DisplayThread { get; private set; }

        public static bool IsRunning { get; private set; }

        public static ScreenSettings Settings { get; private set; }

        public static double FPS { get; private set; }

        public static double UPS { get; private set; }

        public static int TickCount
        {
            get
            {
                return Environment.TickCount - _tickAtStart;
            }
        }

        public static int SkipTicks
        {
            get
            {
                return 1000 / TicksPerSecond;
            }
        }

        public static int TicksPerSecond
        {
            get
            {
                return 1000 / Settings.LogicTickRate;
            }
        }

        public static ILogicContainer LogicContainer
        {
            set
            {
                lock (logic_lock)
                {
                    logics = value;
                }
            }
            get
            {
                return logics;
            }
        }

        public static IRenderJobContainer RenderJobContainer
        {
            set
            {
                lock (job_lock)
                {
                    renders = value;
                }
            }
            get
            {
                return renders;
            }
        }

        private static int _tickAtStart;
        private static Stack<Action> invokes = new Stack<Action>();
        private static ILogicContainer logics;
        private static IRenderJobContainer renders;
        private static GameWindow window;

        private static object job_lock = new object();
        private static object logic_lock = new object();

        public static void DisplayScreenAsync()
        {
            Settings = DEFAULT_SETTINGS;
            DisplayThread = new Thread(new ThreadStart(DisplayScreen));
            DisplayThread.Priority = ThreadPriority.Highest;
            DisplayThread.Name = "Display Thread";
            DisplayThread.Start();
        }

        public static void DisplayScreenAsync(ScreenSettings settings)
        {
            Settings = settings;
            DisplayThread = new Thread(new ThreadStart(delegate { DisplayScreen(settings); }));
            //DisplayThread.Priority = ThreadPriority.Highest;
            DisplayThread.Name = "Display Thread";
            DisplayThread.Start();
        }

        public static void DisplayScreen()
        {
            DisplayScreen(DEFAULT_SETTINGS);
        }

        public static void DisplayScreen(ScreenSettings settings)
        {
            if (DisplayThread == null)
                DisplayThread = Thread.CurrentThread;
            Settings = settings;

            IsRunning = true;
            _prepare();
            if (!Settings.UseOpenTKLoop)
                _gameLoop();
            else
                _openTKStart();
        }

        public static void Invoke(Action action)
        {
            lock (job_lock)
            {
                invokes.Push(action);
            }
        }

        public static void TerminateScreen()
        {
            IsRunning = false;
            if (DisplayThread != null)
            {
                DisplayThread.Interrupt();
                if (Settings.UseOpenTKLoop)
                {
                    window.Close();
                }
                DisplayThread.Join();
            }
        }

        public static void ValidateOpenGLSafe(string MethodName, bool warn = false)
        {
            if (DisplayThread != null && DisplayThread != Thread.CurrentThread)
            {
                if (warn)
                    Logger.Warn("The method \"" + MethodName + "\" SHOULD be invoked in an OpenGL safe thread.");
                else
                    throw new InvalidOperationException("The method \"" + MethodName + "\" must be invoked in an OpenGL safe thread!");
            }
        }

        public static void ValidateOpenGLUnsafe(string MethodName, bool warn = false)
        {
            if (DisplayThread != null && DisplayThread == Thread.CurrentThread)
            {
                if (warn)
                    Logger.Warn("The method \"" + MethodName + "\" SHOULD be invoked OUTSIDE an OpenGL safe thread.");
                else
                    throw new InvalidOperationException("The method \"" + MethodName + "\" must be invoked OUTSIDE an OpenGL safe thread!");
            }
        }

        private static void _prepare()
        {
            GlobalSettings.ScreenSettings = Settings;
            GlobalSettings.EngineSettings = new EngineSettings();

            window = new GameWindow((int)Settings.WindowSize.Width, (int)Settings.WindowSize.Height);
            window.Visible = true;
            window.Title = Settings.WindowTitle;
            window.VSync = Settings.VSync ? VSyncMode.On : VSyncMode.Off;
            window.WindowBorder = WindowBorder.Fixed;

            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.ClearDepth(1.0);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        static void window_UpdateFrame(object sender, FrameEventArgs e)
        {
            window.Title = Settings.WindowTitle + "  FPS: " + FPS + "  UPS: " + UPS;
            _logicTick();
            UPS = window.UpdateFrequency;
        }

        static void window_RenderFrame(object sender, FrameEventArgs e)
        {
            _draw();
            window.SwapBuffers();
            FPS = window.RenderFrequency;
        }

        private static void _openTKStart()
        {
            window.RenderFrame += window_RenderFrame;
            window.UpdateFrame += window_UpdateFrame;

            if (Settings.MaxFPS == -1)
                window.Run(1000 / Settings.LogicTickRate);
            else
                window.Run(1000 / Settings.LogicTickRate, Settings.MaxFPS);
        }

        private static void _gameLoop()
        {
            _tickAtStart = Environment.TickCount;
            int fpsCount = 0;
            long cur;
            long now = TickCount;
            float delta;
            float fpsTime = 0;
            int updates = 0;
            int loop = 0;
            int ntick = TickCount;
            long ms = TickCount;
            while (IsRunning && !window.IsExiting)
            {
                window.Title = Settings.WindowTitle + "  FPS: " + FPS + "  UPS: " + UPS;
                cur = now;
                now = TickCount;
                delta = (now - cur) / 100f;

                window.ProcessEvents();
                
                loop = 0;
                while (TickCount > ntick && loop < Settings.MaxSkippedFrames)
                {
                    _logicTick();

                    ntick += SkipTicks;
                    loop++;
                    updates++;
                    if (TickCount - ms >= 1000)
                    {
                        UPS = updates;
                        updates = 0;
                        ms = TickCount;
                    }
                }

                _draw();
                try
                {
                    window.SwapBuffers();
                }
                catch { }

                if (window.IsExiting)
                {
                    IsRunning = false;
                    break;
                }

                fpsTime += delta;
                fpsCount++;
                if (fpsCount == 100)
                {
                    fpsCount = 0;
                    FPS = (1000f / fpsTime);
                    fpsTime = 0;
                }
            }
        }

        public static void GoFullscreen()
        {

            window.WindowBorder = WindowBorder.Hidden;

            DisplayDevice.Default.ChangeResolution(window.Width, window.Height, DisplayDevice.Default.BitsPerPixel, DisplayDevice.Default.RefreshRate);

            window.WindowState = WindowState.Maximized;
        }

        private static void _draw()
        {
            if (Settings.Fullscreen && window.WindowState != WindowState.Maximized)
            {
                GoFullscreen();
            }
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(0f, 0f, 0f, 1f);

            Settings.Camera.BeforeRender();

            if (renders == null) return;

            lock (job_lock)
            {
                while (invokes.Count > 0)
                {
                    if (invokes.Peek() == null)
                        invokes.Pop();
                    else
                        invokes.Pop().Invoke();
                }

                renders.PreFetch();
                foreach (IRenderJob job in renders.RenderJobs)
                {
                    try
                    {
                        job.PerformJob();
                    }
                    catch (Exception e)
                    {
                        Logger.CaughtException(e);
                    }
                }
                renders.PostFetch();
            }
        }

        private static void _logicTick()
        {
            if (logics == null) return;

            lock (logic_lock)
            {
                logics.PreFetch();
                foreach (ILogical logic in logics.LogicalList) 
                {
                    try
                    {
                        logic.Update();
                    }
                    catch (Exception e)
                    {
                        Logger.CaughtException(e);
                    }
                }
                logics.PostFetch();
            }
        }
    }
}