using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Sharp2D.Core.Graphics
{
    public class GenericCamera : Camera
    {
        public override void BeforeRender()
        {
            GL.Translate(-X, -Y, 0f);
        }
    }
}
