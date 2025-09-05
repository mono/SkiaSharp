namespace SkiaSharp.Views.Blazor;

/// <summary>
/// Provides data for the <see cref="E:SkiaSharp.Views.Blazor.SKGLView.PaintSurface" /> event.
/// </summary>
public class SKPaintGLSurfaceEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance of the <see cref="T:SkiaSharp.Views.Blazor.SKPaintGLSurfaceEventArgs" /> event arguments.
	/// </summary>
	public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget)
		: this(surface, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888)
	{
	}

	/// <summary>
	/// Creates a new instance of the <see cref="T:SkiaSharp.Views.Blazor.SKPaintGLSurfaceEventArgs" /> event arguments.
	/// </summary>
	public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType)
	{
		Surface = surface;
		BackendRenderTarget = renderTarget;
		ColorType = colorType;
		Origin = origin;
		Info = new SKImageInfo(renderTarget.Width, renderTarget.Height, ColorType);
		RawInfo = Info;
	}

	public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKImageInfo info)
		: this(surface, renderTarget, origin, info, info)
	{
	}

	public SKPaintGLSurfaceEventArgs(SKSurface surface, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKImageInfo info, SKImageInfo rawInfo)
	{
		Surface = surface;
		BackendRenderTarget = renderTarget;
		ColorType = info.ColorType;
		Origin = origin;
		Info = info;
		RawInfo = rawInfo;
	}

	/// <summary>
	/// Gets the surface that is currently being drawn on.
	/// </summary>
	public SKSurface Surface { get; private set; }

	/// <summary>
	/// Gets the render target that is currently being drawn.
	/// </summary>
	public GRBackendRenderTarget BackendRenderTarget { get; private set; }

	/// <summary>
	/// Gets the color type of the render target.
	/// </summary>
	public SKColorType ColorType { get; private set; }

	/// <summary>
	/// Gets the surface origin of the render target.
	/// </summary>
	public GRSurfaceOrigin Origin { get; private set; }

	public SKImageInfo Info { get; private set; }

	public SKImageInfo RawInfo { get; private set; }
}
