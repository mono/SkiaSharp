#nullable disable

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp
{
	/// <summary>
	/// Represents a <see cref="T:SkiaSharp.SKWStream" /> (a writeable Skia stream).
	/// </summary>
	public unsafe abstract class SKAbstractManagedWStream : SKWStream
	{
		private static readonly SKManagedWStreamDelegates delegates;

		internal int fromNative;

		static SKAbstractManagedWStream ()
		{
			delegates = new SKManagedWStreamDelegates {
				fWrite = DelegateProxies.SKManagedWStreamWriteProxy,
				fFlush = DelegateProxies.SKManagedWStreamFlushProxy,
				fBytesWritten = DelegateProxies.SKManagedWStreamBytesWrittenProxy,
				fDestroy = DelegateProxies.SKManagedWStreamDestroyProxy
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

		protected internal abstract bool OnWrite (IntPtr buffer, IntPtr size);

		protected internal abstract void OnFlush ();

		protected internal abstract IntPtr OnBytesWritten ();
	}
}
