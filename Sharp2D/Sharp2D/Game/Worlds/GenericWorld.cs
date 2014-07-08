using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using Sharp2D.Game.Tiled;
using Sharp2D.Game.Sprites.Tiled;
using Sharp2D.Game.Sprites;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Graphics.Shaders;
using System.Drawing;
using Sharp2D.Core.Logic;

namespace Sharp2D.Game.Worlds
{
    public abstract class GenericWorld : TiledWorld, ILightWorld
    {
        private float _brightness;
        internal Vector3 AmbientShaderColor;
        private Color _color;
        public Color AmbientColor
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;

                AmbientShaderColor = new Vector3(_color.R / 255f * _brightness, _color.G / 255f * _brightness, _color.B / 255f * _brightness);
            }
        }
        public float AmbientBrightness
        {
            get
            {
                return _brightness;
            }
            set
            {
                _brightness = value; 
                
                AmbientShaderColor = new Vector3(_color.R / 255f * _brightness, _color.G / 255f * _brightness, _color.B / 255f * _brightness);
            
            }
        }

        private List<Light> lights = new List<Light>();
        private GenericRenderJob job;

        public IList<Light> Lights
        {
            get
            {
                return lights.AsReadOnly();
            }
        }

        public override List<Sprites.Sprite> Sprites
        {
            get
            {
                return job.Batch.Sprites;
            }
        }

        protected override void OnLoad()
        {
            job = new GenericRenderJob(this);

            AmbientBrightness = 1f;
            AmbientColor = Color.White;

            SpriteRenderJob.SetDefaultJob(job);

            base.OnLoad();

            Layer[] layers = GetLayerByType(LayerType.ObjectLayer);
            foreach (Layer layer in layers)
            {
                TiledObject[] objects = layer.Objects;
                foreach (TiledObject obj in objects)
                {
                    if (obj.RawType.ToLower() == "light")
                    {
                        float x = obj.X;
                        float y = obj.Y;
                        float radius = Math.Max(obj.Width, obj.Height);
                        x = x + (radius / 2f);
                        y = y + (radius / 2f);

                        float intense = 1f;
                        Color color = Color.White;
                        if (obj.Properties != null)
                        {
                            if (obj.Properties.ContainsKey("brightness"))
                            {
                                float.TryParse(obj.Properties["brightness"], out intense);
                            }
                            if (obj.Properties.ContainsKey("color"))
                            {
                                color = Color.FromName(obj.Properties["color"]);
                            }
                        }

                        Light light = new Light(x, y, intense, radius, color);
                        AddLight(light);
                    }
                }
            }
        }

        protected override void OnDisplay()
        {
            base.OnDisplay();

            DefaultJob = job;

            if (job.Batch.Count > 0)
            {
                job.Batch.ForEach(delegate(Sprite s)
                {
                    UpdateSpriteLights(s);
                });
            }
        }

        public void AddLight(Light light)
        {
            List<Sprite> sprites = Sprites;
            float Y = light.Y + 18f;
            float xmin = light.X - (light.Radius);
            float xmax = light.X + (light.Radius);
            float ymin = Y - (light.Radius);
            float ymax = Y + (light.Radius);
            foreach (Sprite sprite in sprites)
            {
                lock (sprite.light_lock)
                {
                    if (sprite.X + sprite.Width >= xmin && sprite.X <= xmax && sprite.Y >= ymin && sprite.Y - sprite.Height <= ymax)
                    {
                        sprite.Lights.Add(light);
                    }
                }
            }
            foreach (Layer layer in Layers)
            {
                if (!layer.IsTileLayer)
                    continue;
                for (float x = xmin; x < xmax; x += 16)
                {
                    for (float y = ymin; y < ymax; y += 16)
                    {
                        TileSprite sprite = layer[x, y];
                        if (sprite == null || sprite.Lights.Contains(light))
                            continue;
                        lock (sprite.light_lock)
                        {
                            sprite.Lights.Add(light);
                        }
                    }
                }
            }

            lights.Add(light);
        }

        public override void AddSprite(Sprite s)
        {
            base.AddSprite(s);

            UpdateSpriteLights(s);
        }

        public override void AddSprite(Sprite s, SpriteRenderJob job)
        {
            base.AddSprite(s, job);

            UpdateSpriteLights(s);
        }

        public override void RemoveSprite(Sprite s)
        {
            base.RemoveSprite(s);

            lock (s.light_lock)
            {
                s.Lights.Clear();
            }
        }

        public void UpdateSpriteLights(Sprite sprite)
        {
            if (sprite is TileSprite)
                return;

            float X = sprite.X;
            float Y = sprite.Y;
            float Width = sprite.Width;
            float Height = sprite.Height;
            lock (sprite.light_lock)
            {
                sprite.Lights.Clear();
                foreach (Light light in lights)
                {
                    float xmin = light.X - (light.Radius);
                    float xmax = light.X + (light.Radius);
                    float ymin = light.Y - (light.Radius);
                    float ymax = light.Y + (light.Radius);

                    if (X + Width >= xmin && X <= xmax && Y >= ymin && Y - Height <= ymax)
                    {
                        sprite.Lights.Add(light);
                    }
                }
            }
        }
    }
}
