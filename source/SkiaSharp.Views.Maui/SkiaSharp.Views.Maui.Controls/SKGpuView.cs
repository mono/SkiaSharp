#nullable enable

using System;
using Microsoft.Maui.Controls;

namespace SkiaSharp.Views.Maui.Controls
{
	/// <summary>Common behavior for engine-named GPU drawing controls.</summary>
	/// <remarks>Defaults to on-demand drawing, physical pixels, and opt-in touch forwarding.</remarks>
	public abstract class SKGpuView : View, ISKGpuView
	{
		private static readonly BindableProperty ProxyWindowProperty =
			BindableProperty.Create ("ProxyWindow", typeof (Window), typeof (SKGpuView),
				propertyChanged: (bindable, _, _) => ((SKGpuView)bindable).Handler?.UpdateValue (nameof (HasRenderLoop)));
		/// <summary>Identifies logical-pixel drawing.</summary>
		public static readonly BindableProperty IgnorePixelScalingProperty =
			BindableProperty.Create (nameof (IgnorePixelScaling), typeof (bool), typeof (SKGpuView), false);
		/// <summary>Identifies continuous rendering.</summary>
		public static readonly BindableProperty HasRenderLoopProperty =
			BindableProperty.Create (nameof (HasRenderLoop), typeof (bool), typeof (SKGpuView), false);
		/// <summary>Identifies touch forwarding.</summary>
		public static readonly BindableProperty EnableTouchEventsProperty =
			BindableProperty.Create (nameof (EnableTouchEvents), typeof (bool), typeof (SKGpuView), false);
		/// <summary>Initializes common drawing behavior.</summary>
		protected SKGpuView () => SetBinding (ProxyWindowProperty, new Binding (nameof (Window), source: this));
		/// <summary>Gets or sets drawing in logical rather than physical pixels.</summary>
		public bool IgnorePixelScaling { get => (bool)GetValue (IgnorePixelScalingProperty); set => SetValue (IgnorePixelScalingProperty, value); }
		/// <summary>Gets or sets continuous drawing while attached to a window.</summary>
		public bool HasRenderLoop { get => (bool)GetValue (HasRenderLoopProperty); set => SetValue (HasRenderLoopProperty, value); }
		/// <summary>Gets or sets opt-in touch forwarding.</summary>
		public bool EnableTouchEvents { get => (bool)GetValue (EnableTouchEventsProperty); set => SetValue (EnableTouchEventsProperty, value); }
		/// <inheritdoc />
		public SKSize CanvasSize { get; private set; }
		/// <summary>Occurs on the owner thread after context creation.</summary>
		public event EventHandler? ContextCreated;
		/// <summary>Occurs before destruction. Stop workers and dispose dependent surfaces and recorders synchronously.</summary>
		public event EventHandler? ContextDestroying;
		/// <summary>Occurs after the context and drawing size are cleared.</summary>
		public event EventHandler? ContextDestroyed;
		/// <summary>Occurs when rendering fails, without silently switching engines.</summary>
		public event EventHandler<SKGpuRenderFailedEventArgs>? RenderFailed;
		/// <summary>Occurs for opt-in touch input.</summary>
		public event EventHandler<SKTouchEventArgs>? Touch;
		/// <inheritdoc />
		public void InvalidateSurface () => Handler?.Invoke (nameof (ISKGpuView.InvalidateSurface));
		bool ISKGpuView.HasRenderLoop => HasRenderLoop && Window is not null;
		void ISKGpuView.OnCanvasSizeChanged (SKSizeI size) => CanvasSize = size;
		void ISKGpuView.OnTouch (SKTouchEventArgs e) => OnTouch (e);
		void ISKGpuView.OnContextDestroying () => ContextDestroying?.Invoke (this, EventArgs.Empty);
		void ISKGpuView.OnRenderFailed (Exception exception)
		{
			if (RenderFailed is { } failed) failed (this, new SKGpuRenderFailedEventArgs (exception));
			else Console.Error.WriteLine ($"GPU rendering failed: {exception}");
		}
		/// <summary>Raises touch input.</summary>
		/// <param name="e">The input metadata.</param>
		protected virtual void OnTouch (SKTouchEventArgs e) => Touch?.Invoke (this, e);
		/// <summary>Raises a context creation or destruction notification after updating typed context state.</summary>
		/// <param name="created">Whether a context was created.</param>
		protected void OnContextChanged (bool created)
		{
			if (created) ContextCreated?.Invoke (this, EventArgs.Empty);
			else { CanvasSize = SKSize.Empty; ContextDestroyed?.Invoke (this, EventArgs.Empty); }
		}
	}
	/// <summary>A Ganesh drawing control with a replaceable private presentation backend.</summary>
	public class SKGaneshView : SKGpuView, ISKGaneshView
	{
		/// <summary>Creates a Ganesh drawing control.</summary>
		public SKGaneshView () { }
		/// <inheritdoc />
		public GRContext? GRContext { get; private set; }
		/// <summary>Occurs on the owner thread with borrowed frame resources.</summary>
		public event EventHandler<SKPaintGaneshSurfaceEventArgs>? PaintSurface;
		/// <summary>Raises a Ganesh paint callback.</summary>
		/// <param name="e">The borrowed frame.</param>
		protected virtual void OnPaintSurface (SKPaintGaneshSurfaceEventArgs e) => PaintSurface?.Invoke (this, e);
		void ISKGaneshView.OnPaintSurface (SKPaintGaneshSurfaceEventArgs e) => OnPaintSurface (e);
		void ISKGaneshView.OnGRContextChanged (GRContext? context)
		{
			if (GRContext == context) return;
			GRContext = context; OnContextChanged (context is not null);
		}
	}
	/// <summary>A Graphite drawing control supported on iOS, Mac Catalyst and compatible Android devices.</summary>
	/// <remarks>Windows Graphite presentation is explicitly unsupported. No Ganesh or software fallback occurs.</remarks>
	public class SKGraphiteView : SKGpuView, ISKGraphiteView
	{
		/// <summary>Creates a Graphite drawing control.</summary>
		public SKGraphiteView () { }
		/// <inheritdoc />
		public SKGraphiteContext? GraphiteContext { get; private set; }
		/// <summary>Occurs on the owner thread with borrowed frame resources.</summary>
		public event EventHandler<SKPaintGraphiteSurfaceEventArgs>? PaintSurface;
		/// <inheritdoc />
		public SKGraphiteRecorder CreateRecorder () =>
			(Handler as Handlers.SKGraphiteViewHandler)?.CreateRecorder () ??
			throw new InvalidOperationException ("The Graphite handler is not attached.");
		/// <inheritdoc />
		public SKGraphiteRecording SnapRecording (SKGraphiteRecorder recorder) =>
			(Handler as Handlers.SKGraphiteViewHandler)?.SnapRecording (recorder) ??
			throw new InvalidOperationException ("The Graphite handler is not attached.");
		/// <inheritdoc />
		public SKGraphiteInsertStatus InsertRecording (SKGraphiteRecording recording) =>
			(Handler as Handlers.SKGraphiteViewHandler)?.InsertRecording (recording) ??
			throw new InvalidOperationException ("The Graphite handler is not attached.");
		/// <summary>Raises a Graphite paint callback.</summary>
		/// <param name="e">The borrowed frame.</param>
		protected virtual void OnPaintSurface (SKPaintGraphiteSurfaceEventArgs e) => PaintSurface?.Invoke (this, e);
		void ISKGraphiteView.OnPaintSurface (SKPaintGraphiteSurfaceEventArgs e) => OnPaintSurface (e);
		void ISKGraphiteView.OnGraphiteContextChanged (SKGraphiteContext? context)
		{
			if (GraphiteContext == context) return;
			GraphiteContext = context; OnContextChanged (context is not null);
		}
	}
}
