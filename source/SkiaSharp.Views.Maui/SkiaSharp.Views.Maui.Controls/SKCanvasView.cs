#nullable enable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace SkiaSharp.Views.Maui.Controls
{
	/// <summary>
	/// A view that can be drawn on using SkiaSharp drawing commands.
	/// </summary>
	public partial class SKCanvasView : View, ISKCanvasView
	{
		/// <summary>
		/// Implements the <see cref="SKCanvasView.IgnorePixelScaling" /> property, and allows the <see cref="SKCanvasView" /> class to bind it to properties on other objects at run time.
		/// </summary>
		public static readonly BindableProperty IgnorePixelScalingProperty =
			BindableProperty.Create(nameof(IgnorePixelScaling), typeof(bool), typeof(SKCanvasView), false);

		/// <summary>
		/// Implements the <see cref="SKCanvasView.EnableTouchEvents" /> property, and allows the <see cref="SKCanvasView" /> class to bind it to properties on other objects at run time.
		/// </summary>
		public static readonly BindableProperty EnableTouchEventsProperty =
			BindableProperty.Create(nameof(EnableTouchEvents), typeof(bool), typeof(SKCanvasView), false);

		private SKSizeI lastCanvasSize;

		/// <summary>
		/// Creates a new instance of the <see cref="SKCanvasView" /> view.
		/// </summary>
		public SKCanvasView()
		{
		}

		/// <summary>
		/// Occurs when the canvas needs to be redrawn.
		/// </summary>
		/// <remarks>
		/// <para>
		/// There are two ways to draw on this surface: by overriding the
		/// <see cref="SkiaSharp.Views.Maui.Controls.SKCanvasView.OnPaintSurface(SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs)" />
		/// method, or by attaching a handler to the
		/// <see cref="SkiaSharp.Views.Maui.Controls.SKCanvasView.PaintSurface" />
		/// event.
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="csharp">
		/// myView.PaintSurface += (sender, e) => 
		/// {
		///     var surface = e.Surface;
		///     var surfaceWidth = e.Info.Width;
		///     var surfaceHeight = e.Info.Height;
		///     var canvas = surface.Canvas;
		/// 
		///     // draw on the canvas
		/// };
		/// </code>
		/// </example>
		public event EventHandler<SKPaintSurfaceEventArgs>? PaintSurface;

		/// <summary>
		/// Occurs when the the canvas received a touch event.
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
		/// Gets or sets a value indicating whether the drawing canvas should be resized on high resolution displays.
		/// </summary>
		/// <remarks>
		/// By default, when false, the canvas is resized to 1 canvas pixel per display pixel. When true, the canvas is resized to device independent pixels, and then stretched to fill the view. Although performance is improved and all objects are the same size on different display densities, blurring and pixelation may occur.
		/// </remarks>
		public bool IgnorePixelScaling
		{
			get => (bool)GetValue(IgnorePixelScalingProperty);
			set => SetValue(IgnorePixelScalingProperty, value);
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
		/// Informs the canvas that it needs to redraw itself.
		/// </summary>
		/// <remarks>
		/// This needs to be called from the main thread.
		/// </remarks>
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
