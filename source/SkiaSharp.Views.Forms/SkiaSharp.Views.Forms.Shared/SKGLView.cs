using System;
using Xamarin.Forms;

namespace SkiaSharp.Views.Forms
{
	[RenderWith(typeof(SKGLViewRenderer))]
	public class SKGLView : View, ISKGLViewController
	{
		public static readonly BindableProperty HasRenderLoopProperty =
			BindableProperty.Create("HasRenderLoop", typeof(bool), typeof(SKGLView), default(bool));

		public bool HasRenderLoop
		{
			get { return (bool)GetValue(HasRenderLoopProperty); }
			set { SetValue(HasRenderLoopProperty, value); }
		}

		// the user can subscribe to repaint
		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

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

		// the user asks to repaint
		public void InvalidateSurface()
		{
			// send a mesage to the native view
			SurfaceInvalidated?.Invoke(this, EventArgs.Empty);
		}

		// the native view tells the user to repaint
		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		// ISKViewController implementation

		event EventHandler ISKGLViewController.SurfaceInvalidated
		{
			add { SurfaceInvalidated += value; }
			remove { SurfaceInvalidated -= value; }
		}

		event EventHandler<GetCanvasSizeEventArgs> ISKGLViewController.GetCanvasSize
		{
			add { GetCanvasSize += value; }
			remove { GetCanvasSize -= value; }
		}

		void ISKGLViewController.OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			OnPaintSurface(e);
		}

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			return new SizeRequest(new Size(40.0, 40.0));
		}
	}

	internal interface ISKGLViewController : IViewController
	{
		// the native listens to this event
		event EventHandler SurfaceInvalidated;
		event EventHandler<GetCanvasSizeEventArgs> GetCanvasSize;

		// the native view tells the user to repaint
		void OnPaintSurface(SKPaintGLSurfaceEventArgs e);
	}
}
