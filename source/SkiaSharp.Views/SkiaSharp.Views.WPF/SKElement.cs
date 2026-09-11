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
	/// <summary>A visual element that can be drawn on using SkiaSharp drawing commands.</summary>
	/// <remarks />
	[DefaultEvent("PaintSurface")]
	[DefaultProperty("Name")]
	public class SKElement : FrameworkElement
	{
		private const double BitmapDpi = 96.0;

		private readonly bool designMode;

		private WriteableBitmap bitmap;
		private bool ignorePixelScaling;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.WPF.SKElement" /> class.</summary>
		/// <remarks />
		public SKElement()
		{
			designMode = DesignerProperties.GetIsInDesignMode(this);
		}

		/// <summary>Gets the current canvas size.</summary>
		/// <value>The size of the canvas in pixels.</value>
		/// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
		public SKSize CanvasSize { get; private set; }

		/// <summary>Gets or sets a value indicating whether the drawing canvas should be resized on high resolution displays.</summary>
		/// <value><see langword="true" /> to ignore pixel scaling; otherwise, <see langword="false" />.</value>
		/// <remarks>By default, when false, the canvas is resized to 1 canvas pixel per display pixel. When true, the canvas is resized to device independent pixels, and then stretched to fill the view. Although performance is improved and all objects are the same size on different display densities, blurring and pixelation may occur.</remarks>
		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				ignorePixelScaling = value;
				InvalidateVisual();
			}
		}

		/// <summary>Occurs when the canvas needs to be redrawn.</summary>
		/// <remarks />
		[Category("Appearance")]
		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		/// <summary>When overridden in a derived class, participates in rendering operations that are directed by the layout system. The rendering instructions for this element are not used directly when this method is invoked, and are instead preserved for later asynchronous use by layout and drawing.</summary>
		/// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
		/// <remarks />
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

		/// <summary>Implement this to draw on the canvas.</summary>
		/// <param name="e">The event arguments that contain the drawing surface and information.</param>
		/// <remarks />
		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		/// <summary>Raises the SizeChanged event, using the specified information as part of the eventual event data.</summary>
		/// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
		/// <remarks />
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
