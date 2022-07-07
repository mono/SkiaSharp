using System;
using System.ComponentModel;
using System.IO;

namespace SkiaSharp
{
	public unsafe class SKAndroidCodec : SKObject, ISKSkipObjectRegistration
	{
		internal SKAndroidCodec(IntPtr handle, bool owns)
			: base(handle, owns)
		{
		}

		protected override void Dispose(bool disposing) =>
			base.Dispose(disposing);

		protected override void DisposeNative() =>
			SkiaApi.sk_android_codec_destroy(Handle);

		public SKImageInfo Info
		{
			get
			{
				SKImageInfoNative cinfo;
				SkiaApi.sk_android_codec_get_info(Handle, &cinfo);
				return SKImageInfoNative.ToManaged(ref cinfo);
			}
		}

		public SKCodec Codec =>
			SKCodec.GetObject(SkiaApi.sk_android_codec_get_codec(Handle));

		public SKColorSpaceIccProfile ICCProfile =>
			SKColorSpaceIccProfile.GetObject(SkiaApi.sk_android_codec_get_icc_profile(Handle));

		public SKEncodedImageFormat EncodedFormat =>
			SkiaApi.sk_android_codec_get_encoded_format(Handle);

		public SKColorType ComputeOutputColorType(SKColorType requestedColorType)
		{
			return SkiaApi.sk_android_codec_compute_output_color_type(Handle, requestedColorType.ToNative()).FromNative();
		}

		public SKAlphaType ComputeOutputAlphaType(bool requestedUnpremul)
		{
			return SkiaApi.sk_android_codec_compute_output_alpha(Handle, requestedUnpremul);
		}

		public SKColorSpace ComputeOutputColorSpace(SKColorType outputColorType, SKColorSpace preferredColorSpace)
		{
			return SKColorSpace.GetObject(SkiaApi.sk_android_codec_compute_output_color_space(Handle, outputColorType.ToNative(), preferredColorSpace == null ? IntPtr.Zero : preferredColorSpace.Handle));
		}

		public SKSizeI GetSampledDimensions(int sampleSize)
		{
			SKSizeI dimensions;
			SkiaApi.sk_android_codec_get_sampled_dimensions(Handle, sampleSize, &dimensions);
			return dimensions;
		}

		public bool GetSupportedSubset(ref SKRectI desiredSubset)
		{
			fixed (SKRectI* ds = &desiredSubset)
			{
				return SkiaApi.sk_android_codec_get_supported_subset(Handle, ds);
			}
		}

		public SKSizeI GetSampledSubsetDimensions(int sampleSize, ref SKRectI desiredSubset)
		{
			SKSizeI dimensions;
			fixed (SKRectI* ds = &desiredSubset)
			{
				SkiaApi.sk_android_codec_get_sampled_subset_dimensions(Handle, sampleSize, &dimensions, ds);
			}
			return dimensions;
		}

		// android pixels

		public SKCodecResult GetAndroidPixels (out byte[] pixels, SKAndroidCodecOptions options) =>
			GetAndroidPixels (Info, out pixels, options);

		public SKCodecResult GetAndroidPixels (SKImageInfo info, out byte[] pixels, SKAndroidCodecOptions options)
		{
			pixels = new byte[info.BytesSize];
			return GetAndroidPixels (info, pixels, options);
		}

		public SKCodecResult GetAndroidPixels (SKImageInfo info, byte[] pixels, SKAndroidCodecOptions options)
		{
			if (pixels == null)
				throw new ArgumentNullException (nameof (pixels));

			fixed (byte* p = pixels) {
				return GetAndroidPixels (info, (IntPtr)p, info.RowBytes, options);
			}
		}

		public SKCodecResult GetAndroidPixels (SKImageInfo info, IntPtr pixels, SKAndroidCodecOptions options) =>
			GetAndroidPixels (info, pixels, info.RowBytes, options);

		public SKCodecResult GetAndroidPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKAndroidCodecOptions options)
		{
			if (pixels == IntPtr.Zero)
				throw new ArgumentNullException (nameof (pixels));

			if (options == null)
				throw new ArgumentNullException (nameof (options));

			var nInfo = SKImageInfoNative.FromManaged (ref info);
			var nOptions = new SKAndroidCodecOptionsInternal {
				fZeroInitialized = options.ZeroInitialized,
				fSubset = null,
				fSampleSize = options.SampleSize,
			};
			var subset = default (SKRectI);
			if (options.HasSubset) {
				subset = options.Subset.Value;
				nOptions.fSubset = &subset;
			}
			return SkiaApi.sk_android_codec_get_android_pixels (Handle, &nInfo, (void*)pixels, (IntPtr)rowBytes, &nOptions);
		}

		// android pixels simplified

		public byte[] AndroidPixelsSimplified {
			get {
				var result = GetAndroidPixelsSimplified (out var pixels);
				if (result != SKCodecResult.Success && result != SKCodecResult.IncompleteInput) {
					throw new Exception (result.ToString ());
				}
				return pixels;
			}
		}

