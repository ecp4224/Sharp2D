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

        public Camera Camera { get; set; }
    }
}
