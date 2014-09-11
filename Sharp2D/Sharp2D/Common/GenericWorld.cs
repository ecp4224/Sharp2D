using System;
using System.Collections.Generic;
using OpenTK;
using System.Drawing;
using Sharp2D.Game.Worlds;
using Sharp2D.Game.Sprites;

namespace Sharp2D
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

        internal List<Light> lights = new List<Light>();
        internal List<Light> dynamicLights = new List<Light>();
        private GenericRenderJob job;

        public IList<Light> Lights
        {
            get
            {
                return lights.AsReadOnly();
            }
        }

        public override List<Sprite> Sprites
        {
            get
            {
                return job.Batch.Sprites;
            }
        }

        /// <summary>
        /// Invoke a delegate while blocking the render thread. This operation is only really useful for updates to render info that may take a few frames to execute, and
        /// you don't want any drawing during that time
        /// </summary>
        /// <param name="action"></param>
        public void InvokeWithRenderLock(Action action)
        {
            Screen.ValidateOpenGLUnsafe("InvokeWithRenderLock");
            lock (job.RenderLock)
            {
                action();
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
                    if (obj.RawType.ToLower() != "light") continue;
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

                    var light = new Light(x, y, intense, radius, color, LightType.StaticPointLight);
                    AddLight(light);
                }
            }
        }

        protected override void OnDisplay()
        {
            base.OnDisplay();

            DefaultJob = job;

            if (job.Batch.Count > 0)
            {
                job.Batch.ForEach(UpdateSpriteLights);
            }
        }

        public Light AddLight(float X, float Y, LightType LightType)
        {
            var light = new Light(X, Y, LightType);
            AddLight(light);
            return light;
        }

        public Light AddLight(float X, float Y, float Intensity, LightType LightType)
        {
            var light = new Light(X, Y, Intensity, LightType);
            AddLight(light);
            return light;
        }

        public Light AddLight(float X, float Y, float Intensity, float Radius, LightType LightType)
        {
            var light = new Light(X, Y, Intensity, Radius, LightType);
            AddLight(light);
            return light;
        }

        public Light AddLight(float X, float Y, float Intensity, float Radius, Color color, LightType LightType)
        {
            var light = new Light(X, Y, Intensity, Radius, color, LightType);
            AddLight(light);
            return light;
        }

        public void AddLight(Light light)
        {
            if (lights.Contains(light))
                throw new ArgumentException("This light is already in this world!");

            if (light.IsStatic)
            {
                _cullSpritesForLights(light);
            }

            if (light.IsStatic)
            {
                lights.Add(light);
            }
            else
            {
                dynamicLights.Add(light);
            }

            light.World = this;
            light.Load();
        }

        [Obsolete("StaticLights cant move and DynamicLights are dynamic")]
        public void UpdateLight(Light light)
        {
            Screen.ValidateOpenGLUnsafe("UpdateLight");

            /*InvokeWithRenderLock(delegate
            {
                foreach (Sprite s in light.affected)
                {
                    lock (s.light_lock)
                    {
                        s.Lights.Remove(light);
                    }
                }

                _cullSpritesForLights(light);
            });*/
        }

        private void _cullSpritesForLights(Light light)
        {
            if (!light.IsStatic)
                return;

            List<Sprite> sprites = Sprites;
            float xmin = light.X - (light.Radius);
            float xmax = light.X + (light.Radius);
            float ymin = light.Y - (light.Radius);
            float ymax = light.Y + (light.Radius);
            foreach (Sprite sprite in sprites)
            {
                if (!sprite.IsStatic)
                    continue;

                lock (sprite.light_lock)
                {
                    if (sprite.X + (sprite.Width / 2f) >= xmin && sprite.X - (sprite.Width / 2f) <= xmax && sprite.Y + (sprite.Height / 2f) >= ymin && sprite.Y - (sprite.Height / 2f) <= ymax)
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
                        TileSprite sprite = layer[x, y]; //TileSprites are always static
                        if (sprite == null || sprite.Lights.Contains(light))
                            continue;
                        lock (sprite.light_lock)
                        {
                            sprite.Lights.Add(light);
                        }
                    }
                }
            }
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

            if (!s.IsStatic) return;
            lock (s.light_lock)
            {
                s.Lights.Clear();
            }
        }

        public void UpdateSpriteLights(Sprite sprite)
        {
            if (!sprite.IsStatic || sprite is TileSprite)
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

                    if (X + (Width / 2f) >= xmin && X - (Width / 2f) <= xmax && Y + (Height / 2f) >= ymin && Y - (Height / 2f) <= ymax)
                    {
                        sprite.Lights.Add(light);
                    }
                }
            }
        }
    }
}
