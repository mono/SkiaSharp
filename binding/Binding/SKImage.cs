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

	public class SKImage : SKObject
	{
		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_image_unref (Handle);
			}

			base.Dispose (disposing);
		}

		[Preserve]
		internal SKImage (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		// create brand new image

		public static SKImage Create (SKImageInfo info)
		{
			var pixels = Marshal.AllocCoTaskMem (info.BytesSize);
			using (var pixmap = new SKPixmap (info, pixels)) {
				// don't use the managed version as that is just extra overhead which isn't necessary
				return GetObject<SKImage> (SkiaApi.sk_image_new_raster (pixmap.Handle, DelegateProxies.SKImageRasterReleaseDelegateProxyForCoTaskMem, IntPtr.Zero));
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
				return FromPixelData (info, data, rowBytes);
			}
		}

		public static SKImage FromPixelCopy (SKImageInfo info, Stream pixels) =>
			FromPixelCopy (info, pixels, info.RowBytes);

		public static SKImage FromPixelCopy (SKImageInfo info, Stream pixels, int rowBytes)
		{
			if (pixels == null)
				throw new ArgumentNullException (nameof (pixels));
			using (var data = SKData.Create (pixels)) {
				return FromPixelData (info, data, rowBytes);
			}
		}

		public static SKImage FromPixelCopy (SKImageInfo info, byte[] pixels) =>
			FromPixelCopy (info, pixels, info.RowBytes);

		public static SKImage FromPixelCopy (SKImageInfo info, byte[] pixels, int rowBytes)
		{
			if (pixels == null)
				throw new ArgumentNullException (nameof (pixels));
			using (var data = SKData.CreateCopy (pixels)) {
				return FromPixelData (info, data, rowBytes);
			}
		}

		public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels) =>
			FromPixelCopy (info, pixels, info.RowBytes);

		public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			if (pixels == IntPtr.Zero)
				throw new ArgumentNullException (nameof (pixels));

			var nInfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject<SKImage> (SkiaApi.sk_image_new_raster_copy (ref nInfo, pixels, (IntPtr)rowBytes));
		}

		[Obsolete ("The Index8 color type and color table is no longer supported. Use FromPixelCopy(SKImageInfo, IntPtr, int) instead.")]
		public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable) =>
			FromPixelCopy (info, pixels, rowBytes);

		public static SKImage FromPixelCopy (SKPixmap pixmap)
		{
			if (pixmap == null)
				throw new ArgumentNullException (nameof (pixmap));
			return GetObject<SKImage> (SkiaApi.sk_image_new_raster_copy_with_pixmap (pixmap.Handle));
		}

		public static SKImage FromPixelCopy (SKImageInfo info, ReadOnlySpan<byte> pixels) =>
			FromPixelCopy (info, pixels, info.RowBytes);

		public static SKImage FromPixelCopy (SKImageInfo info, ReadOnlySpan<byte> pixels, int rowBytes)
		{
			if (pixels == null)
				throw new ArgumentNullException (nameof (pixels));
			using (var data = SKData.CreateCopy (pixels)) {
				return FromPixelData (info, data, rowBytes);
			}
		}

		// create a new image around existing pixel data

		public static SKImage FromPixelData (SKImageInfo info, SKData data, int rowBytes)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject<SKImage> (SkiaApi.sk_image_new_raster_data (ref cinfo, data.Handle, (IntPtr)rowBytes));
		}

		public static SKImage FromPixels (SKImageInfo info, SKData data, int rowBytes)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject<SKImage> (SkiaApi.sk_image_new_raster_data (ref cinfo, data.Handle, (IntPtr)rowBytes));
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
			return GetObject<SKImage> (SkiaApi.sk_image_new_raster (pixmap.Handle, proxy, ctx));
		}

		// create a new image from encoded data

		public static SKImage FromEncodedData (SKData data, SKRectI subset)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			var handle = SkiaApi.sk_image_new_from_encoded (data.Handle, ref subset);
			return GetObject<SKImage> (handle);
		}

		public static SKImage FromEncodedData (SKData data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			var handle = SkiaApi.sk_image_new_from_encoded (data.Handle, IntPtr.Zero);
			return GetObject<SKImage> (handle);
		}

		public static SKImage FromEncodedData (ReadOnlySpan<byte> data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));
			if (data.Length == 0)
				throw new ArgumentException ("The data buffer was empty.");

			unsafe {
				fixed (byte* b = data) {
					using (var skdata = SKData.Create ((IntPtr)b, data.Length)) {
						return FromEncodedData (skdata);
					}
				}
			}
		}

		public static SKImage FromEncodedData (byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));
			if (data.Length == 0)
				throw new ArgumentException ("The data buffer was empty.");

			unsafe {
				fixed (byte* b = data) {
					using (var skdata = SKData.Create ((IntPtr)b, data.Length)) {
						return FromEncodedData (skdata);
					}
				}
			}
		}

		public static SKImage FromEncodedData (SKStream data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			using (var codec = SKCodec.Create (data)) {
				if (codec == null)
					return null;

				var bitmap = SKBitmap.Decode (codec, codec.Info);
				if (bitmap == null)
					return null;

				bitmap.SetImmutable ();
				return FromPixels (bitmap.PeekPixels (), delegate {
					bitmap.Dispose ();
					bitmap = null;
				});
			}
		}

		public static SKImage FromEncodedData (Stream data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			using (var codec = SKCodec.Create (data)) {
				if (codec == null)
					return null;

				var bitmap = SKBitmap.Decode (codec, codec.Info);
				if (bitmap == null)
					return null;

				bitmap.SetImmutable ();
				return FromPixels (bitmap.PeekPixels (), delegate {
					bitmap.Dispose ();
					bitmap = null;
				});
			}
		}

		public static SKImage FromEncodedData (string filename)
		{
			if (filename == null)
				throw new ArgumentNullException (nameof (filename));

			using (var codec = SKCodec.Create (filename)) {
				if (codec == null)
					return null;

				var bitmap = SKBitmap.Decode (codec, codec.Info);
				if (bitmap == null)
					return null;

				bitmap.SetImmutable ();
				return FromPixels (bitmap.PeekPixels (), delegate {
					bitmap.Dispose ();
					bitmap = null;
				});
			}
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

		[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
		public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc)
		{
			return FromTexture (context, desc, SKAlphaType.Premul, null, null);
		}

		[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType) instead.")]
		public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha)
		{
			return FromTexture (context, desc, alpha, null, null);
		}

		[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType, SKColorSpace, SKImageTextureReleaseDelegate) instead.")]
		public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc)
		{
			return FromTexture (context, desc, alpha, releaseProc, null);
		}

		[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType, SKColorSpace, SKImageTextureReleaseDelegate, object) instead.")]
		public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc, object releaseContext)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			var texture = new GRBackendTexture (desc);
			return FromTexture (context, texture, desc.Origin, desc.Config.ToColorType (), alpha, null, releaseProc, releaseContext);
		}

		[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
		public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc)
		{
			return FromTexture (context, desc, SKAlphaType.Premul, null, null);
		}

		[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType) instead.")]
		public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha)
		{
			return FromTexture (context, desc, alpha, null, null);
		}

		[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType, SKColorSpace, SKImageTextureReleaseDelegate) instead.")]
		public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc)
		{
			return FromTexture (context, desc, alpha, releaseProc, null);
		}

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
			return GetObject<SKImage> (SkiaApi.sk_image_new_from_texture (context.Handle, texture.Handle, origin, colorType, alpha, cs, proxy, ctx));
		}

		[Obsolete ("Use FromAdoptedTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
		public static SKImage FromAdoptedTexture (GRContext context, GRBackendTextureDesc desc)
		{
			return FromAdoptedTexture (context, desc, SKAlphaType.Premul);
		}

		[Obsolete ("Use FromAdoptedTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType) instead.")]
		public static SKImage FromAdoptedTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha)
		{
			var texture = new GRBackendTexture (desc);
			return FromAdoptedTexture (context, texture, desc.Origin, desc.Config.ToColorType (), alpha, null);
		}

		[Obsolete ("Use FromAdoptedTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
		public static SKImage FromAdoptedTexture (GRContext context, GRGlBackendTextureDesc desc)
		{
			return FromAdoptedTexture (context, desc, SKAlphaType.Premul);
		}

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
			return GetObject<SKImage> (SkiaApi.sk_image_new_from_adopted_texture (context.Handle, texture.Handle, origin, colorType, alpha, cs));
		}

		// create a new image from a picture

		public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions)
		{
			return FromPicture (picture, dimensions, null);
		}

		public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions, SKMatrix matrix)
		{
			return FromPicture (picture, dimensions, matrix, null);
		}

		public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions, SKPaint paint)
		{
			if (picture == null)
				throw new ArgumentNullException (nameof (picture));

			var p = (paint == null ? IntPtr.Zero : paint.Handle);
			return GetObject<SKImage> (SkiaApi.sk_image_new_from_picture (picture.Handle, ref dimensions, IntPtr.Zero, p));
		}

		public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions, SKMatrix matrix, SKPaint paint)
		{
			if (picture == null)
				throw new ArgumentNullException (nameof (picture));

			var p = (paint == null ? IntPtr.Zero : paint.Handle);
			return GetObject<SKImage> (SkiaApi.sk_image_new_from_picture (picture.Handle, ref dimensions, ref matrix, p));
		}

		public SKData Encode ()
		{
			return GetObject<SKData> (SkiaApi.sk_image_encode (Handle));
		}

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
			return GetObject<SKData> (SkiaApi.sk_image_encode_specific (Handle, format, quality));
		}

		public int Width => SkiaApi.sk_image_get_width (Handle);
		public int Height => SkiaApi.sk_image_get_height (Handle);
		public uint UniqueId => SkiaApi.sk_image_get_unique_id (Handle);
		public SKAlphaType AlphaType => SkiaApi.sk_image_get_alpha_type (Handle);
		public SKColorType ColorType => SkiaApi.sk_image_get_color_type (Handle);
		public SKColorSpace ColorSpace => GetObject<SKColorSpace> (SkiaApi.sk_image_get_colorspace (Handle));
		public bool IsAlphaOnly => SkiaApi.sk_image_is_alpha_only (Handle);
		public SKData EncodedData => GetObject<SKData> (SkiaApi.sk_image_ref_encoded (Handle));

		public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY)
		{
			return GetObject<SKShader> (SkiaApi.sk_image_make_shader (Handle, tileX, tileY, IntPtr.Zero));
		}

		public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKMatrix localMatrix)
		{
			return GetObject<SKShader> (SkiaApi.sk_image_make_shader (Handle, tileX, tileY, ref localMatrix));
		}

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

		public bool IsTextureBacked => SkiaApi.sk_image_is_texture_backed (Handle);

		public bool IsLazyGenerated => SkiaApi.sk_image_is_lazy_generated (Handle);

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
		{
			return ReadPixels (dstInfo, dstPixels, dstRowBytes, srcX, srcY, SKImageCachingHint.Allow);
		}

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY, SKImageCachingHint cachingHint)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref dstInfo);
			return SkiaApi.sk_image_read_pixels (Handle, ref cinfo, dstPixels, (IntPtr)dstRowBytes, srcX, srcY, cachingHint);
		}

		public bool ReadPixels (SKPixmap pixmap, int srcX, int srcY)
		{
			return ReadPixels (pixmap, srcX, srcY, SKImageCachingHint.Allow);
		}

		public bool ReadPixels (SKPixmap pixmap, int srcX, int srcY, SKImageCachingHint cachingHint)
		{
			if (pixmap == null)
				throw new ArgumentNullException (nameof (pixmap));
			return SkiaApi.sk_image_read_pixels_into_pixmap (Handle, pixmap.Handle, srcX, srcY, cachingHint);
		}

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

		public SKImage Subset (SKRectI subset)
		{
			return GetObject<SKImage> (SkiaApi.sk_image_make_subset (Handle, ref subset));
		}

		public SKImage ToRasterImage ()
		{
			return GetObject<SKImage> (SkiaApi.sk_image_make_non_texture_image (Handle));
		}

		public SKImage ApplyImageFilter (SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPoint outOffset)
		{
			if (filter == null)
				throw new ArgumentNullException (nameof (filter));
			return GetObject<SKImage> (SkiaApi.sk_image_make_with_filter (Handle, filter.Handle, ref subset, ref clipBounds, out outSubset, out outOffset));
		}
	}
}
