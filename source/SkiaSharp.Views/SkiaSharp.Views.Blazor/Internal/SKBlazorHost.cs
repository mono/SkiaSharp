using System;

namespace SkiaSharp.Views.Blazor.Internal
{
	/// <summary>
	/// The kind of Blazor host a view is executing in. Determines whether a view draws directly
	/// in the browser (WebAssembly) or renders on the .NET side and bridges frames to the browser.
	/// </summary>
	internal enum SKBlazorHostKind
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

	internal static class SKBlazorHost
	{
		/// <summary>
		/// Maps a Blazor <c>RendererInfo.Name</c> to a host kind. Falls back to
		/// <see cref="OperatingSystem.IsBrowser"/> when the renderer name is unavailable
		/// (for example on target frameworks earlier than .NET 9).
		/// </summary>
		public static SKBlazorHostKind Resolve(string? rendererName)
		{
			switch (rendererName)
			{
				case "WebAssembly":
					return SKBlazorHostKind.WebAssembly;
				case "Server":
					return SKBlazorHostKind.Server;
				case "WebView":
					return SKBlazorHostKind.Hybrid;
				case "Static":
					return SKBlazorHostKind.StaticSsr;
				default:
					// Host detection is only consulted once a component is interactive. An
					// interactive host that is not the browser and does not identify as Server
					// or static SSR is a native WebView (Blazor Hybrid). Treat any unrecognized
					// non-browser name as Hybrid so the bridged path activates regardless of the
					// exact RendererInfo.Name reported by the host.
					return OperatingSystem.IsBrowser()
						? SKBlazorHostKind.WebAssembly
						: SKBlazorHostKind.Hybrid;
			}
		}

		/// <summary>
		/// Whether the host renders on the .NET side and must bridge frames to the browser
		/// (Server, Hybrid or static SSR) rather than drawing directly in the browser.
		/// </summary>
		public static bool IsBridged(SKBlazorHostKind kind) =>
			kind == SKBlazorHostKind.Server ||
			kind == SKBlazorHostKind.Hybrid ||
			kind == SKBlazorHostKind.StaticSsr;

		/// <summary>
		/// Resolves the effective transfer format: an explicit per-control value wins, then the
		/// global option, then a host default (raw pixels for Hybrid, JPEG otherwise).
		/// </summary>
		public static SKBlazorTransferFormat ResolveTransferFormat(
			SKBlazorHostKind kind,
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
}
