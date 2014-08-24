using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D;

namespace Fireflies
{
    public class BackgroundSprite : Sprite
    {
        public override string Name
        {
            get { return "backdrop"; }
        }

        public BackgroundSprite()
        {
            Texture = Texture.NewTexture("sprites/background.png");
            Texture.LoadTextureFromFile();
            Width = Texture.TextureWidth;
            Height = Texture.TextureHeight;
            NeverClip = true;
        }

        protected override void BeforeDraw()
        {
        }

        protected override void OnLoad()
        {
        }

        protected override void OnUnload()
        {
        }

        protected override void OnDispose()
        {
        }

        protected override void OnDisplay()
        {
        }
    }
}
