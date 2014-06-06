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
        public string Type { get; private set; }

        [JsonProperty(PropertyName="properties")]
        public Dictionary<string, string> Properties { get; private set; }

        [JsonProperty(PropertyName="image")]
        public string ImagePath { get; private set; }

        //private TiledObject[] objects;

        [JsonProperty(PropertyName="data")]
        public long[] Data { get; private set; }

        [JsonIgnore]
        public int LayerNumber { get; internal set; }

        [JsonIgnore]
        public Sprites.SpriteRenderJob RenderJob { get; internal set; }

        public bool IsImageLayer
        {
            get
            {
                return Type == "imagelayer";
            }
        }

        public bool IsObjectLayer
        {
            get
            {
                return Type == "objectgroup";
            }
        }

        public bool IsTileLayer
        {
            get
            {
                return Type == "tilelayer";
            }
        }

        public void Dispose()
        {
            Properties.Clear();
            Data = null;
            RenderJob = null;
        }
    }
}
