using System;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
#if ANDROID
using Native = SkiaSharp.Views.Android;
#elif IOS || MACCATALYST
using Native = SkiaSharp.Views.iOS;
#elif WINDOWS
using UnsupportedView = Microsoft.UI.Xaml.Controls.Grid;
#else
using UnsupportedView = System.Object;
#endif

namespace SkiaSharp.Views.Maui.Handlers
{
	/// <summary>Maps Graphite controls to supported native containers, never to another engine.</summary>
#if ANDROID || IOS || MACCATALYST
	public class SKGraphiteViewHandler : SKGpuViewHandler<ISKGraphiteView, Native.SKGraphiteView>
#else
	public class SKGraphiteViewHandler : ViewHandler<ISKGraphiteView, UnsupportedView>
#endif
	{
		/// <summary>The customizable Graphite property mapper.</summary>
		public static PropertyMapper<ISKGraphiteView, SKGraphiteViewHandler> SKGraphiteViewMapper = new (ViewHandler.ViewMapper) {
			[nameof (ISKGpuView.HasRenderLoop)] = MapProperties,
			[nameof (ISKGpuView.IgnorePixelScaling)] = MapProperties,
			[nameof (ISKGpuView.EnableTouchEvents)] = MapProperties,
		};
		/// <summary>The customizable Graphite command mapper.</summary>
		public static CommandMapper<ISKGraphiteView, SKGraphiteViewHandler> SKGraphiteViewCommandMapper = new (ViewHandler.ViewCommandMapper) {
			[nameof (ISKGpuView.InvalidateSurface)] = OnInvalidateSurface,
		};
		/// <summary>Creates a Graphite handler with default mappers.</summary>
		public SKGraphiteViewHandler () : this (null, null) { }
		/// <summary>Creates a Graphite handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper or null.</param>
		/// <param name="commands">The command mapper or null.</param>
		public SKGraphiteViewHandler (IPropertyMapper? mapper, CommandMapper? commands) :
			base (mapper ?? SKGraphiteViewMapper, commands ?? SKGraphiteViewCommandMapper) { }
		/// <summary>Maps the shared GPU properties.</summary>
		/// <param name="handler">The handler.</param>
		/// <param name="view">The control.</param>
		public static void MapProperties (SKGraphiteViewHandler handler, ISKGraphiteView view)
		{
#if ANDROID || IOS || MACCATALYST
			handler.MapGpuProperties ();
#endif
		}
		/// <summary>Requests a native frame.</summary>
		/// <param name="handler">The handler.</param>
		/// <param name="view">The control.</param>
		/// <param name="args">Unused command metadata.</param>
		public static void OnInvalidateSurface (SKGraphiteViewHandler handler, ISKGraphiteView view, object? args)
		{
#if ANDROID || IOS || MACCATALYST
			handler.PlatformView.InvalidateSurface ();
#else
			throw new PlatformNotSupportedException ("Graphite presentation is not supported on this platform.");
#endif
		}
		/// <summary>Creates a tracked caller-owned recorder on the context owner thread.</summary>
		public SKGraphiteRecorder CreateRecorder () =>
#if ANDROID || IOS || MACCATALYST
			PlatformView.CreateRecorder ();
#else
			throw new PlatformNotSupportedException ("Graphite presentation is not supported on this platform.");
#endif
		/// <summary>Snaps caller work on the recorder's exclusive thread and registers its generation.</summary>
		/// <param name="recorder">A recorder created by this view.</param>
		public SKGraphiteRecording SnapRecording (SKGraphiteRecorder recorder) =>
#if ANDROID || IOS || MACCATALYST
			PlatformView.SnapRecording (recorder);
#else
			throw new PlatformNotSupportedException ("Graphite presentation is not supported on this platform.");
#endif
		/// <summary>Inserts producer work on the owner thread; submission remains view-owned.</summary>
		/// <param name="recording">Work from the current context generation.</param>
		public SKGraphiteInsertStatus InsertRecording (SKGraphiteRecording recording) =>
#if ANDROID || IOS || MACCATALYST
			PlatformView.InsertRecording (recording);
#else
			throw new PlatformNotSupportedException ("Graphite presentation is not supported on this platform.");
#endif
#if ANDROID || IOS || MACCATALYST
		/// <inheritdoc />
		protected override Native.SKGraphiteView CreatePlatformView () =>
#if ANDROID
			new (Context);
#else
			new ();
#endif
		/// <inheritdoc />
		protected override void ConnectHandler (Native.SKGraphiteView platformView) { platformView.PaintSurface += Paint; base.ConnectHandler (platformView); }
		/// <inheritdoc />
		protected override void DisconnectHandler (Native.SKGraphiteView platformView)
		{
			platformView.PaintSurface -= Paint;
			base.DisconnectHandler (platformView);
		}
		/// <inheritdoc />
		protected override void UpdateContext (bool available) => VirtualView.OnGraphiteContextChanged (available ? PlatformView.GraphiteContext : null);
		private void Paint (object? sender, Native.SKPaintGraphiteSurfaceEventArgs e)
		{
			VirtualView.OnCanvasSizeChanged (e.Info.Size);
			VirtualView.OnPaintSurface (new SKPaintGraphiteSurfaceEventArgs (e.Surface, e.Context, e.BackendTexture, e.Info, e.RawInfo) {
				ReadPixelsRequest = e.RequestReadPixels,
			});
		}
#else
		/// <inheritdoc />
		protected override UnsupportedView CreatePlatformView () => throw new PlatformNotSupportedException (
			"Graphite presentation is supported only on iOS, Mac Catalyst and compatible Android devices. Windows Graphite is deferred.");
#endif
	}
}
