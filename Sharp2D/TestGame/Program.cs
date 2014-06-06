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
        static void Main(string[] args)
        {
            ScreenSettings settings = new ScreenSettings();
            settings.GameSize = new Sharp2D.Core.Utils.Rectangle(1280f, 720f);
            settings.WindowSize = settings.GameSize;

            Screen.DisplayScreenAsync(settings);

            TestWorld world = new TestWorld();
            world.Load();
            world.Display();
            Screen.Camera.Z = 2f;
            world.AddLogical(new MoveCamera() { start = Screen.TickCount });
        }
    }

    class MoveCamera : Sharp2D.Core.Logic.ILogical
    {
        public long start;
        public void Update()
        {
            float value = MathUtils.Ease(0f, 30f * 16f, 3000, Screen.TickCount - start);
            Screen.Camera.X = value;
            Console.CursorTop = 2;
            Console.WriteLine("       ");
            Console.WriteLine(Screen.Camera.X);
        }

        public void Dispose()
        {

        }
    }
}
