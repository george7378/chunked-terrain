using Microsoft.Xna.Framework;

namespace ChunkedTerrainCore.Utility
{
    public class DecorItem
    {
        #region Properties

        public Vector3 Position { get; set; }

        public float Rotation { get; set; }

        #endregion

        #region Constructors

        public DecorItem(Vector3 position, float rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        #endregion
    }
}
