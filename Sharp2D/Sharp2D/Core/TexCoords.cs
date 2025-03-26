﻿using System;
using OpenTK;
using OpenTK.Mathematics;
using Sharp2D.Core.Graphics;

namespace Sharp2D.Core
{
    public class TexCoords
    {
        public Vector2 BottomLeft { get; set; }

        public Vector2 BottomRight { get; set; }

        public Vector2 TopLeft { get; set; }

        public Vector2 TopRight { get; set; }

        public float SquardSize
        {
            get
            {
                return (float)Math.Sqrt(
                    ((BottomLeft.X - TopRight.X) * (BottomLeft.X - TopRight.X)) + 
                    ((BottomLeft.Y - TopRight.Y) * (BottomLeft.Y - TopRight.Y))
                    );
            }
        }

        public TexCoords(Vector2 BottomLeft, Vector2 BottomRight, Vector2 TopLeft, Vector2 TopRight)
        {
            this.BottomLeft = BottomLeft;
            this.BottomRight = TopRight;
            this.TopLeft = TopLeft;
            this.TopRight = BottomRight;
        }

        public TexCoords(Vector2 TopLeft, Vector2 BottomRight)
            : this(new Vector2(TopLeft.X, BottomRight.Y), BottomRight, TopLeft, new Vector2(BottomRight.X, TopLeft.Y)) { }

        public TexCoords(float Width, float Height, Texture texture) 
            : this (new Vector2(0f, 0f), new Vector2(Width / texture.TextureWidth, Height / texture.TextureHeight)) { }

        public TexCoords(float x, float y, float width, float height, Texture texture) 
            : this (new Vector2(x / texture.TextureWidth, y / texture.TextureHeight) , new Vector2(width / texture.TextureWidth, height / texture.TextureHeight)) { }
    }
}
