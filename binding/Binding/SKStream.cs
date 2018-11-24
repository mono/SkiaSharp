using System;
using System.IO;

namespace SkiaSharp
{
	public abstract class SKStream : SKObject
	{
		internal SKStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
		
		public bool IsAtEnd {
			get {
				return SkiaApi.sk_stream_is_at_end (Handle);
			}
		}

		public SByte ReadSByte ()
		{
			if (ReadSByte (out var buffer))
				return buffer;
			return default (SByte);
		}

		public Int16 ReadInt16 ()
		{
			if (ReadInt16 (out var buffer))
				return buffer;
			return default (Int16);
		}

		public Int32 ReadInt32 ()
		{
			if (ReadInt32 (out var buffer))
				return buffer;
			return default (Int32);
		}

		public Byte ReadByte ()
		{
			if (ReadByte (out var buffer))
				return buffer;
			return default (Byte);
		}

		public UInt16 ReadUInt16 ()
		{
			if (ReadUInt16 (out var buffer))
				return buffer;
			return default (UInt16);
		}

		public UInt32 ReadUInt32 ()
		{
			if (ReadUInt32 (out var buffer))
				return buffer;
			return default (UInt32);
		}

		public bool ReadBool ()
		{
			if (ReadBool (out var buffer))
				return buffer;
			return default (bool);
		}

		public bool ReadSByte (out SByte buffer)
		{
			return SkiaApi.sk_stream_read_s8 (Handle, out buffer);
		}

		public bool ReadInt16 (out Int16 buffer)
		{
			return SkiaApi.sk_stream_read_s16 (Handle, out buffer);
		}

		public bool ReadInt32 (out Int32 buffer)
		{
			return SkiaApi.sk_stream_read_s32 (Handle, out buffer);
		}

		public bool ReadByte (out Byte buffer)
		{
			return SkiaApi.sk_stream_read_u8 (Handle, out buffer);
		}

		public bool ReadUInt16 (out UInt16 buffer)
		{
			return SkiaApi.sk_stream_read_u16 (Handle, out buffer);
		}

		public bool ReadUInt32 (out UInt32 buffer)
		{
			return SkiaApi.sk_stream_read_u32 (Handle, out buffer);
		}

		public bool ReadBool (out Boolean buffer)
		{
			return SkiaApi.sk_stream_read_bool (Handle, out buffer);
		}

		public int Read (byte[] buffer, int size)
		{
			unsafe {
				fixed (byte* b = buffer) {
					return Read ((IntPtr)b, size);
				}
			}
		}

		public int Read (IntPtr buffer, int size)
		{
			return (int)SkiaApi.sk_stream_read (Handle, buffer, (IntPtr)size);
		}

		public int Peek (IntPtr buffer, int size)
		{
			return (int)SkiaApi.sk_stream_peek (Handle, buffer, (IntPtr)size);
		}

		public int Skip (int size)
		{
			return (int)SkiaApi.sk_stream_skip (Handle, (IntPtr)size);
		}

		public bool Rewind ()
		{
			return SkiaApi.sk_stream_rewind (Handle);
		}

		public bool Seek (int position)
		{
			return SkiaApi.sk_stream_seek (Handle, (IntPtr)position);
		}

		public bool Move (long offset)
		{
			return SkiaApi.sk_stream_move (Handle, offset);
		}
		
		public IntPtr GetMemoryBase ()
		{
			return SkiaApi.sk_stream_get_memory_base (Handle);
		}

		public bool HasPosition {
			get {
				return SkiaApi.sk_stream_has_position (Handle);
			}
		}

		public int Position {
			get {
				return (int)SkiaApi.sk_stream_get_position (Handle);
			}
			set { 
				Seek (value);
			}
		}

		public bool HasLength {
			get {
				return SkiaApi.sk_stream_has_length (Handle);
			}
		}

		public int Length {
			get {
				return (int)SkiaApi.sk_stream_get_length (Handle);
			}
		}
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

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_stream_asset_destroy (Handle);
			}

