#nullable enable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace SkiaSharp.Views.Maui.Controls
{
	public partial class SKCanvasView : View, ISKCanvasView
	{
		public static readonly BindableProperty IgnorePixelScalingProperty =
			BindableProperty.Create(nameof(IgnorePixelScaling), typeof(bool), typeof(SKCanvasView), false);

		public static readonly BindableProperty EnableTouchEventsProperty =
			BindableProperty.Create(nameof(EnableTouchEvents), typeof(bool), typeof(SKCanvasView), false);

		private SKSizeI lastCanvasSize;

		public SKCanvasView()
		{
		}

		public event EventHandler<SKPaintSurfaceEventArgs>? PaintSurface;

		public event EventHandler<SKTouchEventArgs>? Touch;

		public SKSize CanvasSize => lastCanvasSize;

		public bool IgnorePixelScaling
		{
			get => (bool)GetValue(IgnorePixelScalingProperty);
			set => SetValue(IgnorePixelScalingProperty, value);
		}

		public bool EnableTouchEvents
		{
			get => (bool)GetValue(EnableTouchEventsProperty);
			set => SetValue(EnableTouchEventsProperty, value);
		}

		public void InvalidateSurface()
		{
			Handler?.Invoke(nameof(ISKCanvasView.InvalidateSurface));
		}

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		protected virtual void OnTouch(SKTouchEventArgs e)
		{
			Touch?.Invoke(this, e);
		}

		void ISKCanvasView.OnCanvasSizeChanged(SKSizeI size) =>
			lastCanvasSize = size;

		void ISKCanvasView.OnPaintSurface(SKPaintSurfaceEventArgs e) =>
			OnPaintSurface(e);

		void ISKCanvasView.OnTouch(SKTouchEventArgs e) =>
			OnTouch(e);
	}
}
