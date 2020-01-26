using System;

namespace SkiaSharp
{
	public unsafe partial struct SKMatrix : IEquatable<SKMatrix>
	{
		internal const float DegreesToRadians = (float)Math.PI / 180.0f;

		public static readonly SKMatrix Empty;
		public static readonly SKMatrix Identity = new SKMatrix (1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 1f);

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

		public SKMatrix (float value)
		{
			scaleX = value;
			skewX = value;
			transX = value;

			skewY = value;
			scaleY = value;
			transY = value;

			persp0 = value;
			persp1 = value;
			persp2 = value;
		}

		public SKMatrix (ReadOnlySpan<float> values)
		{
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
			readonly get => new float[9] {
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

		public readonly void GetValues (Span<float> values)
		{
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

		public void PreConcat (in SKMatrix matrix)
		{
			fixed (SKMatrix* t = &this)
			fixed (SKMatrix* m = &matrix) {
				SkiaApi.sk_matrix_pre_concat (t, m);
			}
		}

		public void PostConcat (in SKMatrix matrix)
		{
			fixed (SKMatrix* t = &this)
			fixed (SKMatrix* m = &matrix) {
				SkiaApi.sk_matrix_post_concat (t, m);
			}
		}

		public static void Concat (ref SKMatrix target, in SKMatrix first, in SKMatrix second)
		{
			fixed (SKMatrix* t = &target)
			fixed (SKMatrix* f = &first)
			fixed (SKMatrix* s = &second) {
				SkiaApi.sk_matrix_concat (t, f, s);
			}
		}

		public static void PreConcat (ref SKMatrix target, in SKMatrix matrix)
		{
			fixed (SKMatrix* t = &target)
			fixed (SKMatrix* m = &matrix) {
				SkiaApi.sk_matrix_pre_concat (t, m);
			}
		}

		public static void PostConcat (ref SKMatrix target, in SKMatrix matrix)
		{
			fixed (SKMatrix* t = &target)
			fixed (SKMatrix* m = &matrix) {
				SkiaApi.sk_matrix_post_concat (t, m);
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

		public readonly void MapPoints (ReadOnlySpan<SKPoint> points, Span<SKPoint> result)
		{
			if (result.Length != points.Length)
				throw new ArgumentException ("Buffers must be the same size.");

			fixed (SKMatrix* t = &this)
			fixed (SKPoint* rp = result)
			fixed (SKPoint* pp = points) {
				SkiaApi.sk_matrix_map_points (t, rp, pp, result.Length);
			}
		}

		public readonly SKPoint[] MapPoints (ReadOnlySpan<SKPoint> points)
		{
			var res = new SKPoint[points.Length];
			MapPoints (points, res);
			return res;
		}

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

		// MapVectors

		public readonly void MapVectors (ReadOnlySpan<SKPoint> vectors, Span<SKPoint> result)
		{
			if (result.Length != vectors.Length)
				throw new ArgumentException ("Buffers must be the same size.");

			fixed (SKMatrix* t = &this)
			fixed (SKPoint* rp = result)
			fixed (SKPoint* pp = vectors) {
				SkiaApi.sk_matrix_map_vectors (t, rp, pp, result.Length);
			}
		}

		public readonly SKPoint[] MapVectors (ReadOnlySpan<SKPoint> vectors)
		{
			var res = new SKPoint[vectors.Length];
			MapVectors (vectors, res);
			return res;
		}

		public readonly SKPoint MapVector (SKPoint point) =>
			MapVector (point.X, point.Y);

		public readonly SKPoint MapVector (float x, float y)
		{
			SKPoint result;
			fixed (SKMatrix* t = &this) {
				SkiaApi.sk_matrix_map_vector (t, x, y, &result);
			}
			return result;
		}

		// MapRadius

		public readonly float MapRadius (float radius)
		{
			fixed (SKMatrix* t = &this) {
				return SkiaApi.sk_matrix_map_radius (t, radius);
			}
		}

		// equality

		public readonly bool Equals (SKMatrix other) =>
			scaleX == other.scaleX &&
			skewX == other.skewX &&
			transX == other.transX &&
			skewY == other.skewY &&
			scaleY == other.scaleY &&
			transY == other.transY &&
			persp0 == other.persp0 &&
			persp1 == other.persp1 &&
			persp2 == other.persp2;

		// Create*

		public static SKMatrix CreateTranslation (float dx, float dy)
		{
			if (dx == 0 && dy == 0)
				return Identity;

			return new SKMatrix {
				scaleX = 1,
				scaleY = 1,
				transX = dx,
				transY = dy,
				persp2 = 1,
			};
		}

		public static SKMatrix CreateScale (float sx, float sy)
		{
			if (sx == 1 && sy == 1)
				return Identity;

			return new SKMatrix {
				scaleX = sx,
				scaleY = sy,
				persp2 = 1,
			};
		}

		public static SKMatrix CreateScale (float sx, float sy, float pivotX, float pivotY)
		{
			if (sx == 1 && sy == 1)
				return Identity;

			var tx = pivotX - sx * pivotX;
			var ty = pivotY - sy * pivotY;

			return new SKMatrix {
				scaleX = sx,
				scaleY = sy,
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

		public static SKMatrix CreateRotation (float radians, float pivotx, float pivoty)
		{
			if (radians == 0)
				return Identity;

			var sin = (float)Math.Sin (radians);
			var cos = (float)Math.Cos (radians);

			var matrix = Identity;
			SetSinCos (ref matrix, sin, cos, pivotx, pivoty);
			return matrix;
		}

		public static SKMatrix CreateRotationDegrees (float degrees)
		{
			if (degrees == 0)
				return Identity;

			return CreateRotation (degrees * DegreesToRadians);
		}

		public static SKMatrix CreateRotationDegrees (float degrees, float pivotx, float pivoty)
		{
			if (degrees == 0)
				return Identity;

			return CreateRotation (degrees * DegreesToRadians, pivotx, pivoty);
		}

		public static SKMatrix CreateSkew (float sx, float sy)
		{
			if (sx == 0 && sy == 0)
				return Identity;

			return new SKMatrix {
				scaleX = 1,
				skewX = sx,
				skewY = sy,
				scaleY = 1,
				persp2 = 1,
			};
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

		private static void SetSinCos (ref SKMatrix matrix, float sin, float cos, float pivotx = 0f, float pivoty = 0f)
		{
			var oneMinusCos = 1 - cos;

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

		private static float Dot (float a, float b, float c, float d) => a * b + c * d;

		private static float Cross (float a, float b, float c, float d) => a * b - c * d;
	}
}
