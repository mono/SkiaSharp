#nullable disable

using System;
using System.Threading;

namespace SkiaSharp
{
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

		public uint GenerationId => SkiaApi.sk_drawable_get_generation_id (Handle);

		public SKRect Bounds {
			get {
				SKRect bounds;
				SkiaApi.sk_drawable_get_bounds (Handle, &bounds);
				return bounds;
			}
		}

		public int ApproximateBytesUsed =>
			(int)SkiaApi.sk_drawable_approximate_bytes_used (Handle);

		public void Draw (SKCanvas canvas, in SKMatrix matrix)
		{
			fixed (SKMatrix* m = &matrix)
				SkiaApi.sk_drawable_draw (Handle, canvas.Handle, m);
		}

		public void Draw (SKCanvas canvas, float x, float y)
		{
			var matrix = SKMatrix.CreateTranslation (x, y);
			Draw (canvas, matrix);
		}

		// do not unref as this is a plain pointer return, not a reference counted pointer
		public SKPicture Snapshot () =>
			SKPicture.GetObject (SkiaApi.sk_drawable_new_picture_snapshot (Handle), unrefExisting: false);

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
