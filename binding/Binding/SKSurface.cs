using System;

namespace SkiaSharp
{
	public unsafe class SKSurface : SKObject, ISKReferenceCounted
	{
		[Preserve]
		internal SKSurface (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		// RASTER surface

		public static SKSurface Create (SKImageInfo info) =>
			Create (info, 0, null);

		public static SKSurface Create (SKImageInfo info, int rowBytes) =>
			Create (info, rowBytes, null);

		public static SKSurface Create (SKImageInfo info, SKSurfaceProperties props) =>
			Create (info, 0, props);

		public static SKSurface Create (SKImageInfo info, int rowBytes, SKSurfaceProperties props)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_raster (&cinfo, (IntPtr)rowBytes, props?.Handle ?? IntPtr.Zero));
		}

		// convenience RASTER DIRECT to use a SKPixmap instead of SKImageInfo and IntPtr

		public static SKSurface Create (SKPixmap pixmap) =>
			Create (pixmap, null);

		public static SKSurface Create (SKPixmap pixmap, SKSurfaceProperties props)
		{
			if (pixmap == null) {
				throw new ArgumentNullException (nameof (pixmap));
			}
			return Create (pixmap.Info, pixmap.GetPixels (), pixmap.RowBytes, null, null, props);
		}

		// RASTER DIRECT surface

