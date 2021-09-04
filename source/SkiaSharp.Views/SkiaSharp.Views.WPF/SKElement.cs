﻿using System;
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
	[DefaultEvent("PaintSurface")]
	[DefaultProperty("Name")]
	public class SKElement : FrameworkElement
	{
		private readonly bool designMode;

		private WriteableBitmap bitmap;
		private bool ignorePixelScaling;

		public SKElement()
		{
			designMode = DesignerProperties.GetIsInDesignMode(this);
		}

		[Bindable(false)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public SKSize CanvasSize => bitmap == null ? SKSize.Empty : new SKSize(bitmap.PixelWidth, bitmap.PixelHeight);

		public bool IgnorePixelScaling
		{
			get { return ignorePixelScaling; }
			set
			{
				ignorePixelScaling = value;
				InvalidateVisual();
			}
		}

		[Category("Appearance")]
		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			if (designMode)
				return;

			if (Visibility != Visibility.Visible || PresentationSource.FromVisual(this) == null)
				return;

			var size = CreateSize(out var scaleX, out var scaleY);
			if (size.Width <= 0 || size.Height <= 0)
				return;

			var info = new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

			// reset the bitmap if the size has changed
			if (bitmap == null || info.Width != bitmap.PixelWidth || info.Height != bitmap.PixelHeight)
			{
				bitmap = new WriteableBitmap(info.Width, size.Height, 96 * scaleX, 96 * scaleY, PixelFormats.Pbgra32, null);
			}

			// draw on the bitmap
			bitmap.Lock();
			using (var surface = SKSurface.Create(info, bitmap.BackBuffer, bitmap.BackBufferStride))
			{
				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
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

		private SKSizeI CreateSize(out double scaleX, out double scaleY)
		{
			scaleX = 1.0;
			scaleY = 1.0;

			var w = ActualWidth;
			var h = ActualHeight;

			if (!IsPositive(w) || !IsPositive(h))
				return SKSizeI.Empty;

			if (IgnorePixelScaling)
				return new SKSizeI((int)w, (int)h);

			var m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
			scaleX = m.M11;
			scaleY = m.M22;
			return new SKSizeI((int)(w * scaleX), (int)(h * scaleY));

			bool IsPositive(double value)
			{
				return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
			}
		}
	}
}
