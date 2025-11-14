using Android.Graphics;

namespace SkiaSharp.Views.Android
{
	/// <summary>
	/// A container for a locked canvas for a <see cref="SKSurfaceView" />.
	/// </summary>
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

		/// <summary>
		/// Gets information about the locked surface.
		/// </summary>
		public SKImageInfo ImageInfo { get; }

		/// <summary>
		/// Gets the locked surface.
		/// </summary>
		public SKSurface Surface { get; }

		/// <summary>
		/// Gets the canvas from the locked surface.
		/// </summary>
		public SKCanvas Canvas => Surface.Canvas;

		internal Canvas Post()
		{
			surfaceFactory.DrawSurface(Surface, canvas);
			return canvas;
		}
	}
}
