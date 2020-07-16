using System;
using System.Threading;

namespace SkiaSharp
{
	public unsafe abstract class SKAbstractManagedStream : SKStreamAsset
	{
		private static readonly SKManagedStreamDelegates delegates;

		private int fromNative;

		static SKAbstractManagedStream ()
		{
#if __WASM__ && USE_INTPTR_DELEGATES
			var funcs = SkiaApi.BindWasmMembers<SKAbstractManagedStream> (new[] {
				(nameof (SKAbstractManagedStream.ReadInternal), "iiiii"),
				(nameof (SKAbstractManagedStream.PeekInternal), "iiiii"),
				(nameof (SKAbstractManagedStream.IsAtEndInternal), "iii"),
				(nameof (SKAbstractManagedStream.HasPositionInternal), "iii"),
				(nameof (SKAbstractManagedStream.HasLengthInternal), "iii"),
				(nameof (SKAbstractManagedStream.RewindInternal), "iii"),
				(nameof (SKAbstractManagedStream.GetPositionInternal), "iii"),
				(nameof (SKAbstractManagedStream.SeekInternal), "iiii"),
				(nameof (SKAbstractManagedStream.MoveInternal), "iiii"),
				(nameof (SKAbstractManagedStream.GetLengthInternal), "iii"),
				(nameof (SKAbstractManagedStream.DuplicateInternal), "iii"),
				(nameof (SKAbstractManagedStream.ForkInternal), "iii"),
				(nameof (SKAbstractManagedStream.DestroyInternal), "vii"),
			});

			var ReadInternal = funcs[0];
			var PeekInternal = funcs[1];
			var IsAtEndInternal = funcs[2];
			var HasPositionInternal = funcs[3];
			var HasLengthInternal = funcs[4];
			var RewindInternal = funcs[5];
			var GetPositionInternal = funcs[6];
			var SeekInternal = funcs[7];
			var MoveInternal = funcs[8];
			var GetLengthInternal = funcs[9];
			var DuplicateInternal = funcs[10];
			var ForkInternal = funcs[11];
			var DestroyInternal = funcs[12];
#endif

			delegates = new SKManagedStreamDelegates {
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

		[MonoPInvokeCallback (typeof (SKManagedStreamReadProxyDelegate))]
		private static IntPtr ReadInternal (IntPtr s, void* context, void* buffer, IntPtr size)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnRead ((IntPtr)buffer, size);
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamPeekProxyDelegate))]
		private static IntPtr PeekInternal (IntPtr s, void* context, void* buffer, IntPtr size)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnPeek ((IntPtr)buffer, size);
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamIsAtEndProxyDelegate))]
		private static bool IsAtEndInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnIsAtEnd ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamHasPositionProxyDelegate))]
		private static bool HasPositionInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnHasPosition ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamHasLengthProxyDelegate))]
		private static bool HasLengthInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnHasLength ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamRewindProxyDelegate))]
		private static bool RewindInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnRewind ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamGetPositionProxyDelegate))]
		private static IntPtr GetPositionInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnGetPosition ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamSeekProxyDelegate))]
		private static bool SeekInternal (IntPtr s, void* context, IntPtr position)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnSeek (position);
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamMoveProxyDelegate))]
		private static bool MoveInternal (IntPtr s, void* context, int offset)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnMove (offset);
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamGetLengthProxyDelegate))]
		private static IntPtr GetLengthInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnGetLength ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamDuplicateProxyDelegate))]
		private static IntPtr DuplicateInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnDuplicate ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamForkProxyDelegate))]
		private static IntPtr ForkInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnFork ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamDestroyProxyDelegate))]
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
