using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp
{
	public unsafe abstract class SKPngChunkReader : SKObject, ISKSkipObjectRegistration
	{
		private static readonly SKManagedPngChunkReaderDelegates delegates;
		private readonly IntPtr userData;
		private int fromNative;

		static SKPngChunkReader ()
		{
			delegates = new SKManagedPngChunkReaderDelegates {
				fReadChunk = ReadChunkInternal,
				fDestroy = DestroyInternal,
			};

			SkiaApi.sk_managedpngchunkreader_set_procs (delegates);
		}

		protected SKPngChunkReader ()
			: base (IntPtr.Zero, true)
		{
			userData = DelegateProxies.CreateUserData (this, true);
			Handle = SkiaApi.sk_managedpngchunkreader_new ((void*)userData);

			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKPngChunkReader instance.");
		}

		protected override void DisposeNative ()
		{
			if (Interlocked.CompareExchange (ref fromNative, 0, 0) == 0) {
				SkiaApi.sk_managedpngchunkreader_delete (Handle);
			}
		}

		protected abstract bool ReadChunk (string tag, IntPtr data, IntPtr length);

		// impl

		[MonoPInvokeCallback (typeof (SKManagedPngChunkReaderReadChunkProxyDelegate))]
		private static bool ReadChunkInternal (IntPtr d, void* context, void* tag, void* data, IntPtr length)
		{
			var dump = DelegateProxies.GetUserData<SKPngChunkReader> ((IntPtr)context, out _);
			return dump.ReadChunk (Marshal.PtrToStringAnsi ((IntPtr)tag), (IntPtr)data, length);
		}

		[MonoPInvokeCallback (typeof (SKManagedPngChunkReaderDestroyProxyDelegate))]
		private static void DestroyInternal (IntPtr s, void* context)
		{
			var id = DelegateProxies.GetUserData<SKPngChunkReader> ((IntPtr)context, out var gch);
			if (id != null) {
				Interlocked.Exchange (ref id.fromNative, 1);
				id.Dispose ();
			}
			gch.Free ();
		}
	}
}
