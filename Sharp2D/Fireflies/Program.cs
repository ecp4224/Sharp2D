using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sharp2D;

namespace Fireflies
{
    class Program
    {
        static WMPLib.WindowsMediaPlayer wplayer = new WMPLib.WindowsMediaPlayer();
        public static void Main(string[] args)
        {
            Console.Write("How many should I make? ");
            FireflyWorld.FireflyCount = int.Parse(Console.ReadLine());

            var settings = new ScreenSettings {UseOpenTKLoop = true};
            //settings.Fullscreen = true;
            //settings.WindowSize = new System.Drawing.Rectangle(0, 0, 1920, 1080);

            Screen.DisplayScreenAsync(settings);

            wplayer.URL = "bg.mp3";
            wplayer.settings.volume = 100;
            wplayer.controls.play();
            wplayer.settings.setMode("loop", true);

            var world = new FireflyWorld();
            var world2 = new FireflyWorld();
            world.Load();
            world2.Load();
            world.Camera.Z = world2.Camera.Z = 200;
            world.Camera.X = world2.Camera.X = -(24 * 16f);
            world.Camera.Y = world2.Camera.Y = 18 * 16f;

            GlobalSettings.EngineSettings.ShowConsole = true;

            world.AmbientBrightness = 0.1f;
            world2.AmbientBrightness = 0.1f;

            world.Display();

            while (true)
            {
                Thread.Sleep(10000);
                if (world.Displaying)
                    world2.Display();
                else 
                    world.Display();
            }
        }
    }
}
