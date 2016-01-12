//
// Bindings for SKStream
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2015 Xamarin Inc
//
using System;

namespace SkiaSharp
{
	public abstract class SKStream : IDisposable
	{
		internal IntPtr handle;
		internal bool owns;

		internal SKStream (IntPtr handle, bool owns)
		{
			this.owns = owns;
			this.handle = handle;
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero) {
				if (owns) {
				}
				handle = IntPtr.Zero;
			}
		}

		~SKStream ()
		{
			Dispose (false);
		}

		public bool IsAtEnd {
			get {
				return SkiaApi.sk_stream_is_at_end (handle);
			}
		}

		public SByte ReadSByte ()
		{
			return SkiaApi.sk_stream_read_s8 (handle);
		}

		public Int16 ReadInt16 ()
		{
			return SkiaApi.sk_stream_read_s16 (handle);
		}

		public Int32 ReadInt32 ()
		{
			return SkiaApi.sk_stream_read_s32 (handle);
		}

		public Byte ReadByte ()
		{
			return SkiaApi.sk_stream_read_u8 (handle);
		}

		public UInt16 ReadUInt16 ()
		{
			return SkiaApi.sk_stream_read_u16 (handle);
		}

		public UInt32 ReadUInt32 ()
		{
			return SkiaApi.sk_stream_read_u32 (handle);
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

	public abstract class SKStreamMemory : SKStreamAsset
	{
		internal SKStreamMemory (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
	}

	public class SKFileStream : SKStreamAsset
	{
		public SKFileStream (string path)
			: base (SkiaApi.sk_filestream_new (path), true)
		{
		}
	}

	public class SKMemoryStream : SKStreamMemory
	{
		public SKMemoryStream ()
			: base (SkiaApi.sk_memorystream_new (), true)
		{
		}

		public SKMemoryStream (ulong length)
			: base (SkiaApi.sk_memorystream_new_with_length ((IntPtr)length), true)
		{
		}

		internal SKMemoryStream (IntPtr data, IntPtr length, bool copyData = false)
			: base (SkiaApi.sk_memorystream_new_with_data (data, length, copyData), true)
		{
		}

		public SKMemoryStream (SKData data)
			: base (SkiaApi.sk_memorystream_new_with_skdata (data), true)
		{
		}

		public SKMemoryStream (byte[] data)
			: this ()
		{
			SetMemory (data);
		}

		internal void SetMemory (IntPtr data, IntPtr length, bool copyData = false)
		{
			SkiaApi.sk_memorystream_set_memory (handle, data, length, copyData);
		}

		internal void SetMemory (byte[] data, IntPtr length, bool copyData = false)
		{
			SkiaApi.sk_memorystream_set_memory (handle, data, length, copyData);
		}

		public void SetMemory (byte[] data)
		{
			SetMemory (data, (IntPtr)data.Length, true);
		}
	}
}
