using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp.Views.Desktop;

namespace SkiaSharp.Views.WPF
{
	/// <summary>
	/// A visual element that can be drawn on using SkiaSharp drawing commands.
	/// </summary>
	[DefaultEvent("PaintSurface")]
	[DefaultProperty("Name")]
	public class SKElement : FrameworkElement
	{
		private const double BitmapDpi = 96.0;

		private readonly bool designMode;

		private WriteableBitmap bitmap;
		private bool ignorePixelScaling;

		/// <summary>
		/// Creates a new instance of the <see cref="SKElement" /> view.
		/// </summary>
		public SKElement()
		{
			designMode = DesignerProperties.GetIsInDesignMode(this);
		}

		/// <summary>
		/// Gets the current canvas size.
		/// </summary>
		/// <remarks>
		/// The canvas size may be different to the view size as a result of the current device's pixel density.
		/// </remarks>
		public SKSize CanvasSize { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether the drawing canvas should be resized on high resolution displays.
		/// </summary>
		/// <remarks>
		/// By default, when false, the canvas is resized to 1 canvas pixel per display pixel. When true, the canvas is resized to device independent pixels, and then stretched to fill the view. Although performance is improved and all objects are the same size on different display densities, blurring and pixelation may occur.
		/// </remarks>
		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				ignorePixelScaling = value;
				InvalidateVisual();
			}
		}

		/// <summary>
		/// Occurs when the the canvas needs to be redrawn.
		/// </summary>
		[Category("Appearance")]
		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			if (designMode)
				return;

			if (Visibility != Visibility.Visible || PresentationSource.FromVisual(this) == null)
				return;

			var size = CreateSize(out var unscaledSize, out var scaleX, out var scaleY);
			var userVisibleSize = IgnorePixelScaling ? unscaledSize : size;

			CanvasSize = userVisibleSize;

			if (size.Width <= 0 || size.Height <= 0)
				return;

			var info = new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

			// reset the bitmap if the size has changed
			if (bitmap == null || info.Width != bitmap.PixelWidth || info.Height != bitmap.PixelHeight)
			{
				bitmap = new WriteableBitmap(info.Width, size.Height, BitmapDpi * scaleX, BitmapDpi * scaleY, PixelFormats.Pbgra32, null);
			}

			// draw on the bitmap
			bitmap.Lock();
			using (var surface = SKSurface.Create(info, bitmap.BackBuffer, bitmap.BackBufferStride))
			{
				if (IgnorePixelScaling)
				{
					var canvas = surface.Canvas;
					canvas.Scale(scaleX, scaleY);
					canvas.Save();
				}

				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info.WithSize(userVisibleSize), info));
			}

			// draw the bitmap to the screen
			bitmap.AddDirtyRect(new Int32Rect(0, 0, info.Width, size.Height));
			bitmap.Unlock();
			drawingContext.DrawImage(bitmap, new Rect(0, 0, ActualWidth, ActualHeight));
		}

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);

			InvalidateVisual();
		}

		private SKSizeI CreateSize(out SKSizeI unscaledSize, out float scaleX, out float scaleY)
		{
			unscaledSize = SKSizeI.Empty;
			scaleX = 1.0f;
			scaleY = 1.0f;

			var w = ActualWidth;
			var h = ActualHeight;

			if (!IsPositive(w) || !IsPositive(h))
				return SKSizeI.Empty;

			unscaledSize = new SKSizeI((int)w, (int)h);

			var m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
			scaleX = (float)m.M11;
			scaleY = (float)m.M22;
			return new SKSizeI((int)(w * scaleX), (int)(h * scaleY));

			bool IsPositive(double value)
			{
				return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
			}
		}
	}
}
