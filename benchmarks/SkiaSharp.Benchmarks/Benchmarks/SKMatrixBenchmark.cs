using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace SkiaSharp.Benchmarks;

// Before/after comparison for issue #2779: the "Native" methods call the Skia
// C API directly (the path the old SKMatrix used) and the "Managed" methods use
// the new managed SKMatrix math. They must produce identical results; the goal
// is to show the managed path avoids the P/Invoke transition cost.
//
// This class covers the scalar / single-value operations. The batch operations
// that take arrays are parameterised by item count in SKMatrixMapBatchBenchmark.
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public unsafe class SKMatrixBenchmark
{
	private const string SKIA = "libSkiaSharp";

	[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
	private static extern bool sk_matrix_try_invert(SKMatrix* matrix, SKMatrix* result);

	[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
	private static extern void sk_matrix_concat(SKMatrix* result, SKMatrix* first, SKMatrix* second);

	[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
	private static extern void sk_matrix_map_xy(SKMatrix* matrix, float x, float y, SKPoint* result);

	[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
	private static extern void sk_matrix_map_vector(SKMatrix* matrix, float x, float y, SKPoint* result);

	[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
	private static extern void sk_matrix_map_rect(SKMatrix* matrix, SKRect* dest, SKRect* source);

	[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
	private static extern float sk_matrix_map_radius(SKMatrix* matrix, float radius);

	private SKMatrix affine;
	private SKMatrix affine2;
	private SKMatrix translate;
	private SKMatrix perspective;
	private readonly SKRect rect = new SKRect(1, 2, 300, 400);

	[GlobalSetup]
	public void Setup()
	{
		affine = SKMatrix.CreateRotationDegrees(33);
		affine.ScaleX *= 1.5f;
		affine.TransX = 12;
		affine.TransY = -7;

		affine2 = SKMatrix.CreateScaleTranslation(0.75f, 1.25f, 5, 9);
		translate = SKMatrix.CreateTranslation(12, -7);
		perspective = new SKMatrix(1.2f, 0.1f, 10, -0.2f, 0.9f, 20, 0.001f, 0.002f, 1);
	}

	// ===== Invert =====

	[BenchmarkCategory("Invert"), Benchmark(Baseline = true)]
	public SKMatrix Invert_Native()
	{
		SKMatrix m = affine, result;
		sk_matrix_try_invert(&m, &result);
		return result;
	}

	[BenchmarkCategory("Invert"), Benchmark]
	public SKMatrix Invert_Managed()
	{
		affine.TryInvert(out var result);
		return result;
	}

	// ===== Concat =====

	[BenchmarkCategory("Concat"), Benchmark(Baseline = true)]
	public SKMatrix Concat_Native()
	{
		SKMatrix a = affine, b = affine2, result;
		sk_matrix_concat(&result, &a, &b);
		return result;
	}

	[BenchmarkCategory("Concat"), Benchmark]
	public SKMatrix Concat_Managed() =>
		SKMatrix.Concat(affine, affine2);

	// ===== MapPoint =====

	[BenchmarkCategory("MapPoint"), Benchmark(Baseline = true)]
	public SKPoint MapPoint_Native()
	{
		SKMatrix m = affine;
		SKPoint result;
		sk_matrix_map_xy(&m, 12.5f, -7.5f, &result);
		return result;
	}

	[BenchmarkCategory("MapPoint"), Benchmark]
	public SKPoint MapPoint_Managed() =>
		affine.MapPoint(12.5f, -7.5f);

	// ===== MapVector =====

	[BenchmarkCategory("MapVector"), Benchmark(Baseline = true)]
	public SKPoint MapVector_Native()
	{
		SKMatrix m = affine;
		SKPoint result;
		sk_matrix_map_vector(&m, 12.5f, -7.5f, &result);
		return result;
	}

	[BenchmarkCategory("MapVector"), Benchmark]
	public SKPoint MapVector_Managed() =>
		affine.MapVector(12.5f, -7.5f);

	// ===== MapRect (affine) =====

	[BenchmarkCategory("MapRect"), Benchmark(Baseline = true)]
	public SKRect MapRect_Native()
	{
		SKMatrix m = affine;
		SKRect src = rect, result;
		sk_matrix_map_rect(&m, &result, &src);
		return result;
	}

	[BenchmarkCategory("MapRect"), Benchmark]
	public SKRect MapRect_Managed() =>
		affine.MapRect(rect);

	// ===== MapRect: scale and translate fast paths (managed SortAsRect) =====

	[BenchmarkCategory("MapRectScale"), Benchmark(Baseline = true)]
	public SKRect MapRectScale_Native()
	{
		SKMatrix m = affine2;
		SKRect src = rect, result;
		sk_matrix_map_rect(&m, &result, &src);
		return result;
	}

	[BenchmarkCategory("MapRectScale"), Benchmark]
	public SKRect MapRectScale_Managed() =>
		affine2.MapRect(rect);

	[BenchmarkCategory("MapRectTranslate"), Benchmark(Baseline = true)]
	public SKRect MapRectTranslate_Native()
	{
		SKMatrix m = translate;
		SKRect src = rect, result;
		sk_matrix_map_rect(&m, &result, &src);
		return result;
	}

	[BenchmarkCategory("MapRectTranslate"), Benchmark]
	public SKRect MapRectTranslate_Managed() =>
		translate.MapRect(rect);

	// ===== MapRadius =====

	[BenchmarkCategory("MapRadius"), Benchmark(Baseline = true)]
	public float MapRadius_Native()
	{
		SKMatrix m = affine;
		return sk_matrix_map_radius(&m, 25f);
	}

	[BenchmarkCategory("MapRadius"), Benchmark]
	public float MapRadius_Managed() =>
		affine.MapRadius(25f);

	// ===== MapPoint on a perspective matrix =====

	[BenchmarkCategory("MapPointPersp"), Benchmark(Baseline = true)]
	public SKPoint MapPointPersp_Native()
	{
		SKMatrix m = perspective;
		SKPoint result;
		sk_matrix_map_xy(&m, 12.5f, -7.5f, &result);
		return result;
	}

	[BenchmarkCategory("MapPointPersp"), Benchmark]
	public SKPoint MapPointPersp_Managed() =>
		perspective.MapPoint(12.5f, -7.5f);
}

// Batch map operations parameterised by item count so we can see how the managed
// SIMD path scales versus Skia's native procs from small arrays (where the
// per-call P/Invoke transition dominates) up to very large arrays (where it is
// fully amortised and it is pure SIMD-vs-SIMD). Each matrix type exercises a
// different native proc: affine -> Affine_vpts, scale -> Scale_pts,
// translate -> Trans_pts.
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public unsafe class SKMatrixMapBatchBenchmark
{
	private const string SKIA = "libSkiaSharp";

	[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
	private static extern void sk_matrix_map_points(SKMatrix* matrix, SKPoint* dst, SKPoint* src, int count);

	[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
	private static extern void sk_matrix_map_vectors(SKMatrix* matrix, SKPoint* dst, SKPoint* src, int count);

	// low, typical/medium, high and very high item counts
	[Params(4, 64, 1024, 1_000_000)]
	public int Count;

	private SKMatrix affine;
	private SKMatrix scale;
	private SKMatrix translate;
	private SKPoint[] src;
	private SKPoint[] dst;

	[GlobalSetup]
	public void Setup()
	{
		affine = SKMatrix.CreateRotationDegrees(33);
		affine.ScaleX *= 1.5f;
		affine.TransX = 12;
		affine.TransY = -7;

		scale = SKMatrix.CreateScaleTranslation(0.75f, 1.25f, 5, 9);
		translate = SKMatrix.CreateTranslation(12, -7);

		src = new SKPoint[Count];
		dst = new SKPoint[Count];
		for (var i = 0; i < src.Length; i++)
			src[i] = new SKPoint(i * 0.5f, i * -0.25f);
	}

	// ===== MapPoints: affine proc (Affine_vpts, uses the swizzle) =====

	[BenchmarkCategory("MapPoints (affine)"), Benchmark(Baseline = true)]
	public void MapPointsAffine_Native()
	{
		SKMatrix m = affine;
		fixed (SKPoint* s = src)
		fixed (SKPoint* d = dst)
			sk_matrix_map_points(&m, d, s, src.Length);
	}

	[BenchmarkCategory("MapPoints (affine)"), Benchmark]
	public void MapPointsAffine_Managed() =>
		affine.MapPoints(dst, src);

	// ===== MapPoints: scale proc (Scale_pts) =====

	[BenchmarkCategory("MapPoints (scale)"), Benchmark(Baseline = true)]
	public void MapPointsScale_Native()
	{
		SKMatrix m = scale;
		fixed (SKPoint* s = src)
		fixed (SKPoint* d = dst)
			sk_matrix_map_points(&m, d, s, src.Length);
	}

	[BenchmarkCategory("MapPoints (scale)"), Benchmark]
	public void MapPointsScale_Managed() =>
		scale.MapPoints(dst, src);

	// ===== MapPoints: translate proc (Trans_pts) =====

	[BenchmarkCategory("MapPoints (translate)"), Benchmark(Baseline = true)]
	public void MapPointsTranslate_Native()
	{
		SKMatrix m = translate;
		fixed (SKPoint* s = src)
		fixed (SKPoint* d = dst)
			sk_matrix_map_points(&m, d, s, src.Length);
	}

	[BenchmarkCategory("MapPoints (translate)"), Benchmark]
	public void MapPointsTranslate_Managed() =>
		translate.MapPoints(dst, src);

	// ===== MapVectors: affine (drops translation, then maps) =====

	[BenchmarkCategory("MapVectors (affine)"), Benchmark(Baseline = true)]
	public void MapVectorsAffine_Native()
	{
		SKMatrix m = affine;
		fixed (SKPoint* s = src)
		fixed (SKPoint* d = dst)
			sk_matrix_map_vectors(&m, d, s, src.Length);
	}

	[BenchmarkCategory("MapVectors (affine)"), Benchmark]
	public void MapVectorsAffine_Managed() =>
		affine.MapVectors(dst, src);
}
