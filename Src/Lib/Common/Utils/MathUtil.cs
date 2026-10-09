using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utils
{
    public class MathUtil
    {
        public static int RoundToInt(float f)
        {
            return (int)Math.Round((double)f);
        }

        public static Random Random = new Random();

        public static double Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }
            return value;
        }

        public static float Clamp01(float value)
        {
            if (value<0f)
            {
                return 0f;
            }
            if (value>1f)
            {
                return 1f;
            }
            return value;
        }
    }
}
