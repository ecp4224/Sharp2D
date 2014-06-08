﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Settings;
using System.Threading;
using Sharp2D.Core.Logic;
using Sharp2D.Core.Utils;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Sharp2D.Core.Graphics
{
    public class Screen : GameWindow
    {
        public static ScreenSettings DEFAULT_SETTINGS { 
            get 
            {
                ScreenSettings settings = new ScreenSettings(null);
                settings.LogicTickRate = 40; //25 ticks/second
                settings.MaxSkippedFrames = 5; //Skip a max of 5 draw frames
                settings.GameSize = new Rectangle(1280, 720); //720p
                settings.WindowSize = settings.GameSize;
                settings.Fullscreen = false;
                settings.VSync = false;
                settings.Camera = new Sharp2D.Game.Worlds.GenericCamera() {
                    Z = 1f
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
        }

        public static Thread DisplayThread { get; private set; }

        public static bool IsRunning { get; private set; }

        public static ScreenSettings Settings { get; private set; }

        public static float FPS { get; private set; }

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
        private static ILogicContainer logics;
        private static IRenderJobContainer renders;
        private static GameWindow window;

        private static object job_lock = new object();
        private static object logic_lock = new object();

        public static void DisplayScreenAsync()
        {
            DisplayThread = new Thread(new ThreadStart(DisplayScreen));
            DisplayThread.Priority = ThreadPriority.Highest;
            DisplayThread.Name = "Display Thread";
            DisplayThread.Start();
        }

        public static void DisplayScreenAsync(ScreenSettings settings)
        {
            DisplayThread = new Thread(new ThreadStart(delegate { DisplayScreen(settings); }));
            DisplayThread.Priority = ThreadPriority.Highest;
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

        public static void TerminateScreen()
        {
            IsRunning = false;
            if (DisplayThread != null)
            {
                DisplayThread.Interrupt();
                DisplayThread.Join();
            }
        }

        public static void ValidateOpenGLSafe(string MethodName)
        {
            if (DisplayThread != null && DisplayThread != Thread.CurrentThread)
                throw new InvalidOperationException("The method \"" + MethodName + "\" must be called inside an OpenGL safe thread!");
        }

        public static void ValidateOpenGLUnsafe(string MethodName)
        {
            if (DisplayThread != null && DisplayThread == Thread.CurrentThread)
                throw new InvalidOperationException("The method \"" + MethodName + "\" must be called OUTSIDE an OpenGL safe thread!");
        }

        private static void _prepare()
        {
            window = new GameWindow((int)Settings.WindowSize.Width, (int)Settings.WindowSize.Height);
            window.Visible = true;
            window.Title = Settings.WindowTitle;
            window.VSync = Settings.VSync ? VSyncMode.On : VSyncMode.Off;
            window.WindowBorder = WindowBorder.Fixed;

            GL.ClearColor(0f, 1f, 0f, 1f);
            GL.ClearDepth(1.0);
            GL.Viewport(0, 0, (int)Settings.WindowSize.Width, (int)Settings.WindowSize.Height);
            GL.MatrixMode(MatrixMode.Projection);
            
            GL.LoadIdentity();
            
            GL.Ortho(0f, Settings.GameSize.Width, Settings.GameSize.Height, 0f, 0.1f, -1f);
            GL.MatrixMode(MatrixMode.Modelview);

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, 8448);

            GL.LoadIdentity();
        }

        static void window_UpdateFrame(object sender, FrameEventArgs e)
        {
            _logicTick();
            Console.CursorTop = 1;
            Console.WriteLine("                                            ");
            Console.WriteLine("UPS: " + window.UpdateFrequency);
        }

        static void window_RenderFrame(object sender, FrameEventArgs e)
        {
            _draw();
            window.SwapBuffers();
            Console.CursorTop = 0;
            Console.WriteLine("                                            ");
            Console.WriteLine("FPS: " + window.RenderFrequency);
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
            int loop = 0;
            int ntick = TickCount;
            while (IsRunning)
            {
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
                }

                _draw();
                window.SwapBuffers();

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
                    Console.WriteLine("FPS: " + FPS);
                    fpsTime = 0;
                }

                Thread.Sleep(1);
            }
        }

        private static void _draw()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(0f, 1f, 0f, 1f);
            
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            Settings.Camera.BeforeRender();

            if (renders == null) return;

            lock (job_lock)
            {
                renders.PreFetch();
                foreach (IRenderJob job in renders.RenderJobs)
                {
                    job.PerformJob();
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
                    logic.Update();
                }
                logics.PostFetch();
            }
        }
    }
}