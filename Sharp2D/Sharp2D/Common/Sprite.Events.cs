using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Interfaces;

namespace Sharp2D
{
    public partial class Sprite
    {
        public event EventHandler<SpriteEvent> Disposed;
        public event EventHandler<SpriteEvent> Displayed;
        public event EventHandler<SpriteEvent> Loaded;
        public event EventHandler<SpriteEvent> Unloaded;
        public event EventHandler<SpriteEvent> Drawn;
        public event EventHandler<OnMoveableMoved> Moved;
    }

    public class SpriteEvent : EventArgs
    {
        public Sprite Sprite { get; private set; }

        public SpriteEvent(Sprite sprite)
        {
            this.Sprite = sprite;
        }
    }
}
