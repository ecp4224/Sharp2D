using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using Sharp2D.Core.Physics;
using Sharp2D.Game.Sprites.Animations;

namespace Sharp2D.Game.Sprites
{
    public abstract class PhysicsSprite : AnimatedSprite, ICollidable
    {
        private float _x;
        public override float X
        {
            get
            {
                return _x;
            }

            set
            {
                var xSum = 0f;
                var collidables = Hitbox.CollidableCache;
                foreach (var c in collidables)
                {
                    if (c == this) { continue; }

                    var result = Hitbox.CheckCollision(this, c, new Vector2(value - _x, 0));

                    if (!result.WillIntersect) { continue; }

                    xSum += result.TranslationVector.X;
                    _y += result.TranslationVector.Y;
                }
                _x = value + xSum;
            }
        }

        private float _y;
        public override float Y
        {
            get
            {
                return _y;
            }

            set
            {
                var ySum = 0f;
                var collidables = Hitbox.CollidableCache;
                foreach (var c in collidables)
                {
                    if (c == this) { continue; }
                    var result = Hitbox.CheckCollision(this, c, new Vector2(0, value - _y));

                    if (!result.WillIntersect) { continue; }

                    _x += result.TranslationVector.X;
                    ySum += result.TranslationVector.Y;
                }
                
                _y = value + ySum;
            }
        }

        private readonly List<Hitbox> _hitboxes;
        public Hitbox Hitbox { get; set; }

        protected PhysicsSprite()
        {
            Hitbox.CollidableCache.Add(this);

            _hitboxes = Hitbox.Read(Name + "/" + Name + "_hitbox.json");
            if (_hitboxes == null) { Console.WriteLine("Well fuck"); return; }

            Hitbox = _hitboxes[0];
        }

        public CollisionResult CollidesWith(ICollidable c)
        {
            return Hitbox.CheckCollision(this, c, new Vector2(0, 0));
        }

        public void ChangeHitbox(string name) //todo handle invalid name
        {
            Hitbox = (from h in _hitboxes where h.Name == name select h).First();
        }
    }
}
