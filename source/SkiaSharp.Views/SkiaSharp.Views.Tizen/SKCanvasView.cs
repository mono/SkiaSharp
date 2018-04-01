using ElmSharp;
using System;

namespace SkiaSharp.Views.Tizen
{
	/// <summary>
	/// Software rendering for Skia.
	/// </summary>
	public class SKCanvasView : CustomRenderingView
	{
		/// <summary>
		/// Backing field for IgnorePixelScaling.
		/// </summary>
		private bool ignorePixelScaling;

		/// <summary>
		/// Information about the surface.
		/// </summary>
		private SKImageInfo info = new SKImageInfo(0, 0, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

		/// <summary>
		/// Creates new instance with the given object as its parent.
		/// </summary>
		/// <param name="parent">The parent object.</param>
		public SKCanvasView(EvasObject parent) : base(parent)
		{
		}

		/// <summary>
		/// If set to true, the surface is resized to device independent pixels, and then stretched to fill the view.
		/// If set to false, the surface is resized to 1 canvas pixel per display pixel.
		/// </summary>
		/// <remarks>
		/// Default value is false.
		/// </remarks>
		public bool IgnorePixelScaling
		{
			get
			{
				return ignorePixelScaling;
			}

			set
			{
				if (ignorePixelScaling != value)
				{
					ignorePixelScaling = value;
					OnSurfaceSizeChanged();
				}
			}
		}

		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected sealed override int SurfaceWidth => info.Width;

		protected sealed override int SurfaceHeight => info.Height;

		protected virtual void OnDrawFrame(SKSurface surface, SKImageInfo info)
		{
			PaintSurface?.Invoke(this, new SKPaintSurfaceEventArgs(surface, info));
		}

		protected sealed override void OnDrawFrame()
		{
			// draw directly into the EFL image data
			using (var surface = SKSurface.Create(info, Interop.Evas.Image.evas_object_image_data_get(EvasImage, true), info.RowBytes))
			{
				// draw using SkiaSharp
				OnDrawFrame(surface, info);
				surface.Canvas.Flush();
			}
		}

		protected sealed override bool UpdateSurfaceSize(Rect geometry)
		{
			var w = info.Width;
			var h = info.Height;

			if (IgnorePixelScaling)
			{
				info.Width = (int)ScalingInfo.FromPixel(geometry.Width);
				info.Height = (int)ScalingInfo.FromPixel(geometry.Height);
			}
			else
			{
				info.Width = geometry.Width;
				info.Height = geometry.Height;
			}

			return (w != info.Width || h != info.Height);
		}
	}
}
