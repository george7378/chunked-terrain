using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChunkedTerrainCore.Utility;
using System;
using System.Collections.Generic;

namespace ChunkedTerrainCore.Environment
{
	public class TerrainChunk : IDisposable
	{
		#region Constants

		public const float EdgeLength = 100;

		private const int VerticesPerEdge = 8;

		private const int DecorGridSize = 10;

		private const float HalfEdgeLength = EdgeLength/2;
		private const float VertexSpacing = EdgeLength/(VerticesPerEdge - 1);

		private const float DecorTileEdgeLength = EdgeLength/DecorGridSize;
		private const float HalfDecorTileEdgeLength = DecorTileEdgeLength/2;

		#endregion

		#region Fields

		private Random _decorRandom;

		private readonly List<GridTriangle> _gridTriangles;

		#endregion

		#region Properties

		public static ushort[] Indices { get; private set; }

		public Vector2 GridCoordinates { get; set; }

		public VertexBuffer VertexBuffer { get; private set; }

		public BoundingBox BoundingBox { get; private set; }

		public List<DecorItem> DecorItems { get; private set; }

		#endregion

		#region Constructors

		public TerrainChunk(GraphicsDevice graphicsDevice, HeightProvider heightProvider, Vector2 gridCoordinates, NoiseProvider decorNoiseProvider)
		{
			_gridTriangles = new List<GridTriangle>();

			GridCoordinates = gridCoordinates;

			CalculateContents(graphicsDevice, heightProvider, decorNoiseProvider);
		}

		#endregion

		#region Private methods       

