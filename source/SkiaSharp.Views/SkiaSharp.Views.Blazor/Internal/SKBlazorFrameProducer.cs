using System;
using System.Runtime.InteropServices;

namespace SkiaSharp.Views.Blazor.Internal
{
	/// <summary>
	/// Turns a rendered <see cref="SKImage"/> into the byte payload that is transferred to the
	/// browser for a bridged view. Pure and side-effect free so it can be unit tested without a
	/// browser or a Blazor host.
	/// </summary>
	/// <remarks>
	/// Bridged views render into an RGBA surface, so the <see cref="SKBlazorTransferFormat.Put"/>
	/// path produces bytes that are directly usable by both <c>ImageData</c>/<c>putImageData</c>
	/// and WebGL <c>texImage2D</c> without any channel swapping.
	/// </remarks>
	internal static class SKBlazorFrameProducer
	{
		public const string PngContentType = "image/png";
		public const string JpegContentType = "image/jpeg";
		public const string RawContentType = "application/octet-stream";

		/// <summary>The color type bridged views render into (directly browser-compatible).</summary>
		public static readonly SKColorType RgbaColorType = SKColorType.Rgba8888;

		/// <summary>Returns the JavaScript-facing content type for a transfer format.</summary>
		public static string GetContentType(SKBlazorTransferFormat format) => format switch
		{
			SKBlazorTransferFormat.Png => PngContentType,
			SKBlazorTransferFormat.Jpeg => JpegContentType,
			_ => RawContentType,
		};

		/// <summary>Produces the transfer payload for a frame in the requested format.</summary>
		/// <param name="image">The rendered frame. Expected to be RGBA raster-backed.</param>
		/// <param name="format">The transfer format.</param>
		/// <param name="quality">JPEG quality (0-100); ignored for other formats.</param>
		public static byte[] Produce(SKImage image, SKBlazorTransferFormat format, int quality)
		{
			if (image == null)
				throw new ArgumentNullException(nameof(image));

			switch (format)
			{
				case SKBlazorTransferFormat.Png:
					return Encode(image, SKEncodedImageFormat.Png, 100);
				case SKBlazorTransferFormat.Jpeg:
					return Encode(image, SKEncodedImageFormat.Jpeg, ClampQuality(quality));
				case SKBlazorTransferFormat.Put:
				default:
					return ReadRawRgba(image);
			}
		}

		private static byte[] Encode(SKImage image, SKEncodedImageFormat format, int quality)
		{
			using var data = image.Encode(format, quality);
			return data?.ToArray() ?? Array.Empty<byte>();
		}

		private static byte[] ReadRawRgba(SKImage image)
		{
			var info = new SKImageInfo(image.Width, image.Height, RgbaColorType, SKAlphaType.Unpremul);
			var buffer = new byte[info.BytesSize];
			if (buffer.Length == 0)
				return buffer;

			var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			try
			{
				if (!image.ReadPixels(info, handle.AddrOfPinnedObject(), info.RowBytes, 0, 0))
					return Array.Empty<byte>();
				return buffer;
			}
			finally
			{
				handle.Free();
			}
		}

		private static int ClampQuality(int quality)
		{
			if (quality < 0)
				return 0;
			if (quality > 100)
				return 100;
			return quality;
		}
	}
}
