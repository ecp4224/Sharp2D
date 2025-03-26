using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Mathematics;

namespace Sharp2D.Core.Interfaces
{
    public interface IMoveable3d : IMoveable2d
    {
        float Z { get; set; }

        Vector3 Vector3d { get; set; } 
    }
}
