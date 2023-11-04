#nullable disable

using System;

namespace SkiaSharp
{
	public unsafe partial struct SKMatrix
	{
		internal const float DegreesToRadians = (float)Math.PI / 180.0f;

		public readonly static SKMatrix Empty;

		public readonly static SKMatrix Identity = new SKMatrix { scaleX = 1, scaleY = 1, persp2 = 1 };

		private class Indices
		{
			public const int ScaleX = 0;
			public const int SkewX = 1;
			public const int TransX = 2;
			public const int SkewY = 3;
			public const int ScaleY = 4;
			public const int TransY = 5;
			public const int Persp0 = 6;
			public const int Persp1 = 7;
			public const int Persp2 = 8;

			public const int Count = 9;
		}

		public SKMatrix (float[] values)
		{
			if (values == null)
				throw new ArgumentNullException (nameof (values));
			if (values.Length != Indices.Count)
				throw new ArgumentException ($"The matrix array must have a length of {Indices.Count}.", nameof (values));

			scaleX = values[Indices.ScaleX];
			skewX = values[Indices.SkewX];
			transX = values[Indices.TransX];

			skewY = values[Indices.SkewY];
			scaleY = values[Indices.ScaleY];
			transY = values[Indices.TransY];

			persp0 = values[Indices.Persp0];
			persp1 = values[Indices.Persp1];
			persp2 = values[Indices.Persp2];
		}

		public SKMatrix (
			float scaleX, float skewX, float transX,
			float skewY, float scaleY, float transY,
			float persp0, float persp1, float persp2)
		{
			this.scaleX = scaleX;
			this.skewX = skewX;
			this.transX = transX;
			this.skewY = skewY;
			this.scaleY = scaleY;
			this.transY = transY;
			this.persp0 = persp0;
			this.persp1 = persp1;
			this.persp2 = persp2;
		}

		public readonly bool IsIdentity => Equals (Identity);

		// Values

		public float[] Values {
			readonly get =>
				new float[9] {
					scaleX, skewX, transX,
					skewY, scaleY, transY,
					persp0, persp1, persp2
				};
			set {
				if (value == null)
					throw new ArgumentNullException (nameof (Values));
				if (value.Length != Indices.Count)
					throw new ArgumentException ($"The matrix array must have a length of {Indices.Count}.", nameof (Values));

				scaleX = value[Indices.ScaleX];
				skewX = value[Indices.SkewX];
				transX = value[Indices.TransX];

				skewY = value[Indices.SkewY];
				scaleY = value[Indices.ScaleY];
				transY = value[Indices.TransY];

				persp0 = value[Indices.Persp0];
				persp1 = value[Indices.Persp1];
				persp2 = value[Indices.Persp2];
			}
		}

		public readonly void GetValues (float[] values)
		{
			if (values == null)
				throw new ArgumentNullException (nameof (values));
			if (values.Length != Indices.Count)
				throw new ArgumentException ($"The matrix array must have a length of {Indices.Count}.", nameof (values));

			values[Indices.ScaleX] = scaleX;
			values[Indices.SkewX] = skewX;
			values[Indices.TransX] = transX;

			values[Indices.SkewY] = skewY;
			values[Indices.ScaleY] = scaleY;
			values[Indices.TransY] = transY;

			values[Indices.Persp0] = persp0;
			values[Indices.Persp1] = persp1;
			values[Indices.Persp2] = persp2;
		}

		// Create*

		public static SKMatrix CreateIdentity () =>
			new SKMatrix { scaleX = 1, scaleY = 1, persp2 = 1 };

		public static SKMatrix CreateTranslation (float x, float y)
		{
			if (x == 0 && y == 0)
				return Identity;

			return new SKMatrix {
				scaleX = 1,
				scaleY = 1,
				transX = x,
				transY = y,
				persp2 = 1,
			};
		}

