using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Sharp2D.Core.Graphics;

namespace Sharp2D.Game.Worlds
{
    /// <summary>
    /// A static camera with no sprite culling
    /// </summary>
    public class GenericCamera : Camera
    {
        public override bool IsOutsideCamera(float X, float Y, float Width, float Height, float Scale)
        {
            return false;
        }
    }
}
