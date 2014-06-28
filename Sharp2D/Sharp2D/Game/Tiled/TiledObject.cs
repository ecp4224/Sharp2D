using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Sharp2D.Game.Worlds.Tiled;
using Newtonsoft.Json;
using Sharp2D.Core.Physics;

namespace Sharp2D.Game.Tiled
{
    public class TiledObject : ICollidable, IDisposable, ICloneable
    {
        public event EventHandler OnCollision;

        [JsonProperty(PropertyName="ellipse")]
        public bool IsEllipse { get; private set; }

        [JsonProperty(PropertyName = "height")]
        public float Height { get; set; }

        [JsonProperty(PropertyName = "width")]
        public float Width { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "visible")]
        public bool IsVisible { get; set; }

        [JsonProperty(PropertyName = "x")]
        public float X { get; set; }

        [JsonProperty(PropertyName = "y")]
        public float Y { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string RawType { get; private set; }

        [JsonProperty(PropertyName = "polygon")]
        public List<Vector2> Polygon { get; private set; }

        [JsonProperty(PropertyName = "isBound")]
        public bool IsBound { get; private set; }

        [JsonProperty(PropertyName = "properties")]
        public Dictionary<string, string> Properties { get; private set; }

        [JsonIgnore]
        public bool IsSquare
        {
            get
            {
                return !IsEllipse && Polygon == null;
            }
        }

        [JsonIgnore]
        public TiledWorld World { get; internal set; }

        private Hitbox _hitbox;
        public Hitbox Hitbox
        {
            get
            {
                if (_hitbox == null)
                    _hitbox = new Hitbox(Name, Polygon);

                return _hitbox;
            }
            set
            {
                _hitbox = value;
            }
        }

        public TEnum GetTypeAsEnum<TEnum>() where TEnum : struct, IConvertible, IFormattable 
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("TEnum must be a type of Enum!");
            }

            TEnum result;
            Enum.TryParse<TEnum>(RawType, true, out result);
            return result;
        }

        public CollisionResult CollidesWith(ICollidable c)
        {
            return Hitbox.CheckCollision(this, c, new Vector2(0, 0));
        }

        public void Dispose()
        {
            if (Polygon != null)
                Polygon.Clear();
            if (Properties != null)
                Properties.Clear();

            _hitbox = null;
        }

        public object Clone()
        {
            TiledObject obj = new TiledObject();

            obj.IsEllipse = IsEllipse;
            obj.Height = Height;
            obj.Width = Width;
            obj.Name = Name;
            obj.IsVisible = IsVisible;
            obj.X = X;
            obj.Y = Y;
            obj.RawType = RawType;
            obj.Polygon = Polygon;
            obj.IsBound = IsBound;
            obj.Properties = Properties;
            obj.World = World;
            obj._hitbox = _hitbox;

            return obj;
        }
    }
}
