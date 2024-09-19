using ChunkedTerrainCore.Utility;
using System;

namespace ChunkedTerrainCore.Environment
{
    public class TerrainHeightProvider : HeightProvider
    {
        #region Constants

        private const float HeightScale = 200;

        #endregion

        #region Properties

        public NoiseProvider MainNoiseProvider { get; set; }

        public NoiseProvider ModulationNoiseProvider { get; set; }

        #endregion

        #region Constructors

        public TerrainHeightProvider(NoiseProvider mainNoiseProvider, NoiseProvider modulationNoiseProvider)
        {
            MainNoiseProvider = mainNoiseProvider;
            ModulationNoiseProvider = modulationNoiseProvider;
        }

        #endregion

        #region HeightProvider implementation

        public override float GetHeight(float x, float z)
        {
            float mainHeight = MainNoiseProvider.GetValue(x, z);

            float modulationHeight = ModulationNoiseProvider.GetValue(x, z);
            modulationHeight = (float)Math.Pow(modulationHeight, 2);

            return (mainHeight*modulationHeight - 0.15f)*HeightScale;
        }

        #endregion
    }
}