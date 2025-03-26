using OpenTK.Windowing.GraphicsLibraryFramework;
using Sharp2D;
using Sharp2D.Common;
using Sharp2D.Game;
using SkiaSharp;

namespace SomeGame
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = new ScreenSettings
            {
                Fullscreen = false,
                GameSize = new Size(1280, 720),
                WindowTitle = "Some Game",
                WindowSize = new Size(1280, 720)
            };
            
            Screen.DisplayScreen(() =>
            {
                //Set defaults
                Input.Keyboard.SetDefaults(new Dictionary<string, Keys>()
                {
                    {"moveLeft", Keys.D},
                    {"moveRight", Keys.A},
                    {"moveUp", Keys.W},
                    {"moveDown", Keys.S}
                });

                var world = new GameWorld();
                world.Load();
                
                MusicPlayer.Play("bg.ogg");
                
                world.Display();

                world.Camera.Z = 200;
                world.Camera.X = -(49*32f);
                world.Camera.Y = 53 * 32f;

                world.Camera.Bounds = new SKRect(-1000000, 6*32, -11 * 32, 48*32);

                world.AmbientBrightness = 0.1f;
            });
        }
    }
}