using System;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Settings;

namespace Sharp2D.Game.Sprites
{
    public abstract class PlayableSprite : PhysicsSprite
    {
        public float Speed { get; set; }
        private float _jumpVelocity = 10;
        private const float Gravity = 9.81f;
        private long _lastTick = Screen.TickCount;

        protected PlayableSprite()
        {
            Speed = 5;
        }

        public override void Update()
        {
            var count = Screen.TickCount;
            var delta = count - _lastTick;

            if (Input.Keyboard["MoveUp"]) { Y -= Speed; }
            if (Input.Keyboard["MoveDown"]) { Y += Speed; }
            if (Input.Keyboard["MoveLeft"]) { X -= Speed; }
            if (Input.Keyboard["MoveRight"]) { X += Speed; }
            if (Input.Keyboard["Jump"]) { Y -= _jumpVelocity; }

            var gravity = 8 * Gravity / (delta);
            if (Single.IsInfinity(gravity) || Single.IsNaN(gravity))
            {
                gravity = 0;
            }
            Y += gravity;
            _lastTick = count;
        }
    }
}
