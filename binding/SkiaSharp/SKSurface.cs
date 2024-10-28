#nullable disable

using System;
using System.ComponentModel;

namespace SkiaSharp
{
	public unsafe class SKSurface : SKObject, ISKReferenceCounted, ISKSkipObjectRegistration
	{
		internal SKSurface (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

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
			return GetObject (SkiaApi.sk_surface_new_raster (&cinfo, (IntPtr)rowBytes, props?.Handle ?? IntPtr.Zero));
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
			DelegateProxies.Create (del, out _, out var ctx);
			var proxy = del != null ? DelegateProxies.SKSurfaceRasterReleaseProxy : null;
			return GetObject (SkiaApi.sk_surface_new_raster_direct (&cinfo, (void*)pixels, (IntPtr)rowBytes, proxy, (void*)ctx, props?.Handle ?? IntPtr.Zero));
		}

		// GPU BACKEND RENDER TARGET surface

		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, SKColorType colorType) =>
			Create ((GRRecordingContext)context, renderTarget, colorType);

		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType) =>
			Create ((GRRecordingContext)context, renderTarget, origin, colorType);

		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace) =>
			Create ((GRRecordingContext)context, renderTarget, origin, colorType, colorspace);

		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, SKColorType colorType, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, renderTarget, colorType, props);

		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, renderTarget, origin, colorType, props);

		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, renderTarget, origin, colorType, colorspace, props);

		public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, SKColorType colorType) =>
			Create (context, renderTarget, GRSurfaceOrigin.BottomLeft, colorType, null, null);

		public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType) =>
			Create (context, renderTarget, origin, colorType, null, null);

		public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace) =>
			Create (context, renderTarget, origin, colorType, colorspace, null);

		public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, SKColorType colorType, SKSurfaceProperties props) =>
			Create (context, renderTarget, GRSurfaceOrigin.BottomLeft, colorType, null, props);

		public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props) =>
			Create (context, renderTarget, origin, colorType, null, props);

		public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			if (renderTarget == null)
				throw new ArgumentNullException (nameof (renderTarget));

			return GetObject (SkiaApi.sk_surface_new_backend_render_target (context.Handle, renderTarget.Handle, origin, colorType.ToNative (), colorspace?.Handle ?? IntPtr.Zero, props?.Handle ?? IntPtr.Zero));
		}

		// GPU BACKEND TEXTURE surface

		public static SKSurface Create (GRContext context, GRBackendTexture texture, SKColorType colorType) =>
			Create ((GRRecordingContext)context, texture, colorType);

		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType) =>
			Create ((GRRecordingContext)context, texture, origin, colorType);

		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType) =>
			Create ((GRRecordingContext)context, texture, origin, sampleCount, colorType);

		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace) =>
			Create ((GRRecordingContext)context, texture, origin, sampleCount, colorType, colorspace);

		public static SKSurface Create (GRContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, texture, colorType, props);

		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, texture, origin, colorType, props);

		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, texture, origin, sampleCount, colorType, props);

		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, texture, origin, sampleCount, colorType, colorspace, props);

		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, SKColorType colorType) =>
			Create (context, texture, GRSurfaceOrigin.BottomLeft, 0, colorType, null, null);

		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType) =>
			Create (context, texture, origin, 0, colorType, null, null);

		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType) =>
			Create (context, texture, origin, sampleCount, colorType, null, null);

		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace) =>
			Create (context, texture, origin, sampleCount, colorType, colorspace, null);

		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProperties props) =>
			Create (context, texture, GRSurfaceOrigin.BottomLeft, 0, colorType, null, props);

		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props) =>
			Create (context, texture, origin, 0, colorType, null, props);

		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProperties props) =>
			Create (context, texture, origin, sampleCount, colorType, null, props);

		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			if (texture == null)
				throw new ArgumentNullException (nameof (texture));

			return GetObject (SkiaApi.sk_surface_new_backend_texture (context.Handle, texture.Handle, origin, sampleCount, colorType.ToNative (), colorspace?.Handle ?? IntPtr.Zero, props?.Handle ?? IntPtr.Zero));
		}

		// GPU NEW surface

		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info) =>
			Create ((GRRecordingContext)context, budgeted, info);

		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount) =>
			Create ((GRRecordingContext)context, budgeted, info, sampleCount);

		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin) =>
			Create ((GRRecordingContext)context, budgeted, info, sampleCount, origin);

		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, budgeted, info, props);

		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, budgeted, info, sampleCount, props);

		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin, SKSurfaceProperties props, bool shouldCreateWithMips) =>
			Create ((GRRecordingContext)context, budgeted, info, sampleCount, origin, props, false);

		public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info) =>
			Create (context, budgeted, info, 0, GRSurfaceOrigin.BottomLeft, null, false);

		public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info, int sampleCount) =>
			Create (context, budgeted, info, sampleCount, GRSurfaceOrigin.BottomLeft, null, false);

		public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin) =>
			Create (context, budgeted, info, sampleCount, origin, null, false);

		public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info, SKSurfaceProperties props) =>
			Create (context, budgeted, info, 0, GRSurfaceOrigin.BottomLeft, props, false);

		public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProperties props) =>
			Create (context, budgeted, info, sampleCount, GRSurfaceOrigin.BottomLeft, props, false);

		public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin, SKSurfaceProperties props, bool shouldCreateWithMips)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject (SkiaApi.sk_surface_new_render_target (context.Handle, budgeted, &cinfo, sampleCount, origin, props?.Handle ?? IntPtr.Zero, shouldCreateWithMips));
		}

