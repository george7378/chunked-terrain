using System;

namespace ChunkedTerrainCore.Utility
{
    public static class Globals
    {
        #region Standalone methods

        public static float Clamp(float x, float vLower, float vUpper)
        {
            return x < vLower ? vLower : x > vUpper ? vUpper : x;
        }

        public static float LinearInterpolate(float v1, float v2, float w)
        {
            float weight = Clamp(w, 0, 1);

            return v1 + (v2 - v1)*weight;
        }

        public static float CosineInterpolate(float v1, float v2, float w)
        {
            float weight = Clamp(w, 0, 1);

            return v1 + (v2 - v1)*(1 - (float)Math.Cos(weight*Math.PI))/2;
        }
        
        #endregion
    }
}
