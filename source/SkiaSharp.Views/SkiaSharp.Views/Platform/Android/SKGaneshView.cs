#nullable enable

using System;
using System.Threading;
using Android.Content;
using Android.Opengl;
using Android.Util;
using Android.Widget;
using Javax.Microedition.Khronos.Opengles;
using EGLConfig = Javax.Microedition.Khronos.Egl.EGLConfig;

namespace SkiaSharp.Views.Android
{
	/// <summary>A Ganesh drawing container with a private presentation backend.</summary>
	/// <remarks>Currently uses GLES. Paint and context lifecycle events run on the GL owner thread.</remarks>
	public class SKGaneshView : FrameLayout, ISKGaneshView
	{
		private GLTextureView? presenter;
		private Renderer? renderer;
		private GRContext? context;
		private bool hasRenderLoop;
		private bool paused;
		private int generation;
		/// <summary>Creates a Ganesh container.</summary>
		/// <param name="context">The Android context.</param>
		public SKGaneshView (Context context) : base (context) => SetClipChildren (true);
		/// <summary>Creates a Ganesh container from layout attributes.</summary>
		/// <param name="context">The Android context.</param>
		/// <param name="attrs">The layout attributes.</param>
		public SKGaneshView (Context context, IAttributeSet attrs) : base (context, attrs) => SetClipChildren (true);
		/// <inheritdoc />
		public SKSize CanvasSize { get; private set; }
		/// <inheritdoc />
		public GRContext? GRContext => Volatile.Read (ref context);
		/// <inheritdoc />
		public bool IgnorePixelScaling { get; set; }
		/// <inheritdoc />
		public bool HasRenderLoop {
			get => hasRenderLoop;
			set { hasRenderLoop = value; if (presenter is not null) presenter.RenderMode = value ? Rendermode.Continuously : Rendermode.WhenDirty; }
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
			if (presenter is { } view) {
				var currentRenderer = renderer;
				view.QueueEvent (() => currentRenderer?.Retry ());
				view.RequestRender ();
			}
		}
		/// <summary>Suspends rendering while the activity is paused.</summary>
		public void OnPause () { paused = true; presenter?.OnPause (); }
		/// <summary>Resumes rendering after an activity pause.</summary>
		public void OnResume () { paused = false; presenter?.OnResume (); }
		/// <inheritdoc />
		public void Disconnect ()
		{
			CheckUiThread ();
			var old = presenter;
			if (old is not null) generation++;
			presenter = null;
			if (old is not null) {
				RemoveView (old);
				old.Dispose ();
				renderer?.Dispose ();
				renderer = null;
			}
			context = null;
			CanvasSize = SKSize.Empty;
		}
		/// <inheritdoc />
		protected override void OnAttachedToWindow ()
		{
			base.OnAttachedToWindow ();
			if (presenter is null) {
				generation++;
				presenter = new GLTextureView (Context!);
				presenter.SetOpaque (false);
				presenter.SetEGLContextClientVersion (2);
				presenter.SetEGLConfigChooser (8, 8, 8, 8, 0, 8);
				renderer = new Renderer (this);
				presenter.SetRenderer (renderer);
				presenter.RenderMode = hasRenderLoop ? Rendermode.Continuously : Rendermode.WhenDirty;
				AddView (presenter, new LayoutParams (LayoutParams.MatchParent, LayoutParams.MatchParent));
				if (paused) presenter.OnPause ();
			}
		}
		/// <inheritdoc />
		protected override void OnDetachedFromWindow () { Disconnect (); base.OnDetachedFromWindow (); }
		/// <inheritdoc />
		protected override void Dispose (bool disposing) { if (disposing) Disconnect (); base.Dispose (disposing); }
		private static void CheckUiThread ()
		{
			if (global::Android.OS.Looper.MyLooper () != global::Android.OS.Looper.MainLooper)
				throw new InvalidOperationException ("Container lifetime operations require the UI thread.");
		}

