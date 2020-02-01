using ChunkedTerrainCore.Utility.Enums;
using Microsoft.Xna.Framework;
using System;

namespace ChunkedTerrainCore.Utility
{
	public class Camera
	{
		#region Constants

		private const float MaxAltitudeAngle = 1.483530f;

		#endregion

		#region Fields

		private Vector3 _position;
		private float _altitudeAngle, _azimuthAngle;

		private Matrix _projectionMatrix;

		#endregion

		#region Properties

		public Vector3 Position
		{
			get
			{
				return _position;
			}
			set
			{
				_position = value;

				Refresh();
			}
		}

		public float AltitudeAngle
		{
			get
			{
				return _altitudeAngle;
			}
			set
			{
				if (value > MaxAltitudeAngle)
				{
					_altitudeAngle = MaxAltitudeAngle;
				}
				else if (value < -MaxAltitudeAngle)
				{
					_altitudeAngle = -MaxAltitudeAngle;
				}
				else
				{
					_altitudeAngle = value;
				}

				Refresh();
			}
		}

		public float AzimuthAngle
		{
			get
			{
				return _azimuthAngle;
			}
			set
			{
				_azimuthAngle = value;

				Refresh();
			}
		}

		public CameraMode Mode { get; set; }

		public Vector3 LookDirection { get; private set; }

		public Matrix ViewMatrix { get; private set; }

		public Matrix ViewMatrixWaterReflect { get; private set; }

		public Matrix ProjectionMatrix
		{
			get
			{
				return _projectionMatrix;
			}
			set
			{
				_projectionMatrix = value;

				Refresh();
			}
		}

		public BoundingFrustum Frustum { get; private set; }

		public BoundingFrustum FrustumWaterReflect { get; private set; }

		#endregion

		#region Constructors

		public Camera()
		{
			Refresh();
		}

		#endregion

		#region Private methods

		private void Refresh()
		{
			LookDirection = new Vector3((float)(Math.Sin(AzimuthAngle)*Math.Cos(AltitudeAngle)), (float)Math.Sin(AltitudeAngle), (float)(-Math.Cos(AzimuthAngle)*Math.Cos(AltitudeAngle)));
			ViewMatrix = Matrix.CreateLookAt(Position, Position + LookDirection, Vector3.UnitY);
			Frustum = new BoundingFrustum(ViewMatrix*ProjectionMatrix);

			Vector3 positionWaterReflect = new Vector3(Position.X, -Position.Y, Position.Z);
			Vector3 lookDirectionWaterReflect = new Vector3(LookDirection.X, -LookDirection.Y, LookDirection.Z);
			ViewMatrixWaterReflect = Matrix.CreateLookAt(positionWaterReflect, positionWaterReflect + lookDirectionWaterReflect, Vector3.UnitY);
			FrustumWaterReflect = new BoundingFrustum(ViewMatrixWaterReflect*ProjectionMatrix);
		}

		#endregion
	}
}
