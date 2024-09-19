using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChunkedTerrainCore.Environment
{
    public struct TerrainVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinates;
        public Vector2 TextureCoordinatesDetail;

        public TerrainVertex(Vector3 position, Vector3 normal, Vector2 textureCoordinates, Vector2 textureCoordinatesDetail)
        {
            Position = position;
            Normal = normal;

            TextureCoordinates = textureCoordinates;
            TextureCoordinatesDetail = textureCoordinatesDetail;
        }

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float)*3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float)*6, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float)*8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1)
        );
    }
}