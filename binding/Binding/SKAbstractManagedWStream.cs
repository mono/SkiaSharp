using System;
using System.Runtime.InteropServices;
using System.Threading;

using NativePointerDictionary = System.Collections.Concurrent.ConcurrentDictionary<System.IntPtr, SkiaSharp.SKAbstractManagedWStream>;

namespace SkiaSharp
{
	public abstract class SKAbstractManagedWStream : SKWStream
	{
		private static readonly NativePointerDictionary managedStreams = new NativePointerDictionary ();

		// delegate declarations
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate bool write_delegate (IntPtr managedStreamPtr, IntPtr buffer, IntPtr size);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate void flush_delegate (IntPtr managedStreamPtr);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate IntPtr bytesWritten_delegate(IntPtr managedStreamPtr);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate void destroy_delegate (IntPtr managedStreamPtr);

		// delegate fields
		private static readonly write_delegate fWrite;
		private static readonly flush_delegate fFlush;
		private static readonly bytesWritten_delegate fBytesWritten;
		private static readonly destroy_delegate fDestroy;

		private int fromNative;

		static SKAbstractManagedWStream ()
		{
			fWrite = new write_delegate (WriteInternal);
			fFlush = new flush_delegate (FlushInternal);
			fBytesWritten = new bytesWritten_delegate (BytesWrittenInternal);
			fDestroy = new destroy_delegate (DestroyInternal);

			SkiaApi.sk_managedwstream_set_delegates (
				Marshal.GetFunctionPointerForDelegate (fWrite), 
				Marshal.GetFunctionPointerForDelegate (fFlush),
				Marshal.GetFunctionPointerForDelegate (fBytesWritten),
				Marshal.GetFunctionPointerForDelegate (fDestroy));
		}

		protected SKAbstractManagedWStream ()
			: this (true)
		{
		}

		protected SKAbstractManagedWStream (bool owns)
			: base (SkiaApi.sk_managedwstream_new (), owns)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKAbstractManagedWStream instance.");
			}

			managedStreams.TryAdd (Handle, this);
		}

		private void DisposeFromNative ()
		{
			Interlocked.Exchange (ref fromNative, 1);
			Dispose ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				managedStreams.TryRemove (Handle, out var managedStream);
			}

			if (Interlocked.CompareExchange (ref fromNative, 0, 0) == 0 && Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_managedwstream_destroy (Handle);
			}

			base.Dispose (disposing);
		}

		protected abstract bool OnWrite (IntPtr buffer, IntPtr size);

		protected abstract void OnFlush ();

		protected abstract IntPtr OnBytesWritten ();

		// unmanaged <-> managed methods (static for iOS)

		[MonoPInvokeCallback (typeof (write_delegate))]
		private static bool WriteInternal (IntPtr managedStreamPtr, IntPtr buffer, IntPtr size)
		{
			return AsManagedStream (managedStreamPtr).OnWrite (buffer, size);
		}

		[MonoPInvokeCallback (typeof (flush_delegate))]
		private static void FlushInternal (IntPtr managedStreamPtr)
		{
			AsManagedStream (managedStreamPtr).OnFlush ();
		}

		[MonoPInvokeCallback (typeof (bytesWritten_delegate))]
		private static IntPtr BytesWrittenInternal (IntPtr managedStreamPtr)
		{
			return AsManagedStream (managedStreamPtr).OnBytesWritten ();
		}

		[MonoPInvokeCallback (typeof (destroy_delegate))]
		private static void DestroyInternal (IntPtr managedStreamPtr)
		{
			if (AsManagedStream (managedStreamPtr, out var managedStream)) {
				managedStream.DisposeFromNative ();
			}
		}

		private static SKAbstractManagedWStream AsManagedStream (IntPtr ptr)
		{
			if (AsManagedStream (ptr, out var target)) {
				return target;
			}
			throw new ObjectDisposedException ("SKAbstractManagedWStream: " + ptr);
		}

		private static bool AsManagedStream (IntPtr ptr, out SKAbstractManagedWStream target)
		{
			if (managedStreams.TryGetValue (ptr, out target)) {
				return true;
			}
			target = null;
			return false;
		}
	}
}
