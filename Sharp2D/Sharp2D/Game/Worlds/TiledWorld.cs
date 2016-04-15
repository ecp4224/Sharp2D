using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sharp2D.Core;
using Sharp2D.Game.Sprites;

namespace Sharp2D.Game.Worlds
{
    public abstract class TiledWorld : BatchJobWorld
    {
        internal readonly Dictionary<int, TexCoords> texcoords_cache = new Dictionary<int, TexCoords>();
        [JsonProperty(PropertyName = "height")]
        public int Height { get; private set; }

        [JsonProperty(PropertyName = "tileheight")]
        public int TileHeight { get; private set; }

        [JsonProperty(PropertyName = "tilewidth")]
        public int TileWidth { get; private set; }

        [JsonProperty(PropertyName = "version")]
        public int Version { get; private set; }

        [JsonProperty(PropertyName = "width")]
        public int Width { get; private set; }

        [JsonProperty(PropertyName = "properties")]
        public Dictionary<string, string> Properties;

        [JsonProperty(PropertyName = "tilesets")]
        public TileSet[] TileSets { get; private set; }

        [JsonProperty(PropertyName="layers")]
        public Layer[] Layers { get; private set; }

        [JsonProperty(PropertyName="backgroundcolor")]
        public string BackgroundColor { get; private set; }

        public int PixelWidth
        {
            get
            {
                return Width * TileWidth;
            }
        }

        public int PixelHeight
        {
            get
            {
                return Height * TileHeight;
            }
        }

        public TileSet FindTileset(long id)
        {
            foreach (TileSet tileSet in TileSets)
            {
                if (id >= tileSet.FirstGID)
                {
                    if (tileSet.Contains(id))
                        return tileSet;
                }
            }
            return null;
        }

        public Layer[] GetLayerByType(LayerType Type)
        {
            List<Layer> layers = new List<Layer>();
            foreach (Layer layer in Layers)
            {
                if (layer.Type == Type)
                    layers.Add(layer);
            }

            return layers.ToArray<Layer>();
        }

        public TileSprite GetTile(float pixelx, float pixely, Layer layer)
        {
            return layer[pixelx, pixely];
        }

        public TileSprite GetTile(int tilex, int tiley, Layer layer)
        {
            return layer[tilex, tiley];
        }

        public TileSprite[] GetTile(float pixelx, float pixely, LayerType type)
        {
            Layer[] layers = GetLayerByType(type);
            TileSprite[] sprites = new TileSprite[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                sprites[i] = layers[i][pixelx, pixely];
            }

            return sprites;
        }

        public TileSprite[] GetTile(int tilex, int tiley, LayerType type)
        {
            Layer[] layers = GetLayerByType(type);
            TileSprite[] sprites = new TileSprite[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                sprites[i] = layers[i][tilex, tiley];
            }

            return sprites;
        }

        protected override void OnBackgroundDisplay()
        {
            foreach (Layer layer in Layers)
            {
                if (layer.IsTileLayer)
                {
                    for (int i = 0; i < layer.Data.Length; i++)
                    {
                        TileSprite t = layer[i];
                        if (t == null)
                            continue;

                        if (t.IsCollidable)
                        {
                            Hitbox.RemoveCollidable(t);
                        }
                    }
                }
            }
        }

        protected override void OnResumeDisplay()
        {
            foreach (Layer layer in Layers)
            {
                if (layer.IsTileLayer)
                {
                    for (int i = 0; i < layer.Data.Length; i++)
                    {
                        TileSprite t = layer[i];
                        if (t.IsCollidable)
                        {
                            Hitbox.AddCollidable(t);
                        }
                    }
                }
            }
        }

        protected override void OnLoad()
        {
            if (!File.Exists(Name))
            {
                Width = 0;
                Height = 0;
                TileHeight = 0;
                TileWidth = 0;
                Version = -1;
                Properties = new Dictionary<string, string>();
                TileSets = new TileSet[0];
                Layers = new Layer[0];
                return;
            }

            string text = File.ReadAllText(Name);
            JObject obj = JObject.Parse(text);
            Height = (int)obj["height"];
            Width = (int)obj["width"];
            TileHeight = (int)obj["tileheight"];
            TileWidth = (int)obj["tilewidth"];
            Version = (int)obj["version"];
            Properties = obj["properties"].ToObject<Dictionary<string, string>>();
            TileSets = obj["tilesets"].Select(t => t.ToObject<TileSet>()).ToArray();
            Layers = obj["layers"].Select(l => l.ToObject<Layer>()).ToArray();

            for (int i = 0; i < Layers.Length; i++)
            {
                Layers[i].LayerNumber = i + 1;
            }

            foreach (TileSet tileset in TileSets)
            {
                tileset.LoadTexture();
            }

            foreach (Layer layer in Layers)
            {
                
                if (layer.IsTileLayer)
                {
                    for (int i = 0; i < layer.Data.Length; i++)
                    {
                        long id = layer.Data[i];
                        if (id == 0)
                            continue;

                        bool flipH = (id & Layer.FLIPPED_HORIZONTALLY_FLAG) > 0;
                        bool flipV = (id & Layer.FLIPPED_VERTICALLY_FLAG)   > 0;
                        bool flipD = (id & Layer.FLIPPED_DIAGONALLY_FLAG)   > 0;

                        id &= ~(Layer.FLIPPED_DIAGONALLY_FLAG | Layer.FLIPPED_HORIZONTALLY_FLAG | Layer.FLIPPED_VERTICALLY_FLAG);

                        TileSet set = FindTileset(id);

                        TileSprite sprite = new TileSprite((int)id, set, layer, i, this);

                        sprite.Load();

                        layer.SetTile(i, sprite);
                    }
                }
            }
        }

        protected override void OnInitialDisplay()
        {
            foreach (TileSet tileset in TileSets)
            {
                if (!tileset.TileTexture.Created)
                    tileset.TileTexture.Create();
            }
        }

        protected override void OnUnload()
        {
            
        }

        protected override void OnDispose()
        {
            Properties.Clear();
            foreach (Layer l in Layers)
            {
                l.Dispose();
            }

            foreach (TileSet tileset in TileSets)
            {
                tileset.Dispose();
            }

            texcoords_cache.Clear();
        }
    }
}
