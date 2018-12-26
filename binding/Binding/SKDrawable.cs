using System;
using System.Runtime.InteropServices;
using System.Threading;

using NativePointerDictionary = System.Collections.Concurrent.ConcurrentDictionary<System.IntPtr, SkiaSharp.SKDrawable>;

namespace SkiaSharp
{
	public class SKDrawable : SKObject
	{
		private static readonly NativePointerDictionary managedDrawables = new NativePointerDictionary ();

		// delegate declarations
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate void draw_delegate (IntPtr managedDrawablePtr, IntPtr canvasPtr);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate void getBounds_delegate (IntPtr managedDrawablePtr, out SKRect rect);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate IntPtr newPictureSnapshot_delegate (IntPtr managedDrawablePtr);

		// delegate fields
		private static readonly draw_delegate fDraw;
		private static readonly getBounds_delegate fGetBounds;
		private static readonly newPictureSnapshot_delegate fNewPictureSnapshot;

		private int fromNative;

		static SKDrawable ()
		{
			fDraw = new draw_delegate (DrawInternal);
			fGetBounds = new getBounds_delegate (GetBoundsInternal);
			fNewPictureSnapshot = new newPictureSnapshot_delegate (NewPictureSnapshotInternal);

			SkiaApi.sk_manageddrawable_set_delegates (
				Marshal.GetFunctionPointerForDelegate (fDraw),
				Marshal.GetFunctionPointerForDelegate (fGetBounds),
				Marshal.GetFunctionPointerForDelegate (fNewPictureSnapshot));
		}

		protected SKDrawable ()
			: this (true)
		{
		}

		protected SKDrawable (bool owns)
			: base (SkiaApi.sk_manageddrawable_new (), owns)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKDrawable instance.");
			}

			managedDrawables.TryAdd (Handle, this);
		}

		[Preserve]
		internal SKDrawable (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		private void DisposeFromNative ()
		{
			Interlocked.Exchange (ref fromNative, 1);
			Dispose ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				managedDrawables.TryRemove (Handle, out var managedDrawable);
			}

			if (Interlocked.CompareExchange (ref fromNative, 0, 0) == 0 && Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_manageddrawable_destroy (Handle);
			}

			base.Dispose (disposing);
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

		public SKPicture Snapshot () =>
			GetObject<SKPicture> (SkiaApi.sk_drawable_new_picture_snapshot (Handle));

		public void NotifyDrawingChanged () =>
			SkiaApi.sk_drawable_notify_drawing_changed (Handle);

		protected virtual void OnDraw (SKCanvas canvas)
		{
		}

		protected virtual SKRect OnGetBounds () => new SKRect ();

		protected virtual SKPicture OnSnapshot ()
		{
			var recorder = new SKPictureRecorder ();
			var canvas = recorder.BeginRecording (Bounds);
			Draw (canvas, 0, 0);
			return recorder.EndRecording ();
		}

		// unmanaged <-> managed methods (static for iOS)

		[MonoPInvokeCallback (typeof (draw_delegate))]
		private static void DrawInternal (IntPtr managedDrawablePtr, IntPtr canvas)
		{
			AsManagedDrawable (managedDrawablePtr).OnDraw (GetObject<SKCanvas> (canvas));
		}

		[MonoPInvokeCallback (typeof (getBounds_delegate))]
		private static void GetBoundsInternal (IntPtr managedDrawablePtr, out SKRect rect)
		{
			rect = AsManagedDrawable (managedDrawablePtr).OnGetBounds ();
		}

		[MonoPInvokeCallback (typeof (newPictureSnapshot_delegate))]
		private static IntPtr NewPictureSnapshotInternal (IntPtr managedDrawablePtr)
		{
			return AsManagedDrawable (managedDrawablePtr).OnSnapshot ().Handle;
		}

		private static SKDrawable AsManagedDrawable (IntPtr ptr)
		{
			if (AsManagedDrawable (ptr, out var target)) {
				return target;
			}
			throw new ObjectDisposedException ("SKDrawable: " + ptr);
		}

		private static bool AsManagedDrawable (IntPtr ptr, out SKDrawable target)
		{
			if (managedDrawables.TryGetValue (ptr, out target)) {
				return true;
			}
			target = null;
			return false;
		}
	}
}
