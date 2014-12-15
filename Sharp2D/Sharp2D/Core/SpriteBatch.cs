using System;
using System.Collections.Generic;
using Sharp2D.Core.Graphics.Shaders;

namespace Sharp2D.Core
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

    public class SpriteBatch : IDisposable
    {
        private List<ShaderGroup> group = new List<ShaderGroup>();
        private object group_lock = new object();
        private bool valid = false;
        private List<Sprite> _cache = new List<Sprite>();
        private int count = 0;

        public SpriteBatch() { }

        public SpriteBatch(SpriteBatch source)
        {
            foreach (Sprite sprite in source.Sprites)
            {
                Add(sprite);
            }
        }

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

        public virtual int Count
        {
            get
            {
                return count;
            }
        }

        public virtual void Dispose()
        {
            Clear();
            _cache.Clear();
        }

        public virtual void ForEach(Action<Shader, Texture, Sprite> callBack)
        {
            List<Sprite> invalids = new List<Sprite>();
            List<TextureGroup> invalid_group = new List<TextureGroup>();
            foreach (ShaderGroup shader in group)
            {
                foreach (TextureGroup texture in shader.group)
                {
                    foreach (Sprite sprite in texture.group)
                    {
                        if (sprite.Texture.ID != texture.key.ID)
                        {
                            invalids.Add(sprite);
                            invalid_group.Add(texture);
                            continue;
                        }

                        callBack(shader.key, texture.key, sprite);
                    }
                }
            } 
            
            for (int i = 0; i < invalids.Count; i++)
            {
                Sprite sprite = invalids[i];
                TextureGroup t = invalid_group[i];

                ShaderGroup s = GetShaderGroupOrDefault(sprite.Shader);
                lock (group_lock)
                {
                    if (t.group.Contains(sprite))
                    {
                        t.group.Remove(sprite);
                    }
                }

                Add(sprite);
            }

            invalids.Clear();
        }

        public virtual void ForEach(Action<Sprite> callBack)
        {
            List<Sprite> invalids = new List<Sprite>();
            List<TextureGroup> invalid_group = new List<TextureGroup>();
            foreach (ShaderGroup shader in group)
            {
                foreach (TextureGroup texture in shader.group)
                {
                    foreach (Sprite sprite in texture.group)
                    {
                        if (sprite.Texture.ID != texture.key.ID)
                        {
                            invalids.Add(sprite);
                            invalid_group.Add(texture);
                            continue;
                        }

                        callBack(sprite);
                    }
                }
            }

            for (int i = 0; i < invalids.Count; i++)
            {
                Sprite sprite = invalids[i];
                TextureGroup t = invalid_group[i];

                ShaderGroup s = GetShaderGroupOrDefault(sprite.Shader);
                lock (group_lock)
                {
                    if (t.group.Contains(sprite))
                    {
                        t.group.Remove(sprite);
                    }
                }

                Add(sprite);
            }

            invalids.Clear();
        }

        public virtual ShaderGroup GetShaderGroupOrDefault(Shader shader)
        {
            foreach (ShaderGroup s in group)
            {
                if (s.key == shader)
                    return s;
            }
            var ss = new ShaderGroup {key = shader};
            group.Add(ss);
            return ss;
        }

        public virtual ShaderGroup GetShaderGroup(Shader shader)
        {
            foreach (ShaderGroup s in group)
            {
                if (s.key == shader)
                    return s;
            }
            return null;
        }

        public virtual void Add(Sprite sprite)
        {
            lock (group_lock)
            {
                ShaderGroup s = GetShaderGroupOrDefault(sprite.Shader);
                TextureGroup t = s.GetTextureGroupOrDefault(sprite.Texture);
                t.group.Add(sprite);
                valid = false;
            }
            count++;
        }

        public virtual void AddRange(IList<Sprite> sprites)
        {
            foreach (Sprite sprite in Sprites)
            {
                Add(sprite);
            }
        }

        public virtual void Remove(Sprite sprite)
        {
            ShaderGroup s = GetShaderGroupOrDefault(sprite.Shader);
            if (s == null)
                return;
            TextureGroup t = s.GetTextureGroupOrDefault(sprite.Texture);
            if (t == null)
                return;
            lock (group_lock)
            {
                if (t.group.Contains(sprite))
                {
                    t.group.Remove(sprite);
                    sprite.Unload();
                }
                valid = false;
            }

            count--;
        }

        public virtual bool Contains(Sprite sprite)
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

        public virtual void Clear()
        {
            foreach (ShaderGroup g in group)
            {
                foreach (TextureGroup tg in g.group)
                {
                    tg.group.Clear();
                }
                g.group.Clear();
            }
            group.Clear();
        }
    }
}
