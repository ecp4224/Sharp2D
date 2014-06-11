using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Sharp2D.Core.Graphics;

namespace Sharp2D.Core.Utils
{
    public class TexCoords
    {
        public Vector2 BottomLeft { get; set; }

        public Vector2 BottomRight { get; set; }

        public Vector2 TopLeft { get; set; }

        public Vector2 TopRight { get; set; }

        public TexCoords(Vector2 BottomLeft, Vector2 BottomRight, Vector2 TopLeft, Vector2 TopRight)
        {
            this.BottomLeft = BottomLeft;
            this.BottomRight = BottomRight;
            this.TopLeft = TopLeft;
            this.TopRight = TopRight;
        }

        public TexCoords(Vector2 TopLeft, Vector2 BottomRight)
            : this(new Vector2(TopLeft.X, BottomRight.Y), BottomRight, TopLeft, new Vector2(BottomRight.X, TopLeft.Y)) { }

        public TexCoords(float Width, float Height, Texture texture) 
            : this (new Vector2(0f, 0f), new Vector2(Width / texture.TextureWidth, Height / texture.TextureHeight)) { }

        public TexCoords(float x, float y, float width, float height, Texture texture) 
            : this (new Vector2(x / texture.TextureWidth, y / texture.TextureHeight) , new Vector2(width / texture.TextureWidth, height / texture.TextureHeight)) { }
    }
}