		public static SKMatrix CreateScale (float x, float y)
		{
			if (x == 1 && y == 1)
				return Identity;

			return new SKMatrix {
				scaleX = x,
				scaleY = y,
				persp2 = 1,
			};
		}

		public static SKMatrix CreateScale (float x, float y, float pivotX, float pivotY)
		{
			if (x == 1 && y == 1)
				return Identity;

			var tx = pivotX - x * pivotX;
			var ty = pivotY - y * pivotY;

			return new SKMatrix {
				scaleX = x,
				scaleY = y,
				transX = tx,
				transY = ty,
				persp2 = 1,
			};
		}

		public static SKMatrix CreateRotation (float radians)
		{
			if (radians == 0)
				return Identity;

			var sin = (float)Math.Sin (radians);
			var cos = (float)Math.Cos (radians);

			var matrix = Identity;
			SetSinCos (ref matrix, sin, cos);
			return matrix;
		}

		public static SKMatrix CreateRotation (float radians, float pivotX, float pivotY)
		{
			if (radians == 0)
				return Identity;

			var sin = (float)Math.Sin (radians);
			var cos = (float)Math.Cos (radians);

			var matrix = Identity;
			SetSinCos (ref matrix, sin, cos, pivotX, pivotY);
			return matrix;
		}

		public static SKMatrix CreateRotationDegrees (float degrees)
		{
			if (degrees == 0)
				return Identity;

			return CreateRotation (degrees * DegreesToRadians);
		}

		public static SKMatrix CreateRotationDegrees (float degrees, float pivotX, float pivotY)
		{
			if (degrees == 0)
				return Identity;

			return CreateRotation (degrees * DegreesToRadians, pivotX, pivotY);
		}

		public static SKMatrix CreateSkew (float x, float y)
		{
			if (x == 0 && y == 0)
				return Identity;

			return new SKMatrix {
				scaleX = 1,
				skewX = x,
				skewY = y,
				scaleY = 1,
				persp2 = 1,
			};
		}

		public static SKMatrix CreateScaleTranslation (float sx, float sy, float tx, float ty)
		{
			if (sx == 0 && sy == 0 && tx == 0 && ty == 0)
				return Identity;

			return new SKMatrix {
				scaleX = sx,
				skewX = 0,
				transX = tx,

				skewY = 0,
				scaleY = sy,
				transY = ty,

				persp0 = 0,
				persp1 = 0,
				persp2 = 1,
			};
		}

		// Invert

		public readonly bool IsInvertible {
			get {
				fixed (SKMatrix* t = &this) {
					return SkiaApi.sk_matrix_try_invert (t, null);
				}
			}
		}

		public readonly bool TryInvert (out SKMatrix inverse)
		{
			fixed (SKMatrix* i = &inverse)
			fixed (SKMatrix* t = &this) {
				return SkiaApi.sk_matrix_try_invert (t, i);
			}
		}

		public readonly SKMatrix Invert ()
		{
			if (TryInvert (out var matrix))
				return matrix;

			return Empty;
		}

		// *Concat

		public static SKMatrix Concat (SKMatrix first, SKMatrix second)
		{
			SKMatrix target;
			SkiaApi.sk_matrix_concat (&target, &first, &second);
			return target;
		}

		public readonly SKMatrix PreConcat (SKMatrix matrix)
		{
			var target = this;
			SkiaApi.sk_matrix_pre_concat (&target, &matrix);
			return target;
		}

		public readonly SKMatrix PostConcat (SKMatrix matrix)
		{
			var target = this;
			SkiaApi.sk_matrix_post_concat (&target, &matrix);
			return target;
		}

		public static void Concat (ref SKMatrix target, SKMatrix first, SKMatrix second)
		{
			fixed (SKMatrix* t = &target) {
				SkiaApi.sk_matrix_concat (t, &first, &second);
			}
		}

		// MapRect

