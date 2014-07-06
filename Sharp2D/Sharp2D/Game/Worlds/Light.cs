using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Game.Sprites;
using System.Drawing;
using OpenTK;

namespace Sharp2D.Game.Worlds
{
    public class Light
    {
        internal Vector3 ShaderColor;
        private Color _color;
        private float _intense;
        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;

                ShaderColor = new Vector3((_color.R / 255f) * Intensity, (_color.G / 255f) * Intensity, (_color.B / 255f) * Intensity);
            }
        }
        public float X { get; set; }
        public float Y { get; set; }
        public float Intensity
        {
            get
            {
                return _intense;
            }
            set
            {
                _intense = value;

                ShaderColor = new Vector3((_color.R / 255f) * Intensity, (_color.G / 255f) * Intensity, (_color.B / 255f) * Intensity);
            }
        }
        public float Radius { get; set; }

        public Light(float X, float Y, float Intensity, float Radius, Color color)
        {
            this.X = X;
            this.Y = Y;
            this.Intensity = Intensity;
            this.Radius = Radius;
            this.Color = color;
        }

        public Light(float X, float Y, float Intensity, float Radius) : this(X, Y, Intensity, Radius, Color.White) { }

        public Light(float X, float Y, float Intensity) : this(X, Y, Intensity, 1f) { }

        public Light(float X, float Y) : this(X, Y, 1f, 1f) { } //TODO Make good defaults for this
    }
}