		public SKCodecResult GetAndroidPixelsSimplified (out byte[] pixels) =>
			GetAndroidPixelsSimplified (Info, out pixels);

		public SKCodecResult GetAndroidPixelsSimplified (SKImageInfo info, out byte[] pixels)
		{
			pixels = new byte[info.BytesSize];
			return GetAndroidPixelsSimplified (info, pixels);
		}

		public SKCodecResult GetAndroidPixelsSimplified (SKImageInfo info, byte[] pixels)
		{
			if (pixels == null)
				throw new ArgumentNullException (nameof (pixels));

			fixed (byte* p = pixels) {
				return GetAndroidPixelsSimplified (info, (IntPtr)p, info.RowBytes);
			}
		}

		public SKCodecResult GetAndroidPixelsSimplified (SKImageInfo info, IntPtr pixels) =>
			GetAndroidPixelsSimplified (info, pixels, info.RowBytes);

		public SKCodecResult GetAndroidPixelsSimplified (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			if (pixels == IntPtr.Zero)
				throw new ArgumentNullException (nameof (pixels));

			var nInfo = SKImageInfoNative.FromManaged (ref info);
			return SkiaApi.sk_android_codec_get_android_pixels_simplified (Handle, &nInfo, (void*)pixels, (IntPtr)rowBytes);
		}


		// pixels

		public byte[] Pixels
		{
			get
			{
				var result = GetPixels(out var pixels);
				if (result != SKCodecResult.Success && result != SKCodecResult.IncompleteInput)
				{
					throw new Exception(result.ToString());
				}
				return pixels;
			}
		}

		public SKCodecResult GetPixels(out byte[] pixels) =>
			GetPixels(Info, out pixels);

		public SKCodecResult GetPixels(SKImageInfo info, out byte[] pixels)
		{
			pixels = new byte[info.BytesSize];
			return GetPixels(info, pixels);
		}

		public SKCodecResult GetPixels(SKImageInfo info, byte[] pixels)
		{
			if (pixels == null)
				throw new ArgumentNullException(nameof(pixels));

			fixed (byte* p = pixels)
			{
				return GetPixels(info, (IntPtr)p, info.RowBytes);
			}
		}

		public SKCodecResult GetPixels(SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			if (pixels == IntPtr.Zero)
				throw new ArgumentNullException(nameof(pixels));

			var nInfo = SKImageInfoNative.FromManaged(ref info);
			return SkiaApi.sk_android_codec_get_pixels(Handle, &nInfo, (void*)pixels, (IntPtr)rowBytes);
		}

		// create (Codec)

		public static SKAndroidCodec Create(SKCodec codec)
		{
			return Create(codec, SKAndroidCodecExifOrientationBehavior.KIgnore);
		}

		public static SKAndroidCodec Create(SKCodec codec, SKAndroidCodecExifOrientationBehavior behavior)
		{
			if (codec == null)
				throw new ArgumentNullException(nameof(codec));
			var handle = SkiaApi.sk_android_codec_new_from_codec(codec.Handle, behavior);
			SKAndroidCodec c = GetObject(handle);
			codec.RevokeOwnership(c);
			return c;
		}

		// create (streams)

		public static SKAndroidCodec Create(string filename) =>
			Create(filename, null);

		public static SKAndroidCodec Create(string filename, SKPngChunkReader chunkReader)
		{
			if (filename == null)
				throw new ArgumentNullException (nameof(filename));

			var stream = SKFileStream.OpenStream(filename);
			if (stream == null)
			{
				return null;
			}

			return Create(stream, chunkReader);
		}


		public static SKAndroidCodec Create(SKStream stream) =>
			Create(stream, null);

		public static SKAndroidCodec Create(SKStream stream, SKPngChunkReader chunkReader)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));
			if (stream is SKFileStream filestream && !filestream.IsValid)
				throw new ArgumentException("File stream was not valid.", nameof(stream));

			var codec = GetObject(SkiaApi.sk_android_codec_new_from_stream(stream.Handle, chunkReader == null ? IntPtr.Zero : chunkReader.Handle));
			stream.RevokeOwnership(codec);
			return codec;
		}


		// create (data)

		public static SKAndroidCodec Create(SKData data)
		{
			return Create(data, null);
		}

		public static SKAndroidCodec Create(SKData data, SKPngChunkReader chunkReader)
		{
			if (data == null)
				throw new ArgumentNullException(nameof(data));

			return GetObject(SkiaApi.sk_android_codec_new_from_data(data.Handle, chunkReader == null ? IntPtr.Zero : chunkReader.Handle));
		}

		// utils

		internal static SKAndroidCodec GetObject(IntPtr handle) =>
			handle == IntPtr.Zero ? null : new SKAndroidCodec(handle, true);
	}
}
