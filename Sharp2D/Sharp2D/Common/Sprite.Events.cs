using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D
{
    public partial class Sprite
    {
        public event EventHandler OnSpriteMoved;
    }

    public sealed class OnSpriteMoved : EventArgs
    {
        public float OldX { get; private set; }

        public float OldY { get; private set; }

        public Sprite Sprite { get; private set; }

        public OnSpriteMoved(Sprite sprite, float OldX, float OldY)
        {
            this.Sprite = sprite;
            this.OldX = OldX;
            this.OldY = OldY;
        }
    }
}
