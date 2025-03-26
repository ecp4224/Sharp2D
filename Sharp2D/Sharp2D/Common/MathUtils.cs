using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Mathematics;

namespace Sharp2D
{
    public static class MathUtils
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

        public static Vector2 CenterOf(List<Vector2> points)
        {
            if (points.Count == 0)
                throw new ArgumentException("There must be at least 1 point to check against!");
            if (points.Count == 1)
                return points[0];
            
            float x1, y1, x2, y2;
            
            if (points.Count == 2)
            {
                x1 = points[0].X;
                y1 = points[0].Y;
                x2 = points[1].X;
                y2 = points[1].Y;

                return new Vector2((x1 + x2) / 2f, (y1 + y2) / 2f);
            }

            var center = new Vector2(0, 0);
            
            float area = 0f;
            float a;
            
            Vector2 point1, point2;

            for (int i = 0; i < points.Count - 1; i++)
            {
                point1 = points[i];
                point2 = points[i + 1];
                x1 = point1.X;
                y1 = point1.Y;
                x2 = point2.X;
                y2 = point2.Y;

                a = (x1 * y2) - (x2 * y1);
                area += a;
                center.X += (x1 + x2) * a;
                center.Y += (y1 + y2) * a;
            }
            point1 = points[points.Count - 1];
            point2 = points[0];

            x1 = point1.X;
            y1 = point1.Y;
            x2 = point2.X;
            y2 = point2.Y;

            a = (x1 * y2) - (x2 * y1);
            area += a;
            center.X += (x1 + x2) * a;
            center.Y += (y1 + y2) * a;

            area /= 2f;
            center.X /= (6f * area);
            center.Y /= (6f * area);

            return center;
        }
    }
}
