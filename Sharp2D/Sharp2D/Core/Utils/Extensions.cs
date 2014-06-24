using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D.Core.Utils
{
    public static class Extensions
    {
        public static bool IsDefaultForType<T>(this object obj)
        {
            T temp = (T)obj;
            return EqualityComparer<T>.Default.Equals(temp, default(T));
        }
    }
}
