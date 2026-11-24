using System;
using Microsoft.Maui.Handlers;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKCanvasViewHandler : ViewHandler<ISKCanvasView, object>
	{
		/// <summary>Creates the platform-specific view for the current platform.</summary>
		/// <returns>The platform-specific canvas view instance.</returns>
		/// <remarks>Returns an Android SKCanvasView, iOS SKCanvasView, or Windows SKXamlCanvas depending on the platform.</remarks>
		protected override object CreatePlatformView() => throw new NotImplementedException();

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKCanvasView.IgnorePixelScaling" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="canvasView">The canvas view whose property changed.</param>
		/// <remarks>This method is called when the IgnorePixelScaling property changes to update the native view's pixel scaling behavior.</remarks>
		public static void MapIgnorePixelScaling(SKCanvasViewHandler handler, ISKCanvasView canvasView) { }

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKCanvasView.EnableTouchEvents" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="canvasView">The canvas view whose property changed.</param>
		/// <remarks>This method is called when the EnableTouchEvents property changes to update the native view's touch handling.</remarks>
		public static void MapEnableTouchEvents(SKCanvasViewHandler handler, ISKCanvasView canvasView) { }

		/// <summary>Handles the <see cref="M:SkiaSharp.Views.Maui.ISKCanvasView.InvalidateSurface" /> command to trigger a redraw.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="canvasView">The canvas view requesting invalidation.</param>
		/// <param name="args">Optional arguments (not used).</param>
		/// <remarks>This method is called when the cross-platform control requests a surface invalidation, causing the native view to repaint.</remarks>
		public static void OnInvalidateSurface(SKCanvasViewHandler handler, ISKCanvasView canvasView, object? args) { }
	}
}
