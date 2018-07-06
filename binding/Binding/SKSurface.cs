using System;

namespace SkiaSharp
{
	public class SKSurface : SKObject
	{
		public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType) => Create (new SKImageInfo (width, height, colorType, alphaType));
		public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, SKSurfaceProps props) => Create (new SKImageInfo (width, height, colorType, alphaType), props);
		public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes) => Create (new SKImageInfo (width, height, colorType, alphaType), pixels, rowBytes);
		public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes, SKSurfaceProps props) => Create (new SKImageInfo (width, height, colorType, alphaType), pixels, rowBytes, props);

		[Preserve]
		internal SKSurface (IntPtr h, bool owns)
			: base (h, owns)
		{
		}
		
		public static SKSurface Create (SKImageInfo info)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_raster (ref cinfo, IntPtr.Zero));
		}

		public static SKSurface Create (SKImageInfo info, SKSurfaceProps props)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_raster (ref cinfo, ref props));
		}

		public static SKSurface Create (SKPixmap pixmap)
		{
			if (pixmap == null) {
				throw new ArgumentNullException (nameof (pixmap));
			}
			return Create (pixmap.Info, pixmap.GetPixels (), pixmap.RowBytes);
		}

		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_raster_direct (ref cinfo, pixels, (IntPtr)rowBytes, IntPtr.Zero));
		}

		public static SKSurface Create (SKPixmap pixmap, SKSurfaceProps props)
		{
			if (pixmap == null) {
				throw new ArgumentNullException (nameof (pixmap));
			}
			return Create (pixmap.Info, pixmap.GetPixels (), pixmap.RowBytes, props);
		}

		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceProps props)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_raster_direct (ref cinfo, pixels, (IntPtr)rowBytes, ref props));
		}
		
		public static SKSurface Create (GRContext context, GRBackendRenderTargetDesc desc, SKSurfaceProps props)
		{
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_backend_render_target (context.Handle, ref desc, ref props));
		}
		
		public static SKSurface Create (GRContext context, GRBackendRenderTargetDesc desc)
		{
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_backend_render_target (context.Handle, ref desc, IntPtr.Zero));
		}

		public static SKSurface Create (GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props)
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
				return Create (context, d, props);
			}
		}

		public static SKSurface Create (GRContext context, GRGlBackendTextureDesc desc)
		{
			unsafe {
				var h = desc.TextureHandle;
				var hPtr = &h;
				var d = new GRBackendTextureDesc
				{
					Flags = desc.Flags,
					Origin = desc.Origin,
					Width = desc.Width,
					Height = desc.Height,
					Config = desc.Config,
					SampleCount = desc.SampleCount,
					TextureHandle = (IntPtr)hPtr,
				};
				return Create (context, d);
			}
		}

		public static SKSurface CreateAsRenderTarget (GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props)
		{
			unsafe {
				var h = desc.TextureHandle;
				var hPtr = &h;
				var d = new GRBackendTextureDesc
				{
					Flags = desc.Flags,
					Origin = desc.Origin,
					Width = desc.Width,
					Height = desc.Height,
					Config = desc.Config,
					SampleCount = desc.SampleCount,
					TextureHandle = (IntPtr)hPtr,
				};
				return CreateAsRenderTarget (context, d, props);
			}
		}

		public static SKSurface CreateAsRenderTarget (GRContext context, GRGlBackendTextureDesc desc)
		{
			unsafe {
				var h = desc.TextureHandle;
				var hPtr = &h;
				var d = new GRBackendTextureDesc
				{
					Flags = desc.Flags,
					Origin = desc.Origin,
					Width = desc.Width,
					Height = desc.Height,
					Config = desc.Config,
					SampleCount = desc.SampleCount,
					TextureHandle = (IntPtr)hPtr,
				};
				return CreateAsRenderTarget (context, d);
			}
		}

		public static SKSurface Create (GRContext context, GRBackendTextureDesc desc, SKSurfaceProps props)
		{
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_backend_texture (context.Handle, ref desc, ref props));
		}
		
		public static SKSurface Create (GRContext context, GRBackendTextureDesc desc)
		{
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_backend_texture (context.Handle, ref desc, IntPtr.Zero));
		}
		
		public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTextureDesc desc, SKSurfaceProps props)
		{
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_backend_texture_as_render_target (context.Handle, ref desc, ref props));
		}
		
		public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTextureDesc desc)
		{
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_backend_texture_as_render_target (context.Handle, ref desc, IntPtr.Zero));
		}
		
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProps props)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_render_target (context.Handle, budgeted, ref cinfo, sampleCount, ref props));
		}
		
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_render_target (context.Handle, budgeted, ref cinfo, sampleCount, IntPtr.Zero));
		}
		
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_render_target (context.Handle, budgeted, ref cinfo, 0, IntPtr.Zero));
		}
		
		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_surface_unref (Handle);
			}

			base.Dispose (disposing);
		}
		
		public SKCanvas Canvas {
			get {
				return GetObject<SKCanvas> (SkiaApi.sk_surface_get_canvas (Handle), false);
			}
		}

		public SKSurfaceProps SurfaceProps {
			get {
				SKSurfaceProps props;
				SkiaApi.sk_surface_get_props (Handle, out props);
				return props;
			}
		}

		public SKImage Snapshot ()
		{
			return GetObject<SKImage> (SkiaApi.sk_surface_new_image_snapshot (Handle));
		}

		public void Draw (SKCanvas canvas, float x, float y, SKPaint paint)
		{
			if (canvas == null)
				throw new ArgumentNullException (nameof (canvas));

			SkiaApi.sk_surface_draw (Handle, canvas.Handle, x, y, paint == null ? IntPtr.Zero : paint.Handle);
		}

		public SKPixmap PeekPixels ()
		{
			SKPixmap pixmap = new SKPixmap ();
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
			if (pixmap == null) {
				throw new ArgumentNullException (nameof (pixmap));
			}
			return SkiaApi.sk_surface_peek_pixels (Handle, pixmap.Handle);
		}

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref dstInfo);
			return SkiaApi.sk_surface_read_pixels (Handle, ref cinfo, dstPixels, (IntPtr)dstRowBytes, srcX, srcY);
		}
	}
}

