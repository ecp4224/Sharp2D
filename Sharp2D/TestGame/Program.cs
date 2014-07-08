using System;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Logic;
using Sharp2D.Core.Settings;
using Sharp2D.Core.Utils;
using System.Drawing;
using System.Threading;
using Sharp2D.Game.Worlds;

namespace TestGame
{
    class Program
    {
        public static TestSprite spriteSause;
        public static void Main(string[] args)
        {
            try
            {
                ScreenSettings settings = new ScreenSettings();
                settings.UseOpenTKLoop = false;

                Screen.DisplayScreenAsync(settings);

                System.Threading.Thread.Sleep(1000);

                TestWorld world = new TestWorld();

                world.Load();

                world.Display();

                Screen.Camera.Z = 2f;
                //world.AddLogical(new MoveCamera() { Start = Screen.TickCount });

                TestSprite idontevenknowanymore = new TestSprite();
                idontevenknowanymore.X = 456;
                idontevenknowanymore.Y = 680;
                world.AddSprite(idontevenknowanymore);

                Screen.Camera.Z = 100f;
                Screen.Camera.Y = idontevenknowanymore.Y - 55;
                Screen.Camera.X = -idontevenknowanymore.X;

                //if (eddie.CurrentWorld != null)
                //    Logger.Debug(eddie.CurrentWorld.Name);

                //world.AddLogical(new MoveCamera());

                System.Threading.Thread.Sleep(1000);
                //idontevenknowanymore.CurrentlyPlayingAnimation["hat"].Play();

                Random rand = new Random();

                Light light = new Light(idontevenknowanymore.X, idontevenknowanymore.Y, 1f, 50f);
                light.Color = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
                light.Radius = 100;

                world.AddLight(light);

                idontevenknowanymore.Attach(light);

                int testCount = 0;
                for (int i = 0; i < testCount; i++)
                {
                    light = new Light(50f * i, 600, 1f, 50f);
                    light.Color = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
                    light.Radius = 100;

                    world.AddLight(light);
                }
                //world.AmbientColor = Color.Tomato;
                world.AmbientBrightness = 0.4f;

                while (true)
                {
                    idontevenknowanymore.X += 1f;
                    Thread.Sleep(150);
                }
            }
            catch (Exception e)
            {
                Logger.Debug(e.ToString());
            }
        }

        public static void TestBitmapDrawing(TestWorld world)
        {
            Font font = new Font("Oxygen", 18f, FontStyle.Regular, GraphicsUnit.Point); //Get font object

            Texture texture = Texture.NewTexture("jkhjlkjh"); //Create a new texture
            texture.BlankTexture(128, font.Height + 16); //Set the texture to a blank texture with the width of 128 and the height of 16 + the font's height

            Sharp2D.Game.Sprites.DefaultSprite sprite = new Sharp2D.Game.Sprites.DefaultSprite(); //Create a new default sprite
            sprite.Texture = texture; //Set it's texture to the one we just made
            sprite.Displayed += delegate //Handle the Displayed event
            {
                texture.CreateOrUpdate(); //And have it create the texture
            };
            sprite.Height = texture.TextureHeight; //Set the height 
            sprite.Width = texture.TextureWidth; //Set the width
            sprite.X = 600; //Set the X
            sprite.Y = 600; //Set the Y
            world.AddSprite(sprite); //And lastely, add the sprite to the world

            Random rand = new Random();
            char[] chars = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            using (var g = Graphics.FromImage(sprite.Texture.Bitmap))
            {
                using (var b = new SolidBrush(Color.Black))
                {
                    using (var back = new SolidBrush(Color.Green))
                    {
                        g.FillRectangle(back, 0, 0, sprite.Width, sprite.Height);
                        float x = 0f;
                        while (true)
                        {
                            if (x > sprite.Width)
                                continue;

                            string s = "" + chars[rand.Next(chars.Length)];
                            g.DrawString(s, font, b, x, 0);
                            Screen.Invoke(new Action(delegate
                            {
                                if (x > sprite.Width)
                                {
                                    texture.ClearTexture();
                                    g.FillRectangle(back, 0, 0, sprite.Width, sprite.Height);
                                    x = 0f;
                                }
                                texture.CreateOrUpdate();
                            }));

                            x += g.MeasureString(s, font).Width;

                            Thread.Sleep(1000);
                        }
                    }
                }
            }
        }
    }

    class MoveCamera : ILogical
    {
        public long Start;
        public void Update()
        {
            Screen.Camera.X -= 2;
            Logger.WriteAt(0, 0, "FPS: " + Screen.FPS);
        }

        public void Dispose()
        {

        }
    }

    class CheckKeys : ILogical
    {
        public void Update()
        {
            if (Input.Keyboard["Jump"])
            {
                Logger.Log("I LIKE TURTLES!");
            }

            if (Input.Mouse["Shoot"])
            {
                Logger.Log("SCOOTALOO IS THE BEST PONY!");
            }
        }

        public void Dispose()
        {
            
        }
    }
}
