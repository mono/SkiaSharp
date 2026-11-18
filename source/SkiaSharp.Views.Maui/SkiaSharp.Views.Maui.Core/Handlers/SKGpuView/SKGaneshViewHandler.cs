using System;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
#if ANDROID
using Native = SkiaSharp.Views.Android;
#elif IOS || MACCATALYST
using Native = SkiaSharp.Views.iOS;
#elif WINDOWS
using Native = SkiaSharp.Views.Windows;
#endif

namespace SkiaSharp.Views.Maui.Handlers
{
	/// <summary>Maps the Ganesh control to an engine-neutral native container.</summary>
#if ANDROID || IOS || MACCATALYST || WINDOWS
	public class SKGaneshViewHandler : SKGpuViewHandler<ISKGaneshView, Native.SKGaneshView>
#else
	public class SKGaneshViewHandler : ViewHandler<ISKGaneshView, object>
#endif
	{
		/// <summary>The customizable Ganesh property mapper.</summary>
		public static PropertyMapper<ISKGaneshView, SKGaneshViewHandler> SKGaneshViewMapper = new (ViewHandler.ViewMapper) {
			[nameof (ISKGpuView.HasRenderLoop)] = MapProperties,
			[nameof (ISKGpuView.IgnorePixelScaling)] = MapProperties,
			[nameof (ISKGpuView.EnableTouchEvents)] = MapProperties,
		};
		/// <summary>The customizable Ganesh command mapper.</summary>
		public static CommandMapper<ISKGaneshView, SKGaneshViewHandler> SKGaneshViewCommandMapper = new (ViewHandler.ViewCommandMapper) {
			[nameof (ISKGpuView.InvalidateSurface)] = OnInvalidateSurface,
		};
		/// <summary>Creates a Ganesh handler with default mappers.</summary>
		public SKGaneshViewHandler () : this (null, null) { }
		/// <summary>Creates a Ganesh handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper or null.</param>
		/// <param name="commands">The command mapper or null.</param>
		public SKGaneshViewHandler (IPropertyMapper? mapper, CommandMapper? commands) :
			base (mapper ?? SKGaneshViewMapper, commands ?? SKGaneshViewCommandMapper) { }
		/// <summary>Maps the shared GPU properties.</summary>
		/// <param name="handler">The handler.</param>
		/// <param name="view">The control.</param>
		public static void MapProperties (SKGaneshViewHandler handler, ISKGaneshView view)
		{
#if ANDROID || IOS || MACCATALYST || WINDOWS
			handler.MapGpuProperties ();
#endif
		}
		/// <summary>Requests a native frame.</summary>
		/// <param name="handler">The handler.</param>
		/// <param name="view">The control.</param>
		/// <param name="args">Unused command metadata.</param>
		public static void OnInvalidateSurface (SKGaneshViewHandler handler, ISKGaneshView view, object? args)
		{
#if ANDROID || IOS || MACCATALYST || WINDOWS
			handler.PlatformView.InvalidateSurface ();
#endif
		}
#if ANDROID || IOS || MACCATALYST || WINDOWS
		/// <inheritdoc />
		protected override Native.SKGaneshView CreatePlatformView () =>
#if ANDROID
			new (Context);
#else
			new ();
#endif
		/// <inheritdoc />
		protected override void ConnectHandler (Native.SKGaneshView platformView) { platformView.PaintSurface += Paint; base.ConnectHandler (platformView); }
		/// <inheritdoc />
		protected override void DisconnectHandler (Native.SKGaneshView platformView)
		{
			platformView.PaintSurface -= Paint;
			base.DisconnectHandler (platformView);
		}
		/// <inheritdoc />
		protected override void UpdateContext (bool available) => VirtualView.OnGRContextChanged (available ? PlatformView.GRContext : null);
		private void Paint (object? sender, Native.SKPaintGaneshSurfaceEventArgs e)
		{
			VirtualView.OnCanvasSizeChanged (e.Info.Size);
			VirtualView.OnPaintSurface (new SKPaintGaneshSurfaceEventArgs (e.Surface, e.Context, e.BackendRenderTarget, e.Origin, e.Info, e.RawInfo));
		}
#else
		/// <inheritdoc />
		protected override object CreatePlatformView () => throw new PlatformNotSupportedException ("Ganesh views are supported on iOS, Mac Catalyst, Android and Windows.");
#endif
	}
}
