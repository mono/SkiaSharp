#nullable disable

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	// TODO: `MakeCrossContextFromEncoded`
	// TODO: `MakeFromYUVTexturesCopy` and `MakeFromNV12TexturesCopy`
	// TODO: `FromPicture` with bit depth and color space
	// TODO: `GetTextureHandle`
	// TODO: `MakeColorSpace`

	public unsafe class SKImage : SKObject, ISKReferenceCounted
	{
		internal SKImage (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		// create brand new image

		public static SKImage Create (SKImageInfo info)
		{
			var pixels = Marshal.AllocCoTaskMem (info.BytesSize);
			using (var pixmap = new SKPixmap (info, pixels)) {
				// don't use the managed version as that is just extra overhead which isn't necessary
				return GetObject (SkiaApi.sk_image_new_raster (pixmap.Handle, DelegateProxies.SKImageRasterReleaseProxyForCoTaskMem, null));
			}
		}

		// create a new image from a copy of pixel data

		public static SKImage FromPixelCopy (SKImageInfo info, SKStream pixels) =>
			FromPixelCopy (info, pixels, info.RowBytes);

		public static SKImage FromPixelCopy (SKImageInfo info, SKStream pixels, int rowBytes)
		{
			if (pixels == null)
				throw new ArgumentNullException (nameof (pixels));
			using (var data = SKData.Create (pixels)) {
				return FromPixels (info, data, rowBytes);
			}
		}

		public static SKImage FromPixelCopy (SKImageInfo info, Stream pixels) =>
			FromPixelCopy (info, pixels, info.RowBytes);

		public static SKImage FromPixelCopy (SKImageInfo info, Stream pixels, int rowBytes)
		{
			if (pixels == null)
				throw new ArgumentNullException (nameof (pixels));
			using (var data = SKData.Create (pixels)) {
				return FromPixels (info, data, rowBytes);
			}
		}

		public static SKImage FromPixelCopy (SKImageInfo info, byte[] pixels) =>
			FromPixelCopy (info, pixels.AsSpan ());

		public static SKImage FromPixelCopy (SKImageInfo info, byte[] pixels, int rowBytes) =>
			FromPixelCopy (info, pixels.AsSpan (), rowBytes);

		public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels) =>
			FromPixelCopy (info, pixels, info.RowBytes);

		public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			if (pixels == IntPtr.Zero)
				throw new ArgumentNullException (nameof (pixels));

			var nInfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject (SkiaApi.sk_image_new_raster_copy (&nInfo, (void*)pixels, (IntPtr)rowBytes));
		}

		public static SKImage FromPixelCopy (SKPixmap pixmap)
		{
			if (pixmap == null)
				throw new ArgumentNullException (nameof (pixmap));
			return GetObject (SkiaApi.sk_image_new_raster_copy_with_pixmap (pixmap.Handle));
		}

		public static SKImage FromPixelCopy (SKImageInfo info, ReadOnlySpan<byte> pixels) =>
			FromPixelCopy (info, pixels, info.RowBytes);

		public static SKImage FromPixelCopy (SKImageInfo info, ReadOnlySpan<byte> pixels, int rowBytes)
		{
			if (pixels == null)
				throw new ArgumentNullException (nameof (pixels));
			using (var data = SKData.CreateCopy (pixels)) {
				return FromPixels (info, data, rowBytes);
			}
		}

		// create a new image around existing pixel data

		public static SKImage FromPixels (SKImageInfo info, SKData data) =>
			FromPixels (info, data, info.RowBytes);

		public static SKImage FromPixels (SKImageInfo info, SKData data, int rowBytes)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject (SkiaApi.sk_image_new_raster_data (&cinfo, data.Handle, (IntPtr)rowBytes));
		}

		public static SKImage FromPixels (SKImageInfo info, IntPtr pixels)
		{
			using (var pixmap = new SKPixmap (info, pixels, info.RowBytes)) {
				return FromPixels (pixmap, null, null);
			}
		}

		public static SKImage FromPixels (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			using (var pixmap = new SKPixmap (info, pixels, rowBytes)) {
				return FromPixels (pixmap, null, null);
			}
		}

		public static SKImage FromPixels (SKPixmap pixmap)
		{
			return FromPixels (pixmap, null, null);
		}

		public static SKImage FromPixels (SKPixmap pixmap, SKImageRasterReleaseDelegate releaseProc)
		{
			return FromPixels (pixmap, releaseProc, null);
		}

		public static SKImage FromPixels (SKPixmap pixmap, SKImageRasterReleaseDelegate releaseProc, object releaseContext)
		{
			if (pixmap == null)
				throw new ArgumentNullException (nameof (pixmap));

			var del = releaseProc != null && releaseContext != null
				? new SKImageRasterReleaseDelegate ((addr, _) => releaseProc (addr, releaseContext))
				: releaseProc;
			DelegateProxies.Create (del, out _, out var ctx);
			var proxy = del is not null ? DelegateProxies.SKImageRasterReleaseProxy : null;
			return GetObject (SkiaApi.sk_image_new_raster (pixmap.Handle, proxy, (void*)ctx));
		}

		// create a new image from encoded data

		public static SKImage FromEncodedData (SKData data, SKRectI subset)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			return FromEncodedData (data)?.Subset (subset);
		}

		public static SKImage FromEncodedData (SKData data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			var handle = SkiaApi.sk_image_new_from_encoded (data.Handle);
			return GetObject (handle);
		}

		public static SKImage FromEncodedData (ReadOnlySpan<byte> data)
		{
			if (data.IsEmpty)
				throw new ArgumentException ("The data buffer was empty.");

			using (var skdata = SKData.CreateCopy (data)) {
				return FromEncodedData (skdata);
			}
		}

		public static SKImage FromEncodedData (byte[] data) =>
			FromEncodedData (data.AsSpan ());

		public static SKImage FromEncodedData (SKStream data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			using (var skdata = SKData.Create (data)) {
				if (skdata == null)
					return null;
				return FromEncodedData (skdata);
			}
		}

		public static SKImage FromEncodedData (Stream data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			using (var skdata = SKData.Create (data)) {
				if (skdata == null)
					return null;
				return FromEncodedData (skdata);
			}
		}

		public static SKImage FromEncodedData (string filename)
		{
			if (filename == null)
				throw new ArgumentNullException (nameof (filename));

			using (var skdata = SKData.Create (filename)) {
				if (skdata == null)
					return null;
				return FromEncodedData (skdata);
			}
		}

		// create a new image from a bitmap

		public static SKImage FromBitmap (SKBitmap bitmap)
		{
			if (bitmap == null)
				throw new ArgumentNullException (nameof (bitmap));

			var image = GetObject (SkiaApi.sk_image_new_from_bitmap (bitmap.Handle));
			GC.KeepAlive (bitmap);
			return image;
		}

		// create a new image from a GPU texture

		public static SKImage FromTexture (GRContext context, GRBackendTexture texture, SKColorType colorType) =>
			FromTexture ((GRRecordingContext)context, texture, colorType);

		public static SKImage FromTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType) =>
			FromTexture ((GRRecordingContext)context, texture, origin, colorType);

		public static SKImage FromTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha) =>
			FromTexture ((GRRecordingContext)context, texture, origin, colorType, alpha);

		public static SKImage FromTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace) =>
			FromTexture ((GRRecordingContext)context, texture, origin, colorType, alpha, colorspace);

		public static SKImage FromTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc) =>
			FromTexture ((GRRecordingContext)context, texture, origin, colorType, alpha, colorspace, releaseProc);

		public static SKImage FromTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc, object releaseContext) =>
			FromTexture ((GRRecordingContext)context, texture, origin, colorType, alpha, colorspace, releaseProc, releaseContext);

		public static SKImage FromTexture (GRRecordingContext context, GRBackendTexture texture, SKColorType colorType) =>
			FromTexture (context, texture, GRSurfaceOrigin.BottomLeft, colorType, SKAlphaType.Premul, null, null, null);

		public static SKImage FromTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType) =>
			FromTexture (context, texture, origin, colorType, SKAlphaType.Premul, null, null, null);

		public static SKImage FromTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha) =>
			FromTexture (context, texture, origin, colorType, alpha, null, null, null);

		public static SKImage FromTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace) =>
			FromTexture (context, texture, origin, colorType, alpha, colorspace, null, null);

		public static SKImage FromTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc) =>
			FromTexture (context, texture, origin, colorType, alpha, colorspace, releaseProc, null);

		public static SKImage FromTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc, object releaseContext)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			if (texture == null)
				throw new ArgumentNullException (nameof (texture));

			var cs = colorspace == null ? IntPtr.Zero : colorspace.Handle;
			var del = releaseProc != null && releaseContext != null
				? new SKImageTextureReleaseDelegate ((_) => releaseProc (releaseContext))
				: releaseProc;
			DelegateProxies.Create (del, out _, out var ctx);
			var proxy = del is not null ? DelegateProxies.SKImageTextureReleaseProxy : null;
			return GetObject (SkiaApi.sk_image_new_from_texture (context.Handle, texture.Handle, origin, colorType.ToNative (), alpha, cs, proxy, (void*)ctx));
		}

		public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, SKColorType colorType) =>
			FromAdoptedTexture ((GRRecordingContext)context, texture, colorType);

		public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType) =>
			FromAdoptedTexture ((GRRecordingContext)context, texture, origin, colorType);

		public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha) =>
			FromAdoptedTexture ((GRRecordingContext)context, texture, origin, colorType, alpha);

		public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace) =>
			FromAdoptedTexture ((GRRecordingContext)context, texture, origin, colorType, alpha, colorspace);

		public static SKImage FromAdoptedTexture (GRRecordingContext context, GRBackendTexture texture, SKColorType colorType) =>
			FromAdoptedTexture (context, texture, GRSurfaceOrigin.BottomLeft, colorType, SKAlphaType.Premul, null);

		public static SKImage FromAdoptedTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType) =>
			FromAdoptedTexture (context, texture, origin, colorType, SKAlphaType.Premul, null);

		public static SKImage FromAdoptedTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha) =>
			FromAdoptedTexture (context, texture, origin, colorType, alpha, null);

		public static SKImage FromAdoptedTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			if (texture == null)
				throw new ArgumentNullException (nameof (texture));

			var cs = colorspace == null ? IntPtr.Zero : colorspace.Handle;
			return GetObject (SkiaApi.sk_image_new_from_adopted_texture (context.Handle, texture.Handle, origin, colorType.ToNative (), alpha, cs));
		}

		// create a new image from a picture

		public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions) =>
			FromPicture (picture, dimensions, null, null, false, null, null);

		public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions, SKMatrix matrix) =>
			FromPicture (picture, dimensions, &matrix, null, false, null, null);

		public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions, SKPaint paint) =>
			FromPicture (picture, dimensions, null, paint, false, null, null);

		public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions, SKMatrix matrix, SKPaint paint) =>
			FromPicture (picture, dimensions, &matrix, paint, false, null, null);

		private static SKImage FromPicture (SKPicture picture, SKSizeI dimensions, SKMatrix* matrix, SKPaint paint, bool useFloatingPointBitDepth, SKColorSpace colorspace, SKSurfaceProperties props)
		{
			if (picture == null)
				throw new ArgumentNullException (nameof (picture));

			var p = paint?.Handle ?? IntPtr.Zero;
			return GetObject (SkiaApi.sk_image_new_from_picture (picture.Handle, &dimensions, matrix, p, useFloatingPointBitDepth, colorspace?.Handle ?? IntPtr.Zero, props?.Handle ?? IntPtr.Zero));
		}

		public SKData Encode ()
		{
			if (EncodedData is not null)
				return EncodedData;

			return Encode (SKEncodedImageFormat.Png, 100);
		}

		public SKData Encode (SKEncodedImageFormat format, int quality)
		{
			var raster = ToRasterImage (true);
			try {
				using var pixmap = raster.PeekPixels ();
				return pixmap?.Encode (format, quality);
			} finally {
				if (this != raster)
					raster.Dispose ();
			}
		}

		public int Width =>
			SkiaApi.sk_image_get_width (Handle);

		public int Height =>
			SkiaApi.sk_image_get_height (Handle);

		public uint UniqueId =>
			SkiaApi.sk_image_get_unique_id (Handle);

		public SKAlphaType AlphaType =>
			SkiaApi.sk_image_get_alpha_type (Handle);

		public SKColorType ColorType =>
			SkiaApi.sk_image_get_color_type (Handle).FromNative ();

		public SKColorSpace ColorSpace =>
			SKColorSpace.GetObject (SkiaApi.sk_image_get_colorspace (Handle));

		public bool IsAlphaOnly =>
			SkiaApi.sk_image_is_alpha_only (Handle);

		public SKData EncodedData =>
			SKData.GetObject (SkiaApi.sk_image_ref_encoded (Handle));

		public SKImageInfo Info =>
			new SKImageInfo (Width, Height, ColorType, AlphaType, ColorSpace);

		// ToShader

		public SKShader ToShader () =>
			ToShader (SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, SKSamplingOptions.Default, null);

		public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY) =>
			ToShader (tileX, tileY, SKSamplingOptions.Default, null);

		public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKMatrix localMatrix) =>
			ToShader (tileX, tileY, SKSamplingOptions.Default, &localMatrix);

		public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling) =>
			ToShader (tileX, tileY, sampling, null);

		[Obsolete ("Use ToShader(SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling) instead.")]
		public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKFilterQuality quality) =>
			ToShader (tileX, tileY, quality.ToSamplingOptions(), null);

		public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling, SKMatrix localMatrix) =>
			ToShader (tileX, tileY, sampling, &localMatrix);

		[Obsolete ("Use ToShader(SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling, SKMatrix localMatrix) instead.")]
		public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKFilterQuality quality, SKMatrix localMatrix) =>
			ToShader (tileX, tileY, quality.ToSamplingOptions(), &localMatrix);

		private SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling, SKMatrix* localMatrix) =>
			SKShader.GetObject (SkiaApi.sk_image_make_shader (Handle, tileX, tileY, &sampling, localMatrix));

		// ToRawShader

		public SKShader ToRawShader () =>
			ToRawShader (SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, SKSamplingOptions.Default, null);

		public SKShader ToRawShader (SKShaderTileMode tileX, SKShaderTileMode tileY) =>
			ToRawShader (tileX, tileY, SKSamplingOptions.Default, null);

		public SKShader ToRawShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKMatrix localMatrix) =>
			ToRawShader (tileX, tileY, SKSamplingOptions.Default, &localMatrix);

		public SKShader ToRawShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling) =>
			ToRawShader (tileX, tileY, sampling, null);

		public SKShader ToRawShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling, SKMatrix localMatrix) =>
			ToRawShader (tileX, tileY, sampling, &localMatrix);

		private SKShader ToRawShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling, SKMatrix* localMatrix) =>
			SKShader.GetObject (SkiaApi.sk_image_make_raw_shader (Handle, tileX, tileY, &sampling, localMatrix));

		// PeekPixels

		public bool PeekPixels (SKPixmap pixmap)
		{
			if (pixmap == null)
				throw new ArgumentNullException (nameof (pixmap));

			var result = SkiaApi.sk_image_peek_pixels (Handle, pixmap.Handle);
			if (result)
				pixmap.pixelSource = this;
			return result;
		}

		public SKPixmap PeekPixels ()
		{
			var pixmap = new SKPixmap ();
			if (!PeekPixels (pixmap)) {
				pixmap.Dispose ();
				pixmap = null;
			}
			return pixmap;
		}

		public bool IsTextureBacked =>
			SkiaApi.sk_image_is_texture_backed (Handle);

		public bool IsLazyGenerated =>
			SkiaApi.sk_image_is_lazy_generated (Handle);

		public bool IsValid (GRContext context) =>
			IsValid ((GRRecordingContext)context);

		public bool IsValid (GRRecordingContext context) =>
			SkiaApi.sk_image_is_valid (Handle, context?.Handle ?? IntPtr.Zero);

		// ReadPixels

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels) =>
			ReadPixels (dstInfo, dstPixels, dstInfo.RowBytes, 0, 0, SKImageCachingHint.Allow);

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes) =>
			ReadPixels (dstInfo, dstPixels, dstRowBytes, 0, 0, SKImageCachingHint.Allow);

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY) =>
			ReadPixels (dstInfo, dstPixels, dstRowBytes, srcX, srcY, SKImageCachingHint.Allow);

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY, SKImageCachingHint cachingHint)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref dstInfo);
			var result = SkiaApi.sk_image_read_pixels (Handle, &cinfo, (void*)dstPixels, (IntPtr)dstRowBytes, srcX, srcY, cachingHint);
			GC.KeepAlive (this);
			return result;
		}

		public bool ReadPixels (SKPixmap pixmap) =>
			ReadPixels (pixmap, 0, 0, SKImageCachingHint.Allow);

		public bool ReadPixels (SKPixmap pixmap, int srcX, int srcY) =>
			ReadPixels (pixmap, srcX, srcY, SKImageCachingHint.Allow);

		public bool ReadPixels (SKPixmap pixmap, int srcX, int srcY, SKImageCachingHint cachingHint)
		{
			if (pixmap == null)
				throw new ArgumentNullException (nameof (pixmap));

			var result = SkiaApi.sk_image_read_pixels_into_pixmap (Handle, pixmap.Handle, srcX, srcY, cachingHint);
			GC.KeepAlive (this);
			return result;
		}

		// ScalePixels

		[Obsolete("Use ScalePixels(SKPixmap dst, SKSamplingOptions sampling) instead.")]
		public bool ScalePixels (SKPixmap dst, SKFilterQuality quality) =>
			ScalePixels (dst, quality.ToSamplingOptions ());

		[Obsolete("Use ScalePixels(SKPixmap dst, SKSamplingOptions sampling, SKImageCachingHint cachingHint) instead.")]
		public bool ScalePixels (SKPixmap dst, SKFilterQuality quality, SKImageCachingHint cachingHint) =>
			ScalePixels (dst, quality.ToSamplingOptions (), cachingHint);

		public bool ScalePixels (SKPixmap dst, SKSamplingOptions sampling)
		{
			return ScalePixels (dst, sampling, SKImageCachingHint.Allow);
		}

		public bool ScalePixels (SKPixmap dst, SKSamplingOptions sampling, SKImageCachingHint cachingHint)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			return SkiaApi.sk_image_scale_pixels (Handle, dst.Handle, &sampling, cachingHint);
		}

		// Subset

		public SKImage Subset (SKRectI subset)
		{
			return GetObject (SkiaApi.sk_image_make_subset_raster (Handle, &subset));
		}

		public SKImage Subset (GRRecordingContext context, SKRectI subset)
		{
			return GetObject (SkiaApi.sk_image_make_subset (Handle, context?.Handle ?? IntPtr.Zero, &subset));
		}

		// ToRasterImage

		public SKImage ToRasterImage () =>
			ToRasterImage (false);

		public SKImage ToRasterImage (bool ensurePixelData) =>
			ensurePixelData
				? GetObject (SkiaApi.sk_image_make_raster_image (Handle))
				: GetObject (SkiaApi.sk_image_make_non_texture_image (Handle));

		// ToTextureImage

		public SKImage ToTextureImage (GRContext context) =>
			ToTextureImage (context, false, true);

		public SKImage ToTextureImage (GRContext context, bool mipmapped) =>
			ToTextureImage (context, mipmapped, true);

		public SKImage ToTextureImage (GRContext context, bool mipmapped, bool budgeted)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			return GetObject (SkiaApi.sk_image_make_texture_image (Handle, context.Handle, mipmapped, budgeted));
		}

		// ApplyImageFilter

		public SKImage ApplyImageFilter (SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPoint outOffset)
		{
			var image = ApplyImageFilter (filter, subset, clipBounds, out outSubset, out SKPointI outOffsetActual);
			outOffset = outOffsetActual;
			return image;
		}

		public SKImage ApplyImageFilter (SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPointI outOffset)
		{
			if (filter == null)
				throw new ArgumentNullException (nameof (filter));

			fixed (SKRectI* os = &outSubset)
			fixed (SKPointI* oo = &outOffset) {
				return GetObject (SkiaApi.sk_image_make_with_filter_raster (Handle, filter.Handle, &subset, &clipBounds, os, oo));
			}
		}

		public SKImage ApplyImageFilter (GRContext context, SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPointI outOffset) =>
			ApplyImageFilter ((GRRecordingContext)context, filter, subset, clipBounds, out outSubset, out outOffset);

		public SKImage ApplyImageFilter (GRRecordingContext context, SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPointI outOffset)
		{
			if (filter == null)
				throw new ArgumentNullException (nameof (filter));

			fixed (SKRectI* os = &outSubset)
			fixed (SKPointI* oo = &outOffset) {
				return GetObject (SkiaApi.sk_image_make_with_filter (Handle, context?.Handle ?? IntPtr.Zero, filter.Handle, &subset, &clipBounds, os, oo));
			}
		}

		internal static SKImage GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKImage (h, o));
	}
}
