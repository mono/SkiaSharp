#nullable enable

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SkiaSharp.Views.Maui.Controls
{
	/// <summary>A .NET MAUI view that uses the Ganesh GPU backend for hardware-accelerated 2D graphics rendering with SkiaSharp.</summary>
	/// <remarks><para>The <see cref="T:SkiaSharp.Views.Maui.Controls.SKGaneshView" /> provides a Ganesh GPU-accelerated drawing surface for SkiaSharp graphics in .NET MAUI applications.</para><para>Subscribe to the <see cref="E:SkiaSharp.Views.Maui.Controls.SKGaneshView.PaintSurface" /> event to draw custom graphics. Set <see cref="P:SkiaSharp.Views.Maui.Controls.SKGaneshView.HasRenderLoop" /> to <see langword="true" /> for continuous rendering (e.g., for animations).</para></remarks>
	public partial class SKGaneshView : View, ISKGaneshView
	{
		private static readonly BindableProperty ProxyWindowProperty =
			BindableProperty.Create("ProxyWindow", typeof(Window), typeof(SKGaneshView), propertyChanged: OnWindowChanged);

		/// <summary>Identifies the <see cref="P:SkiaSharp.Views.Maui.Controls.SKGaneshView.IgnorePixelScaling" /> bindable property.</summary>
		/// <remarks />
		public static readonly BindableProperty IgnorePixelScalingProperty =
			BindableProperty.Create(nameof(IgnorePixelScaling), typeof(bool), typeof(SKGaneshView), false);

		/// <summary>Identifies the <see cref="P:SkiaSharp.Views.Maui.Controls.SKGaneshView.HasRenderLoop" /> bindable property.</summary>
		/// <remarks />
		public static readonly BindableProperty HasRenderLoopProperty =
			BindableProperty.Create(nameof(HasRenderLoop), typeof(bool), typeof(SKGaneshView), false);

		/// <summary>Identifies the <see cref="P:SkiaSharp.Views.Maui.Controls.SKGaneshView.EnableTouchEvents" /> bindable property.</summary>
		/// <remarks />
		public static readonly BindableProperty EnableTouchEventsProperty =
			BindableProperty.Create(nameof(EnableTouchEvents), typeof(bool), typeof(SKGaneshView), false);

		private SKSizeI lastCanvasSize;
		private GRContext? lastGRContext;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Maui.Controls.SKGaneshView" /> class.</summary>
		/// <remarks />
		public SKGaneshView()
		{
			var binding = new Binding(nameof(Window), source: this);
			SetBinding(ProxyWindowProperty, binding);
		}

		/// <summary>Gets or sets a value indicating whether the view should ignore the device's pixel scaling.</summary>
		/// <value><see langword="true" /> if pixel scaling is ignored and the canvas uses 1:1 pixel mapping; otherwise, <see langword="false" />. The default is <see langword="false" />.</value>
		/// <remarks>When <see langword="false" />, the canvas is scaled to match the device's DPI, resulting in a canvas size larger than the view's size on high-DPI displays.</remarks>
		public bool IgnorePixelScaling
		{
			get => (bool)GetValue(IgnorePixelScalingProperty);
			set => SetValue(IgnorePixelScalingProperty, value);
		}

		/// <summary>Gets or sets a value indicating whether the view should continuously render frames.</summary>
		/// <value><see langword="true" /> if the render loop is enabled; otherwise, <see langword="false" />. The default is <see langword="false" />.</value>
		/// <remarks>When enabled, the <see cref="E:SkiaSharp.Views.Maui.Controls.SKGaneshView.PaintSurface" /> event is raised continuously for each frame, suitable for animations. When disabled, rendering only occurs when <see cref="M:SkiaSharp.Views.Maui.Controls.SKGaneshView.InvalidateSurface" /> is called.</remarks>
		public bool HasRenderLoop
		{
			get => (bool)GetValue(HasRenderLoopProperty);
			set => SetValue(HasRenderLoopProperty, value);
		}

		/// <summary>Gets or sets a value indicating whether touch events are enabled for this view.</summary>
		/// <value><see langword="true" /> if touch events are enabled; otherwise, <see langword="false" />. The default is <see langword="false" />.</value>
		/// <remarks>When enabled, the <see cref="E:SkiaSharp.Views.Maui.Controls.SKGaneshView.Touch" /> event is raised for touch interactions.</remarks>
		public bool EnableTouchEvents
		{
			get => (bool)GetValue(EnableTouchEventsProperty);
			set => SetValue(EnableTouchEventsProperty, value);
		}

		/// <summary>Occurs when the view needs to be painted.</summary>
		/// <remarks>Subscribe to this event to perform custom drawing on the Ganesh GPU-accelerated surface. The event arguments provide access to the <see cref="T:SkiaSharp.SKSurface" /> for drawing, the <see cref="T:SkiaSharp.GRContext" />, and the <see cref="T:SkiaSharp.GRBackendRenderTarget" /> for render target information.</remarks>
		public event EventHandler<SKPaintGaneshSurfaceEventArgs>? PaintSurface;

		/// <summary>Occurs when the view receives a touch event.</summary>
		/// <remarks>This event is only raised when <see cref="P:SkiaSharp.Views.Maui.Controls.SKGaneshView.EnableTouchEvents" /> is <see langword="true" />. Set <see cref="P:SkiaSharp.Views.Maui.SKTouchEventArgs.Handled" /> to <see langword="true" /> to indicate the event was processed.</remarks>
		public event EventHandler<SKTouchEventArgs>? Touch;

		/// <summary>Gets the current size of the canvas in pixels.</summary>
		/// <value>The size of the drawing canvas in pixels.</value>
		/// <remarks>This size accounts for the device's pixel density unless <see cref="P:SkiaSharp.Views.Maui.Controls.SKGaneshView.IgnorePixelScaling" /> is set to <see langword="true" />.</remarks>
		public SKSize CanvasSize => lastCanvasSize;

		/// <summary>Gets the Ganesh GPU graphics context used for rendering.</summary>
		/// <value>The <see cref="T:SkiaSharp.GRContext" /> for the Ganesh GPU rendering context, or <see langword="null" /> if the context is not yet available.</value>
		/// <remarks>The context is created when the view's native handler initializes the GPU surface.</remarks>
		public GRContext? GRContext => lastGRContext;

		/// <summary>Invalidates the view, causing the <see cref="E:SkiaSharp.Views.Maui.Controls.SKGaneshView.PaintSurface" /> event to be raised on the next frame.</summary>
		/// <remarks>Call this method when the content needs to be redrawn. If <see cref="P:SkiaSharp.Views.Maui.Controls.SKGaneshView.HasRenderLoop" /> is <see langword="true" />, calling this method is unnecessary.</remarks>
		public void InvalidateSurface()
		{
			Handler?.Invoke(nameof(ISKGaneshView.InvalidateSurface));
		}

		/// <summary>Raises the <see cref="E:SkiaSharp.Views.Maui.Controls.SKGaneshView.PaintSurface" /> event.</summary>
		/// <param name="e">The event arguments containing the drawing surface and context.</param>
		/// <remarks>Override this method to perform custom drawing operations. Use the <see cref="P:SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs.Surface" /> property to access the <see cref="T:SkiaSharp.SKCanvas" /> for drawing.</remarks>
		protected virtual void OnPaintSurface(SKPaintGaneshSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		/// <summary>Raises the <see cref="E:SkiaSharp.Views.Maui.Controls.SKGaneshView.Touch" /> event.</summary>
		/// <param name="e">The event arguments containing touch event details.</param>
		/// <remarks>Override this method to handle touch events. Set <see cref="P:SkiaSharp.Views.Maui.SKTouchEventArgs.Handled" /> to <see langword="true" /> to indicate the event was processed.</remarks>
		protected virtual void OnTouch(SKTouchEventArgs e)
		{
			Touch?.Invoke(this, e);
		}

		private static void OnWindowChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is not SKGaneshView view)
				return;

			view.Handler?.UpdateValue(nameof(HasRenderLoop));
		}

		/// <summary>This member implements <see cref="P:SkiaSharp.Views.Maui.ISKGaneshView.HasRenderLoop" /> to provide render loop state.</summary>
		/// <value>The value of <see cref="P:SkiaSharp.Views.Maui.Controls.SKGaneshView.HasRenderLoop" /> when attached to a window; otherwise, <see langword="false" />.</value>
		/// <remarks />
		bool ISKGaneshView.HasRenderLoop =>
			HasRenderLoop && Window is not null;

		/// <summary>This member is used internally by the handler to notify size changes.</summary>
		/// <param name="size">The new canvas size in pixels.</param>
		/// <remarks />
		void ISKGaneshView.OnCanvasSizeChanged(SKSizeI size) =>
			lastCanvasSize = size;

		/// <summary>This member is used internally by the handler to notify context changes.</summary>
		/// <param name="context">The new graphics context, or <see langword="null" /> if the context was destroyed.</param>
		/// <remarks />
		void ISKGaneshView.OnGRContextChanged(GRContext? context) =>
			lastGRContext = context;

		/// <summary>This member is used internally by the handler to trigger painting.</summary>
		/// <param name="e">The event arguments containing the drawing surface.</param>
		/// <remarks />
		void ISKGaneshView.OnPaintSurface(SKPaintGaneshSurfaceEventArgs e) =>
			OnPaintSurface(e);

		/// <summary>This member is used internally by the handler to forward touch events.</summary>
		/// <param name="e">The event arguments containing touch event details.</param>
		/// <remarks />
		void ISKGaneshView.OnTouch(SKTouchEventArgs e) =>
			OnTouch(e);
	}
}
