using System;

namespace SkiaSharp
{
	public unsafe partial struct SKRotationScaleMatrix
	{
		public static readonly SKRotationScaleMatrix Empty;

		public static readonly SKRotationScaleMatrix Identity = new SKRotationScaleMatrix (1, 0, 0, 0);

		public SKRotationScaleMatrix (float scos, float ssin, float tx, float ty)
		{
			fSCos = scos;
			fSSin = ssin;
			fTX = tx;
			fTY = ty;
		}

		public readonly SKMatrix ToMatrix () =>
			new SKMatrix (fSCos, -fSSin, fTX, fSSin, fSCos, fTY, 0, 0, 1);

		public static SKRotationScaleMatrix CreateDegrees (float scale, float degrees, float tx, float ty, float anchorX, float anchorY) =>
			Create (scale, degrees * SKMatrix.DegreesToRadians, tx, ty, anchorX, anchorY);

		public static SKRotationScaleMatrix Create (float scale, float radians, float tx, float ty, float anchorX, float anchorY)
		{
			var s = (float)Math.Sin (radians) * scale;
			var c = (float)Math.Cos (radians) * scale;
			var x = tx + -c * anchorX + s * anchorY;
			var y = ty + -s * anchorX - c * anchorY;

			return new SKRotationScaleMatrix (c, s, x, y);
		}

		public static SKRotationScaleMatrix CreateIdentity () =>
			new SKRotationScaleMatrix (1, 0, 0, 0);

		public static SKRotationScaleMatrix CreateTranslation (float x, float y) =>
			new SKRotationScaleMatrix (1, 0, x, y);

		public static SKRotationScaleMatrix CreateScale (float s) =>
			new SKRotationScaleMatrix (s, 0, 0, 0);

		public static SKRotationScaleMatrix CreateRotation (float radians, float anchorX, float anchorY) =>
			Create (1, radians, 0, 0, anchorX, anchorY);

		public static SKRotationScaleMatrix CreateRotationDegrees (float degrees, float anchorX, float anchorY) =>
			CreateDegrees (1, degrees, 0, 0, anchorX, anchorY);
	}
}