		private void CalculateContents(GraphicsDevice graphicsDevice, HeightProvider heightProvider, NoiseProvider decorNoiseProvider)
		{
			float centreXWorld = GridCoordinates.X*EdgeLength;
			float centreZWorld = GridCoordinates.Y*EdgeLength;

			Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

			List<TerrainVertex> resultVertices = new List<TerrainVertex>();
			for (int z = 0; z < VerticesPerEdge; z++)
			{
				for (int x = 0; x < VerticesPerEdge; x++)
				{
					float positionXLocal = x*VertexSpacing - HalfEdgeLength;
					float positionZLocal = z*VertexSpacing - HalfEdgeLength;

					float positionXWorld = positionXLocal + centreXWorld;
					float positionZWorld = positionZLocal + centreZWorld;

					Vector3 position = new Vector3(positionXWorld, heightProvider.GetHeight(positionXWorld, positionZWorld), positionZWorld);
					Vector3 normal = heightProvider.GetNormalFromFiniteOffset(positionXWorld, positionZWorld);

					Vector2 textureCoordinates = new Vector2(positionXLocal, positionZLocal)/HalfEdgeLength;
					Vector2 textureCoordinatesDetail = new Vector2(positionXLocal, positionZLocal)*2/HalfEdgeLength;

					TerrainVertex vertex = new TerrainVertex(position, normal, textureCoordinates, textureCoordinatesDetail);
					resultVertices.Add(vertex);

					min = Vector3.Min(min, position);
					max = Vector3.Max(max, position);
				}
			}

			TerrainVertex[] resultVerticesData = resultVertices.ToArray();

			VertexBuffer = new VertexBuffer(graphicsDevice, TerrainVertex.VertexDeclaration, resultVerticesData.Length, BufferUsage.WriteOnly);
			VertexBuffer.SetData(resultVerticesData);

			// Populate grid triangles
			_gridTriangles.Clear();

			for (int z = 0; z < VerticesPerEdge - 1; z++)
			{
				for (int x = 0; x < VerticesPerEdge - 1; x++)
				{
					int baseIndexPosition = (x + z*(VerticesPerEdge - 1))*6;
					_gridTriangles.Add(new GridTriangle(resultVerticesData[Indices[baseIndexPosition]].Position, resultVerticesData[Indices[baseIndexPosition + 1]].Position, resultVerticesData[Indices[baseIndexPosition + 2]].Position));
					_gridTriangles.Add(new GridTriangle(resultVerticesData[Indices[baseIndexPosition + 3]].Position, resultVerticesData[Indices[baseIndexPosition + 4]].Position, resultVerticesData[Indices[baseIndexPosition + 5]].Position));
				}
			}

			// Include water tile vertices in bounding box
			for (int z = -1; z <= 1; z += 2)
			{
				for (int x = -1; x <= 1; x += 2)
				{
					Vector3 waterTileVertexPosition = new Vector3(centreXWorld + x*HalfEdgeLength, 0, centreZWorld + z*HalfEdgeLength);

					min = Vector3.Min(min, waterTileVertexPosition);
					max = Vector3.Max(max, waterTileVertexPosition);
				}
			}

			BoundingBox = new BoundingBox(min, max);

			// Fill decor according to procedural inputs
			_decorRandom = new Random(GridCoordinates.GetHashCode());

			DecorItems = new List<DecorItem>();
			for (int z = 0; z < DecorGridSize; z++)
			{
				for (int x = 0; x < DecorGridSize; x++)
				{
					float decorCentreXWorld = centreXWorld - HalfEdgeLength + HalfDecorTileEdgeLength + x*DecorTileEdgeLength + (float)(2*_decorRandom.NextDouble() - 1)*HalfDecorTileEdgeLength;
					float decorCentreZWorld = centreZWorld - HalfEdgeLength + HalfDecorTileEdgeLength + z*DecorTileEdgeLength + (float)(2*_decorRandom.NextDouble() - 1)*HalfDecorTileEdgeLength;

					float? decorHeight = GetGridHeight(new Vector3(decorCentreXWorld, 0, decorCentreZWorld));
					if (decorHeight.HasValue && decorHeight.Value > 5)
					{
						if (decorNoiseProvider.GetValue(decorCentreXWorld, decorCentreZWorld) > 0.9f)
						{
							DecorItems.Add(new DecorItem(new Vector3(decorCentreXWorld, decorHeight.Value, decorCentreZWorld), (float)(_decorRandom.NextDouble()*2*Math.PI)));
						}
					}
				}
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Resulting triangles are wound CCW
		/// </summary>
		public static void CalculateIndices()
		{
			List<ushort> resultIndices = new List<ushort>();
			for (int z = 0; z < VerticesPerEdge - 1; z++)
			{
				bool slantLeft = z % 2 == 0;

				for (int x = 0; x < VerticesPerEdge - 1; x++)
				{
					ushort blIndex = (ushort)(x + z*VerticesPerEdge);
					ushort brIndex = (ushort)(blIndex + 1);
					ushort tlIndex = (ushort)(x + (z + 1)*VerticesPerEdge);
					ushort trIndex = (ushort)(tlIndex + 1);

					ushort[] triangle1 = slantLeft ? new ushort[3] { tlIndex, blIndex, brIndex } : new ushort[3] { tlIndex, blIndex, trIndex };
					ushort[] triangle2 = slantLeft ? new ushort[3] { tlIndex, brIndex, trIndex } : new ushort[3] { blIndex, brIndex, trIndex };

					resultIndices.AddRange(triangle1);
					resultIndices.AddRange(triangle2);

					slantLeft = !slantLeft;
				}
			}

			Indices = resultIndices.ToArray();
		}

		public float? GetGridHeight(Vector3 p)
		{
			foreach (GridTriangle gridTriangle in _gridTriangles)
			{
				float? barycentricHeight = gridTriangle.GetBarycentricHeight(p);
				if (barycentricHeight.HasValue)
				{
					return barycentricHeight.Value;
				}
			}

			return null;
		}

		#endregion

		#region IDisposable overrides

		protected virtual void Dispose(bool disposing)
		{
			_gridTriangles.Clear();

			VertexBuffer.Dispose();
		}

		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		#endregion
	}
}