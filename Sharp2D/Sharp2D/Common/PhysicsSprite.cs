using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using Sharp2D.Core.Interfaces;

namespace Sharp2D
{
    public abstract class PhysicsSprite : ModuleSprite, ICollidable
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
                if (Hitbox != null)
                {
                    Hitbox.ForEachCollidable(delegate(ICollidable c)
                    {
                        if (c == this) { return; }

                        var result = Hitbox.CheckCollision(this, c, new Vector2(value - base.X + xSum, 0));

                        if (!result.WillIntersect) { return; }

                        xSum += result.TranslationVector.X;

                        if (OnCollision != null)
                        {
                            OnCollisionEventArgs args = new OnCollisionEventArgs(this, c);
                            OnCollision(this, args);
                        }

                    });
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
                if (Hitbox != null)
                {
                    Hitbox.ForEachCollidable(delegate(ICollidable c)
                    {
                        if (c == this) { return; }
                        var result = Hitbox.CheckCollision(this, c, new Vector2(0, value - base.Y + ySum));

                        if (!result.WillIntersect) { return; }

                        ySum += result.TranslationVector.Y;

                        if (OnCollision != null)
                        {
                            OnCollisionEventArgs args = new OnCollisionEventArgs(this, c);
                            OnCollision(this, args);
                        }

                    });
                }
                
                base.Y = value + ySum;
            }
        }

        private List<Hitbox> _hitboxes;
        public Hitbox Hitbox { get; set; }

        private string _hitbotPath = null;

        public string HitboxConfigPath
        {
            get { return _hitbotPath ?? (_hitbotPath = Name + "/" + Name + "_hitbox.json"); }
            set { _hitbotPath = value; }
        }

        protected override void OnLoad()
        {
            Hitbox.AddCollidable(this);

            _hitboxes = Hitbox.Read(HitboxConfigPath);
            if (_hitboxes == null) { Console.WriteLine("Well fuck"); return; }
            Hitbox = _hitboxes[0];
        }

        protected override void OnUnload()
        {
            Hitbox.RemoveCollidable(this);
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
