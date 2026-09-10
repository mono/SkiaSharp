#nullable disable

using System;
using System.Threading;

namespace SkiaSharp
{
	/// <summary>Represents the base class for objects that draw into <see cref="T:SkiaSharp.SKCanvas" />.</summary>
	/// <remarks><format type="text/markdown"><![CDATA[
	/// ## Remarks
	///
	/// The object has a generation ID, which is guaranteed to be unique across all
	/// drawables. To allow for clients of the drawable that may want to cache the
	/// results, the drawable must change its generation ID whenever its internal
	/// state changes such that it will draw differently.
	/// ]]></format></remarks>
	public unsafe class SKDrawable : SKObject, ISKReferenceCounted
	{
		private static readonly SKManagedDrawableDelegates delegates;

		internal int fromNative;

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKDrawable" />.</summary>
		/// <remarks />
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

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKDrawable" />.</summary>
		/// <remarks />
		protected SKDrawable ()
			: this (true)
		{
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKDrawable" />.</summary>
		/// <param name="owns">The value indicating whether this object should destroy the underlying native object.</param>
		/// <remarks />
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

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKDrawable" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKDrawable" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
		/// <remarks />
		protected override void DisposeNative ()
		{
			if (Interlocked.CompareExchange (ref fromNative, 0, 0) == 0)
				SkiaApi.sk_drawable_unref (Handle);
		}

		/// <summary>Gets the unique value for this instance.</summary>
		/// <value>The unique generation identifier.</value>
		/// <remarks>It is presumed that if two calls return the same value, then drawing this will result in the same image as well.</remarks>
		public uint GenerationId {
			get {
				var result = SkiaApi.sk_drawable_get_generation_id (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Gets the conservative bounds of what the drawable will draw.</summary>
		/// <value>The bounding rectangle of the drawable.</value>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// If the drawable can change what it draws (e.g. animation or in response to
		/// some external change), then this must return a bounds that is always valid for
		/// all possible states.
		/// ]]></format></remarks>
		public SKRect Bounds {
			get {
				SKRect bounds;
				SkiaApi.sk_drawable_get_bounds (Handle, &bounds);
				GC.KeepAlive (this);
				return bounds;
			}
		}

		/// <summary>Gets the approximate number of bytes used by this drawable.</summary>
		/// <value>The approximate memory usage in bytes.</value>
		/// <remarks />
		public int ApproximateBytesUsed {
			get {
				var result = (int)SkiaApi.sk_drawable_approximate_bytes_used (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Draw the current drawable onto the specified canvas.</summary>
		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="matrix">The matrix to use when drawing.</param>
		/// <remarks />
		public void Draw (SKCanvas canvas, in SKMatrix matrix)
		{
			fixed (SKMatrix* m = &matrix)
				SkiaApi.sk_drawable_draw (Handle, canvas.Handle, m);
			GC.KeepAlive (canvas);
			GC.KeepAlive (this);
		}

		/// <summary>Draw the current drawable onto the specified canvas.</summary>
		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="x">The amount to translate along the x-coordinate.</param>
		/// <param name="y">The amount to translate along the y-coordinate.</param>
		/// <remarks />
		public void Draw (SKCanvas canvas, float x, float y)
		{
			var matrix = SKMatrix.CreateTranslation (x, y);
			Draw (canvas, matrix);
		}

		// do not unref as this is a plain pointer return, not a reference counted pointer
		/// <summary>Create an immutable snapshot of the drawing.</summary>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKPicture" /> snapshot.</returns>
		/// <remarks />
		public SKPicture Snapshot ()
		{
			var result = SKPicture.GetObject (SkiaApi.sk_drawable_new_picture_snapshot (Handle), unrefExisting: false);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Invalidate the drawing generation ID, indicating that the drawing has changed.</summary>
		/// <remarks>This is typically used by the object itself in response to its internal state changing.</remarks>
		public void NotifyDrawingChanged ()
		{
			SkiaApi.sk_drawable_notify_drawing_changed (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKDrawable" /> types to draw the drawable to the canvas.</summary>
		/// <param name="canvas">The canvas to draw on.</param>
		/// <remarks>If the generation ID is the same, then the resulting image must be the same.</remarks>
		protected internal virtual void OnDraw (SKCanvas canvas)
		{
		}

		/// <summary>Implemented by derived types to return the approximate memory usage.</summary>
		/// <returns>The approximate number of bytes used by this drawable.</returns>
		/// <remarks />
		protected internal virtual int OnGetApproximateBytesUsed () => 0;

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKDrawable" /> types to return the conservative bounds of what the drawable will draw.</summary>
		/// <returns>Returns the bounds.</returns>
		/// <remarks />
		protected internal virtual SKRect OnGetBounds () => SKRect.Empty;

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKDrawable" /> types to create an immutable snapshot of the drawing.</summary>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKPicture" /> snapshot.</returns>
		/// <remarks />
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
