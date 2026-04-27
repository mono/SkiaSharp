using System;
using System.IO;

namespace SkiaSharp;

public static unsafe class SKWebpEncoder
{
	public static bool EncodeAnimated (SKWStream dst, ReadOnlySpan<SKWebpEncoderFrame> frames, SKWebpEncoderOptions options)
	{
		_ = dst ?? throw new ArgumentNullException (nameof (dst));

		fixed (SKWebpEncoderFrame* f = frames) {
			return SkiaApi.sk_webpencoder_encode_animated (dst.Handle, f, frames.Length, &options);
		}
	}

	public static bool EncodeAnimated (Stream dst, ReadOnlySpan<SKWebpEncoderFrame> frames, SKWebpEncoderOptions options)
	{
		_ = dst ?? throw new ArgumentNullException (nameof (dst));

		using var wrapped = new SKManagedWStream (dst);
		return EncodeAnimated (wrapped, frames, options);
	}

	public static SKData? EncodeAnimated (ReadOnlySpan<SKWebpEncoderFrame> frames, SKWebpEncoderOptions options)
	{
		using var stream = new SKDynamicMemoryWStream ();
		var result = EncodeAnimated (stream, frames, options);
		return result ? stream.DetachAsData () : null;
	}
}
