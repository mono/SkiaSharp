using System;
using Microsoft.Maui.Handlers;
using SkiaSharp.Views.Maui.Platform;
using SkiaSharp.Views.Tizen.NUI;
using ScalingInfo = SkiaSharp.Views.Tizen.ScalingInfo;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKGLViewHandler : ViewHandler<ISKGLView, SKGLSurfaceView>
	{
		/// <summary>Throws because GPU views are not supported on Tizen.</summary>
		/// <exception cref="T:System.PlatformNotSupportedException">Always thrown because <see cref="T:SkiaSharp.Views.Maui.ISKGLView" /> is not implemented on Tizen.</exception>
		/// <remarks />
		protected override SKGLSurfaceView CreatePlatformView() => throw new PlatformNotSupportedException("SKGLView is not yet implemented for Tizen.");

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGLView.IgnorePixelScaling" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the IgnorePixelScaling property changes to update the native view's pixel scaling behavior.</remarks>
		public static void MapIgnorePixelScaling(SKGLViewHandler handler, ISKGLView view) { }

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGLView.HasRenderLoop" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the HasRenderLoop property changes to enable or disable continuous rendering on the native view.</remarks>
		public static void MapHasRenderLoop(SKGLViewHandler handler, ISKGLView view) { }

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGLView.EnableTouchEvents" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the EnableTouchEvents property changes to update the native view's touch handling.</remarks>
		public static void MapEnableTouchEvents(SKGLViewHandler handler, ISKGLView view) { }

		/// <summary>Handles the <see cref="M:SkiaSharp.Views.Maui.ISKGLView.InvalidateSurface" /> command to trigger a redraw.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view requesting invalidation.</param>
		/// <param name="args">Optional arguments (not used).</param>
		/// <remarks>This method is called when the cross-platform control requests a surface invalidation, causing the native GPU view to repaint.</remarks>
		public static void OnInvalidateSurface(SKGLViewHandler handler, ISKGLView view, object? args) { }
	}
}
