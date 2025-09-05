#nullable disable

using System;
using System.ComponentModel;
using System.IO;

namespace SkiaSharp
{
	// TODO: `Create(...)` should have overloads that accept a SKPngChunkReader
	// TODO: missing the `QueryYuv8` and `GetYuv8Planes` members

	/// <summary>
	/// An abstraction layer directly on top of an image codec.
	/// </summary>
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

		/// <summary>
		/// Gets the minimum number of bytes that must be buffered in stream input.
		/// </summary>
		public static int MinBufferedBytesNeeded =>
			(int)SkiaApi.sk_codec_min_buffered_bytes_needed ();

		/// <summary>
		/// Gets the image information from the codec.
		/// </summary>
		public SKImageInfo Info {
			get {
				SKImageInfoNative cinfo;
				SkiaApi.sk_codec_get_info (Handle, &cinfo);
				return SKImageInfoNative.ToManaged (ref cinfo);
			}
		}

		/// <summary>
		/// Gets the image origin from the codec.
		/// </summary>
		public SKEncodedOrigin EncodedOrigin =>
			SkiaApi.sk_codec_get_origin (Handle);

		/// <summary>
		/// Gets the image encoding from the codec.
		/// </summary>
		public SKEncodedImageFormat EncodedFormat =>
			SkiaApi.sk_codec_get_encoded_format (Handle);

		/// <summary>
		/// Returns a size that approximately supports the desired scale factor.
		/// </summary>
		/// <param name="desiredScale">The desired scale factor.</param>
		/// <returns>Returns a supported size.</returns>
		/// <remarks>The codec may not be able to scale efficiently to the exact scale factor requested, so return a size that approximates that scale. Upscaling is not supported, so the original size will be returned.</remarks>
		public SKSizeI GetScaledDimensions (float desiredScale)
		{
			SKSizeI dimensions;
			SkiaApi.sk_codec_get_scaled_dimensions (Handle, desiredScale, &dimensions);
			return dimensions;
		}

		/// <summary>
		/// Modifies the specified subset to one that can decoded from this codec.
		/// </summary>
		/// <param name="desiredSubset">The desired subset of the original bounds, which may be modified to a subset which is supported.</param>
		/// <returns>Returns true if this codec supports decoding the desired subset, otherwise false. The final subset can be used with <see cref="P:SkiaSharp.SKCodecOptions.Subset" />.</returns>
		public bool GetValidSubset (ref SKRectI desiredSubset)
		{
			fixed (SKRectI* ds = &desiredSubset) {
				return SkiaApi.sk_codec_get_valid_subset (Handle, ds);
			}
		}

		/// <summary>
		/// Gets the image data from the codec using the current <see cref="P:SkiaSharp.SKCodec.Info" />.
		/// </summary>
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

		/// <summary>
		/// Gets the number of times to repeat, if this image is animated.
		/// </summary>
		/// <remarks><para>For infinite repetition of frames, this will be -1.</para><para>May require reading the stream to find the repetition count. As such, future decoding calls may require a rewind. For single-frame images, this will be 0.</para></remarks>
		public int RepetitionCount =>
			SkiaApi.sk_codec_get_repetition_count (Handle);

		/// <summary>
		/// Gets the number of frames in the encoded image.
		/// </summary>
		/// <remarks>May require reading through the stream to determine info about the frames. As such, future decoding calls may require a rewind. For single-frame images, this will be zero.</remarks>
		public int FrameCount =>
			SkiaApi.sk_codec_get_frame_count (Handle);

		/// <summary>
		/// Gets information about the frames in the encoded image.
		/// </summary>
		/// <remarks>May require reading through the stream to determine info about the frames. As such, future decoding calls may require a rewind. For single-frame images, this will be an empty array.</remarks>
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

		/// <summary>
		/// Gets information about a specific frame in the encoded image.
		/// </summary>
		/// <param name="index">The index of the frame to retrieve.</param>
		/// <param name="frameInfo">The information about the frame.</param>
		/// <returns>Returns true if the frame was successfully read, otherwise false.</returns>
		/// <remarks>May require reading through the stream to determine info about the frames. As such, future decoding calls may require a rewind.</remarks>
		public bool GetFrameInfo (int index, out SKCodecFrameInfo frameInfo)
		{
			fixed (SKCodecFrameInfo* f = &frameInfo) {
				return SkiaApi.sk_codec_get_frame_info_for_index (Handle, index, f);
			}
		}

		// pixels

