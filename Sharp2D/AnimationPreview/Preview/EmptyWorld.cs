using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Game.Worlds;
using Sharp2D.Game.Sprites;

namespace AnimationPreview.Preview
{
    public class EmptyWorld : GenericWorld
    {
        public override string Name
        {
            get { return "empty"; }
        }

        protected override void OnLoad()
        {
        }

        protected override void OnDisplay()
        {
            SpriteRenderJob job = SpriteRenderJob.CreateDefaultJob();
            AddRenderJob(job);
            DefaultJob = job;
        }

        protected override void OnUnload()
        {
        }

        protected override void OnDispose()
        {
        }
    }
}
