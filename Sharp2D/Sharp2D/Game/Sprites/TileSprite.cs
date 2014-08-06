using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.Drawing;
using Sharp2D;
using Sharp2D.Core;
using Sharp2D.Core.Interfaces;
using Sharp2D.Game.Worlds;

namespace Sharp2D.Game.Sprites
{
    public class TileSprite : Sprite, ICollidable
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

            Layer = 1;

            //Y = -(parentLayer.Height - Y);
            //Y = parentLayer.Height - Y;

            _setTexCoords();

            Texture = TileSet.TileTexture;

            if (parent.TileProperties.ContainsKey(ID) &&
                parent.TileProperties[ID].ContainsKey("collide"))
            {
                Hitbox = new Hitbox("Space", new List<Vector2>
                {
                    new Vector2(0, 0),
                    new Vector2(Width, 0),
                    new Vector2(Width, Height),
                    new Vector2(0, Height)
                });
                Hitbox.AddCollidable(this);
            }
        }

        public bool HasProperty(string key)
        {
            return TileSet.TileProperties.ContainsKey(ID) && TileSet.TileProperties[ID].ContainsKey(key);
        }

        public string GetProperty(string key)
        {
            return !HasProperty(key) ? null : TileSet.TileProperties[ID][key];
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
            y = TileSet.TileTexture.TextureHeight + y;
            width = x + TileSet.TileWidth;
            height = y + TileSet.TileHeight;

            TexCoords = new TexCoords(x, y, width, height, TileSet.TileTexture);

            World.texcoords_cache.Add(ID, TexCoords);

            IsStatic = true;
        }

        private bool cached_alpha_answer;
        private bool valid_alpha_answer = false;
        public bool TileHasAlpha
        {
            get
            {
                if (valid_alpha_answer)
                    return cached_alpha_answer;

                int tilepos = (ID - TileSet.FirstGID) + 1;
                int step = (tilepos - 1) % TileSet.TilesPerRow, row = 0;

                for (int i = 0; i != ID; i++)
                {
                    if (i % TileSet.TilesPerRow == 0 && i != 0)
                        row++;
                }

                int x = TileSet.TileWidth * step;
                int y = TileSet.TileHeight * row;
                int width = TileSet.TileWidth;
                int height = TileSet.TileHeight;


                Bitmap testBmp = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(testBmp))
                {
                    g.DrawImage(Texture.Bitmap, new RectangleF(0, 0, width, height), new RectangleF(x, y, width, height), GraphicsUnit.Pixel);
                }

                cached_alpha_answer = testBmp.ContainsAlpha();
                valid_alpha_answer = true;

                testBmp.Dispose();
                return cached_alpha_answer;
            }
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

        public event EventHandler OnCollision;
        public Hitbox Hitbox { get; set; }

        public CollisionResult CollidesWith(ICollidable c)
        {
            return new CollisionResult {Intersecting = false, WillIntersect = false, TranslationVector = new Vector2()};
        }

        public override string Name
        {
            get { return "Tile #" + ID; }
        }
    }
}
