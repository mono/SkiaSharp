using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using NativePointerDictionary = System.Collections.Concurrent.ConcurrentDictionary<System.IntPtr, SkiaSharp.SKAbstractManagedStream>;

namespace SkiaSharp
{
	public unsafe abstract class SKAbstractManagedStream : SKStreamAsset
	{
		private static readonly SKManagedStreamDelegates delegates;

		private int fromNative;

		static SKAbstractManagedStream ()
		{
#if __WASM__
			//
			// Javascript returns the list of registered methods, then the
			// sk_managedstream_set_delegates gets called through P/Invoke
			// because it is not exported when building for AOT.
			//

			var ret = WebAssembly.Runtime.InvokeJS ($"SkiaSharp.SurfaceManager.registerManagedStream();");
			var funcs = ret.Split (new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select (f => (IntPtr)int.Parse (f, CultureInfo.InvariantCulture))
				.ToArray ();

			if (funcs.Length == 13) {
				delegates = new SKManagedStreamDelegates {
					fRead = funcs[0],
					fPeek = funcs[1],
					fIsAtEnd = funcs[2],
					fHasPosition = funcs[3],
					fHasLength = funcs[4],
					fRewind = funcs[5],
					fGetPosition = funcs[6],
					fSeek = funcs[7],
					fMove = funcs[8],
					fGetLength = funcs[9],
					fDuplicate = funcs[10],
					fFork = funcs[11],
					fDestroy = funcs[12],
				};
			} else {
				throw new InvalidOperationException ($"Mismatch for registerManagedStream returned values (got {funcs.Length}, expected 12)");
			}
#else
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
#endif

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

		protected internal abstract IntPtr OnPeek (IntPtr buffer, IntPtr size);

		protected internal abstract bool OnIsAtEnd ();

		protected internal abstract bool OnHasPosition ();

		protected internal abstract bool OnHasLength ();

		protected internal abstract bool OnRewind ();

		protected internal abstract IntPtr OnGetPosition ();

		protected internal abstract IntPtr OnGetLength ();

		protected internal abstract bool OnSeek (IntPtr position);

		protected internal abstract bool OnMove (int offset);

		protected internal abstract IntPtr OnCreateNew ();

		protected virtual IntPtr OnFork ()
		{
			var stream = OnCreateNew ();
			SkiaApi.sk_stream_seek (stream, SkiaApi.sk_stream_get_position (Handle));
			return stream;
		}

		protected virtual IntPtr OnDuplicate () => OnCreateNew ();

		[MonoPInvokeCallback (typeof (SKManagedStreamReadProxyDelegate))]
		private static IntPtr ReadInternal (IntPtr s, IntPtr context, IntPtr buffer, IntPtr size)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnRead ((IntPtr)buffer, size);
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamPeekProxyDelegate))]
		private static IntPtr PeekInternal (IntPtr s, IntPtr context, IntPtr buffer, IntPtr size)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnPeek ((IntPtr)buffer, size);
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamIsAtEndProxyDelegate))]
		private static bool IsAtEndInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnIsAtEnd ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamHasPositionProxyDelegate))]
		private static bool HasPositionInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnHasPosition ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamHasLengthProxyDelegate))]
		private static bool HasLengthInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnHasLength ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamRewindProxyDelegate))]
		private static bool RewindInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnRewind ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamGetPositionProxyDelegate))]
		private static IntPtr GetPositionInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnGetPosition ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamSeekProxyDelegate))]
		private static bool SeekInternal (IntPtr s, IntPtr context, IntPtr position)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnSeek (position);
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamMoveProxyDelegate))]
		private static bool MoveInternal (IntPtr s, IntPtr context, int offset)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnMove (offset);
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamGetLengthProxyDelegate))]
		private static IntPtr GetLengthInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnGetLength ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamDuplicateProxyDelegate))]
		private static IntPtr DuplicateInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnDuplicate ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamForkProxyDelegate))]
		private static IntPtr ForkInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnFork ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamDestroyProxyDelegate))]
		private static void DestroyInternal (IntPtr s, IntPtr context)
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
