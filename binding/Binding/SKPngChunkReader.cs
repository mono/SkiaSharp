using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp
{
	/// <summary>
	/// Base class for optional callbacks to retrieve meta/chunk data out of a PNG
	/// <br></br> encoded image as it is being decoded.
	/// <br></br> Used by SkCodec.
	/// </summary>
	public unsafe abstract class SKPngChunkReader : SKObject, ISKSkipObjectRegistration
	{
		private static readonly SKManagedPngChunkReaderDelegates delegates;
		private readonly IntPtr userData;
		private int fromNative;

		static SKPngChunkReader()
		{
			delegates = new SKManagedPngChunkReaderDelegates
			{
				fReadChunk = ReadChunkInternal,
				fDestroy = DestroyInternal,
			};

			SkiaApi.sk_managedpngchunkreader_set_procs(delegates);
		}

		protected SKPngChunkReader()
			: base(IntPtr.Zero, true)
		{
			userData = DelegateProxies.CreateUserData(this, true);
			Handle = SkiaApi.sk_managedpngchunkreader_new((void*)userData);

			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException("Unable to create a new SKManagedAllocator instance.");
		}

		protected override void Dispose(bool disposing) =>
			base.Dispose(disposing);

		protected override void DisposeNative()
		{
			if (Interlocked.CompareExchange(ref fromNative, 0, 0) == 0)
			{
				SkiaApi.sk_managedpngchunkreader_delete(Handle);
			}
		}

		/// <summary>
		/// This will be called by the decoder when it sees an unknown chunk.
		/// <br></br>
		/// <br></br> Use by SkCodec:
		/// <br></br> Depending on the location of the unknown chunks, this callback may be
		/// <br></br> called by
		/// <br></br>     - the factory (NewFromStream/NewFromData)
		/// <br></br>     - getPixels
		/// <br></br>     - startScanlineDecode
		/// <br></br>     - the first call to getScanlines/skipScanlines
		/// <br></br> The callback may be called from a different thread (e.g. if the SkCodec
		/// <br></br> is passed to another thread), and it may be called multiple times, if
		/// <br></br> the SkCodec is used multiple times.
		/// <br></br>
		/// <br></br> @param tag Name for this type of chunk.
		/// <br></br> @param data Data to be interpreted by the subclass.
		/// <br></br> @param length Number of bytes of data in the chunk.
		/// <br></br> @return true to continue decoding, or false to indicate an error, which
		/// <br></br>     will cause the decoder to not return the image.
		/// </summary>
		/// <param name="tag">Name for this type of chunk.</param>
		/// <param name="data">Data to be interpreted by the subclass.</param>
		/// <param name="length">Number of bytes of data in the chunk.</param>
		/// <returns>true to continue decoding, or false to indicate an error, which will cause the decoder to not return the image.</returns>
		public abstract bool ReadChunk(string tag, void* data, IntPtr length);

		/// <summary>
		/// This will be called by the decoder when it sees an unknown chunk.
		/// <br></br>
		/// <br></br> Use by SkCodec:
		/// <br></br> Depending on the location of the unknown chunks, this callback may be
		/// <br></br> called by
		/// <br></br>     - the factory (NewFromStream/NewFromData)
		/// <br></br>     - getPixels
		/// <br></br>     - startScanlineDecode
		/// <br></br>     - the first call to getScanlines/skipScanlines
		/// <br></br> The callback may be called from a different thread (e.g. if the SkCodec
		/// <br></br> is passed to another thread), and it may be called multiple times, if
		/// <br></br> the SkCodec is used multiple times.
		/// <br></br>
		/// <br></br> @param tag Name for this type of chunk.
		/// <br></br> @param data Data to be interpreted by the subclass.
		/// <br></br> @param length Number of bytes of data in the chunk.
		/// <br></br> @return true to continue decoding, or false to indicate an error, which
		/// <br></br>     will cause the decoder to not return the image.
		/// </summary>
		/// <param name="tag">Name for this type of chunk.</param>
		/// <param name="data">Data to be interpreted by the subclass.</param>
		/// <param name="length">Number of bytes of data in the chunk.</param>
		/// <returns>true to continue decoding, or false to indicate an error, which will cause the decoder to not return the image.</returns>
		public virtual bool ReadChunk (string tag, IntPtr data, IntPtr length)
		{
			return ReadChunk (tag, data.ToPointer(), length);
		}

		/// <summary>
		/// This will be called by the decoder when it sees an unknown chunk.
		/// <br></br>
		/// <br></br> Override this is you want to pass to C interop.
		/// <br></br>
		/// <br></br> Use by SkCodec:
		/// <br></br> Depending on the location of the unknown chunks, this callback may be
		/// <br></br> called by
		/// <br></br>     - the factory (NewFromStream/NewFromData)
		/// <br></br>     - getPixels
		/// <br></br>     - startScanlineDecode
		/// <br></br>     - the first call to getScanlines/skipScanlines
		/// <br></br> The callback may be called from a different thread (e.g. if the SkCodec
		/// <br></br> is passed to another thread), and it may be called multiple times, if
		/// <br></br> the SkCodec is used multiple times.
		/// <br></br>
		/// <br></br> @param tag Name for this type of chunk.
		/// <br></br> @param data Data to be interpreted by the subclass.
		/// <br></br> @param length Number of bytes of data in the chunk.
		/// <br></br> @return true to continue decoding, or false to indicate an error, which
		/// <br></br>     will cause the decoder to not return the image.
		/// </summary>
		/// <param name="tag">Name for this type of chunk.</param>
		/// <param name="data">Data to be interpreted by the subclass.</param>
		/// <param name="length">Number of bytes of data in the chunk.</param>
		/// <returns>true to continue decoding, or false to indicate an error, which will cause the decoder to not return the image.</returns>
		public virtual bool ReadChunk (void* tag, void* data, IntPtr length)
		{
			return ReadChunk (Marshal.PtrToStringAnsi((IntPtr)tag), data, length);
		}

		// impl

		[MonoPInvokeCallback (typeof(SKManagedPngChunkReaderReadChunkProxyDelegate))]
		private static bool ReadChunkInternal(IntPtr d, void* context, void* tag, void* data, IntPtr length)
		{
			var dump = DelegateProxies.GetUserData<SKPngChunkReader>((IntPtr)context, out _);
			return dump.ReadChunk(tag, data, length);
		}

		[MonoPInvokeCallback(typeof(SKManagedPngChunkReaderDestroyProxyDelegate))]
		private static void DestroyInternal(IntPtr s, void* context)
		{
			var id = DelegateProxies.GetUserData<SKPngChunkReader>((IntPtr)context, out var gch);
			if (id != null)
			{
				Interlocked.Exchange(ref id.fromNative, 1);
				id.Dispose();
			}
			gch.Free();
		}
	}
}