		private sealed class Renderer : Java.Lang.Object, GLTextureView.IRenderer
		{
			private readonly SKGaneshView owner;
			private GRGlInterface? glInterface;
			private SKSurface? surface;
			private GRBackendRenderTarget? target;
			private int width, height;
			private bool failed;
			private readonly int generation;
			public void Retry ()
			{
				if (failed) {
					owner.context?.AbandonContext (false);
					OnSurfaceDestroyed ();
				}
			}
			public Renderer (SKGaneshView owner) { this.owner = owner; generation = owner.generation; }
			public void OnSurfaceCreated (IGL10 gl, EGLConfig config) => OnSurfaceDestroyed ();
			public void OnSurfaceChanged (IGL10 gl, int width, int height)
			{
				this.width = width; this.height = height;
				surface?.Dispose (); surface = null;
				target?.Dispose (); target = null;
				GLES20.GlViewport (0, 0, width, height);
			}
			public void OnSurfaceDestroyed ()
			{
				try {
					if (owner.context is not null)
						owner.ContextDestroying?.Invoke (owner, EventArgs.Empty);
				} finally {
					surface?.Dispose (); surface = null;
					target?.Dispose (); target = null;
					owner.context?.Dispose (); owner.context = null;
					glInterface?.Dispose (); glInterface = null;
					owner.CanvasSize = SKSize.Empty;
					failed = false;
					owner.ContextDestroyed?.Invoke (owner, EventArgs.Empty);
				}
			}
			public void OnDrawFrame (IGL10 gl)
			{
				if (width <= 0 || height <= 0 || failed) return;
				try {
					if (owner.context is null) {
						glInterface = GRGlInterface.Create () ?? throw new InvalidOperationException ("GLES is unavailable.");
						owner.context = GRContext.CreateGl (glInterface) ?? throw new InvalidOperationException ("Ganesh context creation failed.");
						owner.ContextCreated?.Invoke (owner, EventArgs.Empty);
					}
					if (target is null) {
						var values = new int[3];
						GLES20.GlGetIntegerv (GLES20.GlFramebufferBinding, values, 0);
						GLES20.GlGetIntegerv (GLES20.GlStencilBits, values, 1);
						GLES20.GlGetIntegerv (GLES20.GlSamples, values, 2);
						target = new GRBackendRenderTarget (width, height, Math.Min (values[2],
							owner.context.GetMaxSurfaceSampleCount (SKColorType.Rgba8888)), values[1],
							new GRGlFramebufferInfo ((uint)values[0], SKColorType.Rgba8888.ToGlSizedFormat ()));
						surface = SKSurface.Create (owner.context, target, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888) ??
							throw new InvalidOperationException ("Ganesh surface creation failed.");
					}
					var raw = new SKImageInfo (width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
					var density = owner.Resources?.DisplayMetrics?.Density ?? 1;
					var info = owner.IgnorePixelScaling ? raw.WithSize ((int)(width / density), (int)(height / density)) : raw;
					owner.CanvasSize = info.Size;
					using (var restore = new SKAutoCanvasRestore (surface!.Canvas, true)) {
						if (owner.IgnorePixelScaling) surface.Canvas.Scale (density);
						owner.PaintSurface?.Invoke (owner, new SKPaintGaneshSurfaceEventArgs (surface, owner.context,
							target, GRSurfaceOrigin.BottomLeft, info, raw));
					}
					surface.Flush ();
					owner.context.Flush ();
				} catch (Exception ex) {
					failed = true;
					owner.Post (() => {
						if (generation != owner.generation) {
							Console.Error.WriteLine ($"Retired Ganesh generation failed: {ex}");
							return;
						}
						if (owner.RenderFailed is { } handler) handler (owner, new SKGpuRenderFailedEventArgs (ex));
						else Console.Error.WriteLine ($"Ganesh rendering failed: {ex}");
					});
				}
			}
		}
	}
}
