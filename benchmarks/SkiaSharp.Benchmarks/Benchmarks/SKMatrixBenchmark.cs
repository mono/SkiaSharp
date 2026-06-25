using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace SkiaSharp.Benchmarks;

// Before/after comparison for issue #2779: the "Native" methods call the Skia
// C API directly (the path the old SKMatrix used) and the "Managed" methods use
// the new managed SKMatrix math. They must produce identical results; the goal
// is to show the managed path avoids the P/Invoke transition cost.
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
	private static extern void sk_matrix_map_points(SKMatrix* matrix, SKPoint* dst, SKPoint* src, int count);

	[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
	private static extern void sk_matrix_map_vectors(SKMatrix* matrix, SKPoint* dst, SKPoint* src, int count);

	[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
	private static extern void sk_matrix_map_rect(SKMatrix* matrix, SKRect* dest, SKRect* source);

	[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
	private static extern float sk_matrix_map_radius(SKMatrix* matrix, float radius);

	private SKMatrix affine;
	private SKMatrix affine2;
	private SKMatrix translate;
	private SKMatrix perspective;
	private SKPoint[] src;
	private SKPoint[] dst;
	private SKPoint[] srcSmall;
	private SKPoint[] dstSmall;
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

		src = new SKPoint[1024];
		dst = new SKPoint[1024];
		for (var i = 0; i < src.Length; i++)
			src[i] = new SKPoint(i * 0.5f, i * -0.25f);

		// small batch: shows the win when the per-call P/Invoke transition is not amortized
		srcSmall = new SKPoint[16];
		dstSmall = new SKPoint[16];
		for (var i = 0; i < srcSmall.Length; i++)
			srcSmall[i] = new SKPoint(i * 0.5f, i * -0.25f);
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

	// ===== MapRect =====

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

	// ===== MapPoints (batch of 1024) =====

	[BenchmarkCategory("MapPoints1024"), Benchmark(Baseline = true)]
	public void MapPoints_Native()
	{
		SKMatrix m = affine;
		fixed (SKPoint* s = src)
		fixed (SKPoint* d = dst)
			sk_matrix_map_points(&m, d, s, src.Length);
	}

	[BenchmarkCategory("MapPoints1024"), Benchmark]
	public void MapPoints_Managed() =>
		affine.MapPoints(dst, src);

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

	// ===== MapPoints batch: scale and translate procs (no swizzle) =====

	[BenchmarkCategory("MapPointsScale1024"), Benchmark(Baseline = true)]
	public void MapPointsScale_Native()
	{
		SKMatrix m = affine2;
		fixed (SKPoint* s = src)
		fixed (SKPoint* d = dst)
			sk_matrix_map_points(&m, d, s, src.Length);
	}

	[BenchmarkCategory("MapPointsScale1024"), Benchmark]
	public void MapPointsScale_Managed() =>
		affine2.MapPoints(dst, src);

	[BenchmarkCategory("MapPointsTranslate1024"), Benchmark(Baseline = true)]
	public void MapPointsTranslate_Native()
	{
		SKMatrix m = translate;
		fixed (SKPoint* s = src)
		fixed (SKPoint* d = dst)
			sk_matrix_map_points(&m, d, s, src.Length);
	}

	[BenchmarkCategory("MapPointsTranslate1024"), Benchmark]
	public void MapPointsTranslate_Managed() =>
		translate.MapPoints(dst, src);

	// ===== MapPoints batch: small count (P/Invoke transition not amortized) =====

	[BenchmarkCategory("MapPoints16"), Benchmark(Baseline = true)]
	public void MapPointsSmall_Native()
	{
		SKMatrix m = affine;
		fixed (SKPoint* s = srcSmall)
		fixed (SKPoint* d = dstSmall)
			sk_matrix_map_points(&m, d, s, srcSmall.Length);
	}

	[BenchmarkCategory("MapPoints16"), Benchmark]
	public void MapPointsSmall_Managed() =>
		affine.MapPoints(dstSmall, srcSmall);

	// ===== MapVectors batch (1024 and small count) =====

	[BenchmarkCategory("MapVectors1024"), Benchmark(Baseline = true)]
	public void MapVectors_Native()
	{
		SKMatrix m = affine;
		fixed (SKPoint* s = src)
		fixed (SKPoint* d = dst)
			sk_matrix_map_vectors(&m, d, s, src.Length);
	}

	[BenchmarkCategory("MapVectors1024"), Benchmark]
	public void MapVectors_Managed() =>
		affine.MapVectors(dst, src);

	[BenchmarkCategory("MapVectors16"), Benchmark(Baseline = true)]
	public void MapVectorsSmall_Native()
	{
		SKMatrix m = affine;
		fixed (SKPoint* s = srcSmall)
		fixed (SKPoint* d = dstSmall)
			sk_matrix_map_vectors(&m, d, s, srcSmall.Length);
	}

	[BenchmarkCategory("MapVectors16"), Benchmark]
	public void MapVectorsSmall_Managed() =>
		affine.MapVectors(dstSmall, srcSmall);

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
}
