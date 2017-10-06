//
// Bindings for SKManagedWStream
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2017 Xamarin Inc
//
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;

#if __IOS__
using ObjCRuntime;
#endif

namespace SkiaSharp
{
	public class SKManagedWStream : SKWStream
	{
		private static readonly ConcurrentDictionary<IntPtr, SKManagedWStream> managedStreams = new ConcurrentDictionary<IntPtr, SKManagedWStream> ();

		// delegate declarations
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate bool    write_delegate        (IntPtr managedStreamPtr, IntPtr buffer, IntPtr size);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void    flush_delegate        (IntPtr managedStreamPtr);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr  bytesWritten_delegate (IntPtr managedStreamPtr);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void    destroy_delegate      (IntPtr managedStreamPtr);

		// delegate fields
		private static readonly write_delegate fWrite;
		private static readonly flush_delegate fFlush;
		private static readonly bytesWritten_delegate fBytesWritten;
		private static readonly destroy_delegate fDestroy;

		private Stream stream;
		private int fromNative;
		private readonly bool disposeStream;

		static SKManagedWStream()
		{
			fWrite = new write_delegate(WriteInternal);
			fFlush = new flush_delegate(FlushInternal);
			fBytesWritten = new bytesWritten_delegate(BytesWrittenInternal);
			fDestroy = new destroy_delegate(DestroyInternal);

			SkiaApi.sk_managedwstream_set_delegates(
				Marshal.GetFunctionPointerForDelegate(fWrite), 
				Marshal.GetFunctionPointerForDelegate(fFlush),
				Marshal.GetFunctionPointerForDelegate(fBytesWritten),
				Marshal.GetFunctionPointerForDelegate(fDestroy));
		}

		public SKManagedWStream (Stream managedStream)
			: this (managedStream, false)
		{
		}

		public SKManagedWStream (Stream managedStream, bool disposeManagedStream)
			: this (managedStream, disposeManagedStream, true)
		{
		}

		private SKManagedWStream (Stream managedStream, bool disposeManagedStream, bool owns)
			: base (SkiaApi.sk_managedwstream_new (), owns)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKManagedWStream instance.");
			}

			managedStreams.TryAdd (Handle, this);

			stream = managedStream;
			disposeStream = disposeManagedStream;
		}

		void DisposeFromNative ()
		{
			Interlocked.Exchange (ref fromNative, 1);
			Dispose ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				SKManagedWStream managedStream;
				managedStreams.TryRemove (Handle, out managedStream);

				if (disposeStream && stream != null) {
					stream.Dispose ();
					stream = null;
				}
			}

			if (Interlocked.CompareExchange (ref fromNative, 0, 0) == 0 && Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_managedwstream_destroy (Handle);
			}

			base.Dispose (disposing);
		}

		// unmanaged <-> managed methods (static for iOS)

		#if __IOS__
		[MonoPInvokeCallback (typeof (write_delegate))]
		#endif
		private static bool WriteInternal (IntPtr managedStreamPtr, IntPtr buffer, IntPtr size)
		{
			var managedStream = AsManagedStream (managedStreamPtr);
			var count = (int)size;
			var managedBuffer = new byte[count];
			if (buffer != IntPtr.Zero) { 
				Marshal.Copy (buffer, managedBuffer, 0, count);
			}
			managedStream.stream.Write (managedBuffer, 0, count);
			return true;
		}
		#if __IOS__
		[MonoPInvokeCallback (typeof (flush_delegate))]
		#endif
		private static void FlushInternal (IntPtr managedStreamPtr)
		{
			var managedStream = AsManagedStream (managedStreamPtr);
			managedStream.stream.Flush ();
		}
		#if __IOS__
		[MonoPInvokeCallback (typeof (bytesWritten_delegate))]
		#endif
		private static IntPtr BytesWrittenInternal (IntPtr managedStreamPtr)
		{
			var managedStream = AsManagedStream (managedStreamPtr);
			return (IntPtr)managedStream.stream.Position;
		}
		#if __IOS__
		[MonoPInvokeCallback (typeof (destroy_delegate))]
		#endif
		private static void DestroyInternal (IntPtr managedStreamPtr)
		{
			SKManagedWStream managedStream;
			if (AsManagedStream (managedStreamPtr, out managedStream)) {
				managedStream.DisposeFromNative ();
			}
		}
		private static SKManagedWStream AsManagedStream (IntPtr ptr)
		{
			SKManagedWStream target;
			if (AsManagedStream (ptr, out target)) {
				return target;
			}
			throw new ObjectDisposedException ("SKManagedWStream: " + ptr);
		}
		private static bool AsManagedStream (IntPtr ptr, out SKManagedWStream target)
		{
			if (managedStreams.TryGetValue (ptr, out target)) {
				return true;
			}
			target = null;
			return false;
		}
	}
}