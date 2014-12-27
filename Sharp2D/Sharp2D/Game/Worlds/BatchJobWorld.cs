using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Interfaces;
using Sharp2D.Game.Sprites;

namespace Sharp2D.Game.Worlds
{
    public abstract class SpriteWorld : World
    {
        private BatchRenderJob _defaultJob;
        public virtual BatchRenderJob DefaultJob
        {
            get { return _defaultJob;  }
            set
            {
                if (!HasJob(value))
                {
                    AddRenderJob(value);
                }
                _defaultJob = value;
            }
        }

        public virtual List<BatchRenderJob> SpriteRenderJobs
        {
            get
            {
                return base.jobs.OfType<BatchRenderJob>().ToList<BatchRenderJob>();
            }
        }

        public virtual List<Sprite> Sprites
        {
            get
            {
                var sprites = new List<Sprite>();
                var spriteRenderJobs = SpriteRenderJobs;
                foreach (var job in spriteRenderJobs)
                {
                    sprites.AddRange(job.Batch.Sprites);
                }

                return sprites;
            }
        }

        public virtual void AddSprite(Sprite s, BatchRenderJob job)
        {
            if (!HasJob(job))
            {
                AddRenderJob(job);
            }

            job.AddSprite(s);
            s._worlds.Add(this);
            s.OnAddedToWorld(this);

            var logical = s as ILogical;
            if (logical != null)
                AddLogical(logical);
        }

        public virtual void AddSprite(Sprite s)
        {
            if (DefaultJob == null)
            {
                Logger.Warn("Attempted to add Sprite " + s + " to default job, but no default job found!");
            }

            if (DefaultJob != null)
            {
                DefaultJob.AddSprite(s);
                s._worlds.Add(this);
                s.OnAddedToWorld(this);
            }
            var logical = s as ILogical;
            if (logical != null)
                AddLogical(logical);
        }

        public virtual void RemoveSprite(Sprite s)
        {
            List<BatchRenderJob> jobs = SpriteRenderJobs;
            foreach (BatchRenderJob job in jobs)
            {
                if (job.HasSprite(s))
                    job.RemoveSprite(s);
            }
            s._worlds.Remove(this);
            s.OnRemovedFromWorld(this);

            var logical = s as ILogical;
            if (logical != null)
                RemoveLogical(logical);
        }
    }
}
