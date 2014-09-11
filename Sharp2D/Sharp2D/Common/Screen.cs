using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Sharp2D.Core;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Interfaces;

namespace Sharp2D
{
    public class Screen
    {
        public static ScreenSettings DefaultSettings { 
            get 
            {
                var settings = new ScreenSettings(null)
                {
                    LogicTickRate = 40,
                    MaxSkippedFrames = 5,
                    GameSize = new System.Drawing.Rectangle(0, 0, 1280, 720)
                };
                settings.WindowSize = settings.GameSize;
                settings.Fullscreen = false;
                settings.VSync = false;
                settings.Camera = new Game.Worlds.GenericCamera
                {
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

        public static double Fps { get; private set; }

        public static double Ups { get; private set; }

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
                lock (LogicLock)
                {
                    _logics = value;
                }
            }
            get
            {
                return _logics;
            }
        }

        public static IRenderJobContainer RenderJobContainer
        {
            set
            {
                lock (JobLock)
                {
                    _renders = value;
                }
            }
            get
            {
                return _renders;
            }
        }

        public static bool IsFocused
        {
            get { return _window != null && _window.Focused; }
        }

        public static Process CurrentProcess
        {
            get { return _curProcess; }
        }

        private static Process _curProcess;
        private static int _tickAtStart;
        private static readonly Stack<Action> Invokes = new Stack<Action>();
        private static ILogicContainer _logics;
        private static IRenderJobContainer _renders;
        private static GameWindow _window;

        private static readonly object JobLock = new object();
        private static readonly object LogicLock = new object();

        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);

        public static void DisplayScreenAsync()
        {
            Settings = DefaultSettings;
            DisplayThread = new Thread(DisplayScreen)
            {
                Priority = ThreadPriority.Highest,
                Name = "Display Thread"
            };
            DisplayThread.Start();
        }

        public static void DisplayScreenAsync(ScreenSettings settings)
        {
            Settings = settings;
            DisplayThread = new Thread(() => DisplayScreen(settings)) {Name = "Display Thread"};
            //DisplayThread.Priority = ThreadPriority.Highest;
            DisplayThread.Start();
        }

        public static void DisplayScreen()
        {
            DisplayScreen(DefaultSettings);
        }

        public static void DisplayScreen(ScreenSettings settings)
        {
            if (DisplayThread == null)
                DisplayThread = Thread.CurrentThread;
            Settings = settings;

            IsRunning = true;
            _curProcess = Process.GetCurrentProcess();
            _prepare();
            if (!Settings.UseOpenTKLoop)
                _gameLoop();
            else
                _openTKStart();
        }

        public static void RequestFocus()
        {
            if (!IsInOpenGLThread())
            {
                Invoke(RequestFocus);
                return;
            }
            SetForegroundWindow(CurrentProcess.MainWindowHandle.ToInt32());
        }

        public static void Invoke(Action action)
        {
            lock (JobLock)
            {
                Invokes.Push(action);
            }
        }

        public static void TerminateScreen()
        {
            IsRunning = false;
            if (DisplayThread != null)
            {
                _window.Close();
                Environment.Exit(0);
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

        public static bool IsInOpenGLThread()
        {
            return DisplayThread != null && DisplayThread == Thread.CurrentThread;
        }

        private static void _prepare()
        {
            GlobalSettings.ScreenSettings = Settings;
            GlobalSettings.EngineSettings = new EngineSettings();

            GlobalSettings.EngineSettings.ShowConsole = false;
            GlobalSettings.EngineSettings.WriteLog = false;

            _window = new GameWindow(Settings.WindowSize.Width, Settings.WindowSize.Height)
            {
                Visible = true,
                Title = Settings.WindowTitle,
                VSync = Settings.VSync ? VSyncMode.On : VSyncMode.Off,
                WindowBorder = WindowBorder.Fixed
            };

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.ClearDepth(1.0);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Logger.SaveLog();
        }

        static void window_UpdateFrame(object sender, FrameEventArgs e)
        {
            _window.Title = Settings.WindowTitle + "  FPS: " + Fps + "  UPS: " + Ups;
            _logicTick();
            Ups = _window.UpdateFrequency;
        }

        static void window_RenderFrame(object sender, FrameEventArgs e)
        {
            _draw();
            _window.SwapBuffers();
            Fps = _window.RenderFrequency;
        }

        private static void _openTKStart()
        {
            _window.RenderFrame += window_RenderFrame;
            _window.UpdateFrame += window_UpdateFrame;

            if (Settings.MaxFPS == -1)
                _window.Run(1000.0 / Settings.LogicTickRate);
            else
                _window.Run(1000.0 / Settings.LogicTickRate, Settings.MaxFPS);
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
            while (IsRunning && !_window.IsExiting)
            {
                _window.Title = Settings.WindowTitle + "  FPS: " + Fps + "  UPS: " + Ups;
                cur = now;
                now = TickCount;
                delta = (now - cur) / 100f;

                _window.ProcessEvents();
                
                loop = 0;
                while (TickCount > ntick && loop < Settings.MaxSkippedFrames)
                {
                    _logicTick();

                    ntick += SkipTicks;
                    loop++;
                    updates++;
                    if (TickCount - ms < 1000) continue;
                    Ups = updates;
                    updates = 0;
                    ms = TickCount;
                }

                _draw();
                try
                {
                    _window.SwapBuffers();
                }
                catch { }

                if (_window.IsExiting)
                {
                    IsRunning = false;
                    break;
                }

                fpsTime += delta;
                fpsCount++;
                if (fpsCount == 100)
                {
                    fpsCount = 0;
                    Fps = (1000f / fpsTime);
                    fpsTime = 0;
                }
            }
        }

        public static void GoFullscreen()
        {

            _window.WindowBorder = WindowBorder.Hidden;

            DisplayDevice.Default.ChangeResolution(_window.Width, _window.Height, DisplayDevice.Default.BitsPerPixel, DisplayDevice.Default.RefreshRate);

            _window.WindowState = WindowState.Maximized;
        }

        private static void _draw()
        {
            if (Settings.Fullscreen && _window.WindowState != WindowState.Maximized)
            {
                GoFullscreen();
            }
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(0f, 0f, 0f, 1f);

            Settings.Camera.BeforeRender();

            if (_renders == null) return;

            lock (JobLock)
            {
                while (Invokes.Count > 0)
                {
                    if (Invokes.Peek() == null)
                        Invokes.Pop();
                    else
                        Invokes.Pop().Invoke();
                }

                _renders.PreFetch();
                foreach (IRenderJob job in _renders.RenderJobs.TakeWhile(job => IsRunning))
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
                _renders.PostFetch();
            }
        }

        private static void _logicTick()
        {
            if (_logics == null) return;

            lock (LogicLock)
            {
                _logics.PreFetch();
                foreach (ILogical logic in _logics.LogicalList.TakeWhile(logic => IsRunning))
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
                _logics.PostFetch();
            }
        }
    }
}
