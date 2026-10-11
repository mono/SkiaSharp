#nullable enable

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp.Views.GlesInterop;

namespace SkiaSharp.Views.Windows
{
	/// <summary>A Ganesh drawing container with a private presentation backend.</summary>
	/// <remarks>Currently uses GLES through ANGLE. GPU callbacks run on the UI thread.</remarks>
	public class SKGaneshView : Grid, ISKGaneshView
	{
		private Presenter? presenter;
		private bool hasRenderLoop;
		/// <summary>Creates a Ganesh container.</summary>
		public SKGaneshView ()
		{
			Loaded += (_, _) => EnsurePresenter ();
			Unloaded += (_, _) => Disconnect ();
		}
		/// <inheritdoc />
		public SKSize CanvasSize { get; private set; }
		/// <inheritdoc />
		public GRContext? GRContext => presenter?.Context;
		/// <inheritdoc />
		public bool IgnorePixelScaling { get; set; }
		/// <inheritdoc />
		public bool HasRenderLoop {
			get => hasRenderLoop;
			set { hasRenderLoop = value; presenter?.UpdateLoop (); }
		}
		/// <inheritdoc />
		public event EventHandler<SKPaintGaneshSurfaceEventArgs>? PaintSurface;
		/// <inheritdoc />
		public event EventHandler? ContextCreated;
		/// <inheritdoc />
		public event EventHandler? ContextDestroying;
		/// <inheritdoc />
		public event EventHandler? ContextDestroyed;
		/// <inheritdoc />
		public event EventHandler<SKGpuRenderFailedEventArgs>? RenderFailed;
		/// <inheritdoc />
		public void InvalidateSurface ()
		{
			CheckThread ();
			EnsurePresenter ();
			presenter?.Render ();
		}
		/// <inheritdoc />
		public void Disconnect ()
		{
			CheckThread ();
			if (presenter is not { } old) return;
			try { old.Disconnect (); }
			finally { Children.Remove (old); presenter = null; CanvasSize = SKSize.Empty; }
		}
		private void EnsurePresenter ()
		{
			if (!IsLoaded || presenter is not null) return;
			presenter = new Presenter (this);
			Children.Add (presenter);
		}
		private void CheckThread ()
		{
			if (!DispatcherQueue.HasThreadAccess)
				throw new InvalidOperationException ("Ganesh operations require the UI thread.");
		}
		private void Fail (Exception ex)
		{
			if (RenderFailed is { } handler) handler (this, new SKGpuRenderFailedEventArgs (ex));
			else Console.Error.WriteLine ($"Ganesh rendering failed: {ex}");
		}
		private sealed class Presenter : SwapChainPanel
		{
			private readonly SKGaneshView owner;
			private readonly DispatcherTimer timer = new () { Interval = TimeSpan.FromMilliseconds (16) };
			private GlesContext? backend;
			private GRGlInterface? glInterface;
			private GRBackendRenderTarget? target;
			private SKSurface? surface;
			private SKSizeI size;
			private bool failed;
			private bool releasing;
			public GRContext? Context { get; private set; }

