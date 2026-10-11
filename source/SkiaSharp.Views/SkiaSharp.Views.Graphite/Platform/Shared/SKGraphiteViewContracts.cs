using System;
using System.Collections.Generic;

#if __ANDROID__
namespace SkiaSharp.Views.Android
#else
namespace SkiaSharp.Views.iOS
#endif
{
	/// <summary>Describes a Graphite drawing container.</summary>
	public interface ISKGraphiteView : ISKGpuView
	{
		/// <summary>Gets the borrowed context, or null before creation and after detach.</summary>
		/// <remarks>Do not create recorders directly from this context; use the view helper to register their lifetime.</remarks>
		SKGraphiteContext? GraphiteContext { get; }
		/// <summary>Creates a caller-owned recorder with a bounded image provider on the owner thread.</summary>
		/// <remarks>Stop workers and dispose the recorder and its surfaces during ContextDestroying.</remarks>
		SKGraphiteRecorder CreateRecorder ();
		/// <summary>Snaps caller work on the recorder's exclusive thread, registering its context generation.</summary>
		/// <param name="recorder">A recorder created by this view.</param>
		/// <remarks>Dispose the returned recording before ContextDestroying completes. Unregistered recordings cannot be inserted through the view.</remarks>
		SKGraphiteRecording SnapRecording (SKGraphiteRecorder recorder);
		/// <summary>Inserts a caller recording on the owner thread. The view retains exclusive submission ownership.</summary>
		/// <param name="recording">A recording from this context generation.</param>
		SKGraphiteInsertStatus InsertRecording (SKGraphiteRecording recording);
		/// <summary>Occurs when the borrowed frame should be painted.</summary>
		event EventHandler<SKPaintGraphiteSurfaceEventArgs>? PaintSurface;
	}

	/// <summary>Provides borrowed Graphite frame resources and common drawing metadata.</summary>
	public class SKPaintGraphiteSurfaceEventArgs : SKPaintSurfaceEventArgs
	{
		private readonly Readbacks readbacks = new ();
		private sealed class Readbacks
		{
			public readonly List<Action<bool>> Requests = new ();
			public bool Ended;
		}
		/// <summary>Creates frame metadata using physical drawing dimensions.</summary>
		/// <param name="surface">The borrowed surface.</param>
		/// <param name="backendTexture">The borrowed presentation texture.</param>
		/// <param name="context">The borrowed context.</param>
		/// <param name="info">The physical dimensions and format.</param>
		public SKPaintGraphiteSurfaceEventArgs (SKSurface surface, SKGraphiteBackendTexture backendTexture,
			SKGraphiteContext context, SKImageInfo info) : this (surface, backendTexture, context, info, info) { }
		/// <summary>Creates frame metadata.</summary>
		/// <param name="surface">The borrowed surface.</param>
		/// <param name="backendTexture">The borrowed presentation texture.</param>
		/// <param name="context">The borrowed context.</param>
		/// <param name="info">The drawing dimensions and format.</param>
		/// <param name="rawInfo">The physical dimensions and format.</param>
		public SKPaintGraphiteSurfaceEventArgs (SKSurface surface, SKGraphiteBackendTexture backendTexture,
			SKGraphiteContext context, SKImageInfo info, SKImageInfo rawInfo) : base (surface, info, rawInfo)
		{
			Context = context;
			BackendTexture = backendTexture;
		}
		internal SKPaintGraphiteSurfaceEventArgs (SKPaintGraphiteSurfaceEventArgs frame, SKImageInfo info)
			: this (frame.Surface, frame.BackendTexture, frame.Context, info, frame.RawInfo) => readbacks = frame.readbacks;
		/// <summary>Gets the borrowed Graphite context. Submission belongs to the view.</summary>
		public SKGraphiteContext Context { get; }
		/// <summary>Gets the borrowed presentation texture.</summary>
		public SKGraphiteBackendTexture BackendTexture { get; }
		internal bool HasReadbacks => readbacks.Requests.Count != 0;
		/// <summary>Requests pixels from this frame after its drawing has been inserted for execution.</summary>
		/// <param name="info">The requested pixel dimensions and format.</param>
		/// <param name="sourceRect">The source rectangle in physical surface pixels.</param>
		/// <param name="callback">Receives borrowed pixels, or null when the frame or read fails.</param>
		/// <remarks>Call during PaintSurface. Copy the result inside the callback; completion may occur while the view is idle.</remarks>
		public void RequestReadPixels (SKImageInfo info, SKRectI sourceRect, Action<SKImageReadPixelsResult?> callback)
		{
			if (callback is null) throw new ArgumentNullException (nameof (callback));
			if (readbacks.Ended) throw new InvalidOperationException ("The frame has already ended.");
			readbacks.Requests.Add (submitted => {
				if (submitted) Context.RequestReadPixels (Surface, info, sourceRect, callback);
				else callback (null);
			});
		}
		internal void CompleteReadbacks (bool submitted)
		{
			if (readbacks.Ended) return;
			readbacks.Ended = true;
			List<Exception>? failures = null;
			try {
				foreach (var request in readbacks.Requests) {
					try { request (submitted); }
					catch (Exception ex) { (failures ??= new ()).Add (ex); }
				}
			}
			finally { readbacks.Requests.Clear (); }
			if (failures is not null) throw new AggregateException ("Graphite frame readback callbacks failed.", failures);
		}
	}
}
