using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core;
using Sharp2D.Core.Graphics.Shaders;
using Sharp2D.Game.Sprites;

namespace Sharp2D.Game.Worlds
{
    internal class DrawBatch : SpriteBatch
    {
        public List<Sprite> AlphaSprites = new List<Sprite>();
        public int Type;
        private int _drawCount;
        public int DrawCount
        {
            get
            {
                return Type == 0 ? Count : _drawCount;
            }
        }

        public override int Count
        {
            get
            {
                return Type != 2 ? base.Count : AlphaSprites.Count;
            }
        }

        public override void ForEach(Action<Shader, Texture, Sprite> callBack)
        {
            if (Type != 2)
            {
                base.ForEach(callBack);
            }
            else
            {
                foreach (var sprite in AlphaSprites)
                {
                    callBack(sprite.Shader, sprite.Texture, sprite);
                }
            }
        }

        public override void ForEach(Action<Sprite> callBack)
        {
            if (Type != 2)
            {
                base.ForEach(callBack);
            }
            else
            {
                foreach (var sprite in AlphaSprites)
                {
                    callBack(sprite);
                }
            }
        }

        public override void Add(Sprite sprite)
        {
            if (Type != 2)
            {
                base.Add(sprite);
            }
            else
            {
                AlphaSprites.Add(sprite);
            }
            if (Type == 1)
                _drawCount += sprite.Lights.Count;
            else
                _drawCount += sprite.Lights.Count + 1;
        }

        public override void Remove(Sprite sprite)
        {
            if (Type != 2)
            {
                base.Remove(sprite);
            }
            else
            {
                AlphaSprites.Remove(sprite);
                Order();
            }
        }

        public override void PrepareForDraw()
        {
            if (Type == 2) Order();
        }

        private void Order()
        {
            if (Type == 2)
            {
                AlphaSprites.Sort();
            }
        }
    }
    public class GenericSpriteRenderJob : GenericRenderJob
    {
        private readonly DrawPass[] _defaultPasses =
        {
            new DrawNoAlpha(),
            new DrawNoAlphaLight(),
            new DrawAlpha()
        };
        public GenericSpriteRenderJob(GenericWorld parent) : base(parent)
        {
        }

        public override DrawPass[] DefaultPasses
        {
            get { return _defaultPasses; }
        }

        protected override SpriteBatch[] CreateCulledBatches()
        {
            var drawPasses = base.DrawPasses;

            var batches = new DrawBatch[drawPasses.Count];
            for (int i = 0; i < batches.Length; i++)
            {
                batches[i] = new DrawBatch();
                DrawBatch batch = batches[i];
                DrawPass pass = drawPasses[i];

                pass.SetupBatch(batch);
            }

            Batch.ForEach(delegate(Sprite sprite)
            {
                if (sprite.IsOffScreen || !sprite.IsVisible || Math.Abs(sprite.Alpha) < 0.001f) return;
                CullLights(sprite);
                for (int i = 0; i < batches.Length; i++)
                {
                    if (drawPasses[i].MeetsRequirements(sprite))
                        batches[i].Add(sprite);
                }
            });

            float cx = -Screen.Camera.X;
            float cy = Screen.Camera.Y;

            float cullWidth = 380f * (Screen.Camera.Z / 100f);
            float cullHeight = 256f * (Screen.Camera.Z / 100f);
            cullWidth *= (Screen.Settings.GameSize.Width / 1024f);
            cullHeight *= (Screen.Settings.GameSize.Height / 720f);
            cullWidth /= 2f;
            cullHeight /= 2f;
            var parent = base.ParentWorld;

            foreach (Layer layer in parent.Layers)
            {
                if (!layer.IsTileLayer)
                    continue;
                float ex = cx + (cullWidth + (3f * parent.TileWidth));
                float ey = cy + cullHeight;
                float sx = cx - cullWidth;
                float sy = cy - cullHeight;

                int sIx = Math.Max((int)(sx / parent.TileWidth), 0);
                int sIy = Math.Max((int)Math.Ceiling((sy - 8f) / parent.TileHeight), 0);

                int eIx = Math.Max((int)(ex / parent.TileWidth), 0);
                int eIy = Math.Max((int)Math.Ceiling((ey - 8f) / parent.TileHeight), 0);


                for (int x = sIx; x <= eIx; x++)
                {
                    for (int y = sIy; y < eIy; y++)
                    {
                        TileSprite sprite = layer[x, y];
                        if (sprite == null)
                            continue;

                        CullLights(sprite);
                        for (int i = 0; i < batches.Length; i++)
                        {
                            if (drawPasses[i].MeetsRequirements(sprite))
                                batches[i].Add(sprite);
                        }
                    }
                }
            }

            return batches;
        }

        private void CullLights(Sprite sprite)
        {
            if (sprite.IgnoreLights)
            {
                sprite.dynamicLights.Clear();
                sprite.Lights.Clear();
                return;
            }

            float sx = sprite.X;
            float sy = sprite.Y;
            float sw = sprite.Width;
            float sh = sprite.Height;

            foreach (Light light in ParentWorld.dynamicLights)
            {
                if (light.Intensity == 0 || light.Radius == 0)
                    continue;

                float Y = light.Y + 18f;
                float xmin = light.X - (light.Radius) - 8;
                float xmax = light.X + (light.Radius) + 8;
                float ymin = Y - (light.Radius) - 8;
                float ymax = Y + (light.Radius) + 8;
                if (sx + (sw / 2f) >= xmin && sx - (sw / 2f) <= xmax && sy + (sh / 2f) >= ymin && sy - (sh / 2f) <= ymax)
                {
                    sprite.dynamicLights.Add(light);
                }
            }
            if (sprite.IsStatic) return;
            foreach (Light light in ParentWorld.lights)
            {
                if (light.Intensity == 0 || light.Radius == 0)
                    continue;

                float Y = light.Y + 18f;
                float xmin = light.X - (light.Radius);
                float xmax = light.X + (light.Radius);
                float ymin = Y - (light.Radius);
                float ymax = Y + (light.Radius);
                if (sprite.X + (sprite.Width / 2f) >= xmin && sprite.X - (sprite.Width / 2f) <= xmax && sprite.Y + (sprite.Height / 2f) >= ymin && sprite.Y - (sprite.Height / 2f) <= ymax)
                {
                    sprite.Lights.Add(light);
                }
            }
        }
    }
}
