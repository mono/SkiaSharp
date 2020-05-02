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
				return GetObject (SkiaApi.sk_image_new_raster (pixmap.Handle, DelegateProxies.SKImageRasterReleaseDelegateProxyForCoTaskMem, null));
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
			FromPixelCopy (info, pixels, info.RowBytes);

		public static SKImage FromPixelCopy (SKImageInfo info, byte[] pixels, int rowBytes)
		{
			if (pixels == null)
				throw new ArgumentNullException (nameof (pixels));
			using (var data = SKData.CreateCopy (pixels)) {
				return FromPixels (info, data, rowBytes);
			}
		}

		public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels) =>
			FromPixelCopy (info, pixels, info.RowBytes);

		public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			if (pixels == IntPtr.Zero)
				throw new ArgumentNullException (nameof (pixels));

			var nInfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject (SkiaApi.sk_image_new_raster_copy (&nInfo, (void*)pixels, (IntPtr)rowBytes));
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("The Index8 color type and color table is no longer supported. Use FromPixelCopy(SKImageInfo, IntPtr, int) instead.")]
		public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable) =>
			FromPixelCopy (info, pixels, rowBytes);

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

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use FromPixels (SKImageInfo, SKData, int) instead.")]
		public static SKImage FromPixelData (SKImageInfo info, SKData data, int rowBytes)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject (SkiaApi.sk_image_new_raster_data (&cinfo, data.Handle, (IntPtr)rowBytes));
		}

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
			var proxy = DelegateProxies.Create (del, DelegateProxies.SKImageRasterReleaseDelegateProxy, out _, out var ctx);
			return GetObject (SkiaApi.sk_image_new_raster (pixmap.Handle, proxy, (void*)ctx));
		}

		// create a new image from encoded data

		public static SKImage FromEncodedData (SKData data, SKRectI subset)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			var handle = SkiaApi.sk_image_new_from_encoded (data.Handle, &subset);
			return GetObject (handle);
		}

		public static SKImage FromEncodedData (SKData data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			var handle = SkiaApi.sk_image_new_from_encoded (data.Handle, null);
			return GetObject (handle);
		}

		public static SKImage FromEncodedData (ReadOnlySpan<byte> data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));
			if (data.Length == 0)
				throw new ArgumentException ("The data buffer was empty.");

			using (var skdata = SKData.CreateCopy (data)) {
				return FromEncodedData (skdata);
			}
		}

		public static SKImage FromEncodedData (byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));
			if (data.Length == 0)
				throw new ArgumentException ("The data buffer was empty.");

			using (var skdata = SKData.CreateCopy (data)) {
				return FromEncodedData (skdata);
			}
		}

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

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
		public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc)
		{
			return FromTexture (context, desc, SKAlphaType.Premul, null, null);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType) instead.")]
		public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha)
		{
			return FromTexture (context, desc, alpha, null, null);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType, SKColorSpace, SKImageTextureReleaseDelegate) instead.")]
		public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc)
		{
			return FromTexture (context, desc, alpha, releaseProc, null);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType, SKColorSpace, SKImageTextureReleaseDelegate, object) instead.")]
		public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc, object releaseContext)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			var texture = new GRBackendTexture (desc);
			return FromTexture (context, texture, desc.Origin, desc.Config.ToColorType (), alpha, null, releaseProc, releaseContext);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
		public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc)
		{
			return FromTexture (context, desc, SKAlphaType.Premul, null, null);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType) instead.")]
		public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha)
		{
			return FromTexture (context, desc, alpha, null, null);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType, SKColorSpace, SKImageTextureReleaseDelegate) instead.")]
		public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc)
		{
			return FromTexture (context, desc, alpha, releaseProc, null);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType, SKColorSpace, SKImageTextureReleaseDelegate, object) instead.")]
		public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc, object releaseContext)
		{
			var texture = new GRBackendTexture (desc);
			return FromTexture (context, texture, desc.Origin, desc.Config.ToColorType (), alpha, null, releaseProc, releaseContext);
		}

		public static SKImage FromTexture (GRContext context, GRBackendTexture texture, SKColorType colorType)
		{
			return FromTexture (context, texture, GRSurfaceOrigin.BottomLeft, colorType, SKAlphaType.Premul, null, null, null);
		}

		public static SKImage FromTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType)
		{
			return FromTexture (context, texture, origin, colorType, SKAlphaType.Premul, null, null, null);
		}

		public static SKImage FromTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha)
		{
			return FromTexture (context, texture, origin, colorType, alpha, null, null, null);
		}

		public static SKImage FromTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace)
		{
			return FromTexture (context, texture, origin, colorType, alpha, colorspace, null, null);
		}

		public static SKImage FromTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc)
		{
			return FromTexture (context, texture, origin, colorType, alpha, colorspace, releaseProc, null);
		}

		public static SKImage FromTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc, object releaseContext)
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
			return GetObject (SkiaApi.sk_image_new_from_texture (context.Handle, texture.Handle, origin, colorType, alpha, cs, proxy, (void*)ctx));
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use FromAdoptedTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
		public static SKImage FromAdoptedTexture (GRContext context, GRBackendTextureDesc desc)
		{
			return FromAdoptedTexture (context, desc, SKAlphaType.Premul);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use FromAdoptedTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType) instead.")]
		public static SKImage FromAdoptedTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha)
		{
			var texture = new GRBackendTexture (desc);
			return FromAdoptedTexture (context, texture, desc.Origin, desc.Config.ToColorType (), alpha, null);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use FromAdoptedTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
		public static SKImage FromAdoptedTexture (GRContext context, GRGlBackendTextureDesc desc)
		{
			return FromAdoptedTexture (context, desc, SKAlphaType.Premul);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use FromAdoptedTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType) instead.")]
		public static SKImage FromAdoptedTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha)
		{
			var texture = new GRBackendTexture (desc);
			return FromAdoptedTexture (context, texture, desc.Origin, desc.Config.ToColorType (), alpha, null);
		}

		public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, SKColorType colorType)
		{
			return FromAdoptedTexture (context, texture, GRSurfaceOrigin.BottomLeft, colorType, SKAlphaType.Premul, null);
		}

		public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType)
		{
			return FromAdoptedTexture (context, texture, origin, colorType, SKAlphaType.Premul, null);
		}

		public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha)
		{
			return FromAdoptedTexture (context, texture, origin, colorType, alpha, null);
		}

		public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			if (texture == null)
				throw new ArgumentNullException (nameof (texture));

			var cs = colorspace == null ? IntPtr.Zero : colorspace.Handle;
			return GetObject (SkiaApi.sk_image_new_from_adopted_texture (context.Handle, texture.Handle, origin, colorType, alpha, cs));
		}

		// create a new image from a picture

		public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions) =>
			FromPicture (picture, dimensions, null, null);

		public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions, SKMatrix matrix) =>
			FromPicture (picture, dimensions, &matrix, null);

		public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions, SKPaint paint) =>
			FromPicture (picture, dimensions, null, paint);

		public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions, SKMatrix matrix, SKPaint paint) =>
			FromPicture (picture, dimensions, &matrix, paint);

		private static SKImage FromPicture (SKPicture picture, SKSizeI dimensions, SKMatrix* matrix, SKPaint paint)
		{
			if (picture == null)
				throw new ArgumentNullException (nameof (picture));

			var p = paint?.Handle ?? IntPtr.Zero;
			return GetObject (SkiaApi.sk_image_new_from_picture (picture.Handle, &dimensions, matrix, p));
		}

		public SKData Encode () =>
			SKData.GetObject (SkiaApi.sk_image_encode (Handle));

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public SKData Encode (SKPixelSerializer serializer)
		{
			if (serializer == null)
				throw new ArgumentNullException (nameof (serializer));

			// try old data
			var encoded = EncodedData;
			if (encoded != null) {
				if (serializer.UseEncodedData (encoded.Data, (ulong)encoded.Size)) {
					return encoded;
				} else {
					encoded.Dispose ();
					encoded = null;
				}
			}

			// get new data (raster)
			if (!IsTextureBacked) {
				using (var pixmap = PeekPixels ()) {
					return serializer.Encode (pixmap);
				}
			}

			// get new data (texture / gpu)
			// this involves a copy from gpu to cpu first
			if (IsTextureBacked) {
				var info = new SKImageInfo (Width, Height, ColorType, AlphaType, ColorSpace);
				using (var temp = new SKBitmap (info))
				using (var pixmap = temp.PeekPixels ()) {
					if (pixmap != null && ReadPixels (pixmap, 0, 0)) {
						return serializer.Encode (pixmap);
					}
				}
			}

			// some error
			return null;
		}

		public SKData Encode (SKEncodedImageFormat format, int quality)
		{
			return SKData.GetObject (SkiaApi.sk_image_encode_specific (Handle, format, quality));
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
			SkiaApi.sk_image_get_color_type (Handle);

		public SKColorSpace ColorSpace =>
			SKColorSpace.GetObject (SkiaApi.sk_image_get_colorspace (Handle));

		public bool IsAlphaOnly =>
			SkiaApi.sk_image_is_alpha_only (Handle);

		public SKData EncodedData =>
			SKData.GetObject (SkiaApi.sk_image_ref_encoded (Handle));

		// ToShader

		public SKShader ToShader () =>
			ToShader (SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);

		public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY) =>
			SKShader.GetObject (SkiaApi.sk_image_make_shader (Handle, tileX, tileY, null));

		public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKMatrix localMatrix) =>
			SKShader.GetObject (SkiaApi.sk_image_make_shader (Handle, tileX, tileY, &localMatrix));

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

		public bool ScalePixels (SKPixmap dst, SKFilterQuality quality)
		{
			return ScalePixels (dst, quality, SKImageCachingHint.Allow);
		}

		public bool ScalePixels (SKPixmap dst, SKFilterQuality quality, SKImageCachingHint cachingHint)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			return SkiaApi.sk_image_scale_pixels (Handle, dst.Handle, quality, cachingHint);
		}

		// Subset

		public SKImage Subset (SKRectI subset)
		{
			return GetObject (SkiaApi.sk_image_make_subset (Handle, &subset));
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
			ToTextureImage (context, null);

		public SKImage ToTextureImage (GRContext context, SKColorSpace colorspace)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			return GetObject (SkiaApi.sk_image_make_texture_image (Handle, context.Handle, colorspace?.Handle ?? IntPtr.Zero));
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
				return GetObject (SkiaApi.sk_image_make_with_filter (Handle, filter.Handle, &subset, &clipBounds, os, oo));
			}
		}

		internal static SKImage GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKImage (h, o));
	}
}
