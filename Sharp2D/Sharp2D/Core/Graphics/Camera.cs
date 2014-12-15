using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Sharp2D;
using Sharp2D.Core.Interfaces;


namespace Sharp2D.Core.Graphics
{
    public abstract class Camera
    {

        private readonly List<IMoveable2d> _toFollow = new List<IMoveable2d>();

        private bool _moving;
        private PanType _type;
        private long _started;
        private long _duration;
        private Vector2 _start;
        private Vector2 _end;

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
            if (_moving)
            {
                switch (_type)
                {
                    case PanType.Linear:
                        long time = Screen.TickCount - _started;
                        float percent;
                        if (time > _duration)
                        {
                            percent = 1f;
                        }
                        else
                        {
                            percent = (float)time / (float)_duration;
                        }
                        float x = _start.X + ((_end.X - _start.X) * percent);
                        float y = _start.Y + ((_end.Y - _start.Y) * percent);
                        X = x;
                        Y = y;

                        if (x == _end.X && y == _end.Y)
                        {
                            _moving = false;
                        }
                        break;

                    case PanType.Smooth:
                        float newx = MathUtils.Ease(_start.X, _end.X, _duration, Screen.TickCount - _started);
                        float newy = MathUtils.Ease(_start.Y, _end.Y, _duration, Screen.TickCount - _started);

                        X = newx;
                        Y = newy;
                        if (newx == _end.X && newy == _end.Y)
                        {
                            _moving = false;
                        }
                        break;
                }
            }
            else if (_toFollow.Count > 0)
            {
                Vector2 center = MathUtils.CenterOf(_toFollow.AsVector2List());
                X = -center.X;
                Y = center.Y;
            }
        }

        /// <summary>
        /// Add this moveable object to the Camera's list of moveable objects to follow
        /// </summary>
        /// <param name="moveable">The moveable object to follow</param>
        public void Follow2D(IMoveable2d moveable)
        {
            _toFollow.Add(moveable);
        }

        /// <summary>
        /// Remove this moveable object to the Camera's list of moveable objects to follow
        /// </summary>
        /// <param name="moveable"></param>
        public void StopFollowing2D(IMoveable2d moveable)
        {
            _toFollow.Remove(moveable);
        }

        /// <summary>
        /// Clear the Camera's list of moveable objects to follow
        /// </summary>
        public void ClearFollowing()
        {
            _toFollow.Clear();
        }

        /// <summary>
        /// Pan the camera to a point in the World.
        /// </summary>
        /// <param name="point">The location to pan to</param>
        /// <param name="type">The type of pan to do</param>
        /// <param name="duration">How long (in ms) the pan should last for</param>
        public void PanTo(Vector2 point, PanType type, long duration)
        {
            _start = new Vector2(X, Y);
            _end = point;
            this._duration = duration;
            this._type = type;

            _moving = true;
            _started = Screen.TickCount;
        }

        /// <summary>
        /// Pan the camera to a point in the World.
        /// </summary>
        /// <param name="x">The X location to pan to</param>
        /// <param name="y">The Y location to pan to</param>
        /// <param name="type">The type of pan to do</param>
        /// <param name="duration">How long (in ms) the pan should last for</param>
        public void PanTo(float x, float y, PanType type, long duration)
        {
            PanTo(new Vector2(x, y), type, duration);
        }

        /// <summary>
        /// Test if this 2d moveable object is outside the bounds of the camera.
        /// If this moveable object has a scale (ex: sprite), it will be ignored
        /// </summary>
        /// <param name="moveable">The moveable object to test</param>
        /// <returns></returns>
        public bool IsOutsideCamera(IMoveable2d moveable)
        {
            return IsOutsideCamera(moveable.X, moveable.Y, moveable.Width, moveable.Height);
        }

        /// <summary>
        /// Test if a quad is outside the bounds of the camera
        /// </summary>
        /// <param name="x">The X position of the quad</param>
        /// <param name="y">The Y position of the quad</param>
        /// <param name="width">The width of the quad</param>
        /// <param name="height">The height of the quad</param>
        /// <param name="scale">The scale of this quad (if applicable). The default value of this parameter is 1</param>
        /// <returns>True if the quad is outside the camera's bounds, otherwise false.</returns>
        public abstract bool IsOutsideCamera(float x, float y, float width, float height, float scale = 1f);
    }

    public enum PanType
    {
        Linear,
        Smooth
    }
}
