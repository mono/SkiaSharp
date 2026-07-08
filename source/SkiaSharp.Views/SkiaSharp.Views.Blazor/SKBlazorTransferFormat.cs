namespace SkiaSharp.Views.Blazor;

/// <summary>
/// Controls how a rendered frame is transferred to the browser canvas when a view is
/// running in a bridged host (Blazor Server, Blazor Hybrid or static server-side rendering).
/// </summary>
/// <remarks>
/// This setting only applies when the view cannot draw directly in the browser (that is,
/// anywhere other than Blazor WebAssembly). In WebAssembly the frame is drawn natively and
/// this value is ignored.
/// </remarks>
public enum SKBlazorTransferFormat
{
	/// <summary>
	/// Encode each frame as a PNG image. Lossless and preserves transparency, but produces
	/// larger payloads. A good choice when transparency is required.
	/// </summary>
	Png,

	/// <summary>
	/// Encode each frame as a JPEG image. Small payloads suitable for streaming over a
	/// network, but does not preserve transparency. This is the default for every bridged host.
	/// </summary>
	Jpeg,

	/// <summary>
	/// Transfer the raw pixel buffer with no encoding. Avoids encode/decode cost but produces
	/// the largest payloads; it is not recommended over a network (Blazor Server) and, because
	/// the Blazor Hybrid WebView bridge marshals the bytes rather than sharing memory, it is not
	/// recommended there either.
	/// </summary>
	Put,
}
