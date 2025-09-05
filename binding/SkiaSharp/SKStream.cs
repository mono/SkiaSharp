#nullable disable

using System;
using System.IO;

namespace SkiaSharp
{
	/// <summary>
	/// An abstraction for a source of bytes, which can be backed by memory, or a file, or something else.
	/// </summary>
	public unsafe abstract class SKStream : SKObject
	{
		internal SKStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Gets a value indicating whether all the bytes in the stream have been read.
		/// </summary>
		/// <remarks>This method may return true if there was an error, and the stream cannot be read anymore.</remarks>
		public bool IsAtEnd {
			get {
				return SkiaApi.sk_stream_is_at_end (Handle);
			}
		}

		/// <summary>
		/// Read a single, signed byte.
		/// </summary>
		/// <returns>Returns the signed byte that was read.</returns>
		public SByte ReadSByte ()
		{
			if (ReadSByte (out var buffer))
				return buffer;
			return default (SByte);
		}

		/// <summary>
		/// Read a single 16-bit integer.
		/// </summary>
		/// <returns>Returns the 16-bit integer that was read.</returns>
		public Int16 ReadInt16 ()
		{
			if (ReadInt16 (out var buffer))
				return buffer;
			return default (Int16);
		}

		/// <summary>
		/// Read a single 32-bit integer.
		/// </summary>
		/// <returns>Returns the 32-bit integer that was read.</returns>
		public Int32 ReadInt32 ()
		{
			if (ReadInt32 (out var buffer))
				return buffer;
			return default (Int32);
		}

		/// <summary>
		/// Read a single byte.
		/// </summary>
		/// <returns>Returns the byte that was read.</returns>
		public Byte ReadByte ()
		{
			if (ReadByte (out var buffer))
				return buffer;
			return default (Byte);
		}

		/// <summary>
		/// Read a single, unsigned 16-bit integer.
		/// </summary>
		/// <returns>Returns the unsigned 16-bit integer that was read.</returns>
		public UInt16 ReadUInt16 ()
		{
			if (ReadUInt16 (out var buffer))
				return buffer;
			return default (UInt16);
		}

		/// <summary>
		/// Read a single, unsigned 32-bit integer.
		/// </summary>
		/// <returns>Returns the unsigned 32-bit integer that was read.</returns>
		public UInt32 ReadUInt32 ()
		{
			if (ReadUInt32 (out var buffer))
				return buffer;
			return default (UInt32);
		}

		/// <summary>
		/// Read a single boolean.
		/// </summary>
		/// <returns>Returns the boolean that was read.</returns>
		public bool ReadBool ()
		{
			if (ReadBool (out var buffer))
				return buffer;
			return default (bool);
		}

		/// <summary>
		/// Read a single, signed byte.
		/// </summary>
		/// <param name="buffer">The signed byte that was read.</param>
		/// <returns>Returns true if the read was successful, otherwise false.</returns>
		public bool ReadSByte (out SByte buffer)
		{
			fixed (SByte* b = &buffer) {
				return SkiaApi.sk_stream_read_s8 (Handle, b);
			}
		}

		/// <summary>
		/// Read a single 16-bit integer.
		/// </summary>
		/// <param name="buffer">The 16-bit integer that was read.</param>
		/// <returns>Returns true if the read was successful, otherwise false.</returns>
		public bool ReadInt16 (out Int16 buffer)
		{
			fixed (Int16* b = &buffer) {
				return SkiaApi.sk_stream_read_s16 (Handle, b);
			}
		}

		/// <summary>
		/// Read a single 32-bit integer.
		/// </summary>
		/// <param name="buffer">The 32-bit integer that was read.</param>
		/// <returns>Returns true if the read was successful, otherwise false.</returns>
		public bool ReadInt32 (out Int32 buffer)
		{
			fixed (Int32* b = &buffer) {
				return SkiaApi.sk_stream_read_s32 (Handle, b);
			}
		}

