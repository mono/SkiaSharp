using System;

namespace SkiaSharp
{
	// Read Only Streams
	public unsafe abstract class SKStream : SKObject
	{
		internal SKStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public bool IsAtEnd =>
			SkiaApi.sk_stream_is_at_end (Handle);

		public SByte ReadSByte () =>
			ReadSByte (out var buffer) ? buffer : default;

		public Int16 ReadInt16 () =>
			ReadInt16 (out var buffer) ? buffer : default;

		public Int32 ReadInt32 () =>
			ReadInt32 (out var buffer) ? buffer : default;

		public Byte ReadByte () =>
			ReadByte (out var buffer) ? buffer : default;

		public UInt16 ReadUInt16 () =>
			ReadUInt16 (out var buffer) ? buffer : default;

		public UInt32 ReadUInt32 () =>
			ReadUInt32 (out var buffer) ? buffer : default;

		public bool ReadBool () =>
			ReadBool (out var buffer) ? buffer : default;

		public bool ReadSByte (out SByte buffer)
		{
			fixed (SByte* b = &buffer) {
				return SkiaApi.sk_stream_read_s8 (Handle, b);
			}
		}

		public bool ReadInt16 (out Int16 buffer)
		{
			fixed (Int16* b = &buffer) {
				return SkiaApi.sk_stream_read_s16 (Handle, b);
			}
		}

		public bool ReadInt32 (out Int32 buffer)
		{
			fixed (Int32* b = &buffer) {
				return SkiaApi.sk_stream_read_s32 (Handle, b);
			}
		}

		public bool ReadByte (out Byte buffer)
		{
			fixed (Byte* b = &buffer) {
				return SkiaApi.sk_stream_read_u8 (Handle, b);
			}
		}

		public bool ReadUInt16 (out UInt16 buffer)
		{
			fixed (UInt16* b = &buffer) {
				return SkiaApi.sk_stream_read_u16 (Handle, b);
			}
		}

		public bool ReadUInt32 (out UInt32 buffer)
		{
			fixed (UInt32* b = &buffer) {
				return SkiaApi.sk_stream_read_u32 (Handle, b);
			}
		}

		public bool ReadBool (out Boolean buffer)
		{
			byte b;
			var result = SkiaApi.sk_stream_read_bool (Handle, &b);
			buffer = b > 0;
			return result;
		}

		public int Read (Span<byte> buffer) =>
			Read (buffer, buffer.Length);

		public int Read (Span<byte> buffer, int size)
		{
			fixed (byte* b = buffer) {
				return (int)SkiaApi.sk_stream_read (Handle, b, (IntPtr)size);
			}
		}

		public int Read (IntPtr buffer, int size) =>
			(int)SkiaApi.sk_stream_read (Handle, (void*)buffer, (IntPtr)size);

		public int Peek (IntPtr buffer, int size) =>
			(int)SkiaApi.sk_stream_peek (Handle, (void*)buffer, (IntPtr)size);

		public int Skip (int size) =>
			(int)SkiaApi.sk_stream_skip (Handle, (IntPtr)size);

		public bool Rewind () =>
			SkiaApi.sk_stream_rewind (Handle);

		public bool Seek (int position) =>
			SkiaApi.sk_stream_seek (Handle, (IntPtr)position);

		public bool Move (int offset) =>
			Move ((int)offset);

		public IntPtr GetMemoryBase () =>
			(IntPtr)SkiaApi.sk_stream_get_memory_base (Handle);

		internal SKStream Fork () =>
			GetObject<SKStream, SKStreamImplementation> (SkiaApi.sk_stream_fork (Handle));

		internal SKStream Duplicate () =>
			GetObject<SKStream, SKStreamImplementation> (SkiaApi.sk_stream_duplicate (Handle));

		public bool HasPosition =>
			SkiaApi.sk_stream_has_position (Handle);

		public int Position {
			get => (int)SkiaApi.sk_stream_get_position (Handle);
			set => Seek (value);
		}

		public bool HasLength =>
			SkiaApi.sk_stream_has_length (Handle);

