using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D;
using Sharp2D.Core.Interfaces;

namespace SomeGame
{
    public class EndWorld : GenericWorld
    {
        public const int CloudCount = 200;

        public Player player;
        public override string Name
        {
            get { return "worlds/map2.json"; }
        }

        protected override void OnInitialDisplay()
        {
            base.OnInitialDisplay();

            Sprite moon = Sprite.FromImage("sprites/moon.png");
            moon.X = (1f * 32f) + (moon.Width / 2f);
            moon.Y = (20f * 32f) + (moon.Height / 2f) + 3f;
            moon.Layer = 0.5f;
            moon.Scale = 4f;
            moon.IgnoreLights = true;
            moon.NeverClip = true;
            AddSprite(moon);

            Sprite background = Sprite.FromImage("sprites/map2.png");
            background.X = background.Width/2f;
            background.Y = background.Height/2f;
            background.NeverClip = true;
            background.Layer = 1f;
            AddSprite(background);
            AddLogical(() =>
            {
                background.Y = ((Camera.Y*0.25f) - -Camera.Y);
            });

            SpawnFireflies(1, 29, 19, 48, 20);

            FadeIn(null);

            var Rand = new Random();
            for (int i = 0; i < CloudCount; i++)
            {
                var c = new Cloud { X = Rand.Next(-10, 50) * 32f, Y = Rand.Next(20 * 32, 24 * 32), Layer = (float)Rand.NextDouble(), IgnoreLights = true, Alpha = (float)Rand.NextDouble() };
                AddSprite(c);
            }
        }

        private void SpawnFireflies(int minx, int miny, int maxx, int maxy, int maxCount)
        {
            var random = new Random();
            int c = random.Next(maxCount) + 20;
            for (int i = 0; i < c; i++)
            {
                int tx = random.Next(minx * TileWidth, maxx * TileWidth);
                int ty = random.Next(miny * TileHeight, maxy * TileHeight);
                int count = random.Next(8 - 4) + 4;

                for (int z = 0; z < count; z++)
                {
                    int xadd = random.Next(-TileWidth * 2, TileWidth * 2);
                    int yadd = random.Next(-TileHeight * 2, TileHeight * 2);

                    var fly = new Firefly { X = tx + xadd, Y = ty + yadd };
                    Logger.Debug("Spawn @ " + fly.X + ", " + fly.Y);
                    AddLight(fly);
                    AddLogical(fly);
                }
            }
        }

        public void FadeIn(Action complete)
        {
            long start = Screen.TickCount;
            ILogical[] ticker = {null};
            ticker[0] = AddLogical(() =>
            {
                AmbientBrightness = MathUtils.Ease(0f, 0.2f, 2000, Screen.TickCount - start);
                if (AmbientBrightness != 0.2f) return;

                RemoveLogical(ticker[0]);

                if (complete != null)
                    complete();
            });
        }
    }
}
