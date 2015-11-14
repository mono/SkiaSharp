//
// Bindings for SKSurface
//
// Author:
//   Miguel de Icaza
//
// Copyright 2015 Xamarin Inc
//

using System;

namespace SkiaSharp
{
	public class SKSurface : IDisposable
	{
		internal IntPtr handle;
		public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType) => Create (new SKImageInfo (width, height, colorType, alphaType));
		public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, SKSurfaceProps props) => Create (new SKImageInfo (width, height, colorType, alphaType), props);
		public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes) => Create (new SKImageInfo (width, height, colorType, alphaType), pixels, rowBytes);
		public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes, SKSurfaceProps props) => Create (new SKImageInfo (width, height, colorType, alphaType), pixels, rowBytes, props);

		SKSurface (IntPtr h)
		{
			handle = h;
		}

		static SKSurface FromHandle (IntPtr h)
		{
			if (h == IntPtr.Zero)
				return null;
			return new SKSurface (h);
		}
		
		public static SKSurface Create (SKImageInfo info)
		{
			return FromHandle (SkiaApi.sk_surface_new_raster (ref info, IntPtr.Zero));
		}

		public static SKSurface Create (SKImageInfo info, SKSurfaceProps props)
		{
			return FromHandle (SkiaApi.sk_surface_new_raster (ref info, ref props));
		}

		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			return (FromHandle (SkiaApi.sk_surface_new_raster_direct (ref info, pixels, (IntPtr)rowBytes, IntPtr.Zero)));
		}

		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceProps props)
		{
			return (FromHandle (SkiaApi.sk_surface_new_raster_direct (ref info, pixels, (IntPtr)rowBytes, ref props)));
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero) {
				SkiaApi.sk_surface_unref (handle);
				handle = IntPtr.Zero;
				// We set this in case the user tries to use the fetched Canvas (which depends on us) to perform some operations
				canvas.handle = IntPtr.Zero;
			}
		}

		~SKSurface()
		{
			Dispose (false);
		}

		SKCanvas canvas;

		public SKCanvas Canvas {
			get {
				if (canvas == null)
					canvas = new SKCanvas (SkiaApi.sk_surface_get_canvas (handle));
				return canvas;
			}
		}

		public SKImage Snapshot ()
		{
			return new SKImage (SkiaApi.sk_surface_new_image_snapshot (handle));
		}
	}
}