		/// <summary>
		/// Read a single byte.
		/// </summary>
		/// <param name="buffer">The byte that was read.</param>
		/// <returns>Returns true if the read was successful, otherwise false.</returns>
		public bool ReadByte (out Byte buffer)
		{
			fixed (Byte* b = &buffer) {
				return SkiaApi.sk_stream_read_u8 (Handle, b);
			}
		}

		/// <summary>
		/// Read a single, unsigned 16-bit integer.
		/// </summary>
		/// <param name="buffer">The unsigned 16-bit integer that was read.</param>
		/// <returns>Returns true if the read was successful, otherwise false.</returns>
		public bool ReadUInt16 (out UInt16 buffer)
		{
			fixed (UInt16* b = &buffer) {
				return SkiaApi.sk_stream_read_u16 (Handle, b);
			}
		}

		/// <summary>
		/// Read a single, unsigned 32-bit integer.
		/// </summary>
		/// <param name="buffer">The unsigned 32-bit integer that was read.</param>
		/// <returns>Returns true if the read was successful, otherwise false.</returns>
		public bool ReadUInt32 (out UInt32 buffer)
		{
			fixed (UInt32* b = &buffer) {
				return SkiaApi.sk_stream_read_u32 (Handle, b);
			}
		}

		/// <summary>
		/// Read a single boolean.
		/// </summary>
		/// <param name="buffer">The boolean that was read.</param>
		/// <returns>Returns true if the read was successful, otherwise false.</returns>
		public bool ReadBool (out Boolean buffer)
		{
			byte b;
			var result = SkiaApi.sk_stream_read_bool (Handle, &b);
			buffer = b > 0;
			return result;
		}

		/// <summary>
		/// Reads a copy of the specified number of bytes into the provided buffer.
		/// </summary>
		/// <param name="buffer">The buffer to read into.</param>
		/// <param name="size">The number of bytes to read.</param>
		/// <returns>Returns the number of bytes actually read.</returns>
		public int Read (byte[] buffer, int size)
		{
			fixed (byte* b = buffer) {
				return Read ((IntPtr)b, size);
			}
		}

		/// <summary>
		/// Reads a copy of the specified number of bytes into the provided buffer.
		/// </summary>
		/// <param name="buffer">The buffer to read into.</param>
		/// <param name="size">The number of bytes to read.</param>
		/// <returns>Returns the number of bytes actually read.</returns>
		public int Read (IntPtr buffer, int size)
		{
			return (int)SkiaApi.sk_stream_read (Handle, (void*)buffer, (IntPtr)size);
		}

		/// <summary>
		/// Attempt to peek at <paramref name="size" /> bytes.
		/// </summary>
		/// <param name="buffer">The buffer to read into.</param>
		/// <param name="size">The number of bytes to read.</param>
		/// <returns>Returns the number of bytes actually peeked/copied.</returns>
		public int Peek (IntPtr buffer, int size)
		{
			return (int)SkiaApi.sk_stream_peek (Handle, (void*)buffer, (IntPtr)size);
		}

		/// <summary>
		/// Moves the current position on by the specified number of bytes.
		/// </summary>
		/// <param name="size">The number of bytes to skip.</param>
		/// <returns>Returns the actual number bytes that could be skipped.</returns>
		public int Skip (int size)
		{
			return (int)SkiaApi.sk_stream_skip (Handle, (IntPtr)size);
		}

		/// <summary>
		/// Rewinds to the beginning of the stream.
		/// </summary>
		/// <returns>Returns true if the stream is known to be at the beginning after this call returns.</returns>
		public bool Rewind ()
		{
			return SkiaApi.sk_stream_rewind (Handle);
		}

		/// <summary>
		/// Seeks to an absolute position in the stream.
		/// </summary>
		/// <param name="position">The absolute position.</param>
		/// <returns>Returns true if seeking is supported and the seek was successful, otherwise false.</returns>
		/// <remarks>If an attempt is made to move to a position outside the stream, the position will be set to the closest point within the stream (beginning or end).</remarks>
		public bool Seek (int position)
		{
			return SkiaApi.sk_stream_seek (Handle, (IntPtr)position);
		}

