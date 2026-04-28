using System;
using System.IO;

namespace SkiaSharp;

public static unsafe class SKWebpEncoder
{
	// single-frame encoding

	public static bool Encode (SKWStream dst, SKPixmap src, SKWebpEncoderOptions options)
	{
		_ = dst ?? throw new ArgumentNullException (nameof (dst));
		_ = src ?? throw new ArgumentNullException (nameof (src));

		return SkiaApi.sk_webpencoder_encode (dst.Handle, src.Handle, &options);
	}

	public static bool Encode (Stream dst, SKPixmap src, SKWebpEncoderOptions options)
	{
		_ = dst ?? throw new ArgumentNullException (nameof (dst));
		_ = src ?? throw new ArgumentNullException (nameof (src));

		using var wrapped = new SKManagedWStream (dst);
		return Encode (wrapped, src, options);
	}

	public static SKData? Encode (SKPixmap src, SKWebpEncoderOptions options)
	{
		_ = src ?? throw new ArgumentNullException (nameof (src));

		using var stream = new SKDynamicMemoryWStream ();
		var result = Encode (stream, src, options);
		return result ? stream.DetachAsData () : null;
	}

	// animated encoding

	public static bool EncodeAnimated (SKWStream dst, ReadOnlySpan<SKWebpEncoderFrame> frames, SKWebpEncoderOptions options)
	{
		_ = dst ?? throw new ArgumentNullException (nameof (dst));

		using var nativeFrames = Utils.RentArray<SKWebpEncoderFrameNative> (frames.Length);
		for (var i = 0; i < frames.Length; i++) {
			var pixmap = frames[i].Pixmap ?? throw new ArgumentNullException ($"frames[{i}].Pixmap");
			nativeFrames.Span[i] = new SKWebpEncoderFrameNative {
				pixmap = pixmap.Handle,
				duration = (int)frames[i].Duration.TotalMilliseconds,
			};
		}

		fixed (SKWebpEncoderFrameNative* f = nativeFrames.Span) {
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
