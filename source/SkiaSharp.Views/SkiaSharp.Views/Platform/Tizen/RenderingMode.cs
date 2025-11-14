namespace SkiaSharp.Views.Tizen
{
	/// <summary>
	/// Various options for rendering a view.
	/// </summary>
	public enum RenderingMode
	{
		/// <summary>
		/// The view is repainted continuously.
		/// </summary>
		Continuously,
		/// <summary>
		/// The view only redraws the surface when the it is created, resized, or when <see cref="M:SkiaSharp.Views.Tizen.CustomRenderingView.Invalidate" /> is called.
		/// </summary>
		WhenDirty,
	}
}
