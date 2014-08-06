using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D
{
    public partial class Sprite
    {
        public event EventHandler Moved;
        public event EventHandler Disposed;
        public event EventHandler Displayed;
        public event EventHandler Loaded;
        public event EventHandler Unloaded;
        public event EventHandler Drawn;
    }

    public class SpriteEvent : EventArgs
    {
        public Sprite Sprite { get; private set; }

        public SpriteEvent(Sprite sprite)
        {
            this.Sprite = sprite;
        }
    }

    public sealed class OnSpriteMoved : SpriteEvent
    {
        public float OldX { get; private set; }

        public float OldY { get; private set; }

        public OnSpriteMoved(Sprite sprite, float OldX, float OldY) : base(sprite)
        {
            this.OldX = OldX;
            this.OldY = OldY;
        }
    }
}
