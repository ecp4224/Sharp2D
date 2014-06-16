using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Sharp2D.Game.Sprites
{
    public class OpenGL1Camera : Camera
    {
        public OpenGL1Camera()
        {
            Z = 1f;
        }

        public override void BeforeRender()
        {
            GL.Scale(Z, Z, 0f);
            GL.Translate(-X, -Y, 0f);
        }
    }
}
