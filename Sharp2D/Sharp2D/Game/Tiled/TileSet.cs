using System;
using System.Collections.Generic;
using System.Linq;
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
        public Dictionary<string, Dictionary<string, string>> TileProperties { get; private set; }

        [JsonIgnore]
        public Texture TileTexture { get; private set; }

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
        }

        public void Dispose()
        {
            Properties.Clear();
            if (TileProperties != null) //todo temp workaround, see whats causing this
                TileProperties.Clear();
            TileTexture = null;
        }
    }
}
