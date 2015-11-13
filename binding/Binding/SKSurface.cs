using System;

namespace SkiaSharp
{
	public class SKSurface : IDisposable
	{
		internal IntPtr handle;
		public SKSurface (int width, int height, SKColorType colorType, SKAlphaType alphaType) : this (new SKImageInfo (width, height, colorType, alphaType)) {}
		public SKSurface (int width, int height, SKColorType colorType, SKAlphaType alphaType, SKSurfaceProps props) : this (new SKImageInfo (width, height, colorType, alphaType), props) {}
		public SKSurface (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes) : this (new SKImageInfo (width, height, colorType, alphaType), pixels, rowBytes) {}
		public SKSurface (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes, SKSurfaceProps props) : this (new SKImageInfo (width, height, colorType, alphaType), pixels, rowBytes, props) {}

		public SKSurface (SKImageInfo info)
		{
			handle = SkiaApi.sk_surface_new_raster (ref info, IntPtr.Zero);
			if (handle == IntPtr.Zero)
				throw new InvalidOperationException ();
		}

		public SKSurface (SKImageInfo info, SKSurfaceProps props)
		{
			handle = SkiaApi.sk_surface_new_raster (ref info, ref props);
			if (handle == IntPtr.Zero)
				throw new InvalidOperationException ();
		}

		public SKSurface (SKImageInfo info, IntPtr pixels, int rowBytes)
		{
			handle = SkiaApi.sk_surface_new_raster_direct (ref info, pixels, (IntPtr)rowBytes, IntPtr.Zero);
			if (handle == IntPtr.Zero)
				throw new InvalidOperationException ();
		}

		public SKSurface (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceProps props)
		{
			handle = SkiaApi.sk_surface_new_raster_direct (ref info, pixels, (IntPtr)rowBytes, ref props);
			if (handle == IntPtr.Zero)
				throw new InvalidOperationException ();
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
				Console.WriteLine ("got: {0:x}", (long)handle);
				if (canvas == null)
					canvas = new SKCanvas (this, SkiaApi.sk_surface_get_canvas (handle));
				return canvas;
			}
		}
	}
}

