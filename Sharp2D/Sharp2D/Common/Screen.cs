using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Sharp2D.Common;
using Sharp2D.Core;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Interfaces;
using Sharp2D.Game;

namespace Sharp2D
{
    public class Screen : GameWindow
    {
        private Screen(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, Action readyCallback) : base(gameWindowSettings, nativeWindowSettings)
        {
            this.readyCallback = readyCallback;
        }

        public static ScreenSettings DefaultSettings { 
            get 
            {
                var settings = new ScreenSettings(null)
                {
                    LogicTickRate = 40,
                    GameSize = new Size(1280, 720)
                };
                settings.WindowSize = settings.GameSize;
                settings.Fullscreen = false;
                settings.VSync = false;
                settings.WindowTitle = "Sharp2D";
                settings.MaxFPS = -1; //Max FPS
                settings.AsyncReadyCallback = true;
                settings.PauseOnWindowDrag = false;

                return settings;
            } 
        }

        public static Camera Camera
        {
            get { return _window._renders.Camera; }
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
                return Environment.TickCount - _window._tickAtStart;
            }
        }

        public static double SkipTicks
        {
            get
            {
                return 1000 / TicksPerSecond;
            }
        }

        public static double TicksPerSecond
        {
            get
            {
                return 1000.0 / Settings.LogicTickRate;
            }
        }

        public static ILogicContainer LogicContainer
        {
            set
            {
                lock (_window.LogicLock)
                {
                    _window._logics = value;
                }
            }
            get
            {
                return _window._logics;
            }
        }

        public static IRenderJobContainer RenderJobContainer
        {
            set
            {
                lock (_window.JobLock)
                {
                    _window._renders = value;
                }
            }
            get
            {
                return _window._renders;
            }
        }

        public static bool IsFocused
        {
            get { return NativeWindow is { IsFocused: true }; }
        }

        public static Process CurrentProcess
        {
            get { return _curProcess; }
        }

        public static GameWindow NativeWindow
        {
            get { return _window; }
        }

        private int _tickAtStart;
        private readonly Stack<Action> Invokes = new Stack<Action>();
        private ILogicContainer _logics;
        private IRenderJobContainer _renders;
        private Action readyCallback;
        
        private static Screen _window;
        private static Process _curProcess;

        private readonly object JobLock = new object();
        private readonly object LogicLock = new object();

        public static void DisplayScreen(Action readyCallback = null)
        {
            DisplayScreen(DefaultSettings, readyCallback ?? (() =>
            {
                Logger.Log("Screen displayed");
            }));
        }

        public static void DisplayScreen(ScreenSettings settings, Action readyCallback)
        {
            if (DisplayThread == null)
                DisplayThread = Thread.CurrentThread;
            Settings = settings;

            IsRunning = true;
            _curProcess = Process.GetCurrentProcess();

            var gameWindowSettings = new GameWindowSettings()
            {
                UpdateFrequency = 1000.0 / Settings.LogicTickRate,
                Win32SuspendTimerOnDrag = Settings.PauseOnWindowDrag,
            };

            var clientSize = new Vector2i(Settings.WindowSize.Width, Settings.WindowSize.Height);
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Title = Settings.WindowTitle,
                ClientSize = clientSize,
                MaximumClientSize = clientSize,
                MinimumClientSize = clientSize,
                WindowState = Settings.Fullscreen ? WindowState.Fullscreen : WindowState.Normal,
                Vsync = Settings.VSync ? VSyncMode.Adaptive : VSyncMode.Off,
            };

            using (_window = new Screen(gameWindowSettings, nativeWindowSettings, readyCallback))
            {
                _window._prepare();
                _window._openTKStart();
            }
        }

        public static void RequestFocus()
        {
            if (!IsInOpenGLThread())
            {
                Invoke(RequestFocus);
                return;
            }
            NativeWindow.Focus();
        }

        public static void Invoke(Action action)
        {
            lock (_window.JobLock)
            {
                _window.Invokes.Push(action);
            }
        }

        public static void TerminateScreen()
        {
            _window.Close();
        }

        private static void _cleanUp()
        {
            IsRunning = false;
            if (DisplayThread != null)
            {
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

        private void _prepare()
        {
            GlobalSettings.ScreenSettings = Settings;
            
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.ClearDepth(1.0);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }
        
        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Logger.SaveLog();
        }

        void window_UpdateFrame(FrameEventArgs e)
        {
            Input.Update(_window.KeyboardState, _window.MouseState);
            _window.Title = Settings.WindowTitle + "  FPS: " + Fps + "  UPS: " + Ups;
            _logicTick();
            Ups = _window.UpdateTime * 1000;
        }

        void window_RenderFrame(FrameEventArgs e)
        {
            _draw();
            _window.SwapBuffers();
            Fps = _window.UpdateTime * 1000;
        }

        private void _openTKStart()
        {
            _window.RenderFrame += window_RenderFrame;
            _window.UpdateFrame += window_UpdateFrame;
            _window.Load += WindowOnLoad;
            _window.Closing += WindowOnClosing;
            
            _window.Run();
        }

        private void WindowOnClosing(CancelEventArgs obj)
        {
            _cleanUp();
        }

        private void WindowOnLoad()
        {
            if (readyCallback == null) return;
            
            if (Settings.AsyncReadyCallback)
            {
                var displayReadyThread = new Thread(() => readyCallback())
                {
                    Priority = ThreadPriority.Highest,
                    Name = "Display Ready Thread"
                };
                displayReadyThread.Start();
            }
            else
            {
                readyCallback();
            }
        }

        public static void GoFullscreen()
        {
            _window.WindowState = WindowState.Fullscreen;
        }

        private void _draw()
        {
            if (Settings.Fullscreen && _window.WindowState != WindowState.Fullscreen)
            {
                GoFullscreen();
            }
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(0f, 0f, 0f, 1f);

            if (_renders == null) return;

            _renders.Camera.BeforeRender();

            lock (JobLock)
            {
                _renders.Camera.BeforeRender();

                while (Invokes.Count > 0)
                {
                    if (Invokes.Peek() == null)
                        Invokes.Pop();
                    else
                        Invokes.Pop().Invoke();
                }

                try
                {
                    _renders.PreFetch();
                    foreach (IRenderJob job in _renders.RenderJobs.Where(job => job != null && IsRunning))
                    {
                        if (_renders == null)
                            return;
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
                catch (Exception e)
                {
                    Logger.Debug("Error drawing frame!");
                    Logger.CaughtException(e);
                }
            }
        }

        private void _logicTick()
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

        public static void GoWindowed()
        {
            _window.WindowState = WindowState.Normal;
        }
    }
}
