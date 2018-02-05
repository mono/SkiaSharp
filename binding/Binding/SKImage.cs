//
// Bindings for SKImage
//
// Author:
//   Miguel de Icaza
//
// Copyright 2016 Xamarin Inc
//
using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	// public delegates
	public delegate void SKImageRasterReleaseDelegate (IntPtr pixels, object context);
	public delegate void SKImageTextureReleaseDelegate (object context);

	// internal proxy delegates
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void SKImageRasterReleaseDelegateInternal (IntPtr pixels, IntPtr context);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void SKImageTextureReleaseDelegateInternal (IntPtr context);

	// TODO: `FromTexture` with color space
	// TODO: `FromTexture` with `GRBackendTexture` [and color space]
	// TODO: `MakeCrossContextFromEncoded`
	// TODO: `FromAdoptedTexture` with color space
	// TODO: `FromAdoptedTexture` with `GRBackendTexture` [and color space]
	// TODO: `MakeFromYUVTexturesCopy` and `MakeFromNV12TexturesCopy`
	// TODO: `FromPicture` with bit depth and color space
	// TODO: `ColorSpace` property
	// TODO: `IsValid`
	// TODO: `GetTextureHandle`
	// TODO: `ToTextureImage`
	// TODO: `MakeColorSpace`

	public class SKImage : SKObject
	{
		// so the GC doesn't collect the delegate
		private static readonly SKImageRasterReleaseDelegateInternal rasterReleaseDelegateInternal;
		private static readonly SKImageTextureReleaseDelegateInternal textureReleaseDelegateInternal;
		private static readonly SKImageRasterReleaseDelegateInternal coTaskMemReleaseDelegateInternal;
		private static readonly IntPtr rasterReleaseDelegate;
		private static readonly IntPtr textureReleaseDelegate;
		private static readonly IntPtr coTaskMemReleaseDelegate;
		static SKImage()
		{
			rasterReleaseDelegateInternal = new SKImageRasterReleaseDelegateInternal (RasterReleaseInternal);
			textureReleaseDelegateInternal = new SKImageTextureReleaseDelegateInternal (TextureReleaseInternal);
			coTaskMemReleaseDelegateInternal = new SKImageRasterReleaseDelegateInternal (CoTaskMemReleaseInternal);

			rasterReleaseDelegate = Marshal.GetFunctionPointerForDelegate (rasterReleaseDelegateInternal);
			textureReleaseDelegate = Marshal.GetFunctionPointerForDelegate (textureReleaseDelegateInternal);
			coTaskMemReleaseDelegate = Marshal.GetFunctionPointerForDelegate (coTaskMemReleaseDelegateInternal);
		}

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

		public static SKImage Create (SKImageInfo info)
		{
			var pixels = Marshal.AllocCoTaskMem (info.BytesSize);
			using (var pixmap = new SKPixmap (info, pixels)) {
				// don't use the managed version as that is just extra overhead which isn't necessary
				return GetObject<SKImage> (SkiaApi.sk_image_new_raster (pixmap.Handle, coTaskMemReleaseDelegate, IntPtr.Zero));
			}
		}

		public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels)
		{
			return FromPixelCopy (info, pixels, info.RowBytes, null);
		}

		public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			return FromPixelCopy (info, pixels, rowBytes, null);
		}

		public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable)
		{
			if (pixels == IntPtr.Zero)
				throw new ArgumentNullException (nameof (pixels));

			var ct = (ctable == null ? IntPtr.Zero : ctable.Handle);
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			var handle = SkiaApi.sk_image_new_raster_copy_with_colortable (ref cinfo, pixels, (IntPtr) rowBytes, ct);
			return GetObject<SKImage> (handle);
		}

		public static SKImage FromPixelCopy (SKPixmap pixmap)
		{
			if (pixmap == null)
				throw new ArgumentNullException (nameof (pixmap));
			return GetObject<SKImage> (SkiaApi.sk_image_new_raster_copy_with_pixmap (pixmap.Handle));
		}

		public static SKImage FromPixelData (SKImageInfo info, SKData data, int rowBytes)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject<SKImage> (SkiaApi.sk_image_new_raster_data (ref cinfo, data.Handle, (IntPtr) rowBytes));	
		}

		public static SKImage FromPixels (SKImageInfo info, IntPtr pixels)
		{
			using (var pixmap = new SKPixmap (info, pixels, info.RowBytes))
			{
				return FromPixels (pixmap, null, null);
			}
		}

		public static SKImage FromPixels (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			using (var pixmap = new SKPixmap (info, pixels, rowBytes))
			{
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

			if (releaseProc == null) {
				return GetObject<SKImage> (SkiaApi.sk_image_new_raster (pixmap.Handle, IntPtr.Zero, IntPtr.Zero));
			} else {
				var ctx = new NativeDelegateContext (releaseContext, releaseProc);
				return GetObject<SKImage> (SkiaApi.sk_image_new_raster (pixmap.Handle, rasterReleaseDelegate, ctx.NativeContext));
			}
		}

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

		public static SKImage FromBitmap (SKBitmap bitmap)
		{
			if (bitmap == null)
				throw new ArgumentNullException (nameof (bitmap));
			var handle = SkiaApi.sk_image_new_from_bitmap (bitmap.Handle);
			return GetObject<SKImage> (handle);
		}

		public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc)
		{
			return FromTexture (context, desc, SKAlphaType.Premul);
		}

		public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha)
		{
			return FromTexture (context, desc, alpha, null);
		}

		public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc)
		{
			return FromTexture (context, desc, alpha, releaseProc, null);
		}

		public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc, object releaseContext)
		{
			unsafe {
				var h = desc.TextureHandle;
				var hPtr = &h;
				var d = new GRBackendTextureDesc {
					Flags = desc.Flags,
					Origin = desc.Origin,
					Width = desc.Width,
					Height = desc.Height,
					Config = desc.Config,
					SampleCount = desc.SampleCount,
					TextureHandle = (IntPtr)hPtr,
				};
				return FromTexture (context, d, alpha, releaseProc, releaseContext);
			}
		}

		public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc)
		{
			return FromTexture (context, desc, SKAlphaType.Premul);
		}

		public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha)
		{
			return FromTexture (context, desc, alpha, null);
		}

		public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc)
		{
			return FromTexture (context, desc, alpha, releaseProc, null);
		}

		public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc, object releaseContext)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			if (releaseProc == null) {
				return GetObject<SKImage> (SkiaApi.sk_image_new_from_texture (context.Handle, ref desc, alpha, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero));
			} else {
				var ctx = new NativeDelegateContext (releaseContext, releaseProc);
				return GetObject<SKImage> (SkiaApi.sk_image_new_from_texture (context.Handle, ref desc, alpha, IntPtr.Zero, textureReleaseDelegate, ctx.NativeContext));
			}
		}

		public static SKImage FromAdoptedTexture (GRContext context, GRGlBackendTextureDesc desc)
		{
			return FromAdoptedTexture (context, desc, SKAlphaType.Premul);
		}

		public static SKImage FromAdoptedTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha)
		{
			unsafe {
				var h = desc.TextureHandle;
				var hPtr = &h;
				var d = new GRBackendTextureDesc {
					Flags = desc.Flags,
					Origin = desc.Origin,
					Width = desc.Width,
					Height = desc.Height,
					Config = desc.Config,
					SampleCount = desc.SampleCount,
					TextureHandle = (IntPtr)hPtr,
				};
				return FromAdoptedTexture (context, d, alpha);
			}
		}
		
		public static SKImage FromAdoptedTexture (GRContext context, GRBackendTextureDesc desc)
		{
			return FromAdoptedTexture (context, desc, SKAlphaType.Premul);
		}

		public static SKImage FromAdoptedTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			return GetObject<SKImage> (SkiaApi.sk_image_new_from_adopted_texture (context.Handle, ref desc, alpha, IntPtr.Zero));
		}
		
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

		public SKData Encode (SKPixelSerializer serializer)
		{
			if (serializer == null)
				throw new ArgumentNullException (nameof (serializer));

			return GetObject<SKData> (SkiaApi.sk_image_encode_with_serializer (Handle, serializer.Handle));
		}

		public SKData Encode (SKEncodedImageFormat format, int quality)
		{
			return GetObject<SKData> (SkiaApi.sk_image_encode_specific (Handle, format, quality));
		}

		public int Width => SkiaApi.sk_image_get_width (Handle);
		public int Height => SkiaApi.sk_image_get_height (Handle); 
		public uint UniqueId => SkiaApi.sk_image_get_unique_id (Handle);
		public SKAlphaType AlphaType => SkiaApi.sk_image_get_alpha_type (Handle);
		public bool IsAlphaOnly => SkiaApi.sk_image_is_alpha_only(Handle);

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


		// internal proxies

		#if __IOS__
		[ObjCRuntime.MonoPInvokeCallback (typeof (SKImageRasterReleaseDelegateInternal))]
		#endif
		private static void RasterReleaseInternal (IntPtr pixels, IntPtr context)
		{
			using (var ctx = NativeDelegateContext.Unwrap (context)) {
				ctx.GetDelegate<SKImageRasterReleaseDelegate> () (pixels, ctx.ManagedContext);
			}
		}

		#if __IOS__
		[ObjCRuntime.MonoPInvokeCallback (typeof (SKImageRasterReleaseDelegateInternal))]
		#endif
		private static void CoTaskMemReleaseInternal (IntPtr pixels, IntPtr context)
		{
			Marshal.FreeCoTaskMem (pixels);
		}

		#if __IOS__
		[ObjCRuntime.MonoPInvokeCallback (typeof (SKImageTextureReleaseDelegateInternal))]
		#endif
		private static void TextureReleaseInternal (IntPtr context)
		{
			using (var ctx = NativeDelegateContext.Unwrap (context)) {
				ctx.GetDelegate<SKImageTextureReleaseDelegate> () (ctx.ManagedContext);
			}
		}
	}
}
