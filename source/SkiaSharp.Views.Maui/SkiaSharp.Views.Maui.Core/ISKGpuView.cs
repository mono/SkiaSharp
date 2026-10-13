using System;
using Microsoft.Maui;

namespace SkiaSharp.Views.Maui
{
	/// <summary>Common drawing and lifecycle behavior for engine-named GPU controls.</summary>
	public interface ISKGpuView : IView
	{
		/// <summary>Gets the current drawing dimensions.</summary>
		SKSize CanvasSize { get; }
		/// <summary>Gets whether continuous rendering is enabled.</summary>
		bool HasRenderLoop { get; }
		/// <summary>Gets whether drawing uses logical pixel coordinates.</summary>
		bool IgnorePixelScaling { get; }
		/// <summary>Gets whether touch forwarding is enabled.</summary>
		bool EnableTouchEvents { get; }
		/// <summary>Requests a frame.</summary>
		void InvalidateSurface ();
		/// <summary>Updates the control's canvas dimensions.</summary>
		/// <param name="size">The new drawing size.</param>
		void OnCanvasSizeChanged (SKSizeI size);
		/// <summary>Forwards touch metadata.</summary>
		/// <param name="e">Touch metadata.</param>
		void OnTouch (SKTouchEventArgs e);
		/// <summary>Notifies callers to stop and dispose dependent GPU work synchronously on the owner thread.</summary>
		void OnContextDestroying ();
		/// <summary>Notifies callers of rendering failure; there is no engine fallback.</summary>
		/// <param name="exception">The failure.</param>
		void OnRenderFailed (Exception exception);
	}
	/// <summary>A Ganesh drawing control contract.</summary>
	public interface ISKGaneshView : ISKGpuView
	{
		/// <summary>Gets the borrowed context or null when detached.</summary>
		GRContext? GRContext { get; }
		/// <summary>Updates the context and raises its lifecycle notifications.</summary>
		/// <param name="context">The borrowed context or null.</param>
		void OnGRContextChanged (GRContext? context);
		/// <summary>Forwards borrowed frame metadata.</summary>
		/// <param name="e">Frame metadata.</param>
		void OnPaintSurface (SKPaintGaneshSurfaceEventArgs e);
	}
	/// <summary>A Graphite drawing control contract.</summary>
	public interface ISKGraphiteView : ISKGpuView
	{
		/// <summary>Gets the borrowed context or null when detached.</summary>
		SKGraphiteContext? GraphiteContext { get; }
		/// <summary>Creates a tracked caller-owned recorder on the owner thread.</summary>
		SKGraphiteRecorder CreateRecorder ();
		/// <summary>Snaps registered caller work on the recorder's exclusive thread.</summary>
		/// <param name="recorder">A recorder created by this view in the current generation.</param>
		/// <remarks>Dispose returned recordings during ContextDestroying. Registration rejects stale work before native insertion.</remarks>
		SKGraphiteRecording SnapRecording (SKGraphiteRecorder recorder);
		/// <summary>Inserts producer work on the owner thread. Submission remains view-owned.</summary>
		/// <param name="recording">Work from the current context generation.</param>
		SKGraphiteInsertStatus InsertRecording (SKGraphiteRecording recording);
		/// <summary>Updates the context and raises its lifecycle notifications.</summary>
		/// <param name="context">The borrowed context or null.</param>
		void OnGraphiteContextChanged (SKGraphiteContext? context);
		/// <summary>Forwards borrowed frame metadata.</summary>
		/// <param name="e">Frame metadata.</param>
		void OnPaintSurface (SKPaintGraphiteSurfaceEventArgs e);
	}
}
