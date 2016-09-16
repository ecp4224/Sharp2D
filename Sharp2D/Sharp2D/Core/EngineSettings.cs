using System;
using Sharp2D.Core.Interfaces;

namespace Sharp2D.Core
{
    public class EngineSettings : SaveableSettings
    {

        private INativeSystem nativeSystem;
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
                _showConsole = nativeSystem.ToggleConsoleWindow(value);
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

        public bool IsRunningMono
        {
            get
            {
                return Type.GetType("Mono.Runtime") != null;
            }
        } 

        internal EngineSettings(EngineSettings settings) : this()
        {
            if (settings == null)
                return;

            PreferEmbeddedResources = settings.PreferEmbeddedResources;
            _showConsole = settings._showConsole;
            _writeLog = settings._writeLog;
        }

        internal EngineSettings()
        {
            PreferEmbeddedResources = true;
            _showConsole = false;
            _writeLog = false;

            if (IsRunningMono)
            {
                nativeSystem = new MonoNativeSystem();
            }
            else
            {
                nativeSystem = new WindowsNativeSystem();
            }
        }

        protected override void OnLoad()
        {
        }

        protected override void OnSave()
        {
        }
    }
}
