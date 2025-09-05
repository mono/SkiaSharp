namespace SkiaSharp.Views.Blazor;

/// <summary>
/// Provides data for the <see cref="E:SkiaSharp.Views.Blazor.SKCanvasView.PaintSurface" /> event.
/// </summary>
public class SKPaintSurfaceEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new instance of the <see cref="T:SkiaSharp.Views.Blazor.SKPaintSurfaceEventArgs" /> event arguments.
	/// </summary>
	public SKPaintSurfaceEventArgs(SKSurface surface, SKImageInfo info)
		: this(surface, info, info)
	{
	}

	/// <param name="surface"></param>
	/// <param name="info"></param>
	/// <param name="rawInfo"></param>
	public SKPaintSurfaceEventArgs(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo)
	{
		Surface = surface;
		Info = info;
		RawInfo = rawInfo;
	}

	/// <summary>
	/// Gets the surface that is currently being drawn on.
	/// </summary>
	public SKSurface Surface { get; }

	/// <summary>
	/// Gets the information about the surface that is currently being drawn.
	/// </summary>
	public SKImageInfo Info { get; }

	public SKImageInfo RawInfo { get; }
}
