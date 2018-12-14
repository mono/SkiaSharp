using System;
using System.Runtime.InteropServices;
using System.Threading;

using NativePointerDictionary = System.Collections.Concurrent.ConcurrentDictionary<System.IntPtr, SkiaSharp.SKAbstractDrawable>;

namespace SkiaSharp
{
	public abstract class SKAbstractDrawable : SKObject
	{
		private static readonly NativePointerDictionary managedDrawables = new NativePointerDictionary ();

		// delegate declarations
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate IntPtr draw_delegate (IntPtr managedDrawablePtr, IntPtr canvasPtr);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate IntPtr getBounds_delegate (IntPtr managedDrawablePtr);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate IntPtr newPictureSnapshot_delegate (IntPtr managedDrawablePtr);

		// delegate fields
		private static readonly draw_delegate fDraw;
		private static readonly getBounds_delegate fGetBounds;
		private static readonly newPictureSnapshot_delegate fNewPictureSnapshot;

		private int fromNative;

		static SKAbstractDrawable ()
		{
			fDraw = new draw_delegate (DrawInternal);
			fGetBounds = new getBounds_delegate (GetBoundsInternal);
			fNewPictureSnapshot = new newPictureSnapshot_delegate (NewPictureSnapshotInternal);

			SkiaApi.sk_manageddrawable_set_delegates (
				Marshal.GetFunctionPointerForDelegate (fDraw),
				Marshal.GetFunctionPointerForDelegate (fGetBounds),
				Marshal.GetFunctionPointerForDelegate (fNewPictureSnapshot));
		}

		protected SKAbstractDrawable ()
			: this (true)
		{
		}

		protected SKAbstractDrawable (bool owns)
			: base (SkiaApi.sk_manageddrawable_new (), owns)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKAbstractDrawable instance.");
			}

			managedDrawables.TryAdd (Handle, this);
		}

		[Preserve]
		internal SKAbstractDrawable (IntPtr x, bool owns)
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
				SkiaApi.sk_manageddrawable_unref (Handle);
			}

			base.Dispose (disposing);
		}

		protected abstract void OnDraw (SKCanvas canvas);

		protected abstract SKRect OnGetBounds ();

		protected abstract SKPicture OnNewPictureSnapshot ();

		// unmanaged <-> managed methods (static for iOS)

		[MonoPInvokeCallback (typeof (draw_delegate))]
		private static IntPtr DrawInternal (IntPtr managedDrawablePtr, IntPtr canvas)
		{
			AsManagedDrawable (managedDrawablePtr).OnDraw (GetObject<SKCanvas> (canvas));

			return IntPtr.Zero;
		}

		[MonoPInvokeCallback(typeof(getBounds_delegate))]
		private static IntPtr GetBoundsInternal (IntPtr managedDrawablePtr)
		{
			return AsManagedDrawable (managedDrawablePtr).OnGetBounds ();
		}

		[MonoPInvokeCallback(typeof(newPictureSnapshot_delegate))]
		private static IntPtr NewPictureSnapshotInternal (IntPtr managedDrawablePtr)
		{
			return AsManagedDrawable (managedDrawablePtr).OnNewPictureSnapshot ().Handle;
		}

		private static SKAbstractDrawable AsManagedDrawable (IntPtr ptr)
		{
			if (AsManagedDrawable (ptr, out var target)) {
				return target;
			}
			throw new ObjectDisposedException ("SKAbstractDrawable: " + ptr);
		}

		private static bool AsManagedDrawable (IntPtr ptr, out SKAbstractDrawable target)
		{
			if (managedDrawables.TryGetValue (ptr, out target)) {
				return true;
			}
			target = null;
			return false;
		}
	}
}
