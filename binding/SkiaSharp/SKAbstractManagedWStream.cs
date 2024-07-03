#nullable disable

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp
{
	public unsafe abstract class SKAbstractManagedWStream : SKWStream
	{
		private static readonly SKManagedWStreamDelegates delegates;

		private int fromNative;

		static SKAbstractManagedWStream ()
		{
			delegates = new SKManagedWStreamDelegates {
#if USE_LIBRARY_IMPORT
				fWrite = &WriteInternal,
				fFlush = &FlushInternal,
				fBytesWritten = &BytesWrittenInternal,
				fDestroy = &DestroyInternal
#else
				fWrite = WriteInternal,
				fFlush = FlushInternal,
				fBytesWritten = BytesWrittenInternal,
				fDestroy = DestroyInternal,
#endif
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

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
		[MonoPInvokeCallback (typeof (SKManagedWStreamWriteProxyDelegate))]
#endif
		private static bool WriteInternal (IntPtr s, void* context, void* buffer, IntPtr size)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedWStream> ((IntPtr)context, out _);
			return stream.OnWrite ((IntPtr)buffer, size);
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
		[MonoPInvokeCallback (typeof (SKManagedWStreamFlushProxyDelegate))]
#endif
		private static void FlushInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedWStream> ((IntPtr)context, out _);
			stream.OnFlush ();
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
		[MonoPInvokeCallback (typeof (SKManagedWStreamBytesWrittenProxyDelegate))]
#endif
		private static IntPtr BytesWrittenInternal (IntPtr s, void* context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedWStream> ((IntPtr)context, out _);
			return stream.OnBytesWritten ();
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
		[MonoPInvokeCallback (typeof (SKManagedWStreamDestroyProxyDelegate))]
#endif
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
