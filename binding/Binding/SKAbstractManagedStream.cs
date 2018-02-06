//
// SKAbstractManagedStream
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2017 Xamarin Inc
//
using System;
using System.Runtime.InteropServices;
using System.Threading;

#if __IOS__
using ObjCRuntime;
#endif

using NativePointerDictionary = System.Collections.Concurrent.ConcurrentDictionary<System.IntPtr, SkiaSharp.SKAbstractManagedStream>;

namespace SkiaSharp
{
	public abstract class SKAbstractManagedStream : SKStreamAsset
	{
		private static readonly NativePointerDictionary managedStreams = new NativePointerDictionary ();

		// delegate declarations
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate IntPtr read_delegate (IntPtr managedStreamPtr, IntPtr buffer, IntPtr size);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate IntPtr peek_delegate (IntPtr managedStreamPtr, IntPtr buffer, IntPtr size);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate bool isAtEnd_delegate (IntPtr managedStreamPtr);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate bool hasPosition_delegate (IntPtr managedStreamPtr);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate bool hasLength_delegate (IntPtr managedStreamPtr);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate bool rewind_delegate (IntPtr managedStreamPtr);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate IntPtr getPosition_delegate (IntPtr managedStreamPtr);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate bool seek_delegate (IntPtr managedStreamPtr, IntPtr position);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate bool move_delegate (IntPtr managedStreamPtr, int offset);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate IntPtr getLength_delegate (IntPtr managedStreamPtr);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate IntPtr createNew_delegate (IntPtr managedStreamPtr);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate void destroy_delegate (IntPtr managedStreamPtr);

		// delegate fields
		private static readonly read_delegate fRead;
		private static readonly peek_delegate fPeek;
		private static readonly isAtEnd_delegate fIsAtEnd;
		private static readonly hasPosition_delegate fHasPosition;
		private static readonly hasLength_delegate fHasLength;
		private static readonly rewind_delegate fRewind;
		private static readonly getPosition_delegate fGetPosition;
		private static readonly seek_delegate fSeek;
		private static readonly move_delegate fMove;
		private static readonly getLength_delegate fGetLength;
		private static readonly createNew_delegate fCreateNew;
		private static readonly destroy_delegate fDestroy;

		private int fromNative;

		static SKAbstractManagedStream ()
		{
			fRead = new read_delegate (ReadInternal);
			fPeek = new peek_delegate (PeekInternal);
			fIsAtEnd = new isAtEnd_delegate (IsAtEndInternal);
			fHasPosition = new hasPosition_delegate (HasPositionInternal);
			fHasLength = new hasLength_delegate (HasLengthInternal);
			fRewind = new rewind_delegate (RewindInternal);
			fGetPosition = new getPosition_delegate (GetPositionInternal);
			fSeek = new seek_delegate (SeekInternal);
			fMove = new move_delegate (MoveInternal);
			fGetLength = new getLength_delegate (GetLengthInternal);
			fCreateNew = new createNew_delegate (CreateNewInternal);
			fDestroy = new destroy_delegate (DestroyInternal);

			SkiaApi.sk_managedstream_set_delegates (
				Marshal.GetFunctionPointerForDelegate (fRead),
				Marshal.GetFunctionPointerForDelegate (fPeek),
				Marshal.GetFunctionPointerForDelegate (fIsAtEnd),
				Marshal.GetFunctionPointerForDelegate (fHasPosition),
				Marshal.GetFunctionPointerForDelegate (fHasLength),
				Marshal.GetFunctionPointerForDelegate (fRewind),
				Marshal.GetFunctionPointerForDelegate (fGetPosition),
				Marshal.GetFunctionPointerForDelegate (fSeek),
				Marshal.GetFunctionPointerForDelegate (fMove),
				Marshal.GetFunctionPointerForDelegate (fGetLength),
				Marshal.GetFunctionPointerForDelegate (fCreateNew),
				Marshal.GetFunctionPointerForDelegate (fDestroy));
		}

		protected SKAbstractManagedStream ()
			: this (true)
		{
		}

		protected SKAbstractManagedStream (bool owns)
			: base (SkiaApi.sk_managedstream_new (), owns)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKAbstractManagedStream instance.");
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
				SKAbstractManagedStream managedStream;
				managedStreams.TryRemove (Handle, out managedStream);
			}

			if (Interlocked.CompareExchange (ref fromNative, 0, 0) == 0 && Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_managedstream_destroy (Handle);
			}

