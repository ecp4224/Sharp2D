using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Graphics;
using System.IO;
using Newtonsoft.Json;

namespace Sharp2D.Game.Tiled
{
    public class TileSet : IDisposable
    {
        [JsonProperty(PropertyName="firstgid")]
        public int FirstGID { get; private set; }

        [JsonProperty(PropertyName="image")]
        public string ImagePath { get; private set; }

        [JsonProperty(PropertyName="imageheight")]
        public int ImageHeight { get; private set; }

        [JsonProperty(PropertyName="imagewidth")]
        public int ImageWidth { get; private set; }

        [JsonProperty(PropertyName="name")]
        public string Name { get; private set; }

        [JsonProperty(PropertyName="margin")]
        public int Margin { get; private set; }

        [JsonProperty(PropertyName="spacing")]
        public int Spacing { get; private set; }

        [JsonProperty(PropertyName="tileheight")]
        public int TileHeight { get; private set; }

        [JsonProperty(PropertyName="tilewidth")]
        public int TileWidth { get; private set; }

        [JsonProperty(PropertyName="properties")]
        public Dictionary<string, string> Properties { get; private set; }

        [JsonProperty(PropertyName="tileproperties")]
        private Dictionary<string, Dictionary<string, string>> _tileProperties { get; set; }

        [JsonIgnore]
        public Texture TileTexture { get; private set; }

        [JsonIgnore]
        public TileProperties TileProperties { get; private set; }

        public int TilesPerRow
        {
            get
            {
                return ImageWidth / TileWidth;
            }
        }

        public int RowCount
        {
            get
            {
                return ImageHeight / TileHeight;
            }
        }

        public bool Contains(long id)
        {
            return id >= FirstGID && id <= ((TilesPerRow * RowCount) + (FirstGID - 1));
        }

        internal void LoadTexture()
        {
            if (File.Exists("worlds/" + ImagePath))
                ImagePath = "worlds/" + ImagePath;

            TileTexture = Texture.NewTexture(ImagePath);
            try
            {
                TileTexture.LoadTextureFromFile();
            }
            catch
            {
                try
                {
                    TileTexture.LoadTextureFromResource();
                }
                catch
                {
                    throw new System.IO.FileNotFoundException("Could not find texture for tileset", ImagePath);
                }
            }

            if (Properties == null)
                Properties = new Dictionary<string, string>();
            if (_tileProperties == null)
            {
                _tileProperties = new Dictionary<string, Dictionary<string, string>>();
            }

            TileProperties = new TileProperties(_tileProperties, FirstGID);
        }

        public void Dispose()
        {
            if (Properties != null)
                Properties.Clear();
            if (TileProperties != null)
                TileProperties.Clear();
            TileTexture = null;
        }
    }

    public class TileProperties : Dictionary<string, Dictionary<string, string>>
    {
        private int FirstGID;
        public TileProperties(Dictionary<string, Dictionary<string, string>> properties, int FirstGID)
            : base(properties)
        {
            this.FirstGID = FirstGID;
        }

        public bool ContainsKey(int ID)
        {
            ID -= FirstGID;

            return base.ContainsKey("" + ID);
        }

        public Dictionary<string, string> this[int ID]
        {
            get
            {
                ID -= FirstGID;

                return base["" + ID];
            }
            set
            {
                ID -= FirstGID;

                base["" + ID] = value;
            }
        }
    }
}
