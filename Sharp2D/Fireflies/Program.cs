using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D;

namespace Fireflies
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.Write("How many should I make? ");
            FireflyWorld.FIREFLY_COUNT = int.Parse(Console.ReadLine());

            Screen.DisplayScreenAsync();

            FireflyWorld world = new FireflyWorld();
            world.Load();

            world.AmbientBrightness = 0.1f;

            world.Display();

            Screen.Camera.Z = 200;
            Screen.Camera.Y = 18 * 16f;
            Screen.Camera.X = -(24 * 16f);
        }
    }
}
