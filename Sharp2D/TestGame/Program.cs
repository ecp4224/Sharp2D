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
            Screen.DisplayScreenAsync();

            TestWorld world = new TestWorld();
            world.Load();
            world.Display();
            Screen.Camera.Z = 2f;
            world.AddLogical(new MoveCamera() { Start = Screen.TickCount });
        }
    }

    class MoveCamera : Sharp2D.Core.Logic.ILogical
    {
        public long Start;
        public void Update()
        {
            float value = MathUtils.Ease(0f, 30f * 16f, 3000, Screen.TickCount - Start);
            Screen.Camera.X = value;
        }

        public void Dispose()
        {

        }
    }
}
