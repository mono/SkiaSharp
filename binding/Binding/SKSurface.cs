using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	public class SKSurface : SKObject
	{
		[Obsolete ("Use Create(SKImageInfo) instead.")]
		public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType) => Create (new SKImageInfo (width, height, colorType, alphaType));
		[Obsolete ("Use Create(SKImageInfo, SKSurfaceProperties) instead.")]
		public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, SKSurfaceProps props) => Create (new SKImageInfo (width, height, colorType, alphaType), props);
		[Obsolete ("Use Create(SKImageInfo, IntPtr, int) instead.")]
		public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes) => Create (new SKImageInfo (width, height, colorType, alphaType), pixels, rowBytes);
		[Obsolete ("Use Create(SKImageInfo, IntPtr, int, SKSurfaceProperties) instead.")]
		public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes, SKSurfaceProps props) => Create (new SKImageInfo (width, height, colorType, alphaType), pixels, rowBytes, props);

		[Preserve]
		internal SKSurface (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		// RASTER surface

		[Obsolete ("Use Create(SKImageInfo, SKSurfaceProperties) instead.")]
		public static SKSurface Create (SKImageInfo info, SKSurfaceProps props) =>
			Create (info, 0, new SKSurfaceProperties (props));

		public static SKSurface Create (SKImageInfo info) =>
			Create (info, 0, null);

		public static SKSurface Create (SKImageInfo info, int rowBytes) =>
			Create (info, rowBytes, null);

		public static SKSurface Create (SKImageInfo info, SKSurfaceProperties props) =>
			Create (info, 0, props);

		public static SKSurface Create (SKImageInfo info, int rowBytes, SKSurfaceProperties props)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_raster (ref cinfo, (IntPtr)rowBytes, props?.Handle ?? IntPtr.Zero));
		}

		// convenience RASTER DIRECT to use a SKPixmap instead of SKImageInfo and IntPtr

		[Obsolete ("Use Create(SKPixmap, SKSurfaceProperties) instead.")]
		public static SKSurface Create (SKPixmap pixmap, SKSurfaceProps props) =>
			Create (pixmap, new SKSurfaceProperties (props));

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

		[Obsolete ("Use Create(SKImageInfo, IntPtr, rowBytes, SKSurfaceProperties) instead.")]
		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceProps props) =>
			Create (info, pixels, rowBytes, null, null, new SKSurfaceProperties (props));

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
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_raster_direct (ref cinfo, pixels, (IntPtr)rowBytes, proxy, ctx, props?.Handle ?? IntPtr.Zero));
		}

		// GPU BACKEND RENDER TARGET surface

		[Obsolete ("Use Create(GRContext, GRBackendRenderTarget, GRSurfaceOrigin, SKColorType) instead.")]
		public static SKSurface Create (GRContext context, GRBackendRenderTargetDesc desc)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			var renderTarget = new GRBackendRenderTarget (context.Backend, desc);
			return Create (context, renderTarget, desc.Origin, desc.Config.ToColorType (), null, null);
		}

		[Obsolete ("Use Create(GRContext, GRBackendRenderTarget, GRSurfaceOrigin, SKColorType, SKSurfaceProperties) instead.")]
		public static SKSurface Create (GRContext context, GRBackendRenderTargetDesc desc, SKSurfaceProps props)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			var renderTarget = new GRBackendRenderTarget (context.Backend, desc);
			return Create (context, renderTarget, desc.Origin, desc.Config.ToColorType (), null, new SKSurfaceProperties (props));
		}

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

		[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
		public static SKSurface Create (GRContext context, GRGlBackendTextureDesc desc) =>
			Create (context, new GRBackendTexture (desc), desc.Origin, desc.SampleCount, desc.Config.ToColorType (), null, null);

		[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
		public static SKSurface Create (GRContext context, GRBackendTextureDesc desc) =>
			Create (context, new GRBackendTexture (desc), desc.Origin, desc.SampleCount, desc.Config.ToColorType (), null, null);

		[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProperties) instead.")]
		public static SKSurface Create (GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props) =>
			Create (context, new GRBackendTexture (desc), desc.Origin, desc.SampleCount, desc.Config.ToColorType (), null, new SKSurfaceProperties (props));

		[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProperties) instead.")]
		public static SKSurface Create (GRContext context, GRBackendTextureDesc desc, SKSurfaceProps props) =>
			Create (context, new GRBackendTexture (desc), desc.Origin, desc.SampleCount, desc.Config.ToColorType (), null, new SKSurfaceProperties (props));

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

		[Obsolete ("Use CreateAsRenderTarget(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
		public static SKSurface CreateAsRenderTarget (GRContext context, GRGlBackendTextureDesc desc) =>
			CreateAsRenderTarget (context, new GRBackendTexture (desc), desc.Origin, desc.SampleCount, desc.Config.ToColorType (), null, null);

		[Obsolete ("Use CreateAsRenderTarget(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
		public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTextureDesc desc) =>
			CreateAsRenderTarget (context, new GRBackendTexture (desc), desc.Origin, desc.SampleCount, desc.Config.ToColorType (), null, null);

		[Obsolete ("Use CreateAsRenderTarget(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProperties) instead.")]
		public static SKSurface CreateAsRenderTarget (GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props) =>
			CreateAsRenderTarget (context, new GRBackendTexture (desc), desc.Origin, desc.SampleCount, desc.Config.ToColorType (), null, new SKSurfaceProperties (props));

		[Obsolete ("Use CreateAsRenderTarget(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProperties) instead.")]
		public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTextureDesc desc, SKSurfaceProps props) =>
			CreateAsRenderTarget (context, new GRBackendTexture (desc), desc.Origin, desc.SampleCount, desc.Config.ToColorType (), null, new SKSurfaceProperties (props));

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

		[Obsolete ("Use Create(GRContext, bool, SKImageInfo, int, SKSurfaceProperties) instead.")]
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProps props) =>
			Create (context, budgeted, info, sampleCount, GRSurfaceOrigin.BottomLeft, new SKSurfaceProperties (props), false);

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
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_render_target (context.Handle, budgeted, ref cinfo, sampleCount, origin, props?.Handle ?? IntPtr.Zero, shouldCreateWithMips));
		}

		// NULL surface

		public static SKSurface CreateNull (int width, int height) =>
			GetObject<SKSurface> (SkiaApi.sk_surface_new_null (width, height));

		//

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_surface_unref (Handle);
			}

			base.Dispose (disposing);
		}

		public SKCanvas Canvas =>
			GetObject<SKCanvas> (SkiaApi.sk_surface_get_canvas (Handle), false);

		[Obsolete ("Use SurfaceProperties instead.")]
		public SKSurfaceProps SurfaceProps {
			get {
				var props = SurfaceProperties;
				return new SKSurfaceProps {
					Flags = props.Flags,
					PixelGeometry = props.PixelGeometry
				};
			}
		}

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
			return SkiaApi.sk_surface_read_pixels (Handle, ref cinfo, dstPixels, (IntPtr)dstRowBytes, srcX, srcY);
		}
	}
}
