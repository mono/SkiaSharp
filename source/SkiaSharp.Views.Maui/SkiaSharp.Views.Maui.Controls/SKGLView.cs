#nullable enable

using System;
using System.ComponentModel;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace SkiaSharp.Views.Maui.Controls
{
	/// <summary>
	/// A hardware-accelerated view that can be drawn on using SkiaSharp drawing commands.
	/// </summary>
	public partial class SKGLView : View, ISKGLView
	{
		private static readonly BindableProperty ProxyWindowProperty =
			BindableProperty.Create("ProxyWindow", typeof(Window), typeof(SKGLView), propertyChanged: OnWindowChanged);

		public static readonly BindableProperty IgnorePixelScalingProperty =
			BindableProperty.Create(nameof(IgnorePixelScaling), typeof(bool), typeof(SKGLView), false);

		/// <summary>
		/// Identifies the <see cref="SKGLView.HasRenderLoop" /> bindable property.
		/// </summary>
		public static readonly BindableProperty HasRenderLoopProperty =
			BindableProperty.Create(nameof(HasRenderLoop), typeof(bool), typeof(SKGLView), false);

		/// <summary>
		/// Implements the <see cref="SKGLView.EnableTouchEvents" /> property, and allows the <see cref="SKGLView" /> class to bind it to properties on other objects at run time.
		/// </summary>
		public static readonly BindableProperty EnableTouchEventsProperty =
			BindableProperty.Create(nameof(EnableTouchEvents), typeof(bool), typeof(SKGLView), false);

		private SKSizeI lastCanvasSize;
		private GRContext? lastGRContext;

		/// <summary>
		/// Creates a new instance of the <see cref="SKGLView" /> view.
		/// </summary>
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

		/// <summary>
		/// Gets or sets a value indicating whether the surface is drawn using a render loop.
		/// </summary>
		public bool HasRenderLoop
		{
			get => (bool)GetValue(HasRenderLoopProperty);
			set => SetValue(HasRenderLoopProperty, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether or not to enable touch events for this view.
		/// </summary>
		public bool EnableTouchEvents
		{
			get => (bool)GetValue(EnableTouchEventsProperty);
			set => SetValue(EnableTouchEventsProperty, value);
		}

		/// <summary>
		/// Occurs when the surface needs to be redrawn.
		/// </summary>
		/// <remarks>
		/// There are two ways to draw on this surface: by overriding the
		/// <see cref="SkiaSharp.Views.Maui.Controls.SKGLView.OnPaintSurface(SkiaSharp.Views.Maui.SKPaintGLSurfaceEventArgs)" />
		/// method, or by attaching a handler to the
		/// <see cref="SkiaSharp.Views.Maui.Controls.SKGLView.PaintSurface" />
		/// event.
		/// ## Examples
		/// ```csharp
		/// SKGLView myView = ...;
		/// myView.PaintSurface += (sender, e) => {
		/// var surface = e.Surface;
		/// var surfaceWidth = e.BackendRenderTarget.Width;
		/// var surfaceHeight = e.BackendRenderTarget.Height;
		/// var canvas = surface.Canvas;
		/// // draw on the canvas
		/// canvas.Flush ();
		/// };
		/// ```
		/// </remarks>
		public event EventHandler<SKPaintGLSurfaceEventArgs>? PaintSurface;

		/// <summary>
		/// Occurs when the the surface received a touch event.
		/// </summary>
		public event EventHandler<SKTouchEventArgs>? Touch;

		/// <summary>
		/// Gets the current canvas size.
		/// </summary>
		/// <remarks>
		/// The canvas size may be different to the view size as a result of the current device's pixel density.
		/// </remarks>
		public SKSize CanvasSize => lastCanvasSize;

		/// <summary>
		/// Gets the current GPU context.
		/// </summary>
		public GRContext? GRContext => lastGRContext;

		/// <summary>
		/// Informs the surface that it needs to redraw itself.
		/// </summary>
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
