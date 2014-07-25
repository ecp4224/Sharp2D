using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Sharp2D.Core.Interfaces
{
    public interface IMoveable2d
    {
        float X { get; set; }
        float Y { get; set; }

        Vector2 Vector2d { get; set; } 
    }
}
