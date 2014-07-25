using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Interfaces;

namespace Sharp2D.Game.Drawables
{
    public class DrawableRenderJob : IRenderJob
    {
        private List<IDrawable> drawables = new List<IDrawable>();
        private readonly object draw_lock = new object();
        public List<IDrawable> Drawables
        {
            get
            {
                lock (draw_lock)
                {
                    return drawables;
                }
            }
        }
 
        public void PerformJob()
        {
            lock (draw_lock)
            {
                foreach (IDrawable drawable in drawables)
                {
                    drawable.Draw();
                }
            }
        }

        public void Dispose()
        {
            drawables.Clear();
            drawables = null;
        }
    }
}
