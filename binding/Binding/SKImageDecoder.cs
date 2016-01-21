//
// Bindings for SKImageDecoder
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2015 Xamarin Inc
//

namespace SkiaSharp
{
	public class SKImageDecoder
	{
		public static bool DecodeBounds (SKStreamRewindable stream, out SKImageInfo info, ref SKImageDecoderFormat format)
		{
			return DecodeBounds (stream, SKColorType.Unknown, out info, ref format);
		}

		public static bool DecodeBounds (SKStreamRewindable stream, SKColorType pref, out SKImageInfo info, ref SKImageDecoderFormat format)
		{
			var bitmap = new SKBitmap ();
			var result = SkiaApi.sk_imagedecoder_decode_stream (stream.handle, bitmap.handle, pref, SKImageDecoderMode.DecodeBounds, ref format);
			info = bitmap.Info;
			bitmap.Dispose ();
			return result;
		}

		public static bool Decode (SKStreamRewindable stream, SKBitmap bitmap, SKColorType pref, SKImageDecoderMode mode, ref SKImageDecoderFormat format)
		{
			return SkiaApi.sk_imagedecoder_decode_stream (stream.handle, bitmap.handle, pref, mode, ref format);
		}

		public static bool Decode (SKStreamRewindable stream, SKBitmap bitmap, SKColorType pref, SKImageDecoderMode mode)
		{
			SKImageDecoderFormat format = SKImageDecoderFormat.Unknown;
			return Decode (stream, bitmap, pref, mode, ref format);
		}

		public static bool Decode (SKStreamRewindable stream, SKBitmap bitmap)
		{
			return Decode (stream, bitmap, SKColorType.Unknown, SKImageDecoderMode.DecodePixels);
		}
	}
}
