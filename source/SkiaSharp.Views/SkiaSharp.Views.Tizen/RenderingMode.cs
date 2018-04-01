namespace SkiaSharp.Views.Tizen
{
	/// <summary>
	/// Rendering mode used by the CustomRenderingView.
	/// </summary>
	public enum RenderingMode
	{
		/// <summary>
		/// View decides when to repaint the surface.
		/// </summary>
		Continuously,
		/// <summary>
		/// Surface is repainted on demand.
		/// </summary>
		WhenDirty,
	}
}
