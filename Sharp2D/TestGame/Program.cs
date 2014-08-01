using System;
using System.Drawing;
using System.Threading;
using Sharp2D.Game.Worlds;
using System.Collections.Generic;
using System.Diagnostics;
using Sharp2D;
using Sharp2D.Game;
using Sharp2D.Core.Interfaces;

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

                Stopwatch watch = new Stopwatch();

                Console.WriteLine("Staring world load timer..");
                watch.Start();

                TestWorld world = new TestWorld();

                world.Load();

                world.Display();

                watch.Stop();
                Console.WriteLine("World loaded and displayed in: " + watch.ElapsedMilliseconds);

                Random rand = new Random();

                Screen.Camera.Z = 150f;
                int TEST = 0;
                for (int i = 0; i < TEST; i++)
                {
                    TestSprite wat = new TestSprite();
                    wat.X = rand.Next(600 - 400) + 400;
                    wat.Y = rand.Next(700 - 500) + 580;
                    wat.Layer = (float)rand.NextDouble();
                    wat.FlipState = FlipState.Horizontal;

                    world.AddSprite(wat);

                    Screen.Camera.Y = wat.Y - 55;
                    Screen.Camera.X = -wat.X;
                }

                Light light2 = new Light(456, 500, 1f, 50f, LightType.DynamicPointLight);
                Light light = new Light(456, 680, 1f, 50f, LightType.DynamicPointLight);
                Screen.Camera.X = -456;
                Screen.Camera.Y = 680;
                light2.Color = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
                light.Color = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
                light.Radius = 100;
                light2.Radius = 100;

                world.AddLight(light);
                world.AddLight(light2);

                int testCount = 10;
                for (int i = 0; i < testCount; i++)
                {
                    light = new Light(50f * i, 600, 1f, 50f, LightType.StaticPointLight);
                    light.Color = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
                    light.Radius = 100;

                    world.AddLight(light);
                }
                world.AmbientBrightness = 0.5f;

                double count = 0;
                world.AddLogical(delegate
                {
                    count += 0.2;
                    double c = Math.Cos(count);
                    double s = Math.Sin(count);
                    light.X = 456f + (float)(c * 50.0);
                    light.Y = 680f + (float)(s * 50.0);

                    light2.X = 456f + (float)(-c * 100.0);
                    light2.Y = 500f + (float)(-s * 100.0);
                });
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
