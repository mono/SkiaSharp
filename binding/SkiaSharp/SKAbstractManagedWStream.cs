#nullable disable

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp
{
	/// <summary>Represents a <see cref="T:SkiaSharp.SKWStream" /> (a writeable Skia stream).</summary>
	/// <remarks />
	public unsafe abstract class SKAbstractManagedWStream : SKWStream
	{
		private static readonly SKManagedWStreamDelegates delegates;

		internal int fromNative;

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKAbstractManagedWStream" />.</summary>
		/// <remarks />
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

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKAbstractManagedWStream" />.</summary>
		/// <remarks />
		protected SKAbstractManagedWStream ()
			: this (true)
		{
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKAbstractManagedWStream" />.</summary>
		/// <param name="owns">The value indicating whether this object should destroy the underlying native object.</param>
		/// <remarks />
		protected SKAbstractManagedWStream (bool owns)
			: base (IntPtr.Zero, owns)
		{
			var ctx = DelegateProxies.CreateUserData (this, true);
			Handle = SkiaApi.sk_managedwstream_new ((void*)ctx);
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKAbstractManagedWStream" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKAbstractManagedWStream" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
		/// <remarks />
		protected override void DisposeNative ()
		{
			if (Interlocked.CompareExchange (ref fromNative, 0, 0) == 0)
				SkiaApi.sk_managedwstream_destroy (Handle);
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedWStream" /> types to copy the specified number of bytes from the specified buffer into the underlying stream.</summary>
		/// <param name="buffer">The buffer to copy into the underlying stream.</param>
		/// <param name="size">The number of bytes to copy from the buffer.</param>
		/// <returns>Returns <see langword="true" /> on success, otherwise <see langword="false" />.</returns>
		/// <remarks />
		protected internal abstract bool OnWrite (IntPtr buffer, IntPtr size);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedWStream" /> types to flush the bytes to the underlying stream.</summary>
		/// <remarks />
		protected internal abstract void OnFlush ();

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedWStream" /> types to specify the number of bytes currently written to the stream.</summary>
		/// <returns>Returns the number of bytes currently written to the stream.</returns>
		/// <remarks />
		protected internal abstract IntPtr OnBytesWritten ();
	}
}
