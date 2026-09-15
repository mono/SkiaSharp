using Android.Graphics;

namespace SkiaSharp.Views.Android
{
	/// <summary>A container for a locked canvas for a <see cref="T:SkiaSharp.Views.Android.SKSurfaceView" />.</summary>
	/// <remarks />
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

		/// <summary>Gets information about the locked surface.</summary>
		/// <value>The information about the locked surface.</value>
		/// <remarks />
		public SKImageInfo ImageInfo { get; }

		/// <summary>Gets the locked surface.</summary>
		/// <value>The locked surface.</value>
		/// <remarks />
		public SKSurface Surface { get; }

		/// <summary>Gets the canvas from the locked surface.</summary>
		/// <value>The canvas from the locked surface.</value>
		/// <remarks />
		public SKCanvas Canvas => Surface.Canvas;

		internal Canvas Post()
		{
			surfaceFactory.DrawSurface(Surface, canvas);
			return canvas;
		}
	}
}
