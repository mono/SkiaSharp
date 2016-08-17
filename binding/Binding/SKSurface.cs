//
// Bindings for SKSurface
//
// Author:
//   Miguel de Icaza
//
// Copyright 2016 Xamarin Inc
//

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
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_raster (ref info, IntPtr.Zero));
		}

		public static SKSurface Create (SKImageInfo info, SKSurfaceProps props)
		{
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_raster (ref info, ref props));
		}

		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_raster_direct (ref info, pixels, (IntPtr)rowBytes, IntPtr.Zero));
		}

		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceProps props)
		{
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_raster_direct (ref info, pixels, (IntPtr)rowBytes, ref props));
		}
		
		public static SKSurface Create (GRContext context, GRBackendRenderTargetDesc desc, SKSurfaceProps props)
		{
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_backend_render_target (context.Handle, ref desc, ref props));
		}
		
		public static SKSurface Create (GRContext context, GRBackendRenderTargetDesc desc)
		{
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_backend_render_target (context.Handle, ref desc, IntPtr.Zero));
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
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_render_target (context.Handle, budgeted, ref info, sampleCount, ref props));
		}
		
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount)
		{
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_render_target (context.Handle, budgeted, ref info, sampleCount, IntPtr.Zero));
		}
		
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info)
		{
			return GetObject<SKSurface> (SkiaApi.sk_surface_new_render_target (context.Handle, budgeted, ref info, 0, IntPtr.Zero));
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

		public SKImage Snapshot ()
		{
			return GetObject<SKImage> (SkiaApi.sk_surface_new_image_snapshot (Handle));
		}
	}
}

