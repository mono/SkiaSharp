//
// Bindings for SKStream
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2015 Xamarin Inc
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
		internal delegate IntPtr  read_delegate         (IntPtr managedStreamPtr, IntPtr buffer, IntPtr size);
		internal delegate bool    isAtEnd_delegate      (IntPtr managedStreamPtr);
		internal delegate bool    rewind_delegate       (IntPtr managedStreamPtr);
		internal delegate IntPtr  getPosition_delegate  (IntPtr managedStreamPtr);
		internal delegate bool    seek_delegate         (IntPtr managedStreamPtr, IntPtr position);
		internal delegate bool    move_delegate         (IntPtr managedStreamPtr, long offset);
		internal delegate IntPtr  getLength_delegate    (IntPtr managedStreamPtr);
		internal delegate IntPtr  createNew_delegate    (IntPtr managedStreamPtr);
		internal delegate void    destroy_delegate      (IntPtr managedStreamPtr);

		// delegate fields
		private static readonly IntPtr fRead;
		private static readonly IntPtr fIsAtEnd;
		private static readonly IntPtr fRewind;
		private static readonly IntPtr fGetPosition;
		private static readonly IntPtr fSeek;
		private static readonly IntPtr fMove;
		private static readonly IntPtr fGetLength;
		private static readonly IntPtr fCreateNew;
		private static readonly IntPtr fDestroy;

		private readonly Stream stream;

		static SKManagedStream()
		{
			fRead = Marshal.GetFunctionPointerForDelegate(new read_delegate(ReadInternal));
			fIsAtEnd = Marshal.GetFunctionPointerForDelegate (new isAtEnd_delegate(IsAtEndInternal));
			fRewind = Marshal.GetFunctionPointerForDelegate (new rewind_delegate(RewindInternal));
			fGetPosition = Marshal.GetFunctionPointerForDelegate (new getPosition_delegate(GetPositionInternal));
			fSeek = Marshal.GetFunctionPointerForDelegate (new seek_delegate(SeekInternal));
			fMove = Marshal.GetFunctionPointerForDelegate (new move_delegate(MoveInternal));
			fGetLength = Marshal.GetFunctionPointerForDelegate (new getLength_delegate(GetLengthInternal));
			fGetLength = Marshal.GetFunctionPointerForDelegate (new getLength_delegate(GetLengthInternal));
			fCreateNew = Marshal.GetFunctionPointerForDelegate (new createNew_delegate(CreateNewInternal));
			fDestroy = Marshal.GetFunctionPointerForDelegate (new destroy_delegate(DestroyInternal));

			SkiaApi.sk_managedstream_set_delegates (fRead, fIsAtEnd, fRewind, fGetPosition, fSeek, fMove, fGetLength, fCreateNew, fDestroy);
		}

		public SKManagedStream (Stream managedStream)
			: base (SkiaApi.sk_managedstream_new (), true)
		{
			managedStreams.Add (handle, new WeakReference<SKManagedStream>(this));

			stream = managedStream;
		}

		protected override void Dispose (bool disposing)
		{
			if (managedStreams.ContainsKey(handle)) {
				managedStreams.Remove (handle);
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
			Marshal.Copy (managedBuffer, 0, buffer, count);
			return (IntPtr)result;
		}
		#if __IOS__
		[MonoPInvokeCallback(typeof(isAtEnd_delegate))]
		#endif
		private static bool IsAtEndInternal (IntPtr managedStreamPtr)
		{
			var managedStream = AsManagedStream (managedStreamPtr);
			return managedStream.stream.Position >= managedStream.stream.Length;
		}
		#if __IOS__
		[MonoPInvokeCallback(typeof(rewind_delegate))]
		#endif
		private static bool RewindInternal (IntPtr managedStreamPtr)
		{
			var managedStream = AsManagedStream (managedStreamPtr);
			managedStream.stream.Position = 0;
			return true;
		}
		#if __IOS__
		[MonoPInvokeCallback(typeof(getPosition_delegate))]
		#endif
		private static IntPtr GetPositionInternal (IntPtr managedStreamPtr)
		{
			var managedStream = AsManagedStream (managedStreamPtr);
			return (IntPtr)managedStream.stream.Position;
		}
		#if __IOS__
		[MonoPInvokeCallback(typeof(seek_delegate))]
		#endif
		private static bool SeekInternal (IntPtr managedStreamPtr, IntPtr position)
		{
			var managedStream = AsManagedStream (managedStreamPtr);
			managedStream.stream.Position = (long)position;
			return true;
		}
		#if __IOS__
		[MonoPInvokeCallback(typeof(move_delegate))]
		#endif
		private static bool MoveInternal (IntPtr managedStreamPtr, long offset)
		{
			var managedStream = AsManagedStream (managedStreamPtr);
			managedStream.stream.Position = managedStream.stream.Position + offset;
			return true;
		}
		#if __IOS__
		[MonoPInvokeCallback(typeof(getLength_delegate))]
		#endif
		private static IntPtr GetLengthInternal (IntPtr managedStreamPtr)
		{
			var managedStream = AsManagedStream (managedStreamPtr);
			return (IntPtr)managedStream.stream.Length;
		}
		#if __IOS__
		[MonoPInvokeCallback(typeof(createNew_delegate))]
		#endif
		private static IntPtr CreateNewInternal (IntPtr managedStreamPtr)
		{
			var managedStream = AsManagedStream (managedStreamPtr);
			var newStream = new SKManagedStream (managedStream.stream);
			return newStream.handle;
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
			if (managedStreams.TryGetValue (ptr, out weak)) {
				if (weak.TryGetTarget(out target)) {
					return true;
				}
			}
			target = null;
			return false;
		}
	}
}
