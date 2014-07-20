using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Sharp2D.Game.Sprites
{
    [Obsolete("OpenGL 1 is no longer supported at this time", true)]
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

        public override bool IsOutsideCamera(float X, float Y, float Width, float Height)
        {
            return (X + Width) - Screen.Camera.X < -32 || Math.Abs(Screen.Camera.X - (X - Width)) > 32 + (Screen.Settings.GameSize.Width / Screen.Camera.Z) || (Y + Height) - Screen.Camera.Y < -32 || Math.Abs(Screen.Camera.Y - (Y - Height)) > 32 + (Screen.Settings.GameSize.Height / Screen.Camera.Z);
        }
    }
}