		public int Length =>
			(int)SkiaApi.sk_stream_get_length (Handle);
	}

	internal class SKStreamImplementation : SKStream
	{
		[Preserve]
		internal SKStreamImplementation (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void DisposeNative () =>
			SkiaApi.sk_stream_destroy (Handle);
	}

	public abstract class SKStreamRewindable : SKStream
	{
		internal SKStreamRewindable (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
	}

	public abstract class SKStreamSeekable : SKStreamRewindable
	{
		internal SKStreamSeekable (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
	}

	public abstract class SKStreamAsset : SKStreamSeekable
	{
		internal SKStreamAsset (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
	}

	internal class SKStreamAssetImplementation : SKStreamAsset
	{
		[Preserve]
		internal SKStreamAssetImplementation (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void DisposeNative () =>
			SkiaApi.sk_stream_asset_destroy (Handle);
	}

	public abstract class SKStreamMemory : SKStreamAsset
	{
		internal SKStreamMemory (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
	}

	public unsafe class SKFileStream : SKStreamAsset
	{
		[Preserve]
		internal SKFileStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKFileStream (string path)
			: base (CreateNew (path), true)
		{
			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKFileStream instance.");
		}

		private static IntPtr CreateNew (string path)
		{
			var bytes = StringUtilities.GetEncodedText (path, SKTextEncoding.Utf8);
			fixed (byte* p = bytes) {
				return SkiaApi.sk_filestream_new (p);
			}
		}

		protected override void DisposeNative () =>
			SkiaApi.sk_filestream_destroy (Handle);

		public bool IsValid =>
			SkiaApi.sk_filestream_is_valid (Handle);

		public static bool IsPathSupported (string path) => true;

		public static SKStreamAsset OpenStream (string path)
		{
			var stream = new SKFileStream (path);
			if (!stream.IsValid) {
				stream.Dispose ();
				stream = null;
			}
			return stream;
		}
	}

	public unsafe class SKMemoryStream : SKStreamMemory
	{
		[Preserve]
		internal SKMemoryStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKMemoryStream ()
			: this (SkiaApi.sk_memorystream_new (), true)
		{
			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKMemoryStream instance.");
		}

		public SKMemoryStream (int length)
			: this (SkiaApi.sk_memorystream_new_with_length ((IntPtr)length), true)
		{
			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKMemoryStream instance.");
		}

		public SKMemoryStream (SKData data)
			: this (IntPtr.Zero, true)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			Handle = SkiaApi.sk_memorystream_new_with_skdata (data.Handle);

			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKMemoryStream instance.");
		}

		public SKMemoryStream (ReadOnlySpan<byte> data)
			: this (IntPtr.Zero, true)
		{
			fixed (byte* d = data) {
				Handle = SkiaApi.sk_memorystream_new_with_data (d, (IntPtr)data.Length, true);
			}

			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKMemoryStream instance.");
		}

		protected override void DisposeNative () =>
			SkiaApi.sk_memorystream_destroy (Handle);

		public void SetMemory (ReadOnlySpan<byte> data)
		{
			fixed (byte* d = data) {
				SkiaApi.sk_memorystream_set_memory (Handle, d, (IntPtr)data.Length, true);
			}
		}
	}

	// Writeable Streams

	public unsafe abstract class SKWStream : SKObject
	{
		internal SKWStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public virtual int BytesWritten =>
			(int)SkiaApi.sk_wstream_bytes_written (Handle);

		public virtual bool Write (ReadOnlySpan<byte> buffer, int size)
		{
			fixed (byte* b = buffer) {
				return SkiaApi.sk_wstream_write (Handle, (void*)b, (IntPtr)size);
			}
		}

		public bool NewLine () =>
			SkiaApi.sk_wstream_newline (Handle);

		public virtual void Flush () =>
			SkiaApi.sk_wstream_flush (Handle);

		public bool Write8 (Byte value) =>
			SkiaApi.sk_wstream_write_8 (Handle, value);

