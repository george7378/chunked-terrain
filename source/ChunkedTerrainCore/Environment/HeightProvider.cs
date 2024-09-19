using Microsoft.Xna.Framework;

namespace ChunkedTerrainCore.Environment
{
    public abstract class HeightProvider
    {
        #region Constants

        private const float NormalSampleOffset = 0.5f;

        #endregion

        #region Methods

        public abstract float GetHeight(float x, float z);

        public float GetHeight(Vector3 location)
        {
            return GetHeight(location.X, location.Z);
        }

        public Vector3 GetNormalFromFiniteOffset(float x, float z)
        {
            float hL = GetHeight(x - NormalSampleOffset, z);
            float hR = GetHeight(x + NormalSampleOffset, z);
            float hD = GetHeight(x, z - NormalSampleOffset);
            float hU = GetHeight(x, z + NormalSampleOffset);

            return Vector3.Normalize(new Vector3(hL - hR, 2, hD - hU));
        }

        public Vector3 GetNormalFromFiniteOffset(Vector3 location)
        {
            return GetNormalFromFiniteOffset(location.X, location.Z);
        }

        #endregion
    }
}