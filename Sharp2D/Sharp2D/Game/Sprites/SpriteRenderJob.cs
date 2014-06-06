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
        protected readonly ConcurrentDictionary<Texture, List<Sprite>> groups = new ConcurrentDictionary<Texture, List<Sprite>>();
        protected readonly object group_lock = new object();

        public abstract void PerformJob();

        public virtual void Dispose()
        {
            foreach (Texture t in groups.Keys)
            {
                foreach (Sprite s in groups[t])
                {
                    s.OnUnload();
                    s.Dispose();
                }
            }
            groups.Clear();
        }

        public virtual void AddSprite(Sprite sprite)
        {
            lock (group_lock)
            {
                groups.GetOrAdd(sprite.Texture, new List<Sprite>()).Add(sprite);
            }
            sprite.OnLoad();
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
                    sprite.OnUnload();
                }
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
