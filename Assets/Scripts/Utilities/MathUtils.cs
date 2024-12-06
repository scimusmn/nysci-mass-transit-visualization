using UnityEngine;

namespace SMM
{
    public static class MathUtils
    {
        public static double Remap(double value, double iMin, double iMax, double oMin, double oMax)
        {
            return (value - iMin) / (oMin - iMin) * (oMax - iMax) + iMax;
        }
    }
}
