#nullable enable

using Microsoft.Maui;

namespace SkiaSharp.Views.Maui
{
	/// <summary>Defines the interface for a Graphite GPU-accelerated SkiaSharp view in .NET MAUI.</summary>
	/// <remarks>This interface is implemented by <see cref="T:SkiaSharp.Views.Maui.Controls.SKGraphiteView" /> and used by platform-specific handlers to communicate between the cross-platform view and native Graphite GPU-backed views.</remarks>
	public interface ISKGraphiteView : IView
	{
		/// <summary>Gets the current size of the canvas in pixels.</summary>
		/// <value>The size of the canvas in pixels.</value>
		/// <remarks>The size depends on the <see cref="P:SkiaSharp.Views.Maui.ISKGraphiteView.IgnorePixelScaling" /> setting. When <see langword="false" />, this returns physical pixels; when <see langword="true" />, this returns logical pixels matching the view size.</remarks>
		SKSize CanvasSize { get; }

		/// <summary>Gets the Graphite GPU context used for rendering.</summary>
		/// <value>The <see cref="T:SkiaSharp.SKGraphiteContext" /> instance, or <see langword="null" /> if the context is not yet available.</value>
		/// <remarks>The context is created by the platform handler and may be <see langword="null" /> before the view is fully initialized.</remarks>
		SKGraphiteContext? GraphiteContext { get; }

		/// <summary>Gets a value indicating whether the view uses a continuous render loop.</summary>
		/// <value><see langword="true" /> if the view renders continuously; <see langword="false" /> if it only renders when invalidated.</value>
		/// <remarks>When <see langword="true" />, <see cref="M:SkiaSharp.Views.Maui.ISKGraphiteView.OnPaintSurface(SkiaSharp.Views.Maui.SKPaintGraphiteSurfaceEventArgs)" /> is called continuously for animations. When <see langword="false" />, call <see cref="M:SkiaSharp.Views.Maui.ISKGraphiteView.InvalidateSurface" /> to trigger redraws.</remarks>
		bool HasRenderLoop { get; }

		/// <summary>Gets a value indicating whether the drawing canvas should ignore the physical pixel density.</summary>
		/// <value><see langword="true" /> to use logical pixels; <see langword="false" /> to use physical pixels.</value>
		/// <remarks>When <see langword="false" /> (the default), the canvas size is scaled to match the physical pixel density, resulting in sharper rendering on high-DPI displays. When <see langword="true" />, the canvas uses logical pixels matching the view's layout size.</remarks>
		bool IgnorePixelScaling { get; }

		/// <summary>Gets a value indicating whether touch events are enabled for this view.</summary>
		/// <value><see langword="true" /> if touch events are enabled; otherwise, <see langword="false" />.</value>
		/// <remarks>When enabled, the <see cref="M:SkiaSharp.Views.Maui.ISKGraphiteView.OnTouch(SkiaSharp.Views.Maui.SKTouchEventArgs)" /> method is called for touch interactions.</remarks>
		bool EnableTouchEvents { get; }

		/// <summary>Invalidates the surface, causing a redraw.</summary>
		/// <remarks>Call this method when you need to trigger a repaint of the view. When <see cref="P:SkiaSharp.Views.Maui.ISKGraphiteView.HasRenderLoop" /> is <see langword="false" />, this is the primary way to request rendering updates.</remarks>
		void InvalidateSurface();

		/// <summary>Called by the handler when the canvas size changes.</summary>
		/// <param name="size">The new canvas size in pixels.</param>
		/// <remarks>This method is invoked by the platform handler to notify the view that the underlying canvas size has changed, typically due to layout changes or device rotation.</remarks>
		void OnCanvasSizeChanged(SKSizeI size);

		/// <summary>Called by the handler when the Graphite GPU context changes.</summary>
		/// <param name="context">The new Graphite GPU context, or <see langword="null" /> if the context was destroyed.</param>
		/// <remarks>This method is invoked when the GPU context is created, recreated, or destroyed. Implementations should release any GPU resources tied to the old context and recreate them with the new context if needed.</remarks>
		void OnGraphiteContextChanged(SKGraphiteContext? context);

		/// <summary>Called by the handler when the surface needs to be painted.</summary>
		/// <param name="e">The event arguments containing the surface, canvas, and Graphite GPU context information.</param>
		/// <remarks>This method is invoked during the paint cycle. Implementations should perform drawing operations using the canvas provided in the event arguments. The surface is GPU-backed and supports hardware-accelerated rendering.</remarks>
		void OnPaintSurface(SKPaintGraphiteSurfaceEventArgs e);

		/// <summary>Called by the handler when a touch event occurs.</summary>
		/// <param name="e">The event arguments containing touch information.</param>
		/// <remarks>This method is only called when <see cref="P:SkiaSharp.Views.Maui.ISKGraphiteView.EnableTouchEvents" /> is <see langword="true" />. Implementations should handle touch interactions such as presses, moves, and releases.</remarks>
		void OnTouch(SKTouchEventArgs e);
	}
}
