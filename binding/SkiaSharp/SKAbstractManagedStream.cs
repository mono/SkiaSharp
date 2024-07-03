#nullable disable

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp
{
	public unsafe abstract class SKAbstractManagedStream : SKStreamAsset
	{
		private static readonly SKManagedStreamDelegates delegates;

		private int fromNative;

		static SKAbstractManagedStream ()
		{
			delegates = new SKManagedStreamDelegates {
#if USE_LIBRARY_IMPORT
				fRead = &ReadInternal,
				fPeek = &PeekInternal,
				fIsAtEnd = &IsAtEndInternal,
				fHasPosition = &HasPositionInternal,
				fHasLength = &HasLengthInternal,
				fRewind = &RewindInternal,
				fGetPosition = &GetPositionInternal,
				fSeek = &SeekInternal,
				fMove = &MoveInternal,
				fGetLength = &GetLengthInternal,
				fDuplicate = &DuplicateInternal,
				fFork = &ForkInternal,
				fDestroy = &DestroyInternal,
#else
				fRead = ReadInternal,
				fPeek = PeekInternal,
				fIsAtEnd = IsAtEndInternal,
				fHasPosition = HasPositionInternal,
				fHasLength = HasLengthInternal,
				fRewind = RewindInternal,
				fGetPosition = GetPositionInternal,
				fSeek = SeekInternal,
				fMove = MoveInternal,
				fGetLength = GetLengthInternal,
				fDuplicate = DuplicateInternal,
				fFork = ForkInternal,
				fDestroy = DestroyInternal,
#endif
			};
			SkiaApi.sk_managedstream_set_procs (delegates);
		}

		protected SKAbstractManagedStream ()
			: this (true)
		{
		}

		protected SKAbstractManagedStream (bool owns)
			: base (IntPtr.Zero, owns)
		{
			var ctx = DelegateProxies.CreateUserData (this, true);
			Handle = SkiaApi.sk_managedstream_new ((void*)ctx);
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative ()
		{
			if (Interlocked.CompareExchange (ref fromNative, 0, 0) == 0)
				SkiaApi.sk_managedstream_destroy (Handle);
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

		protected virtual IntPtr OnFork ()
		{
			var stream = OnCreateNew ();
			SkiaApi.sk_stream_seek (stream, SkiaApi.sk_stream_get_position (Handle));
			return stream;
		}

		protected virtual IntPtr OnDuplicate () => OnCreateNew ();

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (SKManagedStreamReadProxyDelegate))]
#endif
		private static IntPtr ReadInternal (IntPtr s, void* context, void* buffer, IntPtr size)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnRead ((IntPtr)buffer, size);
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (SKManagedStreamPeekProxyDelegate))]
#endif
		private static IntPtr PeekInternal (IntPtr s, void* context, void* buffer, IntPtr size)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnPeek ((IntPtr)buffer, size);
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (SKManagedStreamIsAtEndProxyDelegate))]
#endif
		private static bool IsAtEndInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnIsAtEnd ();
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (SKManagedStreamHasPositionProxyDelegate))]
#endif
		private static bool HasPositionInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnHasPosition ();
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (SKManagedStreamHasLengthProxyDelegate))]
#endif
		private static bool HasLengthInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnHasLength ();
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (SKManagedStreamRewindProxyDelegate))]
#endif
		private static bool RewindInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnRewind ();
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (SKManagedStreamGetPositionProxyDelegate))]
#endif
		private static IntPtr GetPositionInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnGetPosition ();
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (SKManagedStreamSeekProxyDelegate))]
#endif
		private static bool SeekInternal (IntPtr s, void* context, IntPtr position)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnSeek (position);
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (SKManagedStreamMoveProxyDelegate))]
#endif
		private static bool MoveInternal (IntPtr s, void* context, int offset)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnMove (offset);
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (SKManagedStreamGetLengthProxyDelegate))]
#endif
		private static IntPtr GetLengthInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnGetLength ();
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (SKManagedStreamDuplicateProxyDelegate))]
#endif
		private static IntPtr DuplicateInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnDuplicate ();
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (SKManagedStreamForkProxyDelegate))]
#endif
		private static IntPtr ForkInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnFork ();
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (SKManagedStreamDestroyProxyDelegate))]
#endif
		private static void DestroyInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out var gch);
			if (stream != null) {
				Interlocked.Exchange (ref stream.fromNative, 1);
				stream.Dispose ();
			}
			gch.Free ();
		}
	}
}
