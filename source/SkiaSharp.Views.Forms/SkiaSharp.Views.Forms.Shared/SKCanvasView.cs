#nullable enable

using System;

#if __MAUI__
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
#else
using Xamarin.Forms;
#endif

#if __MAUI__
namespace SkiaSharp.Views.Maui.Controls
#else
namespace SkiaSharp.Views.Forms
#endif
{
#if !__MAUI__
	[RenderWith(typeof(SKCanvasViewRenderer))]
#endif
	public partial class SKCanvasView : View, ISKCanvasViewController
	{
		public static readonly BindableProperty IgnorePixelScalingProperty =
			BindableProperty.Create(nameof(IgnorePixelScaling), typeof(bool), typeof(SKCanvasView), false);

		public static readonly BindableProperty EnableTouchEventsProperty =
			BindableProperty.Create(nameof(EnableTouchEvents), typeof(bool), typeof(SKCanvasView), false);

		// the user can subscribe to repaint
		public event EventHandler<SKPaintSurfaceEventArgs>? PaintSurface;

		// the user can subscribe to touch events
		public event EventHandler<SKTouchEventArgs>? Touch;

		// the native listens to this event
		private event EventHandler? SurfaceInvalidated;
		private event EventHandler<GetPropertyValueEventArgs<SKSize>>? GetCanvasSize;

		// the user asks the for the size
		public SKSize CanvasSize
		{
			get
			{
				// send a mesage to the native view
				var args = new GetPropertyValueEventArgs<SKSize>();
				GetCanvasSize?.Invoke(this, args);
				return args.Value;
			}
		}

		public bool IgnorePixelScaling
		{
			get { return (bool)GetValue(IgnorePixelScalingProperty); }
			set { SetValue(IgnorePixelScalingProperty, value); }
		}

		public bool EnableTouchEvents
		{
			get { return (bool)GetValue(EnableTouchEventsProperty); }
			set { SetValue(EnableTouchEventsProperty, value); }
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

		// the native view responds to a touch
		protected virtual void OnTouch(SKTouchEventArgs e)
		{
			Touch?.Invoke(this, e);
		}

		// ISKViewController implementation

		event EventHandler ISKCanvasViewController.SurfaceInvalidated
		{
			add { SurfaceInvalidated += value; }
			remove { SurfaceInvalidated -= value; }
		}

		event EventHandler<GetPropertyValueEventArgs<SKSize>> ISKCanvasViewController.GetCanvasSize
		{
			add { GetCanvasSize += value; }
			remove { GetCanvasSize -= value; }
		}

		void ISKCanvasViewController.OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			OnPaintSurface(e);
		}

		void ISKCanvasViewController.OnTouch(SKTouchEventArgs e)
		{
			OnTouch(e);
		}

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			return new SizeRequest(new Size(40.0, 40.0));
		}
	}

	public interface ISKCanvasViewController : IViewController
	{
		// the native listens to this event
		event EventHandler SurfaceInvalidated;
		event EventHandler<GetPropertyValueEventArgs<SKSize>> GetCanvasSize;

		// the native view tells the user to repaint
		void OnPaintSurface(SKPaintSurfaceEventArgs e);

		// the native view responds to a touch
		void OnTouch(SKTouchEventArgs e);
	}
}
