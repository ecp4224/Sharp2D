using System;
using Sharp2D.Core.Interfaces;
using Sharp2D.Game.Sprites;

namespace Sharp2D.Core.Graphics
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
