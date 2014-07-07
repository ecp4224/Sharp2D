using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Game.Worlds;

namespace TestGame
{
    public class TestWorld : GenericWorld
    {
        public override string Name
        {
            get { return "worlds/TestWorld.json"; }
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            Console.WriteLine("HI");
        }
    }
}