			base.Dispose (disposing);
		}
	}

	public abstract class SKStreamMemory : SKStreamAsset
	{
		internal SKStreamMemory (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
	}

	public class SKFileStream : SKStreamAsset
	{
		[Preserve]
		internal SKFileStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKFileStream (string path)
			: base (SkiaApi.sk_filestream_new (StringUtilities.GetEncodedText (path, SKEncoding.Utf8)), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKFileStream instance.");
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_filestream_destroy (Handle);
			}

			base.Dispose (disposing);
		}

		public bool IsValid => SkiaApi.sk_filestream_is_valid (Handle);

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

	public class SKMemoryStream : SKStreamMemory
	{
		[Preserve]
		internal SKMemoryStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKMemoryStream ()
			: this (SkiaApi.sk_memorystream_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKMemoryStream instance.");
			}
		}

		public SKMemoryStream (ulong length)
			: this(SkiaApi.sk_memorystream_new_with_length ((IntPtr)length), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKMemoryStream instance.");
			}
		}

		internal SKMemoryStream (IntPtr data, IntPtr length, bool copyData = false)
			: this(SkiaApi.sk_memorystream_new_with_data (data, length, copyData), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKMemoryStream instance.");
			}
		}

		public SKMemoryStream (SKData data)
			: this(SkiaApi.sk_memorystream_new_with_skdata (data.Handle), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKMemoryStream instance.");
			}
		}

		public SKMemoryStream (byte[] data)
			: this ()
		{
			SetMemory (data);
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_memorystream_destroy (Handle);
			}

			base.Dispose (disposing);
		}

		internal void SetMemory (IntPtr data, IntPtr length, bool copyData = false)
		{
			SkiaApi.sk_memorystream_set_memory (Handle, data, length, copyData);
		}

		internal void SetMemory (byte[] data, IntPtr length, bool copyData = false)
		{
			SkiaApi.sk_memorystream_set_memory (Handle, data, length, copyData);
		}

		public void SetMemory (byte[] data)
		{
			SetMemory (data, (IntPtr)data.Length, true);
		}
	}

	public abstract class SKWStream : SKObject
	{
		internal SKWStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
		
		public virtual int BytesWritten {
			get {
				return (int)SkiaApi.sk_wstream_bytes_written (Handle);
			}
		}

		public virtual bool Write (byte[] buffer, int size)
		{
			unsafe {
				fixed (byte* b = buffer) {
					return SkiaApi.sk_wstream_write (Handle, (IntPtr)b, (IntPtr)size);
				}
			}
		}

		public bool NewLine ()
		{
			return SkiaApi.sk_wstream_newline (Handle);
		}

		public virtual void Flush ()
		{
			SkiaApi.sk_wstream_flush (Handle);
		}

		public bool Write8 (Byte value)
		{
			return SkiaApi.sk_wstream_write_8 (Handle, value);
		}

		public bool Write16 (UInt16 value)
		{
			return SkiaApi.sk_wstream_write_16 (Handle, value);
		}

		public bool Write32 (UInt32 value)
		{
			return SkiaApi.sk_wstream_write_32 (Handle, value);
		}

		public bool WriteText (string value)
		{
			return SkiaApi.sk_wstream_write_text (Handle, value);
		}

		public bool WriteDecimalAsTest (Int32 value)
		{
			return SkiaApi.sk_wstream_write_dec_as_text (Handle, value);
		}

		public bool WriteBigDecimalAsText (Int64 value, int digits)
		{
			return SkiaApi.sk_wstream_write_bigdec_as_text (Handle, value, digits);
		}

		public bool WriteHexAsText (UInt32 value, int digits)
		{
			return SkiaApi.sk_wstream_write_hex_as_text (Handle, value, digits);
		}

		public bool WriteScalarAsText (float value)
		{
			return SkiaApi.sk_wstream_write_scalar_as_text (Handle, value);
		}

		public bool WriteBool (bool value)
		{
			return SkiaApi.sk_wstream_write_bool (Handle, value);
		}

		public bool WriteScalar (float value)
		{
			return SkiaApi.sk_wstream_write_scalar (Handle, value);
		}

		public bool WritePackedUInt32 (UInt32 value)
		{
			return SkiaApi.sk_wstream_write_packed_uint (Handle, (IntPtr)value);
		}

		public bool WriteStream (SKStream input, int length)
		{
			if (input == null) {
				throw new ArgumentNullException (nameof(input));
			}

			return SkiaApi.sk_wstream_write_stream (Handle, input.Handle, (IntPtr)length);
		}

		public static int GetSizeOfPackedUInt32 (UInt32 value)
		{
			return SkiaApi.sk_wstream_get_size_of_packed_uint ((IntPtr) value);
		}
	}

	public class SKFileWStream : SKWStream
	{
		[Preserve]
		internal SKFileWStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKFileWStream (string path)
			: base (SkiaApi.sk_filewstream_new (StringUtilities.GetEncodedText (path, SKEncoding.Utf8)), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKFileWStream instance.");
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_filewstream_destroy (Handle);
			}

			base.Dispose (disposing);
		}

		public bool IsValid => SkiaApi.sk_filewstream_is_valid (Handle);

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

	public class SKDynamicMemoryWStream : SKWStream
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

		public SKStreamAsset DetachAsStream ()
		{
			return GetObject<SKStreamAssetImplementation> (SkiaApi.sk_dynamicmemorywstream_detach_as_stream (Handle));
		}

		public SKData DetachAsData ()
		{
			return GetObject<SKData> (SkiaApi.sk_dynamicmemorywstream_detach_as_data (Handle));
		}

		public void CopyTo (IntPtr data)
		{
			SkiaApi.sk_dynamicmemorywstream_copy_to (Handle, data);
		}

		public bool CopyTo (SKWStream dst)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			return SkiaApi.sk_dynamicmemorywstream_write_to_stream (Handle, dst.Handle);
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_dynamicmemorywstream_destroy (Handle);
			}

			base.Dispose (disposing);
		}
	}
}
