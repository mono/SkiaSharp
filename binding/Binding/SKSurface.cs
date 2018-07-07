using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	// public delegates
	public delegate void SKSurfaceReleaseDelegate (IntPtr address, object context);

	// internal proxy delegates
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate void SKSurfaceReleaseDelegateInternal (IntPtr address, IntPtr context);

	public class SKSurface : SKObject
	{
		[Obsolete ("Use Create(SKImageInfo) instead.")]
		public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType) => Create (new SKImageInfo (width, height, colorType, alphaType));
		[Obsolete ("Use Create(SKImageInfo, SKSurfaceProps) instead.")]
		public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, SKSurfaceProps props) => Create (new SKImageInfo (width, height, colorType, alphaType), props);
		[Obsolete ("Use Create(SKImageInfo, IntPtr, int) instead.")]
		public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes) => Create (new SKImageInfo (width, height, colorType, alphaType), pixels, rowBytes);
		[Obsolete ("Use Create(SKImageInfo, IntPtr, int, SKSurfaceProps) instead.")]
		public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes, SKSurfaceProps props) => Create (new SKImageInfo (width, height, colorType, alphaType), pixels, rowBytes, props);

		// so the GC doesn't collect the delegate
		private static readonly SKSurfaceReleaseDelegateInternal releaseDelegateInternal;
		private static readonly IntPtr releaseDelegate;
		static SKSurface ()
		{
			releaseDelegateInternal = new SKSurfaceReleaseDelegateInternal (SKSurfaceReleaseInternal);
			releaseDelegate = Marshal.GetFunctionPointerForDelegate (releaseDelegateInternal);
		}

		[Preserve]
		internal SKSurface (IntPtr h, bool owns)
			: base (h, owns)
		{
		}
		
		// RASTER surface

		public static SKSurface Create (SKImageInfo info)
		{
			return Create (info, 0);
		}

		public static SKSurface Create (SKImageInfo info, int rowBytes)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_raster (ref cinfo, (IntPtr)rowBytes, IntPtr.Zero));
		}

		public static SKSurface Create (SKImageInfo info, SKSurfaceProps props)
		{
			return Create (info, 0, props);
		}

		public static SKSurface Create (SKImageInfo info, int rowBytes, SKSurfaceProps props)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_raster (ref cinfo, (IntPtr)rowBytes, ref props));
		}

		// convenience RASTER DIRECT to use a SKPixmap instead of SKImageInfo and IntPtr

		public static SKSurface Create (SKPixmap pixmap)
		{
			if (pixmap == null) {
				throw new ArgumentNullException (nameof (pixmap));
			}
			return Create (pixmap.Info, pixmap.GetPixels (), pixmap.RowBytes);
		}

		public static SKSurface Create (SKPixmap pixmap, SKSurfaceProps props)
		{
			if (pixmap == null) {
				throw new ArgumentNullException (nameof (pixmap));
			}
			return Create (pixmap.Info, pixmap.GetPixels (), pixmap.RowBytes, props);
		}

		// RASTER DIRECT surface

		public static SKSurface Create (SKImageInfo info, IntPtr pixels)
		{
			return Create (info, pixels, info.RowBytes, null, null);
		}

		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			return Create (info, pixels, rowBytes, null, null);
		}

		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceReleaseDelegate releaseProc, object context)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			if (releaseProc == null) {
				return GetObject<SKSurface> (SkiaApi.sk_surface_new_raster_direct (ref cinfo, pixels, (IntPtr)rowBytes, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero));
			} else {
				var ctx = new NativeDelegateContext (context, releaseProc);
				return GetObject<SKSurface> (SkiaApi.sk_surface_new_raster_direct (ref cinfo, pixels, (IntPtr)rowBytes, releaseDelegate, ctx.NativeContext, IntPtr.Zero));
			}
		}

		public static SKSurface Create (SKImageInfo info, IntPtr pixels, SKSurfaceProps props)
		{
			return Create (info, pixels, info.RowBytes, null, null, props);
		}

		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceProps props)
		{
			return Create (info, pixels, rowBytes, null, null, props);
		}

		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceReleaseDelegate releaseProc, object context, SKSurfaceProps props)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			if (releaseProc == null) {
				return GetObject<SKSurface> (SkiaApi.sk_surface_new_raster_direct (ref cinfo, pixels, (IntPtr)rowBytes, IntPtr.Zero, IntPtr.Zero, ref props));
			} else {
				var ctx = new NativeDelegateContext (context, releaseProc);
				return GetObject<SKSurface> (SkiaApi.sk_surface_new_raster_direct (ref cinfo, pixels, (IntPtr)rowBytes, releaseDelegate, ctx.NativeContext, ref props));
			}
		}
		
		// GPU BACKEND RENDER TARGET surface

		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProps props)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			if (renderTarget == null)
				throw new ArgumentNullException (nameof (renderTarget));

			var cs = colorspace == null ? IntPtr.Zero : colorspace.Handle;
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_backend_render_target (context.Handle, renderTarget.Handle, origin, colorType, cs, ref props));
		}

		public static SKSurface Create (GRContext context, GRBackendRenderTargetDesc desc, SKSurfaceProps props)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			var renderTarget = new GRBackendRenderTarget (context.Backend, desc);
			return Create (context, renderTarget, desc.Origin, desc.Config.ToColorType (), null, props);
		}
		
		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			if (renderTarget == null)
				throw new ArgumentNullException (nameof (renderTarget));

			var cs = colorspace == null ? IntPtr.Zero : colorspace.Handle;
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_backend_render_target (context.Handle, renderTarget.Handle, origin, colorType, cs, IntPtr.Zero));
		}

		public static SKSurface Create (GRContext context, GRBackendRenderTargetDesc desc)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			var renderTarget = new GRBackendRenderTarget (context.Backend, desc);
			return Create (context, renderTarget, desc.Origin, desc.Config.ToColorType (), null);
		}

		// GPU BACKEND TEXTURE surface

		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProps props)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			if (texture == null)
				throw new ArgumentNullException (nameof (texture));

			var cs = colorspace == null ? IntPtr.Zero : colorspace.Handle;
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_backend_texture (context.Handle, texture.Handle, origin, sampleCount, colorType, cs, ref props));
		}

		public static SKSurface Create (GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			var texture = new GRBackendTexture (desc);
			return Create (context, texture, desc.Origin, desc.SampleCount, desc.Config.ToColorType(), null, props);
		}

		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			if (texture == null)
				throw new ArgumentNullException (nameof (texture));

			var cs = colorspace == null ? IntPtr.Zero : colorspace.Handle;
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_backend_texture (context.Handle, texture.Handle, origin, sampleCount, colorType, cs, IntPtr.Zero));
		}

		public static SKSurface Create (GRContext context, GRGlBackendTextureDesc desc)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			var texture = new GRBackendTexture (desc);
			return Create (context, texture, desc.Origin, desc.SampleCount, desc.Config.ToColorType (), null);
		}

		// GPU BACKEND TEXTURE AS RENDER TARGET surface

		public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProps props)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			if (texture == null)
				throw new ArgumentNullException (nameof (texture));

			var cs = colorspace == null ? IntPtr.Zero : colorspace.Handle;
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_backend_texture_as_render_target (context.Handle, texture.Handle, origin, sampleCount, colorType, cs, ref props));
		}

		public static SKSurface CreateAsRenderTarget (GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			var texture = new GRBackendTexture (desc);
			return CreateAsRenderTarget (context, texture, desc.Origin, desc.SampleCount, desc.Config.ToColorType (), null, props);
		}

		public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			if (texture == null)
				throw new ArgumentNullException (nameof (texture));

			var cs = colorspace == null ? IntPtr.Zero : colorspace.Handle;
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_backend_texture_as_render_target (context.Handle, texture.Handle, origin, sampleCount, colorType, cs, IntPtr.Zero));
		}

		public static SKSurface CreateAsRenderTarget (GRContext context, GRGlBackendTextureDesc desc)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			var texture = new GRBackendTexture (desc);
			return CreateAsRenderTarget (context, texture, desc.Origin, desc.SampleCount, desc.Config.ToColorType (), null);
		}

		// GPU NEW surface

		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin, SKSurfaceProps props, bool shouldCreateWithMips)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_render_target (context.Handle, budgeted, ref cinfo, sampleCount, origin, ref props, shouldCreateWithMips));
		}

		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProps props)
		{
			return Create (context, budgeted, info, sampleCount, GRSurfaceOrigin.BottomLeft, props, false);
		}
		
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, SKSurfaceProps props)
		{
			return Create (context, budgeted, info, 0, GRSurfaceOrigin.BottomLeft, props, false);
		}
		
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_render_target (context.Handle, budgeted, ref cinfo, sampleCount, origin, IntPtr.Zero, false));
		}

		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount)
		{
			return Create (context, budgeted, info, sampleCount, GRSurfaceOrigin.BottomLeft);
		}
		
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info)
		{
			return Create (context, budgeted, info, 0, GRSurfaceOrigin.BottomLeft);
		}

		// NULL surface

		public static SKSurface CreateNull (int width, int height)
		{
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_null (width, height));
		}

		//

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

		// internal proxy

		[MonoPInvokeCallback (typeof (SKSurfaceReleaseDelegateInternal))]
		private static void SKSurfaceReleaseInternal (IntPtr address, IntPtr context)
		{
			using (var ctx = NativeDelegateContext.Unwrap (context)) {
				ctx.GetDelegate<SKSurfaceReleaseDelegate> () (address, ctx.ManagedContext);
			}
		}
	}
}

