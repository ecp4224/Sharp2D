using System;
using System.Linq;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Settings;
using Sharp2D.Core.Utils;
using Sharp2D.Game.Tiled;
using Sharp2D.Game.Worlds;

namespace Sharp2D.Game.Sprites
{
    public abstract class PlayableSprite : PhysicsSprite
    {
        public float Speed { get; set; }
        private float _jumpVelocity;
        private const float Gravity = 100f;
        private long _lastTick = Screen.TickCount;

        public bool IsMoving
        {
            get;
            private set;
        }

        protected PlayableSprite()
        {
            Speed = 5;
        }

        public bool IsOnGround()
        {
            bool ground = false;
        
            if (!(CurrentWorld is GenericWorld)) { return false; }
            var tiles = (CurrentWorld as GenericWorld).GetTile(X, Y + Height / 2f, LayerType.TileLayer);
            foreach (var tile in tiles.Where(tile => tile != null && !tile.HasProperty("ignore"))) { ground = true; }

            return ground;
        }

        public override void Update()
        {
            base.Update();

            var count = Screen.TickCount;
            var delta = count - _lastTick;

            IsMoving = Input.Keyboard["MoveUp"] || Input.Keyboard["MoveDown"] || Input.Keyboard["MoveLeft"] || Input.Keyboard["MoveRight"];

            if (Input.Keyboard["MoveUp"]) { Y -= Speed; }
            if (Input.Keyboard["MoveDown"]) { Y += Speed; }
            if (Input.Keyboard["MoveLeft"]) { X -= Speed; }
            if (Input.Keyboard["MoveRight"]) { X += Speed; }
            if (Input.Keyboard["Jump"])
            {
                if (!IsOnGround())
                {
                    _jumpVelocity = 240f;
                }
                
            }

            float fact = 0;
            if (_jumpVelocity > 0)
            {
                fact = 2f * _jumpVelocity / delta;

                if (Single.IsInfinity(fact) || Single.IsNaN(fact))
                {
                    fact = 0;
                }

                _jumpVelocity -= fact;
                if (_jumpVelocity < 0.8f || fact < 0.35f)
                {
                    _jumpVelocity = 0;
                }
            }
            
            var gravity = Gravity / (delta) + 3.5f;
            Logger.Log("Fact: " + fact + " jumpv: " + _jumpVelocity + " y: " + Y + " grav: " + gravity);
            if (Single.IsInfinity(gravity) || Single.IsNaN(gravity))
            {
                gravity = 0;
            }
            Y += gravity - fact;
            _lastTick = count;
        }
    }
}
