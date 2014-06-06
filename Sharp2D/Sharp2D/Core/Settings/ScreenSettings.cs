using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Utils;
using Sharp2D.Core.Graphics;

namespace Sharp2D.Core.Settings
{
    public class ScreenSettings
    {
        public string WindowTitle { get; set; }

        public int LogicTickRate { get; set; }
        public int MaxSkippedFrames { get; set; }
        public Rectangle GameSize { get; set; }
        public Rectangle WindowSize { get; set; }
        public bool Fullscreen { get; set; }
        public bool VSync { get; set; }
        public int MaxFPS { get; set; }
        private bool _opentk;
        public bool UseOpenTKLoop
        {
            get
            {
                return _opentk || MaxFPS != -1;
            }
            set
            {
                _opentk = value;
            }
        }

        public Camera Camera { get; set; }

        public ScreenSettings(ScreenSettings orginal)
        {
            if (orginal == null)
                return;
            WindowTitle = orginal.WindowTitle;
            LogicTickRate = orginal.LogicTickRate;
            MaxSkippedFrames = orginal.MaxSkippedFrames;
            GameSize = orginal.GameSize;
            WindowSize = orginal.GameSize;
            Fullscreen = orginal.Fullscreen;
            VSync = orginal.VSync;
            Camera = orginal.Camera;
        }

        public ScreenSettings() : this(Screen.DEFAULT_SETTINGS) { }
    }
}