		/// <summary>
		/// Seeks to an relative offset in the stream.
		/// </summary>
		/// <param name="offset">The relative offset.</param>
		/// <returns>Returns true if seeking is supported and the seek was successful, otherwise false.</returns>
		/// <remarks>If an attempt is made to move to a position outside the stream, the position will be set to the closest point within the stream (beginning or end).</remarks>
		public bool Move (long offset) => Move ((int)offset);

		/// <summary>
		/// Seeks to an relative offset in the stream.
		/// </summary>
		/// <param name="offset">The relative offset.</param>
		/// <returns>Returns true if seeking is supported and the seek was successful, otherwise false.</returns>
		/// <remarks>If an attempt is made to move to a position outside the stream, the position will be set to the closest point within the stream (beginning or end).</remarks>
		public bool Move (int offset)
		{
			return SkiaApi.sk_stream_move (Handle, offset);
		}

		/// <summary>
		/// Returns the memory address of the data if the stream is a memory stream.
		/// </summary>
		/// <returns>Returns the memory address of the data, or IntPtr.Zero if the stream is not a memory stream.</returns>
		public IntPtr GetMemoryBase ()
		{
			return (IntPtr)SkiaApi.sk_stream_get_memory_base (Handle);
		}

		internal SKStream Fork () => GetObject (SkiaApi.sk_stream_fork (Handle));

		internal SKStream Duplicate () => GetObject (SkiaApi.sk_stream_duplicate (Handle));

		/// <summary>
		/// Gets a value indicating whether this stream can report it's current position.
		/// </summary>
		public bool HasPosition {
			get {
				return SkiaApi.sk_stream_has_position (Handle);
			}
		}

		/// <summary>
		/// Gets the current position in the stream. If this is not supported, the position will be reported as 0.
		/// </summary>
		public int Position {
			get {
				return (int)SkiaApi.sk_stream_get_position (Handle);
			}
			set { 
				Seek (value);
			}
		}

		/// <summary>
		/// Gets a value indicating whether this stream can report it's total length.
		/// </summary>
		public bool HasLength {
			get {
				return SkiaApi.sk_stream_has_length (Handle);
			}
		}

