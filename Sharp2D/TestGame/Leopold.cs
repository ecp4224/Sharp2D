using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D;

namespace TestGame
{
    public class Leopold : PhysicsSprite
    {
        public override string Name
        {
            get { return "Leopold"; }
        }

        public Leopold()
        {
            Texture = Texture.NewTexture("sprites/Leopold/Leopold.png");
            Texture.LoadTextureFromFile();
            Width = Texture.TextureWidth;
            Height = Texture.TextureHeight;
        }

        protected override void BeforeDraw()
        {
        }

        protected override void OnUnload()
        {
        }

        protected override void OnDisplay()
        {
        }
    }
}
