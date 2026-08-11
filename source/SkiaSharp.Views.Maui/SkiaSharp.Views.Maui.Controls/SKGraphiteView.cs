#nullable enable

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SkiaSharp.Views.Maui.Controls
{
	public class SKGraphiteView : View, ISKGraphiteView
	{
		private static readonly BindableProperty ProxyWindowProperty =
			BindableProperty.Create (
				"ProxyWindow",
				typeof (Window),
				typeof (SKGraphiteView),
				propertyChanged: OnWindowChanged);

		public static readonly BindableProperty IgnorePixelScalingProperty =
			BindableProperty.Create (
				nameof (IgnorePixelScaling),
				typeof (bool),
				typeof (SKGraphiteView),
				false);

		public static readonly BindableProperty HasRenderLoopProperty =
			BindableProperty.Create (
				nameof (HasRenderLoop),
				typeof (bool),
				typeof (SKGraphiteView),
				false);

		public static readonly BindableProperty EnableTouchEventsProperty =
			BindableProperty.Create (
				nameof (EnableTouchEvents),
				typeof (bool),
				typeof (SKGraphiteView),
				false);

		private SKSizeI lastCanvasSize;
		private SKGraphiteContext? lastGraphiteContext;

		public SKGraphiteView ()
		{
			var binding = new Binding (nameof (Window), source: this);
			SetBinding (ProxyWindowProperty, binding);
		}

		public bool IgnorePixelScaling
		{
			get => (bool)GetValue (IgnorePixelScalingProperty);
			set => SetValue (IgnorePixelScalingProperty, value);
		}

		public bool HasRenderLoop
		{
			get => (bool)GetValue (HasRenderLoopProperty);
			set => SetValue (HasRenderLoopProperty, value);
		}

		public bool EnableTouchEvents
		{
			get => (bool)GetValue (EnableTouchEventsProperty);
			set => SetValue (EnableTouchEventsProperty, value);
		}

		public SKSize CanvasSize => lastCanvasSize;

		public SKGraphiteContext? GraphiteContext => lastGraphiteContext;

		public SKGraphiteBackend Backend =>
			lastGraphiteContext?.Backend ?? SKGraphiteBackend.Unknown;

		public event EventHandler<SKPaintGraphiteSurfaceEventArgs>? PaintSurface;

		public event EventHandler<SKGraphiteRenderFailedEventArgs>? RenderFailed;

		public event EventHandler<SKTouchEventArgs>? Touch;

		public void InvalidateSurface () =>
			Handler?.Invoke (nameof (ISKGraphiteView.InvalidateSurface));

		protected virtual void OnPaintSurface (SKPaintGraphiteSurfaceEventArgs e) =>
			PaintSurface?.Invoke (this, e);

		protected virtual void OnRenderFailed (Exception exception)
		{
			if (RenderFailed is null)
				throw new InvalidOperationException ("Graphite rendering failed.", exception);
			RenderFailed.Invoke (this, new SKGraphiteRenderFailedEventArgs (exception));
		}

		protected virtual void OnTouch (SKTouchEventArgs e) =>
			Touch?.Invoke (this, e);

		bool ISKGraphiteView.HasRenderLoop =>
			HasRenderLoop && Window is not null;

		void ISKGraphiteView.OnCanvasSizeChanged (SKSizeI size) =>
			lastCanvasSize = size;

		void ISKGraphiteView.OnGraphiteContextChanged (SKGraphiteContext? context) =>
			lastGraphiteContext = context;

		void ISKGraphiteView.OnPaintSurface (SKPaintGraphiteSurfaceEventArgs e) =>
			OnPaintSurface (e);

		void ISKGraphiteView.OnRenderFailed (Exception exception) =>
			OnRenderFailed (exception);

		void ISKGraphiteView.OnTouch (SKTouchEventArgs e) =>
			OnTouch (e);

		private static void OnWindowChanged (
			BindableObject bindable,
			object oldValue,
			object newValue)
		{
			if (bindable is SKGraphiteView view)
				view.Handler?.UpdateValue (nameof (HasRenderLoop));
		}
	}
}
