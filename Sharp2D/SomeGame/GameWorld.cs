using Sharp2D;
using Sharp2D.Core.Interfaces;
using SkiaSharp;

namespace SomeGame
{
    public class GameWorld : GenericWorld
    {
        private Player player;
        public override string Name
        {
            get { return "worlds/map.json"; }
        }

        protected override void OnInitialDisplay()
        {
            base.OnInitialDisplay();

            //player = new Player {X = (2*32f), Y = 13*32f, Scale = 0.75f};
            player = new Player {X = (49*32f), Y = 53*32f, Scale = 0.75f};
            //AddLogical(new SmoothCameraFollow(player, this));
            Camera.Follow2D(player);
            AddSprite(player);

            var light = new Light(player.X, player.Y, 0.4f, 150f, SKColors.White, LightType.DynamicPointLight);
            player.Attach(light);
            AddLight(light);


            var fly = new Firefly {X = player.X, Y = player.Y};
            Logger.Debug("Spawn @ " + fly.X + ", " + fly.Y);
            AddLight(fly);
            AddLogical(fly);


            SpawnFireflies(8, 51, 50, 53, 3);

            SpawnFireflies(0, 41, 5, 51, 2);

            SpawnFireflies(0, 24, 35, 30, 2);

            SpawnFireflies(0, 10, 56, 16, 3);

            SpawnFireflies(58, 5, 74, 20, 10);
        }

        private void SpawnFireflies(int minx, int miny, int maxx, int maxy, int maxCount)
        {
            var random = new Random();
            int c = random.Next(maxCount) + 1;
            for (int i = 0; i < c; i++)
            {
                int tx = random.Next(minx * 32, maxx * 32);
                int ty = random.Next(miny * 32, maxy * 32);
                int count = random.Next(15-10) + 10;

                for (int z = 0; z < count; z++)
                {
                    int xadd = random.Next(-32 * 2, 32 * 2);
                    int yadd = random.Next(-32 * 2, 32 * 2);

                    var fly = new Firefly { X = tx + xadd, Y = ty + yadd };
                    Logger.Debug("Spawn @ " + fly.X + ", " + fly.Y);
                    AddLight(fly);
                    AddLogical(fly);
                }
            }
        }

        public void FadeOut(Action complete)
        {
            long start = Screen.TickCount;
            ILogical[] ticker = {null};
            ticker[0] = AddLogical(() =>
            {
                AmbientBrightness = MathUtils.Ease(0.1f, 0f, 2000, Screen.TickCount - start);
                if (AmbientBrightness != 0f) return;
                
                if (complete != null)
                    complete();

                RemoveLogical(ticker[0]);
            });
        }
    }
}
