using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp
{
	public abstract class SKAbstractManagedWStream : SKWStream
	{
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal delegate bool WriteDelegate (IntPtr s, IntPtr context, IntPtr buffer, IntPtr size);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate void FlushDelegate (IntPtr s, IntPtr context);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate IntPtr BytesWrittenDelegate (IntPtr s, IntPtr context);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate void DestroyDelegate (IntPtr s, IntPtr context);

		[StructLayout (LayoutKind.Sequential)]
		internal struct Procs
		{
			public WriteDelegate fWrite;
			public FlushDelegate fFlush;
			public BytesWrittenDelegate fBytesWritten;
			public DestroyDelegate fDestroy;
		}

		private static readonly Procs delegates;

		private int fromNative;

		static SKAbstractManagedWStream ()
		{
			delegates = new Procs {
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
			DelegateProxies.Create (this, out _, out var ctx);
			Handle = SkiaApi.sk_managedwstream_new (ctx);
		}

		protected override void DisposeNative ()
		{
			if (Interlocked.CompareExchange (ref fromNative, 0, 0) == 0)
				SkiaApi.sk_managedwstream_destroy (Handle);
		}

		protected abstract bool OnWrite (IntPtr buffer, IntPtr size);

		protected abstract void OnFlush ();

		protected abstract IntPtr OnBytesWritten ();

		[MonoPInvokeCallback (typeof (WriteDelegate))]
		private static bool WriteInternal (IntPtr s, IntPtr context, IntPtr buffer, IntPtr size)
		{
			var stream = DelegateProxies.Get<SKAbstractManagedWStream> (context, out _);
			return stream.OnWrite (buffer, size);
		}

		[MonoPInvokeCallback (typeof (FlushDelegate))]
		private static void FlushInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.Get<SKAbstractManagedWStream> (context, out _);
			stream.OnFlush ();
		}

		[MonoPInvokeCallback (typeof (BytesWrittenDelegate))]
		private static IntPtr BytesWrittenInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.Get<SKAbstractManagedWStream> (context, out _);
			return stream.OnBytesWritten ();
		}

		[MonoPInvokeCallback (typeof (DestroyDelegate))]
		private static void DestroyInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.Get<SKAbstractManagedWStream> (context, out var gch);
			Interlocked.Exchange (ref stream.fromNative, 1);
			gch.Free ();
			stream.Dispose ();
		}
	}
}
