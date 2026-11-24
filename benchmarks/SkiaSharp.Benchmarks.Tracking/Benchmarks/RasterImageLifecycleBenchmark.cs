using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks.Tracking;

// Short-lived native-backed object churn: allocate + dispose many small objects per op.
// This is the exact create/dispose lifecycle the recent leak fixes touch — #4455 (the
// CoTaskMem raster buffer inside SKImage.Create) and #4453 (the GCHandle for a managed
// release delegate in SKData.Create). MemoryDiagnoser's Allocated bytes + GC counts are the
// tracked signal here: a future per-object managed-alloc regression (an added closure,
// boxing, or an un-freed handle) would show as a step in this benchmark's allocation line.
public class RasterImageLifecycleBenchmark
{
	[Params(256)]
	public int Count { get; set; }

	private const int Width = 64;
	private const int Height = 64;
	private const int BufferBytes = 256;

	private SKImageInfo _info;
	private SKDataReleaseDelegate _release = null!;

	[GlobalSetup]
	public void Setup ()
	{
		_info = new SKImageInfo (Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul);
		// A single reused delegate, so the only per-iteration managed allocation attributable
		// to the create path is what it does internally (a native GCHandle), not a fresh
		// closure per call.
		_release = static (address, _) => Marshal.FreeCoTaskMem (address);
	}

	// #4455: SKImage.Create(info) allocates a CoTaskMem raster buffer, wraps it, and frees it
	// on Dispose via a built-in release proc. Create + Dispose, Count times.
	[Benchmark]
	public int CreateRasterImage ()
	{
		var alive = 0;
		for (var i = 0; i < Count; i++) {
			using var image = SKImage.Create (_info);
			if (image is not null)
				alive++;
		}
		return alive;
	}

	// #4453: SKData.Create(ptr, len, releaseProc) pins the managed delegate via a GCHandle for
	// native; Dispose invokes the proc, which frees the buffer. Each op owns a fresh CoTaskMem
	// buffer that the SKData takes ownership of through the release proc.
	[Benchmark]
	public int CreateDataWithReleaseProc ()
	{
		var alive = 0;
		for (var i = 0; i < Count; i++) {
			var buffer = Marshal.AllocCoTaskMem (BufferBytes);
			using var data = SKData.Create (buffer, BufferBytes, _release);
			if (data is not null)
				alive++;
			else
				Marshal.FreeCoTaskMem (buffer); // create failed => proc never runs; free here
		}
		return alive;
	}
}
