using System;

namespace SkiaSharp.Views.Blazor.Internal;

/// <summary>
/// The kind of Blazor host a view is executing in. Determines whether a view draws directly
/// in the browser (WebAssembly) or renders on the .NET side and bridges frames to the browser.
/// </summary>
internal enum HostKind
{
	/// <summary>Unknown / not yet interactive.</summary>
	Unknown,

	/// <summary>Blazor WebAssembly: draws directly in the browser (the "direct" path).</summary>
	WebAssembly,

	/// <summary>Blazor Server: renders on the server, streams frames over SignalR.</summary>
	Server,

	/// <summary>Blazor Hybrid (<c>BlazorWebView</c>): renders natively in-process.</summary>
	Hybrid,

	/// <summary>Static server-side rendering / prerender: a single poster frame, no interactivity.</summary>
	StaticSsr,
}

internal static class Host
{
	/// <summary>
	/// Maps a Blazor <c>RendererInfo.Name</c> to a host kind. Host detection is only consulted
	/// once a component is interactive, so any unrecognized non-browser name is treated as a
	/// native WebView (Blazor Hybrid); only the browser falls back to WebAssembly.
	/// </summary>
	public static HostKind Resolve(string? rendererName)
	{
		switch (rendererName)
		{
			case "WebAssembly":
				return HostKind.WebAssembly;
			case "Server":
				return HostKind.Server;
			case "WebView":
				return HostKind.Hybrid;
			case "Static":
				return HostKind.StaticSsr;
			default:
				return OperatingSystem.IsBrowser()
					? HostKind.WebAssembly
					: HostKind.Hybrid;
		}
	}

	/// <summary>
	/// Whether the host renders on the .NET side and must bridge frames to the browser
	/// (Server, Hybrid or static SSR) rather than drawing directly in the browser.
	/// </summary>
	public static bool IsBridged(HostKind kind) =>
		kind == HostKind.Server ||
		kind == HostKind.Hybrid ||
		kind == HostKind.StaticSsr;

	/// <summary>
	/// Resolves the effective transfer format: an explicit per-control value wins, then the
	/// global option, then a host-independent JPEG default.
	/// </summary>
	public static SKBlazorTransferFormat ResolveTransferFormat(
		HostKind kind,
		SKBlazorTransferFormat? perControl,
		SKBlazorOptions options)
	{
		if (perControl.HasValue)
			return perControl.Value;

		if (options.TransferFormat.HasValue)
			return options.TransferFormat.Value;

		// JPEG by default for every bridged host. Keeping the per-frame payload small matters
		// even for Blazor Hybrid: the WebView bridge marshals the bytes across the native/JS
		// boundary rather than sharing memory, so pushing a raw full-resolution buffer every
		// frame (SKBlazorTransferFormat.Put) is far heavier than a small encoded frame. Apps
		// can still opt into Put (lossless, no encode) or Png (lossless with alpha).
		return SKBlazorTransferFormat.Jpeg;
	}
}
