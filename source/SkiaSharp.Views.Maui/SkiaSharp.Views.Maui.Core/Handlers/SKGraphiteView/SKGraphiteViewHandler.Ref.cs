#nullable enable

using System;
using Microsoft.Maui.Handlers;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKGraphiteViewHandler : ViewHandler<ISKGraphiteView, object>
	{
		/// <summary>Creates the platform-specific Graphite GPU view for the current platform.</summary>
		/// <returns>The platform-specific Graphite GPU-backed view instance.</returns>
		/// <exception cref="T:System.PlatformNotSupportedException">Always thrown on unsupported target frameworks.</exception>
		/// <remarks />
		protected override object CreatePlatformView() =>
			throw new PlatformNotSupportedException("SKGraphiteView is not supported on this platform.");

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGraphiteView.IgnorePixelScaling" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the IgnorePixelScaling property changes to update the native view's pixel scaling behavior.</remarks>
		public static void MapIgnorePixelScaling(SKGraphiteViewHandler handler, ISKGraphiteView view)
		{
		}

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGraphiteView.HasRenderLoop" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the HasRenderLoop property changes to enable or disable continuous rendering on the native view.</remarks>
		public static void MapHasRenderLoop(SKGraphiteViewHandler handler, ISKGraphiteView view)
		{
		}

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGraphiteView.EnableTouchEvents" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the EnableTouchEvents property changes to update the native view's touch handling.</remarks>
		public static void MapEnableTouchEvents(SKGraphiteViewHandler handler, ISKGraphiteView view)
		{
		}

		/// <summary>Handles the <see cref="M:SkiaSharp.Views.Maui.ISKGraphiteView.InvalidateSurface" /> command to trigger a redraw.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view requesting invalidation.</param>
		/// <param name="args">Optional arguments (not used).</param>
		/// <remarks>This method is called when the cross-platform control requests a surface invalidation, causing the native GPU view to repaint.</remarks>
		public static void OnInvalidateSurface(SKGraphiteViewHandler handler, ISKGraphiteView view, object? args)
		{
		}
	}
}
