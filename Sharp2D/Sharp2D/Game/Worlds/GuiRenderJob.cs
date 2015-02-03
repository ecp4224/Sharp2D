using Sharp2D.Core;
using Sharp2D.Core.Graphics;

namespace Sharp2D.Game.Worlds
{
    public class GuiRenderJob : GenericRenderJob
    {
        private readonly DrawPass[] _defaultPasses =
        {
            new DrawNoAlphaNoAmbient(),
            new DrawAlphaNoLightsNoAmbient(),
        };

        public GuiRenderJob(GenericWorld parent) : base(parent)
        {
        }

        public override DrawPass[] DefaultPasses
        {
            get { return _defaultPasses; }
        }

        protected override SpriteBatch[] CreateCulledBatches()
        {
            var drawPasses = base.DrawPasses;

            var batches = new DrawBatch[drawPasses.Count];
            for (int i = 0; i < batches.Length; i++)
            {
                batches[i] = new DrawBatch();
                DrawBatch batch = batches[i];
                DrawPass pass = drawPasses[i];

                pass.SetupBatch(batch);
            }

            Batch.ForEach(delegate(Sprite sprite)
            {
                if (sprite.IsOffScreen || !sprite.IsVisible) return;
                for (int i = 0; i < batches.Length; i++)
                {
                    if (drawPasses[i].MeetsRequirements(sprite))
                        batches[i].Add(sprite);
                }
            });

            return batches;
        }
    }
}
