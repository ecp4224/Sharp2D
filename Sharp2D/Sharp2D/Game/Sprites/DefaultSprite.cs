using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D.Game.Sprites
{
    public class DefaultSprite : Sprite
    {
        public event EventHandler Displayed;
        public event EventHandler Loaded;
        public event EventHandler Unloaded;
        public event EventHandler Disposed;
        public event EventHandler BeforeDrawn;

        protected override void BeforeDraw()
        {
            if (BeforeDrawn != null)
            {
                BeforeDrawn(this, new EventArgs());
            }
        }

        protected override void OnLoad()
        {
            if (Loaded != null)
            {
                Loaded(this, new EventArgs());
            }
        }

        protected override void OnUnload()
        {
            if (Unloaded != null)
            {
                Unloaded(this, new EventArgs());
            }
        }

        protected override void OnDispose()
        {
            if (Disposed != null)
            {
                Disposed(this, new EventArgs());
            }
        }

        protected override void OnDisplay()
        {
            if (Displayed != null)
            {
                Displayed(this, new EventArgs());
            }
        }

        public override string Name
        {
            get { return "DefaultSprite"; }
        }
    }
}
