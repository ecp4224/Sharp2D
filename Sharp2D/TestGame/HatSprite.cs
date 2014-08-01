using OpenTK.Input;
using Sharp2D;

namespace TestGame
{
    public class HatSprite : AnimatedSprite
    {
        public override string Name
        {
            get { return "hat"; }
        }

        public HatSprite()
        {
            Texture = Texture.NewTexture("sprites/hat/hat.png");
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
