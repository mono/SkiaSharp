//
// Bindings for SKImageDecoder
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2016 Xamarin Inc
//

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
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
				SKImageInfo info;
				SkiaApi.sk_codec_get_info (Handle, out info);
				return info;
			}
		}

		public SKCodecOrigin Origin {
			get { return SkiaApi.sk_codec_get_origin (Handle); }
		}

		public SKEncodedFormat EncodedFormat {
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

			GCHandle handle = default (GCHandle);
			try {
				handle = GCHandle.Alloc (pixels, GCHandleType.Pinned);
				return GetPixels (info, handle.AddrOfPinnedObject ());
			} finally {
				if (handle.IsAllocated) {
					handle.Free ();
				}
			}
		}

		public unsafe SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount)
		{
			if (pixels == IntPtr.Zero)
				throw new ArgumentNullException (nameof (pixels));

			var nativeOptions = new SKCodecOptionsInternal {
				fZeroInitialized = options.ZeroInitialized,
				fSubset = null
			};
			if (options.HasSubset) {
				var subset = options.Subset.Value;
				nativeOptions.fSubset = &subset;
			}
			return SkiaApi.sk_codec_get_pixels (Handle, ref info, pixels, (IntPtr)rowBytes, ref nativeOptions, colorTable, ref colorTableCount);
		}

		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount)
		{
			return GetPixels (info, pixels, info.RowBytes, options, colorTable, ref colorTableCount);
		}

		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, IntPtr colorTable, ref int colorTableCount)
		{
			return GetPixels (info, pixels, info.RowBytes, SKCodecOptions.Default, colorTable, ref colorTableCount);
		}

		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels)
		{
			int colorTableCount = 0;
			return GetPixels (info, pixels, info.RowBytes, SKCodecOptions.Default, IntPtr.Zero, ref colorTableCount);
		}

		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount)
		{
			return GetPixels (info, pixels, rowBytes, options, colorTable == null ? IntPtr.Zero : colorTable.ReadColors (), ref colorTableCount);
		}

		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount)
		{
			return GetPixels (info, pixels, info.RowBytes, options, colorTable, ref colorTableCount);
		}

		public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKColorTable colorTable, ref int colorTableCount)
		{
			return GetPixels (info, pixels, info.RowBytes, SKCodecOptions.Default, colorTable, ref colorTableCount);
		}

		public unsafe SKCodecResult StartIncrementalDecode(SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount)
		{
			if (pixels == IntPtr.Zero)
				throw new ArgumentNullException (nameof (pixels));

			var nativeOptions = new SKCodecOptionsInternal {
				fZeroInitialized = options.ZeroInitialized,
				fSubset = null
			};
			if (options.HasSubset) {
				var subset = options.Subset.Value;
				nativeOptions.fSubset = &subset;
			}
			return SkiaApi.sk_codec_start_incremental_decode (Handle, ref info, pixels, (IntPtr)rowBytes, ref nativeOptions, colorTable, ref colorTableCount);
		}

		public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options)
		{
			int colorTableCount = 0;
			return StartIncrementalDecode (info, pixels, rowBytes, options, IntPtr.Zero, ref colorTableCount);
		}

		public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			return SkiaApi.sk_codec_start_incremental_decode (Handle, ref info, pixels, (IntPtr)rowBytes, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
		}

		public unsafe SKCodecResult StartIncrementalDecode(SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount)
		{
			return StartIncrementalDecode (info, pixels, rowBytes, options, colorTable == null ? IntPtr.Zero : colorTable.ReadColors (), ref colorTableCount);
		}

		public SKCodecResult IncrementalDecode (out int rowsDecoded)
		{
			return SkiaApi.sk_codec_incremental_decode (Handle, out rowsDecoded);
		}

		public SKCodecResult IncrementalDecode ()
		{
			int rowsDecoded;
			return SkiaApi.sk_codec_incremental_decode (Handle, out rowsDecoded);
		}

		public static SKCodec Create (SKStream stream)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));
			var codec = GetObject<SKCodec> (SkiaApi.sk_codec_new_from_stream (stream.Handle));
			stream.RevokeOwnership (codec);
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
