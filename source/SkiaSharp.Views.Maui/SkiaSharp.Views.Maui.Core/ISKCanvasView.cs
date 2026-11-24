using Microsoft.Maui;

namespace SkiaSharp.Views.Maui
{
	/// <summary>Defines the interface for a software-rendered SkiaSharp canvas view in .NET MAUI.</summary>
	/// <remarks>This interface is implemented by <see cref="T:SkiaSharp.Views.Maui.Controls.SKCanvasView" /> and used by platform-specific handlers to communicate between the cross-platform view and native platform views.</remarks>
	public interface ISKCanvasView : IView
	{
		/// <summary>Gets the current size of the canvas in pixels.</summary>
		/// <value>The size of the canvas in pixels.</value>
		/// <remarks>The size depends on the <see cref="P:SkiaSharp.Views.Maui.ISKCanvasView.IgnorePixelScaling" /> setting. When <see langword="false" />, this returns physical pixels; when <see langword="true" />, this returns logical pixels matching the view size.</remarks>
		SKSize CanvasSize { get; }

		/// <summary>Gets a value indicating whether the drawing canvas should ignore the physical pixel density.</summary>
		/// <value><see langword="true" /> to use logical pixels; <see langword="false" /> to use physical pixels.</value>
		/// <remarks>When <see langword="false" /> (the default), the canvas size is scaled to match the physical pixel density, resulting in sharper rendering on high-DPI displays. When <see langword="true" />, the canvas uses logical pixels matching the view's layout size.</remarks>
		bool IgnorePixelScaling { get; }

		/// <summary>Gets a value indicating whether touch events are enabled for this view.</summary>
		/// <value><see langword="true" /> if touch events are enabled; otherwise, <see langword="false" />.</value>
		/// <remarks>When enabled, the <see cref="M:SkiaSharp.Views.Maui.ISKCanvasView.OnTouch(SkiaSharp.Views.Maui.SKTouchEventArgs)" /> method is called for touch interactions.</remarks>
		bool EnableTouchEvents { get; }

		/// <summary>Invalidates the canvas surface, causing a redraw.</summary>
		/// <remarks>Call this method when you need to trigger a repaint of the canvas. This will cause <see cref="M:SkiaSharp.Views.Maui.ISKCanvasView.OnPaintSurface(SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs)" /> to be called on the next frame.</remarks>
		void InvalidateSurface();

		/// <summary>Called by the handler when the canvas size changes.</summary>
		/// <param name="size">The new canvas size in pixels.</param>
		/// <remarks>This method is invoked by the platform handler to notify the view that the underlying canvas size has changed, typically due to layout changes or device rotation.</remarks>
		void OnCanvasSizeChanged(SKSizeI size);

		/// <summary>Called by the handler when the surface needs to be painted.</summary>
		/// <param name="e">The event arguments containing the surface and canvas information.</param>
		/// <remarks>This method is invoked by the platform handler during the paint cycle. Implementations should perform drawing operations using the canvas provided in the event arguments.</remarks>
		void OnPaintSurface(SKPaintSurfaceEventArgs e);

		/// <summary>Called by the handler when a touch event occurs.</summary>
		/// <param name="e">The event arguments containing touch information.</param>
		/// <remarks>This method is only called when <see cref="P:SkiaSharp.Views.Maui.ISKCanvasView.EnableTouchEvents" /> is <see langword="true" />. Implementations should handle touch interactions such as presses, moves, and releases.</remarks>
		void OnTouch(SKTouchEventArgs e);
	}
}
