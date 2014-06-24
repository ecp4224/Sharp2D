using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Graphics;

namespace Sharp2D.Game.Sprites
{
    public class OpenGL3Camera : Camera
    {
        public override void BeforeRender()
        {
        }

        public override bool IsOutsideCamera(float X, float Y, float Width, float Height)
        {
            Y = -Y;
            float temp = Screen.Camera.Z / 100f;
            float temp2 = 7f / temp;
            float temp3 = 64f * temp;
            return
                (X + Width) + Screen.Camera.X < -temp3 - (Screen.Settings.GameSize.Width / temp2) ||
                Screen.Camera.X + (X + Width) > temp3 + (Screen.Settings.GameSize.Width / temp2) ||
                (Y + Height) + Screen.Camera.Y < -temp3 - (Screen.Settings.GameSize.Height / temp2) ||
                Screen.Camera.Y + (Y + Height) > temp3 + (Screen.Settings.GameSize.Height / temp2);
        }
    }
}
