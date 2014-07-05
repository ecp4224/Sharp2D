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
    public abstract class SpriteRenderJob : IRenderJob
    {
        private static Type DefaultJob;
        private static SpriteRenderJob DefaultJobObj;

        public static SpriteRenderJob CreateDefaultJob()
        {
            if (DefaultJobObj != null)
            {
                return DefaultJobObj;
            }

            if (DefaultJob == null)
            {
                throw new ArgumentException("No default job type or object set!");
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

        public static void SetDefaultJob(SpriteRenderJob job)
        {
            DefaultJobObj = job;
        }

        private SpriteBatch batch = new SpriteBatch();
        public SpriteBatch Batch
        {
            get
            {
                return batch;
            }
        }

        public abstract void PerformJob();

        public virtual void Dispose()
        {
            batch.Dispose();
        }

        public virtual void AddSprite(Sprite sprite)
        {
            sprite.Load();
            batch.Add(sprite);
        }

        public virtual void RemoveSprite(Sprite sprite)
        {
            batch.Remove(sprite);
        }

        public virtual bool HasSprite(Sprite sprite)
        {
            return batch.Contains(sprite);
        }
    }
}
