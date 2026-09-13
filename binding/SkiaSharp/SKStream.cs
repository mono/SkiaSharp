#nullable disable

using System;
using System.IO;

namespace SkiaSharp
{
	/// <summary>An abstraction for a source of bytes, which can be backed by memory, or a file, or something else.</summary>
	/// <remarks />
	public unsafe abstract class SKStream : SKObject
	{
		internal SKStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
		
		/// <summary>Gets a value indicating whether all the bytes in the stream have been read.</summary>
		/// <value><see langword="true" /> if all the bytes have been read; otherwise, <see langword="false" />.</value>
		/// <remarks>This property may return <see langword="true" /> if there was an error, and the stream cannot be read anymore.</remarks>
		public bool IsAtEnd {
			get {
				var result = SkiaApi.sk_stream_is_at_end (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Read a single, signed byte.</summary>
		/// <returns>Returns the signed byte that was read.</returns>
		/// <remarks />
		public SByte ReadSByte ()
		{
			if (ReadSByte (out var buffer))
				return buffer;
			return default (SByte);
		}

		/// <summary>Read a single 16-bit integer.</summary>
		/// <returns>Returns the 16-bit integer that was read.</returns>
		/// <remarks />
		public Int16 ReadInt16 ()
		{
			if (ReadInt16 (out var buffer))
				return buffer;
			return default (Int16);
		}

		/// <summary>Read a single 32-bit integer.</summary>
		/// <returns>Returns the 32-bit integer that was read.</returns>
		/// <remarks />
		public Int32 ReadInt32 ()
		{
			if (ReadInt32 (out var buffer))
				return buffer;
			return default (Int32);
		}

		/// <summary>Read a single byte.</summary>
		/// <returns>Returns the byte that was read.</returns>
		/// <remarks />
		public Byte ReadByte ()
		{
			if (ReadByte (out var buffer))
				return buffer;
			return default (Byte);
		}

		/// <summary>Read a single, unsigned 16-bit integer.</summary>
		/// <returns>Returns the unsigned 16-bit integer that was read.</returns>
		/// <remarks />
		public UInt16 ReadUInt16 ()
		{
			if (ReadUInt16 (out var buffer))
				return buffer;
			return default (UInt16);
		}

		/// <summary>Read a single, unsigned 32-bit integer.</summary>
		/// <returns>Returns the unsigned 32-bit integer that was read.</returns>
		/// <remarks />
		public UInt32 ReadUInt32 ()
		{
			if (ReadUInt32 (out var buffer))
				return buffer;
			return default (UInt32);
		}

		/// <summary>Read a single boolean.</summary>
		/// <returns>Returns the boolean that was read.</returns>
		/// <remarks />
		public bool ReadBool ()
		{
			if (ReadBool (out var buffer))
				return buffer;
			return default (bool);
		}

		/// <summary>Read a single, signed byte.</summary>
		/// <param name="buffer">The signed byte that was read.</param>
		/// <returns><see langword="true" /> if the read was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool ReadSByte (out SByte buffer)
		{
			fixed (SByte* b = &buffer) {
				var result = SkiaApi.sk_stream_read_s8 (Handle, b);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Read a single 16-bit integer.</summary>
		/// <param name="buffer">The 16-bit integer that was read.</param>
		/// <returns><see langword="true" /> if the read was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool ReadInt16 (out Int16 buffer)
		{
			fixed (Int16* b = &buffer) {
				var result = SkiaApi.sk_stream_read_s16 (Handle, b);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Read a single 32-bit integer.</summary>
		/// <param name="buffer">The 32-bit integer that was read.</param>
		/// <returns><see langword="true" /> if the read was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool ReadInt32 (out Int32 buffer)
		{
			fixed (Int32* b = &buffer) {
				var result = SkiaApi.sk_stream_read_s32 (Handle, b);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Read a single byte.</summary>
		/// <param name="buffer">The byte that was read.</param>
		/// <returns><see langword="true" /> if the read was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool ReadByte (out Byte buffer)
		{
			fixed (Byte* b = &buffer) {
				var result = SkiaApi.sk_stream_read_u8 (Handle, b);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Read a single, unsigned 16-bit integer.</summary>
		/// <param name="buffer">The unsigned 16-bit integer that was read.</param>
		/// <returns><see langword="true" /> if the read was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool ReadUInt16 (out UInt16 buffer)
		{
			fixed (UInt16* b = &buffer) {
				var result = SkiaApi.sk_stream_read_u16 (Handle, b);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Read a single, unsigned 32-bit integer.</summary>
		/// <param name="buffer">The unsigned 32-bit integer that was read.</param>
		/// <returns><see langword="true" /> if the read was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool ReadUInt32 (out UInt32 buffer)
		{
			fixed (UInt32* b = &buffer) {
				var result = SkiaApi.sk_stream_read_u32 (Handle, b);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Read a single boolean.</summary>
		/// <param name="buffer">The boolean that was read.</param>
		/// <returns><see langword="true" /> if the read was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool ReadBool (out Boolean buffer)
		{
			byte b;
			var result = SkiaApi.sk_stream_read_bool (Handle, &b);
			GC.KeepAlive (this);
			buffer = b > 0;
			return result;
		}

		/// <summary>Reads a copy of the specified number of bytes into the provided buffer.</summary>
		/// <param name="buffer">The buffer to read into.</param>
		/// <param name="size">The number of bytes to read.</param>
		/// <returns>Returns the number of bytes actually read.</returns>
		/// <remarks />
		public int Read (byte[] buffer, int size)
		{
			fixed (byte* b = buffer) {
				return Read ((IntPtr)b, size);
			}
		}

		/// <summary>Reads a copy of the specified number of bytes into the provided buffer.</summary>
		/// <param name="buffer">The buffer to read into.</param>
		/// <param name="size">The number of bytes to read.</param>
		/// <returns>Returns the number of bytes actually read.</returns>
		/// <remarks />
		public int Read (IntPtr buffer, int size)
		{
			var result = (int)SkiaApi.sk_stream_read (Handle, (void*)buffer, (IntPtr)size);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Attempt to peek at <paramref name="size" /> bytes.</summary>
		/// <param name="buffer">The buffer to read into.</param>
		/// <param name="size">The number of bytes to read.</param>
		/// <returns>Returns the number of bytes actually peeked/copied.</returns>
		/// <remarks />
		public int Peek (IntPtr buffer, int size)
		{
			var result = (int)SkiaApi.sk_stream_peek (Handle, (void*)buffer, (IntPtr)size);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Moves the current position on by the specified number of bytes.</summary>
		/// <param name="size">The number of bytes to skip.</param>
		/// <returns>Returns the actual number bytes that could be skipped.</returns>
		/// <remarks />
		public int Skip (int size)
		{
			var result = (int)SkiaApi.sk_stream_skip (Handle, (IntPtr)size);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Rewinds to the beginning of the stream.</summary>
		/// <returns><see langword="true" /> if the stream is known to be at the beginning after this call returns.</returns>
		/// <remarks />
		public bool Rewind ()
		{
			var result = SkiaApi.sk_stream_rewind (Handle);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Seeks to an absolute position in the stream.</summary>
		/// <param name="position">The absolute position.</param>
		/// <returns><see langword="true" /> if seeking is supported and the seek was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks>If an attempt is made to move to a position outside the stream, the position will be set to the closest point within the stream (beginning or end).</remarks>
		public bool Seek (int position)
		{
			var result = SkiaApi.sk_stream_seek (Handle, (IntPtr)position);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Seeks to an relative offset in the stream.</summary>
		/// <param name="offset">The relative offset.</param>
		/// <returns><see langword="true" /> if seeking is supported and the seek was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks>If an attempt is made to move to a position outside the stream, the position will be set to the closest point within the stream (beginning or end).</remarks>
		[Obsolete ("The native stream move offset is capped at a 32-bit int. Use Move(int) instead.")]
		public bool Move (long offset) => Move (checked ((int)offset));

		/// <summary>Seeks to an relative offset in the stream.</summary>
		/// <param name="offset">The relative offset.</param>
		/// <returns><see langword="true" /> if seeking is supported and the seek was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks>If an attempt is made to move to a position outside the stream, the position will be set to the closest point within the stream (beginning or end).</remarks>
		public bool Move (int offset)
		{
			var result = SkiaApi.sk_stream_move (Handle, offset);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Returns the memory address of the data if the stream is a memory stream.</summary>
		/// <returns>Returns the memory address of the data, or IntPtr.Zero if the stream is not a memory stream.</returns>
		/// <remarks />
		public IntPtr GetMemoryBase ()
		{
			var result = (IntPtr)SkiaApi.sk_stream_get_memory_base (Handle);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Returns the entire contents of the stream as an <see cref="T:SkiaSharp.SKData" /> object.</summary>
		/// <returns>A new <see cref="T:SkiaSharp.SKData" /> containing the entire contents of the stream.</returns>
		/// <remarks></remarks>
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

		/// <summary>Gets a value indicating whether this stream can report its current position.</summary>
		/// <value><see langword="true" /> if the stream can report its current position; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool HasPosition {
			get {
				var result = SkiaApi.sk_stream_has_position (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Gets or sets the current position in the stream. If this is not supported, the position will be reported as 0.</summary>
		/// <value>The current position in the stream, or 0 if not supported.</value>
		/// <remarks />
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

		/// <summary>Gets a value indicating whether this stream can report its total length.</summary>
		/// <value><see langword="true" /> if the stream can report its total length; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool HasLength {
			get {
				var result = SkiaApi.sk_stream_has_length (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Gets the total length of the stream. If this is not supported, the length will be reported as 0.</summary>
		/// <value>The total length of the stream, or 0 if not supported.</value>
		/// <remarks />
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

	/// <summary>An abstract, rewindable stream.</summary>
	/// <remarks />
	public abstract class SKStreamRewindable : SKStream
	{
		internal SKStreamRewindable (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
	}

	/// <summary>An abstract, rewindable stream that supports the seek operation.</summary>
	/// <remarks />
	public abstract class SKStreamSeekable : SKStreamRewindable
	{
		internal SKStreamSeekable (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
	}

	/// <summary>An abstract, seekable stream with a known length.</summary>
	/// <remarks />
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

	/// <summary>An abstract, memory-based stream.</summary>
	/// <remarks />
	public abstract class SKStreamMemory : SKStreamAsset
	{
		internal SKStreamMemory (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
	}

	/// <summary>A seekable stream backed by a file on the file system.</summary>
	/// <remarks />
	public unsafe class SKFileStream : SKStreamAsset
	{
		internal SKFileStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKFileStream" /> that wraps the file with the specified path.</summary>
		/// <param name="path">The existing file to open for reading.</param>
		/// <remarks />
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

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKFileStream" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKFileStream" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
		/// <remarks />
		protected override void DisposeNative () =>
			SkiaApi.sk_filestream_destroy (Handle);

		/// <summary>Gets a value indicating whether the file could be opened.</summary>
		/// <value><see langword="true" /> if the file was opened successfully; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IsValid {
			get {
				var result = SkiaApi.sk_filestream_is_valid (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Determines whether the specified path is supported by a <see cref="T:SkiaSharp.SKFileStream" />.</summary>
		/// <param name="path">The path to check.</param>
		/// <returns>Returns <see langword="true" /> if the path is supported, otherwise <see langword="false" />.</returns>
		/// <remarks />
		public static bool IsPathSupported (string path) => true;

		/// <summary>Opens a read-only stream to the specified file.</summary>
		/// <param name="path">The path to the file to open.</param>
		/// <returns>Returns a stream that contains the file contents.</returns>
		/// <remarks />
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

	/// <summary>A memory-based stream.</summary>
	/// <remarks />
	public unsafe class SKMemoryStream : SKStreamMemory
	{
		internal SKMemoryStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKMemoryStream" /> with an empty buffer.</summary>
		/// <remarks />
		public SKMemoryStream ()
			: this (SkiaApi.sk_memorystream_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKMemoryStream instance.");
			}
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKMemoryStream" /> with a buffer size of the specified size.</summary>
		/// <param name="length">The size of the stream buffer.</param>
		/// <remarks />
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

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKMemoryStream" /> with the buffer being the provided data.</summary>
		/// <param name="data">The data to initialize the stream with.</param>
		/// <remarks />
		public SKMemoryStream (SKData data)
			: this(SkiaApi.sk_memorystream_new_with_skdata (data.Handle), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKMemoryStream instance.");
			}
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKMemoryStream" /> with a copy of the provided data.</summary>
		/// <param name="data">The data to initialize the stream with.</param>
		/// <remarks />
		public SKMemoryStream (byte[] data)
			: this ()
		{
			SetMemory (data);
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKMemoryStream" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKMemoryStream" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
		/// <remarks />
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

		/// <summary>Resets the stream with a copy of the provided data.</summary>
		/// <param name="data">The data to reset the stream to.</param>
		/// <remarks />
		public void SetMemory (byte[] data)
		{
			SetMemory (data, (IntPtr)data.Length, true);
		}
	}

	/// <summary>An abstraction for writing a stream of bytes, which can be backed by memory, or a file, or something else.</summary>
	/// <remarks />
	public unsafe abstract class SKWStream : SKObject
	{
		internal SKWStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
		
		/// <summary>Gets the number of bytes written so far.</summary>
		/// <value>The number of bytes written.</value>
		/// <remarks />
		public virtual int BytesWritten {
			get {
				var result = (int)SkiaApi.sk_wstream_bytes_written (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Write the provided data to the stream.</summary>
		/// <param name="buffer">The data buffer to write.</param>
		/// <param name="size">The number of bytes from the buffer to write.</param>
		/// <returns><see langword="true" /> if the write succeeded; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public virtual bool Write (byte[] buffer, int size)
		{
			fixed (byte* b = buffer) {
				var result = SkiaApi.sk_wstream_write (Handle, (void*)b, (IntPtr)size);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Write a newline character to the stream, if one was not already written.</summary>
		/// <returns><see langword="true" /> if the write succeeded; otherwise, <see langword="false" />.</returns>
		/// <remarks>If the last character was a newline character, this method does nothing.</remarks>
		public bool NewLine ()
		{
			var result = SkiaApi.sk_wstream_newline (Handle);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Flush the buffer to the underlying destination.</summary>
		/// <remarks />
		public virtual void Flush ()
		{
			SkiaApi.sk_wstream_flush (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Write a single byte to the stream.</summary>
		/// <param name="value">The byte to write.</param>
		/// <returns><see langword="true" /> if the write succeeded; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool Write8 (Byte value)
		{
			var result = SkiaApi.sk_wstream_write_8 (Handle, value);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Write a single, unsigned 16-bit integer to the stream.</summary>
		/// <param name="value">The unsigned 16-bit integer to write.</param>
		/// <returns><see langword="true" /> if the write succeeded; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool Write16 (UInt16 value)
		{
			var result = SkiaApi.sk_wstream_write_16 (Handle, value);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Write a single, unsigned 32-bit integer to the stream.</summary>
		/// <param name="value">The unsigned 32-bit integer to write.</param>
		/// <returns><see langword="true" /> if the write succeeded; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool Write32 (UInt32 value)
		{
			var result = SkiaApi.sk_wstream_write_32 (Handle, value);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Write a string to the stream as a string.</summary>
		/// <param name="value">The string to write.</param>
		/// <returns><see langword="true" /> if the write succeeded; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool WriteText (string value)
		{
			var result = SkiaApi.sk_wstream_write_text (Handle, value);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Write a 32-bit integer to the stream as a string.</summary>
		/// <param name="value">The 32-bit integer to write.</param>
		/// <returns><see langword="true" /> if the write succeeded; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool WriteDecimalAsTest (Int32 value)
		{
			var result = SkiaApi.sk_wstream_write_dec_as_text (Handle, value);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Write a single 64-bit integer to the stream as a string.</summary>
		/// <param name="value">The 64-bit integer to write.</param>
		/// <param name="digits">The number of digits (length) to use when writing.</param>
		/// <returns><see langword="true" /> if the write succeeded; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool WriteBigDecimalAsText (Int64 value, int digits)
		{
			var result = SkiaApi.sk_wstream_write_bigdec_as_text (Handle, value, digits);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Write an unsigned, 32-bit integer to the stream as a hexadecimal string.</summary>
		/// <param name="value">The unsigned, 32-bit integer to write.</param>
		/// <param name="digits">The number of digits (length) to use when writing.</param>
		/// <returns><see langword="true" /> if the write succeeded; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool WriteHexAsText (UInt32 value, int digits)
		{
			var result = SkiaApi.sk_wstream_write_hex_as_text (Handle, value, digits);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Write a single, floating-point number to the stream as text.</summary>
		/// <param name="value">The floating-point number to write.</param>
		/// <returns><see langword="true" /> if the write succeeded; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool WriteScalarAsText (float value)
		{
			var result = SkiaApi.sk_wstream_write_scalar_as_text (Handle, value);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Write a single boolean to the stream.</summary>
		/// <param name="value">The boolean to write.</param>
		/// <returns><see langword="true" /> if the write succeeded; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool WriteBool (bool value)
		{
			var result = SkiaApi.sk_wstream_write_bool (Handle, value);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Write a single, floating-point number to the stream.</summary>
		/// <param name="value">The floating-point number to write.</param>
		/// <returns><see langword="true" /> if the write succeeded; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool WriteScalar (float value)
		{
			var result = SkiaApi.sk_wstream_write_scalar (Handle, value);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Write a single, unsigned 32-bit integer to the stream in the smallest space possible.</summary>
		/// <param name="value">The unsigned 32-bit integer to write.</param>
		/// <returns><see langword="true" /> if the write succeeded; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool WritePackedUInt32 (UInt32 value)
		{
			var result = SkiaApi.sk_wstream_write_packed_uint (Handle, (IntPtr)value);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Write the contents of the specified stream to this stream.</summary>
		/// <param name="input">The stream to write.</param>
		/// <param name="length">The number of bytes to write.</param>
		/// <returns><see langword="true" /> if the write succeeded; otherwise, <see langword="false" />.</returns>
		/// <remarks />
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

		/// <summary>Returns the number of bytes in the stream required to store the specified value.</summary>
		/// <param name="value">The value to store.</param>
		/// <returns>Returns the number of bytes required.</returns>
		/// <remarks />
		public static int GetSizeOfPackedUInt32 (UInt32 value)
		{
			return SkiaApi.sk_wstream_get_size_of_packed_uint ((IntPtr) value);
		}
	}

	/// <summary>A writeable stream backed by a file on the file system.</summary>
	/// <remarks />
	public unsafe class SKFileWStream : SKWStream
	{
		internal SKFileWStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKFileWStream" /> that wraps the file with the specified path.</summary>
		/// <param name="path">The new or existing file to open for writing.</param>
		/// <remarks />
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

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKFileWStream" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKFileWStream" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
		/// <remarks />
		protected override void DisposeNative () =>
			SkiaApi.sk_filewstream_destroy (Handle);

		/// <summary>Gets a value indicating whether the file could be opened.</summary>
		/// <value><see langword="true" /> if the file was opened successfully; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IsValid {
			get {
				var result = SkiaApi.sk_filewstream_is_valid (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Determines whether the specified path is supported by a <see cref="T:SkiaSharp.SKFileWStream" />.</summary>
		/// <param name="path">The path to check.</param>
		/// <returns>Returns <see langword="true" /> if the path is supported, otherwise <see langword="false" />.</returns>
		/// <remarks />
		public static bool IsPathSupported (string path) => true;

		/// <summary>Opens a write-only stream to the specified file.</summary>
		/// <param name="path">The path to the file to open.</param>
		/// <returns>Returns a stream that contains the file contents.</returns>
		/// <remarks />
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

	/// <summary>A writeable, dynamically-sized, memory-based stream.</summary>
	/// <remarks />
	public unsafe class SKDynamicMemoryWStream : SKWStream
	{
		internal SKDynamicMemoryWStream (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Create a new instance of <see cref="T:SkiaSharp.SKDynamicMemoryWStream" /> with an empty buffer.</summary>
		/// <remarks />
		public SKDynamicMemoryWStream ()
			: base (SkiaApi.sk_dynamicmemorywstream_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKDynamicMemoryWStream instance.");
			}
		}

		/// <summary>Returns a copy of the data written so far.</summary>
		/// <returns>A copy of the data.</returns>
		/// <remarks>The caller is responsible for releasing the memory.</remarks>
		public SKData CopyToData ()
		{
			var data = SKData.Create (BytesWritten);
			CopyTo (data.Data);
			return data;
		}

		/// <summary>Returns a read-only stream with the current data, and then resets the current stream.</summary>
		/// <returns>The stream with the data.</returns>
		/// <remarks>After calling this method, this stream is reset to its empty state.</remarks>
		public SKStreamAsset DetachAsStream ()
		{
			var result = SKStreamAssetImplementation.GetObject (SkiaApi.sk_dynamicmemorywstream_detach_as_stream (Handle));
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Returns a <see cref="T:SkiaSharp.SKData" /> instance of the data in the current stream, and then resets the current stream.</summary>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKData" /> instance.</returns>
		/// <remarks>After calling this method, this stream is reset to its empty state.</remarks>
		public SKData DetachAsData ()
		{
			var result = SKData.GetObject (SkiaApi.sk_dynamicmemorywstream_detach_as_data (Handle));
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Copies the data from the current stream to a memory location.</summary>
		/// <param name="data">The memory location to copy the data to.</param>
		/// <remarks />
		public void CopyTo (IntPtr data)
		{
			SkiaApi.sk_dynamicmemorywstream_copy_to (Handle, (void*)data);
			GC.KeepAlive (this);
		}

		/// <summary>Copies the data from the current stream to a byte span.</summary>
		/// <param name="data">The byte span to copy the data to.</param>
		/// <remarks>The span must be at least as large as the number of bytes written to the stream.</remarks>
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

		/// <summary>Copies the data from the current stream to the specified stream.</summary>
		/// <param name="dst">The stream to copy the data to.</param>
		/// <returns><see langword="true" /> if the copy was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool CopyTo (SKWStream dst)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			var result = SkiaApi.sk_dynamicmemorywstream_write_to_stream (Handle, dst.Handle);
			GC.KeepAlive (dst);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Copies the data from the current stream to the specified .NET stream.</summary>
		/// <param name="dst">The stream to copy the data to.</param>
		/// <returns><see langword="true" /> if the copy was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool CopyTo (Stream dst)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			using var wrapped = new SKManagedWStream (dst);
			return CopyTo (wrapped);
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKDynamicMemoryWStream" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKDynamicMemoryWStream" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
		/// <remarks />
		protected override void DisposeNative () =>
			SkiaApi.sk_dynamicmemorywstream_destroy (Handle);
	}
}
