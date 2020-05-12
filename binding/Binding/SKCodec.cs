using System;
using System.ComponentModel;
using System.IO;

namespace SkiaSharp
{
	// TODO: `Create(...)` should have overloads that accept a SKPngChunkReader
	// TODO: missing the `QueryYuv8` and `GetYuv8Planes` members

	public unsafe class SKCodec : SKObject, ISKSkipObjectRegistration
	{
		internal SKCodec (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_codec_destroy (Handle);

		public static int MinBufferedBytesNeeded =>
			(int)SkiaApi.sk_codec_min_buffered_bytes_needed ();

		public SKImageInfo Info {
			get {
				SKImageInfoNative cinfo;
				SkiaApi.sk_codec_get_info (Handle, &cinfo);
				return SKImageInfoNative.ToManaged (ref cinfo);
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use EncodedOrigin instead.")]
		public SKCodecOrigin Origin =>
			(SKCodecOrigin)EncodedOrigin;

		public SKEncodedOrigin EncodedOrigin =>
			SkiaApi.sk_codec_get_origin (Handle);

		public SKEncodedImageFormat EncodedFormat =>
			SkiaApi.sk_codec_get_encoded_format (Handle);

		public SKSizeI GetScaledDimensions (float desiredScale)
		{
			SKSizeI dimensions;
			SkiaApi.sk_codec_get_scaled_dimensions (Handle, desiredScale, &dimensions);
			return dimensions;
		}

		public bool GetValidSubset (ref SKRectI desiredSubset)
		{
			fixed (SKRectI* ds = &desiredSubset) {
				return SkiaApi.sk_codec_get_valid_subset (Handle, ds);
			}
		}

		public byte[] Pixels {
			get {
				var result = GetPixels (out var pixels);
				if (result != SKCodecResult.Success && result != SKCodecResult.IncompleteInput) {
					throw new Exception (result.ToString ());
				}
				return pixels;
			}
		}

		// frames

		public int RepetitionCount =>
			SkiaApi.sk_codec_get_repetition_count (Handle);

		public int FrameCount =>
			SkiaApi.sk_codec_get_frame_count (Handle);

		public SKCodecFrameInfo[] FrameInfo {
			get {
				var length = SkiaApi.sk_codec_get_frame_count (Handle);
				var info = new SKCodecFrameInfo[length];
				fixed (SKCodecFrameInfo* i = info) {
					SkiaApi.sk_codec_get_frame_info (Handle, i);
				}
				return info;
			}
		}

		public bool GetFrameInfo (int index, out SKCodecFrameInfo frameInfo)
		{
			fixed (SKCodecFrameInfo* f = &frameInfo) {
				return SkiaApi.sk_codec_get_frame_info_for_index (Handle, index, f);
			}
		}

		// pixels

		public SKCodecResult GetPixels (out byte[] pixels) =>
			GetPixels (Info, out pixels);

		public SKCodecResult GetPixels (SKImageInfo info, out byte[] pixels)
		{
			pixels = new byte[info.BytesSize];
			return GetPixels (info, pixels);
		}

		public SKCodecResult GetPixels (SKImageInfo info, byte[] pixels)
		{
			if (pixels == null)
				throw new ArgumentNullException (nameof (pixels));

			fixed (byte* p = pixels) {
				return GetPixels (info, (IntPtr)p, info.RowBytes, SKCodecOptions.Default);
			}
		}

		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels) =>
			GetPixels (info, pixels, info.RowBytes, SKCodecOptions.Default);

		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKCodecOptions options) =>
			GetPixels (info, pixels, info.RowBytes, options);

		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options)
		{
			if (pixels == IntPtr.Zero)
				throw new ArgumentNullException (nameof (pixels));

			var nInfo = SKImageInfoNative.FromManaged (ref info);
			var nOptions = new SKCodecOptionsInternal {
				fZeroInitialized = options.ZeroInitialized,
				fSubset = null,
				fFrameIndex = options.FrameIndex,
				fPriorFrame = options.PriorFrame,
				fPremulBehavior = options.PremulBehavior,
			};
			var subset = default (SKRectI);
			if (options.HasSubset) {
				subset = options.Subset.Value;
				nOptions.fSubset = &subset;
			}
			return SkiaApi.sk_codec_get_pixels (Handle, &nInfo, (void*)pixels, (IntPtr)rowBytes, &nOptions);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr, int, SKCodecOptions) instead.")]
		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount) =>
			GetPixels (info, pixels, rowBytes, options);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr, SKCodecOptions) instead.")]
		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount) =>
			GetPixels (info, pixels, info.RowBytes, options);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr) instead.")]
		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, IntPtr colorTable, ref int colorTableCount) =>
			GetPixels (info, pixels, info.RowBytes, SKCodecOptions.Default);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr, int, SKCodecOptions) instead.")]
		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount) =>
			GetPixels (info, pixels, rowBytes, options);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr, SKCodecOptions) instead.")]
		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount) =>
			GetPixels (info, pixels, info.RowBytes, options);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr) instead.")]
		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKColorTable colorTable, ref int colorTableCount) =>
			GetPixels (info, pixels, info.RowBytes, SKCodecOptions.Default);

		// incremental (start)

		public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options)
		{
			if (pixels == IntPtr.Zero)
				throw new ArgumentNullException (nameof (pixels));

			var nInfo = SKImageInfoNative.FromManaged (ref info);
			var nOptions = new SKCodecOptionsInternal {
				fZeroInitialized = options.ZeroInitialized,
				fSubset = null,
				fFrameIndex = options.FrameIndex,
				fPriorFrame = options.PriorFrame,
				fPremulBehavior = options.PremulBehavior,
			};
			var subset = default (SKRectI);
			if (options.HasSubset) {
				subset = options.Subset.Value;
				nOptions.fSubset = &subset;
			}

			return SkiaApi.sk_codec_start_incremental_decode (Handle, &nInfo, (void*)pixels, (IntPtr)rowBytes, &nOptions);
		}

		public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return SkiaApi.sk_codec_start_incremental_decode (Handle, &cinfo, (void*)pixels, (IntPtr)rowBytes, null);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The Index8 color type and color table is no longer supported. Use StartIncrementalDecode(SKImageInfo, IntPtr, int, SKCodecOptions) instead.")]
		public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount) =>
			StartIncrementalDecode (info, pixels, rowBytes, options);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The Index8 color type and color table is no longer supported. Use StartIncrementalDecode(SKImageInfo, IntPtr, int, SKCodecOptions) instead.")]
		public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount) =>
			StartIncrementalDecode (info, pixels, rowBytes, options);

		// incremental (step)

		public SKCodecResult IncrementalDecode (out int rowsDecoded)
		{
			fixed (int* r = &rowsDecoded) {
				return SkiaApi.sk_codec_incremental_decode (Handle, r);
			}
		}

		public SKCodecResult IncrementalDecode () =>
			SkiaApi.sk_codec_incremental_decode (Handle, null);

		// scanline (start)

		public SKCodecResult StartScanlineDecode (SKImageInfo info, SKCodecOptions options)
		{
			var nInfo = SKImageInfoNative.FromManaged (ref info);
			var nOptions = new SKCodecOptionsInternal {
				fZeroInitialized = options.ZeroInitialized,
				fSubset = null,
				fFrameIndex = options.FrameIndex,
				fPriorFrame = options.PriorFrame,
				fPremulBehavior = options.PremulBehavior,
			};
			var subset = default (SKRectI);
			if (options.HasSubset) {
				subset = options.Subset.Value;
				nOptions.fSubset = &subset;
			}

			return SkiaApi.sk_codec_start_scanline_decode (Handle, &nInfo, &nOptions);
		}

		public SKCodecResult StartScanlineDecode (SKImageInfo info)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return SkiaApi.sk_codec_start_scanline_decode (Handle, &cinfo, null);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The Index8 color type and color table is no longer supported. Use StartScanlineDecode(SKImageInfo, SKCodecOptions) instead.")]
		public SKCodecResult StartScanlineDecode (SKImageInfo info, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount) =>
			StartScanlineDecode (info, options);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The Index8 color type and color table is no longer supported. Use StartScanlineDecode(SKImageInfo, SKCodecOptions) instead.")]
		public SKCodecResult StartScanlineDecode (SKImageInfo info, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount) =>
			StartScanlineDecode (info, options);

		// scanline (step)

		public int GetScanlines (IntPtr dst, int countLines, int rowBytes)
		{
			if (dst == IntPtr.Zero)
				throw new ArgumentNullException (nameof (dst));

			return SkiaApi.sk_codec_get_scanlines (Handle, (void*)dst, countLines, (IntPtr)rowBytes);
		}

		public bool SkipScanlines (int countLines) =>
			SkiaApi.sk_codec_skip_scanlines (Handle, countLines);

		public SKCodecScanlineOrder ScanlineOrder =>
			SkiaApi.sk_codec_get_scanline_order (Handle);

		public int NextScanline => SkiaApi.sk_codec_next_scanline (Handle);

		public int GetOutputScanline (int inputScanline) =>
			SkiaApi.sk_codec_output_scanline (Handle, inputScanline);

		// create (streams)

		public static SKCodec Create (string filename) =>
			Create (filename, out var result);

		public static SKCodec Create (string filename, out SKCodecResult result)
		{
			var stream = SKFileStream.OpenStream (filename);
			if (stream == null) {
				result = SKCodecResult.InternalError;
				return null;
			}

			return Create (stream, out result);
		}

		public static SKCodec Create (Stream stream) =>
			Create (stream, out var result);

		public static SKCodec Create (Stream stream, out SKCodecResult result) =>
			Create (WrapManagedStream (stream), out result);

		public static SKCodec Create (SKStream stream) =>
			Create (stream, out var result);

		public static SKCodec Create (SKStream stream, out SKCodecResult result)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			fixed (SKCodecResult* r = &result) {
				var codec = GetObject (SkiaApi.sk_codec_new_from_stream (stream.Handle, r));
				stream.RevokeOwnership (codec);
				return codec;
			}
		}

		// create (data)

		public static SKCodec Create (SKData data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			return GetObject (SkiaApi.sk_codec_new_from_data (data.Handle));
		}

		// utils

		internal static SKStream WrapManagedStream (Stream stream)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}

			// we will need a seekable stream, so buffer it if need be
			if (stream.CanSeek) {
				return new SKManagedStream (stream, true);
			} else {
				return new SKFrontBufferedManagedStream (stream, MinBufferedBytesNeeded, true);
			}
		}

		internal static SKCodec GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new SKCodec (handle, true);
	}
}
