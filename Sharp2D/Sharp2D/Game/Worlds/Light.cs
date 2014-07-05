using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Game.Sprites;

namespace Sharp2D.Game.Worlds
{
    public class Light
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Intensity { get; set; }
        public float Radius { get; set; }

        public Light(float X, float Y, float Intensity, float Radius)
        {
            this.X = X;
            this.Y = Y;
            this.Intensity = Intensity;
            this.Radius = Radius;
        }

        public Light(float X, float Y, float Intensity) : this(X, Y, Intensity, 1f) { }

        public Light(float X, float Y) : this(X, Y, 1f, 1f) { } //TODO Make good defaults for this
    }
}
