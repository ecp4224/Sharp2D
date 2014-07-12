using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace Sharp2D.Core.Utils
{
    public static class Extensions
    {
        public static bool IsDefaultForType<T>(this object obj)
        {
            T temp = (T)obj;
            return EqualityComparer<T>.Default.Equals(temp, default(T));
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
