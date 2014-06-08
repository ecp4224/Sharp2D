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
        public SpriteRenderJob DefaultJob
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

        public void AddSprite(Sprite s, SpriteRenderJob job)
        {
            if (!HasJob(job))
            {
                AddRenderJob(job);
            }

            job.AddSprite(s);

            if (s is ILogical)
                AddLogical((ILogical)s);
        }

        public void AddSprite(Sprite s)
        {
            if (DefaultJob != null)
                DefaultJob.AddSprite(s);
            if (s is ILogical)
                AddLogical((ILogical)s);
        }

        public void RemoveSprite(Sprite s)
        {
            foreach (SpriteRenderJob job in GetSpriteJobs())
            {
                if (job.HasSprite(s))
                    job.RemoveSprite(s);
            }
        }

        public List<SpriteRenderJob> GetSpriteJobs()
        {
            return base.jobs.OfType<SpriteRenderJob>().ToList<SpriteRenderJob>();
        }
    }
}
