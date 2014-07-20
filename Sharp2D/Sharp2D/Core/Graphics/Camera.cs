using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Utils;
using OpenTK;


namespace Sharp2D.Core.Graphics
{
    public abstract class Camera
    {

        private List<IMoveable2d> toFollow = new List<IMoveable2d>();

        private bool moving = false;
        private PanType type;
        private long started;
        private long duration;
        private Vector2 start;
        private Vector2 end;

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
        public virtual void BeforeRender()
        {
            if (moving)
            {
                switch (type)
                {
                    case PanType.Linear:
                        long time = Screen.TickCount - started;
                        float percent;
                        if (time > duration)
                        {
                            percent = 1f;
                        }
                        else
                        {
                            percent = (float)time / (float)duration;
                        }
                        float x = start.X + ((end.X - start.X) * percent);
                        float y = start.Y + ((end.Y - start.Y) * percent);
                        X = x;
                        Y = y;

                        if (x == end.X && y == end.Y)
                        {
                            moving = false;
                        }
                        break;

                    case PanType.Smooth:
                        float newx = MathUtils.Ease(start.X, end.X, duration, Screen.TickCount - started);
                        float newy = MathUtils.Ease(start.Y, end.Y, duration, Screen.TickCount - started);

                        X = newx;
                        Y = newy;
                        if (newx == end.X && newy == end.Y)
                        {
                            moving = false;
                        }
                        break;
                }
            }
            else if (toFollow.Count > 0)
            {
                Vector2 center = MathUtils.CenterOf(toFollow.AsVector2List());
                X = center.X;
                Y = center.Y;
            }
        }

        /// <summary>
        /// Add this moveable object to the Camera's list of moveable objects to follow
        /// </summary>
        /// <param name="moveable">The moveable object to follow</param>
        public void Follow2d(IMoveable2d moveable)
        {
            toFollow.Add(moveable);
        }

        /// <summary>
        /// Remove this moveable object to the Camera's list of moveable objects to follow
        /// </summary>
        /// <param name="moveable"></param>
        public void StopFollowing2d(IMoveable2d moveable)
        {
            toFollow.Remove(moveable);
        }

        /// <summary>
        /// Clear the Camera's list of moveable objects to follow
        /// </summary>
        public void ClearFollowing()
        {
            toFollow.Clear();
        }

        /// <summary>
        /// Pan the camera to a point in the World.
        /// </summary>
        /// <param name="Point">The location to pan to</param>
        /// <param name="Type">The type of pan to do</param>
        /// <param name="Duration">How long (in ms) the pan should last for</param>
        public void PanTo(Vector2 Point, PanType Type, long Duration)
        {
            start = new Vector2(X, Y);
            end = Point;
            duration = Duration;
            type = Type;

            moving = true;
            started = Screen.TickCount;
        }

        /// <summary>
        /// Pan the camera to a point in the World.
        /// </summary>
        /// <param name="X">The X location to pan to</param>
        /// <param name="Y">The Y location to pan to</param>
        /// <param name="Type">The type of pan to do</param>
        /// <param name="Duration">How long (in ms) the pan should last for</param>
        public void PanTo(float X, float Y, PanType Type, long Duration)
        {
            PanTo(new Vector2(X, Y), Type, Duration);
        }

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

    public enum PanType
    {
        Linear,
        Smooth
    }
}
