using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core;

namespace Sharp2D
{
    public static class GlobalSettings
    {
        public static ScreenSettings ScreenSettings { get; internal set; }
        public static EngineSettings EngineSettings { get; internal set; }
    }
}
