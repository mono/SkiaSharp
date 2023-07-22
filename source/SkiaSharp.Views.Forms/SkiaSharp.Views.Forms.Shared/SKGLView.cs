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
	[RenderWith(typeof(SKGLViewRenderer))]
#endif
	public partial class SKGLView : View, ISKGLViewController
	{
		public static readonly BindableProperty HasRenderLoopProperty =
			BindableProperty.Create(nameof(HasRenderLoop), typeof(bool), typeof(SKGLView), false);

		public static readonly BindableProperty EnableTouchEventsProperty =
			BindableProperty.Create(nameof(EnableTouchEvents), typeof(bool), typeof(SKGLView), false);

		public bool HasRenderLoop
		{
			get => (bool)GetValue(HasRenderLoopProperty);
			set => SetValue(HasRenderLoopProperty, value);
		}

		public bool EnableTouchEvents
		{
			get => (bool)GetValue(EnableTouchEventsProperty);
			set => SetValue(EnableTouchEventsProperty, value);
		}

		// the user can subscribe to repaint
		public event EventHandler<SKPaintGLSurfaceEventArgs>? PaintSurface;

		// the user can subscribe to touch events
		public event EventHandler<SKTouchEventArgs>? Touch;

		// the native listens to this event
		private event EventHandler? SurfaceInvalidated;
		private event EventHandler<GetPropertyValueEventArgs<SKSize>>? GetCanvasSize;
		private event EventHandler<GetPropertyValueEventArgs<GRContext>>? GetGRContext;

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

		// the user asks the for the current GRContext
		public GRContext GRContext
		{
			get
			{
				// send a mesage to the native view
				var args = new GetPropertyValueEventArgs<GRContext>();
				GetGRContext?.Invoke(this, args);
				return args.Value;
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

		// the native view responds to a touch
		protected virtual void OnTouch(SKTouchEventArgs e)
		{
			Touch?.Invoke(this, e);
		}

		// ISKViewController implementation

		event EventHandler ISKGLViewController.SurfaceInvalidated
		{
			add { SurfaceInvalidated += value; }
			remove { SurfaceInvalidated -= value; }
		}

		event EventHandler<GetPropertyValueEventArgs<SKSize>> ISKGLViewController.GetCanvasSize
		{
			add { GetCanvasSize += value; }
			remove { GetCanvasSize -= value; }
		}

		event EventHandler<GetPropertyValueEventArgs<GRContext>> ISKGLViewController.GetGRContext
		{
			add { GetGRContext += value; }
			remove { GetGRContext -= value; }
		}

		void ISKGLViewController.OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			OnPaintSurface(e);
		}

		void ISKGLViewController.OnTouch(SKTouchEventArgs e)
		{
			OnTouch(e);
		}

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			return new SizeRequest(new Size(40.0, 40.0));
		}
	}

	public interface ISKGLViewController : IViewController
	{
		// the native listens to this event
		event EventHandler SurfaceInvalidated;
		event EventHandler<GetPropertyValueEventArgs<SKSize>> GetCanvasSize;
		event EventHandler<GetPropertyValueEventArgs<GRContext>> GetGRContext;

		// the native view tells the user to repaint
		void OnPaintSurface(SKPaintGLSurfaceEventArgs e);

		// the native view responds to a touch
		void OnTouch(SKTouchEventArgs e);
	}
}
