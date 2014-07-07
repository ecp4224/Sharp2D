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
        }

        protected override void OnDisplay()
        {
            base.OnDisplay();

            DefaultJob = job;
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
                if (sprite.X > xmin && sprite.X < xmax && sprite.Y > ymin && sprite.Y < ymax)
                {
                    lock (sprite.light_lock)
                    {
                        sprite.Lights.Add(light);
                    }
                }
            }
            foreach (Layer layer in Layers)
            {
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
                /*int s_i_x = Math.Max((int)(xmin / 16f), 0);
                int s_i_y = Math.Max((int)((ymin - 8f) / 16f), 0);

                int e_i_x = Math.Max((int)(xmax / 16f), 0);
                int e_i_y = Math.Max((int)((ymax - 8f) / 16f), 0);
                e_i_y++;


                for (int x = s_i_x; x < e_i_x; x++)
                {
                    for (int y = s_i_y; y < e_i_y; y++)
                    {
                        TileSprite sprite = layer[x, y];
                        if (sprite == null)
                            continue;

                        sprite.Lights.Add(light);
                    }
                }*/
            }
        }


        public void UpdateSpriteLights(Sprite sprite)
        {
            if (sprite is TileSprite)
                return;

            float X = sprite.X;
            float Y = sprite.Y;
            lock (sprite.light_lock)
            {
                sprite.Lights.Clear();
                foreach (Light light in lights)
                {
                    float xmin = light.X - (light.Radius);
                    float xmax = light.X + (light.Radius);
                    float ymin = light.Y - (light.Radius);
                    float ymax = light.Y + (light.Radius);
                    
                    if (X > xmin && X < xmax && Y > ymin && Y < ymax)
                    {
                        sprite.Lights.Add(light);
                    }
                }
            }
        }
    }
}
