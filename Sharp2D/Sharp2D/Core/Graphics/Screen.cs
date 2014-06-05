using System;
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
                ScreenSettings settings = new ScreenSettings();
                settings.LogicTickRate = 200; //5 ticks/second
                settings.MaxSkippedFrames = 5; //Skip a max of 5 draw frames
                settings.GameSize = new Rectangle(1280, 720); //720p
                settings.WindowSize = settings.GameSize;
                settings.Fullscreen = false;
                settings.VSync = false;
                settings.Camera = new GenericCamera();
                settings.WindowTitle = "Sharp2D";

                return settings;
            } 
        }

        public static Thread DisplayThread { get; private set; }

        public static bool IsRunning { get; private set; }

        public static ScreenSettings Settings { get; private set; }

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

        public static void DisplayScreen()
        {
            DisplayScreen(DEFAULT_SETTINGS);
        }

        public static void DisplayScreen(ScreenSettings settings)
        {
            Settings = settings;

            IsRunning = true;
            _prepare();
            _gameLoop();
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
            window = new GameWindow();
            window.Visible = true;
            window.Title = Settings.WindowTitle;

            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.ClearDepth(1.0);
            GL.Viewport(0, 0, (int)Settings.WindowSize.Width, (int)Settings.WindowSize.Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, 8448);

            GL.LoadIdentity();
        }

        private static void _gameLoop()
        {
            DisplayThread = Thread.CurrentThread;
            _tickAtStart = Environment.TickCount;
            int loop = 0;
            int ntick = TickCount;
            while (IsRunning)
            {
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
            }
        }

        private static void _draw()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(0f, 0f, 0f, 1f);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { 9728 });
            GL.LoadIdentity();

            Settings.Camera.BeforeRender();

            if (renders == null) return;

            lock (job_lock)
            {
                foreach (IRenderJob job in renders.RenderJobs)
                {
                    job.PerformJob();
                }
            }
        }

        private static void _logicTick()
        {
            if (logics == null) return;

            lock (logic_lock)
            {
                foreach (ILogical logic in logics.LogicalList) 
                {
                    logic.Update();
                }
            }
        }
    }
}
