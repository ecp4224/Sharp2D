using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using Sharp2D.Core.Interfaces;

namespace Sharp2D
{
    public static class Extensions
    {
        public static bool IsDefaultForType<T>(this object obj)
        {
            T temp = (T)obj;
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
            List<Vector3> toReturn = new List<Vector3>(obj.Count);

            foreach (IMoveable3d s in obj)
            {
                toReturn.Add(s.Vector3d);
            }

            return toReturn;
        }

        public static bool ContainsAlpha(this Bitmap Bitmap)
        {
            BitmapData bmpData = Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            bool answer = false;
            unsafe
            {
                byte* ptrAlpha = ((byte*)bmpData.Scan0.ToPointer()) + 3;
                for (int i = bmpData.Width * bmpData.Height; i > 0; --i)  // prefix-- should be faster
                {
                    if (*ptrAlpha < 255 && *ptrAlpha > 0)
                        answer = true;

                    ptrAlpha += 4;
                }
            }

            Bitmap.UnlockBits(bmpData);

            return answer;
        }
    }
}