#if __MACOS__ || __IOS__

		public static SKSurface Create (GRContext context, CoreAnimation.CAMetalLayer layer, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, out CoreAnimation.ICAMetalDrawable drawable) =>
			Create ((GRRecordingContext)context, layer, origin, sampleCount, colorType, out drawable);

		public static SKSurface Create (GRContext context, CoreAnimation.CAMetalLayer layer, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, out CoreAnimation.ICAMetalDrawable drawable) =>
			Create ((GRRecordingContext)context, layer, origin, sampleCount, colorType, colorspace, out drawable);

		public static SKSurface Create (GRContext context, CoreAnimation.CAMetalLayer layer, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props, out CoreAnimation.ICAMetalDrawable drawable) =>
			Create ((GRRecordingContext)context, layer, origin, sampleCount, colorType, colorspace, props, out drawable);

		public static SKSurface Create (GRRecordingContext context, CoreAnimation.CAMetalLayer layer, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, out CoreAnimation.ICAMetalDrawable drawable) =>
			Create (context, layer, origin, sampleCount, colorType, null, null, out drawable);

		public static SKSurface Create (GRRecordingContext context, CoreAnimation.CAMetalLayer layer, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, out CoreAnimation.ICAMetalDrawable drawable) =>
			Create (context, layer, origin, sampleCount, colorType, colorspace, null, out drawable);

		public static SKSurface Create (GRRecordingContext context, CoreAnimation.CAMetalLayer layer, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props, out CoreAnimation.ICAMetalDrawable drawable)
		{
			void* drawablePtr;
			var surface = GetObject (SkiaApi.sk_surface_new_metal_layer (context.Handle, (void*)(IntPtr)layer.Handle, origin, sampleCount, colorType.ToNative (), colorspace?.Handle ?? IntPtr.Zero, props?.Handle ?? IntPtr.Zero, &drawablePtr));
			drawable = ObjCRuntime.Runtime.GetINativeObject<CoreAnimation.ICAMetalDrawable> ((IntPtr)drawablePtr, true);
			return surface;
		}

		public static SKSurface Create (GRRecordingContext context, MetalKit.MTKView view, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType) =>
			Create (context, view, origin, sampleCount, colorType, null, null);

		public static SKSurface Create (GRRecordingContext context, MetalKit.MTKView view, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace) =>
			Create (context, view, origin, sampleCount, colorType, colorspace, null);

		public static SKSurface Create (GRRecordingContext context, MetalKit.MTKView view, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props) =>
			GetObject (SkiaApi.sk_surface_new_metal_view (context.Handle, (void*)(IntPtr)view.Handle, origin, sampleCount, colorType.ToNative (), colorspace?.Handle ?? IntPtr.Zero, props?.Handle ?? IntPtr.Zero));

#endif

		// NULL surface

		public static SKSurface CreateNull (int width, int height) =>
			GetObject (SkiaApi.sk_surface_new_null (width, height));

		//

		public SKCanvas Canvas =>
			OwnedBy (SKCanvas.GetObject (SkiaApi.sk_surface_get_canvas (Handle), false, unrefExisting: false), this);

		public SKSurfaceProperties SurfaceProperties =>
			OwnedBy (SKSurfaceProperties.GetObject (SkiaApi.sk_surface_get_props (Handle), false), this);

		public GRRecordingContext Context =>
			GRRecordingContext.GetObject (SkiaApi.sk_surface_get_recording_context (Handle), false, unrefExisting: false);

		public SKImage Snapshot () =>
			SKImage.GetObject (SkiaApi.sk_surface_new_image_snapshot (Handle));

		public SKImage Snapshot (SKRectI bounds) =>
			SKImage.GetObject (SkiaApi.sk_surface_new_image_snapshot_with_crop (Handle, &bounds));

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

			var result = SkiaApi.sk_surface_peek_pixels (Handle, pixmap.Handle);
			if (result)
				pixmap.pixelSource = this;
			return result;
		}

		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref dstInfo);
			var result = SkiaApi.sk_surface_read_pixels (Handle, &cinfo, (void*)dstPixels, (IntPtr)dstRowBytes, srcX, srcY);
			GC.KeepAlive (this);
			return result;
		}

		public void Flush () => Flush (true);

		public void Flush (bool submit, bool synchronous = false)
		{
			if (Context is not GRContext grContext)
				return;

			if (submit)
				grContext.Flush (submit, synchronous);
			else
				grContext.Flush ();
		}

		internal static SKSurface GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new SKSurface (handle, true);
	}
}
