using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Game.Sprites;
using System.Drawing;
using OpenTK;
using Sharp2D.Core.Logic;

namespace Sharp2D.Game.Worlds
{
    public class Light : IAttachable
    {
        internal List<Sprite> affected = new List<Sprite>();

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

        private GenericWorld _world;
        public GenericWorld World
        {
            get
            {
                return _world;
            }
            internal set
            {
                _world = value;
            }
        }

        private float x;
        private float y;
        public float X
        {
            get
            {
                return x;
            }
            set
            {
                float dif = value - x;

                x = value;

                foreach (IAttachable attach in _children)
                {
                    attach.X += dif;
                }

                if (_world != null)
                    _world.UpdateLight(this);
            }
        }
        public float Y
        {
            get
            {
                return y;
            }
            set
            {
                float dif = value - y;

                y = value;

                foreach (IAttachable attach in _children)
                {
                    attach.Y += dif;
                }


                if (_world != null)
                    _world.UpdateLight(this);
            }
        }
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


        private List<IAttachable> _children = new List<IAttachable>();
        private List<IAttachable> _parents = new List<IAttachable>();
        public IList<IAttachable> Children
        {
            get { return _children; }
        }

        public IList<IAttachable> Parents
        {
            get { return _parents; }
        }

        public void Attach(IAttachable ToAttach)
        {
            if (_children.Contains(ToAttach))
                throw new ArgumentException("This attachable is already attached!");

            _children.Add(ToAttach);
            ToAttach.Parents.Add(this);
        }
    }
}
