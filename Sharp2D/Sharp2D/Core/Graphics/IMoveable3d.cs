using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Sharp2D.Core.Graphics
{
    public interface IMoveable3d : IMoveable2d
    {
        float Z { get; set; }

        Vector3 Vector3d { get; set; } 
    }
}