			public Presenter (SKGaneshView owner)
			{
				this.owner = owner;
				Loaded += LoadedView;
				Unloaded += UnloadedView;
				SizeChanged += Resized;
				CompositionScaleChanged += ScaleChanged;
				timer.Tick += Tick;
			}
			private void LoadedView (object sender, RoutedEventArgs e) { UpdateLoop (); Render (); }
			private void UnloadedView (object sender, RoutedEventArgs e) => Disconnect ();
			private void Tick (object? sender, object e) => Render ();
			private void Resized (object sender, SizeChangedEventArgs e) => Resize ();
			private void ScaleChanged (SwapChainPanel sender, object e) => Resize ();
			public void UpdateLoop ()
			{
				if (owner.hasRenderLoop && IsLoaded && !failed) timer.Start ();
				else timer.Stop ();
			}
			private void Resize ()
			{
				try {
					if (backend is not null) {
						backend.MakeCurrent ();
						surface?.Dispose (); surface = null;
						target?.Dispose (); target = null;
						backend.DestroySurface ();
					}
					Render ();
				} catch (Exception ex) {
					timer.Stop ();
					try { owner.Fail (ex); }
					finally { ReleaseContext (); }
				}
			}
			public unsafe void Render ()
			{
				if (!IsLoaded || owner.Visibility != Visibility.Visible || ActualWidth <= 0 || ActualHeight <= 0) return;
				try {
					failed = false;
					backend ??= new GlesContext ();
					if (!backend.HasSurface) backend.CreateSurface (this, null, CompositionScaleX);
					backend.MakeCurrent ();
					backend.GetSurfaceDimensions (out var width, out var height);
					if (width <= 0 || height <= 0) return;
					backend.SetViewportSize (width, height);
					if (Context is null) {
						glInterface = GRGlInterface.Create () ?? throw new InvalidOperationException ("ANGLE interface creation failed.");
						Context = GRContext.CreateGl (glInterface) ?? throw new InvalidOperationException ("Ganesh context creation failed.");
						owner.ContextCreated?.Invoke (owner, EventArgs.Empty);
					}
					var nextSize = new SKSizeI (width, height);
					if (target is null || nextSize != size) {
						surface?.Dispose (); surface = null;
						target?.Dispose (); target = null;
						int framebuffer, stencil, samples;
						Gles.glGetIntegerv (Gles.GL_FRAMEBUFFER_BINDING, &framebuffer);
						Gles.glGetIntegerv (Gles.GL_STENCIL_BITS, &stencil);
						Gles.glGetIntegerv (Gles.GL_SAMPLES, &samples);
						target = new GRBackendRenderTarget (width, height,
							Math.Min (samples, Context.GetMaxSurfaceSampleCount (SKColorType.Rgba8888)), stencil,
							new GRGlFramebufferInfo ((uint)framebuffer, SKColorType.Rgba8888.ToGlSizedFormat ()));
						surface = SKSurface.Create (Context, target, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888) ??
							throw new InvalidOperationException ("Ganesh surface creation failed.");
						size = nextSize;
					}
					var raw = new SKImageInfo (width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
					var info = owner.IgnorePixelScaling ? raw.WithSize ((int)ActualWidth, (int)ActualHeight) : raw;
					owner.CanvasSize = info.Size;
					using (var restore = new SKAutoCanvasRestore (surface!.Canvas, true)) {
						surface.Canvas.Clear (SKColors.Transparent);
						if (owner.IgnorePixelScaling)
							surface.Canvas.Scale (width / (float)Math.Max (1, info.Width), height / (float)Math.Max (1, info.Height));
						owner.PaintSurface?.Invoke (owner, new SKPaintGaneshSurfaceEventArgs (
							surface, Context, target, GRSurfaceOrigin.BottomLeft, info, raw));
					}
					surface.Flush ();
					Context.Flush ();
					if (!backend.SwapBuffers ()) throw new InvalidOperationException ("ANGLE presentation failed.");
					UpdateLoop ();
				} catch (Exception ex) {
					failed = true;
					timer.Stop ();
					try { owner.Fail (ex); }
					finally { ReleaseContext (); }
				}
			}
			public void Disconnect ()
			{
				timer.Stop ();
				timer.Tick -= Tick;
				Loaded -= LoadedView;
				Unloaded -= UnloadedView;
				SizeChanged -= Resized;
				CompositionScaleChanged -= ScaleChanged;
				ReleaseContext ();
			}
			private void ReleaseContext ()
			{
				if (releasing) return;
				releasing = true;
				var announced = Context is not null;
				try {
					if (announced) {
						try { backend?.MakeCurrent (); }
						catch (Exception ex) {
							Context?.AbandonContext (false);
							owner.Fail (ex);
						}
					}
					if (announced) owner.ContextDestroying?.Invoke (owner, EventArgs.Empty);
				} finally {
					Context?.AbandonContext (false);
					surface?.Dispose (); surface = null;
					target?.Dispose (); target = null;
					Context?.Dispose (); Context = null;
					glInterface?.Dispose (); glInterface = null;
					backend?.Dispose (); backend = null;
					size = SKSizeI.Empty;
					owner.CanvasSize = SKSize.Empty;
					releasing = false;
					if (announced) owner.ContextDestroyed?.Invoke (owner, EventArgs.Empty);
				}
			}
		}
	}
}