		/// <summary>
		/// Gets the total length of the stream. If this is not supported, the length will be reported as 0.
		/// </summary>
		public int Length {
			get {
				return (int)SkiaApi.sk_stream_get_length (Handle);
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

	/// <summary>
	/// An abstract, rewindable stream.
	/// </summary>
	public abstract class SKStreamRewindable : SKStream
	{
		internal SKStreamRewindable (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
	}

	/// <summary>
	/// An abstract, rewindable stream that supports the seek operation.
	/// </summary>
	public abstract class SKStreamSeekable : SKStreamRewindable
	{
		internal SKStreamSeekable (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
	}

	/// <summary>
	/// An abstract, seekable stream with a known length.
	/// </summary>
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

	/// <summary>
	/// An abstract, memory-based stream.
	/// </summary>
	public abstract class SKStreamMemory : SKStreamAsset
	{
		internal SKStreamMemory (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
	}

	/// <summary>
	/// A seekable stream backed by a file on the file system.
	/// </summary>
	public unsafe class SKFileStream : SKStreamAsset
	{
		internal SKFileStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKFileStream" /> that wraps the file with the specified path.
		/// </summary>
		/// <param name="path">The existing file to open for reading.</param>
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

		/// <summary>
		/// Gets a value indicating whether the file could be opened.
		/// </summary>
		public bool IsValid => SkiaApi.sk_filestream_is_valid (Handle);

		/// <summary>
		/// Determines whether the specified path is supported by a <see cref="T:SkiaSharp.SKFileStream" />.
		/// </summary>
		/// <param name="path">The path to check.</param>
		/// <returns>Returns <see langword="true" /> if the path is supported, otherwise <see langword="false" />.</returns>
		public static bool IsPathSupported (string path) => true;

		/// <summary>
		/// Opens a read-only stream to the specified file.
		/// </summary>
		/// <param name="path">The path to the file to open.</param>
		/// <returns>Returns a stream that contains the file contents.</returns>
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

	/// <summary>
	/// A memory-based stream.
	/// </summary>
	public unsafe class SKMemoryStream : SKStreamMemory
	{
		internal SKMemoryStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKMemoryStream" /> with an empty buffer.
		/// </summary>
		public SKMemoryStream ()
			: this (SkiaApi.sk_memorystream_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKMemoryStream instance.");
			}
		}

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKMemoryStream" /> with a buffer size of the specified size.
		/// </summary>
		/// <param name="length">The size of the stream buffer.</param>
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

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKMemoryStream" /> with the buffer being the provided data.
		/// </summary>
		/// <param name="data">The data to initialize the stream with.</param>
		public SKMemoryStream (SKData data)
			: this(SkiaApi.sk_memorystream_new_with_skdata (data.Handle), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKMemoryStream instance.");
			}
		}

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKMemoryStream" /> with a copy of the provided data.
		/// </summary>
		/// <param name="data">The data to initialize the stream with.</param>
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
		}

		internal void SetMemory (byte[] data, IntPtr length, bool copyData = false)
		{
			fixed (byte* d = data) {
				SkiaApi.sk_memorystream_set_memory (Handle, d, length, copyData);
			}
		}

		/// <summary>
		/// Resets the stream with a copy of the provided data.
		/// </summary>
		/// <param name="data">The data to reset the stream to.</param>
		public void SetMemory (byte[] data)
		{
			SetMemory (data, (IntPtr)data.Length, true);
		}
	}

	/// <summary>
	/// An abstraction for writing a stream of bytes, which can be backed by memory, or a file, or something else.
	/// </summary>
	public unsafe abstract class SKWStream : SKObject
	{
		internal SKWStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Gets the number of bytes written so far.
		/// </summary>
		public virtual int BytesWritten {
			get {
				return (int)SkiaApi.sk_wstream_bytes_written (Handle);
			}
		}

		/// <summary>
		/// Write the provided data to the stream.
		/// </summary>
		/// <param name="buffer">The data buffer to write.</param>
		/// <param name="size">The number of bytes from the buffer to write.</param>
		/// <returns>Returns true if the write succeeded, otherwise false.</returns>
		public virtual bool Write (byte[] buffer, int size)
		{
			fixed (byte* b = buffer) {
				return SkiaApi.sk_wstream_write (Handle, (void*)b, (IntPtr)size);
			}
		}

		/// <summary>
		/// Write a newline character to the stream, if one was not already written.
		/// </summary>
		/// <returns>Returns true if the write succeeded, otherwise false.</returns>
		/// <remarks>If the last character was a newline character, this method does nothing.</remarks>
		public bool NewLine ()
		{
			return SkiaApi.sk_wstream_newline (Handle);
		}

		/// <summary>
		/// Flush the buffer to the underlying destination.
		/// </summary>
		public virtual void Flush ()
		{
			SkiaApi.sk_wstream_flush (Handle);
		}

		/// <summary>
		/// Write a single byte to the stream.
		/// </summary>
		/// <param name="value">The byte to write.</param>
		/// <returns>Returns true if the write succeeded, otherwise false.</returns>
		public bool Write8 (Byte value)
		{
			return SkiaApi.sk_wstream_write_8 (Handle, value);
		}

		/// <summary>
		/// Write a single, unsigned 16-bit integer to the stream.
		/// </summary>
		/// <param name="value">The unsigned 16-bit integer to write.</param>
		/// <returns>Returns true if the write succeeded, otherwise false.</returns>
		public bool Write16 (UInt16 value)
		{
			return SkiaApi.sk_wstream_write_16 (Handle, value);
		}

		/// <summary>
		/// Write a single, unsigned 32-bit integer to the stream.
		/// </summary>
		/// <param name="value">The unsigned 32-bit integer to write.</param>
		/// <returns>Returns true if the write succeeded, otherwise false.</returns>
		public bool Write32 (UInt32 value)
		{
			return SkiaApi.sk_wstream_write_32 (Handle, value);
		}

		/// <summary>
		/// Write a string to the stream as a string.
		/// </summary>
		/// <param name="value">The string to write.</param>
		/// <returns>Returns true if the write succeeded, otherwise false.</returns>
		public bool WriteText (string value)
		{
			return SkiaApi.sk_wstream_write_text (Handle, value);
		}

		/// <summary>
		/// Write a 32-bit integer to the stream as a string.
		/// </summary>
		/// <param name="value">The 32-bit integer to write.</param>
		/// <returns>Returns true if the write succeeded, otherwise false.</returns>
		public bool WriteDecimalAsTest (Int32 value)
		{
			return SkiaApi.sk_wstream_write_dec_as_text (Handle, value);
		}

		/// <summary>
		/// Write a single 64-bit integer to the stream as a string.
		/// </summary>
		/// <param name="value">The 64-bit integer to write.</param>
		/// <param name="digits">The number of digits (length) to use when writing.</param>
		/// <returns>Returns true if the write succeeded, otherwise false.</returns>
		public bool WriteBigDecimalAsText (Int64 value, int digits)
		{
			return SkiaApi.sk_wstream_write_bigdec_as_text (Handle, value, digits);
		}

		/// <summary>
		/// Write an unsigned, 32-bit integer to the stream as a hexadecimal string.
		/// </summary>
		/// <param name="value">The unsigned, 32-bit integer to write.</param>
		/// <param name="digits">The number of digits (length) to use when writing.</param>
		/// <returns>Returns true if the write succeeded, otherwise false.</returns>
		public bool WriteHexAsText (UInt32 value, int digits)
		{
			return SkiaApi.sk_wstream_write_hex_as_text (Handle, value, digits);
		}

		/// <summary>
		/// Write a single, floating-point number to the stream as text.
		/// </summary>
		/// <param name="value">The floating-point number to write.</param>
		/// <returns>Returns true if the write succeeded, otherwise false.</returns>
		public bool WriteScalarAsText (float value)
		{
			return SkiaApi.sk_wstream_write_scalar_as_text (Handle, value);
		}

		/// <summary>
		/// Write a single boolean to the stream.
		/// </summary>
		/// <param name="value">The boolean to write.</param>
		/// <returns>Returns true if the write succeeded, otherwise false.</returns>
		public bool WriteBool (bool value)
		{
			return SkiaApi.sk_wstream_write_bool (Handle, value);
		}

		/// <summary>
		/// Write a single, floating-point number to the stream.
		/// </summary>
		/// <param name="value">The floating-point number to write.</param>
		/// <returns>Returns true if the write succeeded, otherwise false.</returns>
		public bool WriteScalar (float value)
		{
			return SkiaApi.sk_wstream_write_scalar (Handle, value);
		}

		/// <summary>
		/// Write a single, unsigned 32-bit integer to the stream in the smallest space possible.
		/// </summary>
		/// <param name="value">The unsigned 32-bit integer to write.</param>
		/// <returns>Returns true if the write succeeded, otherwise false.</returns>
		public bool WritePackedUInt32 (UInt32 value)
		{
			return SkiaApi.sk_wstream_write_packed_uint (Handle, (IntPtr)value);
		}

		/// <summary>
		/// Write the contents of the specified stream to this stream.
		/// </summary>
		/// <param name="input">The stream to write.</param>
		/// <param name="length">The number of bytes to write.</param>
		/// <returns>Returns true if the write succeeded, otherwise false.</returns>
		public bool WriteStream (SKStream input, int length)
		{
			if (input == null) {
				throw new ArgumentNullException (nameof(input));
			}

			return SkiaApi.sk_wstream_write_stream (Handle, input.Handle, (IntPtr)length);
		}

		/// <summary>
		/// Returns the number of bytes in the stream required to store the specified value.
		/// </summary>
		/// <param name="value">The value to store.</param>
		/// <returns>Returns the number of bytes required.</returns>
		public static int GetSizeOfPackedUInt32 (UInt32 value)
		{
			return SkiaApi.sk_wstream_get_size_of_packed_uint ((IntPtr) value);
		}
	}

	/// <summary>
	/// A writeable stream backed by a file on the file system.
	/// </summary>
	public unsafe class SKFileWStream : SKWStream
	{
		internal SKFileWStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKFileWStream" /> that wraps the file with the specified path.
		/// </summary>
		/// <param name="path">The new or existing file to open for writing.</param>
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

		/// <summary>
		/// Gets a value indicating whether the file could be opened.
		/// </summary>
		public bool IsValid => SkiaApi.sk_filewstream_is_valid (Handle);

		/// <summary>
		/// Determines whether the specified path is supported by a <see cref="T:SkiaSharp.SKFileWStream" />.
		/// </summary>
		/// <param name="path">The path to check.</param>
		/// <returns>Returns <see langword="true" /> if the path is supported, otherwise <see langword="false" />.</returns>
		public static bool IsPathSupported (string path) => true;

		/// <summary>
		/// Opens a write-only stream to the specified file.
		/// </summary>
		/// <param name="path">The path to the file to open.</param>
		/// <returns>Returns a stream that contains the file contents.</returns>
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

	/// <summary>
	/// A writeable, dynamically-sized, memory-based stream.
	/// </summary>
	public unsafe class SKDynamicMemoryWStream : SKWStream
	{
		internal SKDynamicMemoryWStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Create a new instance of <see cref="T:SkiaSharp.SKDynamicMemoryWStream" /> with an empty buffer.
		/// </summary>
		public SKDynamicMemoryWStream ()
			: base (SkiaApi.sk_dynamicmemorywstream_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKDynamicMemoryWStream instance.");
			}
		}

		/// <summary>
		/// Returns a copy of the data written so far.
		/// </summary>
		/// <returns>A copy of the data.</returns>
		/// <remarks>The caller is responsible for releasing the memory.</remarks>
		public SKData CopyToData ()
		{
			var data = SKData.Create (BytesWritten);
			CopyTo (data.Data);
			return data;
		}

		/// <summary>
		/// Returns a read-only stream with the current data, and then resets the current stream.
		/// </summary>
		/// <returns>The stream with the data.</returns>
		/// <remarks>After calling this method, this stream is reset to it's empty state.</remarks>
		public SKStreamAsset DetachAsStream ()
		{
			return SKStreamAssetImplementation.GetObject (SkiaApi.sk_dynamicmemorywstream_detach_as_stream (Handle));
		}

		/// <summary>
		/// Returns a <see cref="T:SkiaSharp.SKData" /> instance of the data in the current stream, and then resets the current stream.
		/// </summary>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKData" /> instance.</returns>
		/// <remarks>After calling this method, this stream is reset to it's empty state.</remarks>
		public SKData DetachAsData ()
		{
			return SKData.GetObject (SkiaApi.sk_dynamicmemorywstream_detach_as_data (Handle));
		}

		/// <summary>
		/// Copies the data from the current stream to a memory location.
		/// </summary>
		/// <param name="data">The memory location to copy the data to.</param>
		public void CopyTo (IntPtr data)
		{
			SkiaApi.sk_dynamicmemorywstream_copy_to (Handle, (void*)data);
		}

		/// <param name="data"></param>
		public void CopyTo (Span<byte> data)
		{
			var size = BytesWritten;
			if (data.Length < size)
				throw new Exception ($"Not enough space to copy. Expected at least {size}, but received {data.Length}.");

			fixed (void* d = data) {
				SkiaApi.sk_dynamicmemorywstream_copy_to (Handle, d);
			}
		}

		/// <summary>
		/// Copies the data from the current stream to the specified stream.
		/// </summary>
		/// <param name="dst">The stream to copy the data to.</param>
		public bool CopyTo (SKWStream dst)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			return SkiaApi.sk_dynamicmemorywstream_write_to_stream (Handle, dst.Handle);
		}

		/// <param name="dst"></param>
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
