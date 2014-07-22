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
        public event EventHandler OnCollision;
        public override float X
        {
            get
            {
                return base.X;
            }

            set
            {
                var xSum = 0f;
                var collidables = Hitbox.CollidableCache;
                foreach (var c in collidables)
                {
                    if (c == this) { continue; }

                    var result = Hitbox.CheckCollision(this, c, new Vector2(value - base.X + xSum, 0));

                    if (!result.WillIntersect) { continue; }

                    xSum += result.TranslationVector.X;

                    if (OnCollision != null)
                    {
                        OnCollisionEventArgs args = new OnCollisionEventArgs(this, c);
                        OnCollision(this, args);
                    }
                }
                base.X = value + xSum;
            }
        }

        public override float Y
        {
            get
            {
                return base.Y;
            }

            set
            {
                var ySum = 0f;
                var collidables = Hitbox.CollidableCache;
                foreach (var c in collidables)
                {
                    if (c == this) { continue; }
                    var result = Hitbox.CheckCollision(this, c, new Vector2(0, value - base.Y + ySum));

                    if (!result.WillIntersect) { continue; }

                    ySum += result.TranslationVector.Y;

                    if (OnCollision != null)
                    {
                        OnCollisionEventArgs args = new OnCollisionEventArgs(this, c);
                        OnCollision(this, args);
                    }
                }
                
                base.Y = value + ySum;
            }
        }

        private readonly List<Hitbox> _hitboxes;
        public Hitbox Hitbox { get; set; }

        protected override void OnLoad()
        {
            base.OnLoad();

            Hitbox._collidableCache.Add(this);

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
