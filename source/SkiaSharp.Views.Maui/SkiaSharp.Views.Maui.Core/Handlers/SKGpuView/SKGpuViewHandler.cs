using System;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
#if ANDROID || IOS || MACCATALYST || WINDOWS
using SkiaSharp.Views.Maui.Platform;
#endif

#if ANDROID
using Native = SkiaSharp.Views.Android;
using PlatformView = Android.Views.View;
#elif IOS || MACCATALYST
using Native = SkiaSharp.Views.iOS;
using PlatformView = UIKit.UIView;
#elif WINDOWS
using Native = SkiaSharp.Views.Windows;
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#endif

#if ANDROID || IOS || MACCATALYST || WINDOWS
namespace SkiaSharp.Views.Maui.Handlers
{
	/// <summary>Shared lifecycle and input infrastructure for engine-specific handlers.</summary>
	/// <typeparam name="TVirtual">The engine control contract.</typeparam>
	/// <typeparam name="TPlatform">The engine-neutral platform container.</typeparam>
	public abstract class SKGpuViewHandler<TVirtual, TPlatform> : ViewHandler<TVirtual, TPlatform>
		where TVirtual : class, ISKGpuView
		where TPlatform : PlatformView, Native.ISKGpuView
	{
		private SKTouchHandler? touch;
		/// <summary>Initializes a GPU handler.</summary>
		/// <param name="mapper">The property mapper.</param>
		/// <param name="commands">The command mapper.</param>
		protected SKGpuViewHandler (IPropertyMapper mapper, CommandMapper commands) : base (mapper, commands) { }
		/// <inheritdoc />
		protected override void ConnectHandler (TPlatform platformView)
		{
			platformView.ContextCreated += ContextCreated;
			platformView.ContextDestroying += ContextDestroying;
			platformView.ContextDestroyed += ContextDestroyed;
			platformView.RenderFailed += Failed;
			base.ConnectHandler (platformView);
		}
		/// <inheritdoc />
		protected override void DisconnectHandler (TPlatform platformView)
		{
			platformView.HasRenderLoop = false;
			try { platformView.Disconnect (); }
			finally {
				touch?.Detach (platformView); touch = null;
				platformView.ContextCreated -= ContextCreated;
				platformView.ContextDestroying -= ContextDestroying;
				platformView.ContextDestroyed -= ContextDestroyed;
				platformView.RenderFailed -= Failed;
				UpdateContext (false);
				VirtualView.OnCanvasSizeChanged (SKSizeI.Empty);
				base.DisconnectHandler (platformView);
			}
		}
		/// <summary>Updates typed context state after a native lifecycle event.</summary>
		/// <param name="available">Whether the context is active.</param>
		protected abstract void UpdateContext (bool available);
		private void ContextCreated (object? sender, EventArgs e) => UpdateContext (true);
		private void ContextDestroying (object? sender, EventArgs e) => VirtualView.OnContextDestroying ();
		private void ContextDestroyed (object? sender, EventArgs e) { UpdateContext (false); VirtualView.OnCanvasSizeChanged (SKSizeI.Empty); }
		private void Failed (object? sender, Native.SKGpuRenderFailedEventArgs e) => VirtualView.OnRenderFailed (e.Exception);
		/// <summary>Synchronizes common drawing and touch properties.</summary>
		protected void MapGpuProperties ()
		{
			PlatformView.HasRenderLoop = VirtualView.HasRenderLoop;
			PlatformView.IgnorePixelScaling = VirtualView.IgnorePixelScaling;
			touch ??= new SKTouchHandler (e => VirtualView.OnTouch (e), ScaleTouch);
			touch.SetEnabled (PlatformView, VirtualView.EnableTouchEvents);
			PlatformView.InvalidateSurface ();
		}
		private SKPoint ScaleTouch (double x, double y)
		{
#if ANDROID
			if (VirtualView.IgnorePixelScaling) {
				var density = PlatformView.Resources?.DisplayMetrics?.Density ?? 1;
				x /= density; y /= density;
			}
#elif IOS || MACCATALYST
			if (!VirtualView.IgnorePixelScaling) { x *= PlatformView.ContentScaleFactor; y *= PlatformView.ContentScaleFactor; }
#elif WINDOWS
			if (!VirtualView.IgnorePixelScaling) { var scale = PlatformView.XamlRoot?.RasterizationScale ?? 1; x *= scale; y *= scale; }
#endif
			return new SKPoint ((float)x, (float)y);
		}
	}
}
#endif
