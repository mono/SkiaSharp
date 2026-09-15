#nullable enable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace SkiaSharp.Views.Maui.Controls
{
	/// <summary>A .NET MAUI view that can be used to draw 2D graphics using SkiaSharp with software (CPU) rendering.</summary>
	/// <remarks><para>The <see cref="T:SkiaSharp.Views.Maui.Controls.SKCanvasView" /> provides a software-rendered drawing surface for SkiaSharp graphics in .NET MAUI applications. For GPU-accelerated rendering, consider using <see cref="T:SkiaSharp.Views.Maui.Controls.SKGLView" /> instead.</para><para>Subscribe to the <see cref="E:SkiaSharp.Views.Maui.Controls.SKCanvasView.PaintSurface" /> event to draw custom graphics using the <see cref="T:SkiaSharp.SKCanvas" /> provided in the event arguments.</para><para>To request a redraw, call <see cref="M:SkiaSharp.Views.Maui.Controls.SKCanvasView.InvalidateSurface" />. The view also supports touch events when <see cref="P:SkiaSharp.Views.Maui.Controls.SKCanvasView.EnableTouchEvents" /> is set to <see langword="true" />.</para><para>The underlying platform views are:</para><list type="bullet"><item><description><b>Android</b>: <c>SKCanvasView</c> (Android)</description></item><item><description><b>iOS/Mac Catalyst/tvOS</b>: <c>SKCanvasView</c> (iOS)</description></item><item><description><b>Windows</b>: <c>SKXamlCanvas</c></description></item></list></remarks>
	public partial class SKCanvasView : View, ISKCanvasView
	{
		/// <summary>Identifies the <see cref="P:SkiaSharp.Views.Maui.Controls.SKCanvasView.IgnorePixelScaling" /> bindable property.</summary>
		/// <remarks />
		public static readonly BindableProperty IgnorePixelScalingProperty =
			BindableProperty.Create(nameof(IgnorePixelScaling), typeof(bool), typeof(SKCanvasView), false);

		/// <summary>Identifies the <see cref="P:SkiaSharp.Views.Maui.Controls.SKCanvasView.EnableTouchEvents" /> bindable property.</summary>
		/// <remarks />
		public static readonly BindableProperty EnableTouchEventsProperty =
			BindableProperty.Create(nameof(EnableTouchEvents), typeof(bool), typeof(SKCanvasView), false);

		private SKSizeI lastCanvasSize;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.Controls.SKCanvasView" /> class.</summary>
		/// <remarks />
		public SKCanvasView()
		{
		}

		/// <summary>Occurs when the surface needs to be painted.</summary>
		/// <remarks>Subscribe to this event to draw custom graphics using the <see cref="T:SkiaSharp.SKCanvas" /> provided in the <see cref="T:SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs" />.</remarks>
		public event EventHandler<SKPaintSurfaceEventArgs>? PaintSurface;

		/// <summary>Occurs when a touch event is detected on the view.</summary>
		/// <remarks>This event is only raised when <see cref="P:SkiaSharp.Views.Maui.Controls.SKCanvasView.EnableTouchEvents" /> is set to <see langword="true" />. Set <see cref="P:SkiaSharp.Views.Maui.SKTouchEventArgs.Handled" /> to <see langword="true" /> to indicate that the event has been handled.</remarks>
		public event EventHandler<SKTouchEventArgs>? Touch;

		/// <summary>Gets the current size of the canvas in pixels.</summary>
		/// <value>The size of the drawing canvas in pixels.</value>
		/// <remarks>This size accounts for the device's pixel density unless <see cref="P:SkiaSharp.Views.Maui.Controls.SKCanvasView.IgnorePixelScaling" /> is set to <see langword="true" />.</remarks>
		public SKSize CanvasSize => lastCanvasSize;

		/// <summary>Gets or sets a value indicating whether the canvas should ignore the device's pixel density scaling.</summary>
		/// <value><see langword="true" /> to ignore pixel scaling and use 1:1 pixel mapping; otherwise, <see langword="false" />. The default is <see langword="false" />.</value>
		/// <remarks>When <see langword="false" />, the canvas size is scaled by the device pixel density, resulting in a larger pixel canvas on high-DPI devices. When <see langword="true" />, the canvas size matches the view's logical size.</remarks>
		public bool IgnorePixelScaling
		{
			get => (bool)GetValue(IgnorePixelScalingProperty);
			set => SetValue(IgnorePixelScalingProperty, value);
		}

		/// <summary>Gets or sets a value indicating whether touch events are enabled for this view.</summary>
		/// <value><see langword="true" /> if touch events are enabled; otherwise, <see langword="false" />. The default is <see langword="false" />.</value>
		/// <remarks>When enabled, the <see cref="E:SkiaSharp.Views.Maui.Controls.SKCanvasView.Touch" /> event is raised for touch interactions.</remarks>
		public bool EnableTouchEvents
		{
			get => (bool)GetValue(EnableTouchEventsProperty);
			set => SetValue(EnableTouchEventsProperty, value);
		}

		/// <summary>Invalidates the surface and requests a redraw.</summary>
		/// <remarks>Call this method when you want to trigger a repaint of the canvas. This will cause the <see cref="E:SkiaSharp.Views.Maui.Controls.SKCanvasView.PaintSurface" /> event to be raised.</remarks>
		public void InvalidateSurface()
		{
			Handler?.Invoke(nameof(ISKCanvasView.InvalidateSurface));
		}

		/// <summary>Called when the surface needs to be painted.</summary>
		/// <param name="e">The event arguments containing the surface and canvas information.</param>
		/// <remarks>Override this method in a derived class to perform custom drawing. The base implementation raises the <see cref="E:SkiaSharp.Views.Maui.Controls.SKCanvasView.PaintSurface" /> event.</remarks>
		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		/// <summary>Called when a touch event occurs on the view.</summary>
		/// <param name="e">The event arguments containing touch information.</param>
		/// <remarks>Override this method in a derived class to handle touch events. The base implementation raises the <see cref="E:SkiaSharp.Views.Maui.Controls.SKCanvasView.Touch" /> event. Touch events are only raised when <see cref="P:SkiaSharp.Views.Maui.Controls.SKCanvasView.EnableTouchEvents" /> is <see langword="true" />.</remarks>
		protected virtual void OnTouch(SKTouchEventArgs e)
		{
			Touch?.Invoke(this, e);
		}

		/// <summary>This member is used internally by the handler and should not be called directly.</summary>
		/// <param name="size">The new canvas size in pixels.</param>
		/// <remarks />
		void ISKCanvasView.OnCanvasSizeChanged(SKSizeI size) =>
			lastCanvasSize = size;

		/// <summary>This member is used internally by the handler and should not be called directly.</summary>
		/// <param name="e">The paint surface event arguments.</param>
		/// <remarks />
		void ISKCanvasView.OnPaintSurface(SKPaintSurfaceEventArgs e) =>
			OnPaintSurface(e);

		/// <summary>This member is used internally by the handler and should not be called directly.</summary>
		/// <param name="e">The touch event arguments.</param>
		/// <remarks />
		void ISKCanvasView.OnTouch(SKTouchEventArgs e) =>
			OnTouch(e);
	}
}
