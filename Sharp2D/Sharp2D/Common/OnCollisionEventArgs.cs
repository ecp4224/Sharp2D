using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Interfaces;

namespace Sharp2D
{
    public class OnCollisionEventArgs : EventArgs
    {
        public ICollidable With { get; private set; }

        public ICollidable Collider { get; private set; }

        public OnCollisionEventArgs(ICollidable collider, ICollidable with)
        {
            this.With = with;
            this.Collider = collider;
        }
    }
}
