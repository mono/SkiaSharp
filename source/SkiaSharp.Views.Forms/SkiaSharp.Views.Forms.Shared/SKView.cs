using System;
using Xamarin.Forms;

namespace SkiaSharp.Views.Forms
{
	[RenderWith(typeof(SKViewRenderer))]
	public class SKView : View, ISKViewController
	{
		// the user can subscribe to repaint
		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		// the native listens to this event
		private event EventHandler SurfaceInvalidated;

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

		event EventHandler ISKViewController.SurfaceInvalidated
		{
			add { SurfaceInvalidated += value; }
			remove { SurfaceInvalidated -= value; }
		}

		void ISKViewController.OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			OnPaintSurface(e);
		}
	}

	internal interface ISKViewController : IViewController
	{
		// the native listens to this event
		event EventHandler SurfaceInvalidated;

		// the native view tells the user to repaint
		void OnPaintSurface(SKPaintSurfaceEventArgs e);
	}
}
