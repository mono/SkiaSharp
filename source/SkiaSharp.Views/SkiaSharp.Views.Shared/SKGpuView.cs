#nullable enable

using System;

#if !HAS_UNO && !HAS_UNO_WINUI && !__DESKTOP__ && !__BLAZOR__ && !__MACOS__ && !__TVOS__ && !__TIZEN__
#if __ANDROID__
namespace SkiaSharp.Views.Android
#elif __IOS__
namespace SkiaSharp.Views.iOS
#elif WINDOWS
namespace SkiaSharp.Views.Windows
#endif
{
	/// <summary>Describes an engine-neutral GPU drawing host.</summary>
	/// <remarks>GPU callbacks run on the context owner thread. Frame surfaces are borrowed.</remarks>
	public interface ISKGpuView
	{
		/// <summary>Gets the current drawing size, accounting for pixel scaling.</summary>
		SKSize CanvasSize { get; }
		/// <summary>Gets or sets continuous rendering. The default is false.</summary>
		bool HasRenderLoop { get; set; }
		/// <summary>Gets or sets drawing in logical rather than physical pixels.</summary>
		bool IgnorePixelScaling { get; set; }
		/// <summary>Requests a frame on the context owner thread.</summary>
		void InvalidateSurface ();
		/// <summary>Stops rendering and releases this host's GPU resources.</summary>
		void Disconnect ();
		/// <summary>Occurs on the owner thread after context creation.</summary>
		event EventHandler? ContextCreated;
		/// <summary>Occurs before context destruction. Stop and dispose dependent GPU work synchronously.</summary>
		event EventHandler? ContextDestroying;
		/// <summary>Occurs after the public context has been cleared.</summary>
		event EventHandler? ContextDestroyed;
		/// <summary>Occurs when rendering fails. No other engine is substituted.</summary>
		event EventHandler<SKGpuRenderFailedEventArgs>? RenderFailed;
	}

	/// <summary>Describes a Ganesh drawing host.</summary>
	public interface ISKGaneshView : ISKGpuView
	{
		/// <summary>Gets the borrowed context, or null before initialization and after detach.</summary>
		GRContext? GRContext { get; }
		/// <summary>Occurs when the frame should be painted.</summary>
		event EventHandler<SKPaintGaneshSurfaceEventArgs>? PaintSurface;
	}

	/// <summary>Reports a GPU presentation failure.</summary>
	public sealed class SKGpuRenderFailedEventArgs : EventArgs
	{
		/// <summary>Creates failure metadata.</summary>
		/// <param name="exception">The presentation or drawing failure.</param>
		public SKGpuRenderFailedEventArgs (Exception exception) =>
			Exception = exception ?? throw new ArgumentNullException (nameof (exception));
		/// <summary>Gets the failure. Rendering remains stopped until explicit invalidation or reattachment.</summary>
		public Exception Exception { get; }
	}

	/// <summary>Provides borrowed Ganesh frame resources and common drawing metadata.</summary>
	public class SKPaintGaneshSurfaceEventArgs : SKPaintSurfaceEventArgs
	{
		/// <summary>Creates frame metadata.</summary>
		/// <param name="surface">The borrowed frame surface.</param>
		/// <param name="context">The borrowed Ganesh context.</param>
		/// <param name="renderTarget">The borrowed backend render target.</param>
		/// <param name="origin">The render target origin.</param>
		/// <param name="info">The drawing dimensions and format.</param>
		/// <param name="rawInfo">The physical dimensions and format.</param>
		public SKPaintGaneshSurfaceEventArgs (SKSurface surface, GRContext context,
			GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKImageInfo info, SKImageInfo rawInfo)
			: base (surface, info, rawInfo)
		{
			Context = context;
			BackendRenderTarget = renderTarget;
			Origin = origin;
		}
		/// <summary>Gets the borrowed Ganesh context.</summary>
		public GRContext Context { get; }
		/// <summary>Gets the borrowed render target.</summary>
		public GRBackendRenderTarget BackendRenderTarget { get; }
		/// <summary>Gets the render target origin.</summary>
		public GRSurfaceOrigin Origin { get; }
	}
}
#endif
