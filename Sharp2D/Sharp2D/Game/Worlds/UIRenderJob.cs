using Sharp2D.Core;
using Sharp2D.Core.Graphics;

namespace Sharp2D.Game.Worlds
{
    public class GuiRenderJob : GenericRenderJob
    {
        public GuiRenderJob(GenericWorld parent) : base(parent)
        {
        }

        public override DrawPass[] DefaultPasses
        {
            get { return new DrawPass[0]; }
        }

        protected override SpriteBatch[] CreateCulledBatches()
        {
            return new SpriteBatch[0];
        }
    }
}
