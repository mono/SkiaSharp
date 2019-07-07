using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp
{
	public class SKDrawable : SKObject, ISKReferenceCounted
	{
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate void DrawDelegate (IntPtr d, IntPtr context, IntPtr canvas);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate void GetBoundsDelegate (IntPtr d, IntPtr context, out SKRect rect);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate IntPtr NewPictureSnapshotDelegate (IntPtr d, IntPtr context);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate void DestroyDelegate (IntPtr d, IntPtr context);

		[StructLayout (LayoutKind.Sequential)]
		internal struct Procs
		{
			public DrawDelegate fDraw;
			public GetBoundsDelegate fGetBounds;
			public NewPictureSnapshotDelegate fNewPictureSnapshot;
			public DestroyDelegate fDestroy;
		}

		private static readonly Procs delegates;

		private int fromNative;

		static SKDrawable ()
		{
			delegates = new Procs {
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
			DelegateProxies.Create (this, out _, out var ctx);
			Handle = SkiaApi.sk_manageddrawable_new (ctx);

			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKDrawable instance.");
			}
		}

		[Preserve]
		internal SKDrawable (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		protected override void DisposeNative ()
		{
			if (Interlocked.CompareExchange (ref fromNative, 0, 0) == 0)
				SkiaApi.sk_drawable_unref (Handle);
		}

		public uint GenerationId => SkiaApi.sk_drawable_get_generation_id (Handle);

		public SKRect Bounds {
			get {
				SkiaApi.sk_drawable_get_bounds (Handle, out var bounds);
				return bounds;
			}
		}

		public void Draw (SKCanvas canvas, ref SKMatrix matrix) =>
			SkiaApi.sk_drawable_draw (Handle, canvas.Handle, ref matrix);

		public void Draw (SKCanvas canvas, float x, float y)
		{
			var matrix = SKMatrix.MakeTranslation (x, y);
			Draw (canvas, ref matrix);
		}

		// do not unref as this is a plain pointer return, not a reference counted pointer
		public SKPicture Snapshot () =>
			GetObject<SKPicture> (SkiaApi.sk_drawable_new_picture_snapshot (Handle), unrefExisting: false);

		public void NotifyDrawingChanged () =>
			SkiaApi.sk_drawable_notify_drawing_changed (Handle);

		protected virtual void OnDraw (SKCanvas canvas)
		{
		}

		protected virtual SKRect OnGetBounds () => SKRect.Empty;

		protected virtual SKPicture OnSnapshot ()
		{
			using (var recorder = new SKPictureRecorder ()) {
				var canvas = recorder.BeginRecording (Bounds);
				Draw (canvas, 0, 0);
				return recorder.EndRecording ();
			}
		}

		[MonoPInvokeCallback (typeof (DrawDelegate))]
		private static void DrawInternal (IntPtr d, IntPtr context, IntPtr canvas)
		{
			var drawable = DelegateProxies.Get<SKDrawable> (context, out _);
			drawable.OnDraw (GetObject<SKCanvas> (canvas));
		}

		[MonoPInvokeCallback (typeof (GetBoundsDelegate))]
		private static void GetBoundsInternal (IntPtr d, IntPtr context, out SKRect rect)
		{
			var drawable = DelegateProxies.Get<SKDrawable> (context, out _);
			rect = drawable.OnGetBounds ();
		}

		[MonoPInvokeCallback (typeof (NewPictureSnapshotDelegate))]
		private static IntPtr NewPictureSnapshotInternal (IntPtr d, IntPtr context)
		{
			var drawable = DelegateProxies.Get<SKDrawable> (context, out _);
			return drawable.OnSnapshot ()?.Handle ?? IntPtr.Zero;
		}

		[MonoPInvokeCallback (typeof (DestroyDelegate))]
		private static void DestroyInternal (IntPtr d, IntPtr context)
		{
			var drawable = DelegateProxies.Get<SKDrawable> (context, out var gch);
			Interlocked.Exchange (ref drawable.fromNative, 1);
			gch.Free ();
			drawable.Dispose ();
		}
	}
}