		public static SKSurface Create (SKImageInfo info, IntPtr pixels) =>
			Create (info, pixels, info.RowBytes, null, null, null);

		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes) =>
			Create (info, pixels, rowBytes, null, null, null);

		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceReleaseDelegate releaseProc, object context) =>
			Create (info, pixels, rowBytes, releaseProc, context, null);

		public static SKSurface Create (SKImageInfo info, IntPtr pixels, SKSurfaceProperties props) =>
			Create (info, pixels, info.RowBytes, null, null, props);

		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceProperties props) =>
			Create (info, pixels, rowBytes, null, null, props);

		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceReleaseDelegate releaseProc, object context, SKSurfaceProperties props)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			var del = releaseProc != null && context != null
				? new SKSurfaceReleaseDelegate ((addr, _) => releaseProc (addr, context))
				: releaseProc;
			var proxy = DelegateProxies.Create (del, DelegateProxies.SKSurfaceReleaseDelegateProxy, out _, out var ctx);
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_raster_direct (&cinfo, (void*)pixels, (IntPtr)rowBytes, proxy, (void*)ctx, props?.Handle ?? IntPtr.Zero));
		}

		// GPU BACKEND RENDER TARGET surface

		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, SKColorType colorType) =>
			Create (context, renderTarget, GRSurfaceOrigin.BottomLeft, colorType, null, null);

		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType) =>
			Create (context, renderTarget, origin, colorType, null, null);

		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace) =>
			Create (context, renderTarget, origin, colorType, colorspace, null);

		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, SKColorType colorType, SKSurfaceProperties props) =>
			Create (context, renderTarget, GRSurfaceOrigin.BottomLeft, colorType, null, props);

		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props) =>
			Create (context, renderTarget, origin, colorType, null, props);

		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			if (renderTarget == null)
				throw new ArgumentNullException (nameof (renderTarget));

			return GetObject<SKSurface> (SkiaApi.sk_surface_new_backend_render_target (context.Handle, renderTarget.Handle, origin, colorType, colorspace?.Handle ?? IntPtr.Zero, props?.Handle ?? IntPtr.Zero));
		}

		// GPU BACKEND TEXTURE surface

		public static SKSurface Create (GRContext context, GRBackendTexture texture, SKColorType colorType) =>
			Create (context, texture, GRSurfaceOrigin.BottomLeft, 0, colorType, null, null);

		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType) =>
			Create (context, texture, origin, 0, colorType, null, null);

		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType) =>
			Create (context, texture, origin, sampleCount, colorType, null, null);

		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace) =>
			Create (context, texture, origin, sampleCount, colorType, colorspace, null);

		public static SKSurface Create (GRContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProperties props) =>
			Create (context, texture, GRSurfaceOrigin.BottomLeft, 0, colorType, null, props);

		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props) =>
			Create (context, texture, origin, 0, colorType, null, props);

		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProperties props) =>
			Create (context, texture, origin, sampleCount, colorType, null, props);

		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			if (texture == null)
				throw new ArgumentNullException (nameof (texture));

			return GetObject<SKSurface> (SkiaApi.sk_surface_new_backend_texture (context.Handle, texture.Handle, origin, sampleCount, colorType, colorspace?.Handle ?? IntPtr.Zero, props?.Handle ?? IntPtr.Zero));
		}

		// GPU BACKEND TEXTURE AS RENDER TARGET surface

		public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, SKColorType colorType) =>
			CreateAsRenderTarget (context, texture, GRSurfaceOrigin.BottomLeft, 0, colorType, null, null);

		public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType) =>
			CreateAsRenderTarget (context, texture, origin, 0, colorType, null, null);

		public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType) =>
			CreateAsRenderTarget (context, texture, origin, sampleCount, colorType, null, null);

		public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace) =>
			CreateAsRenderTarget (context, texture, origin, sampleCount, colorType, colorspace, null);

		public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProperties props) =>
			CreateAsRenderTarget (context, texture, GRSurfaceOrigin.BottomLeft, 0, colorType, null, props);

		public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props) =>
			CreateAsRenderTarget (context, texture, origin, 0, colorType, null, props);

		public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProperties props) =>
			CreateAsRenderTarget (context, texture, origin, sampleCount, colorType, null, props);

		public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			if (texture == null)
				throw new ArgumentNullException (nameof (texture));

			return GetObject<SKSurface> (SkiaApi.sk_surface_new_backend_texture_as_render_target (context.Handle, texture.Handle, origin, sampleCount, colorType, colorspace?.Handle ?? IntPtr.Zero, props?.Handle ?? IntPtr.Zero));
		}

		// GPU NEW surface

		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info) =>
			Create (context, budgeted, info, 0, GRSurfaceOrigin.BottomLeft, null, false);

		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount) =>
			Create (context, budgeted, info, sampleCount, GRSurfaceOrigin.BottomLeft, null, false);

		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin) =>
			Create (context, budgeted, info, sampleCount, GRSurfaceOrigin.BottomLeft, null, false);

		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, SKSurfaceProperties props) =>
			Create (context, budgeted, info, 0, GRSurfaceOrigin.BottomLeft, props, false);

		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProperties props) =>
			Create (context, budgeted, info, sampleCount, GRSurfaceOrigin.BottomLeft, props, false);

		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin, SKSurfaceProperties props, bool shouldCreateWithMips)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_render_target (context.Handle, budgeted, &cinfo, sampleCount, origin, props?.Handle ?? IntPtr.Zero, shouldCreateWithMips));
		}

		// NULL surface

		public static SKSurface CreateNull (int width, int height) =>
			GetObject<SKSurface> (SkiaApi.sk_surface_new_null (width, height));

		//

		public SKCanvas Canvas =>
			GetObject<SKCanvas> (SkiaApi.sk_surface_get_canvas (Handle), false);

		public SKSurfaceProperties SurfaceProperties =>
			GetObject<SKSurfaceProperties> (SkiaApi.sk_surface_get_props (Handle), false);

		public SKImage Snapshot () =>
			GetObject<SKImage> (SkiaApi.sk_surface_new_image_snapshot (Handle));

		public void Draw (SKCanvas canvas, float x, float y, SKPaint paint)
		{
			if (canvas == null)
				throw new ArgumentNullException (nameof (canvas));

			SkiaApi.sk_surface_draw (Handle, canvas.Handle, x, y, paint == null ? IntPtr.Zero : paint.Handle);
		}

		public SKPixmap PeekPixels ()
		{
			var pixmap = new SKPixmap ();
			var result = PeekPixels (pixmap);
			if (result) {
				return pixmap;
			} else {
				pixmap.Dispose ();
				return null;
			}
		}

		public bool PeekPixels (SKPixmap pixmap)
		{
			if (pixmap == null)
				throw new ArgumentNullException (nameof (pixmap));

			return SkiaApi.sk_surface_peek_pixels (Handle, pixmap.Handle);
		}

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref dstInfo);
			return SkiaApi.sk_surface_read_pixels (Handle, &cinfo, (void*)dstPixels, (IntPtr)dstRowBytes, srcX, srcY);
		}
	}
}
