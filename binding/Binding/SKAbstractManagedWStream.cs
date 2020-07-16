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
#if __WASM__ && USE_INTPTR_DELEGATES
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
		private static bool WriteInternal (IntPtr s, void* context, void* buffer, IntPtr size)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedWStream> ((IntPtr)context, out _);
			return stream.OnWrite ((IntPtr)buffer, size);
		}

		[MonoPInvokeCallback (typeof (SKManagedWStreamFlushProxyDelegate))]
		private static void FlushInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedWStream> ((IntPtr)context, out _);
			stream.OnFlush ();
		}

		[MonoPInvokeCallback (typeof (SKManagedWStreamBytesWrittenProxyDelegate))]
		private static IntPtr BytesWrittenInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedWStream> ((IntPtr)context, out _);
			return stream.OnBytesWritten ();
		}

		[MonoPInvokeCallback (typeof (SKManagedWStreamDestroyProxyDelegate))]
		private static void DestroyInternal (IntPtr s, void* context)
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