		/// <summary>
		/// Decode the bitmap into the specified memory block.
		/// </summary>
		/// <param name="pixels">The memory block with the decoded bitmap.</param>
		/// <returns>Returns <see cref="F:SkiaSharp.SKCodecResult.Success" /> on success, or another value explaining the type of failure.</returns>
		public SKCodecResult GetPixels (out byte[] pixels) =>
			GetPixels (Info, out pixels);

		/// <summary>
		/// Decode the bitmap into the specified memory block.
		/// </summary>
		/// <param name="info">The description of the desired output format expected by the caller.</param>
		/// <param name="pixels">The memory block with the decoded bitmap.</param>
		/// <returns>Returns <see cref="F:SkiaSharp.SKCodecResult.Success" /> on success, or another value explaining the type of failure.</returns>
		/// <remarks>The specified <see cref="SkiaSharp.SKImageInfo" />, can either be
		/// <see cref="SkiaSharp.SKCodec.Info" />, or a new instance with a different
		/// configuration - which the codec may choose to ignore.
		/// If the specified size is different from the size from
		/// <see cref="SkiaSharp.SKCodec.Info" />, then the codec will attempt to scale the
		/// resulting bitmap. If the codec cannot perform this scale, this method will
		/// return <see cref="SkiaSharp.SKCodecResult.InvalidScale" />.</remarks>
		public SKCodecResult GetPixels (SKImageInfo info, out byte[] pixels)
		{
			pixels = new byte[info.BytesSize];
			return GetPixels (info, pixels);
		}

		/// <summary>
		/// Decode the bitmap into the specified memory block.
		/// </summary>
		/// <param name="info">The description of the desired output format expected by the caller.</param>
		/// <param name="pixels">The memory block to hold the decoded bitmap, with a length of at least <see cref="P:SkiaSharp.SKImageInfo.BytesSize" />.</param>
		/// <returns>Returns <see cref="F:SkiaSharp.SKCodecResult.Success" /> on success, or another value explaining the type of failure.</returns>
		/// <remarks>The specified <see cref="SkiaSharp.SKImageInfo" />, can either be
		/// <see cref="SkiaSharp.SKCodec.Info" />, or a new instance with a different
		/// configuration - which the codec may choose to ignore.
		/// If the specified size is different from the size from
		/// <see cref="SkiaSharp.SKCodec.Info" />, then the codec will attempt to scale the
		/// resulting bitmap. If the codec cannot perform this scale, this method will
		/// return <see cref="SkiaSharp.SKCodecResult.InvalidScale" />.</remarks>
		public SKCodecResult GetPixels (SKImageInfo info, byte[] pixels)
		{
			if (pixels == null)
				throw new ArgumentNullException (nameof (pixels));

			fixed (byte* p = pixels) {
				return GetPixels (info, (IntPtr)p, info.RowBytes, SKCodecOptions.Default);
			}
		}

