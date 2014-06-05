using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D.Core.Graphics
{
    public interface IRenderJobContainer
    {
        void PreFetch(); //Warn before fetching
        List<IRenderJob> RenderJobs { get; } //Fetch
        void PostFetch(); //Inform the fetch is complete
    }
}
