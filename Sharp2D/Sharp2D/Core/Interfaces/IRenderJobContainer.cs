using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Graphics;

namespace Sharp2D.Core.Interfaces
{
    /// <summary>
    /// <para>An RenderJobContainer is a container for multiple RenderJobs, and is used by the Screen to use multiple RenderJobs</para>
    /// <para>The Screen calls PreFetch, then retrieves the RenderJobs List, then after it's done with it, invokes PostFetch. This is useful for doing thread-safe operations</para>
    /// </summary>
    public interface IRenderJobContainer
    {
        /// <summary>
        /// This method is invoked by the Screen before it gets the list of RenderJobs
        /// </summary>
        void PreFetch(); //Warn before fetching

        /// <summary>
        /// A list of RenderJobs the screen should use
        /// </summary>
        List<IRenderJob> RenderJobs { get; } //Fetch

        /// <summary>
        /// The camera used by this RenderJobContainer
        /// </summary>
        Camera Camera { get; }

        /// <summary>
        /// This method is invoked after the screen is done with the list for the current frame.
        /// </summary>
        void PostFetch(); //Inform the fetch is complete
    }
}
