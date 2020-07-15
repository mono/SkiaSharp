using System;
using System.Threading;

namespace SkiaSharp
{
	public unsafe abstract class SKAbstractManagedWStream : SKWStream
	{
		private static readonly SKManagedWStreamDelegates delegates;

		private int fromNative;

		static SKAbstractManagedWStream ()
		{
#if __WASM__
			var funcs = SkiaApi.BindWasmMembers<SKAbstractManagedWStream> (new[] {
				(nameof (SKAbstractManagedWStream.WriteInternal), "iiiii"),
				(nameof (SKAbstractManagedWStream.FlushInternal), "vii"),
				(nameof (SKAbstractManagedWStream.BytesWrittenInternal), "iii"),
				(nameof (SKAbstractManagedWStream.DestroyInternal), "vii"),
			});

			var WriteInternal = funcs[0];
			var FlushInternal = funcs[1];
			var BytesWrittenInternal = funcs[2];
			var DestroyInternal = funcs[3];
#endif

			delegates = new SKManagedWStreamDelegates {
				fWrite = WriteInternal,
				fFlush = FlushInternal,
				fBytesWritten = BytesWrittenInternal,
				fDestroy = DestroyInternal,
			};

			SkiaApi.sk_managedwstream_set_procs (delegates);
		}

		protected SKAbstractManagedWStream ()
			: this (true)
		{
		}

		protected SKAbstractManagedWStream (bool owns)
			: base (IntPtr.Zero, owns)
		{
			var ctx = DelegateProxies.CreateUserData (this, true);
			Handle = SkiaApi.sk_managedwstream_new ((void*)ctx);
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative ()
		{
			if (Interlocked.CompareExchange (ref fromNative, 0, 0) == 0)
				SkiaApi.sk_managedwstream_destroy (Handle);
		}

		protected abstract bool OnWrite (IntPtr buffer, IntPtr size);

		protected abstract void OnFlush ();

		protected abstract IntPtr OnBytesWritten ();

		[MonoPInvokeCallback (typeof (SKManagedWStreamWriteProxyDelegate))]
#if __WASM__
		private static bool WriteInternal (IntPtr s, IntPtr context, IntPtr buffer, IntPtr size)
#else
		private static bool WriteInternal (IntPtr s, void* context, void* buffer, IntPtr size)
#endif
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedWStream> ((IntPtr)context, out _);
			return stream.OnWrite ((IntPtr)buffer, size);
		}

		[MonoPInvokeCallback (typeof (SKManagedWStreamFlushProxyDelegate))]
#if __WASM__
		private static void FlushInternal (IntPtr s, IntPtr context)
#else
		private static void FlushInternal (IntPtr s, void* context)
#endif
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedWStream> ((IntPtr)context, out _);
			stream.OnFlush ();
		}

		[MonoPInvokeCallback (typeof (SKManagedWStreamBytesWrittenProxyDelegate))]
#if __WASM__
		private static IntPtr BytesWrittenInternal (IntPtr s, IntPtr context)
#else
		private static IntPtr BytesWrittenInternal (IntPtr s, void* context)
#endif
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedWStream> ((IntPtr)context, out _);
			return stream.OnBytesWritten ();
		}

		[MonoPInvokeCallback (typeof (SKManagedWStreamDestroyProxyDelegate))]
#if __WASM__
		private static void DestroyInternal (IntPtr s, IntPtr context)
#else
		private static void DestroyInternal (IntPtr s, void* context)
#endif
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedWStream> ((IntPtr)context, out var gch);
			if (stream != null) {
				Interlocked.Exchange (ref stream.fromNative, 1);
				stream.Dispose ();
			}
			gch.Free ();
		}
	}
}
