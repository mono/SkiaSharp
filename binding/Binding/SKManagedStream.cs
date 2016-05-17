//
// Bindings for SKStream
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2016 Xamarin Inc
//
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

#if __IOS__
using ObjCRuntime;
#endif

namespace SkiaSharp
{
	public class SKManagedStream : SKStreamAsset
	{
		private static readonly Dictionary<IntPtr, WeakReference<SKManagedStream>> managedStreams = new Dictionary<IntPtr, WeakReference<SKManagedStream>>();

		// delegate declarations
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr  read_delegate         (IntPtr managedStreamPtr, IntPtr buffer, IntPtr size);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr  peek_delegate         (IntPtr managedStreamPtr, IntPtr buffer, IntPtr size);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate bool    isAtEnd_delegate      (IntPtr managedStreamPtr);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate bool    rewind_delegate       (IntPtr managedStreamPtr);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr  getPosition_delegate  (IntPtr managedStreamPtr);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate bool    seek_delegate         (IntPtr managedStreamPtr, IntPtr position);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate bool    move_delegate         (IntPtr managedStreamPtr, long offset);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr  getLength_delegate    (IntPtr managedStreamPtr);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr  createNew_delegate    (IntPtr managedStreamPtr);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void    destroy_delegate      (IntPtr managedStreamPtr);

		// delegate fields
		private static readonly read_delegate fRead;
		private static readonly peek_delegate fPeek;
		private static readonly isAtEnd_delegate fIsAtEnd;
		private static readonly rewind_delegate fRewind;
		private static readonly getPosition_delegate fGetPosition;
		private static readonly seek_delegate fSeek;
		private static readonly move_delegate fMove;
		private static readonly getLength_delegate fGetLength;
		private static readonly createNew_delegate fCreateNew;
		private static readonly destroy_delegate fDestroy;

		private readonly Stream stream;
		private readonly bool disposeStream;

		static SKManagedStream()
		{
			fRead = new read_delegate(ReadInternal);
			fPeek = new peek_delegate(PeekInternal);
			fIsAtEnd = new isAtEnd_delegate(IsAtEndInternal);
			fRewind = new rewind_delegate(RewindInternal);
			fGetPosition = new getPosition_delegate(GetPositionInternal);
			fSeek = new seek_delegate(SeekInternal);
			fMove = new move_delegate(MoveInternal);
			fGetLength = new getLength_delegate(GetLengthInternal);
			fCreateNew = new createNew_delegate(CreateNewInternal);
			fDestroy = new destroy_delegate(DestroyInternal);

			SkiaApi.sk_managedstream_set_delegates(
				Marshal.GetFunctionPointerForDelegate(fRead), 
				Marshal.GetFunctionPointerForDelegate(fPeek), 
				Marshal.GetFunctionPointerForDelegate(fIsAtEnd), 
				Marshal.GetFunctionPointerForDelegate(fRewind),
				Marshal.GetFunctionPointerForDelegate(fGetPosition),
				Marshal.GetFunctionPointerForDelegate(fSeek),
				Marshal.GetFunctionPointerForDelegate(fMove),
				Marshal.GetFunctionPointerForDelegate(fGetLength),
				Marshal.GetFunctionPointerForDelegate(fCreateNew),
				Marshal.GetFunctionPointerForDelegate(fDestroy));
		}

		public SKManagedStream (Stream managedStream)
			: this (managedStream, false)
		{
		}

		public SKManagedStream (Stream managedStream, bool disposeManagedStream)
			: base (SkiaApi.sk_managedstream_new ())
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKManagedStream instance.");
			}

			lock (managedStreams)
				managedStreams.Add (Handle, new WeakReference<SKManagedStream>(this));