		public bool Write16 (UInt16 value) =>
			SkiaApi.sk_wstream_write_16 (Handle, value);

		public bool Write32 (UInt32 value) =>
			SkiaApi.sk_wstream_write_32 (Handle, value);

		public bool WriteText (string value) =>
			SkiaApi.sk_wstream_write_text (Handle, value);

		public bool WriteDecimalAsTest (Int32 value) =>
			SkiaApi.sk_wstream_write_dec_as_text (Handle, value);

		public bool WriteBigDecimalAsText (Int64 value, int digits) =>
			SkiaApi.sk_wstream_write_bigdec_as_text (Handle, value, digits);

		public bool WriteHexAsText (UInt32 value, int digits) =>
			SkiaApi.sk_wstream_write_hex_as_text (Handle, value, digits);

		public bool WriteScalarAsText (float value) =>
			SkiaApi.sk_wstream_write_scalar_as_text (Handle, value);

		public bool WriteBool (bool value) =>
			SkiaApi.sk_wstream_write_bool (Handle, value);

		public bool WriteScalar (float value) =>
			SkiaApi.sk_wstream_write_scalar (Handle, value);

		public bool WritePackedUInt32 (UInt32 value) =>
			SkiaApi.sk_wstream_write_packed_uint (Handle, (IntPtr)value);

		public bool WriteStream (SKStream input, int length)
		{
			if (input == null)
				throw new ArgumentNullException (nameof (input));

			return SkiaApi.sk_wstream_write_stream (Handle, input.Handle, (IntPtr)length);
		}

		public static int GetSizeOfPackedUInt32 (UInt32 value) =>
			SkiaApi.sk_wstream_get_size_of_packed_uint ((IntPtr)value);
	}

	public unsafe class SKFileWStream : SKWStream
	{
		[Preserve]
		internal SKFileWStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKFileWStream (string path)
			: base (CreateNew (path), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKFileWStream instance.");
			}
		}

		private static IntPtr CreateNew (string path)
		{
			var bytes = StringUtilities.GetEncodedText (path, SKTextEncoding.Utf8);
			fixed (byte* p = bytes) {
				return SkiaApi.sk_filewstream_new (p);
			}
		}

		protected override void DisposeNative () =>
			SkiaApi.sk_filewstream_destroy (Handle);

		public bool IsValid =>
			SkiaApi.sk_filewstream_is_valid (Handle);

		public static bool IsPathSupported (string path) => true;

		public static SKWStream OpenStream (string path)
		{
			var stream = new SKFileWStream (path);
			if (!stream.IsValid) {
				stream.Dispose ();
				stream = null;
			}
			return stream;
		}
	}

	public unsafe class SKDynamicMemoryWStream : SKWStream
	{
		[Preserve]
		internal SKDynamicMemoryWStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKDynamicMemoryWStream ()
			: base (SkiaApi.sk_dynamicmemorywstream_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKDynamicMemoryWStream instance.");
			}
		}

		public SKData CopyToData ()
		{
			var data = SKData.Create (BytesWritten);
			CopyTo (data.Data);
			return data;
		}

		public SKStreamAsset DetachAsStream () =>
			GetObject<SKStreamAsset, SKStreamAssetImplementation> (SkiaApi.sk_dynamicmemorywstream_detach_as_stream (Handle));

		public SKData DetachAsData () =>
			GetObject<SKData> (SkiaApi.sk_dynamicmemorywstream_detach_as_data (Handle));

		public void CopyTo (IntPtr data) =>
			SkiaApi.sk_dynamicmemorywstream_copy_to (Handle, (void*)data);

		public void CopyTo (Span<byte> data)
		{
			fixed (void* d = data) {
				SkiaApi.sk_dynamicmemorywstream_copy_to (Handle, d);
			}
		}

		public bool CopyTo (SKWStream dst)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			return SkiaApi.sk_dynamicmemorywstream_write_to_stream (Handle, dst.Handle);
		}

		protected override void DisposeNative () =>
			SkiaApi.sk_dynamicmemorywstream_destroy (Handle);
	}
}
