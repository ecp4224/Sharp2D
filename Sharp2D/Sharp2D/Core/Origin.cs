using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D.Core
{
    [Flags]
    public enum Origin
    {
        Center = 0,
        Bottom = 1,
        Top = 2,
        Left = 4,
        Right = 8,

        Center_Bottom = Center | Bottom,
        Center_Top = Center | Top,
        Center_Left = Center | Left,
        Center_Right = Center | Right,
        Bottom_Left = Bottom | Left,
        Bottom_Right = Bottom | Right,
        Top_Left = Top | Left,
        Top_Right = Top | Right,
        Center_Center = Center | Center
    }
}
