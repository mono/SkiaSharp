using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	// Verifies that the managed SKMatrix math (which replaced the native interop
	// calls) is bit-for-bit identical to the native Skia C API across a wide
	// range of matrices, points, rectangles and edge cases.
	public unsafe class SKMatrixManagedTest : SKTest
	{
		// ===== native helpers (call the C API directly) =====

		private static bool NativeTryInvert (SKMatrix m, out SKMatrix inverse)
		{
			SKMatrix result;
			var ok = SkiaApi.sk_matrix_try_invert (&m, &result);
			inverse = result;
			return ok;
		}

		private static bool NativeIsInvertible (SKMatrix m) =>
			SkiaApi.sk_matrix_try_invert (&m, null);

		private static SKMatrix NativeConcat (SKMatrix a, SKMatrix b)
		{
			SKMatrix result = default;
			SkiaApi.sk_matrix_concat (&result, &a, &b);
			return result;
		}

		private static SKMatrix NativePreConcat (SKMatrix target, SKMatrix m)
		{
			SkiaApi.sk_matrix_pre_concat (&target, &m);
			return target;
		}

		private static SKMatrix NativePostConcat (SKMatrix target, SKMatrix m)
		{
			SkiaApi.sk_matrix_post_concat (&target, &m);
			return target;
		}

		private static SKPoint NativeMapPoint (SKMatrix m, float x, float y)
		{
			SKPoint result;
			SkiaApi.sk_matrix_map_xy (&m, x, y, &result);
			return result;
		}

		private static SKPoint NativeMapVector (SKMatrix m, float x, float y)
		{
			SKPoint result;
			SkiaApi.sk_matrix_map_vector (&m, x, y, &result);
			return result;
		}

		private static SKPoint[] NativeMapPoints (SKMatrix m, SKPoint[] src)
		{
			var dst = new SKPoint[src.Length];
			fixed (SKPoint* s = src)
			fixed (SKPoint* d = dst)
				SkiaApi.sk_matrix_map_points (&m, d, s, src.Length);
			return dst;
		}

		private static SKPoint[] NativeMapVectors (SKMatrix m, SKPoint[] src)
		{
			var dst = new SKPoint[src.Length];
			fixed (SKPoint* s = src)
			fixed (SKPoint* d = dst)
				SkiaApi.sk_matrix_map_vectors (&m, d, s, src.Length);
			return dst;
		}

		private static SKPoint[] NativeMapPointsInPlace (SKMatrix m, SKPoint[] src)
		{
			var buf = (SKPoint[])src.Clone ();
			fixed (SKPoint* p = buf)
				SkiaApi.sk_matrix_map_points (&m, p, p, buf.Length);
			return buf;
		}

		private static SKPoint[] NativeMapVectorsInPlace (SKMatrix m, SKPoint[] src)
		{
			var buf = (SKPoint[])src.Clone ();
			fixed (SKPoint* p = buf)
				SkiaApi.sk_matrix_map_vectors (&m, p, p, buf.Length);
			return buf;
		}

		private static SKRect NativeMapRect (SKMatrix m, SKRect src)
		{
			SKRect dst;
			SkiaApi.sk_matrix_map_rect (&m, &dst, &src);
			return dst;
		}

		private static float NativeMapRadius (SKMatrix m, float radius) =>
			SkiaApi.sk_matrix_map_radius (&m, radius);

		// ===== bit-exact comparison helpers =====

		private static int FloatBits (float f) =>
			*(int*)&f;

		private static bool BitEqual (float a, float b) =>
			(float.IsNaN (a) && float.IsNaN (b)) ||
			FloatBits (a) == FloatBits (b);

		private static void AssertFloat (float native, float managed, string what)
		{
			if (!BitEqual (native, managed)) {
				var nb = FloatBits (native);
				var mb = FloatBits (managed);
				Assert.Fail ($"{what}: native={native} (0x{nb:X8}) != managed={managed} (0x{mb:X8})");
			}
		}

		private static void AssertMatrix (SKMatrix native, SKMatrix managed, string what)
		{
			var n = native.Values;
			var m = managed.Values;
			for (var i = 0; i < 9; i++)
				AssertFloat (n[i], m[i], $"{what}[{i}]");
		}

		private static void AssertPoint (SKPoint native, SKPoint managed, string what)
		{
			AssertFloat (native.X, managed.X, $"{what}.X");
			AssertFloat (native.Y, managed.Y, $"{what}.Y");
		}

		private static void AssertRect (SKRect native, SKRect managed, string what)
		{
			AssertFloat (native.Left, managed.Left, $"{what}.Left");
			AssertFloat (native.Top, managed.Top, $"{what}.Top");
			AssertFloat (native.Right, managed.Right, $"{what}.Right");
			AssertFloat (native.Bottom, managed.Bottom, $"{what}.Bottom");
		}

		// ===== test data =====

		private static IEnumerable<SKMatrix> GetTestMatrices ()
		{
			// identity, translate, scale, scale + translate
			yield return SKMatrix.Identity;
			yield return SKMatrix.CreateTranslation (10, 20);
			yield return SKMatrix.CreateTranslation (-3.5f, 7.25f);
			yield return SKMatrix.CreateScale (2, 3);
			yield return SKMatrix.CreateScale (-2, 0.5f);
			yield return SKMatrix.CreateScale (2, 3, 4, 5);
			yield return SKMatrix.CreateScaleTranslation (1.5f, -2.5f, 10, -20);

			// rotation / skew / affine
			yield return SKMatrix.CreateRotationDegrees (30);
			yield return SKMatrix.CreateRotationDegrees (90);
			yield return SKMatrix.CreateRotationDegrees (-123.4f, 5, 6);
			yield return SKMatrix.CreateSkew (0.3f, -0.7f);
			yield return new SKMatrix (1, 0.5f, 10, -0.25f, 2, -5, 0, 0, 1);

			// degenerate (non-invertible)
			yield return SKMatrix.CreateScale (0, 1);
			yield return SKMatrix.CreateScale (1, 0);
			yield return new SKMatrix (0, 0, 0, 0, 0, 0, 0, 0, 1);
			yield return new SKMatrix (1, 1, 0, 1, 1, 0, 0, 0, 1); // collinear rows

			// perspective
			yield return new SKMatrix (1, 0, 0, 0, 1, 0, 0.001f, 0.002f, 1);
			yield return new SKMatrix (2, 0.3f, 10, -0.4f, 1.5f, -20, 0.0005f, -0.0011f, 0.9f);
			yield return new SKMatrix (1, 0, 5, 0, 1, 7, 0, 0, 2);

			// negative zero translation / scale (sign edge cases)
			yield return new SKMatrix (1, 0, -0.0f, 0, 1, -0.0f, 0, 0, 1);
			yield return new SKMatrix (-0.0f, 0, 3, 0, -0.0f, 4, 0, 0, 1);

			// non-finite members
			yield return new SKMatrix (float.NaN, 0, 0, 0, 1, 0, 0, 0, 1);
			yield return new SKMatrix (1, 0, float.PositiveInfinity, 0, 1, 0, 0, 0, 1);
			yield return new SKMatrix (float.MaxValue, 0, 0, 0, float.MaxValue, 0, 0, 0, 1);
			yield return new SKMatrix (float.Epsilon, 0, 0, 0, float.Epsilon, 0, 0, 0, 1);

			// scale-by-infinity: exercises sort_as_rect NaN semantics (Inf*0 = NaN)
			yield return SKMatrix.CreateScale (float.PositiveInfinity, 2);
			yield return SKMatrix.CreateScale (2, float.PositiveInfinity);
			yield return SKMatrix.CreateScale (float.NegativeInfinity, 3);
			yield return SKMatrix.CreateScale (float.PositiveInfinity, float.NegativeInfinity);
			yield return new SKMatrix (float.PositiveInfinity, 0, 5, 0, float.PositiveInfinity, 7, 0, 0, 1);
			// affine with an infinite skew lane
			yield return new SKMatrix (1, float.PositiveInfinity, 0, 0.5f, 2, 0, 0, 0, 1);

			// deterministic random matrices
			var rnd = new Random (12345);
			for (var i = 0; i < 200; i++)
				yield return RandomMatrix (rnd);
		}

		private static SKMatrix RandomMatrix (Random rnd)
		{
			float F () => (float)((rnd.NextDouble () - 0.5) * 200.0);
			float P () => (float)((rnd.NextDouble () - 0.5) * 0.01);

			// roughly 1 in 4 matrices get a perspective row
			var persp = rnd.Next (4) == 0;
			return new SKMatrix (
				F (), F (), F (),
				F (), F (), F (),
				persp ? P () : 0, persp ? P () : 0, persp ? (float)(rnd.NextDouble () + 0.5) : 1);
		}

		private static readonly SKPoint[] TestPoints =
		{
			new SKPoint (0, 0),
			new SKPoint (1, 0),
			new SKPoint (0, 1),
			new SKPoint (-3.5f, 7.25f),
			new SKPoint (100, -200),
			new SKPoint (0.001f, -0.002f),
			new SKPoint (-0.0f, 0.0f),
			new SKPoint (1e20f, -1e20f),
			new SKPoint (12345.678f, -98765.43f),
			new SKPoint (float.PositiveInfinity, 1),
			new SKPoint (2, float.NegativeInfinity),
			new SKPoint (float.NaN, 3),
		};

		private static readonly SKRect[] TestRects =
		{
			new SKRect (0, 0, 10, 20),
			new SKRect (-5, -5, 5, 5),
			new SKRect (10, 20, 1, 2),       // unsorted
			new SKRect (-100.5f, 33.3f, 200.25f, -44.4f),
			new SKRect (0, 0, 0, 0),
			new SKRect (1.5f, -2.5f, 1.5f, -2.5f),
			new SKRect (0, 0, float.PositiveInfinity, float.PositiveInfinity),
			new SKRect (float.NegativeInfinity, -1, 1, float.PositiveInfinity),
			new SKRect (float.NaN, 0, 10, 20),
			new SKRect (-10, float.NaN, 10, 20),
		};

		// ===== tests =====

		[Fact]
		public void InvertMatchesNative ()
		{
			foreach (var m in GetTestMatrices ()) {
				var nativeOk = NativeTryInvert (m, out var nativeInv);
				var managedOk = m.TryInvert (out var managedInv);

				Assert.Equal (nativeOk, managedOk);
				AssertMatrix (nativeInv, managedInv, $"TryInvert({Describe (m)})");

				Assert.Equal (NativeIsInvertible (m), m.IsInvertible);
			}
		}

		[Fact]
		public void ConcatMatchesNative ()
		{
			var matrices = new List<SKMatrix> (GetTestMatrices ());
			var rnd = new Random (999);

			foreach (var a in matrices) {
				// pair each matrix with a handful of others
				for (var k = 0; k < 5; k++) {
					var b = matrices[rnd.Next (matrices.Count)];

					AssertMatrix (NativeConcat (a, b), SKMatrix.Concat (a, b), "Concat");
					AssertMatrix (NativePreConcat (a, b), a.PreConcat (b), "PreConcat");
					AssertMatrix (NativePostConcat (a, b), a.PostConcat (b), "PostConcat");

					SKMatrix refTarget = default;
					SKMatrix.Concat (ref refTarget, a, b);
					AssertMatrix (NativeConcat (a, b), refTarget, "Concat(ref)");
				}
			}
		}

		[Fact]
		public void MapPointMatchesNative ()
		{
			foreach (var m in GetTestMatrices ()) {
				foreach (var p in TestPoints) {
					AssertPoint (NativeMapPoint (m, p.X, p.Y), m.MapPoint (p.X, p.Y), "MapPoint");
					AssertPoint (NativeMapPoint (m, p.X, p.Y), m.MapPoint (p), "MapPoint(SKPoint)");
				}
			}
		}

		[Fact]
		public void MapVectorMatchesNative ()
		{
			foreach (var m in GetTestMatrices ()) {
				foreach (var p in TestPoints) {
					AssertPoint (NativeMapVector (m, p.X, p.Y), m.MapVector (p.X, p.Y), "MapVector");
					AssertPoint (NativeMapVector (m, p.X, p.Y), m.MapVector (p), "MapVector(SKPoint)");
				}
			}
		}

		[Fact]
		public void MapPointsBatchMatchesNative ()
		{
			// vary the count so the SIMD pair loop and the odd-element scalar tail are both hit;
			// 6 and 10 additionally exercise the single-pair lead-in together with the unrolled
			// 4-point loop in the same call, and 11 adds an odd tail on top of the unrolled loop
			foreach (var count in new[] { 0, 1, 2, 3, 5, 6, 8, 10, 11, TestPoints.Length }) {
				var pts = TestPoints.Take (count).ToArray ();
				foreach (var m in GetTestMatrices ()) {
					var native = NativeMapPoints (m, pts);
					var managed = m.MapPoints (pts);
					Assert.Equal (native.Length, managed.Length);
					for (var i = 0; i < native.Length; i++)
						AssertPoint (native[i], managed[i], $"MapPoints(n={count})[{i}]");
				}
			}
		}

		[Fact]
		public void MapVectorsBatchMatchesNative ()
		{
			foreach (var count in new[] { 0, 1, 2, 3, 5, 6, 8, 10, 11, TestPoints.Length }) {
				var pts = TestPoints.Take (count).ToArray ();
				foreach (var m in GetTestMatrices ()) {
					if (count == 0) {
						// native helper would marshal an empty array; skip the trivial empty case
						Assert.Empty (m.MapVectors (pts));
						continue;
					}
					var native = NativeMapVectors (m, pts);
					var managed = m.MapVectors (pts);
					Assert.Equal (native.Length, managed.Length);
					for (var i = 0; i < native.Length; i++)
						AssertPoint (native[i], managed[i], $"MapVectors(n={count})[{i}]");
				}
			}
		}

		[Fact]
		public void MapPointsInPlaceMatchesNative ()
		{
			// dst == src: exercises the in-place aliasing path of the SIMD batch loops
			foreach (var count in new[] { 1, 2, 3, 5, 6, 8, 10, 11, TestPoints.Length }) {
				var pts = TestPoints.Take (count).ToArray ();
				foreach (var m in GetTestMatrices ()) {
					var native = NativeMapPointsInPlace (m, pts);
					var managed = (SKPoint[])pts.Clone ();
					m.MapPoints (managed, managed);
					for (var i = 0; i < native.Length; i++)
						AssertPoint (native[i], managed[i], $"MapPointsInPlace(n={count})[{i}]");
				}
			}
		}

		[Fact]
		public void MapVectorsInPlaceMatchesNative ()
		{
			foreach (var count in new[] { 1, 2, 3, 5, 6, 8, 10, 11, TestPoints.Length }) {
				var pts = TestPoints.Take (count).ToArray ();
				foreach (var m in GetTestMatrices ()) {
					var native = NativeMapVectorsInPlace (m, pts);
					var managed = (SKPoint[])pts.Clone ();
					m.MapVectors (managed, managed);
					for (var i = 0; i < native.Length; i++)
						AssertPoint (native[i], managed[i], $"MapVectorsInPlace(n={count})[{i}]");
				}
			}
		}

		[Fact]
		public void MapVectorsOverlappingSpansMatchNative ()
		{
			// dst overlaps src and is shifted one element ahead in a shared buffer.
			// SkMatrix::mapVectors walks the perspective case back-to-front so this
			// aliased-but-shifted layout stays correct; the managed path must mirror
			// that direction to produce the same result as native for every matrix.
			foreach (var count in new[] { 2, 3, 5, 8, 11 }) {
				var seed = TestPoints.Take (count).ToArray ();
				foreach (var m in GetTestMatrices ()) {
					var nativeBuf = new SKPoint[count + 1];
					Array.Copy (seed, 0, nativeBuf, 0, count);
					fixed (SKPoint* p = nativeBuf)
						SkiaApi.sk_matrix_map_vectors (&m, p + 1, p, count);

					var managedBuf = new SKPoint[count + 1];
					Array.Copy (seed, 0, managedBuf, 0, count);
					m.MapVectors (managedBuf.AsSpan (1, count), managedBuf.AsSpan (0, count));

					for (var i = 0; i < count + 1; i++)
						AssertPoint (nativeBuf[i], managedBuf[i], $"MapVectorsOverlap(n={count})[{i}]");
				}
			}
		}

		[Fact]
		public void MapRectMatchesNative ()
		{
			foreach (var m in GetTestMatrices ()) {
				foreach (var r in TestRects)
					AssertRect (NativeMapRect (m, r), m.MapRect (r), $"MapRect({Describe (m)})");
			}
		}

		[Fact]
		public void MapRadiusMatchesNative ()
		{
			var radii = new[] { 0f, 1f, 10f, -5f, 0.001f, 1e10f, 12345.6f };
			foreach (var m in GetTestMatrices ()) {
				foreach (var radius in radii)
					AssertFloat (NativeMapRadius (m, radius), m.MapRadius (radius), "MapRadius");
			}
		}

		private static string Describe (SKMatrix m)
		{
			var v = m.Values;
			return $"[{v[0]},{v[1]},{v[2]};{v[3]},{v[4]},{v[5]};{v[6]},{v[7]},{v[8]}]";
		}
	}
}
