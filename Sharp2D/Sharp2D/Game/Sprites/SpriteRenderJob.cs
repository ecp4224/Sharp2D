using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Graphics.Shaders;

namespace Sharp2D.Game.Sprites
{
    public class ShaderGroup
    {
        public Shader key;
        public List<TextureGroup> group = new List<TextureGroup>();

        public TextureGroup GetTextureGroupOrDefault(Texture tex)
        {
            foreach (TextureGroup t in group)
            {
                if (t.key == tex || t.key.ID == tex.ID)
                    return t;
            }

            TextureGroup new_group = new TextureGroup();
            new_group.key = tex;
            group.Add(new_group);

            return new_group;
        }

        public TextureGroup GetTextureGroup(Texture tex)
        {
            foreach (TextureGroup t in group)
            {
                if (t.key == tex || t.key.ID == tex.ID)
                    return t;
            }
            return null;
        }
    }
    public class TextureGroup
    {
        public Texture key;
        public List<Sprite> group = new List<Sprite>();
    }
    
    public abstract class SpriteRenderJob : IRenderJob
    {
        private static Type DefaultJob;

        public static SpriteRenderJob CreateDefaultJob()
        {
            if (DefaultJob == null)
            {
                DefaultJob = typeof(OpenGL1SpriteRenderJob);
            }

            return (SpriteRenderJob)Activator.CreateInstance(DefaultJob);
        }

        public static void SetDefaultJob<T>()
        {
            DefaultJob = typeof(T);
        }

        public static void SetDefaultJob(Type t)
        {
            DefaultJob = t;
        }

        private bool valid = false;
        private List<Sprite> _cache = new List<Sprite>();
        public List<Sprite> Sprites
        {
            get
            {
                lock (group_lock)
                {
                    if (!valid)
                    {
                        _cache = new List<Sprite>();
                        foreach (ShaderGroup s in group)
                        {
                            foreach (TextureGroup t in s.group)
                            {
                                _cache.AddRange(t.group);
                            }
                        }
                        valid = true;
                    }
                }
                return _cache;
            }
        }


        protected readonly List<ShaderGroup> group = new List<ShaderGroup>();
        protected readonly object group_lock = new object();

        public abstract void PerformJob();

        public virtual void Dispose()
        {
            foreach (ShaderGroup s in group)
            {
                foreach (TextureGroup t in s.group) 
                {
                    foreach (Sprite sprite in t.group)
                    {
                        sprite.Unload();
                        sprite.Dispose();
                    }
                }
            }
            group.Clear();
        }

        public ShaderGroup GetShaderGroupOrDefault(Shader shader)
        {
            foreach (ShaderGroup s in group)
            {
                if (s.key == shader)
                    return s;
            }
            ShaderGroup ss = new ShaderGroup();
            ss.key = shader;
            group.Add(ss);
            return ss;
        }

        public ShaderGroup GetShaderGroup(Shader shader)
        {
            foreach (ShaderGroup s in group)
            {
                if (s.key == shader)
                    return s;
            }
            return null;
        }

        public virtual void AddSprite(Sprite sprite)
        {
            sprite.Load();
            lock (group_lock)
            {
                ShaderGroup s = GetShaderGroupOrDefault(sprite.Shader);
                TextureGroup t = s.GetTextureGroupOrDefault(sprite.Texture);
                t.group.Add(sprite);
                valid = false;
            }
        }

        public virtual void RemoveSprite(Sprite sprite)
        {
            ShaderGroup s = GetShaderGroupOrDefault(sprite.Shader);
            TextureGroup t = s.GetTextureGroupOrDefault(sprite.Texture);
            lock (group_lock)
            {
                if (t.group.Contains(sprite))
                {
                    t.group.Remove(sprite);
                    sprite.Unload();
                }
                valid = false;
            }
        }

        public virtual bool HasSprite(Sprite sprite)
        {
            ShaderGroup group = GetShaderGroup(sprite.Shader);
            if (group != null)
            {
                TextureGroup tgroup = group.GetTextureGroup(sprite.Texture);
                if (tgroup != null)
                {
                    return tgroup.group.Contains(sprite);
                }
            }
            return false;
        }
    }
}
