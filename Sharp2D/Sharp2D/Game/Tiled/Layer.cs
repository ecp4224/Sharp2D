using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sharp2D.Game.Tiled
{
    public class Layer : IDisposable
    {
        public static readonly uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        public static readonly uint FLIPPED_VERTICALLY_FLAG   = 0x40000000;
        public static readonly uint FLIPPED_DIAGONALLY_FLAG   = 0x20000000;

        [JsonProperty(PropertyName="height")]
        public int Height { get; private set; }

        [JsonProperty(PropertyName="width")]
        public int Width { get; private set; }

        [JsonProperty(PropertyName="name")]
        public string Name { get; private set; }

        [JsonProperty(PropertyName="opacity")]
        public float Opacity { get; set; }

        [JsonProperty(PropertyName="visible")]
        public bool Visible { get; private set; }

        [JsonProperty(PropertyName="x")]
        public int X { get; private set; }

        [JsonProperty(PropertyName="y")]
        public int Y { get; private set; }

        [JsonProperty(PropertyName="type")]
        public string RawType { get; private set; }

        [JsonProperty(PropertyName="properties")]
        public Dictionary<string, string> Properties { get; private set; }

        [JsonProperty(PropertyName="image")]
        public string ImagePath { get; private set; }

        [JsonProperty(PropertyName="objects")]
        public List<TiledObject> Objects { get; private set; }

        [JsonProperty(PropertyName="data")]
        public long[] Data { get; private set; }

        [JsonIgnore]
        private LayerType type;

        [JsonIgnore]
        public LayerType Type
        {
            get
            {
                if (type == null)
                {
                    if (RawType == "imagelayer")
                        type = LayerType.ImageLayer;
                    else if (RawType == "tilelayer")
                        type = LayerType.TileLayer;
                    else if (RawType == "objectgroup")
                        type = LayerType.ObjectLayer;
                    else
                        type = LayerType.Unknown;
                }
                return type;
            }
        }

        [JsonIgnore]
        public int LayerNumber { get; internal set; }

        [JsonIgnore]
        public Sprites.SpriteRenderJob RenderJob { get; internal set; }

        [JsonIgnore]
        public bool IsPlayerLayer
        {
            get
            {
                return Properties != null && Properties.ContainsKey("playerlayer") && Properties["playerlayer"].ToLower() == "true";
            }
        }

        [JsonIgnore]
        public bool IsImageLayer
        {
            get
            {
                return RawType == "imagelayer";
            }
        }

        [JsonIgnore]
        public bool IsObjectLayer
        {
            get
            {
                return RawType == "objectgroup";
            }
        }

        [JsonIgnore]
        public bool IsTileLayer
        {
            get
            {
                return RawType == "tilelayer";
            }
        }

        public void Dispose()
        {
            if (Properties != null) Properties.Clear();
            Properties = null;
            Data = null;
            RenderJob = null;
        }
    }

    public enum LayerType
    {
        ImageLayer,
        ObjectLayer,
        TileLayer,
        Unknown
    }
}
