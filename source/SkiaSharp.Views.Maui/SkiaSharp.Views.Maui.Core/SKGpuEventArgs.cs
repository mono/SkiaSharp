using System;

namespace SkiaSharp.Views.Maui
{
	/// <summary>Provides borrowed Ganesh frame resources and common drawing metadata.</summary>
	public class SKPaintGaneshSurfaceEventArgs : SKPaintSurfaceEventArgs
	{
		/// <summary>Creates frame metadata.</summary>
		/// <param name="surface">The borrowed surface.</param>
		/// <param name="context">The borrowed Ganesh context.</param>
		/// <param name="renderTarget">The borrowed render target.</param>
		/// <param name="origin">The render target origin.</param>
		/// <param name="info">The drawing dimensions and format.</param>
		/// <param name="rawInfo">The physical dimensions and format.</param>
		public SKPaintGaneshSurfaceEventArgs (SKSurface surface, GRContext context, GRBackendRenderTarget renderTarget,
			GRSurfaceOrigin origin, SKImageInfo info, SKImageInfo rawInfo) : base (surface, info, rawInfo)
		{
			Context = context; BackendRenderTarget = renderTarget; Origin = origin;
		}
		/// <summary>Gets the borrowed context.</summary>
		public GRContext Context { get; }
		/// <summary>Gets the borrowed render target.</summary>
		public GRBackendRenderTarget BackendRenderTarget { get; }
		/// <summary>Gets the render target origin.</summary>
		public GRSurfaceOrigin Origin { get; }
	}
	/// <summary>Provides borrowed Graphite frame resources and common drawing metadata.</summary>
	public class SKPaintGraphiteSurfaceEventArgs : SKPaintSurfaceEventArgs
	{
		internal Action<SKImageInfo, SKRectI, Action<SKImageReadPixelsResult?>>? ReadPixelsRequest { get; set; }
		/// <summary>Creates frame metadata.</summary>
		/// <param name="surface">The borrowed surface.</param>
		/// <param name="context">The borrowed context.</param>
		/// <param name="backendTexture">The borrowed texture.</param>
		/// <param name="info">The drawing dimensions and format.</param>
		/// <param name="rawInfo">The physical dimensions and format.</param>
		public SKPaintGraphiteSurfaceEventArgs (SKSurface surface, SKGraphiteContext context, SKGraphiteBackendTexture backendTexture,
			SKImageInfo info, SKImageInfo rawInfo) : base (surface, info, rawInfo)
		{
			Context = context; BackendTexture = backendTexture;
		}
		/// <summary>Gets the borrowed context; do not submit independently or create untracked recorders.</summary>
		public SKGraphiteContext Context { get; }
		/// <summary>Gets the borrowed presentation texture.</summary>
		public SKGraphiteBackendTexture BackendTexture { get; }
		/// <summary>Requests pixels after this frame's drawing is inserted for execution.</summary>
		/// <param name="info">The requested pixel dimensions and format.</param>
		/// <param name="sourceRect">The source rectangle in physical surface pixels.</param>
		/// <param name="callback">Receives borrowed pixels, or null when the frame or read fails.</param>
		/// <remarks>Call during PaintSurface and copy pixels inside the callback. Completion is processed even when rendering on demand.</remarks>
		public void RequestReadPixels (SKImageInfo info, SKRectI sourceRect, Action<SKImageReadPixelsResult?> callback)
		{
			if (callback is null) throw new ArgumentNullException (nameof (callback));
			var request = ReadPixelsRequest ?? throw new InvalidOperationException ("These arguments are not associated with an active view frame.");
			request (info, sourceRect, callback);
		}
	}
	/// <summary>Reports a GPU presentation failure.</summary>
	public sealed class SKGpuRenderFailedEventArgs : EventArgs
	{
		/// <summary>Creates failure metadata.</summary>
		/// <param name="exception">The failure.</param>
		public SKGpuRenderFailedEventArgs (Exception exception) =>
			Exception = exception ?? throw new ArgumentNullException (nameof (exception));
		/// <summary>Gets the failure. Retry is explicit; no renderer is silently substituted.</summary>
		public Exception Exception { get; }
	}
}
