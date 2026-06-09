#nullable disable

using System;
using System.IO;

namespace SkiaSharp
{
	public unsafe abstract class SKStream : SKObject
	{
		internal SKStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
		
		public bool IsAtEnd {
			get {
				var result = SkiaApi.sk_stream_is_at_end (Handle);
				GC.KeepAlive (this);
				return result;
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
			fixed (SByte* b = &buffer) {
				var result = SkiaApi.sk_stream_read_s8 (Handle, b);
				GC.KeepAlive (this);
				return result;
			}
		}

		public bool ReadInt16 (out Int16 buffer)
		{
			fixed (Int16* b = &buffer) {
				var result = SkiaApi.sk_stream_read_s16 (Handle, b);
				GC.KeepAlive (this);
				return result;
			}
		}

		public bool ReadInt32 (out Int32 buffer)
		{
			fixed (Int32* b = &buffer) {
				var result = SkiaApi.sk_stream_read_s32 (Handle, b);
				GC.KeepAlive (this);
				return result;
			}
		}

		public bool ReadByte (out Byte buffer)
		{
			fixed (Byte* b = &buffer) {
				var result = SkiaApi.sk_stream_read_u8 (Handle, b);
				GC.KeepAlive (this);
				return result;
			}
		}

		public bool ReadUInt16 (out UInt16 buffer)
		{
			fixed (UInt16* b = &buffer) {
				var result = SkiaApi.sk_stream_read_u16 (Handle, b);
				GC.KeepAlive (this);
				return result;
			}
		}

		public bool ReadUInt32 (out UInt32 buffer)
		{
			fixed (UInt32* b = &buffer) {
				var result = SkiaApi.sk_stream_read_u32 (Handle, b);
				GC.KeepAlive (this);
				return result;
			}
		}

		public bool ReadBool (out Boolean buffer)
		{
			byte b;
			var result = SkiaApi.sk_stream_read_bool (Handle, &b);
			GC.KeepAlive (this);
			buffer = b > 0;
			return result;
		}

		public int Read (byte[] buffer, int size)
		{
			fixed (byte* b = buffer) {
				return Read ((IntPtr)b, size);
			}
		}

		public int Read (IntPtr buffer, int size)
		{
			var result = (int)SkiaApi.sk_stream_read (Handle, (void*)buffer, (IntPtr)size);
			GC.KeepAlive (this);
			return result;
		}

		public int Peek (IntPtr buffer, int size)
		{
			var result = (int)SkiaApi.sk_stream_peek (Handle, (void*)buffer, (IntPtr)size);
			GC.KeepAlive (this);
			return result;
		}

		public int Skip (int size)
		{
			var result = (int)SkiaApi.sk_stream_skip (Handle, (IntPtr)size);
			GC.KeepAlive (this);
			return result;
		}

		public bool Rewind ()
		{
			var result = SkiaApi.sk_stream_rewind (Handle);
			GC.KeepAlive (this);
			return result;
		}

		public bool Seek (int position)
		{
			var result = SkiaApi.sk_stream_seek (Handle, (IntPtr)position);
			GC.KeepAlive (this);
			return result;
		}

		[Obsolete ("The native stream move offset is capped at a 32-bit int. Use Move(int) instead.")]
		public bool Move (long offset) => Move (checked ((int)offset));

		public bool Move (int offset)
		{
			var result = SkiaApi.sk_stream_move (Handle, offset);
			GC.KeepAlive (this);
			return result;
		}

		public IntPtr GetMemoryBase ()
		{
			var result = (IntPtr)SkiaApi.sk_stream_get_memory_base (Handle);
			GC.KeepAlive (this);
			return result;
		}

		public SKData GetData ()
		{
			var result = SKData.GetObject (SkiaApi.sk_stream_get_data (Handle));
			GC.KeepAlive (this);
			return result;
		}

		internal SKStream Fork ()
		{
			var result = GetObject (SkiaApi.sk_stream_fork (Handle));
			GC.KeepAlive (this);
			return result;
		}

		internal SKStream Duplicate ()
		{
			var result = GetObject (SkiaApi.sk_stream_duplicate (Handle));
			GC.KeepAlive (this);
			return result;
		}

		public bool HasPosition {
			get {
				var result = SkiaApi.sk_stream_has_position (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		public int Position {
			get {
				var result = (int)SkiaApi.sk_stream_get_position (Handle);
				GC.KeepAlive (this);
				return result;
			}
			set {
				Seek (value);
			}
		}

		public bool HasLength {
			get {
				var result = SkiaApi.sk_stream_has_length (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		public int Length {
			get {
				var result = (int)SkiaApi.sk_stream_get_length (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		internal static SKStream GetObject (IntPtr handle) =>
			GetOrAddObject<SKStream> (handle, (h, o) => new SKStreamImplementation (h, o));
	}

	internal class SKStreamImplementation : SKStream
	{
		internal SKStreamImplementation (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

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

		internal static new SKStreamAsset GetObject (IntPtr handle) =>
			GetOrAddObject<SKStreamAsset> (handle, (h, o) => new SKStreamAssetImplementation (h, o));
	}

	internal class SKStreamAssetImplementation : SKStreamAsset
	{
		internal SKStreamAssetImplementation (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

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
		internal SKFileStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKFileStream (string path)
			: base (CreateNew (path), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKFileStream instance.");
			}
		}

		private static IntPtr CreateNew (string path)
		{
			var bytes = StringUtilities.GetEncodedText (path, SKTextEncoding.Utf8, true);
			fixed (byte* p = bytes) {
				return SkiaApi.sk_filestream_new (p);
			}
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_filestream_destroy (Handle);

		public bool IsValid {
			get {
				var result = SkiaApi.sk_filestream_is_valid (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

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
			: this(SkiaApi.sk_memorystream_new_with_data ((void*)data, length, copyData), true)
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

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_memorystream_destroy (Handle);

		internal void SetMemory (IntPtr data, IntPtr length, bool copyData = false)
		{
			SkiaApi.sk_memorystream_set_memory (Handle, (void*)data, length, copyData);
			GC.KeepAlive (this);
		}

		internal void SetMemory (byte[] data, IntPtr length, bool copyData = false)
		{
			fixed (byte* d = data) {
				SkiaApi.sk_memorystream_set_memory (Handle, d, length, copyData);
				GC.KeepAlive (this);
			}
		}

		public void SetMemory (byte[] data)
		{
			SetMemory (data, (IntPtr)data.Length, true);
		}
	}

	public unsafe abstract class SKWStream : SKObject
	{
		internal SKWStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
		
		public virtual int BytesWritten {
			get {
				var result = (int)SkiaApi.sk_wstream_bytes_written (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		public virtual bool Write (byte[] buffer, int size)
		{
			fixed (byte* b = buffer) {
				var result = SkiaApi.sk_wstream_write (Handle, (void*)b, (IntPtr)size);
				GC.KeepAlive (this);
				return result;
			}
		}

		public bool NewLine ()
		{
			var result = SkiaApi.sk_wstream_newline (Handle);
			GC.KeepAlive (this);
			return result;
		}

		public virtual void Flush ()
		{
			SkiaApi.sk_wstream_flush (Handle);
			GC.KeepAlive (this);
		}

		public bool Write8 (Byte value)
		{
			var result = SkiaApi.sk_wstream_write_8 (Handle, value);
			GC.KeepAlive (this);
			return result;
		}

		public bool Write16 (UInt16 value)
		{
			var result = SkiaApi.sk_wstream_write_16 (Handle, value);
			GC.KeepAlive (this);
			return result;
		}

		public bool Write32 (UInt32 value)
		{
			var result = SkiaApi.sk_wstream_write_32 (Handle, value);
			GC.KeepAlive (this);
			return result;
		}

		public bool WriteText (string value)
		{
			var result = SkiaApi.sk_wstream_write_text (Handle, value);
			GC.KeepAlive (this);
			return result;
		}

		public bool WriteDecimalAsTest (Int32 value)
		{
			var result = SkiaApi.sk_wstream_write_dec_as_text (Handle, value);
			GC.KeepAlive (this);
			return result;
		}

		public bool WriteBigDecimalAsText (Int64 value, int digits)
		{
			var result = SkiaApi.sk_wstream_write_bigdec_as_text (Handle, value, digits);
			GC.KeepAlive (this);
			return result;
		}

		public bool WriteHexAsText (UInt32 value, int digits)
		{
			var result = SkiaApi.sk_wstream_write_hex_as_text (Handle, value, digits);
			GC.KeepAlive (this);
			return result;
		}

		public bool WriteScalarAsText (float value)
		{
			var result = SkiaApi.sk_wstream_write_scalar_as_text (Handle, value);
			GC.KeepAlive (this);
			return result;
		}

		public bool WriteBool (bool value)
		{
			var result = SkiaApi.sk_wstream_write_bool (Handle, value);
			GC.KeepAlive (this);
			return result;
		}

		public bool WriteScalar (float value)
		{
			var result = SkiaApi.sk_wstream_write_scalar (Handle, value);
			GC.KeepAlive (this);
			return result;
		}

		public bool WritePackedUInt32 (UInt32 value)
		{
			var result = SkiaApi.sk_wstream_write_packed_uint (Handle, (IntPtr)value);
			GC.KeepAlive (this);
			return result;
		}

		public bool WriteStream (SKStream input, int length)
		{
			if (input == null) {
				throw new ArgumentNullException (nameof(input));
			}

			var result = SkiaApi.sk_wstream_write_stream (Handle, input.Handle, (IntPtr)length);
			GC.KeepAlive (input);
			GC.KeepAlive (this);
			return result;
		}

		public static int GetSizeOfPackedUInt32 (UInt32 value)
		{
			return SkiaApi.sk_wstream_get_size_of_packed_uint ((IntPtr) value);
		}
	}

	public unsafe class SKFileWStream : SKWStream
	{
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
			var bytes = StringUtilities.GetEncodedText (path, SKTextEncoding.Utf8, true);
			fixed (byte* p = bytes) {
				return SkiaApi.sk_filewstream_new (p);
			}
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_filewstream_destroy (Handle);

		public bool IsValid {
			get {
				var result = SkiaApi.sk_filewstream_is_valid (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

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
			var result = SKStreamAssetImplementation.GetObject (SkiaApi.sk_dynamicmemorywstream_detach_as_stream (Handle));
			GC.KeepAlive (this);
			return result;
		}

		public SKData DetachAsData ()
		{
			var result = SKData.GetObject (SkiaApi.sk_dynamicmemorywstream_detach_as_data (Handle));
			GC.KeepAlive (this);
			return result;
		}

		public void CopyTo (IntPtr data)
		{
			SkiaApi.sk_dynamicmemorywstream_copy_to (Handle, (void*)data);
			GC.KeepAlive (this);
		}

		public void CopyTo (Span<byte> data)
		{
			var size = BytesWritten;
			if (data.Length < size)
				throw new Exception ($"Not enough space to copy. Expected at least {size}, but received {data.Length}.");

			fixed (void* d = data) {
				SkiaApi.sk_dynamicmemorywstream_copy_to (Handle, d);
				GC.KeepAlive (this);
			}
		}

		public bool CopyTo (SKWStream dst)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			var result = SkiaApi.sk_dynamicmemorywstream_write_to_stream (Handle, dst.Handle);
			GC.KeepAlive (dst);
			GC.KeepAlive (this);
			return result;
		}

		public bool CopyTo (Stream dst)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			using var wrapped = new SKManagedWStream (dst);
			return CopyTo (wrapped);
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_dynamicmemorywstream_destroy (Handle);
	}
}