			stream = managedStream;
			disposeStream = disposeManagedStream;
		}

		protected override void Dispose (bool disposing)
		{
			lock (managedStreams){
				if (managedStreams.ContainsKey(Handle)) {
					managedStreams.Remove (Handle);
				}
			}

			if (disposeStream && stream != null) {
				stream.Dispose ();
			}

			base.Dispose (disposing);
		}

		// unmanaged <-> managed methods (static for iOS)

		#if __IOS__
		[MonoPInvokeCallback(typeof(read_delegate))]
		#endif
		private static IntPtr ReadInternal (IntPtr managedStreamPtr, IntPtr buffer, IntPtr size)
		{
			var managedStream = AsManagedStream (managedStreamPtr);
			var count = (int)size;
			var managedBuffer = new byte[count];
			var result = managedStream.stream.Read (managedBuffer, 0, count);
			if (buffer != IntPtr.Zero) { 
				Marshal.Copy (managedBuffer, 0, buffer, count);
			}
			return (IntPtr)result;
		}
		#if __IOS__
		[MonoPInvokeCallback(typeof(peek_delegate))]
		#endif
		private static IntPtr PeekInternal (IntPtr managedStreamPtr, IntPtr buffer, IntPtr size)
		{
			var managedStream = AsManagedStream (managedStreamPtr);
			if (!managedStream.stream.CanSeek) {
				return (IntPtr)0;
			}
			var oldPos = managedStream.stream.Position;
			var count = (int)size;
			var managedBuffer = new byte[count];
			var result = managedStream.stream.Read (managedBuffer, 0, count);
			if (buffer != IntPtr.Zero) { 
				Marshal.Copy (managedBuffer, 0, buffer, count);
			}
			managedStream.stream.Position = oldPos;
			return (IntPtr)result;
		}
		#if __IOS__
		[MonoPInvokeCallback(typeof(isAtEnd_delegate))]
		#endif
		private static bool IsAtEndInternal (IntPtr managedStreamPtr)
		{
			var managedStream = AsManagedStream (managedStreamPtr);
			if (!managedStream.stream.CanSeek) {
				throw new NotSupportedException ("Unable to detect the End Of Stream if the stream is not seekable.");
			}
			return managedStream.stream.Position >= managedStream.stream.Length;
		}
		#if __IOS__
		[MonoPInvokeCallback(typeof(rewind_delegate))]
		#endif
		private static bool RewindInternal (IntPtr managedStreamPtr)
		{
			var managedStream = AsManagedStream (managedStreamPtr);
			if (!managedStream.stream.CanSeek) {
				return false;
			}
			managedStream.stream.Position = 0;
			return true;
		}
		#if __IOS__
		[MonoPInvokeCallback(typeof(getPosition_delegate))]
		#endif
		private static IntPtr GetPositionInternal (IntPtr managedStreamPtr)
		{
			var managedStream = AsManagedStream (managedStreamPtr);
			if (!managedStream.stream.CanSeek) {
				return (IntPtr)0;
			}
			return (IntPtr)managedStream.stream.Position;
		}
		#if __IOS__
		[MonoPInvokeCallback(typeof(seek_delegate))]
		#endif
		private static bool SeekInternal (IntPtr managedStreamPtr, IntPtr position)
		{
			var managedStream = AsManagedStream (managedStreamPtr);
			if (!managedStream.stream.CanSeek) {
				return false;
			}
			managedStream.stream.Position = (long)position;
			return true;
		}
		#if __IOS__
		[MonoPInvokeCallback(typeof(move_delegate))]
		#endif
		private static bool MoveInternal (IntPtr managedStreamPtr, long offset)
		{
			var managedStream = AsManagedStream (managedStreamPtr);
			if (!managedStream.stream.CanSeek) {
				return false;
			}
			managedStream.stream.Position = managedStream.stream.Position + offset;
			return true;
		}
		#if __IOS__
		[MonoPInvokeCallback(typeof(getLength_delegate))]
		#endif
		private static IntPtr GetLengthInternal (IntPtr managedStreamPtr)
		{
			var managedStream = AsManagedStream (managedStreamPtr);
			if (!managedStream.stream.CanSeek) {
				return (IntPtr)0;
			}
			return (IntPtr)managedStream.stream.Length;
		}
		#if __IOS__
		[MonoPInvokeCallback(typeof(createNew_delegate))]
		#endif
		private static IntPtr CreateNewInternal (IntPtr managedStreamPtr)
		{
			var managedStream = AsManagedStream (managedStreamPtr);
			var newStream = new SKManagedStream (managedStream.stream);
			return newStream.Handle;
		}
		#if __IOS__
		[MonoPInvokeCallback(typeof(destroy_delegate))]
		#endif
		private static void DestroyInternal (IntPtr managedStreamPtr)
		{
			SKManagedStream managedStream;
			if (AsManagedStream (managedStreamPtr, out managedStream)) {
				managedStream.Dispose ();
			} else {
				Debug.WriteLine ("Destroying disposed SKManagedStream: " + managedStreamPtr);
			}
		}
		private static SKManagedStream AsManagedStream(IntPtr ptr)
		{
			SKManagedStream target;
			if (AsManagedStream (ptr, out target)) {
				return target;
			}
			throw new ObjectDisposedException ("SKManagedStream: " + ptr);
		}
		private static bool AsManagedStream(IntPtr ptr, out SKManagedStream target)
		{
			WeakReference<SKManagedStream> weak;
			lock (managedStreams){
				if (managedStreams.TryGetValue (ptr, out weak)) {
					if (weak.TryGetTarget(out target)) {
						return true;
					}
				}
			}
			target = null;
			return false;
		}
	}
}
