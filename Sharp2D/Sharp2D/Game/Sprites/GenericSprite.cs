using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D.Game.Sprites
{
    public class GenericSprite : PhysicsSprite
    {
        private readonly string _name;

        public override string Name
        {
            get { return _name; }
        }

        public GenericSprite(string name, Texture texture)
        {
            this._name = name;
            this.Texture = texture;
        }

        protected override void OnLoad()
        {
            Width = Texture.TextureWidth;
            Height = Texture.TextureHeight;

            base.OnLoad();
        }

        protected override void BeforeDraw()
        {
            
        }

        protected override void OnDisplay()
        {
            
        }
    }
}
