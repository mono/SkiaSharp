namespace SkiaSharp.Views.Blazor
{
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
		/// larger payloads. A good default for Blazor Server when transparency is required.
		/// </summary>
		Png,

		/// <summary>
		/// Encode each frame as a JPEG image. Small payloads suitable for streaming over a
		/// network, but does not preserve transparency. The default for Blazor Server.
		/// </summary>
		Jpeg,

		/// <summary>
		/// Transfer the raw pixel buffer with no encoding. Avoids encode/decode cost and is the
		/// default for Blazor Hybrid (where the transport is in-process), but produces the
		/// largest payloads and is not recommended over a network.
		/// </summary>
		Put,
	}
}
