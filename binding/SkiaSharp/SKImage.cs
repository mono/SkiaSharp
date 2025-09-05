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

	/// <summary>
	/// An abstraction for drawing a rectangle of pixels.
	/// </summary>
	/// <remarks><para>An image is an abstraction of pixels, though the particular type of image could be actually storing its data on the GPU, or as drawing commands (picture or PDF or otherwise), ready to be played back into another canvas.</para><para></para><para>The content of an image is always immutable, though the actual storage may change, if for example that image can be recreated via encoded data or other means.</para><para></para><para>An image always has a non-zero dimensions. If there is a request to create a new image, either directly or via a surface, and either of the requested dimensions are zero, then <see langword="null" /> will be returned.</para></remarks>
	public unsafe class SKImage : SKObject, ISKReferenceCounted
	{
		internal SKImage (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		// create brand new image

		/// <summary>
		/// Creates a new raster-based <see cref="T:SkiaSharp.SKImage" /> using the specified information.
		/// </summary>
		/// <param name="info">The image information to use.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImage" /> instance.</returns>
		public static SKImage Create (SKImageInfo info)
		{
			var pixels = Marshal.AllocCoTaskMem (info.BytesSize);
			using (var pixmap = new SKPixmap (info, pixels)) {
				// don't use the managed version as that is just extra overhead which isn't necessary
				return GetObject (SkiaApi.sk_image_new_raster (pixmap.Handle, DelegateProxies.SKImageRasterReleaseProxyForCoTaskMem, null));
			}
		}

		// create a new image from a copy of pixel data

		/// <summary>
		/// Creates a new image from a copy of the stream data.
		/// </summary>
		/// <param name="info">The image information describing the encoding of the image in the stream.</param>
		/// <param name="pixels">The stream of image data.</param>
		/// <returns>A new image with a copy of the contents of the specified buffer, or <see langword="null" /> on error.</returns>
		public static SKImage FromPixelCopy (SKImageInfo info, SKStream pixels) =>
			FromPixelCopy (info, pixels, info.RowBytes);

		/// <summary>
		/// Creates a new image from a copy of the stream data.
		/// </summary>
		/// <param name="info">The image information describing the encoding of the image in the stream.</param>
		/// <param name="pixels">The stream of image data.</param>
		/// <param name="rowBytes">The specified the number of bytes used per row in the image.</param>
		/// <returns>A new image with a copy of the contents of the specified buffer, or <see langword="null" /> on error.</returns>
		public static SKImage FromPixelCopy (SKImageInfo info, SKStream pixels, int rowBytes)
		{
			if (pixels == null)
				throw new ArgumentNullException (nameof (pixels));
			using (var data = SKData.Create (pixels)) {
				return FromPixels (info, data, rowBytes);
			}
		}

		/// <summary>
		/// Creates a new image from a copy of the stream data.
		/// </summary>
		/// <param name="info">The image information describing the encoding of the image in the stream.</param>
		/// <param name="pixels">The stream of image data.</param>
		/// <returns>A new image with a copy of the contents of the specified buffer, or <see langword="null" /> on error.</returns>
		public static SKImage FromPixelCopy (SKImageInfo info, Stream pixels) =>
			FromPixelCopy (info, pixels, info.RowBytes);

		/// <summary>
		/// Creates a new image from a copy of the stream data.
		/// </summary>
		/// <param name="info">The image information describing the encoding of the image in the stream.</param>
		/// <param name="pixels">The stream of image data.</param>
		/// <param name="rowBytes">The specified the number of bytes used per row in the image.</param>
		/// <returns>A new image with a copy of the contents of the specified buffer, or <see langword="null" /> on error.</returns>
		public static SKImage FromPixelCopy (SKImageInfo info, Stream pixels, int rowBytes)
		{
			if (pixels == null)
				throw new ArgumentNullException (nameof (pixels));
			using (var data = SKData.Create (pixels)) {
				return FromPixels (info, data, rowBytes);
			}
		}

		/// <summary>
		/// Creates a new image from a copy of an in-memory buffer.
		/// </summary>
		/// <param name="info">The image information describing the encoding of the image in memory.</param>
		/// <param name="pixels">The buffer of image data.</param>
		/// <returns>A new image with a copy of the contents of the specified buffer, or <see langword="null" /> on error.</returns>
		public static SKImage FromPixelCopy (SKImageInfo info, byte[] pixels) =>
			FromPixelCopy (info, pixels, info.RowBytes);

		/// <summary>
		/// Creates a new image from a copy of an in-memory buffer.
		/// </summary>
		/// <param name="info">The image information describing the encoding of the image in memory.</param>
		/// <param name="pixels">The buffer of image data.</param>
		/// <param name="rowBytes">The specified the number of bytes used per row in the image.</param>
		/// <returns>A new image with a copy of the contents of the specified buffer, or <see langword="null" /> on error.</returns>
		public static SKImage FromPixelCopy (SKImageInfo info, byte[] pixels, int rowBytes)
		{
			if (pixels == null)
				throw new ArgumentNullException (nameof (pixels));
			using (var data = SKData.CreateCopy (pixels)) {
				return FromPixels (info, data, rowBytes);
			}
		}

		/// <summary>
		/// Creates a new image from a copy of an in-memory buffer.
		/// </summary>
		/// <param name="info">The image information describing the encoding of the image in memory.</param>
		/// <param name="pixels">The pointer to the image in memory.</param>
		/// <returns>A new image with a copy of the contents of the specified buffer, or <see langword="null" /> on error.</returns>
		public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels) =>
			FromPixelCopy (info, pixels, info.RowBytes);

		/// <summary>
		/// Creates a new image from a copy of an in-memory buffer.
		/// </summary>
		/// <param name="info">The image information describing the encoding of the image in memory.</param>
		/// <param name="pixels">The pointer to the image in memory.</param>
		/// <param name="rowBytes">The specified the number of bytes used per row in the image.</param>
		/// <returns>A new image with a copy of the contents of the specified buffer, or <see langword="null" /> on error.</returns>
		public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			if (pixels == IntPtr.Zero)
				throw new ArgumentNullException (nameof (pixels));

			var nInfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject (SkiaApi.sk_image_new_raster_copy (&nInfo, (void*)pixels, (IntPtr)rowBytes));
		}

		/// <summary>
		/// Creates a new image from a copy of an in-memory buffer.
		/// </summary>
		/// <param name="pixmap">The pixmap that contains the image information and buffer location.</param>
		/// <returns>A new image with a copy of the contents of the specified buffer, or <see langword="null" /> on error.</returns>
		public static SKImage FromPixelCopy (SKPixmap pixmap)
		{
			if (pixmap == null)
				throw new ArgumentNullException (nameof (pixmap));
			return GetObject (SkiaApi.sk_image_new_raster_copy_with_pixmap (pixmap.Handle));
		}

		/// <summary>
		/// Creates a new image from a copy of an in-memory buffer.
		/// </summary>
		/// <param name="info">The image information describing the encoding of the image in memory.</param>
		/// <param name="pixels">The buffer of image data.</param>
		/// <returns>A new image with a copy of the contents of the specified buffer, or <see langword="null" /> on error.</returns>
		public static SKImage FromPixelCopy (SKImageInfo info, ReadOnlySpan<byte> pixels) =>
			FromPixelCopy (info, pixels, info.RowBytes);

		/// <summary>
		/// Creates a new image from a copy of an in-memory buffer.
		/// </summary>
		/// <param name="info">The image information describing the encoding of the image in memory.</param>
		/// <param name="pixels">The buffer of image data.</param>
		/// <param name="rowBytes">The specified the number of bytes used per row in the image.</param>
		/// <returns>A new image with a copy of the contents of the specified buffer, or <see langword="null" /> on error.</returns>
		public static SKImage FromPixelCopy (SKImageInfo info, ReadOnlySpan<byte> pixels, int rowBytes)
		{
			if (pixels == null)
				throw new ArgumentNullException (nameof (pixels));
			using (var data = SKData.CreateCopy (pixels)) {
				return FromPixels (info, data, rowBytes);
			}
		}

		// create a new image around existing pixel data

		/// <param name="info"></param>
		/// <param name="data"></param>
		public static SKImage FromPixels (SKImageInfo info, SKData data) =>
			FromPixels (info, data, info.RowBytes);

		/// <summary>
		/// Creates a new image from an in-memory buffer.
		/// </summary>
		/// <param name="info">The image information describing the encoding of the image in memory.</param>
		/// <param name="data">The data object that contains the pixel data.</param>
		/// <param name="rowBytes">The specified the number of bytes used per row in the image.</param>
		/// <returns>A new image wrapping the specified buffer, or <see langword="null" /> on error.</returns>
		public static SKImage FromPixels (SKImageInfo info, SKData data, int rowBytes)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject (SkiaApi.sk_image_new_raster_data (&cinfo, data.Handle, (IntPtr)rowBytes));
		}

		/// <summary>
		/// Creates a new image from an in-memory buffer.
		/// </summary>
		/// <param name="info">The image information describing the encoding of the image in memory.</param>
		/// <param name="pixels">The pointer to the image in memory.</param>
		/// <returns>A new image wrapping the specified buffer, or <see langword="null" /> on error.</returns>
		public static SKImage FromPixels (SKImageInfo info, IntPtr pixels)
		{
			using (var pixmap = new SKPixmap (info, pixels, info.RowBytes)) {
				return FromPixels (pixmap, null, null);
			}
		}

		/// <summary>
		/// Creates a new image from an in-memory buffer.
		/// </summary>
		/// <param name="info">The image information describing the encoding of the image in memory.</param>
		/// <param name="pixels">The pointer to the image in memory.</param>
		/// <param name="rowBytes">The specified the number of bytes used per row in the image.</param>
		/// <returns>A new image wrapping the specified buffer, or <see langword="null" /> on error.</returns>
		public static SKImage FromPixels (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			using (var pixmap = new SKPixmap (info, pixels, rowBytes)) {
				return FromPixels (pixmap, null, null);
			}
		}

		/// <summary>
		/// Creates a new image from an in-memory buffer.
		/// </summary>
		/// <param name="pixmap">The pixmap that contains the image information and buffer location.</param>
		/// <returns>A new image wrapping the specified buffer, or <see langword="null" /> on error.</returns>
		public static SKImage FromPixels (SKPixmap pixmap)
		{
			return FromPixels (pixmap, null, null);
		}

		/// <summary>
		/// Creates a new image from an in-memory buffer.
		/// </summary>
		/// <param name="pixmap">The pixmap that contains the image information and buffer location.</param>
		/// <param name="releaseProc">The delegate to invoke when the image is about to be destroyed.</param>
		/// <returns>A new image wrapping the specified buffer, or <see langword="null" /> on error.</returns>
		public static SKImage FromPixels (SKPixmap pixmap, SKImageRasterReleaseDelegate releaseProc)
		{
			return FromPixels (pixmap, releaseProc, null);
		}

		/// <summary>
		/// Creates a new image from an in-memory buffer.
		/// </summary>
		/// <param name="pixmap">The pixmap that contains the image information and buffer location.</param>
		/// <param name="releaseProc">The delegate to invoke when the image is about to be destroyed.</param>
		/// <param name="releaseContext">The user data to use when invoking the delegate.</param>
		/// <returns>A new image wrapping the specified buffer, or <see langword="null" /> on error.</returns>
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

		/// <summary>
		/// Creates a new image from an encoded image wrapped by the data.
		/// </summary>
		/// <param name="data">The data holding the encoded image.</param>
		/// <param name="subset">The bounds for a subset of the image.</param>
		/// <returns>The decoded image, or <see langword="null" /> on error.</returns>
		public static SKImage FromEncodedData (SKData data, SKRectI subset)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			return FromEncodedData (data)?.Subset (subset);
		}

		/// <summary>
		/// Creates a new image from an encoded image wrapped by the data.
		/// </summary>
		/// <param name="data">The data holding the encoded image.</param>
		/// <returns>The decoded image, or <see langword="null" /> on error.</returns>
		public static SKImage FromEncodedData (SKData data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			var handle = SkiaApi.sk_image_new_from_encoded (data.Handle);
			return GetObject (handle);
		}

		/// <summary>
		/// Creates a new image from an encoded image buffer.
		/// </summary>
		/// <param name="data">The buffer holding the encoded image.</param>
		/// <returns>The decoded image, or <see langword="null" /> on error.</returns>
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

		/// <summary>
		/// Creates a new image from an encoded image buffer.
		/// </summary>
		/// <param name="data">The buffer holding the encoded image.</param>
		/// <returns>The decoded image, or <see langword="null" /> on error.</returns>
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

		/// <summary>
		/// Creates a new image from an encoded image stream.
		/// </summary>
		/// <param name="data">The stream holding the encoded image.</param>
		/// <returns>The decoded image, or <see langword="null" /> on error.</returns>
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

		/// <summary>
		/// Creates a new image from an encoded image stream.
		/// </summary>
		/// <param name="data">The stream holding the encoded image.</param>
		/// <returns>The decoded image, or <see langword="null" /> on error.</returns>
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

		/// <summary>
		/// Creates a new image from an encoded image file.
		/// </summary>
		/// <param name="filename">The path to an encoded image on the file system.</param>
		/// <returns>The decoded image, or <see langword="null" /> on error.</returns>
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

		/// <summary>
		/// Creates a new image from the provided <see cref="T:SkiaSharp.SKBitmap" />.
		/// </summary>
		/// <param name="bitmap">The bitmap that will be used as the source for the image.</param>
		/// <returns>An image whose contents are the contents of the specified bitmap.</returns>
		/// <remarks>If the bitmap is marked immutable, and its pixel memory is shareable, it may be shared instead of copied.</remarks>
		public static SKImage FromBitmap (SKBitmap bitmap)
		{
			if (bitmap == null)
				throw new ArgumentNullException (nameof (bitmap));

			var image = GetObject (SkiaApi.sk_image_new_from_bitmap (bitmap.Handle));
			GC.KeepAlive (bitmap);
			return image;
		}

		// create a new image from a GPU texture

		/// <summary>
		/// Creates a new image from the specified texture.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing backend texture.</param>
		/// <param name="colorType">The color type to use for the image.</param>
		/// <returns>Returns the new image, or <see langword="null" /> if the specified texture is unsupported</returns>
		public static SKImage FromTexture (GRContext context, GRBackendTexture texture, SKColorType colorType) =>
			FromTexture ((GRRecordingContext)context, texture, colorType);

		/// <summary>
		/// Creates a new image from the specified texture.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing backend texture.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="colorType">The color type to use for the image.</param>
		/// <returns>Returns the new image, or <see langword="null" /> if the specified texture is unsupported</returns>
		public static SKImage FromTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType) =>
			FromTexture ((GRRecordingContext)context, texture, origin, colorType);

		/// <summary>
		/// Creates a new image from the specified texture.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing backend texture.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="colorType">The color type to use for the image.</param>
		/// <param name="alpha">The transparency mode to use for the image.</param>
		/// <returns>Returns the new image, or <see langword="null" /> if the specified texture is unsupported</returns>
		public static SKImage FromTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha) =>
			FromTexture ((GRRecordingContext)context, texture, origin, colorType, alpha);

		/// <summary>
		/// Creates a new image from the specified texture.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing backend texture.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="colorType">The color type to use for the image.</param>
		/// <param name="alpha">The transparency mode to use for the image.</param>
		/// <param name="colorspace">The colorspace to use for the image.</param>
		/// <returns>Returns the new image, or <see langword="null" /> if the specified texture is unsupported</returns>
		public static SKImage FromTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace) =>
			FromTexture ((GRRecordingContext)context, texture, origin, colorType, alpha, colorspace);

		/// <summary>
		/// Creates a new image from the specified texture.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing backend texture.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="colorType">The color type to use for the image.</param>
		/// <param name="alpha">The transparency mode to use for the image.</param>
		/// <param name="colorspace">The colorspace to use for the image.</param>
		/// <param name="releaseProc">The delegate to invoke when the image is about to be destroyed.</param>
		/// <returns>Returns the new image, or <see langword="null" /> if the specified texture is unsupported</returns>
		public static SKImage FromTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc) =>
			FromTexture ((GRRecordingContext)context, texture, origin, colorType, alpha, colorspace, releaseProc);

		/// <summary>
		/// Creates a new image from the specified texture.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing backend texture.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="colorType">The color type to use for the image.</param>
		/// <param name="alpha">The transparency mode to use for the image.</param>
		/// <param name="colorspace">The colorspace to use for the image.</param>
		/// <param name="releaseProc">The delegate to invoke when the image is about to be destroyed.</param>
		/// <param name="releaseContext">The user data to use when invoking the delegate.</param>
		/// <returns>Returns the new image, or <see langword="null" /> if the specified texture is unsupported</returns>
		public static SKImage FromTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc, object releaseContext) =>
			FromTexture ((GRRecordingContext)context, texture, origin, colorType, alpha, colorspace, releaseProc, releaseContext);

		/// <param name="context"></param>
		/// <param name="texture"></param>
		/// <param name="colorType"></param>
		public static SKImage FromTexture (GRRecordingContext context, GRBackendTexture texture, SKColorType colorType) =>
			FromTexture (context, texture, GRSurfaceOrigin.BottomLeft, colorType, SKAlphaType.Premul, null, null, null);

		/// <param name="context"></param>
		/// <param name="texture"></param>
		/// <param name="origin"></param>
		/// <param name="colorType"></param>
		public static SKImage FromTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType) =>
			FromTexture (context, texture, origin, colorType, SKAlphaType.Premul, null, null, null);

		/// <param name="context"></param>
		/// <param name="texture"></param>
		/// <param name="origin"></param>
		/// <param name="colorType"></param>
		/// <param name="alpha"></param>
		public static SKImage FromTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha) =>
			FromTexture (context, texture, origin, colorType, alpha, null, null, null);

		/// <param name="context"></param>
		/// <param name="texture"></param>
		/// <param name="origin"></param>
		/// <param name="colorType"></param>
		/// <param name="alpha"></param>
		/// <param name="colorspace"></param>
		public static SKImage FromTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace) =>
			FromTexture (context, texture, origin, colorType, alpha, colorspace, null, null);

		/// <param name="context"></param>
		/// <param name="texture"></param>
		/// <param name="origin"></param>
		/// <param name="colorType"></param>
		/// <param name="alpha"></param>
		/// <param name="colorspace"></param>
		/// <param name="releaseProc"></param>
		public static SKImage FromTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc) =>
			FromTexture (context, texture, origin, colorType, alpha, colorspace, releaseProc, null);

		/// <param name="context"></param>
		/// <param name="texture"></param>
		/// <param name="origin"></param>
		/// <param name="colorType"></param>
		/// <param name="alpha"></param>
		/// <param name="colorspace"></param>
		/// <param name="releaseProc"></param>
		/// <param name="releaseContext"></param>
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

		/// <summary>
		/// Creates a new image from the specified texture.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing backend texture.</param>
		/// <param name="colorType">The color type to use for the image.</param>
		/// <returns>Returns the new image, or <see langword="null" /> if the specified texture is unsupported.</returns>
		/// <remarks>SkiaSharp will delete or recycle the texture when the image is released.</remarks>
		public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, SKColorType colorType) =>
			FromAdoptedTexture ((GRRecordingContext)context, texture, colorType);

		/// <summary>
		/// Creates a new image from the specified texture.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing backend texture.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="colorType">The color type to use for the image.</param>
		/// <returns>Returns the new image, or <see langword="null" /> if the specified texture is unsupported.</returns>
		/// <remarks>SkiaSharp will delete or recycle the texture when the image is released.</remarks>
		public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType) =>
			FromAdoptedTexture ((GRRecordingContext)context, texture, origin, colorType);

		/// <summary>
		/// Creates a new image from the specified texture.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing backend texture.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="colorType">The color type to use for the image.</param>
		/// <param name="alpha">The transparency mode to use for the image.</param>
		/// <returns>Returns the new image, or <see langword="null" /> if the specified texture is unsupported.</returns>
		/// <remarks>SkiaSharp will delete or recycle the texture when the image is released.</remarks>
		public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha) =>
			FromAdoptedTexture ((GRRecordingContext)context, texture, origin, colorType, alpha);

		/// <summary>
		/// Creates a new image from the specified texture.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing backend texture.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="colorType">The color type to use for the image.</param>
		/// <param name="alpha">The transparency mode to use for the image.</param>
		/// <param name="colorspace">The colorspace to use for the image.</param>
		/// <returns>Returns the new image, or <see langword="null" /> if the specified texture is unsupported.</returns>
		/// <remarks>SkiaSharp will delete or recycle the texture when the image is released.</remarks>
		public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace) =>
			FromAdoptedTexture ((GRRecordingContext)context, texture, origin, colorType, alpha, colorspace);

		/// <param name="context"></param>
		/// <param name="texture"></param>
		/// <param name="colorType"></param>
		public static SKImage FromAdoptedTexture (GRRecordingContext context, GRBackendTexture texture, SKColorType colorType) =>
			FromAdoptedTexture (context, texture, GRSurfaceOrigin.BottomLeft, colorType, SKAlphaType.Premul, null);

		/// <param name="context"></param>
		/// <param name="texture"></param>
		/// <param name="origin"></param>
		/// <param name="colorType"></param>
		public static SKImage FromAdoptedTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType) =>
			FromAdoptedTexture (context, texture, origin, colorType, SKAlphaType.Premul, null);

		/// <param name="context"></param>
		/// <param name="texture"></param>
		/// <param name="origin"></param>
		/// <param name="colorType"></param>
		/// <param name="alpha"></param>
		public static SKImage FromAdoptedTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha) =>
			FromAdoptedTexture (context, texture, origin, colorType, alpha, null);

		/// <param name="context"></param>
		/// <param name="texture"></param>
		/// <param name="origin"></param>
		/// <param name="colorType"></param>
		/// <param name="alpha"></param>
		/// <param name="colorspace"></param>
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

		/// <summary>
		/// Creates a new image from the provided <see cref="T:SkiaSharp.SKPicture" />.
		/// </summary>
		/// <param name="picture">The picture.</param>
		/// <param name="dimensions">The size of the raster image.</param>
		/// <returns>An image whose contents is the picture.</returns>
		public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions) =>
			FromPicture (picture, dimensions, null, null, false, SKColorSpace.CreateSrgb (), null);

		/// <summary>
		/// Creates a new image from the provided <see cref="T:SkiaSharp.SKPicture" />.
		/// </summary>
		/// <param name="picture">The picture.</param>
		/// <param name="dimensions">The size of the raster image.</param>
		/// <param name="matrix">The matrix to use when reading the image.</param>
		/// <returns>An image whose contents is the picture.</returns>
		public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions, SKMatrix matrix) =>
			FromPicture (picture, dimensions, &matrix, null, false, SKColorSpace.CreateSrgb (), null);

		/// <summary>
		/// Creates a new image from the provided <see cref="T:SkiaSharp.SKPicture" />.
		/// </summary>
		/// <param name="picture">The picture.</param>
		/// <param name="dimensions">The size of the raster image.</param>
		/// <param name="paint">The paint to use when reading the image.</param>
		/// <returns>An image whose contents is the picture.</returns>
		public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions, SKPaint paint) =>
			FromPicture (picture, dimensions, null, paint, false, SKColorSpace.CreateSrgb (), null);

		/// <summary>
		/// Creates a new image from the provided <see cref="T:SkiaSharp.SKPicture" />.
		/// </summary>
		/// <param name="picture">The picture.</param>
		/// <param name="dimensions">The size of the raster image.</param>
		/// <param name="matrix">The matrix to use when reading the image.</param>
		/// <param name="paint">The paint to use when reading the image.</param>
		/// <returns>An image whose contents is the picture.</returns>
		public static SKImage FromPicture (SKPicture picture, SKSizeI dimensions, SKMatrix matrix, SKPaint paint) =>
			FromPicture (picture, dimensions, &matrix, paint, false, SKColorSpace.CreateSrgb (), null);

		private static SKImage FromPicture (SKPicture picture, SKSizeI dimensions, SKMatrix* matrix, SKPaint paint, bool useFloatingPointBitDepth, SKColorSpace colorspace, SKSurfaceProperties props)
		{
			if (picture == null)
				throw new ArgumentNullException (nameof (picture));

			var p = paint?.Handle ?? IntPtr.Zero;
			var cs = colorspace?.Handle ?? IntPtr.Zero;
			var prps = props?.Handle ?? IntPtr.Zero;
			return GetObject (SkiaApi.sk_image_new_from_picture (picture.Handle, &dimensions, matrix, p, useFloatingPointBitDepth, cs, prps));
		}

		/// <summary>
		/// Encodes the image using the <see cref="F:SkiaSharp.SKImageEncodeFormat.Png" /> format.
		/// </summary>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKData" /> wrapping the encoded image.</returns>
		/// <remarks>Use the overload that takes a <see cref="T:SkiaSharp.SKImageEncodeFormat" /> if you want to encode in a different format.</remarks>
		public SKData Encode ()
		{
			if (EncodedData is not null)
				return EncodedData;

			return Encode (SKEncodedImageFormat.Png, 100);
		}

		/// <summary>
		/// Encodes the image using the specified format.
		/// </summary>
		/// <param name="format">The file format used to encode the image.</param>
		/// <param name="quality">The quality level to use for the image. This is in the range from 0-100.</param>
		/// <returns>Returns the <see cref="T:SkiaSharp.SKData" /> wrapping the encoded image.</returns>
		/// <remarks>The quality is a suggestion, and not all formats (for example, PNG) respect or support it.</remarks>
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

		/// <summary>
		/// Gets the image width.
		/// </summary>
		public int Width =>
			SkiaApi.sk_image_get_width (Handle);

		/// <summary>
		/// Gets the image height.
		/// </summary>
		public int Height =>
			SkiaApi.sk_image_get_height (Handle);

		/// <summary>
		/// Gets the unique ID associated with the image.
		/// </summary>
		public uint UniqueId =>
			SkiaApi.sk_image_get_unique_id (Handle);

		/// <summary>
		/// Gets the configured <see cref="T:SkiaSharp.SKAlphaType" /> for the bitmap.
		/// </summary>
		public SKAlphaType AlphaType =>
			SkiaApi.sk_image_get_alpha_type (Handle);

		/// <summary>
		/// Gets the image color type.
		/// </summary>
		public SKColorType ColorType =>
			SkiaApi.sk_image_get_color_type (Handle).FromNative ();

		/// <summary>
		/// Gets the image color space.
		/// </summary>
		public SKColorSpace ColorSpace =>
			SKColorSpace.GetObject (SkiaApi.sk_image_get_colorspace (Handle));

		/// <summary>
		/// Gets a value indicating whether the image will be drawn as a mask, with no intrinsic color of its own
		/// </summary>
		public bool IsAlphaOnly =>
			SkiaApi.sk_image_is_alpha_only (Handle);

		/// <summary>
		/// Gets the encoded image pixels as a <see cref="T:SkiaSharp.SKData" />, if the image was created from supported encoded stream format.
		/// </summary>
		/// <remarks>Returns <see langword="null" /> if the image mage contents are not encoded.</remarks>
		public SKData EncodedData =>
			SKData.GetObject (SkiaApi.sk_image_ref_encoded (Handle));

		public SKImageInfo Info =>
			new SKImageInfo (Width, Height, ColorType, AlphaType, ColorSpace);

		// ToShader

		public SKShader ToShader () =>
			ToShader (SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, SKSamplingOptions.Default, null);

		/// <summary>
		/// Creates a new bitmap shader from the current image.
		/// </summary>
		/// <param name="tileX">The method in which to tile along the x-axis.</param>
		/// <param name="tileY">The method in which to tile along the y-axis.</param>
		/// <returns>Returns a new bitmap shader that will draw the current image.</returns>
		public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY) =>
			ToShader (tileX, tileY, SKSamplingOptions.Default, null);

		/// <summary>
		/// Creates a new bitmap shader from the current image.
		/// </summary>
		/// <param name="tileX">The method in which to tile along the x-axis.</param>
		/// <param name="tileY">The method in which to tile along the y-axis.</param>
		/// <param name="localMatrix">The local matrix to use with the shader.</param>
		/// <returns>Returns a new bitmap shader that will draw the current image.</returns>
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

		/// <summary>
		/// Returns the pixmap if the image is raster-based.
		/// </summary>
		/// <param name="pixmap">The pixmap to receive the pixel information.</param>
		/// <returns>Returns <see langword="true" /> on success, or <see langword="false" /> if the image does not have access to pixel data.</returns>
		public bool PeekPixels (SKPixmap pixmap)
		{
			if (pixmap == null)
				throw new ArgumentNullException (nameof (pixmap));

			var result = SkiaApi.sk_image_peek_pixels (Handle, pixmap.Handle);
			if (result)
				pixmap.pixelSource = this;
			return result;
		}

		/// <summary>
		/// Returns the pixmap if the image is raster-based.
		/// </summary>
		/// <returns>Returns the pixmap, or <see langword="null" /> if the image does not have access to pixel data.</returns>
		public SKPixmap PeekPixels ()
		{
			var pixmap = new SKPixmap ();
			if (!PeekPixels (pixmap)) {
				pixmap.Dispose ();
				pixmap = null;
			}
			return pixmap;
		}

		/// <summary>
		/// Gets a value indicating whether the image is texture backed.
		/// </summary>
		public bool IsTextureBacked =>
			SkiaApi.sk_image_is_texture_backed (Handle);

		/// <summary>
		/// Gets a value indicating whether the image is backed by an image-generator or other source that creates (and caches) its pixels / texture on-demand.
		/// </summary>
		/// <remarks>If this method returns <see langword="false" />, then <see cref="M:SkiaSharp.SKImage.PeekPixels" /> will return <see langword="null" />.</remarks>
		public bool IsLazyGenerated =>
			SkiaApi.sk_image_is_lazy_generated (Handle);

		/// <param name="context"></param>
		public bool IsValid (GRContext context) =>
			IsValid ((GRRecordingContext)context);

		/// <param name="context"></param>
		public bool IsValid (GRRecordingContext context) =>
			SkiaApi.sk_image_is_valid (Handle, context?.Handle ?? IntPtr.Zero);

		// ReadPixels

		/// <param name="dstInfo"></param>
		/// <param name="dstPixels"></param>
		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels) =>
			ReadPixels (dstInfo, dstPixels, dstInfo.RowBytes, 0, 0, SKImageCachingHint.Allow);

		/// <param name="dstInfo"></param>
		/// <param name="dstPixels"></param>
		/// <param name="dstRowBytes"></param>
		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes) =>
			ReadPixels (dstInfo, dstPixels, dstRowBytes, 0, 0, SKImageCachingHint.Allow);

		/// <summary>
		/// Copies the pixels from the image into the specified buffer.
		/// </summary>
		/// <param name="dstInfo">The image information describing the destination pixel buffer.</param>
		/// <param name="dstPixels">The pixel buffer to read the pixel data into.</param>
		/// <param name="dstRowBytes">The number of bytes in each row of in the destination buffer.</param>
		/// <param name="srcX">The source x-coordinate to start reading from.</param>
		/// <param name="srcY">The source y-coordinate to start reading from.</param>
		/// <returns>Returns <see langword="true" /> if the pixels were read, or <see langword="false" /> if there was an error.</returns>
		/// <remarks>This method may return <see langword="false" /> if the source rectangle [<paramref name="srcX" />, <paramref name="srcY" />, dstInfo.Width, dstInfo.Height] does not intersect the image, or if the color type/alpha type could not be converted to the destination types.</remarks>
		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY) =>
			ReadPixels (dstInfo, dstPixels, dstRowBytes, srcX, srcY, SKImageCachingHint.Allow);

		/// <summary>
		/// Copies the pixels from the image into the specified buffer.
		/// </summary>
		/// <param name="dstInfo">The image information describing the destination pixel buffer.</param>
		/// <param name="dstPixels">The pixel buffer to read the pixel data into.</param>
		/// <param name="dstRowBytes">The number of bytes in each row of in the destination buffer.</param>
		/// <param name="srcX">The source x-coordinate to start reading from.</param>
		/// <param name="srcY">The source y-coordinate to start reading from.</param>
		/// <param name="cachingHint">Whether or not to cache intermediate results.</param>
		/// <returns>Returns <see langword="true" /> if the pixels were read, or <see langword="false" /> if there was an error.</returns>
		/// <remarks>This method may return <see langword="false" /> if the source rectangle [<paramref name="srcX" />, <paramref name="srcY" />, dstInfo.Width, dstInfo.Height] does not intersect the image, or if the color type/alpha type could not be converted to the destination types.</remarks>
		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY, SKImageCachingHint cachingHint)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref dstInfo);
			var result = SkiaApi.sk_image_read_pixels (Handle, &cinfo, (void*)dstPixels, (IntPtr)dstRowBytes, srcX, srcY, cachingHint);
			GC.KeepAlive (this);
			return result;
		}

		/// <param name="pixmap"></param>
		public bool ReadPixels (SKPixmap pixmap) =>
			ReadPixels (pixmap, 0, 0, SKImageCachingHint.Allow);

		/// <summary>
		/// Copies the pixels from the image into the specified buffer.
		/// </summary>
		/// <param name="pixmap">The pixmap to read the pixel data into.</param>
		/// <param name="srcX">The source x-coordinate to start reading from.</param>
		/// <param name="srcY">The source y-coordinate to start reading from.</param>
		/// <returns>Returns <see langword="true" /> if the pixels were read, or <see langword="false" /> if there was an error.</returns>
		/// <remarks>This method may return <see langword="false" /> if the source rectangle [<paramref name="srcX" />, <paramref name="srcY" />, dst.Info.Width, dst.Info.Height] does not intersect the image, or if the color type/alpha type could not be converted to the destination types.</remarks>
		public bool ReadPixels (SKPixmap pixmap, int srcX, int srcY) =>
			ReadPixels (pixmap, srcX, srcY, SKImageCachingHint.Allow);

		/// <summary>
		/// Copies the pixels from the image into the specified buffer.
		/// </summary>
		/// <param name="pixmap">The pixmap to read the pixel data into.</param>
		/// <param name="srcX">The source x-coordinate to start reading from.</param>
		/// <param name="srcY">The source y-coordinate to start reading from.</param>
		/// <param name="cachingHint">Whether or not to cache intermediate results.</param>
		/// <returns>Returns <see langword="true" /> if the pixels were read, or <see langword="false" /> if there was an error.</returns>
		/// <remarks>This method may return <see langword="false" /> if the source rectangle [<paramref name="srcX" />, <paramref name="srcY" />, dst.Info.Width, dst.Info.Height] does not intersect the image, or if the color type/alpha type could not be converted to the destination types.</remarks>
		public bool ReadPixels (SKPixmap pixmap, int srcX, int srcY, SKImageCachingHint cachingHint)
		{
			if (pixmap == null)
				throw new ArgumentNullException (nameof (pixmap));

			var result = SkiaApi.sk_image_read_pixels_into_pixmap (Handle, pixmap.Handle, srcX, srcY, cachingHint);
			GC.KeepAlive (this);
			return result;
		}

		// ScalePixels

		/// <summary>
		/// Copies the pixels from this image into the destination pixmap, scaling the image if the dimensions differ.
		/// </summary>
		/// <param name="dst">The pixmap describing the destination pixel buffer.</param>
		/// <param name="quality">The quality of scaling to use.</param>
		/// <returns>Returns <see langword="true" /> on success, or <see langword="false" /> if the color type/alpha type could not be converted to the destination types.</returns>
		[Obsolete("Use ScalePixels(SKPixmap dst, SKSamplingOptions sampling) instead.")]
		public bool ScalePixels (SKPixmap dst, SKFilterQuality quality) =>
			ScalePixels (dst, quality.ToSamplingOptions ());

		/// <summary>
		/// Copies the pixels from this image into the destination pixmap, scaling the image if the dimensions differ.
		/// </summary>
		/// <param name="dst">The pixmap describing the destination pixel buffer.</param>
		/// <param name="quality">The quality of scaling to use.</param>
		/// <param name="cachingHint">Whether or not to cache intermediate results.</param>
		/// <returns>Returns <see langword="true" /> on success, or <see langword="false" /> if the color type/alpha type could not be converted to the destination types.</returns>
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

		/// <summary>
		/// Returns a new image that is a subset of this image.
		/// </summary>
		/// <param name="subset">The rectangle indicating the subset to obtain.</param>
		/// <returns>Returns the new image, or <see langword="null" /> if there was an error or the rectangle does not intersect the image.</returns>
		/// <remarks>The underlying implementation may share the pixels, or it may make a copy.</remarks>
		public SKImage Subset (SKRectI subset)
		{
			return GetObject (SkiaApi.sk_image_make_subset_raster (Handle, &subset));
		}

		public SKImage Subset (GRRecordingContext context, SKRectI subset)
		{
			return GetObject (SkiaApi.sk_image_make_subset (Handle, context?.Handle ?? IntPtr.Zero, &subset));
		}

		// ToRasterImage

		/// <summary>
		/// Returns a raster-based image of the current image.
		/// </summary>
		/// <returns>Returns a raster-based copy of a texture image, or the same image if it already raster-based.</returns>
		public SKImage ToRasterImage () =>
			ToRasterImage (false);

		/// <param name="ensurePixelData"></param>
		public SKImage ToRasterImage (bool ensurePixelData) =>
			ensurePixelData
				? GetObject (SkiaApi.sk_image_make_raster_image (Handle))
				: GetObject (SkiaApi.sk_image_make_non_texture_image (Handle));

		// ToTextureImage

		/// <param name="context"></param>
		public SKImage ToTextureImage (GRContext context) =>
			ToTextureImage (context, false, true);

		/// <param name="context"></param>
		/// <param name="mipmapped"></param>
		public SKImage ToTextureImage (GRContext context, bool mipmapped) =>
			ToTextureImage (context, mipmapped, true);

		public SKImage ToTextureImage (GRContext context, bool mipmapped, bool budgeted)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			return GetObject (SkiaApi.sk_image_make_texture_image (Handle, context.Handle, mipmapped, budgeted));
		}

		// ApplyImageFilter

		/// <summary>
		/// Applies a given image filter to this image, and return the filtered result.
		/// </summary>
		/// <param name="filter">The filter to apply to the current image.</param>
		/// <param name="subset">The active portion of this image.</param>
		/// <param name="clipBounds">Constrains the device-space extent of the image to the given rectangle.</param>
		/// <param name="outSubset">The active portion of the resulting image</param>
		/// <param name="outOffset">The amount to translate the resulting image relative to the source when it is drawn.</param>
		/// <returns>Returns the resulting image after the filter is applied, or <see langword="null" /> if the image could not be created or would be transparent black (#00000000).</returns>
		public SKImage ApplyImageFilter (SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPoint outOffset)
		{
			var image = ApplyImageFilter (filter, subset, clipBounds, out outSubset, out SKPointI outOffsetActual);
			outOffset = outOffsetActual;
			return image;
		}

		/// <summary>
		/// Applies a given image filter to this image, and return the filtered result.
		/// </summary>
		/// <param name="filter">The filter to apply to the current image.</param>
		/// <param name="subset">The active portion of this image.</param>
		/// <param name="clipBounds">Constrains the device-space extent of the image to the given rectangle.</param>
		/// <param name="outSubset">The active portion of the resulting image</param>
		/// <param name="outOffset">The amount to translate the resulting image relative to the source when it is drawn.</param>
		/// <returns>Returns the resulting image after the filter is applied, or <see langword="null" /> if the image could not be created or would be transparent black (#00000000).</returns>
		public SKImage ApplyImageFilter (SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPointI outOffset)
		{
			if (filter == null)
				throw new ArgumentNullException (nameof (filter));

			fixed (SKRectI* os = &outSubset)
			fixed (SKPointI* oo = &outOffset) {
				return GetObject (SkiaApi.sk_image_make_with_filter_raster (Handle, filter.Handle, &subset, &clipBounds, os, oo));
			}
		}

		/// <param name="context"></param>
		/// <param name="filter"></param>
		/// <param name="subset"></param>
		/// <param name="clipBounds"></param>
		/// <param name="outSubset"></param>
		/// <param name="outOffset"></param>
		public SKImage ApplyImageFilter (GRContext context, SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPointI outOffset) =>
			ApplyImageFilter ((GRRecordingContext)context, filter, subset, clipBounds, out outSubset, out outOffset);

		/// <param name="context"></param>
		/// <param name="filter"></param>
		/// <param name="subset"></param>
		/// <param name="clipBounds"></param>
		/// <param name="outSubset"></param>
		/// <param name="outOffset"></param>
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
