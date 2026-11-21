#nullable enable

using System;
using Microsoft.Maui.Handlers;

namespace SkiaSharp.Views.Maui.Handlers
{
	public partial class SKGaneshViewHandler : ViewHandler<ISKGaneshView, object>
	{
		/// <summary>Creates the platform-specific Ganesh GPU view for the current platform.</summary>
		/// <returns>The platform-specific Ganesh GPU-backed view instance.</returns>
		/// <exception cref="T:System.PlatformNotSupportedException">Always thrown on unsupported target frameworks.</exception>
		/// <remarks />
		protected override object CreatePlatformView() =>
			throw new PlatformNotSupportedException("SKGaneshView is not supported on this platform.");

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGaneshView.IgnorePixelScaling" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the IgnorePixelScaling property changes to update the native view's pixel scaling behavior.</remarks>
		public static void MapIgnorePixelScaling(SKGaneshViewHandler handler, ISKGaneshView view)
		{
		}

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGaneshView.HasRenderLoop" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the HasRenderLoop property changes to enable or disable continuous rendering on the native view.</remarks>
		public static void MapHasRenderLoop(SKGaneshViewHandler handler, ISKGaneshView view)
		{
		}

		/// <summary>Maps the <see cref="P:SkiaSharp.Views.Maui.ISKGaneshView.EnableTouchEvents" /> property to the platform view.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view whose property changed.</param>
		/// <remarks>This method is called when the EnableTouchEvents property changes to update the native view's touch handling.</remarks>
		public static void MapEnableTouchEvents(SKGaneshViewHandler handler, ISKGaneshView view)
		{
		}

		/// <summary>Handles the <see cref="M:SkiaSharp.Views.Maui.ISKGaneshView.InvalidateSurface" /> command to trigger a redraw.</summary>
		/// <param name="handler">The handler instance.</param>
		/// <param name="view">The view requesting invalidation.</param>
		/// <param name="args">Optional arguments (not used).</param>
		/// <remarks>This method is called when the cross-platform control requests a surface invalidation, causing the native GPU view to repaint.</remarks>
		public static void OnInvalidateSurface(SKGaneshViewHandler handler, ISKGaneshView view, object? args)
		{
		}
	}
}
