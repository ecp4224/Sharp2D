using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Graphics;

namespace Sharp2D.Game.Sprites
{
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
                        foreach (Texture tex in groups.Keys)
                        {
                            foreach (Sprite sprite in groups[tex])
                            {
                                _cache.Add(sprite);
                            }
                        }
                        valid = true;
                    }
                }
                return _cache;
            }
        }

        protected readonly ConcurrentDictionary<Texture, List<Sprite>> groups = new ConcurrentDictionary<Texture, List<Sprite>>();
        protected readonly object group_lock = new object();

        public abstract void PerformJob();

        public virtual void Dispose()
        {
            foreach (Texture t in groups.Keys)
            {
                foreach (Sprite s in groups[t])
                {
                    s.Unload();
                    s.Dispose();
                }
            }
            groups.Clear();
        }

        public virtual void AddSprite(Sprite sprite)
        {
            sprite.Load();
            lock (group_lock)
            {
                groups.GetOrAdd(sprite.Texture, new List<Sprite>()).Add(sprite);
                valid = false;
            }
        }

        public virtual void RemoveSprite(Sprite sprite)
        {
            List<Sprite> temp;
            groups.TryGetValue(sprite.Texture, out temp);
            lock (group_lock)
            {
                if (temp.Contains(sprite))
                {
                    temp.Remove(sprite);
                    sprite.Unload();
                }
                valid = false;
            }
        }

        public virtual bool HasSprite(Sprite sprite)
        {
            lock (group_lock)
            {
                return groups.ContainsKey(sprite.Texture) && groups[sprite.Texture].Contains(sprite);
            }
        }
    }
}
