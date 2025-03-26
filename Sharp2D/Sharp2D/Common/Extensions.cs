using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenTK.Mathematics;
using Sharp2D.Core.Interfaces;
using SkiaSharp;

namespace Sharp2D
{
    public static class Extensions
    {
        public static bool IsDefaultForType<T>(this object obj)
        {
            var temp = (T)obj;
            return EqualityComparer<T>.Default.Equals(temp, default(T));
        }

        public static void Notify(this object obj)
        {
            Monitor.Enter(obj);
            try
            {
                Monitor.Pulse(obj);
            }
            finally
            {
                Monitor.Exit(obj);
            }
        }

        public static void NotifyAll(this object obj)
        {
            Monitor.Enter(obj);
            try
            {
                Monitor.PulseAll(obj);
            }
            finally
            {
                Monitor.Exit(obj);
            }
        }

        public static void Wait(this object obj)
        {
            Monitor.Enter(obj);
            try
            {
                Monitor.Wait(obj);
            }
            finally
            {
                Monitor.Exit(obj);
            }
        }

        public static List<Vector2> AsVector2List(this List<IMoveable2d> obj)
        {
            var toReturn = new List<Vector2>(obj.Count);
            toReturn.AddRange(obj.Select(s => s.Vector2d));

            return toReturn;
        }

        public static List<Vector3> AsVector3List(this List<IMoveable3d> obj)
        {
            var toReturn = new List<Vector3>(obj.Count);
            toReturn.AddRange(obj.Select(s => s.Vector3d));

            return toReturn;
        }

        public static bool ContainsAlpha(this SKBitmap Bitmap)
        {
            for (int y = 0; y < Bitmap.Height; y++)
            {
                for (int x = 0; x < Bitmap.Width; x++)
                {
                    SKColor color = Bitmap.GetPixel(x, y);
                    if (color.Alpha < 255)
                        return true;
                }
            }

            return false;
        }
    }
}
