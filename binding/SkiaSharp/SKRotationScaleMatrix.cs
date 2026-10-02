#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>Represents a 2D rotation and uniform scale matrix.</summary>
	/// <remarks>This struct stores rotation as scaled cosine/sine values (<see cref="P:SkiaSharp.SKRotationScaleMatrix.SCos" /> and <see cref="P:SkiaSharp.SKRotationScaleMatrix.SSin" />) and translation (<see cref="P:SkiaSharp.SKRotationScaleMatrix.TX" /> and <see cref="P:SkiaSharp.SKRotationScaleMatrix.TY" />). Use <see cref="M:SkiaSharp.SKRotationScaleMatrix.ToMatrix" /> to convert to a full <see cref="T:SkiaSharp.SKMatrix" />.</remarks>
	public unsafe partial struct SKRotationScaleMatrix
	{
		/// <summary>Represents an empty matrix with all components set to zero.</summary>
		/// <remarks />
		public static readonly SKRotationScaleMatrix Empty;

		/// <summary>Represents the identity matrix (scale of 1, no rotation, no translation).</summary>
		/// <remarks />
		public static readonly SKRotationScaleMatrix Identity = new SKRotationScaleMatrix (1, 0, 0, 0);

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKRotationScaleMatrix" /> with the specified components.</summary>
		/// <param name="scos">The scaled cosine value (scale * cos(angle)).</param>
		/// <param name="ssin">The scaled sine value (scale * sin(angle)).</param>
		/// <param name="tx">The x-axis translation component.</param>
		/// <param name="ty">The y-axis translation component.</param>
		/// <remarks />
		public SKRotationScaleMatrix (float scos, float ssin, float tx, float ty)
		{
			fSCos = scos;
			fSSin = ssin;
			fTX = tx;
			fTY = ty;
		}

		/// <summary>Converts this rotation-scale matrix to a full 3x3 transformation matrix.</summary>
		/// <returns>A <see cref="T:SkiaSharp.SKMatrix" /> representing the same transformation.</returns>
		/// <remarks />
		public readonly SKMatrix ToMatrix () =>
			new SKMatrix (fSCos, -fSSin, fTX, fSSin, fSCos, fTY, 0, 0, 1);

		/// <summary>Creates a rotation-scale matrix with the specified scale, rotation (in degrees), translation, and anchor point.</summary>
		/// <param name="scale">The uniform scale factor.</param>
		/// <param name="degrees">The rotation angle in degrees.</param>
		/// <param name="tx">The x-axis translation component.</param>
		/// <param name="ty">The y-axis translation component.</param>
		/// <param name="anchorX">The x-coordinate of the anchor point for rotation.</param>
		/// <param name="anchorY">The y-coordinate of the anchor point for rotation.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRotationScaleMatrix" /> representing the combined transformation.</returns>
		/// <remarks />
		public static SKRotationScaleMatrix CreateDegrees (float scale, float degrees, float tx, float ty, float anchorX, float anchorY) =>
			Create (scale, degrees * SKMatrix.DegreesToRadians, tx, ty, anchorX, anchorY);

		/// <summary>Creates a rotation-scale matrix with the specified scale, rotation (in radians), translation, and anchor point.</summary>
		/// <param name="scale">The uniform scale factor.</param>
		/// <param name="radians">The rotation angle in radians.</param>
		/// <param name="tx">The x-axis translation component.</param>
		/// <param name="ty">The y-axis translation component.</param>
		/// <param name="anchorX">The x-coordinate of the anchor point for rotation.</param>
		/// <param name="anchorY">The y-coordinate of the anchor point for rotation.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRotationScaleMatrix" /> representing the combined transformation.</returns>
		/// <remarks />
		public static SKRotationScaleMatrix Create (float scale, float radians, float tx, float ty, float anchorX, float anchorY)
		{
			var s = (float)Math.Sin (radians) * scale;
			var c = (float)Math.Cos (radians) * scale;
			var x = tx + -c * anchorX + s * anchorY;
			var y = ty + -s * anchorX - c * anchorY;

			return new SKRotationScaleMatrix (c, s, x, y);
		}

		/// <summary>Creates an identity matrix that represents no transformation.</summary>
		/// <returns>A new <see cref="T:SkiaSharp.SKRotationScaleMatrix" /> with scale of 1, no rotation, and no translation.</returns>
		/// <remarks />
		public static SKRotationScaleMatrix CreateIdentity () =>
			new SKRotationScaleMatrix (1, 0, 0, 0);

		/// <summary>Creates a translation matrix with no rotation or scale.</summary>
		/// <param name="x">The x-axis translation value.</param>
		/// <param name="y">The y-axis translation value.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRotationScaleMatrix" /> representing the translation.</returns>
		/// <remarks />
		public static SKRotationScaleMatrix CreateTranslation (float x, float y) =>
			new SKRotationScaleMatrix (1, 0, x, y);

		/// <summary>Creates a uniform scale matrix with no rotation or translation.</summary>
		/// <param name="s">The uniform scale factor.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRotationScaleMatrix" /> representing the scale transformation.</returns>
		/// <remarks />
		public static SKRotationScaleMatrix CreateScale (float s) =>
			new SKRotationScaleMatrix (s, 0, 0, 0);

		/// <summary>Creates a rotation matrix with the specified angle (in radians) around an anchor point.</summary>
		/// <param name="radians">The rotation angle in radians.</param>
		/// <param name="anchorX">The x-coordinate of the anchor point for rotation.</param>
		/// <param name="anchorY">The y-coordinate of the anchor point for rotation.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRotationScaleMatrix" /> representing the rotation.</returns>
		/// <remarks />
		public static SKRotationScaleMatrix CreateRotation (float radians, float anchorX, float anchorY) =>
			Create (1, radians, 0, 0, anchorX, anchorY);

		/// <summary>Creates a rotation matrix with the specified angle (in degrees) around an anchor point.</summary>
		/// <param name="degrees">The rotation angle in degrees.</param>
		/// <param name="anchorX">The x-coordinate of the anchor point for rotation.</param>
		/// <param name="anchorY">The y-coordinate of the anchor point for rotation.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRotationScaleMatrix" /> representing the rotation.</returns>
		/// <remarks />
		public static SKRotationScaleMatrix CreateRotationDegrees (float degrees, float anchorX, float anchorY) =>
			CreateDegrees (1, degrees, 0, 0, anchorX, anchorY);
	}
}
