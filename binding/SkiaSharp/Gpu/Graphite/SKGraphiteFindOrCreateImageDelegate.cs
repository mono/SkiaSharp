using System;

namespace SkiaSharp
{
	/// <param name="recorder">The recorder that the returned image is created for.</param>
	/// <param name="image">The source image to find or upload.</param>
	/// <param name="mipmapped">
	///       <see langword="true" /> to create the texture with mipmaps; otherwise, <see langword="false" />.</param>
	/// <summary>Represents the method that returns a Graphite-backed image for the specified source image, uploading it to the recorder if it has not been cached yet.</summary>
	/// <returns>A Graphite-backed image for the source image, or <see langword="null" /> if one could not be created.</returns>
	/// <remarks />
	public delegate SKImage SKGraphiteFindOrCreateImageDelegate (SKGraphiteRecorder recorder, SKImage image, bool mipmapped);

	// Internal: signature the C# proxy stores in its GCHandle. Returning IntPtr.Zero drops the draw.
	internal delegate IntPtr SKGraphiteFindOrCreateImageProxy (IntPtr recorderHandle, IntPtr imageHandle, bool mipmapped);
}
