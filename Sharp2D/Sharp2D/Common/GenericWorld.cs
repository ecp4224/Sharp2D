using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Interfaces;
using Sharp2D.Game.Worlds;
using Sharp2D.Game.Sprites;
using Sharp2D.Render;
using SkiaSharp;

namespace Sharp2D
{
    public abstract class GenericWorld : TiledWorld, ILightWorld
    {
        private float _brightness;
        internal Vector3 AmbientShaderColor;
        private SKColor _color;
        public SKColor AmbientColor
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;

                AmbientShaderColor = new Vector3(_color.Red / 255f * _brightness, _color.Green / 255f * _brightness, _color.Blue / 255f * _brightness);
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
                
                AmbientShaderColor = new Vector3(_color.Red / 255f * _brightness, _color.Green / 255f * _brightness, _color.Blue / 255f * _brightness);
            
            }
        }

        internal List<Light> lights = new List<Light>();
        internal List<Light> dynamicLights = new List<Light>();
        private GenericSpriteRenderJob spriteJob;
        private GuiRenderJob guiJob;
        private TextRenderJob textJob;

        public GenericSpriteRenderJob SpriteRenderJob
        {
            get { return spriteJob; }
        }

        public GuiRenderJob GuiRenderJob
        {
            get { return guiJob; }
        }

        public TextRenderJob TextRenderJob
        {
            get { return textJob; }
        }

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
                return spriteJob.Batch.Sprites;
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
            lock (GenericRenderJob.RenderLock)
            {
                action();
            }
        }
        
        public static SKColor ColorFromName(string name)
        {
            switch(name.ToLower())
            {
                case "black": return SKColors.Black;
                case "white": return SKColors.White;
                case "red": return SKColors.Red;
                case "green": return SKColors.Green;
                case "blue": return SKColors.Blue;
                case "yellow": return SKColors.Yellow;
                case "cyan": return SKColors.Cyan;
                case "magenta": return SKColors.Magenta;
                // Add additional mappings as needed.
                default:
                    // Fallback to transparent or throw an exception if an unknown name is provided.
                    return SKColors.Transparent;
            }
        }

        protected override void OnLoad()
        {
            spriteJob = new GenericSpriteRenderJob(this);
            guiJob = new GuiRenderJob(this);
            textJob = new TextRenderJob(this);

            AmbientBrightness = 1f;
            AmbientColor = new SKColor(255, 255, 255, 255);

            base.OnLoad();

            AddRenderJob(textJob);

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
                    SKColor color = SKColors.White;
                    if (obj.Properties != null)
                    {
                        if (obj.Properties.ContainsKey("brightness"))
                        {
                            float.TryParse(obj.Properties["brightness"], out intense);
                        }
                        if (obj.Properties.ContainsKey("color"))
                        {
                            color = ColorFromName(obj.Properties["color"]);
                        }
                    }

                    var light = new Light(x, y, intense, radius, color, LightType.StaticPointLight);
                    AddLight(light);
                }
            }
        }

        protected override void OnInitialDisplay()
        {
            base.OnInitialDisplay();

            DefaultJob = spriteJob;

            if (spriteJob.Batch.Count > 0)
            {
                spriteJob.Batch.ForEach(UpdateSpriteLights);
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

        public Light AddLight(float X, float Y, float Intensity, float Radius, SKColor color, LightType LightType)
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

        public override void AddSprite(Sprite s, BatchRenderJob job)
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
                        Logger.Log($"Sprite {sprite.Name} is affected by light at ({light.X}, {light.Y})");
                    }
                }
            }
        }
    }
}
