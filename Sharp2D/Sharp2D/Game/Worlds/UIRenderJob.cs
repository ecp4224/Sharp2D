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
            get { throw new System.NotImplementedException(); }
        }

        protected override SpriteBatch[] CreateCulledBatches()
        {
            throw new System.NotImplementedException();
        }
    }
}