			base.Dispose (disposing);
		}

		protected abstract IntPtr OnRead (IntPtr buffer, IntPtr size);

		protected abstract IntPtr OnPeek (IntPtr buffer, IntPtr size);

		protected abstract bool OnIsAtEnd ();

		protected abstract bool OnHasPosition ();

		protected abstract bool OnHasLength ();

		protected abstract bool OnRewind ();

		protected abstract IntPtr OnGetPosition ();

		protected abstract IntPtr OnGetLength ();

		protected abstract bool OnSeek (IntPtr position);

		protected abstract bool OnMove (int offset);

		protected abstract IntPtr OnCreateNew ();

		// unmanaged <-> managed methods (static for iOS)

#if __IOS__
		[MonoPInvokeCallback (typeof (read_delegate))]
#endif
		private static IntPtr ReadInternal (IntPtr managedStreamPtr, IntPtr buffer, IntPtr size)
		{
			return AsManagedStream (managedStreamPtr).OnRead (buffer, size);
		}
#if __IOS__
		[MonoPInvokeCallback(typeof(peek_delegate))]
#endif
		private static IntPtr PeekInternal (IntPtr managedStreamPtr, IntPtr buffer, IntPtr size)
		{
			return AsManagedStream (managedStreamPtr).OnPeek (buffer, size);
		}
#if __IOS__
		[MonoPInvokeCallback(typeof(isAtEnd_delegate))]
#endif
		private static bool IsAtEndInternal (IntPtr managedStreamPtr)
		{
			return AsManagedStream (managedStreamPtr).OnIsAtEnd ();
		}
#if __IOS__
		[MonoPInvokeCallback(typeof(hasPosition_delegate))]
#endif
		private static bool HasPositionInternal (IntPtr managedStreamPtr)
		{
			return AsManagedStream (managedStreamPtr).OnHasPosition ();
		}
#if __IOS__
		[MonoPInvokeCallback(typeof(hasLength_delegate))]
#endif
		private static bool HasLengthInternal (IntPtr managedStreamPtr)
		{
			return AsManagedStream (managedStreamPtr).OnHasLength ();
		}
#if __IOS__
		[MonoPInvokeCallback(typeof(rewind_delegate))]
#endif
		private static bool RewindInternal (IntPtr managedStreamPtr)
		{
			return AsManagedStream (managedStreamPtr).OnRewind ();
		}
#if __IOS__
		[MonoPInvokeCallback(typeof(getPosition_delegate))]
#endif
		private static IntPtr GetPositionInternal (IntPtr managedStreamPtr)
		{
			return AsManagedStream (managedStreamPtr).OnGetPosition ();
		}
#if __IOS__
		[MonoPInvokeCallback(typeof(seek_delegate))]
#endif
		private static bool SeekInternal (IntPtr managedStreamPtr, IntPtr position)
		{
			return AsManagedStream (managedStreamPtr).OnSeek (position);
		}
#if __IOS__
		[MonoPInvokeCallback(typeof(move_delegate))]
#endif
		private static bool MoveInternal (IntPtr managedStreamPtr, int offset)
		{
			return AsManagedStream (managedStreamPtr).OnMove (offset);
		}
#if __IOS__
		[MonoPInvokeCallback(typeof(getLength_delegate))]
#endif
		private static IntPtr GetLengthInternal (IntPtr managedStreamPtr)
		{
			return AsManagedStream (managedStreamPtr).OnGetLength ();
		}
#if __IOS__
		[MonoPInvokeCallback(typeof(createNew_delegate))]
#endif
		private static IntPtr CreateNewInternal (IntPtr managedStreamPtr)
		{
			return AsManagedStream (managedStreamPtr).OnCreateNew ();
		}
#if __IOS__
		[MonoPInvokeCallback(typeof(destroy_delegate))]
#endif
		private static void DestroyInternal (IntPtr managedStreamPtr)
		{
			SKAbstractManagedStream managedStream;
			if (AsManagedStream (managedStreamPtr, out managedStream)) {
				managedStream.DisposeFromNative ();
			}
		}
		private static SKAbstractManagedStream AsManagedStream (IntPtr ptr)
		{
			SKAbstractManagedStream target;
			if (AsManagedStream (ptr, out target)) {
				return target;
			}
			throw new ObjectDisposedException ("SKAbstractManagedStream: " + ptr);
		}
		private static bool AsManagedStream (IntPtr ptr, out SKAbstractManagedStream target)
		{
			if (managedStreams.TryGetValue (ptr, out target)) {
				return true;
			}
			target = null;
			return false;
		}
	}
}
