using System;

namespace SkiaSharp
{
	/// <summary>
	/// Convert <paramref name="image"/> (raster or lazy) to a Graphite-backed image suitable for
	/// drawing on <paramref name="recorder"/>, or return null to drop the draw. The returned
	/// SKImage's reference is consumed by Skia.
	///
	/// THREADING: runs on the recorder's owning thread, not necessarily the thread that
	/// constructed the callback. Any caches captured in the closure must be thread-safe.
	///
	/// EXCEPTIONS: never throw — the FFI boundary catches and converts to a null return
	/// (draw dropped).
	/// </summary>
	public delegate SKImage SKGraphiteFindOrCreateImageDelegate (SKGraphiteRecorder recorder, SKImage image, bool mipmapped);

	// Internal: signature the C# proxy stores in its GCHandle. Returning IntPtr.Zero drops the draw.
	internal delegate IntPtr SKGraphiteFindOrCreateImageProxy (IntPtr recorderHandle, IntPtr imageHandle, bool mipmapped);
}
