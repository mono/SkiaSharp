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
		internal SKStream (IntPtr handle)
			: base (handle)
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
		internal SKStreamRewindable (IntPtr handle)
			: base (handle)
		{
		}
	}

	public abstract class SKStreamSeekable : SKStreamRewindable
	{
		internal SKStreamSeekable (IntPtr handle)
			: base (handle)
		{
		}
	}

	public abstract class SKStreamAsset : SKStreamSeekable
	{
		internal SKStreamAsset (IntPtr handle)
			: base (handle)
		{
		}
	}

	public abstract class SKStreamMemory : SKStreamAsset
	{
		internal SKStreamMemory (IntPtr handle)
			: base (handle)
		{
		}
	}

	public class SKFileStream : SKStreamAsset
	{
		internal static bool linkskip = true;
		static SKFileStream ()
		{
			if (!linkskip) {
				new SKFileStream (IntPtr.Zero);
			}
		}

		internal SKFileStream (IntPtr handle)
			: base (handle)
		{
		}

		public SKFileStream (string path)
			: base (SkiaApi.sk_filestream_new (path))
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKFileStream instance.");
			}
		}
	}

	public class SKMemoryStream : SKStreamMemory
	{
		[Preserve]
		internal SKMemoryStream(IntPtr handle)
			: base (handle)
		{
		}

		public SKMemoryStream ()
			: this (SkiaApi.sk_memorystream_new ())
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKMemoryStream instance.");
			}
		}

		public SKMemoryStream (ulong length)
			: this(SkiaApi.sk_memorystream_new_with_length ((IntPtr)length))
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKMemoryStream instance.");
			}
		}

		internal SKMemoryStream (IntPtr data, IntPtr length, bool copyData = false)
			: this(SkiaApi.sk_memorystream_new_with_data (data, length, copyData))
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKMemoryStream instance.");
			}
		}

		public SKMemoryStream (SKData data)
			: this(SkiaApi.sk_memorystream_new_with_skdata (data))
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
}
