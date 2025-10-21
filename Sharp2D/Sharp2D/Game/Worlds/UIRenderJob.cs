using Sharp2D;
using Sharp2D.Core;
using Sharp2D.Core.Graphics;

namespace Sharp2D.Game.Worlds
{
    public class GuiRenderJob : GenericRenderJob
    {
        private readonly DrawPass[] _defaultPasses =
        {
            new DrawUiSprite()
        };

        public GuiRenderJob(GenericWorld parent) : base(parent)
        {
        }

        public override DrawPass[] DefaultPasses
        {
            get
            {
                return _defaultPasses;
            }
        }

        protected override SpriteBatch[] CreateCulledBatches()
        {
            var drawPasses = DrawPasses;
            var batches = new DrawBatch[drawPasses.Count];
            for (int i = 0; i < batches.Length; i++)
            {
                batches[i] = new DrawBatch();
                drawPasses[i].SetupBatch(batches[i]);
            }

            Batch.ForEach(delegate(Sprite sprite)
            {
                if (!sprite.IsVisible)
                {
                    return;
                }

                for (int i = 0; i < batches.Length; i++)
                {
                    if (drawPasses[i].MeetsRequirements(sprite))
                    {
                        batches[i].Add(sprite);
                    }
                }
            });

            return batches;
        }
    }
}
