using Sharp2D;
using Sharp2D.Core.Interfaces;
using Sharp2D.Game;
using SkiaSharp;

namespace Fireflies
{
    public class MouseLight : ILogical
    {
        private Light light;

        public Light Light
        {
            get { return light; }
        }

        public MouseLight()
        {
            var mouse = Input.Mouse.GetMousePosition();
            light = new Light(mouse.X, mouse.Y, 3f, 88f, SKColors.Yellow, LightType.DynamicPointLight);
        }

        public void Dispose()
        {
            light = null;
        }

        public void Update()
        {
            var mouse = Input.Mouse.GetMousePosition();
            light.X = mouse.X;
            light.Y = mouse.Y;
            //light.X = (mouse.X - (Screen.NativeWindow.X - (Screen.NativeWindow.Width / 2f)));
            //light.Y = (mouse.Y - (Screen.NativeWindow.Y - (Screen.NativeWindow.Height / 2f)));

            Console.WriteLine((light.X / 16f) + " : " + (light.Y / 16f));
        }
    }
}