		public readonly SKRect MapRect (SKRect source)
		{
			SKRect dest;
			fixed (SKMatrix* m = &this) {
				SkiaApi.sk_matrix_map_rect (m, &dest, &source);
			}
			return dest;
		}

		// MapPoints

		public readonly SKPoint MapPoint (SKPoint point) =>
			MapPoint (point.X, point.Y);

		public readonly SKPoint MapPoint (float x, float y)
		{
			SKPoint result;
			fixed (SKMatrix* t = &this) {
				SkiaApi.sk_matrix_map_xy (t, x, y, &result);
			}
			return result;
		}

		public readonly void MapPoints (SKPoint[] result, SKPoint[] points)
		{
			if (result == null)
				throw new ArgumentNullException (nameof (result));
			if (points == null)
				throw new ArgumentNullException (nameof (points));
			if (result.Length != points.Length)
				throw new ArgumentException ("Buffers must be the same size.");

			fixed (SKMatrix* t = &this)
			fixed (SKPoint* rp = result)
			fixed (SKPoint* pp = points) {
				SkiaApi.sk_matrix_map_points (t, rp, pp, result.Length);
			}
		}

		public readonly SKPoint[] MapPoints (SKPoint[] points)
		{
			if (points == null)
				throw new ArgumentNullException (nameof (points));

			var res = new SKPoint[points.Length];
			MapPoints (res, points);
			return res;
		}

		// MapVectors

		public readonly SKPoint MapVector (SKPoint vector) =>
			MapVector (vector.X, vector.Y);

		public readonly SKPoint MapVector (float x, float y)
		{
			SKPoint result;
			fixed (SKMatrix* t = &this) {
				SkiaApi.sk_matrix_map_vector (t, x, y, &result);
			}
			return result;
		}

		public readonly void MapVectors (SKPoint[] result, SKPoint[] vectors)
		{
			if (result == null)
				throw new ArgumentNullException (nameof (result));
			if (vectors == null)
				throw new ArgumentNullException (nameof (vectors));
			if (result.Length != vectors.Length)
				throw new ArgumentException ("Buffers must be the same size.");

			fixed (SKMatrix* t = &this)
			fixed (SKPoint* rp = result)
			fixed (SKPoint* pp = vectors) {
				SkiaApi.sk_matrix_map_vectors (t, rp, pp, result.Length);
			}
		}

		public readonly SKPoint[] MapVectors (SKPoint[] vectors)
		{
			if (vectors == null)
				throw new ArgumentNullException (nameof (vectors));

			var res = new SKPoint[vectors.Length];
			MapVectors (res, vectors);
			return res;
		}

		// MapRadius

		public readonly float MapRadius (float radius)
		{
			fixed (SKMatrix* t = &this) {
				return SkiaApi.sk_matrix_map_radius (t, radius);
			}
		}

		// private

		private static void SetSinCos (ref SKMatrix matrix, float sin, float cos)
		{
			matrix.scaleX = cos;
			matrix.skewX = -sin;
			matrix.transX = 0;
			matrix.skewY = sin;
			matrix.scaleY = cos;
			matrix.transY = 0;
			matrix.persp0 = 0;
			matrix.persp1 = 0;
			matrix.persp2 = 1;
		}

		private static void SetSinCos (ref SKMatrix matrix, float sin, float cos, float pivotx, float pivoty)
		{
			float oneMinusCos = 1 - cos;

			matrix.scaleX = cos;
			matrix.skewX = -sin;
			matrix.transX = Dot (sin, pivoty, oneMinusCos, pivotx);
			matrix.skewY = sin;
			matrix.scaleY = cos;
			matrix.transY = Dot (-sin, pivotx, oneMinusCos, pivoty);
			matrix.persp0 = 0;
			matrix.persp1 = 0;
			matrix.persp2 = 1;
		}

		private static float Dot (float a, float b, float c, float d) =>
			a * b + c * d;

		private static float Cross (float a, float b, float c, float d) =>
			a * b - c * d;
	}
}
