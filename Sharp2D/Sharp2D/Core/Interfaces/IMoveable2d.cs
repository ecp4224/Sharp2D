using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Mathematics;

namespace Sharp2D.Core.Interfaces
{
    public interface IMoveable2d
    {
        float X { get; set; }
        float Y { get; set; }
        float Width { get; set; }
        float Height { get; set; }

        event EventHandler<OnMoveableMoved> Moved;

        Vector2 Vector2d { get; set; } 
    }

    public class MoveableEvent : EventArgs
    {
        public IMoveable2d Moveable { get; private set; }

        public MoveableEvent(IMoveable2d moveable)
        {
            this.Moveable = moveable;
        }
    }

    public sealed class OnMoveableMoved : MoveableEvent
    {
        public float OldX { get; private set; }

        public float OldY { get; private set; }

        public OnMoveableMoved(IMoveable2d moveable, float OldX, float OldY)
            : base(moveable)
        {
            this.OldX = OldX;
            this.OldY = OldY;
        }
    }
}
