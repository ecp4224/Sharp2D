using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D.Core.Utils
{
    public class MathUtils
    {
        //Code taken from: https://code.google.com/p/replicaisland/source/browse/trunk/src/com/replica/replicaisland/Lerp.java?r=5
        //Because I'm a no good dirty scrub
        public static float Ease(float start, float target, float duration, float timeSinceStart)
        {
            float value = start;
            if (timeSinceStart > 0.0f && timeSinceStart < duration)
            {
                float range = target - start;
                float percent = timeSinceStart / (duration / 2.0f);
                if (percent < 1.0f)
                {
                    value = start + ((range / 2.0f) * percent * percent * percent);
                }
                else
                {
                    float shiftedPercent = percent - 2.0f;
                    value = start + ((range / 2.0f) *
                            ((shiftedPercent * shiftedPercent * shiftedPercent) + 2.0f));
                }
            }
            else if (timeSinceStart >= duration)
            {
                value = target;
            }
            return value;
        }
    }
}
