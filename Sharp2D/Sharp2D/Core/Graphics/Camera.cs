using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D.Core.Graphics
{
    public abstract class Camera
    {
        /// <summary>
        /// The X position of this Camera
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// The Y position of this Camera
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// The Z position of this Camera
        /// </summary>
        public float Z { get; set; }
         
        /// <summary>
        /// Setup the camera before all the RenderJobs perform their jobs.
        /// </summary>
        public abstract void BeforeRender();

        /// <summary>
        /// Test if a quad is outside the bounds of the camera
        /// </summary>
        /// <param name="X">The X position of the quad</param>
        /// <param name="Y">The Y position of the quad</param>
        /// <param name="Width">The width of the quad</param>
        /// <param name="Height">The height of the quad</param>
        /// <returns>True if the quad is outside the camera's bounds, otherwise false.</returns>
        public abstract bool IsOutsideCamera(float X, float Y, float Width, float Height);
    }
}
