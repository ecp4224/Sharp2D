using System;
using System.Drawing;
using OpenTK.Input;
using Sharp2D;
using Sharp2D.Core;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Interfaces;

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
            var mouse = Mouse.GetState();
            light = new Light(mouse.X, mouse.Y, 3f, 88f, Color.Yellow, LightType.DynamicPointLight);
        }

        public void Dispose()
        {
            light = null;
        }

        public void Update()
        {
            var mouse = Mouse.GetState();
            light.X = (mouse.X - (Screen.NativeWindow.X - (Screen.NativeWindow.Width / 2f)));
            light.Y = (mouse.Y - (Screen.NativeWindow.Y - (Screen.NativeWindow.Height / 2f)));

            Console.WriteLine((light.X / 16f) + " : " + (light.Y / 16f));
        }
    }
}
