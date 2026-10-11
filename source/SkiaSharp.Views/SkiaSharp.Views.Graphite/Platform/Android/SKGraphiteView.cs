using System;
using System.Threading;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidSurface = Android.Views.Surface;

namespace SkiaSharp.Views.Android
{
	/// <summary>A Graphite drawing container with a private presentation backend.</summary>
	/// <remarks>Currently uses native Vulkan 1.1 on Android API 24 or later. GPU callbacks run on a dedicated owner thread.</remarks>
	public class SKGraphiteView : FrameLayout, ISKGraphiteView
	{
		private Presenter? presenter;
		private RenderThread? retiring;
		private SKGraphiteContext? context;
		private int canvasWidth, canvasHeight;
		private bool hasRenderLoop;
		private bool paused;
		private int generation;
		/// <summary>Creates a Graphite container.</summary>
		/// <param name="context">The Android context.</param>
		public SKGraphiteView (Context context) : base (context) => Initialize ();
		/// <summary>Creates a Graphite container from layout attributes.</summary>
		/// <param name="context">The Android context.</param>
		/// <param name="attrs">The layout attributes.</param>
		public SKGraphiteView (Context context, IAttributeSet attrs) : base (context, attrs) => Initialize ();
		private void Initialize () => SetClipChildren (true);
		/// <inheritdoc />
		public SKSize CanvasSize => new SKSize (Volatile.Read (ref canvasWidth), Volatile.Read (ref canvasHeight));
		/// <inheritdoc />
		public SKGraphiteContext? GraphiteContext => Volatile.Read (ref context);
		/// <inheritdoc />
		public bool IgnorePixelScaling { get; set; }
		/// <inheritdoc />
		public bool HasRenderLoop {
			get => hasRenderLoop;
			set { hasRenderLoop = value; presenter?.Worker?.Wake (); }
		}
		/// <inheritdoc />
		public event EventHandler<SKPaintGraphiteSurfaceEventArgs>? PaintSurface;
		/// <inheritdoc />
		public event EventHandler? ContextCreated;
		/// <inheritdoc />
		public event EventHandler? ContextDestroying;
		/// <inheritdoc />
		public event EventHandler? ContextDestroyed;
		/// <inheritdoc />
		public event EventHandler<SKGpuRenderFailedEventArgs>? RenderFailed;
		/// <inheritdoc />
		public SKGraphiteRecorder CreateRecorder () => GetRenderer ().CreateRecorder ();
		/// <inheritdoc />
		public SKGraphiteRecording SnapRecording (SKGraphiteRecorder recorder) => GetRenderer ().SnapRecording (recorder);
		/// <inheritdoc />
		public SKGraphiteInsertStatus InsertRecording (SKGraphiteRecording recording) => GetRenderer ().InsertRecording (recording);
		private VulkanGraphiteRenderer GetRenderer () => presenter?.Worker?.Renderer ??
			throw new InvalidOperationException ("The Graphite context is not active.");
		/// <inheritdoc />
		public void InvalidateSurface ()
		{
			Post (() => {
				if (!IsAttachedToWindow) return;
				if (presenter?.Worker?.IsStopping == true) {
					if (presenter?.Worker?.IsAlive == true) {
						Fail (new InvalidOperationException ("Dispose caller recorders from the failed generation before retrying."));
						return;
					}
					Disconnect ();
				}
				EnsurePresenter ();
				presenter?.Worker?.RequestFrame ();
			});
		}
		/// <summary>Suspends presentation while the activity is paused.</summary>
		public void OnPause () { paused = true; presenter?.Worker?.Wake (); }
		/// <summary>Resumes presentation after an activity pause.</summary>
		public void OnResume () { paused = false; InvalidateSurface (); }
		/// <inheritdoc />
		public void Disconnect ()
		{
			if (Looper.MyLooper () != Looper.MainLooper) throw new InvalidOperationException ("Container lifetime operations require the UI thread.");
			if (presenter is not { } old) return;
			generation++;
			presenter = null;
			retiring = old.Worker;
			old.Stop ();
			RemoveView (old);
			old.Dispose ();
			Volatile.Write (ref context, null);
			canvasWidth = canvasHeight = 0;
		}
		/// <inheritdoc />
		protected override void OnAttachedToWindow ()
		{
			base.OnAttachedToWindow ();
			EnsurePresenter ();
		}
		private void EnsurePresenter ()
		{
			if (presenter is not null) return;
			if (retiring?.IsAlive == true) {
				Fail (new InvalidOperationException ("Dispose caller recorders from the previous generation before reattaching."));
				return;
			}
			if (Build.VERSION.SdkInt < BuildVersionCodes.N) {
				Fail (new PlatformNotSupportedException ("Graphite Vulkan presentation requires Android API 24 or later."));
				return;
			}
			retiring = null;
			generation++;
			presenter = new Presenter (this);
			AddView (presenter, new LayoutParams (LayoutParams.MatchParent, LayoutParams.MatchParent));
		}
		/// <inheritdoc />
		protected override void OnDetachedFromWindow () { Disconnect (); base.OnDetachedFromWindow (); }
		/// <inheritdoc />
		protected override void Dispose (bool disposing) { if (disposing) Disconnect (); base.Dispose (disposing); }
		private void Fail (Exception exception) => Fail (exception, generation);
		private void Fail (Exception exception, int failedGeneration)
		{
			Post (() => {
				if (generation != failedGeneration) {
					Console.Error.WriteLine ($"Retired Graphite generation failed: {exception}");
					return;
				}
				if (RenderFailed is { } failed) failed (this, new SKGpuRenderFailedEventArgs (exception));
				else Console.Error.WriteLine ($"Graphite rendering failed: {exception}");
			});
		}
		private void Paint (SKPaintGraphiteSurfaceEventArgs e)
		{
			var density = Resources?.DisplayMetrics?.Density ?? 1;
			var info = IgnorePixelScaling ? e.Info.WithSize ((int)(e.Info.Width / density), (int)(e.Info.Height / density)) : e.Info;
			Volatile.Write (ref canvasWidth, info.Width); Volatile.Write (ref canvasHeight, info.Height);
			if (IgnorePixelScaling) e.Surface.Canvas.Scale (density);
			PaintSurface?.Invoke (this, new SKPaintGraphiteSurfaceEventArgs (e, info));
		}
		private sealed class Presenter : TextureView, TextureView.ISurfaceTextureListener
		{
			private readonly SKGraphiteView owner;
			private AndroidSurface? surface;
			public RenderThread? Worker { get; private set; }
			public Presenter (SKGraphiteView owner) : base (owner.Context!) { this.owner = owner; SetOpaque (false); SurfaceTextureListener = this; }
			public void OnSurfaceTextureAvailable (SurfaceTexture texture, int width, int height)
			{
				if (Worker?.IsAlive == true) {
					owner.Fail (new InvalidOperationException ("The previous Graphite session is still shutting down."));
					return;
				}
				texture.SetDefaultBufferSize (width, height);
				surface = new AndroidSurface (texture);
				Worker = new RenderThread (owner, surface, width, height);
				Worker.Start ();
			}
			public void OnSurfaceTextureSizeChanged (SurfaceTexture texture, int width, int height)
			{
				if (width > 0 && height > 0) texture.SetDefaultBufferSize (width, height);
				Worker?.Resize (width, height);
			}
			public void OnSurfaceTextureUpdated (SurfaceTexture texture) { }
			public bool OnSurfaceTextureDestroyed (SurfaceTexture texture) { Stop (); return true; }
			public void Stop ()
			{
				Worker?.Stop ();
				surface?.Dispose (); surface = null;
			}
			protected override void Dispose (bool disposing)
			{
				if (disposing) { Stop (); SurfaceTextureListener = null; }
				base.Dispose (disposing);
			}
		}
		private sealed class RenderThread
		{
			private readonly SKGraphiteView owner;
			private readonly AndroidSurface surface;
			private readonly Thread thread;
			private readonly int generation;
			private readonly object wakeLock = new ();
			private readonly AutoResetEvent wake = new (false);
			private readonly ManualResetEventSlim presentationReleased = new (false);
			private int width, height;
			private int resize = 1, requested = 1, stopping;
			public VulkanGraphiteRenderer? Renderer { get; private set; }
			public bool IsAlive => thread.IsAlive;
			public bool IsStopping => Volatile.Read (ref stopping) != 0;
			public RenderThread (SKGraphiteView owner, AndroidSurface surface, int width, int height)
			{
				this.owner = owner; this.surface = surface; this.width = width; this.height = height;
				generation = owner.generation;
				thread = new Thread (Run) { IsBackground = true, Name = "SkiaSharp Graphite presentation" };
			}
			public void Start () => thread.Start ();
			public void Wake ()
			{
				lock (wakeLock) { if (Volatile.Read (ref stopping) == 0) wake.Set (); }
			}
			public void RequestFrame () { Interlocked.Exchange (ref requested, 1); Wake (); }
			public void Resize (int width, int height) { this.width = width; this.height = height; Interlocked.Exchange (ref resize, 1); RequestFrame (); }
			public void Stop ()
			{
				lock (wakeLock) {
					if (Interlocked.Exchange (ref stopping, 1) == 0) wake.Set ();
				}
				if (System.Environment.CurrentManagedThreadId == thread.ManagedThreadId) return;
				// Window resources must be released before TextureView returns its surface.
				presentationReleased.Wait ();
			}
			private void Run ()
			{
				try {
					Renderer = new VulkanGraphiteRenderer (surface, width, height);
					Volatile.Write (ref owner.context, Renderer.Context);
					owner.ContextCreated?.Invoke (owner, EventArgs.Empty);
					while (Volatile.Read (ref stopping) == 0) {
						if (Interlocked.Exchange (ref resize, 0) != 0) Renderer.Resize (width, height);
						if (!owner.paused && (owner.hasRenderLoop || Interlocked.Exchange (ref requested, 0) != 0)) {
							if (!Renderer.Render (owner.Paint)) Interlocked.Exchange (ref requested, 1);
						}
						// Poll completions even when the on-demand view has no pending frame.
						Renderer.CheckAsyncWorkCompletion ();
						wake.WaitOne (16);
					}
				} catch (Exception ex) { owner.Fail (ex, generation); }
				finally {
					Interlocked.Exchange (ref stopping, 1);
					try {
						if (Renderer is not null) owner.ContextDestroying?.Invoke (owner, EventArgs.Empty);
					} catch (Exception ex) { owner.Fail (ex, generation); }
					try { Renderer?.BeginClose (); }
					catch (Exception ex) { owner.Fail (ex, generation); }
					Volatile.Write (ref owner.context, null);
					owner.canvasWidth = owner.canvasHeight = 0;
					try { owner.ContextDestroyed?.Invoke (owner, EventArgs.Empty); }
					catch (Exception ex) { owner.Fail (ex, generation); }
					presentationReleased.Set ();
					// At most one retired session per view. Registered producer recorders
					// retain the device, and final destruction stays on this owner thread.
					while (Renderer?.TryFinishClose () == false) wake.WaitOne (16);
					Renderer = null;
					lock (wakeLock) wake.Dispose ();
				}
			}
		}
	}
}
