using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Settings;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Graphics.Shaders;
using Sharp2D.Core.Utils;

namespace TestGame
{
    class Program
    {
        public static TestSprite spriteSause;
        public static void Main(string[] args)
        {
            Screen.DisplayScreenAsync();

            System.Threading.Thread.Sleep(1000);

            TestWorld world = new TestWorld();

            world.Load();

            world.Display();

            Screen.Camera.Z = 2f;
            //world.AddLogical(new MoveCamera() { Start = Screen.TickCount });

            TestSprite idontevenknowanymore = new TestSprite();
            idontevenknowanymore.ChangeHitbox("PonyHitbox");
            idontevenknowanymore.X = 256;
            idontevenknowanymore.Y = 128;
            world.AddSprite(idontevenknowanymore);

            spriteSause = new TestSprite(); 
            spriteSause.X = 256;
            spriteSause.Y = 128;
            spriteSause.MoveFlag = true;

            
            TestSprite enemy = new TestSprite();
            enemy.ChangeHitbox("TriangleHitbox");
            enemy.X = 512;
            enemy.Y = 128;
            world.AddSprite(enemy);
            world.AddSprite(spriteSause);

            TestSprite eddie = new TestSprite();
            eddie.ChangeHitbox("UhidkHitbox");
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
