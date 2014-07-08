using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Logic;
using Sharp2D.Core.Graphics;
using Sharp2D.Game.Sprites;

namespace Sharp2D.Game.Worlds
{
    public abstract class SpriteWorld : World
    {
        private SpriteRenderJob _defaultJob;
        public virtual SpriteRenderJob DefaultJob
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

        public virtual List<SpriteRenderJob> SpriteRenderJobs
        {
            get
            {
                return base.jobs.OfType<SpriteRenderJob>().ToList<SpriteRenderJob>();
            }
        }

        public virtual List<Sprite> Sprites
        {
            get
            {
                List<Sprite> sprites = new List<Sprite>();
                List<SpriteRenderJob> jobs = SpriteRenderJobs;
                foreach (SpriteRenderJob job in jobs)
                {
                    sprites.AddRange(job.Batch.Sprites);
                }

                return sprites;
            }
        }

        public virtual void AddSprite(Sprite s, SpriteRenderJob job)
        {
            if (!HasJob(job))
            {
                AddRenderJob(job);
            }

            job.AddSprite(s);
            s._worlds.Add(this);
            s.OnAddedToWorld(this);

            if (s is ILogical)
                AddLogical((ILogical)s);
        }

        public virtual void AddSprite(Sprite s)
        {
            if (DefaultJob != null)
            {
                DefaultJob.AddSprite(s);
                s._worlds.Add(this);
                s.OnAddedToWorld(this);
            }
            if (s is ILogical)
                AddLogical((ILogical)s);
        }

        public virtual void RemoveSprite(Sprite s)
        {
            List<SpriteRenderJob> jobs = SpriteRenderJobs;
            foreach (SpriteRenderJob job in jobs)
            {
                if (job.HasSprite(s))
                    job.RemoveSprite(s);
            }
            s._worlds.Remove(this);
            s.OnRemovedFromWorld(this);
        }
    }
}
