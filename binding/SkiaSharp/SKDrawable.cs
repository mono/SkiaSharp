#nullable disable

using System;
using System.Threading;

namespace SkiaSharp
{
	/// <summary>
	/// Represents the base class for objects that draw into <see cref="T:SkiaSharp.SKCanvas" />.
	/// </summary>
	/// <remarks>The object has a generation ID, which is guaranteed to be unique across all
	/// drawables. To allow for clients of the drawable that may want to cache the
	/// results, the drawable must change its generation ID whenever its internal
	/// state changes such that it will draw differently.</remarks>
	public unsafe class SKDrawable : SKObject, ISKReferenceCounted
	{
		private static readonly SKManagedDrawableDelegates delegates;

		internal int fromNative;

		static SKDrawable ()
		{
			delegates = new SKManagedDrawableDelegates {
				fDraw = DelegateProxies.SKManagedDrawableDrawProxy,
				fGetBounds = DelegateProxies.SKManagedDrawableGetBoundsProxy,
				fApproximateBytesUsed = DelegateProxies.SKManagedDrawableApproximateBytesUsedProxy,
				fMakePictureSnapshot = DelegateProxies.SKManagedDrawableMakePictureSnapshotProxy,
				fDestroy = DelegateProxies.SKManagedDrawableDestroyProxy
			};

			SkiaApi.sk_manageddrawable_set_procs (delegates);
		}

		protected SKDrawable ()
			: this (true)
		{
		}

		protected SKDrawable (bool owns)
			: base (IntPtr.Zero, owns)
		{
			var ctx = DelegateProxies.CreateUserData (this, true);
			Handle = SkiaApi.sk_manageddrawable_new ((void*)ctx);

			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKDrawable instance.");
			}
		}

		internal SKDrawable (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative ()
		{
			if (Interlocked.CompareExchange (ref fromNative, 0, 0) == 0)
				SkiaApi.sk_drawable_unref (Handle);
		}

		/// <summary>
		/// Gets the unique value for this instance.
		/// </summary>
		/// <remarks>It is presumed that if two calls return the same value, then drawing this will result in the same image as well.</remarks>
		public uint GenerationId => SkiaApi.sk_drawable_get_generation_id (Handle);

		/// <summary>
		/// Gets the conservative bounds of what the drawable will draw.
		/// </summary>
		/// <remarks>If the drawable can change what it draws (e.g. animation or in response to
		/// some external change), then this must return a bounds that is always valid for
		/// all possible states.</remarks>
		public SKRect Bounds {
			get {
				SKRect bounds;
				SkiaApi.sk_drawable_get_bounds (Handle, &bounds);
				return bounds;
			}
		}

		public int ApproximateBytesUsed =>
			(int)SkiaApi.sk_drawable_approximate_bytes_used (Handle);

		/// <summary>
		/// Draw the current drawable onto the specified canvas.
		/// </summary>
		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="matrix">The matrix to use when drawing.</param>
		public void Draw (SKCanvas canvas, in SKMatrix matrix)
		{
			fixed (SKMatrix* m = &matrix)
				SkiaApi.sk_drawable_draw (Handle, canvas.Handle, m);
		}

		/// <summary>
		/// Draw the current drawable onto the specified canvas.
		/// </summary>
		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="x">The amount to translate along the x-coordinate.</param>
		/// <param name="y">The amount to translate along the y-coordinate.</param>
		public void Draw (SKCanvas canvas, float x, float y)
		{
			var matrix = SKMatrix.CreateTranslation (x, y);
			Draw (canvas, matrix);
		}

		// do not unref as this is a plain pointer return, not a reference counted pointer
		/// <summary>
		/// Create an immutable snapshot of the drawing.
		/// </summary>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKPicture" /> snapshot.</returns>
		public SKPicture Snapshot () =>
			SKPicture.GetObject (SkiaApi.sk_drawable_new_picture_snapshot (Handle), unrefExisting: false);

		/// <summary>
		/// Invalidate the drawing generation ID, indicating that the drawing has changed.
		/// </summary>
		/// <remarks>This is typically used by the object itself in response to its internal state changing.</remarks>
		public void NotifyDrawingChanged () =>
			SkiaApi.sk_drawable_notify_drawing_changed (Handle);

		protected internal virtual void OnDraw (SKCanvas canvas)
		{
		}

		protected internal virtual int OnGetApproximateBytesUsed () => 0;

		protected internal virtual SKRect OnGetBounds () => SKRect.Empty;

		protected internal virtual SKPicture OnSnapshot ()
		{
			using var recorder = new SKPictureRecorder ();
			var canvas = recorder.BeginRecording (Bounds);
			Draw (canvas, 0, 0);
			return recorder.EndRecording ();
		}

		internal static SKDrawable GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKDrawable (h, o));
	}
}
