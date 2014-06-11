using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Settings;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Utils;

namespace TestGame
{
    class Program
    {
        public static TestSprite spriteSause;
        public static void Main(string[] args)
        {
            ScreenSettings settings = new ScreenSettings();
            settings.GameSize = new Sharp2D.Core.Utils.Rectangle(1280f, 720f);
            settings.WindowSize = settings.GameSize;

            Screen.DisplayScreenAsync(settings);

            TestWorld world = new TestWorld();

            world.Load();

            world.Display();

            Screen.Camera.Z = 2f;
            world.AddLogical(new MoveCamera() { Start = Screen.TickCount });

            TestSprite idontevenknowanymore = new TestSprite();
            idontevenknowanymore.ChangeHitbox("PonyHitbox");
            idontevenknowanymore.TexCoords = new Rectangle(0, 0, 1, 1);
            idontevenknowanymore.X = 256;
            idontevenknowanymore.Y = 512;
            world.AddSprite(idontevenknowanymore);

            spriteSause = new TestSprite(); //messy messy mess
            spriteSause.TexCoords = new Rectangle(0, 0, 1, 1);
            spriteSause.X = 256;
            spriteSause.Y = 128;
            spriteSause.MoveFlag = true;

            
            TestSprite enemy = new TestSprite();
            enemy.ChangeHitbox("TriangleHitbox");
            enemy.TexCoords = new Rectangle(0, 0, 1, 1);
            enemy.X = 512;
            enemy.Y = 128;
            world.AddSprite(enemy);
            world.AddSprite(spriteSause);

            TestSprite eddie = new TestSprite();
            eddie.ChangeHitbox("UhidkHitbox");
            eddie.TexCoords = new Rectangle(0, 0, 1, 1);
            eddie.X = 512;
            eddie.Y = 512;
            world.AddSprite(eddie);

        }
    }

    class MoveCamera : Sharp2D.Core.Logic.ILogical
    {
        public long Start;
        public void Update()
        {
            if (Program.spriteSause != null)
            {
                Screen.Camera.X = Program.spriteSause.X - 300;
                Screen.Camera.Y = Program.spriteSause.Y - 200;
            }
        }

        public void Dispose()
        {

        }
    }
}
