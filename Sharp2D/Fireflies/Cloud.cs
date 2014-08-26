using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D;
using Sharp2D.Core.Interfaces;

namespace Fireflies
{
    public class Cloud : Sprite, ILogical
    {
        private static readonly string[] CLOUDS = new string[] {
            "sprites/cloud1.png",
            "sprites/cloud2.png",
            "sprites/cloud3.png"
        };
        private static readonly Random random = new Random();


        private float speed;
        private float startX;
        public Cloud()
        {
            Texture = Texture.NewTexture(CLOUDS[random.Next(CLOUDS.Length)]);
            if (!Texture.Loaded)
                Texture.LoadTextureFromFile();

            Width = Texture.TextureWidth;
            Height = Texture.TextureHeight;

            speed = (float)Math.Min(random.NextDouble() * (1.0 / random.NextDouble()), 2.5);
        }

        public void Update()
        {
            X += speed;
            if (X >= 63 * 16f && random.NextDouble() < 0.8)
            {
                X = startX;
            }
        }

        public override string Name
        {
            get { return "cloud"; }
        }

        protected override void BeforeDraw()
        {
        }

        protected override void OnLoad()
        {
            startX = random.Next(-10, 1);
            Y -= ((1f / speed) * 2f);

            Y += random.Next(-20, 20);

            Scale = speed;
            this.speed = (float)Math.Min(speed, 0.5);

            if (random.NextDouble() < 0.4)
                FlipState = Sharp2D.FlipState.Horizontal;

            NeverClip = true;
        }

        protected override void OnUnload()
        {
        }

        protected override void OnDispose()
        {
        }

        protected override void OnDisplay()
        {
        }
    }
}
