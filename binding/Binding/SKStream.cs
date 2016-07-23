//
// Bindings for SKStream
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2016 Xamarin Inc
//
using System;

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
			return SkiaApi.sk_stream_read_s8 (Handle);
		}

		public Int16 ReadInt16 ()
		{
			return SkiaApi.sk_stream_read_s16 (Handle);
		}

		public Int32 ReadInt32 ()
		{
			return SkiaApi.sk_stream_read_s32 (Handle);
		}

		public Byte ReadByte ()
		{
			return SkiaApi.sk_stream_read_u8 (Handle);
		}

		public UInt16 ReadUInt16 ()
		{
			return SkiaApi.sk_stream_read_u16 (Handle);
		}

		public UInt32 ReadUInt32 ()
		{
			return SkiaApi.sk_stream_read_u32 (Handle);
		}

		public int Read (byte[] buffer, int size)
		{
			unsafe {
				fixed (byte *b = &buffer [0]) {
					return (int)SkiaApi.sk_stream_read (Handle, (IntPtr) b, (IntPtr)size);
				}
			}
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
			: base (SkiaApi.sk_filestream_new (path), true)
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
			: this(SkiaApi.sk_memorystream_new_with_skdata (data), true)
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
		
		public int BytesWritten {
			get {
				return (int)SkiaApi.sk_wstream_bytes_written (Handle);
			}
		}

		public bool Write (byte[] buffer, int size)
		{
			unsafe {
				fixed (byte *b = &buffer [0]) {
					return SkiaApi.sk_wstream_write (Handle, (IntPtr) b, (IntPtr)size);
				}
			}
		}

		public void NewLine ()
		{
			SkiaApi.sk_wstream_newline (Handle);
		}

		public void Flush ()
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
			: base (SkiaApi.sk_filewstream_new (path), true)
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
			return GetObject<SKData> (SkiaApi.sk_dynamicmemorywstream_copy_to_data (Handle));
		}

		public SKStreamAsset DetachAsStream ()
		{
			return GetObject<SKStreamAssetImplementation> (SkiaApi.sk_dynamicmemorywstream_detach_as_stream (Handle));
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
