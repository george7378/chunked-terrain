using Microsoft.Xna.Framework;

namespace ChunkedTerrainCore.Utility
{
    public class GridTriangle
    {
        #region Fields

        private readonly float _c1, _c2, _c3, _c4, _denominator, _p1Y, _p2Y;
        private readonly Vector3 _p3;

        #endregion

        #region Constrcuctors

        public GridTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            _c1 = p2.Z - p3.Z;
            _c2 = p3.X - p2.X;
            _c3 = p3.Z - p1.Z;
            _c4 = p1.X - p3.X;

            _denominator = _c1*_c4 - _c2*_c3;

            _p1Y = p1.Y;
            _p2Y = p2.Y;

            _p3 = p3;
        }

        #endregion

        #region Methods

        public float? GetBarycentricHeight(Vector3 p)
        {
            float? result = null;

            float v1 = p.X - _p3.X;
            float v2 = p.Z - _p3.Z;

            float u = (_c1*v1 + _c2*v2)/_denominator;
            if (u >= 0 && u <= 1)
            {
                float v = (_c3*v1 + _c4*v2)/_denominator;
                if (v >= 0 && v <= 1)
                {
                    float w = 1 - u - v;
                    if (w >= 0 && w <= 1)
                    {
                        result = u*_p1Y + v*_p2Y + w*_p3.Y;
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
