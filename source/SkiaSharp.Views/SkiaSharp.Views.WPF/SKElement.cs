using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF.OutputImage;

namespace SkiaSharp.Views.WPF
{
	[DefaultEvent("PaintSurface")]
	[DefaultProperty("Name")]
	public class SKElement : FrameworkElement
	{
		private static WaterfallContext context;
		private readonly bool designMode;

		private IOutputImage image;
		private bool ignorePixelScaling;

		public SKElement()
		{
			designMode = DesignerProperties.GetIsInDesignMode(this);
		}

		[Bindable(false)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public SKSize CanvasSize => image == null ? SKSize.Empty : new SKSize(image.Size.Width, image.Size.Height);

		[Bindable(false)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public GLMode Mode => context.Mode;

		[Bindable(false)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public GRContext? GRContext => context.GrContext;

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

			if (Visibility != Visibility.Visible)
				return;

			var size = CreateSize();
			if (size.Width <= 0 || size.Height <= 0)
				return;

			if (context == null)
			{
				context = new WaterfallContext();
				context.Initialize();
			}

			if (image == null)
			{
				image = context.CreateOutputImage(size);
			}
			else
			{
				image.TryResize(size);
			}


			var info = new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

			// draw on the bitmap
			if (image.TryLock())
			{
				using (var surface = image.CreateSurface(context))
				{
					OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
				}
			}

			if (context.IsGpuRendering)
			{
				context.GrContext?.Flush();
				OpenTK.Graphics.ES20.GL.Flush();
			}

			image.Unlock();

			drawingContext.DrawImage(image.Source, new Rect(0, 0, ActualWidth, ActualHeight));
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

		private SizeWithDpi CreateSize()
		{
			var w = ActualWidth;
			var h = ActualHeight;

			if (!IsPositive(w) || !IsPositive(h))
				return SizeWithDpi.Empty;

			if (IgnorePixelScaling)
				return new SizeWithDpi((int)w, (int)h);

			var m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
			return new SizeWithDpi((int)(w * m.M11), (int)(h * m.M22), 96.0 * m.M11, 96.0 * m.M22);

			bool IsPositive(double value)
			{
				return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
			}
		}
	}
}
