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
            Screen.DisplayScreenAsync();

            TestWorld world = new TestWorld();

            world.Load();

            world.Display();

            Screen.Camera.Z = 2f;
            world.AddLogical(new MoveCamera() { Start = Screen.TickCount });

            Texture tex = Texture.NewTexture("sprites/Hans/Hans.png");
            tex.LoadTextureFromFile();
            TestSprite idontevenknowanymore = new TestSprite();
            idontevenknowanymore.ChangeHitbox("PonyHitbox");
            idontevenknowanymore.TexCoords = new TexCoords(0, 0, tex);
            idontevenknowanymore.X = 256;
            idontevenknowanymore.Y = 512;
            world.AddSprite(idontevenknowanymore);

            spriteSause = new TestSprite(); //messy messy mess
            //spriteSause.TexCoords = new TexCoords(0, 0, null);
            spriteSause.X = 256;
            spriteSause.Y = 128;
            spriteSause.MoveFlag = true;

            
            TestSprite enemy = new TestSprite();
            enemy.ChangeHitbox("TriangleHitbox");
            //enemy.TexCoords = new TexCoords(0, 0, null);
            enemy.X = 512;
            enemy.Y = 128;
            world.AddSprite(enemy);
            world.AddSprite(spriteSause);

            TestSprite eddie = new TestSprite();
            eddie.ChangeHitbox("UhidkHitbox");
            //eddie.TexCoords = new TexCoords(0, 0, null);
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
