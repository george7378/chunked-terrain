using ChunkedTerrainCore.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ChunkedTerrainCore.Environment
{
    public class World : IDisposable
    {
        #region Constants

        public const float ChunkRenderDistance = 1000;

        #endregion

        #region Fields

        private readonly GraphicsDevice _graphicsDevice;

        private readonly HeightProvider _heightProvider;

        private readonly NoiseProvider _decorNoiseProvider;

        private readonly List<TerrainChunk> _newTerrainChunks;

        private Thread _chunkCreationThread;

        #endregion

        #region Properties

        public List<TerrainChunk> TerrainChunks { get; private set; }

        public DirectionLight Light { get; set; }

        #endregion

        #region Constructors

        public World(GraphicsDevice graphicsDevice, HeightProvider heightProvider, DirectionLight light, NoiseProvider decorNoiseProvider)
        {
            _graphicsDevice = graphicsDevice;

            _heightProvider = heightProvider;

            _decorNoiseProvider = decorNoiseProvider;

            _newTerrainChunks = new List<TerrainChunk>();

            TerrainChunks = new List<TerrainChunk>();
            Light = light;
        }

        #endregion

        #region Private methods

        private void CreateNewTerrainChunks(object parameters)
        {
            _newTerrainChunks.Clear();

            List<Vector2> newTerrainChunkGridCoordinates = parameters as List<Vector2>;

            foreach (Vector2 gridCoordinates in newTerrainChunkGridCoordinates)
            {
                _newTerrainChunks.Add(new TerrainChunk(_graphicsDevice, _heightProvider, gridCoordinates, _decorNoiseProvider));
            }
        }
        
        #endregion

        #region Methods

        public float GetGridHeight(Vector3 p)
        {
            foreach (TerrainChunk terrainChunk in TerrainChunks)
            {
                if (p.X >= terrainChunk.BoundingBox.Min.X && p.X <= terrainChunk.BoundingBox.Max.X && p.Z >= terrainChunk.BoundingBox.Min.Z && p.Z <= terrainChunk.BoundingBox.Max.Z)
                {
                    float? gridHeight = terrainChunk.GetGridHeight(p);

                    return gridHeight.HasValue ? gridHeight.Value : 0;
                }
            }

            return 0;
        }

        public void Update(Vector3 cameraPosition)
        {
            // Dispose of chunks which are out of range
            for (int i = TerrainChunks.Count - 1; i >= 0; i--)
            {
                TerrainChunk terrainChunk = TerrainChunks.ElementAt(i);

                if (new Vector2(cameraPosition.X - terrainChunk.GridCoordinates.X*TerrainChunk.EdgeLength, cameraPosition.Z - terrainChunk.GridCoordinates.Y*TerrainChunk.EdgeLength).Length() > ChunkRenderDistance)
                {
                    TerrainChunks.RemoveAt(i);
                    terrainChunk.Dispose();
                }
            }

            // Absorb and create new chunks if background thread isn't busy
            if (_chunkCreationThread == null || !_chunkCreationThread.IsAlive)
            {
                TerrainChunks.AddRange(_newTerrainChunks);

                List<Vector2> newTerrainChunkGridCoordinates = new List<Vector2>();
                for (int gridCoordinateZ = (int)Math.Ceiling((cameraPosition.Z - ChunkRenderDistance)/TerrainChunk.EdgeLength); gridCoordinateZ <= (int)Math.Floor((cameraPosition.Z + ChunkRenderDistance)/TerrainChunk.EdgeLength); gridCoordinateZ++)
                {
                    for (int gridCoordinateX = (int)Math.Ceiling((cameraPosition.X - ChunkRenderDistance)/TerrainChunk.EdgeLength); gridCoordinateX <= (int)Math.Floor((cameraPosition.X + ChunkRenderDistance)/TerrainChunk.EdgeLength); gridCoordinateX++)
                    {
                        Vector2 gridCoordinates = new Vector2(gridCoordinateX, gridCoordinateZ);

                        if (new Vector2(cameraPosition.X - gridCoordinates.X*TerrainChunk.EdgeLength, cameraPosition.Z - gridCoordinates.Y*TerrainChunk.EdgeLength).Length() <= ChunkRenderDistance && !TerrainChunks.Any(c => c.GridCoordinates == gridCoordinates))
                        {
                            newTerrainChunkGridCoordinates.Add(gridCoordinates);
                        }
                    }
                }

                _chunkCreationThread = new Thread(CreateNewTerrainChunks);
                _chunkCreationThread.Start(newTerrainChunkGridCoordinates);
            }
        }

        #endregion

        #region IDisposable overrides

        protected virtual void Dispose(bool disposing)
        {
            foreach (TerrainChunk terrainChunk in TerrainChunks)
            {
                terrainChunk.Dispose();
            }

            if (_chunkCreationThread != null)
            {
                _chunkCreationThread.Join();
            }

            foreach (TerrainChunk newTerrainChunk in _newTerrainChunks)
            {
                newTerrainChunk.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}