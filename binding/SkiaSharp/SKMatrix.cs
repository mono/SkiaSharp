#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>A 3x3 transformation matrix with perspective.</summary>
	/// <remarks>It extends the traditional 2D affine transformation matrix with three perspective components that allow simple 3D effects to be created with it. Those components must be manually set by using the <see cref="P:SkiaSharp.SKMatrix.Persp0" />, <see cref="P:SkiaSharp.SKMatrix.Persp1" />, <see cref="P:SkiaSharp.SKMatrix.Persp2" /> fields of the matrix.</remarks>
	public unsafe partial struct SKMatrix
	{
		internal const float DegreesToRadians = (float)Math.PI / 180.0f;

		/// <summary>Represents an empty matrix with all values set to zero.</summary>
		/// <remarks />
		public readonly static SKMatrix Empty;

		/// <summary>Gets the identity matrix.</summary>
		/// <remarks />
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

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKMatrix" /> struct from an array of 9 values in row-major order.</summary>
		/// <param name="values">An array of 9 values in row-major order.</param>
		/// <remarks />
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

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKMatrix" /> using the specified values.</summary>
		/// <param name="scaleX">The scaling in the x-direction.</param>
		/// <param name="skewX">The skew in the x-direction.</param>
		/// <param name="transX">The translation in the x-direction.</param>
		/// <param name="skewY">The skew in the y-direction.</param>
		/// <param name="scaleY">The scaling in the y-direction.</param>
		/// <param name="transY">The translation in the y-direction.</param>
		/// <param name="persp0">The x-perspective.</param>
		/// <param name="persp1">The y-perspective.</param>
		/// <param name="persp2">The z-perspective.</param>
		/// <remarks />
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

		/// <summary>Gets a value indicating whether this matrix is the identity matrix.</summary>
		/// <value>Returns <see langword="true" /> if this is an identity matrix; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public readonly bool IsIdentity => Equals (Identity);

		// Values

		/// <summary>Gets or sets the matrix as a flat array: [ScaleX, SkewX, TransX, SkewY, ScaleY, TransY, Persp0, Persp1, Persp2].</summary>
		/// <value>The matrix as a flat array of 9 values.</value>
		/// <remarks />
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

		/// <summary>Populates the specified array with the matrix values.</summary>
		/// <param name="values">The array to populate.</param>
		/// <remarks>The result will be the same as <see cref="P:SkiaSharp.SKMatrix.Values" />.</remarks>
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

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKMatrix" /> struct as an identity matrix.</summary>
		/// <returns>Returns the new identity matrix.</returns>
		/// <remarks />
		public static SKMatrix CreateIdentity () =>
			new SKMatrix { scaleX = 1, scaleY = 1, persp2 = 1 };

		/// <summary>Creates a translation matrix.</summary>
		/// <param name="x">The translation in the x-direction.</param>
		/// <param name="y">The translation in the y-direction.</param>
		/// <returns>Returns the new translation matrix.</returns>
		/// <remarks />
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

		/// <summary>Creates a scaling matrix.</summary>
		/// <param name="x">The scale factor in the x-direction.</param>
		/// <param name="y">The scale factor in the y-direction.</param>
		/// <returns>Returns the new scaling matrix.</returns>
		/// <remarks />
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

		/// <summary>Creates a scaling matrix around a pivot point.</summary>
		/// <param name="x">The scale factor in the x-direction.</param>
		/// <param name="y">The scale factor in the y-direction.</param>
		/// <param name="pivotX">The x-coordinate of the pivot point.</param>
		/// <param name="pivotY">The y-coordinate of the pivot point.</param>
		/// <returns>Returns the new scaling matrix.</returns>
		/// <remarks />
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

		/// <summary>Creates a rotation matrix.</summary>
		/// <param name="radians">The rotation angle in radians.</param>
		/// <returns>Returns the new rotation matrix.</returns>
		/// <remarks />
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

		/// <summary>Creates a rotation matrix around a pivot point.</summary>
		/// <param name="radians">The rotation angle in radians.</param>
		/// <param name="pivotX">The x-coordinate of the pivot point.</param>
		/// <param name="pivotY">The y-coordinate of the pivot point.</param>
		/// <returns>Returns the new rotation matrix.</returns>
		/// <remarks />
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

		/// <summary>Creates a rotation matrix.</summary>
		/// <param name="degrees">The rotation angle in degrees.</param>
		/// <returns>Returns the new rotation matrix.</returns>
		/// <remarks />
		public static SKMatrix CreateRotationDegrees (float degrees)
		{
			if (degrees == 0)
				return Identity;

			return CreateRotation (degrees * DegreesToRadians);
		}

		/// <summary>Creates a rotation matrix around a pivot point.</summary>
		/// <param name="degrees">The rotation angle in degrees.</param>
		/// <param name="pivotX">The x-coordinate of the pivot point.</param>
		/// <param name="pivotY">The y-coordinate of the pivot point.</param>
		/// <returns>Returns the new rotation matrix.</returns>
		/// <remarks />
		public static SKMatrix CreateRotationDegrees (float degrees, float pivotX, float pivotY)
		{
			if (degrees == 0)
				return Identity;

			return CreateRotation (degrees * DegreesToRadians, pivotX, pivotY);
		}

		/// <summary>Creates a skewing matrix.</summary>
		/// <param name="x">The skew factor in the x-direction.</param>
		/// <param name="y">The skew factor in the y-direction.</param>
		/// <returns>Returns the new skewing matrix.</returns>
		/// <remarks />
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

		/// <summary>Creates a matrix with scale and translation.</summary>
		/// <param name="sx">The scale factor in the x-direction.</param>
		/// <param name="sy">The scale factor in the y-direction.</param>
		/// <param name="tx">The translation in the x-direction.</param>
		/// <param name="ty">The translation in the y-direction.</param>
		/// <returns>Returns the new matrix.</returns>
		/// <remarks />
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

		/// <summary>Gets a value indicating whether this matrix can be inverted.</summary>
		/// <value>Returns <see langword="true" /> if the matrix is invertible; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public readonly bool IsInvertible {
			get {
				fixed (SKMatrix* t = &this) {
					return SkiaApi.sk_matrix_try_invert (t, null);
				}
			}
		}

		/// <summary>Attempts to invert the matrix, if possible the inverse matrix contains the result.</summary>
		/// <param name="inverse">The destination value to store the inverted matrix if the matrix can be inverted.</param>
		/// <returns><see langword="true" /> if the matrix can be inverted, and the inverse parameter is initialized with the inverted matrix; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool TryInvert (out SKMatrix inverse)
		{
			fixed (SKMatrix* i = &inverse)
			fixed (SKMatrix* t = &this) {
				return SkiaApi.sk_matrix_try_invert (t, i);
			}
		}

		/// <summary>Returns the inverse of this matrix, if it exists.</summary>
		/// <returns>Returns the inverse matrix, or <see cref="F:SkiaSharp.SKMatrix.Empty" /> if the matrix is not invertible.</returns>
		/// <remarks />
		public readonly SKMatrix Invert ()
		{
			if (TryInvert (out var matrix))
				return matrix;

			return Empty;
		}

		// *Concat

		/// <summary>Concatenates two matrices and returns the result.</summary>
		/// <param name="first">The first matrix to concatenate.</param>
		/// <param name="second">The second matrix to concatenate.</param>
		/// <returns>Returns the concatenated matrix.</returns>
		/// <remarks />
		public static SKMatrix Concat (SKMatrix first, SKMatrix second)
		{
			SKMatrix target;
			SkiaApi.sk_matrix_concat (&target, &first, &second);
			return target;
		}

		/// <summary>Pre-concatenates a matrix to this matrix and returns the result.</summary>
		/// <param name="matrix">The matrix to pre-concatenate.</param>
		/// <returns>Returns the concatenated matrix.</returns>
		/// <remarks>This represents: result = this * matrix</remarks>
		public readonly SKMatrix PreConcat (SKMatrix matrix)
		{
			var target = this;
			SkiaApi.sk_matrix_pre_concat (&target, &matrix);
			return target;
		}

		/// <summary>Post-concatenates a matrix to this matrix and returns the result.</summary>
		/// <param name="matrix">The matrix to post-concatenate.</param>
		/// <returns>Returns the concatenated matrix.</returns>
		/// <remarks>This represents: result = matrix * this</remarks>
		public readonly SKMatrix PostConcat (SKMatrix matrix)
		{
			var target = this;
			SkiaApi.sk_matrix_post_concat (&target, &matrix);
			return target;
		}

		/// <summary>Concatenates the specified matrices into the resulting target matrix.</summary>
		/// <param name="target">The result matrix value.</param>
		/// <param name="first">The first matrix to concatenate.</param>
		/// <param name="second">The second matrix to concatenate.</param>
		/// <remarks>Either source matrices can also be the target matrix.</remarks>
		public static void Concat (ref SKMatrix target, SKMatrix first, SKMatrix second)
		{
			fixed (SKMatrix* t = &target) {
				SkiaApi.sk_matrix_concat (t, &first, &second);
			}
		}

		// MapRect

		/// <summary>Applies the matrix to a rectangle.</summary>
		/// <param name="source">The source rectangle to map.</param>
		/// <returns>Returns the mapped rectangle.</returns>
		/// <remarks />
		public readonly SKRect MapRect (SKRect source)
		{
			SKRect dest;
			fixed (SKMatrix* m = &this) {
				SkiaApi.sk_matrix_map_rect (m, &dest, &source);
			}
			return dest;
		}

		// MapPoints

		/// <summary>Applies the matrix to a point.</summary>
		/// <param name="point">The point to map.</param>
		/// <returns>Returns the mapped point.</returns>
		/// <remarks>Mapping points uses all components of the matrix. Use <see cref="M:SkiaSharp.SKMatrix.MapVector(System.Single,System.Single)" /> to ignore the translation.</remarks>
		public readonly SKPoint MapPoint (SKPoint point) =>
			MapPoint (point.X, point.Y);

		/// <summary>Applies the matrix to a point.</summary>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <returns>Returns the mapped point.</returns>
		/// <remarks>Mapping points uses all components of the matrix. Use <see cref="M:SkiaSharp.SKMatrix.MapVector(System.Single,System.Single)" /> to ignore the translation.</remarks>
		public readonly SKPoint MapPoint (float x, float y)
		{
			SKPoint result;
			fixed (SKMatrix* t = &this) {
				SkiaApi.sk_matrix_map_xy (t, x, y, &result);
			}
			return result;
		}

		/// <summary>Transforms points using this matrix.</summary>
		/// <param name="result">The span where the transformed points will be stored.</param>
		/// <param name="points">The span of points to transform.</param>
		/// <remarks />
		public readonly void MapPoints (Span<SKPoint> result, ReadOnlySpan<SKPoint> points)
		{
			if (result.Length != points.Length)
				throw new ArgumentException ("Buffers must be the same size.");

			fixed (SKMatrix* t = &this)
			fixed (SKPoint* rp = result)
			fixed (SKPoint* pp = points) {
				SkiaApi.sk_matrix_map_points (t, rp, pp, result.Length);
			}
		}

		/// <summary>Applies the matrix to an array of points.</summary>
		/// <param name="result">The array where the mapped results will be stored (needs to have the same number of elements of the <paramref name="points" /> array).</param>
		/// <param name="points">The array of points to be mapped.</param>
		/// <remarks>Mapping points uses all components of the matrix. Use <see cref="M:SkiaSharp.SKMatrix.MapVectors(SkiaSharp.SKPoint[],SkiaSharp.SKPoint[])" /> to ignore the translation.</remarks>
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

		/// <summary>Applies the matrix to an array of points.</summary>
		/// <param name="points">The array of points to be mapped.</param>
		/// <returns>Returns the new array allocated with the mapped results.</returns>
		/// <remarks>Mapping points uses all components of the matrix. Use <see cref="M:SkiaSharp.SKMatrix.MapVectors(SkiaSharp.SKPoint[])" /> to ignore the translation.</remarks>
		public readonly SKPoint[] MapPoints (SKPoint[] points)
		{
			if (points == null)
				throw new ArgumentNullException (nameof (points));

			var res = new SKPoint[points.Length];
			MapPoints (res, points);
			return res;
		}

		// MapVectors

		/// <summary>Transforms a vector using this matrix, ignoring translation.</summary>
		/// <param name="vector">The vector to transform.</param>
		/// <returns>Returns the transformed vector.</returns>
		/// <remarks>Mapping vectors ignores the translation component in the matrix. Use <see cref="M:SkiaSharp.SKMatrix.MapPoint(SkiaSharp.SKPoint)" /> to take the translation into consideration.</remarks>
		public readonly SKPoint MapVector (SKPoint vector) =>
			MapVector (vector.X, vector.Y);

		/// <summary>Applies the matrix to a vector, ignoring translation.</summary>
		/// <param name="x">The x-component of the vector.</param>
		/// <param name="y">The y-component of the vector.</param>
		/// <returns>Returns the mapped point.</returns>
		/// <remarks>Mapping vectors ignores the translation component in the matrix. Use <see cref="M:SkiaSharp.SKMatrix.MapPoint(SkiaSharp.SKPoint)" /> to take the translation into consideration.</remarks>
		public readonly SKPoint MapVector (float x, float y)
		{
			SKPoint result;
			fixed (SKMatrix* t = &this) {
				SkiaApi.sk_matrix_map_vector (t, x, y, &result);
			}
			return result;
		}

		/// <summary>Transforms vectors using this matrix, ignoring translation.</summary>
		/// <param name="result">The span where the transformed vectors will be stored.</param>
		/// <param name="vectors">The span of vectors to transform.</param>
		/// <remarks>Mapping vectors ignores the translation component in the matrix. Use <see cref="M:SkiaSharp.SKMatrix.MapPoints(System.Span{SkiaSharp.SKPoint},System.ReadOnlySpan{SkiaSharp.SKPoint})" /> to take the translation into consideration.</remarks>
		public readonly void MapVectors (Span<SKPoint> result, ReadOnlySpan<SKPoint> vectors)
		{
			if (result.Length != vectors.Length)
				throw new ArgumentException ("Buffers must be the same size.");

			fixed (SKMatrix* t = &this)
			fixed (SKPoint* rp = result)
			fixed (SKPoint* pp = vectors) {
				SkiaApi.sk_matrix_map_vectors (t, rp, pp, result.Length);
			}
		}

		/// <summary>Applies the matrix to the array of vectors and returns the mapped results.</summary>
		/// <param name="result">The array where the mapped results will be stored (needs to have the same number of elements of the <paramref name="vectors" /> array).</param>
		/// <param name="vectors">The array of vectors to map.</param>
		/// <remarks>Mapping vectors ignores the translation component in the matrix. Use <see cref="M:SkiaSharp.SKMatrix.MapPoints(SkiaSharp.SKPoint[],SkiaSharp.SKPoint[])" /> to take the translation into consideration.</remarks>
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

		/// <summary>Applies the matrix to the array of vectors, ignoring translation, and returns the mapped results.</summary>
		/// <param name="vectors">The array of vectors to map.</param>
		/// <returns>Returns the new array allocated with the mapped results.</returns>
		/// <remarks>Mapping vectors ignores the translation component in the matrix. Use <see cref="M:SkiaSharp.SKMatrix.MapPoints(SkiaSharp.SKPoint[])" /> to take the translation into consideration.</remarks>
		public readonly SKPoint[] MapVectors (SKPoint[] vectors)
		{
			if (vectors == null)
				throw new ArgumentNullException (nameof (vectors));

			var res = new SKPoint[vectors.Length];
			MapVectors (res, vectors);
			return res;
		}

		// MapRadius

		/// <summary>Calculates the mean radius of a circle after it has been mapped by this matrix.</summary>
		/// <param name="radius">The radius to map.</param>
		/// <returns>Returns the mean radius.</returns>
		/// <remarks />
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
