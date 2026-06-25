#nullable disable

using System;
using System.Numerics;
#if NET8_0_OR_GREATER
using System.Runtime.Intrinsics;
#endif

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

		public readonly bool IsInvertible =>
			TryInvertInternal (out _);

		public readonly bool TryInvert (out SKMatrix inverse)
		{
			if (TryInvertInternal (out inverse))
				return true;

			// Match the native shim: on failure the result is left as the
			// default-constructed (identity) matrix.
			inverse = Identity;
			return false;
		}

		public readonly SKMatrix Invert ()
		{
			if (TryInvert (out var matrix))
				return matrix;

			return Empty;
		}

		// *Concat

		public static SKMatrix Concat (SKMatrix first, SKMatrix second) =>
			SetConcat (first, second);

		public readonly SKMatrix PreConcat (SKMatrix matrix) =>
			matrix.GetMatrixType () == TypeMaskIdentity ? this : SetConcat (this, matrix);

		public readonly SKMatrix PostConcat (SKMatrix matrix) =>
			matrix.GetMatrixType () == TypeMaskIdentity ? this : SetConcat (matrix, this);

		public static void Concat (ref SKMatrix target, SKMatrix first, SKMatrix second) =>
			target = SetConcat (first, second);

		// MapRect

		public readonly SKRect MapRect (SKRect source)
		{
			var type = GetMatrixType ();

			// identity or translate
			if (type <= TypeMaskTranslate) {
				return SortAsRect (
					source.Left + transX, source.Top + transY,
					source.Right + transX, source.Bottom + transY);
			}

			// scale and/or translate
			if ((type & (TypeMaskAffine | TypeMaskPerspective)) == 0) {
				return SortAsRect (
					source.Left * scaleX + transX, source.Top * scaleY + transY,
					source.Right * scaleX + transX, source.Bottom * scaleY + transY);
			}

			// perspective: fall back to the native path-clipping implementation
			if ((type & TypeMaskPerspective) != 0) {
				SKRect dest;
				fixed (SKMatrix* m = &this) {
					SkiaApi.sk_matrix_map_rect (m, &dest, &source);
				}
				return dest;
			}

			// affine
			return MapRectAffine (source);
		}

		// MapPoints

		public readonly SKPoint MapPoint (SKPoint point) =>
			MapPoint (point.X, point.Y);

		public readonly SKPoint MapPoint (float x, float y)
		{
			// Mirrors SkMatrix::mapPoint: perspective uses the perspective proc,
			// everything else uses the inlined affine math (mapPointAffine) so a
			// pure scale/translate still maps through the same expression.
			if (HasPerspective)
				return MapPerspective (x, y);

			return new SKPoint ((x * scaleX + y * skewX) + transX, (x * skewY + y * scaleY) + transY);
		}

		public readonly void MapPoints (Span<SKPoint> result, ReadOnlySpan<SKPoint> points)
		{
			if (result.Length != points.Length)
				throw new ArgumentException ("Buffers must be the same size.");

			MapPointsInternal (result, points);
		}

		public readonly void MapPoints (SKPoint[] result, SKPoint[] points)
		{
			if (result == null)
				throw new ArgumentNullException (nameof (result));
			if (points == null)
				throw new ArgumentNullException (nameof (points));
			if (result.Length != points.Length)
				throw new ArgumentException ("Buffers must be the same size.");

			MapPointsInternal (result, points);
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
			if (HasPerspective) {
				var v = MapPerspective (x, y);
				var o = MapPerspective (0, 0);
				return new SKPoint (v.X - o.X, v.Y - o.Y);
			}

			// Drop translation, then map as a point through the proc-table math
			// (matches SkMatrix::mapVectors, which routes through mapPoints).
			var tmp = this;
			tmp.transX = 0;
			tmp.transY = 0;
			return tmp.MapPointByType (x, y);
		}

		public readonly void MapVectors (Span<SKPoint> result, ReadOnlySpan<SKPoint> vectors)
		{
			if (result.Length != vectors.Length)
				throw new ArgumentException ("Buffers must be the same size.");

			MapVectorsInternal (result, vectors);
		}

		public readonly void MapVectors (SKPoint[] result, SKPoint[] vectors)
		{
			if (result == null)
				throw new ArgumentNullException (nameof (result));
			if (vectors == null)
				throw new ArgumentNullException (nameof (vectors));
			if (result.Length != vectors.Length)
				throw new ArgumentException ("Buffers must be the same size.");

			MapVectorsInternal (result, vectors);
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
			var v0 = MapVector (radius, 0);
			var v1 = MapVector (0, radius);

			var d0 = PointLength (v0.X, v0.Y);
			var d1 = PointLength (v1.X, v1.Y);

			// geometric mean
			return (float)Math.Sqrt (d0 * d1);
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

		// Managed re-implementation of the SkMatrix math (kept bit-for-bit
		// compatible with the native C API so there is no behavioural change).

		// SkMatrix::TypeMask values.
		private const int TypeMaskIdentity = 0;
		private const int TypeMaskTranslate = 0x01;
		private const int TypeMaskScale = 0x02;
		private const int TypeMaskAffine = 0x04;
		private const int TypeMaskPerspective = 0x08;
		private const int TypeMaskRectStaysRect = 0x10;

		// SK_ScalarNearlyZero == SK_Scalar1 / (1 << 12)
		private const float ScalarNearlyZero = 1.0f / (1 << 12);

		private readonly bool HasPerspective =>
			persp0 != 0 || persp1 != 0 || persp2 != 1;

		// Mirrors SkMatrix::computeTypeMask (including the kRectStaysRect bit).
		private readonly int GetTypeMask ()
		{
			if (persp0 != 0 || persp1 != 0 || persp2 != 1) {
				// Perspective implies every other transform flag (conservative).
				return TypeMaskTranslate | TypeMaskScale | TypeMaskAffine | TypeMaskPerspective;
			}

			var mask = 0;

			if (transX != 0 || transY != 0)
				mask |= TypeMaskTranslate;

			if (skewX != 0 || skewY != 0) {
				// Skew always implies scale + affine (matches Skia's conservative rule).
				mask |= TypeMaskAffine | TypeMaskScale;

				// rectStaysRect: primary diagonal all zero and secondary diagonal all non-zero.
				if (scaleX == 0 && scaleY == 0 && skewX != 0 && skewY != 0)
					mask |= TypeMaskRectStaysRect;
			} else {
				if (scaleX != 1 || scaleY != 1)
					mask |= TypeMaskScale;

				if (scaleX != 0 && scaleY != 0)
					mask |= TypeMaskRectStaysRect;
			}

			return mask;
		}

		private readonly int GetMatrixType () =>
			GetTypeMask () & 0x0F;

		// Invert

		private readonly bool TryInvertInternal (out SKMatrix inverse)
		{
			var type = GetMatrixType ();

			if (type == TypeMaskIdentity) {
				inverse = this;
				return true;
			}

			// Scale and/or translation only.
			if ((type & ~(TypeMaskScale | TypeMaskTranslate)) == 0) {
				if ((type & TypeMaskScale) != 0) {
					var invSX = 1.0f / scaleX;
					var invSY = 1.0f / scaleY;
					// Denormalized (non-zero) scale factors overflow when inverted.
					if (!IsFinite (invSX, invSY)) {
						inverse = default;
						return false;
					}

					var invTX = -transX * invSX;
					var invTY = -transY * invSY;
					if (!IsFinite (invTX, invTY)) {
						inverse = default;
						return false;
					}

					inverse = new SKMatrix {
						scaleX = invSX,
						skewX = 0,
						transX = invTX,
						skewY = 0,
						scaleY = invSY,
						transY = invTY,
						persp0 = 0,
						persp1 = 0,
						persp2 = 1,
					};
					return true;
				}

				// Translate only.
				if (!IsFinite (transX, transY)) {
					inverse = default;
					return false;
				}

				inverse = CreateTranslation (-transX, -transY);
				return true;
			}

			var isPersp = (type & TypeMaskPerspective) != 0;
			var invDet = InverseDeterminant (isPersp);
			if (invDet == 0) {
				inverse = default;
				return false;
			}

			inverse = ComputeInverse (invDet, isPersp);
			if (!inverse.IsFiniteInternal ()) {
				inverse = default;
				return false;
			}

			return true;
		}

		private readonly double Determinant (bool isPerspective)
		{
			if (isPerspective) {
				return scaleX * DCross (scaleY, persp2, transY, persp1)
					+ skewX * DCross (transY, persp0, skewY, persp2)
					+ transX * DCross (skewY, persp1, scaleY, persp0);
			}

			return DCross (scaleX, scaleY, skewX, skewY);
		}

		private readonly double InverseDeterminant (bool isPerspective)
		{
			var det = Determinant (isPerspective);

			// Compare against the cube of the nearly-zero constant since the
			// determinant scales with the cube of the matrix members.
			var tolerance = ScalarNearlyZero * ScalarNearlyZero * ScalarNearlyZero;
			if (Math.Abs ((float)det) <= tolerance)
				return 0;

			return 1.0 / det;
		}

		private readonly SKMatrix ComputeInverse (double invDet, bool isPersp)
		{
			SKMatrix inv;
			if (isPersp) {
				inv.scaleX = ScrossDscale (scaleY, persp2, transY, persp1, invDet);
				inv.skewX = ScrossDscale (transX, persp1, skewX, persp2, invDet);
				inv.transX = ScrossDscale (skewX, transY, transX, scaleY, invDet);

				inv.skewY = ScrossDscale (transY, persp0, skewY, persp2, invDet);
				inv.scaleY = ScrossDscale (scaleX, persp2, transX, persp0, invDet);
				inv.transY = ScrossDscale (transX, skewY, scaleX, transY, invDet);

				inv.persp0 = ScrossDscale (skewY, persp1, scaleY, persp0, invDet);
				inv.persp1 = ScrossDscale (skewX, persp0, scaleX, persp1, invDet);
				inv.persp2 = ScrossDscale (scaleX, scaleY, skewX, skewY, invDet);
			} else {
				inv.scaleX = (float)(scaleY * invDet);
				inv.skewX = (float)(-skewX * invDet);
				inv.transX = DcrossDscale (skewX, transY, scaleY, transX, invDet);

				inv.skewY = (float)(-skewY * invDet);
				inv.scaleY = (float)(scaleX * invDet);
				inv.transY = DcrossDscale (skewY, transX, scaleX, transY, invDet);

				inv.persp0 = 0;
				inv.persp1 = 0;
				inv.persp2 = 1;
			}
			return inv;
		}

		// *Concat

		private static SKMatrix SetConcat (SKMatrix a, SKMatrix b)
		{
			var aType = a.GetMatrixType ();
			var bType = b.GetMatrixType ();

			if (aType == TypeMaskIdentity)
				return b;
			if (bType == TypeMaskIdentity)
				return a;

			// Scale and/or translation only.
			if (((aType | bType) & (TypeMaskAffine | TypeMaskPerspective)) == 0) {
				return new SKMatrix {
					scaleX = a.scaleX * b.scaleX,
					skewX = 0,
					transX = a.scaleX * b.transX + a.transX,
					skewY = 0,
					scaleY = a.scaleY * b.scaleY,
					transY = a.scaleY * b.transY + a.transY,
					persp0 = 0,
					persp1 = 0,
					persp2 = 1,
				};
			}

			SKMatrix tmp;
			if (((aType | bType) & TypeMaskPerspective) != 0) {
				tmp.scaleX = RowCol3 (a.scaleX, a.skewX, a.transX, b.scaleX, b.skewY, b.persp0);
				tmp.skewX = RowCol3 (a.scaleX, a.skewX, a.transX, b.skewX, b.scaleY, b.persp1);
				tmp.transX = RowCol3 (a.scaleX, a.skewX, a.transX, b.transX, b.transY, b.persp2);
				tmp.skewY = RowCol3 (a.skewY, a.scaleY, a.transY, b.scaleX, b.skewY, b.persp0);
				tmp.scaleY = RowCol3 (a.skewY, a.scaleY, a.transY, b.skewX, b.scaleY, b.persp1);
				tmp.transY = RowCol3 (a.skewY, a.scaleY, a.transY, b.transX, b.transY, b.persp2);
				tmp.persp0 = RowCol3 (a.persp0, a.persp1, a.persp2, b.scaleX, b.skewY, b.persp0);
				tmp.persp1 = RowCol3 (a.persp0, a.persp1, a.persp2, b.skewX, b.scaleY, b.persp1);
				tmp.persp2 = RowCol3 (a.persp0, a.persp1, a.persp2, b.transX, b.transY, b.persp2);
			} else {
				tmp.scaleX = MulAddMul (a.scaleX, b.scaleX, a.skewX, b.skewY);
				tmp.skewX = MulAddMul (a.scaleX, b.skewX, a.skewX, b.scaleY);
				tmp.transX = MulAddMul (a.scaleX, b.transX, a.skewX, b.transY) + a.transX;
				tmp.skewY = MulAddMul (a.skewY, b.scaleX, a.scaleY, b.skewY);
				tmp.scaleY = MulAddMul (a.skewY, b.skewX, a.scaleY, b.scaleY);
				tmp.transY = MulAddMul (a.skewY, b.transX, a.scaleY, b.transY) + a.transY;
				tmp.persp0 = 0;
				tmp.persp1 = 0;
				tmp.persp2 = 1;
			}
			return tmp;
		}

		// Map points / vectors

		private readonly SKPoint MapPerspective (float x, float y)
		{
			var px = (x * scaleX + y * skewX) + transX;
			var py = (x * skewY + y * scaleY) + transY;
			var pz = (x * persp0 + y * persp1) + persp2;
			if (pz != 0)
				pz = 1.0f / pz;
			return new SKPoint (px * pz, py * pz);
		}

		// Single-point map that mirrors getMapPtsProc()/the proc table (used by
		// mapVectors). Unlike mapPoint, a pure scale/translate avoids the y*kx term.
		private readonly SKPoint MapPointByType (float x, float y)
		{
			var type = GetMatrixType ();

			if ((type & TypeMaskPerspective) != 0)
				return MapPerspective (x, y);
			if ((type & TypeMaskAffine) != 0)
				return new SKPoint ((x * scaleX + y * skewX) + transX, (x * skewY + y * scaleY) + transY);
			if ((type & TypeMaskScale) != 0)
				return new SKPoint (x * scaleX + transX, y * scaleY + transY);
			if ((type & TypeMaskTranslate) != 0)
				return new SKPoint (x + transX, y + transY);
			return new SKPoint (x, y);
		}

		private readonly void MapPointsInternal (Span<SKPoint> dst, ReadOnlySpan<SKPoint> src)
		{
			var type = GetMatrixType ();
			var count = src.Length;
			if (count == 0)
				return;

			if ((type & TypeMaskPerspective) != 0) {
				for (var i = 0; i < count; i++) {
					var p = src[i];
					dst[i] = MapPerspective (p.X, p.Y);
				}
			} else if ((type & TypeMaskAffine) != 0) {
				MapAffineBatch (dst, src);
			} else if ((type & TypeMaskScale) != 0) {
				MapScaleBatch (dst, src);
			} else if ((type & TypeMaskTranslate) != 0) {
				MapTranslateBatch (dst, src);
			} else {
				src.CopyTo (dst);
			}
		}

		// SIMD batch map procs. Each Vector4 holds two points (x0, y0, x1, y1)
		// which mirrors Skia's skvx::float4 procs (Trans_pts/Scale_pts/Affine_vpts)
		// exactly, so the result is bit-for-bit identical to the native path. We
		// process two points per iteration and handle a trailing odd point with
		// the scalar formula (IEEE addition is commutative, so it is bit-identical).

		// Swaps the X/Y lanes of each point pair: (x0,y0,x1,y1) -> (y0,x0,y1,x1).
#if NET8_0_OR_GREATER
		[System.Runtime.CompilerServices.MethodImpl (System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private static Vector4 SwapXY (Vector4 v) =>
			Vector128.Shuffle (v.AsVector128 (), Vector128.Create (1, 0, 3, 2)).AsVector4 ();
#else
		private static Vector4 SwapXY (Vector4 v) =>
			new Vector4 (v.Y, v.X, v.W, v.Z);
#endif

		private readonly void MapTranslateBatch (Span<SKPoint> dst, ReadOnlySpan<SKPoint> src)
		{
			var count = src.Length;
			float tx = transX, ty = transY;
			var trans = new Vector4 (tx, ty, tx, ty);

			// Mirrors Skia's Trans_pts: a scalar lead-in for the odd point, then
			// one pair, then the main loop of two float4 (four points) per iteration.
			fixed (SKPoint* sp = src)
			fixed (SKPoint* dp = dst) {
				var s = (float*)sp;
				var d = (float*)dp;
				var n = count;
				if ((n & 1) != 0) {
					d[0] = s[0] + tx;
					d[1] = s[1] + ty;
					s += 2; d += 2;
				}
				n >>= 1;
				if ((n & 1) != 0) {
					*(Vector4*)d = *(Vector4*)s + trans;
					s += 4; d += 4;
				}
				n >>= 1;
				for (var i = 0; i < n; i++) {
					*(Vector4*)(d + 0) = *(Vector4*)(s + 0) + trans;
					*(Vector4*)(d + 4) = *(Vector4*)(s + 4) + trans;
					s += 8; d += 8;
				}
			}
		}

		private readonly void MapScaleBatch (Span<SKPoint> dst, ReadOnlySpan<SKPoint> src)
		{
			var count = src.Length;
			float sx = scaleX, sy = scaleY, tx = transX, ty = transY;
			var scale = new Vector4 (sx, sy, sx, sy);
			var trans = new Vector4 (tx, ty, tx, ty);

			// Mirrors Skia's Scale_pts: scalar lead-in, then one pair, then four
			// points (two float4) per iteration so the multiplies pipeline.
			fixed (SKPoint* sp = src)
			fixed (SKPoint* dp = dst) {
				var s = (float*)sp;
				var d = (float*)dp;
				var n = count;
				if ((n & 1) != 0) {
					d[0] = s[0] * sx + tx;
					d[1] = s[1] * sy + ty;
					s += 2; d += 2;
				}
				n >>= 1;
				if ((n & 1) != 0) {
					*(Vector4*)d = *(Vector4*)s * scale + trans;
					s += 4; d += 4;
				}
				n >>= 1;
				for (var i = 0; i < n; i++) {
					*(Vector4*)(d + 0) = *(Vector4*)(s + 0) * scale + trans;
					*(Vector4*)(d + 4) = *(Vector4*)(s + 4) * scale + trans;
					s += 8; d += 8;
				}
			}
		}

		private readonly void MapAffineBatch (Span<SKPoint> dst, ReadOnlySpan<SKPoint> src)
		{
			var count = src.Length;
			float sx = scaleX, sy = scaleY, kx = skewX, ky = skewY, tx = transX, ty = transY;
			var scale = new Vector4 (sx, sy, sx, sy);
			var skew = new Vector4 (kx, ky, kx, ky);
			var trans = new Vector4 (tx, ty, tx, ty);
			var pairs = count >> 1;

			fixed (SKPoint* sp = src)
			fixed (SKPoint* dp = dst) {
				var s = (float*)sp;
				var d = (float*)dp;
				for (var i = 0; i < pairs; i++) {
					var o = i << 2;
					var v = *(Vector4*)(s + o);
					*(Vector4*)(d + o) = v * scale + SwapXY (v) * skew + trans;
				}
			}

			if ((count & 1) != 0) {
				var p = src[count - 1];
				dst[count - 1] = new SKPoint ((p.X * sx + p.Y * kx) + tx, (p.X * ky + p.Y * sy) + ty);
			}
		}

		private readonly void MapVectorsInternal (Span<SKPoint> dst, ReadOnlySpan<SKPoint> src)
		{
			if (HasPerspective) {
				var origin = MapPerspective (0, 0);
				for (var i = 0; i < src.Length; i++) {
					var v = MapPerspective (src[i].X, src[i].Y);
					dst[i] = new SKPoint (v.X - origin.X, v.Y - origin.Y);
				}
				return;
			}

			// Drop translation, then map as points.
			var tmp = this;
			tmp.transX = 0;
			tmp.transY = 0;
			tmp.MapPointsInternal (dst, src);
		}

		// MapRect

		private readonly SKRect MapRectAffine (SKRect src)
		{
			float sx = scaleX, sy = scaleY, kx = skewX, ky = skewY, tx = transX, ty = transY;
			float l = src.Left, t = src.Top, r = src.Right, b = src.Bottom;

			// Map the four corners using the affine procs.
			var x0 = (l * sx + t * kx) + tx;
			var y0 = (l * ky + t * sy) + ty;
			var x1 = (r * sx + t * kx) + tx;
			var y1 = (r * ky + t * sy) + ty;
			var x2 = (r * sx + b * kx) + tx;
			var y2 = (r * ky + b * sy) + ty;
			var x3 = (l * sx + b * kx) + tx;
			var y3 = (l * ky + b * sy) + ty;

			// SkRect::Bounds (64-bit variant): min/max plus a finiteness probe.
			var minX = x0; var minY = y0; var maxX = x0; var maxY = y0;
			minX = Math.Min (x1, minX); minY = Math.Min (y1, minY); maxX = Math.Max (x1, maxX); maxY = Math.Max (y1, maxY);
			minX = Math.Min (x2, minX); minY = Math.Min (y2, minY); maxX = Math.Max (x2, maxX); maxY = Math.Max (y2, maxY);
			minX = Math.Min (x3, minX); minY = Math.Min (y3, minY); maxX = Math.Max (x3, maxX); maxY = Math.Max (y3, maxY);

			float nx = 0, ny = 0;
			nx *= x0; ny *= y0;
			nx *= x1; ny *= y1;
			nx *= x2; ny *= y2;
			nx *= x3; ny *= y3;

			if (nx == 0 && ny == 0)
				return new SKRect (minX, minY, maxX, maxY);

			return new SKRect (float.NaN, float.NaN, float.NaN, float.NaN);
		}

		// Mirrors the skvx sort_as_rect helper. skvx::min/max use a specific
		// comparison form (NaN keeps the first operand) that differs from
		// Vector4.Min/Max, so the scalar form below is used to stay bit-exact.
		// ltrb = (l, t, r, b), rblt = (r, b, l, t); result is
		// (min[2], min[3], max[0], max[1]).
		private static SKRect SortAsRect (float l, float t, float r, float b) =>
			new SKRect (SkvxMin (r, l), SkvxMin (b, t), SkvxMax (l, r), SkvxMax (t, b));

		// skvx::min(x, y) == (y < x) ? y : x
		private static float SkvxMin (float x, float y) =>
			y < x ? y : x;

		// skvx::max(x, y) == (x < y) ? y : x
		private static float SkvxMax (float x, float y) =>
			x < y ? y : x;

		// MapRadius

		private static float PointLength (float dx, float dy)
		{
			var mag2 = dx * dx + dy * dy;
			if (IsFinite (mag2))
				return (float)Math.Sqrt (mag2);

			double xx = dx;
			double yy = dy;
			return (float)Math.Sqrt (xx * xx + yy * yy);
		}

		// Math helpers (kept bit-identical to the SkMatrix C++ helpers).

		private readonly bool IsFiniteInternal ()
		{
			var prod = scaleX - scaleX;
			prod *= skewX;
			prod *= transX;
			prod *= skewY;
			prod *= scaleY;
			prod *= transY;
			prod *= persp0;
			prod *= persp1;
			prod *= persp2;
			return !float.IsNaN (prod);
		}

		private static bool IsFinite (float x)
		{
			var prod = x - x;
			return !float.IsNaN (prod);
		}

		private static bool IsFinite (float a, float b)
		{
			var prod = (a - a) * b;
			return !float.IsNaN (prod);
		}

		private static double DCross (double a, double b, double c, double d) =>
			a * b - c * d;

		private static float ScrossDscale (float a, float b, float c, float d, double scale)
		{
			// scross is computed in float, then scaled in double.
			var scross = a * b - c * d;
			return (float)(scross * scale);
		}

		private static float DcrossDscale (double a, double b, double c, double d, double scale) =>
			(float)((a * b - c * d) * scale);

		private static float MulAddMul (float a, float b, float c, float d) =>
			(float)((double)a * b + (double)c * d);

		private static float RowCol3 (float r0, float r1, float r2, float c0, float c3, float c6) =>
			r0 * c0 + r1 * c3 + r2 * c6;
	}
}
