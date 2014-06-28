using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D.Core.Physics
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
