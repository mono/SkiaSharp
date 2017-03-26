using System;
using Xamarin.Forms;

namespace SkiaSharp.Views.Forms
{
	[RenderWith(typeof(SKCanvasViewRenderer))]
	public class SKCanvasView : View, ISKCanvasViewController
	{
		public static readonly BindableProperty IgnorePixelScalingProperty = 
			BindableProperty.Create(nameof(IgnorePixelScaling), typeof(bool), typeof(SKCanvasView), default(bool));

		// the user can subscribe to repaint
		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		// the native listens to this event
		private event EventHandler SurfaceInvalidated;
		private event EventHandler<GetCanvasSizeEventArgs> GetCanvasSize;

		// the user asks the for the size
		public SKSize CanvasSize
		{
			get
			{
				// send a mesage to the native view
				var args = new GetCanvasSizeEventArgs();
				GetCanvasSize?.Invoke(this, args);
				return args.CanvasSize;
			}
		}

		public bool IgnorePixelScaling
		{
			get { return (bool)GetValue(IgnorePixelScalingProperty); }
			set { SetValue(IgnorePixelScalingProperty, value); }
		}

		// the user asks to repaint
		public void InvalidateSurface()
		{
			// send a mesage to the native view
			SurfaceInvalidated?.Invoke(this, EventArgs.Empty);
		}

		// the native view tells the user to repaint
		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		// ISKViewController implementation

		event EventHandler ISKCanvasViewController.SurfaceInvalidated
		{
			add { SurfaceInvalidated += value; }
			remove { SurfaceInvalidated -= value; }
		}

		event EventHandler<GetCanvasSizeEventArgs> ISKCanvasViewController.GetCanvasSize
		{
			add { GetCanvasSize += value; }
			remove { GetCanvasSize -= value; }
		}

		void ISKCanvasViewController.OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			OnPaintSurface(e);
		}

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			return new SizeRequest(new Size(40.0, 40.0));
		}
	}

	internal interface ISKCanvasViewController : IViewController
	{
		// the native listens to this event
		event EventHandler SurfaceInvalidated;
		event EventHandler<GetCanvasSizeEventArgs> GetCanvasSize;

		// the native view tells the user to repaint
		void OnPaintSurface(SKPaintSurfaceEventArgs e);
	}
}
