using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace SkiaSharp
{
	public unsafe abstract class SKAbstractManagedStream : SKStreamAsset
	{
		private static readonly SKManagedStreamDelegates delegates;

		private int fromNative;

		static SKAbstractManagedStream ()
		{
#if __WASM__
			const string js =
				"SkiaSharp_SkiaApi.bindMembers('[SkiaSharp] SkiaSharp.SKAbstractManagedStream', {" +
				"  '" + nameof (SKAbstractManagedStream.ReadInternal) + "':        'iiiii'," +
				"  '" + nameof (SKAbstractManagedStream.PeekInternal) + "':        'iiiii'," +
				"  '" + nameof (SKAbstractManagedStream.IsAtEndInternal) + "':     'iii'," +
				"  '" + nameof (SKAbstractManagedStream.HasPositionInternal) + "': 'iii'," +
				"  '" + nameof (SKAbstractManagedStream.HasLengthInternal) + "':   'iii'," +
				"  '" + nameof (SKAbstractManagedStream.RewindInternal) + "':      'iii'," +
				"  '" + nameof (SKAbstractManagedStream.GetPositionInternal) + "': 'iii'," +
				"  '" + nameof (SKAbstractManagedStream.SeekInternal) + "':        'iiii'," +
				"  '" + nameof (SKAbstractManagedStream.MoveInternal) + "':        'iiii'," +
				"  '" + nameof (SKAbstractManagedStream.GetLengthInternal) + "':   'iii'," +
				"  '" + nameof (SKAbstractManagedStream.DuplicateInternal) + "':   'iii'," +
				"  '" + nameof (SKAbstractManagedStream.ForkInternal) + "':        'iii'," +
				"  '" + nameof (SKAbstractManagedStream.DestroyInternal) + "':     'vii'," +
				"});";
			const int expected = 13;

			var ret = WebAssembly.Runtime.InvokeJS (js);
			var funcs = ret.Split (new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select (f => (IntPtr)int.Parse (f, CultureInfo.InvariantCulture))
				.ToArray ();

			if (funcs.Length != expected)
				throw new InvalidOperationException ($"Mismatch when binding 'SkiaSharp.SKAbstractManagedStream' members. Returned {funcs.Length}, expected {expected}.");

			// we can do magic with variables
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
#if __WASM__
		private static IntPtr ReadInternal (IntPtr s, IntPtr context, IntPtr buffer, IntPtr size)
#else
		private static IntPtr ReadInternal (IntPtr s, void* context, void* buffer, IntPtr size)
#endif
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnRead ((IntPtr)buffer, size);
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamPeekProxyDelegate))]
#if __WASM__
		private static IntPtr PeekInternal (IntPtr s, IntPtr context, IntPtr buffer, IntPtr size)
#else
		private static IntPtr PeekInternal (IntPtr s, void* context, void* buffer, IntPtr size)
#endif
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnPeek ((IntPtr)buffer, size);
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamIsAtEndProxyDelegate))]
#if __WASM__
		private static bool IsAtEndInternal (IntPtr s, IntPtr context)
#else
		private static bool IsAtEndInternal (IntPtr s, void* context)
#endif
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnIsAtEnd ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamHasPositionProxyDelegate))]
#if __WASM__
		private static bool HasPositionInternal (IntPtr s, IntPtr context)
#else
		private static bool HasPositionInternal (IntPtr s, void* context)
#endif
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnHasPosition ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamHasLengthProxyDelegate))]
#if __WASM__
		private static bool HasLengthInternal (IntPtr s, IntPtr context)
#else
		private static bool HasLengthInternal (IntPtr s, void* context)
#endif
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnHasLength ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamRewindProxyDelegate))]
#if __WASM__
		private static bool RewindInternal (IntPtr s, IntPtr context)
#else
		private static bool RewindInternal (IntPtr s, void* context)
#endif
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnRewind ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamGetPositionProxyDelegate))]
#if __WASM__
		private static IntPtr GetPositionInternal (IntPtr s, IntPtr context)
#else
		private static IntPtr GetPositionInternal (IntPtr s, void* context)
#endif
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnGetPosition ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamSeekProxyDelegate))]
#if __WASM__
		private static bool SeekInternal (IntPtr s, IntPtr context, IntPtr position)
#else
		private static bool SeekInternal (IntPtr s, void* context, IntPtr position)
#endif
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnSeek (position);
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamMoveProxyDelegate))]
#if __WASM__
		private static bool MoveInternal (IntPtr s, IntPtr context, int offset)
#else
		private static bool MoveInternal (IntPtr s, void* context, int offset)
#endif
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnMove (offset);
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamGetLengthProxyDelegate))]
#if __WASM__
		private static IntPtr GetLengthInternal (IntPtr s, IntPtr context)
#else
		private static IntPtr GetLengthInternal (IntPtr s, void* context)
#endif
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnGetLength ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamDuplicateProxyDelegate))]
#if __WASM__
		private static IntPtr DuplicateInternal (IntPtr s, IntPtr context)
#else
		private static IntPtr DuplicateInternal (IntPtr s, void* context)
#endif
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnDuplicate ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamForkProxyDelegate))]
#if __WASM__
		private static IntPtr ForkInternal (IntPtr s, IntPtr context)
#else
		private static IntPtr ForkInternal (IntPtr s, void* context)
#endif
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnFork ();
		}

		[MonoPInvokeCallback (typeof (SKManagedStreamDestroyProxyDelegate))]
#if __WASM__
		private static void DestroyInternal (IntPtr s, IntPtr context)
#else
		private static void DestroyInternal (IntPtr s, void* context)
#endif
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
