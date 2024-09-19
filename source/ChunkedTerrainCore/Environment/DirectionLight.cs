using Microsoft.Xna.Framework;

namespace ChunkedTerrainCore.Environment
{
    public class DirectionLight
    {
        #region Properties

        public Vector3 Direction { get; set; }

        public float Power { get; set; }

        public float AmbientPower { get; set; }

        public float SpecularExponent { get; set; }

        #endregion

        #region Constructors

        public DirectionLight(Vector3 direction, float power, float ambientPower, float specularExponent)
        {
            Direction = direction;
            Power = power;
            AmbientPower = ambientPower;
            SpecularExponent = specularExponent;
        }

        #endregion
    }
}