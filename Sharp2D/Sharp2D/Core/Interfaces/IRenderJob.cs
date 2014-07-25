using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D.Core.Interfaces
{
    /// <summary>
    /// <para>A render job is something that draws things on the screen.</para>
    /// <para>PerformJob is always called in an OpenGL safe thread</para>
    /// </summary>
    public interface IRenderJob : IDisposable
    {
        void PerformJob();
    }
}
