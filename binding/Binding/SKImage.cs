using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	// TODO: `MakeCrossContextFromEncoded`
	// TODO: `MakeFromYUVTexturesCopy` and `MakeFromNV12TexturesCopy`
	// TODO: `FromPicture` with bit depth and color space
	// TODO: `IsValid`
	// TODO: `GetTextureHandle`
	// TODO: `ToTextureImage`
	// TODO: `MakeColorSpace`

	public unsafe class SKImage : SKObject, ISKReferenceCounted
	{
		[Preserve]
		internal SKImage (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		// create brand new image

		public static SKImage Create (SKImageInfo info)
		{
			var pixels = Marshal.AllocCoTaskMem (info.BytesSize);
			using var pixmap = new SKPixmap (info, pixels);
			// don't use the managed version as that is just extra overhead which isn't necessary
			return GetObject<SKImage> (SkiaApi.sk_image_new_raster (pixmap.Handle, DelegateProxies.SKImageRasterReleaseDelegateProxyForCoTaskMem, null));
		}

		// create a new image from a copy of pixel data

		public static SKImage FromPixelCopy (SKImageInfo info, SKStream pixels) =>
			FromPixelCopy (info, pixels, info.RowBytes);

		public static SKImage FromPixelCopy (SKImageInfo info, SKStream pixels, int rowBytes)
		{
			if (pixels == null)
				throw new ArgumentNullException (nameof (pixels));

			using var data = SKData.Create (pixels);
			return FromPixels (info, data, rowBytes);
		}

		public static SKImage FromPixelCopy (SKImageInfo info, Stream pixels) =>
			FromPixelCopy (info, pixels, info.RowBytes);

		public static SKImage FromPixelCopy (SKImageInfo info, Stream pixels, int rowBytes)
		{
			if (pixels == null)
				throw new ArgumentNullException (nameof (pixels));

			using var data = SKData.Create (pixels);
			return FromPixels (info, data, rowBytes);
		}

		public static SKImage FromPixelCopy (SKImageInfo info, ReadOnlySpan<byte> pixels) =>
			FromPixelCopy (info, pixels, info.RowBytes);

		public static SKImage FromPixelCopy (SKImageInfo info, ReadOnlySpan<byte> pixels, int rowBytes)
		{
			using var data = SKData.CreateCopy (pixels);
			return FromPixels (info, data, rowBytes);
		}

		public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels) =>
			FromPixelCopy (info, pixels, info.RowBytes);

		public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			if (pixels == IntPtr.Zero)
				throw new ArgumentNullException (nameof (pixels));

			var nInfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject<SKImage> (SkiaApi.sk_image_new_raster_copy (&nInfo, (void*)pixels, (IntPtr)rowBytes));
		}

		public static SKImage FromPixelCopy (SKPixmap pixmap)
		{
			if (pixmap == null)
				throw new ArgumentNullException (nameof (pixmap));

			return GetObject<SKImage> (SkiaApi.sk_image_new_raster_copy_with_pixmap (pixmap.Handle));
		}

		// create a new image around existing pixel data

		public static SKImage FromPixels (SKImageInfo info, SKData data, int rowBytes)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject<SKImage> (SkiaApi.sk_image_new_raster_data (&cinfo, data.Handle, (IntPtr)rowBytes));
		}

		public static SKImage FromPixels (SKImageInfo info, IntPtr pixels)
		{
			using var pixmap = new SKPixmap (info, pixels, info.RowBytes);
			return FromPixels (pixmap);
		}

		public static SKImage FromPixels (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			using var pixmap = new SKPixmap (info, pixels, rowBytes);
			return FromPixels (pixmap);
		}

		public static SKImage FromPixels (SKPixmap pixmap, SKImageRasterReleaseDelegate releaseProc = null, object releaseContext = null)
		{
			if (pixmap == null)
				throw new ArgumentNullException (nameof (pixmap));

			var del = releaseProc != null && releaseContext != null
				? new SKImageRasterReleaseDelegate ((addr, _) => releaseProc (addr, releaseContext))
				: releaseProc;
			var proxy = DelegateProxies.Create (del, DelegateProxies.SKImageRasterReleaseDelegateProxy, out _, out var ctx);
			return GetObject<SKImage> (SkiaApi.sk_image_new_raster (pixmap.Handle, proxy, (void*)ctx));
		}

		// create a new image from encoded data

		public static SKImage FromEncodedData (SKData data, SKRectI subset)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			var handle = SkiaApi.sk_image_new_from_encoded (data.Handle, &subset);
			return GetObject<SKImage> (handle);
		}

		public static SKImage FromEncodedData (SKData data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			var handle = SkiaApi.sk_image_new_from_encoded (data.Handle, null);
			return GetObject<SKImage> (handle);
		}

		public static SKImage FromEncodedData (ReadOnlySpan<byte> data)
		{
			if (data.Length == 0)
				throw new ArgumentException ("The data buffer was empty.");

			using var skdata = SKData.CreateCopy (data);
			return FromEncodedData (skdata);
		}

		public static SKImage FromEncodedData (SKStream data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			using var skdata = SKData.Create (data);
			if (skdata == null)
				return null;
			return FromEncodedData (skdata);
		}

		public static SKImage FromEncodedData (Stream data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			using var skdata = SKData.Create (data);
			if (skdata == null)
				return null;
			return FromEncodedData (skdata);
		}

		public static SKImage FromEncodedData (string filename)
		{
			if (filename == null)
				throw new ArgumentNullException (nameof (filename));

			using var skdata = SKData.Create (filename);
			if (skdata == null)
				return null;
			return FromEncodedData (skdata);
		}

		// create a new image from a bitmap

		public static SKImage FromBitmap (SKBitmap bitmap)
		{
			if (bitmap == null)
				throw new ArgumentNullException (nameof (bitmap));

			var handle = SkiaApi.sk_image_new_from_bitmap (bitmap.Handle);
			return GetObject<SKImage> (handle);
		}

		// create a new image from a GPU texture

		public static SKImage FromTexture (GRContext context, GRBackendTexture texture, SKColorType colorType) =>
			FromTexture (context, texture, GRSurfaceOrigin.BottomLeft, colorType);

		public static SKImage FromTexture (
			GRContext context,
			GRBackendTexture texture,
			GRSurfaceOrigin origin,
			SKColorType colorType,
			SKAlphaType alpha = SKAlphaType.Premul,
			SKColorSpace colorspace = null,
			SKImageTextureReleaseDelegate releaseProc = null,
			object releaseContext = null)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			if (texture == null)
				throw new ArgumentNullException (nameof (texture));

			var cs = colorspace == null ? IntPtr.Zero : colorspace.Handle;
			var del = releaseProc != null && releaseContext != null
				? new SKImageTextureReleaseDelegate ((_) => releaseProc (releaseContext))
				: releaseProc;
			var proxy = DelegateProxies.Create (del, DelegateProxies.SKImageTextureReleaseDelegateProxy, out _, out var ctx);
			return GetObject<SKImage> (SkiaApi.sk_image_new_from_texture (context.Handle, texture.Handle, origin, colorType, alpha, cs, proxy, (void*)ctx));
		}

		public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, SKColorType colorType) =>
			FromAdoptedTexture (context, texture, GRSurfaceOrigin.BottomLeft, colorType);

		public static SKImage FromAdoptedTexture (
			GRContext context,
			GRBackendTexture texture,
			GRSurfaceOrigin origin,
			SKColorType colorType,
			SKAlphaType alpha = SKAlphaType.Premul,
			SKColorSpace colorspace = null)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			if (texture == null)
				throw new ArgumentNullException (nameof (texture));

			var cs = colorspace == null ? IntPtr.Zero : colorspace.Handle;
			return GetObject<SKImage> (SkiaApi.sk_image_new_from_adopted_texture (context.Handle, texture.Handle, origin, colorType, alpha, cs));
		}

		// create a new image from a picture

		public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions, SKPaint paint = null)
		{
			if (picture == null)
				throw new ArgumentNullException (nameof (picture));

			return GetObject<SKImage> (SkiaApi.sk_image_new_from_picture (picture.Handle, &dimensions, null, paint?.Handle ?? IntPtr.Zero));
		}

		public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions, SKMatrix matrix, SKPaint paint = null)
		{
			if (picture == null)
				throw new ArgumentNullException (nameof (picture));

			return GetObject<SKImage> (SkiaApi.sk_image_new_from_picture (picture.Handle, &dimensions, &matrix, paint?.Handle ?? IntPtr.Zero));
		}

		// Encode

		public SKData Encode () =>
			GetObject<SKData> (SkiaApi.sk_image_encode (Handle));

		public SKData Encode (SKEncodedImageFormat format, int quality) =>
			GetObject<SKData> (SkiaApi.sk_image_encode_specific (Handle, format, quality));

		// Properties

		public int Width => SkiaApi.sk_image_get_width (Handle);

		public int Height => SkiaApi.sk_image_get_height (Handle);

		public uint UniqueId => SkiaApi.sk_image_get_unique_id (Handle);

		public SKAlphaType AlphaType => SkiaApi.sk_image_get_alpha_type (Handle);

		public SKColorType ColorType => SkiaApi.sk_image_get_color_type (Handle);

		public SKColorSpace ColorSpace => GetObject<SKColorSpace> (SkiaApi.sk_image_get_colorspace (Handle));

		public bool IsAlphaOnly => SkiaApi.sk_image_is_alpha_only (Handle);

		public SKData EncodedData => GetObject<SKData> (SkiaApi.sk_image_ref_encoded (Handle));

		public bool IsTextureBacked => SkiaApi.sk_image_is_texture_backed (Handle);

		public bool IsLazyGenerated => SkiaApi.sk_image_is_lazy_generated (Handle);

		// ToShader

		public SKShader ToShader () =>
			ToShader (SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);

		public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY) =>
			GetObject<SKShader> (SkiaApi.sk_image_make_shader (Handle, tileX, tileY, null));

		public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKMatrix localMatrix) =>
			GetObject<SKShader> (SkiaApi.sk_image_make_shader (Handle, tileX, tileY, &localMatrix));

		// PeekPixels

		public bool PeekPixels (SKPixmap pixmap)
		{
			if (pixmap == null)
				throw new ArgumentNullException (nameof (pixmap));
			return SkiaApi.sk_image_peek_pixels (Handle, pixmap.Handle);
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

		// ReadPixels

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY, SKImageCachingHint cachingHint = SKImageCachingHint.Allow)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref dstInfo);
			return SkiaApi.sk_image_read_pixels (Handle, &cinfo, (void*)dstPixels, (IntPtr)dstRowBytes, srcX, srcY, cachingHint);
		}

		public bool ReadPixels (SKPixmap pixmap, int srcX, int srcY, SKImageCachingHint cachingHint = SKImageCachingHint.Allow)
		{
			if (pixmap == null)
				throw new ArgumentNullException (nameof (pixmap));

			return SkiaApi.sk_image_read_pixels_into_pixmap (Handle, pixmap.Handle, srcX, srcY, cachingHint);
		}

		// ScalePixels

		public bool ScalePixels (SKPixmap dst, SKFilterQuality quality, SKImageCachingHint cachingHint = SKImageCachingHint.Allow)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			return SkiaApi.sk_image_scale_pixels (Handle, dst.Handle, quality, cachingHint);
		}

		// Subset

		public SKImage Subset (SKRectI subset) =>
			GetObject<SKImage> (SkiaApi.sk_image_make_subset (Handle, &subset));

		// ToRasterImage

		public SKImage ToRasterImage () =>
			GetObject<SKImage> (SkiaApi.sk_image_make_non_texture_image (Handle));

		// ToBitmap

		public SKBitmap ToBitmap ()
		{
			var info = new SKImageInfo (Width, Height, SKImageInfo.PlatformColorType, AlphaType);
			var bmp = new SKBitmap (info);
			if (!ReadPixels (info, bmp.GetPixels (), info.RowBytes, 0, 0)) {
				bmp.Dispose ();
				bmp = null;
			}
			return bmp;
		}

		// ApplyImageFilter

		public SKImage ApplyImageFilter (SKImageFilter filter, SKRectI subset, SKRectI clipBounds)
		{
			if (filter == null)
				throw new ArgumentNullException (nameof (filter));

			return GetObject<SKImage> (SkiaApi.sk_image_make_with_filter (Handle, filter.Handle, &subset, &clipBounds, null, null));
		}

		public SKImage ApplyImageFilter (SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPointI outOffset)
		{
			if (filter == null)
				throw new ArgumentNullException (nameof (filter));

			fixed (SKRectI* os = &outSubset)
			fixed (SKPointI* oo = &outOffset) {
				return GetObject<SKImage> (SkiaApi.sk_image_make_with_filter (Handle, filter.Handle, &subset, &clipBounds, os, oo));
			}
		}
	}
}