		/// <summary>
		/// Decode the bitmap into the specified memory block.
		/// </summary>
		/// <param name="info">The description of the desired output format expected by the caller.</param>
		/// <param name="pixels">The memory block to hold the decoded bitmap, with a total size of at least <see cref="P:SkiaSharp.SKImageInfo.BytesSize" />.</param>
		/// <returns>Returns <see cref="F:SkiaSharp.SKCodecResult.Success" /> on success, or another value explaining the type of failure.</returns>
		/// <remarks>The specified <see cref="SkiaSharp.SKImageInfo" />, can either be
		/// <see cref="SkiaSharp.SKCodec.Info" />, or a new instance with a different
		/// configuration - which the codec may choose to ignore.
		/// If the specified size is different from the size from
		/// <see cref="SkiaSharp.SKCodec.Info" />, then the codec will attempt to scale the
		/// resulting bitmap. If the codec cannot perform this scale, this method will
		/// return <see cref="SkiaSharp.SKCodecResult.InvalidScale" />.</remarks>
		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels) =>
			GetPixels (info, pixels, info.RowBytes, SKCodecOptions.Default);

		/// <summary>
		/// Decode the bitmap into the specified memory block.
		/// </summary>
		/// <param name="info">The description of the desired output format expected by the caller.</param>
		/// <param name="pixels">The memory block to hold the decoded bitmap, with a total size of at least <see cref="P:SkiaSharp.SKImageInfo.BytesSize" />.</param>
		/// <param name="options">The bitmap decoding options.</param>
		/// <returns>Returns <see cref="F:SkiaSharp.SKCodecResult.Success" /> on success, or another value explaining the type of failure.</returns>
		/// <remarks>The specified <see cref="SkiaSharp.SKImageInfo" />, can either be
		/// <see cref="SkiaSharp.SKCodec.Info" />, or a new instance with a different
		/// configuration - which the codec may choose to ignore.
		/// If the specified size is different from the size from
		/// <see cref="SkiaSharp.SKCodec.Info" />, then the codec will attempt to scale the
		/// resulting bitmap. If the codec cannot perform this scale, this method will
		/// return <see cref="SkiaSharp.SKCodecResult.InvalidScale" />.</remarks>
		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKCodecOptions options) =>
			GetPixels (info, pixels, info.RowBytes, options);

		/// <summary>
		/// Decode the bitmap into the specified memory block.
		/// </summary>
		/// <param name="info">The description of the desired output format expected by the caller.</param>
		/// <param name="pixels">The memory block to hold the decoded bitmap, with a total size of at least <see cref="P:SkiaSharp.SKImageInfo.BytesSize" />.</param>
		/// <param name="rowBytes">The number of bytes in a row, typically <see cref="P:SkiaSharp.SKImageInfo.RowBytes" />.</param>
		/// <param name="options">The bitmap decoding options.</param>
		/// <returns>Returns <see cref="F:SkiaSharp.SKCodecResult.Success" /> on success, or another value explaining the type of failure.</returns>
		/// <remarks>The specified <see cref="SkiaSharp.SKImageInfo" />, can either be
		/// <see cref="SkiaSharp.SKCodec.Info" />, or a new instance with a different
		/// configuration - which the codec may choose to ignore.
		/// If the specified size is different from the size from
		/// <see cref="SkiaSharp.SKCodec.Info" />, then the codec will attempt to scale the
		/// resulting bitmap. If the codec cannot perform this scale, this method will
		/// return <see cref="SkiaSharp.SKCodecResult.InvalidScale" />.</remarks>
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
			};
			var subset = default (SKRectI);
			if (options.HasSubset) {
				subset = options.Subset.Value;
				nOptions.fSubset = &subset;
			}
			return SkiaApi.sk_codec_get_pixels (Handle, &nInfo, (void*)pixels, (IntPtr)rowBytes, &nOptions);
		}

		// incremental (start)

		/// <summary>
		/// Prepare for an incremental decode with the specified options.
		/// </summary>
		/// <param name="info">The image information of the destination. If the dimensions do not match those of <see cref="P:SkiaSharp.SKCodec.Info" />, this implies a scale.</param>
		/// <param name="pixels">The memory to write to. Needs to be large enough to hold the subset, if present, or the full image.</param>
		/// <param name="rowBytes">The stride of the memory to write to.</param>
		/// <param name="options">The decoding options, including if memory is zero initialized and whether to decode a subset.</param>
		/// <returns>Returns <see cref="F:SkiaSharp.SKCodecResult.Success" /> on success, or another value explaining the type of failure.</returns>
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
			};
			var subset = default (SKRectI);
			if (options.HasSubset) {
				subset = options.Subset.Value;
				nOptions.fSubset = &subset;
			}

			return SkiaApi.sk_codec_start_incremental_decode (Handle, &nInfo, (void*)pixels, (IntPtr)rowBytes, &nOptions);
		}

		/// <summary>
		/// Prepare for an incremental decode with the specified options.
		/// </summary>
		/// <param name="info">The image information of the destination. If the dimensions do not match those of <see cref="P:SkiaSharp.SKCodec.Info" />, this implies a scale.</param>
		/// <param name="pixels">The memory to write to. Needs to be large enough to hold the subset, if present, or the full image.</param>
		/// <param name="rowBytes">The stride of the memory to write to.</param>
		/// <returns>Returns <see cref="F:SkiaSharp.SKCodecResult.Success" /> on success, or another value explaining the type of failure.</returns>
		public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return SkiaApi.sk_codec_start_incremental_decode (Handle, &cinfo, (void*)pixels, (IntPtr)rowBytes, null);
		}

		// incremental (step)

		/// <summary>
		/// Start or continue the incremental decode.
		/// </summary>
		/// <param name="rowsDecoded">The total number of lines initialized. Only meaningful if this method returns <see cref="F:SkiaSharp.SKCodecResult.IncompleteInput" />.</param>
		/// <returns>Returns <see cref="F:SkiaSharp.SKCodecResult.Success" /> if all lines requested in <see cref="M:SkiaSharp.SKCodec.StartIncrementalDecode(SkiaSharp.SKImageInfo,System.IntPtr,System.Int32)" /> have been completely decoded. <see cref="F:SkiaSharp.SKCodecResult.IncompleteInput" /> otherwise.</returns>
		/// <remarks>Unlike <see cref="M:SkiaSharp.SKCodec.GetPixels(System.Byte[]@)" />, this does not do any filling. This is left up to the caller, since they may be skipping lines or continuing the decode later.</remarks>
		public SKCodecResult IncrementalDecode (out int rowsDecoded)
		{
			fixed (int* r = &rowsDecoded) {
				return SkiaApi.sk_codec_incremental_decode (Handle, r);
			}
		}

		/// <summary>
		/// Start or continue the incremental decode.
		/// </summary>
		/// <returns>Returns <see cref="F:SkiaSharp.SKCodecResult.Success" /> if all lines requested in <see cref="M:SkiaSharp.SKCodec.StartIncrementalDecode(SkiaSharp.SKImageInfo,System.IntPtr,System.Int32)" /> have been completely decoded. <see cref="F:SkiaSharp.SKCodecResult.IncompleteInput" /> otherwise.</returns>
		/// <remarks>Unlike <see cref="M:SkiaSharp.SKCodec.GetPixels(System.Byte[]@)" />, this does not do any filling. This is left up to the caller, since they may be skipping lines or continuing the decode later.</remarks>
		public SKCodecResult IncrementalDecode () =>
			SkiaApi.sk_codec_incremental_decode (Handle, null);

		// scanline (start)

		/// <summary>
		/// Prepare for a scanline decode with the specified options.
		/// </summary>
		/// <param name="info">The image information of the destination. If the dimensions do not match those of <see cref="P:SkiaSharp.SKCodec.Info" />, this implies a scale.</param>
		/// <param name="options">The decoding options, including if memory is zero initialized and whether to decode a subset.</param>
		/// <returns>Returns <see cref="F:SkiaSharp.SKCodecResult.Success" /> on success, or another value explaining the type of failure.</returns>
		/// <remarks>Not all codecs support this.</remarks>
		public SKCodecResult StartScanlineDecode (SKImageInfo info, SKCodecOptions options)
		{
			var nInfo = SKImageInfoNative.FromManaged (ref info);
			var nOptions = new SKCodecOptionsInternal {
				fZeroInitialized = options.ZeroInitialized,
				fSubset = null,
				fFrameIndex = options.FrameIndex,
				fPriorFrame = options.PriorFrame,
			};
			var subset = default (SKRectI);
			if (options.HasSubset) {
				subset = options.Subset.Value;
				nOptions.fSubset = &subset;
			}

			return SkiaApi.sk_codec_start_scanline_decode (Handle, &nInfo, &nOptions);
		}

		/// <summary>
		/// Prepare for a scanline decode.
		/// </summary>
		/// <param name="info">The image information of the destination. If the dimensions do not match those of <see cref="P:SkiaSharp.SKCodec.Info" />, this implies a scale.</param>
		/// <returns>Returns <see cref="F:SkiaSharp.SKCodecResult.Success" /> on success, or another value explaining the type of failure.</returns>
		/// <remarks>Not all codecs support this.</remarks>
		public SKCodecResult StartScanlineDecode (SKImageInfo info)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return SkiaApi.sk_codec_start_scanline_decode (Handle, &cinfo, null);
		}

		// scanline (step)

		/// <summary>
		/// Writes the next set of scanlines into the destination.
		/// </summary>
		/// <param name="dst">The memory location to store the scanlines.</param>
		/// <param name="countLines">The number of lines to write.</param>
		/// <param name="rowBytes">The number of bytes per row.</param>
		/// <returns>Returns the number of lines successfully decoded.</returns>
		/// <remarks>If number of lines successfully decoded is less than <paramref name="countLines" />, this will fill the remaining lines with a default value.</remarks>
		public int GetScanlines (IntPtr dst, int countLines, int rowBytes)
		{
			if (dst == IntPtr.Zero)
				throw new ArgumentNullException (nameof (dst));

			return SkiaApi.sk_codec_get_scanlines (Handle, (void*)dst, countLines, (IntPtr)rowBytes);
		}

		/// <summary>
		/// Skip the specified number of scanlines.
		/// </summary>
		/// <param name="countLines">The number of scanlines to skip.</param>
		/// <returns>Returns <see langword="true" /> if the scanlines were successfully skipped, otherwise <see langword="false" /> on failure (incomplete input, the number of lines is less than zero, read all the lines).</returns>
		public bool SkipScanlines (int countLines) =>
			SkiaApi.sk_codec_skip_scanlines (Handle, countLines);

		/// <summary>
		/// Gets the order in which scanlines will be returned by the scanline decoder.
		/// </summary>
		public SKCodecScanlineOrder ScanlineOrder =>
			SkiaApi.sk_codec_get_scanline_order (Handle);

		/// <summary>
		/// Gets the y-coordinate of the next row to be returned by the scanline decoder.
		/// </summary>
		public int NextScanline => SkiaApi.sk_codec_next_scanline (Handle);

		/// <summary>
		/// Returns the output y-coordinate of the row that corresponds to an input y-coordinate.
		/// </summary>
		/// <param name="inputScanline">The scanline that is located in the encoded data.</param>
		/// <returns>Returns the output y-coordinate of the row.</returns>
		/// <remarks>This will equal <paramref name="inputScanline" />, except in the case of strangely encoded image types (bottom-up BMPs, interlaced GIFs).</remarks>
		public int GetOutputScanline (int inputScanline) =>
			SkiaApi.sk_codec_output_scanline (Handle, inputScanline);

		// create (streams)

		/// <summary>
		/// Creates a codec from the specified file.
		/// </summary>
		/// <param name="filename">The path to an encoded image on the file system.</param>
		/// <returns>Returns the new instance of the codec, or <see langword="null" /> if there was an error.</returns>
		public static SKCodec Create (string filename) =>
			Create (filename, out var result);

		/// <summary>
		/// Creates a codec from the specified file.
		/// </summary>
		/// <param name="filename">The path to an encoded image on the file system.</param>
		/// <param name="result">The result of the creation operation.</param>
		/// <returns>Returns the new instance of the codec, or <see langword="null" /> if there was an error.</returns>
		public static SKCodec Create (string filename, out SKCodecResult result)
		{
			var stream = SKFileStream.OpenStream (filename);
			if (stream == null) {
				result = SKCodecResult.InternalError;
				return null;
			}

			return Create (stream, out result);
		}

		/// <summary>
		/// Creates a codec from the specified stream.
		/// </summary>
		/// <param name="stream">The stream to use when creating the codec.</param>
		/// <returns>Returns the new instance of the codec, or <see langword="null" /> if there was an error.</returns>
		/// <remarks>If <see langword="null" /> is returned, the stream is deleted immediately. Otherwise, the codec takes ownership of it, and will delete it when done with it.</remarks>
		public static SKCodec Create (Stream stream) =>
			Create (stream, out var result);

		/// <summary>
		/// Creates a codec from the specified stream.
		/// </summary>
		/// <param name="stream">The stream to use when creating the codec.</param>
		/// <param name="result">The result of the creation operation.</param>
		/// <returns>Returns the new instance of the codec, or <see langword="null" /> if there was an error.</returns>
		/// <remarks>If <see langword="null" /> is returned, the stream is deleted immediately. Otherwise, the codec takes ownership of it, and will delete it when done with it.</remarks>
		public static SKCodec Create (Stream stream, out SKCodecResult result) =>
			Create (WrapManagedStream (stream), out result);

		/// <summary>
		/// Creates a codec from the specified stream.
		/// </summary>
		/// <param name="stream">The stream to use when creating the codec.</param>
		/// <returns>Returns the new instance of the codec, or <see langword="null" /> if there was an error.</returns>
		/// <remarks>If <see langword="null" /> is returned, the stream is deleted immediately. Otherwise, the codec takes ownership of it, and will delete it when done with it.</remarks>
		public static SKCodec Create (SKStream stream) =>
			Create (stream, out var result);

		/// <summary>
		/// Creates a codec from the specified stream.
		/// </summary>
		/// <param name="stream">The stream to use when creating the codec.</param>
		/// <param name="result">The result of the creation operation.</param>
		/// <returns>Returns the new instance of the codec, or <see langword="null" /> if there was an error.</returns>
		/// <remarks>If <see langword="null" /> is returned, the stream is deleted immediately. Otherwise, the codec takes ownership of it, and will delete it when done with it.</remarks>
		public static SKCodec Create (SKStream stream, out SKCodecResult result)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));
			if (stream is SKFileStream filestream && !filestream.IsValid)
				throw new ArgumentException ("File stream was not valid.", nameof(stream));

			fixed (SKCodecResult* r = &result) {
				var codec = GetObject (SkiaApi.sk_codec_new_from_stream (stream.Handle, r));
				stream.RevokeOwnership (codec);
				return codec;
			}
		}

		// create (data)

		/// <summary>
		/// Creates a codec from the specified data.
		/// </summary>
		/// <param name="data">The data to use when creating the codec.</param>
		/// <returns>Returns the new instance of the codec, or <see langword="null" /> if there was an error.</returns>
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
