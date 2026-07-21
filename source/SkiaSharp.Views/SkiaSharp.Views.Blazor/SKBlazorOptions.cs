namespace SkiaSharp.Views.Blazor;

/// <summary>
/// Global defaults for SkiaSharp Blazor views, applied when a view running in a bridged
/// host (Blazor Server, Blazor Hybrid or static server-side rendering) does not specify its
/// own values. Register with <c>services.AddSkiaSharpViewsBlazor(...)</c>.
/// </summary>
public sealed class SKBlazorOptions
{
	internal static readonly SKBlazorOptions Default = new SKBlazorOptions();

	/// <summary>
	/// The default frame transfer format. When <see langword="null"/> (the default) every
	/// bridged host uses <see cref="SKBlazorTransferFormat.Jpeg"/>, which keeps the per-frame
	/// payload small for both the Blazor Server network transport and the Blazor Hybrid WebView
	/// bridge.
	/// </summary>
	public SKBlazorTransferFormat? TransferFormat { get; set; }

	/// <summary>
	/// The JPEG quality (0-100) used when <see cref="SKBlazorTransferFormat.Jpeg"/> is
	/// selected. Ignored for other formats. Defaults to <c>85</c>.
	/// </summary>
	public int Quality { get; set; } = 85;
}
