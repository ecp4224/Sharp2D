﻿using System;
using OpenTK;
using OpenTK.Mathematics;

namespace Sharp2D.Core.Interfaces
{
    public interface ICollidable
    {
        float X { get; set; }
        float Y { get; set; }
        float Width { get; set; }
        float Height { get; set; }

        event EventHandler OnCollision;

        Hitbox Hitbox { get; set; }

        CollisionResult CollidesWith(ICollidable c);
    }

    public struct CollisionResult
    {
        public bool WillIntersect;
        public bool Intersecting;
        public Vector2 TranslationVector;
    }
}
