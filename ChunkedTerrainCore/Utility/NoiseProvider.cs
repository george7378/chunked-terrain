using System;

namespace ChunkedTerrainCore.Utility
{
    public class NoiseProvider
    {
        #region Properties

        public int Seed { get; set; }

        public int NumberOfOctaves { get; set; }

        public float Persistence { get; set; }

        public float Zoom { get; set; }

        #endregion

        #region Constructors

        public NoiseProvider(int seed, int numberOfOctaves, float persistence, float zoom)
        {
            Seed = seed;
            NumberOfOctaves = numberOfOctaves;
            Persistence = persistence;
            Zoom = zoom;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Returns a value between 0 and 1
        /// </summary>
        private float GridNoise(int x, int z)
        {
            int n = (1619*x + 31337*z + 1013*Seed) & 0x7fffffff;
            n = (n >> 13) ^ n;

            return ((n*(n*n*60493 + 19990303) + 1376312589) & 0x7fffffff)/(float)int.MaxValue;
        }

        private float InterpolatedGridNoise(float x, float z)
        {
            int integerX = (int)x;
            float fractionalX = x - integerX;

            int integerZ = (int)z;
            float fractionalZ = z - integerZ;

            float v1 = GridNoise(integerX, integerZ);
            float v2 = GridNoise(integerX + 1, integerZ);
            float v3 = GridNoise(integerX, integerZ + 1);
            float v4 = GridNoise(integerX + 1, integerZ + 1);

            float i1 = Globals.CosineInterpolate(v1, v2, fractionalX);
            float i2 = Globals.CosineInterpolate(v3, v4, fractionalX);

            return Globals.CosineInterpolate(i1, i2, fractionalZ);
        }

        #endregion

        #region Public methods

        public float GetValue(float x, float z)
        {
            float total = 0;
            for (int o = 0; o < NumberOfOctaves; o++)
            {
                int frequency = (int)Math.Pow(2, o);
                float amplitude = (float)Math.Pow(Persistence, o);

                total += InterpolatedGridNoise(Math.Abs(x)*frequency/Zoom, Math.Abs(z)*frequency/Zoom)*amplitude;
            }

            return total;
        }

        #endregion
    }
}