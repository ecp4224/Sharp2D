using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Utils;

namespace Sharp2D.Game.Sprite
{
    public class Sprite
    {
        public Texture Texture { get; set; }
        public Vector Position { get; set; }
        public Rectangle TexCoords { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float X
        {
            get
            {
                return Position.X;
            }
            set
            {
                Position.X = value;
            }
        }

        public float Y
        {
            get
            {
                return Position.Y;
            }
            set
            {
                Position.Y = value;
            }
        }
    }
}