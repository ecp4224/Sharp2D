using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Game.Worlds.Tiled;

namespace TestGame
{
    public class TestWorld : TiledWorld
    {
        public override string Name
        {
            get { return "worlds/testworld.json"; }
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            Console.WriteLine("HI");
        }
    }
}
