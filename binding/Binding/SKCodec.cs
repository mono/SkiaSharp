﻿using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	// TODO: `Create(...)` should have overloads that accept a SKPngChunkReader
	// TODO: missing the `QueryYuv8` and `GetYuv8Planes` members
	// TODO: might be useful to wrap `GetFrameInfo` (single result)

	public class SKCodec : SKObject
	{
		[Preserve]
		internal SKCodec (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_codec_destroy (Handle);
			}

			base.Dispose (disposing);
		}

		public static int MinBufferedBytesNeeded {
			get { return (int)SkiaApi.sk_codec_min_buffered_bytes_needed (); }
		}

		public SKImageInfo Info {
			get {
				SKImageInfoNative cinfo;
				SkiaApi.sk_codec_get_info (Handle, out cinfo);
				return SKImageInfoNative.ToManaged (ref cinfo);
			}
		}

		public SKEncodedOrigin Origin {
			get { return SkiaApi.sk_codec_get_origin (Handle); }
		}

		public SKEncodedImageFormat EncodedFormat {
			get { return SkiaApi.sk_codec_get_encoded_format (Handle); }
		}

		public SKSizeI GetScaledDimensions (float desiredScale)
		{
			SKSizeI dimensions;
			SkiaApi.sk_codec_get_scaled_dimensions (Handle, desiredScale, out dimensions);
			return dimensions;
		}

		public bool GetValidSubset (ref SKRectI desiredSubset)
		{
			return SkiaApi.sk_codec_get_valid_subset (Handle, ref desiredSubset);
		}

		public byte[] Pixels {
			get {
				byte[] pixels = null;
				var result = GetPixels (out pixels);
				if (result != SKCodecResult.Success && result != SKCodecResult.IncompleteInput) {
					throw new Exception (result.ToString ());
				}
				return pixels;
			}
		}

		public int RepetitionCount => SkiaApi.sk_codec_get_repetition_count (Handle);

		public int FrameCount => SkiaApi.sk_codec_get_frame_count (Handle);

		public SKCodecFrameInfo[] FrameInfo {
			get {
				var length = SkiaApi.sk_codec_get_frame_count (Handle);
				var info = new SKCodecFrameInfo[length];
				SkiaApi.sk_codec_get_frame_info (Handle, info);
				return info;
			}
		}

		public bool GetFrameInfo (int index, out SKCodecFrameInfo frameInfo)
		{
			return SkiaApi.sk_codec_get_frame_info_for_index (Handle, index, out frameInfo);
		}

		public SKCodecResult GetPixels (out byte[] pixels)
		{
			return GetPixels (Info, out pixels);
		}

		public SKCodecResult GetPixels (SKImageInfo info, out byte[] pixels)
		{
			pixels = new byte[info.BytesSize];
			return GetPixels (info, pixels);
		}

		public SKCodecResult GetPixels (SKImageInfo info, byte[] pixels)
		{
			if (pixels == null)
				throw new ArgumentNullException (nameof (pixels));

			unsafe {
				fixed (byte* p = &pixels[0]) {
					return GetPixels (info, (IntPtr)p);
				}
			}
		}

		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options)
		{
			if (pixels == IntPtr.Zero)
				throw new ArgumentNullException (nameof (pixels));

			var nOptions = SKCodecOptionsInternal.FromManaged (ref options);
			var nInfo = SKImageInfoNative.FromManaged (ref info);

			return SkiaApi.sk_codec_get_pixels (Handle, ref nInfo, pixels, (IntPtr)rowBytes, ref nOptions);
		}

		[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr, int, SKCodecOptions) instead.")]
		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount)
		{
			return GetPixels (info, pixels, rowBytes, options);
		}

		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKCodecOptions options)
		{
			return GetPixels (info, pixels, info.RowBytes, options);
		}

		[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr, SKCodecOptions) instead.")]
		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount)
		{
			return GetPixels (info, pixels, info.RowBytes, options);
		}

		[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr) instead.")]
		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, IntPtr colorTable, ref int colorTableCount)
		{
			return GetPixels (info, pixels, info.RowBytes, SKCodecOptions.Default);
		}

		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels)
		{
			return GetPixels (info, pixels, info.RowBytes, SKCodecOptions.Default);
		}

		[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr, int, SKCodecOptions) instead.")]
		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount)
		{
			return GetPixels (info, pixels, rowBytes, options);
		}

		[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr, SKCodecOptions) instead.")]
		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount)
		{
			return GetPixels (info, pixels, info.RowBytes, options);
		}

		[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr) instead.")]
		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKColorTable colorTable, ref int colorTableCount)
		{
			return GetPixels (info, pixels, info.RowBytes, SKCodecOptions.Default);
		}

		[Obsolete ("The Index8 color type and color table is no longer supported. Use StartIncrementalDecode(SKImageInfo, IntPtr, int, SKCodecOptions) instead.")]
		public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount)
		{
			return StartIncrementalDecode (info, pixels, rowBytes, options);
		}

		public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options)
		{
			if (pixels == IntPtr.Zero)
				throw new ArgumentNullException (nameof (pixels));

			var nOptions = SKCodecOptionsInternal.FromManaged (ref options);
			var nInfo = SKImageInfoNative.FromManaged (ref info);

			return SkiaApi.sk_codec_start_incremental_decode (Handle, ref nInfo, pixels, (IntPtr)rowBytes, ref nOptions);
		}

		public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return SkiaApi.sk_codec_start_incremental_decode (Handle, ref cinfo, pixels, (IntPtr)rowBytes, IntPtr.Zero);
		}

		[Obsolete ("The Index8 color type and color table is no longer supported. Use StartIncrementalDecode(SKImageInfo, IntPtr, int, SKCodecOptions) instead.")]
		public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount)
		{
			return StartIncrementalDecode (info, pixels, rowBytes, options);
		}

		public SKCodecResult IncrementalDecode (out int rowsDecoded)
		{
			return SkiaApi.sk_codec_incremental_decode (Handle, out rowsDecoded);
		}

		public SKCodecResult IncrementalDecode ()
		{
			return SkiaApi.sk_codec_incremental_decode (Handle, out var rowsDecoded);
		}

		[Obsolete ("The Index8 color type and color table is no longer supported. Use StartScanlineDecode(SKImageInfo, SKCodecOptions) instead.")]
		public SKCodecResult StartScanlineDecode (SKImageInfo info, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount)
		{
			return StartScanlineDecode (info, options);
		}

		public SKCodecResult StartScanlineDecode (SKImageInfo info, SKCodecOptions options)
		{
			var nOptions = SKCodecOptionsInternal.FromManaged (ref options);
			var nInfo = SKImageInfoNative.FromManaged (ref info);

			return SkiaApi.sk_codec_start_scanline_decode (Handle, ref nInfo, ref nOptions);
		}

		public SKCodecResult StartScanlineDecode (SKImageInfo info)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return SkiaApi.sk_codec_start_scanline_decode (Handle, ref cinfo, IntPtr.Zero);
		}

		[Obsolete ("The Index8 color type and color table is no longer supported. Use StartScanlineDecode(SKImageInfo, SKCodecOptions) instead.")]
		public SKCodecResult StartScanlineDecode (SKImageInfo info, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount)
		{
			return StartScanlineDecode (info, options);
		}

		public int GetScanlines (IntPtr dst, int countLines, int rowBytes)
		{
			if (dst == IntPtr.Zero)
				throw new ArgumentNullException (nameof (dst));

			return SkiaApi.sk_codec_get_scanlines (Handle, dst, countLines, (IntPtr)rowBytes);
		}

		public bool SkipScanlines (int countLines) => SkiaApi.sk_codec_skip_scanlines (Handle, countLines);

		public SKCodecScanlineOrder ScanlineOrder => SkiaApi.sk_codec_get_scanline_order (Handle);

		public int NextScanline => SkiaApi.sk_codec_next_scanline (Handle);

		public int GetOutputScanline (int inputScanline) => SkiaApi.sk_codec_output_scanline (Handle, inputScanline);

		public static SKCodec Create (SKStream stream)
		{
			return Create (stream, out var result);
		}

		public static SKCodec Create (SKStream stream, out SKCodecResult result)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));
			var codec = GetObject<SKCodec> (SkiaApi.sk_codec_new_from_stream (stream.Handle, out result));
			stream.RevokeOwnership ();
			return codec;
		}

		public static SKCodec Create (SKData data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));
			return GetObject<SKCodec> (SkiaApi.sk_codec_new_from_data (data.Handle));
		}
	}
}
