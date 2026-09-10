using System;
using System.IO;

namespace SkiaSharp;

/// <summary>Provides methods for encoding images and animated sequences to the WebP format.</summary>
/// <remarks><![CDATA[
/// ## Remarks
///
/// `SKWebpEncoder` provides static methods for encoding a single <xref:SkiaSharp.SKPixmap> to WebP data, as well as encoding multi-frame animated WebP sequences from a span of <xref:SkiaSharp.SKWebpEncoderFrame> values.
///
/// Encoding options such as quality and compression method are specified via <xref:SkiaSharp.SKWebpEncoderOptions>.
///
/// ## Examples
///
/// Encoding a bitmap to a WebP file:
///
/// ```csharp
/// using var bitmap = new SKBitmap(200, 200);
/// using var canvas = new SKCanvas(bitmap);
/// canvas.DrawColor(SKColors.CornflowerBlue);
///
/// using var pixmap = bitmap.PeekPixels();
/// using var data = SKWebpEncoder.Encode(pixmap, SKWebpEncoderOptions.Default);
/// File.WriteAllBytes("output.webp", data.ToArray());
/// ```
/// ]]></remarks>
public static unsafe class SKWebpEncoder
{
	// single-frame encoding

	/// <summary>Encodes the specified pixel data to WebP format and writes the result to the specified stream.</summary>
	/// <param name="dst">The stream to which the encoded WebP data is written.</param>
	/// <param name="src">The pixel data to encode.</param>
	/// <param name="options">The WebP encoder options, such as quality and compression method.</param>
	/// <returns><see langword="true" /> if encoding succeeded; otherwise, <see langword="false" />.</returns>
	/// <remarks />
	public static bool Encode (SKWStream dst, SKPixmap src, SKWebpEncoderOptions options)
	{
		_ = dst ?? throw new ArgumentNullException (nameof (dst));
		_ = src ?? throw new ArgumentNullException (nameof (src));

		var result = SkiaApi.sk_webpencoder_encode (dst.Handle, src.Handle, &options);
		GC.KeepAlive (dst);
		GC.KeepAlive (src);
		return result;
	}

	/// <summary>Encodes the specified pixel data to WebP format and writes the result to the specified managed stream.</summary>
	/// <param name="dst">The managed stream to which the encoded WebP data is written.</param>
	/// <param name="src">The pixel data to encode.</param>
	/// <param name="options">The WebP encoder options, such as quality and compression method.</param>
	/// <returns><see langword="true" /> if encoding succeeded; otherwise, <see langword="false" />.</returns>
	/// <remarks />
	public static bool Encode (Stream dst, SKPixmap src, SKWebpEncoderOptions options)
	{
		_ = dst ?? throw new ArgumentNullException (nameof (dst));
		_ = src ?? throw new ArgumentNullException (nameof (src));

		using var wrapped = new SKManagedWStream (dst);
		return Encode (wrapped, src, options);
	}

	/// <summary>Encodes the specified pixel data to the WebP format and returns the result as <see cref="T:SkiaSharp.SKData" />.</summary>
	/// <param name="src">The pixel data to encode.</param>
	/// <param name="options">The WebP encoder options, such as quality and compression method.</param>
	/// <returns>A new <see cref="T:SkiaSharp.SKData" /> containing the encoded WebP data, or <see langword="null" /> if encoding failed.</returns>
	/// <remarks />
	public static SKData? Encode (SKPixmap src, SKWebpEncoderOptions options)
	{
		_ = src ?? throw new ArgumentNullException (nameof (src));

		using var stream = new SKDynamicMemoryWStream ();
		var result = Encode (stream, src, options);
		return result ? stream.DetachAsData () : null;
	}

	// animated encoding

	/// <summary>Encodes the specified frames as an animated WebP and writes the result to the specified stream.</summary>
	/// <param name="dst">The stream to which the encoded animated WebP data is written.</param>
	/// <param name="frames">A read-only span of <see cref="T:SkiaSharp.SKWebpEncoderFrame" /> values describing each frame of the animation.</param>
	/// <param name="options">The WebP encoder options, such as quality and compression method.</param>
	/// <returns><see langword="true" /> if encoding succeeded; otherwise, <see langword="false" />.</returns>
	/// <remarks />
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
			var result = SkiaApi.sk_webpencoder_encode_animated (dst.Handle, f, frames.Length, &options);
			GC.KeepAlive (dst);
			return result;
		}
	}

	/// <summary>Encodes the specified frames as an animated WebP and writes the result to the specified managed stream.</summary>
	/// <param name="dst">The managed stream to which the encoded animated WebP data is written.</param>
	/// <param name="frames">A read-only span of <see cref="T:SkiaSharp.SKWebpEncoderFrame" /> values describing each frame of the animation.</param>
	/// <param name="options">The WebP encoder options, such as quality and compression method.</param>
	/// <returns><see langword="true" /> if encoding succeeded; otherwise, <see langword="false" />.</returns>
	/// <remarks />
	public static bool EncodeAnimated (Stream dst, ReadOnlySpan<SKWebpEncoderFrame> frames, SKWebpEncoderOptions options)
	{
		_ = dst ?? throw new ArgumentNullException (nameof (dst));

		using var wrapped = new SKManagedWStream (dst);
		return EncodeAnimated (wrapped, frames, options);
	}

	/// <summary>Encodes the specified frames as an animated WebP and returns the result as <see cref="T:SkiaSharp.SKData" />.</summary>
	/// <param name="frames">A read-only span of <see cref="T:SkiaSharp.SKWebpEncoderFrame" /> values describing each frame of the animation.</param>
	/// <param name="options">The WebP encoder options, such as quality and compression method.</param>
	/// <returns>A new <see cref="T:SkiaSharp.SKData" /> containing the encoded animated WebP data, or <see langword="null" /> if encoding failed.</returns>
	/// <remarks />
	public static SKData? EncodeAnimated (ReadOnlySpan<SKWebpEncoderFrame> frames, SKWebpEncoderOptions options)
	{
		using var stream = new SKDynamicMemoryWStream ();
		var result = EncodeAnimated (stream, frames, options);
		return result ? stream.DetachAsData () : null;
	}
}
