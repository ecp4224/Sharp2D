using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Sharp2D.Core
{
    public class EngineSettings : SaveableSettings
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private bool _showConsole;
        private bool _writeLog;

        /// <summary>
        /// <para>Have Sharp2D use it's own embedded files for shaders and other files, or not.</para>
        /// <para>Sharp2D loads shader files from the "shader" folder when not using embedded resources</para>
        /// </summary>
        public bool PreferEmbeddedResources { get; set; }

        /// <summary>
        /// <para>Whether Sharp2D should show the console</para>
        /// </summary>
        public bool ShowConsole
        {
            get
            {
                return _showConsole;
            }
            set
            {
                _showConsole = value; 
                
                var handle = GetConsoleWindow();

                if (_showConsole)
                    ShowWindow(handle, SW_SHOW);
                else
                    ShowWindow(handle, SW_HIDE);
            }
        }

        /// <summary>
        /// <para>Whether Sharp2D should save console log</para>
        /// </summary>
        public bool WriteLog
        {
            get
            {
                return _writeLog;
            }
            set
            {
                _writeLog = value;

                Logger.Redirect(_writeLog);
            }
        }

        public EngineSettings(EngineSettings settings)
        {
            if (settings == null)
                return;

            PreferEmbeddedResources = settings.PreferEmbeddedResources;
        }

        public EngineSettings()
        {
            PreferEmbeddedResources = true;
        }

        protected override void OnLoad()
        {
        }

        protected override void OnSave()
        {
        }
    }
}
