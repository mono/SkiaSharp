//
// Bindings for SKImageDecoder
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2015 Xamarin Inc
//

using System;

namespace SkiaSharp
{
	public class SKImageDecoder : IDisposable
	{
		internal IntPtr handle;

		public SKImageDecoder(SKStreamRewindable stream)
			: this(SkiaApi.sk_imagedecoder_factory(stream.handle))
		{
		}

		internal SKImageDecoder(IntPtr handle)
		{
			this.handle = handle;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (handle != IntPtr.Zero)
			{
				SkiaApi.sk_imagedecoder_destructor(handle);
				handle = IntPtr.Zero;
			}
		}

		~SKImageDecoder()
		{
			Dispose(false);
		}

		public SKImageDecoderFormat Format
		{
			get { return SkiaApi.sk_imagedecoder_get_decoder_format(handle); }
		}

		public string FormatName
		{
			get { return SkiaApi.sk_imagedecoder_get_format_name_from_decoder(handle); }
		}

		public bool SkipWritingZeros
		{
			get { return SkiaApi.sk_imagedecoder_get_skip_writing_zeros(handle); }
			set { SkiaApi.sk_imagedecoder_set_skip_writing_zeros(handle, value); }
		}

		public bool DitherImage
		{
			get { return SkiaApi.sk_imagedecoder_get_dither_image(handle); }
			set { SkiaApi.sk_imagedecoder_set_dither_image(handle, value); }
		}

		public bool PreferQualityOverSpeed
		{
			get { return SkiaApi.sk_imagedecoder_get_prefer_quality_over_speed(handle); }
			set { SkiaApi.sk_imagedecoder_set_prefer_quality_over_speed(handle, value); }
		}

		public bool RequireUnpremultipliedColors
		{
			get { return SkiaApi.sk_imagedecoder_get_require_unpremultiplied_colors(handle); }
			set { SkiaApi.sk_imagedecoder_set_require_unpremultiplied_colors(handle, value); }
		}

		public int SampleSize
		{
			get { return SkiaApi.sk_imagedecoder_get_sample_size(handle); }
			set { SkiaApi.sk_imagedecoder_set_sample_size(handle, value); }
		}

		public bool ShouldCancelDecode
		{
			get { return SkiaApi.sk_imagedecoder_should_cancel_decode(handle); }
		}

		public void CancelDecode()
		{
			SkiaApi.sk_imagedecoder_cancel_decode(handle);
		}

		public SKImageDecoderResult Decode(SKStream stream, SKBitmap bitmap, SKColorType pref = SKColorType.Unknown, SKImageDecoderMode mode = SKImageDecoderMode.DecodePixels)
		{
			return SkiaApi.sk_imagedecoder_decode(handle, stream.handle, bitmap.handle, pref, mode);
		}

		public static SKImageDecoderFormat GetFormat(SKStreamRewindable stream)
		{
			return SkiaApi.sk_imagedecoder_get_stream_format(stream.handle);
		}

		public static string GetFormatName(SKImageDecoderFormat format)
		{
			return SkiaApi.sk_imagedecoder_get_format_name_from_format(format);
		}

		public static bool DecodeStreamBounds(SKStreamRewindable stream, out SKImageInfo info, SKColorType pref = SKColorType.Unknown)
		{
			SKImageDecoderFormat format = SKImageDecoderFormat.Unknown;
			return DecodeStreamBounds(stream, out info, pref, ref format);
		}

		public static bool DecodeStreamBounds(SKStreamRewindable stream, out SKImageInfo info, SKColorType pref, ref SKImageDecoderFormat format)
		{
			using (var bitmap = new SKBitmap())
			{
				if (DecodeStream(stream, bitmap, pref, SKImageDecoderMode.DecodePixels, ref format))
				{
					info = bitmap.Info;
					return true;
				}
				info = SKImageInfo.Empty;
				return false;
			}
		}

		public static bool DecodeFileBounds(string filename, out SKImageInfo info, SKColorType pref = SKColorType.Unknown)
		{
			SKImageDecoderFormat format = SKImageDecoderFormat.Unknown;
			return DecodeFileBounds(filename, out info, pref, ref format);
		}

		public static bool DecodeFileBounds(string filename, out SKImageInfo info, SKColorType pref, ref SKImageDecoderFormat format)
		{
			using (var bitmap = new SKBitmap())
			{
				if (DecodeFile(filename, bitmap, pref, SKImageDecoderMode.DecodePixels, ref format))
				{
					info = bitmap.Info;
					return true;
				}
				info = SKImageInfo.Empty;
				return false;
			}
		}

		public static bool DecodeMemoryBounds(byte[] buffer, out SKImageInfo info, SKColorType pref = SKColorType.Unknown)
		{
			SKImageDecoderFormat format = SKImageDecoderFormat.Unknown;
			return DecodeMemoryBounds(buffer, out info, pref, ref format);
		}

		public static bool DecodeMemoryBounds(byte[] buffer, out SKImageInfo info, SKColorType pref, ref SKImageDecoderFormat format)
		{
			using (var bitmap = new SKBitmap())
			{
				if (DecodeMemory(buffer, bitmap, pref, SKImageDecoderMode.DecodePixels, ref format))
				{
					info = bitmap.Info;
					return true;
				}
				info = SKImageInfo.Empty;
				return false;
			}
		}

		public static bool DecodeStream(SKStreamRewindable stream, SKBitmap bitmap, SKColorType pref = SKColorType.Unknown, SKImageDecoderMode mode = SKImageDecoderMode.DecodePixels)
		{
			SKImageDecoderFormat format = SKImageDecoderFormat.Unknown;
			return DecodeStream(stream, bitmap, pref, mode, ref format);
		}

		public static bool DecodeStream(SKStreamRewindable stream, SKBitmap bitmap, SKColorType pref, SKImageDecoderMode mode, ref SKImageDecoderFormat format)
		{
			return SkiaApi.sk_imagedecoder_decode_stream(stream.handle, bitmap.handle, pref, mode, ref format);
		}

		public static bool DecodeFile(string filename, SKBitmap bitmap, SKColorType pref = SKColorType.Unknown, SKImageDecoderMode mode = SKImageDecoderMode.DecodePixels)
		{
			SKImageDecoderFormat format = SKImageDecoderFormat.Unknown;
			return DecodeFile(filename, bitmap, pref, mode, ref format);
		}

		public static bool DecodeFile(string filename, SKBitmap bitmap, SKColorType pref, SKImageDecoderMode mode, ref SKImageDecoderFormat format)
		{
			return SkiaApi.sk_imagedecoder_decode_file(filename, bitmap.handle, pref, mode, ref format);
		}

		public static bool DecodeMemory(byte[] buffer, SKBitmap bitmap, SKColorType pref = SKColorType.Unknown, SKImageDecoderMode mode = SKImageDecoderMode.DecodePixels)
		{
			SKImageDecoderFormat format = SKImageDecoderFormat.Unknown;
			return DecodeMemory(buffer, bitmap, pref, mode, ref format);
		}

		public static bool DecodeMemory(byte[] buffer, SKBitmap bitmap, SKColorType pref, SKImageDecoderMode mode, ref SKImageDecoderFormat format)
		{
			return SkiaApi.sk_imagedecoder_decode_memory(buffer, (IntPtr)buffer.Length, bitmap.handle, pref, mode, ref format);
		}
	}
}
