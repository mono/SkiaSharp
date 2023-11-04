using Android.Graphics;

namespace SkiaSharp.Views.Android
{
	public class SKLockedSurface
	{
		private readonly Canvas canvas;
		private readonly SurfaceFactory surfaceFactory;

		internal SKLockedSurface(Canvas canvas, SurfaceFactory surfaceFactory)
		{
			this.canvas = canvas;
			this.surfaceFactory = surfaceFactory;

			// create a surface
			Surface = surfaceFactory.CreateSurface(out var info);
			ImageInfo = info;
		}

		public SKImageInfo ImageInfo { get; }

		public SKSurface Surface { get; }

		public SKCanvas Canvas => Surface.Canvas;

		internal Canvas Post()
		{
			surfaceFactory.DrawSurface(Surface, canvas);
			return canvas;
		}
	}
}
