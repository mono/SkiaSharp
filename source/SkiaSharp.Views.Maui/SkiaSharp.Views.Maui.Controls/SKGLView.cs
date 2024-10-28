#nullable enable

using System;
using System.ComponentModel;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace SkiaSharp.Views.Maui.Controls
{
	public partial class SKGLView : View, ISKGLView
	{
		private static readonly BindableProperty ProxyWindowProperty =
			BindableProperty.Create("ProxyWindow", typeof(Window), typeof(SKGLView), propertyChanged: OnWindowChanged);

		public static readonly BindableProperty IgnorePixelScalingProperty =
			BindableProperty.Create(nameof(IgnorePixelScaling), typeof(bool), typeof(SKGLView), false);

		public static readonly BindableProperty HasRenderLoopProperty =
			BindableProperty.Create(nameof(HasRenderLoop), typeof(bool), typeof(SKGLView), false);

		public static readonly BindableProperty EnableTouchEventsProperty =
			BindableProperty.Create(nameof(EnableTouchEvents), typeof(bool), typeof(SKGLView), false);

		private SKSizeI lastCanvasSize;
		private GRContext? lastGRContext;

		public SKGLView()
		{
			var binding = new Binding(nameof(Window), source: this);
			SetBinding(ProxyWindowProperty, binding);
		}

		public bool IgnorePixelScaling
		{
			get => (bool)GetValue(IgnorePixelScalingProperty);
			set => SetValue(IgnorePixelScalingProperty, value);
		}

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

		public event EventHandler<SKPaintGLSurfaceEventArgs>? PaintSurface;

		public event EventHandler<SKTouchEventArgs>? Touch;

		public SKSize CanvasSize => lastCanvasSize;

		public GRContext? GRContext => lastGRContext;

		public void InvalidateSurface()
		{
			Handler?.Invoke(nameof(ISKGLView.InvalidateSurface));
		}

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		protected virtual void OnTouch(SKTouchEventArgs e)
		{
			Touch?.Invoke(this, e);
		}

		private static void OnWindowChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is not SKGLView view)
				return;

			view.Handler?.UpdateValue(nameof(HasRenderLoop));
		}

		bool ISKGLView.HasRenderLoop =>
			HasRenderLoop && Window is not null;

		void ISKGLView.OnCanvasSizeChanged(SKSizeI size) =>
			lastCanvasSize = size;

		void ISKGLView.OnGRContextChanged(GRContext? context) =>
			lastGRContext = context;

		void ISKGLView.OnPaintSurface(SKPaintGLSurfaceEventArgs e) =>
			OnPaintSurface(e);

		void ISKGLView.OnTouch(SKTouchEventArgs e) =>
			OnTouch(e);
	}
}
