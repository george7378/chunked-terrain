using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ChunkedTerrainCore.Environment;
using ChunkedTerrainCore.Utility;
using ChunkedTerrainCore.Utility.Enums;
using System;
using System.Linq;

namespace ChunkedTerrainCore
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class ChunkedTerrainGame : Game
	{
		#region Constants

		private const int WaterMapSize = 512;

		#endregion

		#region Fields

		private readonly GraphicsDeviceManager _graphics;

		private RasterizerState _defaultRasterizerState, _treeRasterizerState;

		private MouseState _oldMouseState;
		private KeyboardState _oldKeyboardState;

		private World _world;
		private Camera _camera;

		private Effect _terrainEffect;
		private Effect _waterEffect;
		private Effect _treeEffect;

		private Texture2D _groundTexture, _groundSlopeTexture, _groundDetailTexture;
		private Texture2D _waterNormalTexture;
		private Texture2D _treeTexture;
		private Texture2D _sunTexture;

		private Model _waterTileModel;
		private Model _treeModel;

		private SpriteBatch _spriteBatch;

		private Matrix _waterTileScaleMatrix;

		private Vector2 _waterTextureCoordinateOffset1, _waterTextureCoordinateOffset2;

		private bool _mouseLookActive;

		private RenderTarget2D _waterRefractionMapRenderTarget, _waterReflectionMapRenderTarget;

		private IndexBuffer _terrainChunkIndexBuffer;

		#endregion

		#region Constructors

		public ChunkedTerrainGame()
		{
			_graphics = new GraphicsDeviceManager(this) { PreferMultiSampling = true };

			Content.RootDirectory = "Content";
		}

		#endregion

		#region Private methods

		#region Content loading

		private void LoadWaterTile()
		{
			_waterTileModel = Content.Load<Model>("Models/WaterTile");

			foreach (ModelMesh mesh in _waterTileModel.Meshes)
			{
				foreach (ModelMeshPart part in mesh.MeshParts)
				{
					part.Effect = _waterEffect;
				}
			}
		}

		private void LoadTree()
		{
			_treeModel = Content.Load<Model>("Models/Tree");

			foreach (ModelMesh mesh in _treeModel.Meshes)
			{
				foreach (ModelMeshPart part in mesh.MeshParts)
				{
					part.Effect = _treeEffect;
				}
			}
		}

		#endregion

		#region Content drawing

		private void DrawTerrain(bool enableFog, WaterClipMode waterClipMode, float waterClipDepthOffset, bool waterReflect)
		{
			_terrainEffect.CurrentTechnique = _terrainEffect.Techniques["TerrainTechnique"];

			_terrainEffect.Parameters["ViewProjection"].SetValue((waterReflect ? _camera.ViewMatrixWaterReflect : _camera.ViewMatrix)*_camera.ProjectionMatrix);
			_terrainEffect.Parameters["EnableFog"].SetValue(enableFog);
			_terrainEffect.Parameters["WaterClipMode"].SetValue((int)waterClipMode);
			_terrainEffect.Parameters["LightPower"].SetValue(_world.Light.Power);
			_terrainEffect.Parameters["AmbientLightPower"].SetValue(_world.Light.AmbientPower);
			_terrainEffect.Parameters["WaterClipDepthOffset"].SetValue(waterClipDepthOffset);
			_terrainEffect.Parameters["FogStart"].SetValue(500.0f);
			_terrainEffect.Parameters["FogEnd"].SetValue(World.ChunkRenderDistance);
			_terrainEffect.Parameters["CameraPosition"].SetValue(_camera.Position);
			_terrainEffect.Parameters["LightDirection"].SetValue(_world.Light.Direction);
			_terrainEffect.Parameters["FogColour"].SetValue(Color.CornflowerBlue.ToVector3());
			_terrainEffect.Parameters["GroundTexture"].SetValue(_groundTexture);
			_terrainEffect.Parameters["GroundSlopeTexture"].SetValue(_groundSlopeTexture);
			_terrainEffect.Parameters["GroundDetailTexture"].SetValue(_groundDetailTexture);

			GraphicsDevice.Indices = _terrainChunkIndexBuffer;

			foreach (EffectPass pass in _terrainEffect.CurrentTechnique.Passes)
			{
				pass.Apply();

				foreach (TerrainChunk chunk in _world.TerrainChunks.Where(c => (waterReflect ? _camera.FrustumWaterReflect : _camera.Frustum).Intersects(c.BoundingBox)))
				{
					GraphicsDevice.SetVertexBuffer(chunk.VertexBuffer);

					GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _terrainChunkIndexBuffer.IndexCount/3);

					GraphicsDevice.SetVertexBuffer(null);
				}
			}

			GraphicsDevice.Indices = null;
		}

		private void DrawWater()
		{
			GraphicsDevice.BlendState = BlendState.NonPremultiplied;

			foreach (TerrainChunk chunk in _world.TerrainChunks.Where(c => _camera.Frustum.Intersects(c.BoundingBox)))
			{
				Matrix waterTileWorldMatrix = _waterTileScaleMatrix*Matrix.CreateTranslation(new Vector3(chunk.GridCoordinates.X*TerrainChunk.EdgeLength, 0, chunk.GridCoordinates.Y*TerrainChunk.EdgeLength));

				foreach (ModelMesh mesh in _waterTileModel.Meshes)
				{
					foreach (Effect effect in mesh.Effects)
					{
						effect.CurrentTechnique = effect.Techniques["WaterTechnique"];

						effect.Parameters["World"].SetValue(waterTileWorldMatrix);
						effect.Parameters["WorldViewProjection"].SetValue(waterTileWorldMatrix*_camera.ViewMatrix*_camera.ProjectionMatrix);
						effect.Parameters["SpecularExponent"].SetValue(_world.Light.SpecularExponent);
						effect.Parameters["WaveScale"].SetValue(0.1f);
						effect.Parameters["FogStart"].SetValue(500.0f);
						effect.Parameters["FogEnd"].SetValue(World.ChunkRenderDistance);
						effect.Parameters["TextureCoordinateOffset1"].SetValue(_waterTextureCoordinateOffset1);
						effect.Parameters["TextureCoordinateOffset2"].SetValue(_waterTextureCoordinateOffset2);
						effect.Parameters["CameraPosition"].SetValue(_camera.Position);
						effect.Parameters["LightDirection"].SetValue(_world.Light.Direction);
						effect.Parameters["FogColour"].SetValue(Color.CornflowerBlue.ToVector3());
						effect.Parameters["WaterIntrinsicColour"].SetValue(new Vector3(0, 0.1f, 0.2f));
						effect.Parameters["NormalMapTexture"].SetValue(_waterNormalTexture);
						effect.Parameters["RefractionMapTexture"].SetValue(_waterRefractionMapRenderTarget);
						effect.Parameters["ReflectionMapTexture"].SetValue(_waterReflectionMapRenderTarget);
					}

					mesh.Draw();
				}
			}

			GraphicsDevice.BlendState = BlendState.Opaque;
		}

		private void DrawDecor()
		{
			GraphicsDevice.RasterizerState = _treeRasterizerState;

			foreach (TerrainChunk chunk in _world.TerrainChunks.Where(c => _camera.Frustum.Intersects(c.BoundingBox)))
			{
				foreach (DecorItem decorItem in chunk.DecorItems)
				{
					Matrix treeWorldMatrix = Matrix.CreateRotationY(decorItem.Rotation)*Matrix.CreateTranslation(decorItem.Position);

					foreach (ModelMesh mesh in _treeModel.Meshes)
					{
						foreach (Effect effect in mesh.Effects)
						{
							effect.CurrentTechnique = effect.Techniques["TreeTechnique"];

							effect.Parameters["World"].SetValue(treeWorldMatrix);
							effect.Parameters["WorldViewProjection"].SetValue(treeWorldMatrix*_camera.ViewMatrix*_camera.ProjectionMatrix);
							effect.Parameters["LightPower"].SetValue(_world.Light.Power);
							effect.Parameters["FogStart"].SetValue(500.0f);
							effect.Parameters["FogEnd"].SetValue(World.ChunkRenderDistance);
							effect.Parameters["CameraPosition"].SetValue(_camera.Position);
							effect.Parameters["FogColour"].SetValue(Color.CornflowerBlue.ToVector3());
							effect.Parameters["TreeTexture"].SetValue(_treeTexture);
						}

						mesh.Draw();
					}
				}
			}

			GraphicsDevice.RasterizerState = _defaultRasterizerState;
		}
		
		private void DrawSky()
		{
			_spriteBatch.Begin();

			// Sun
			if (Vector3.Dot(_camera.LookDirection, -_world.Light.Direction) > 0)
			{
				Vector3 sunScreenPosition = GraphicsDevice.Viewport.Project(_camera.Position - _world.Light.Direction, _camera.ProjectionMatrix, _camera.ViewMatrix, Matrix.Identity);
				_spriteBatch.Draw(_sunTexture, new Vector2(sunScreenPosition.X, sunScreenPosition.Y), null, Color.White, 0, new Vector2(128, 128), 1, SpriteEffects.None, 0);
			}

			_spriteBatch.End();

			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
		}

		#endregion

		#region Misc.

		private void ProcessInput()
		{
			MouseState newMouseState = Mouse.GetState();
			KeyboardState newKeyboardState = Keyboard.GetState();

			// Various states
			if (_oldKeyboardState.IsKeyDown(Keys.Space) && newKeyboardState.IsKeyUp(Keys.Space))
			{
				switch (_camera.Mode)
				{
					case CameraMode.Walk:
						_camera.Mode = CameraMode.Fly;
						break;

					case CameraMode.Fly:
						_camera.Mode = CameraMode.Walk;
						break;
				}
			}

			if (_oldKeyboardState.IsKeyDown(Keys.C) && newKeyboardState.IsKeyUp(Keys.C))
			{
				_mouseLookActive = !_mouseLookActive;
			}

			// Camera movement
			Vector3 newCameraPosition = _camera.Position;

			Vector2 strafeVector = new Vector2(newKeyboardState.IsKeyDown(Keys.A) ? -1 : newKeyboardState.IsKeyDown(Keys.D) ? 1 : 0, newKeyboardState.IsKeyDown(Keys.S) ? -1 : newKeyboardState.IsKeyDown(Keys.W) ? 1 : 0);
			if (strafeVector.Length() > 0)
			{
				Vector3 forwardVector = Vector3.Zero;
				Vector3 rightVector = Vector3.Zero;

				switch (_camera.Mode)
				{
					case CameraMode.Walk:
						forwardVector = Vector3.Normalize(new Vector3(_camera.LookDirection.X, 0, _camera.LookDirection.Z));
						rightVector = Vector3.Transform(forwardVector, Matrix.CreateRotationY((float)(-Math.PI/2)));
						break;

					case CameraMode.Fly:
						forwardVector = _camera.LookDirection;
						rightVector = Vector3.Normalize(Vector3.Cross(forwardVector, Vector3.UnitY));
						break;
				}

				newCameraPosition += 0.5f*(forwardVector*strafeVector.Y + rightVector*strafeVector.X);
			}

			if (_camera.Mode == CameraMode.Walk)
			{
				float gridHeight = _world.GetGridHeight(newCameraPosition);
				newCameraPosition.Y = (gridHeight > 0 ? gridHeight : 0) + 5;
			}

			_camera.Position = newCameraPosition;

			if (_mouseLookActive)
			{
				_camera.AltitudeAngle -= 0.01f*(newMouseState.Y - _oldMouseState.Y);
				_camera.AzimuthAngle += 0.01f* (newMouseState.X - _oldMouseState.X);

				Mouse.SetPosition((int)(GraphicsDevice.Viewport.Width/2.0f), (int)(GraphicsDevice.Viewport.Height/2.0f));
				newMouseState = Mouse.GetState();
			}

			_oldMouseState = newMouseState;
			_oldKeyboardState = newKeyboardState;
		}

		#endregion

		#endregion

		#region Game overrides

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			TerrainChunk.CalculateIndices();

			_defaultRasterizerState = GraphicsDevice.RasterizerState;
			_treeRasterizerState = new RasterizerState()
			{
				CullMode = CullMode.None
			};

			_oldMouseState = Mouse.GetState();
			_oldKeyboardState = Keyboard.GetState();

			int worldSeed = 0;
			NoiseProvider mainNoiseProvider = new NoiseProvider(worldSeed, 4, 0.2f, 200);
			NoiseProvider modulationNoiseProvider = new NoiseProvider(worldSeed + 1, 2, 0.2f, 600);
			DirectionLight light = new DirectionLight(Vector3.Normalize(new Vector3(0, -0.3f, -1)), 1, 0.1f, 64);
			NoiseProvider decorNoiseProvider = new NoiseProvider(worldSeed + 2, 2, 0.2f, 200);
			_world = new World(GraphicsDevice, new TerrainHeightProvider(mainNoiseProvider, modulationNoiseProvider), light, decorNoiseProvider);

			Matrix cameraProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), GraphicsDevice.Viewport.AspectRatio, 1, World.ChunkRenderDistance);
			_camera = new Camera()
			{
				ProjectionMatrix = cameraProjectionMatrix
			};

			_spriteBatch = new SpriteBatch(GraphicsDevice);

			_waterTileScaleMatrix = Matrix.CreateScale(TerrainChunk.EdgeLength);

			_mouseLookActive = true;

			_waterRefractionMapRenderTarget = new RenderTarget2D(GraphicsDevice, WaterMapSize, WaterMapSize, false, SurfaceFormat.Color, DepthFormat.Depth24);
			_waterReflectionMapRenderTarget = new RenderTarget2D(GraphicsDevice, WaterMapSize, WaterMapSize, false, SurfaceFormat.Color, DepthFormat.Depth24);

			_terrainChunkIndexBuffer = new IndexBuffer(GraphicsDevice, typeof(ushort), TerrainChunk.Indices.Length, BufferUsage.WriteOnly);
			_terrainChunkIndexBuffer.SetData(TerrainChunk.Indices);

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			_terrainEffect = Content.Load<Effect>("Effects/TerrainEffect");
			_waterEffect = Content.Load<Effect>("Effects/WaterEffect");
			_treeEffect = Content.Load<Effect>("Effects/TreeEffect");

			_groundTexture = Content.Load<Texture2D>("Textures/ground");
			_groundSlopeTexture = Content.Load<Texture2D>("Textures/groundSlope");
			_groundDetailTexture = Content.Load<Texture2D>("Textures/groundDetail");
			_waterNormalTexture = Content.Load<Texture2D>("Textures/waterNormal");
			_treeTexture = Content.Load<Texture2D>("Textures/tree");
			_sunTexture = Content.Load<Texture2D>("Textures/sun");

			LoadWaterTile();
			LoadTree();
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// game-specific content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			ProcessInput();

			IsMouseVisible = !_mouseLookActive;

			float timeDelta = gameTime.ElapsedGameTime.Milliseconds/1000.0f;

			_waterTextureCoordinateOffset1.X = (_waterTextureCoordinateOffset1.X + timeDelta/10) % 1;
			_waterTextureCoordinateOffset1.Y = (_waterTextureCoordinateOffset1.Y + timeDelta/10) % 1;
			_waterTextureCoordinateOffset2.X = ((_waterTextureCoordinateOffset2.X - timeDelta/15) % 1) + 1;

			_world.Update(_camera.Position);

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			// PASS 1: Draw the water refraction map
			GraphicsDevice.SetRenderTarget(_waterRefractionMapRenderTarget);
			GraphicsDevice.Clear(Color.Black);

				DrawTerrain(false, WaterClipMode.Above, -1, false);

			// PASS 2: Draw the water reflection map
			GraphicsDevice.SetRenderTarget(_waterReflectionMapRenderTarget);
			GraphicsDevice.Clear(Color.CornflowerBlue);

				DrawTerrain(true, WaterClipMode.Below, 0, true);

			// PASS 3: Draw the scene
			GraphicsDevice.SetRenderTarget(null);
			GraphicsDevice.Clear(Color.CornflowerBlue);

				DrawSky();
				DrawTerrain(true, WaterClipMode.None, 0, false);
				DrawWater();
				DrawDecor();

			base.Draw(gameTime);
		}

		#endregion
	}
}
