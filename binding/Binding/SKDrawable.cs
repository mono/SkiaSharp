using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp
{
	public unsafe class SKDrawable : SKObject, ISKReferenceCounted
	{
		private static readonly SKManagedDrawableDelegates delegates;

		private int fromNative;

		static SKDrawable ()
		{
			delegates = new SKManagedDrawableDelegates {
				fDraw = DrawInternal,
				fGetBounds = GetBoundsInternal,
				fNewPictureSnapshot = NewPictureSnapshotInternal,
				fDestroy = DestroyInternal,
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

		public void Draw (SKCanvas canvas, ref SKMatrix matrix)
		{
			fixed (SKMatrix* m = &matrix) {
				SkiaApi.sk_drawable_draw (Handle, canvas.Handle, m);
			}
		}

		public void Draw (SKCanvas canvas, float x, float y)
		{
			var matrix = SKMatrix.CreateTranslation (x, y);
			Draw (canvas, ref matrix);
		}

		// do not unref as this is a plain pointer return, not a reference counted pointer
		public SKPicture Snapshot () =>
			SKPicture.GetObject (SkiaApi.sk_drawable_new_picture_snapshot (Handle), unrefExisting: false);

		public void NotifyDrawingChanged () =>
			SkiaApi.sk_drawable_notify_drawing_changed (Handle);

		protected virtual void OnDraw (SKCanvas canvas)
		{
		}

		protected virtual SKRect OnGetBounds () => SKRect.Empty;

		protected virtual SKPicture OnSnapshot ()
		{
			using var recorder = new SKPictureRecorder();
			var canvas = recorder.BeginRecording(Bounds);
			Draw(canvas, 0, 0);
			return recorder.EndRecording();
        }

		[MonoPInvokeCallback (typeof (SKManagedDrawableDrawProxyDelegate))]
		private static void DrawInternal (IntPtr d, void* context, IntPtr canvas)
		{
			var drawable = DelegateProxies.GetUserData<SKDrawable> ((IntPtr)context, out _);
			drawable.OnDraw (SKCanvas.GetObject (canvas, false));
		}

		[MonoPInvokeCallback (typeof (SKManagedDrawableGetBoundsProxyDelegate))]
		private static void GetBoundsInternal (IntPtr d, void* context, SKRect* rect)
		{
			var drawable = DelegateProxies.GetUserData<SKDrawable> ((IntPtr)context, out _);
			var bounds = drawable.OnGetBounds ();
			*rect = bounds;
		}

		[MonoPInvokeCallback (typeof (SKManagedDrawableNewPictureSnapshotProxyDelegate))]
		private static IntPtr NewPictureSnapshotInternal (IntPtr d, void* context)
		{
			var drawable = DelegateProxies.GetUserData<SKDrawable> ((IntPtr)context, out _);
			return drawable.OnSnapshot ()?.Handle ?? IntPtr.Zero;
		}

		[MonoPInvokeCallback (typeof (SKManagedDrawableDestroyProxyDelegate))]
		private static void DestroyInternal (IntPtr d, void* context)
		{
			var drawable = DelegateProxies.GetUserData<SKDrawable> ((IntPtr)context, out var gch);
			if (drawable != null) {
				Interlocked.Exchange (ref drawable.fromNative, 1);
				drawable.Dispose ();
			}
			gch.Free ();
		}

		internal static SKDrawable GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKDrawable (h, o));
	}
}
