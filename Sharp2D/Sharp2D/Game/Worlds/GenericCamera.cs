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
    public class GenericCamera : Camera
    {
        public override void BeforeRender()
        {
            
        }

        public override bool IsOutsideCamera(float X, float Y, float Width, float Height)
        {
            return false;
        }
    }
}
