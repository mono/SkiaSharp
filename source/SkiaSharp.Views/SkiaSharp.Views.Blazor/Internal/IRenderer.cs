using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace SkiaSharp.Views.Blazor.Internal;

/// <summary>
/// Paints one frame into the supplied surface. <paramref name="glRenderTarget"/> and
/// <paramref name="glOrigin"/> are only meaningful for <c>SKGLView</c> (they are
/// <see langword="null"/>/default for <c>SKCanvasView</c>); the component uses them to build the
/// appropriate paint event args.
/// </summary>
internal delegate void RenderPaintHandler(
	SKSurface surface,
	SKImageInfo info,
	SKImageInfo rawInfo,
	GRBackendRenderTarget? glRenderTarget,
	GRSurfaceOrigin glOrigin);

/// <summary>
/// A rendering strategy for a Blazor SkiaSharp view. Concrete implementations are the in-browser
/// direct renderers (<see cref="CanvasDirectRenderer"/>, <see cref="GLDirectRenderer"/>) and the
/// bridged renderer (<see cref="BridgedRenderer"/>). The component owns one instance and drives it
/// through this interface, so it does not need to branch on the host.
/// </summary>
internal interface IRenderer : IAsyncDisposable
{
	/// <summary>The device pixel ratio currently in effect.</summary>
	double Dpi { get; }

	/// <summary>Whether frames are produced continuously.</summary>
	bool EnableRenderLoop { get; set; }

	/// <summary>The coordinate space handed to the paint callback (see the view's IgnorePixelScaling).</summary>
	bool IgnorePixelScaling { get; set; }

	/// <summary>Bridged-only transfer format override; ignored by direct renderers.</summary>
	SKBlazorTransferFormat? TransferFormat { get; set; }

	/// <summary>Bridged-only JPEG quality override; ignored by direct renderers.</summary>
	int? Quality { get; set; }

	/// <summary>Initializes the renderer against the view's canvas element.</summary>
	Task InitializeAsync(ElementReference canvas);

	/// <summary>Requests that a frame be rendered.</summary>
	void Invalidate();
}
