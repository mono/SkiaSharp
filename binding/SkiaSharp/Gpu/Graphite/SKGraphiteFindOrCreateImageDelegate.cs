using System;

namespace SkiaSharp
{
	public delegate SKImage SKGraphiteFindOrCreateImageDelegate (SKGraphiteRecorder recorder, SKImage image, bool mipmapped);

	// Internal: signature the C# proxy stores in its GCHandle. Returning IntPtr.Zero drops the draw.
	internal delegate IntPtr SKGraphiteFindOrCreateImageProxy (IntPtr recorderHandle, IntPtr imageHandle, bool mipmapped);
}
