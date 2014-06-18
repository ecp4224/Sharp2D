using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Game.Tiled;
using Sharp2D.Game.Worlds.Tiled;

namespace Sharp2D.Game.Sprites.Tiled
{
    public class TileSprite : Sprite
    {
        public TiledWorld World { get; private set; }
        public int ID { get; private set; }
        public TileSet TileSet { get; private set; }
        public Layer ParentLayer { get; private set; }
        public bool FlippedDiagonally { get; internal set; }
        public bool FlippedHorizontally { get; internal set; }
        public bool FlippedVertically { get; internal set; }

        public TileSprite(int ID, TileSet parent, Layer parentLayer, int data_index, TiledWorld world)
        {
            this.World = world;
            this.ID = ID;
            this.TileSet = parent;
            this.ParentLayer = parentLayer;

            Width = TileSet.TileWidth;
            Height = TileSet.TileHeight;

            X = data_index % parentLayer.Width;
            Y = data_index / parentLayer.Width;

            X *= TileSet.TileWidth;
            Y *= TileSet.TileHeight;

            _setTexCoords();

            Texture = TileSet.TileTexture;
        }

        private void _setTexCoords()
        {
            if (World.texcoords_cache.ContainsKey(ID))
            {
                TexCoords = World.texcoords_cache[ID];
                return;
            }

            int tilepos = (ID - TileSet.FirstGID) + 1;
            float x, y, width, height;
            int step = (tilepos - 1) % TileSet.TilesPerRow, row = 0;

            for (int i = 0; i != ID; i++)
            {
                if (i % TileSet.TilesPerRow == 0 && i != 0)
                    row++;
            }

            x = TileSet.TileWidth * step;
            y = TileSet.TileHeight * row;
            width = x + TileSet.TileWidth;
            height = y + TileSet.TileHeight;

            TexCoords = new Core.Utils.TexCoords(x, y, width, height, TileSet.TileTexture);

            World.texcoords_cache.Add(ID, TexCoords);
        }


        protected override void OnLoad()
        {
            //TODO Do some loading of something
        }

        protected override void OnUnload()
        {
            //TODO Do some unloading of something
        }

        protected override void OnDispose()
        {
            //TODO Dispose something..
        }

        protected override void OnDisplay()
        {
            //TODO Display related stuff (badpokerface)
        }

        protected override void BeforeDraw()
        {
            //TODO Do something before it's drawn
        }
    }
}
