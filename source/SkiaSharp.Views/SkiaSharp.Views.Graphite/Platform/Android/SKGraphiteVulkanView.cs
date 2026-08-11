using System;
using System.Threading;
using Android.Content;
using Android.Graphics;
using Android.Opengl;
using Android.OS;
using Android.Util;
using Android.Views;

using AndroidSurface = Android.Views.Surface;

namespace SkiaSharp.Views.Android
{
	public class SKGraphiteVulkanView : TextureView, TextureView.ISurfaceTextureListener
	{
		private RenderThread? renderThread;
		private AndroidSurface? surface;
		private Rendermode renderMode = Rendermode.Continuously;
		private bool paused;
		private int canvasWidth;
		private int canvasHeight;
		private SKGraphiteContext? graphiteContext;

		public SKGraphiteVulkanView (Context context)
			: base (context)
		{
			Initialize ();
		}

		public SKGraphiteVulkanView (Context context, IAttributeSet attrs)
			: base (context, attrs)
		{
			Initialize ();
		}

		public SKSize CanvasSize =>
			new SKSize (
				Volatile.Read (ref canvasWidth),
				Volatile.Read (ref canvasHeight));

		public SKGraphiteContext? GraphiteContext =>
			Volatile.Read (ref graphiteContext);

		public Rendermode RenderMode
		{
			get => renderMode;
			set {
				renderMode = value;
				renderThread?.SetRenderMode (value);
			}
		}

		public event EventHandler<SKPaintGraphiteSurfaceEventArgs>? PaintSurface;

		public event EventHandler<SKGraphiteRenderFailedEventArgs>? RenderFailed;

		public void RequestRender () =>
			renderThread?.RequestRender ();

		public void OnPause ()
		{
			paused = true;
			renderThread?.SetPaused (true);
		}

		public void OnResume ()
		{
			paused = false;
			renderThread?.SetPaused (false);
		}

		public void OnSurfaceTextureAvailable (SurfaceTexture surfaceTexture, int width, int height)
		{
			surface?.Dispose ();
			surface = new AndroidSurface (surfaceTexture);
			EnsureRenderThread ().SetSurface (surface, width, height);
		}

		public bool OnSurfaceTextureDestroyed (SurfaceTexture surfaceTexture)
		{
			renderThread?.ClearSurface ();
			surface?.Dispose ();
			surface = null;
			return true;
		}

		public void OnSurfaceTextureSizeChanged (SurfaceTexture surfaceTexture, int width, int height) =>
			renderThread?.Resize (width, height);

		public void OnSurfaceTextureUpdated (SurfaceTexture surfaceTexture)
		{
		}

		protected virtual void OnPaintSurface (SKPaintGraphiteSurfaceEventArgs e) =>
			PaintSurface?.Invoke (this, e);

		protected virtual void OnRenderFailed (Exception exception)
		{
			if (RenderFailed is null)
				throw new InvalidOperationException ("Graphite rendering failed.", exception);
			RenderFailed.Invoke (this, new SKGraphiteRenderFailedEventArgs (exception));
		}

		protected override void OnAttachedToWindow ()
		{
			base.OnAttachedToWindow ();
			EnsureRenderThread ();
		}

		protected override void OnDetachedFromWindow ()
		{
			StopRenderThread ();
			base.OnDetachedFromWindow ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				StopRenderThread ();
				surface?.Dispose ();
				surface = null;
			}

			base.Dispose (disposing);
		}

		private void Initialize ()
		{
			if (Build.VERSION.SdkInt < BuildVersionCodes.N)
				throw new PlatformNotSupportedException (
					"Graphite Vulkan views require Android 7.0 (API 24) or later.");

			SurfaceTextureListener = this;
			SetOpaque (false);
		}

		private RenderThread EnsureRenderThread ()
		{
			if (renderThread is not null)
				return renderThread;

			renderThread = new RenderThread (
				OnPaintSurface,
				UpdateGraphiteState,
				exception => Post (() => OnRenderFailed (exception)));
			renderThread.SetRenderMode (renderMode);
			renderThread.SetPaused (paused);
			renderThread.Start ();
			return renderThread;
		}

		private void StopRenderThread ()
		{
			renderThread?.Stop ();
			renderThread = null;
			UpdateGraphiteState (null, 0, 0);
		}

		private void UpdateGraphiteState (SKGraphiteContext? context, int width, int height)
		{
			Volatile.Write (ref graphiteContext, context);
			Volatile.Write (ref canvasWidth, width);
			Volatile.Write (ref canvasHeight, height);
		}

		private sealed class RenderThread
		{
			private readonly object sync = new ();
			private readonly Action<SKPaintGraphiteSurfaceEventArgs> paint;
			private readonly Action<SKGraphiteContext?, int, int> stateChanged;
			private readonly Action<Exception> failed;
			private readonly Thread thread;

			private AndroidSurface? surface;
			private int width;
			private int height;
			private bool surfaceChanged;
			private bool surfaceReleasePending;
			private bool renderRequested = true;
			private bool paused;
			private bool exitRequested;
			private Rendermode renderMode = Rendermode.Continuously;

			public RenderThread (
				Action<SKPaintGraphiteSurfaceEventArgs> paint,
				Action<SKGraphiteContext?, int, int> stateChanged,
				Action<Exception> failed)
			{
				this.paint = paint;
				this.stateChanged = stateChanged;
				this.failed = failed;
				thread = new Thread (Run) {
					IsBackground = true,
					Name = nameof (SKGraphiteVulkanView),
				};
			}

			public void Start () => thread.Start ();

			public void SetSurface (AndroidSurface value, int width, int height)
			{
				lock (sync) {
					surface = value;
					this.width = width;
					this.height = height;
					surfaceChanged = true;
					renderRequested = true;
					Monitor.PulseAll (sync);
				}
			}

			public void ClearSurface ()
			{
				lock (sync) {
					surface = null;
					surfaceReleasePending = true;
					Monitor.PulseAll (sync);
					while (surfaceReleasePending && thread.IsAlive)
						Monitor.Wait (sync);
				}
			}

			public void Resize (int width, int height)
			{
				lock (sync) {
					this.width = width;
					this.height = height;
					surfaceChanged = true;
					renderRequested = true;
					Monitor.PulseAll (sync);
				}
			}

			public void RequestRender ()
			{
				lock (sync) {
					renderRequested = true;
					Monitor.PulseAll (sync);
				}
			}

			public void SetPaused (bool value)
			{
				lock (sync) {
					paused = value;
					if (!paused)
						renderRequested = true;
					Monitor.PulseAll (sync);
				}
			}

			public void SetRenderMode (Rendermode value)
			{
				lock (sync) {
					renderMode = value;
					renderRequested = true;
					Monitor.PulseAll (sync);
				}
			}

			public void Stop ()
			{
				lock (sync) {
					exitRequested = true;
					Monitor.PulseAll (sync);
				}
				if (Thread.CurrentThread != thread && thread.IsAlive)
					thread.Join ();
			}

			private void Run ()
			{
				VulkanGraphiteRenderer? renderer = null;
				try {
					while (true) {
						AndroidSurface? nextSurface;
						int nextWidth;
						int nextHeight;
						bool releaseSurface;
						bool resize;

						lock (sync) {
							while (!exitRequested &&
								!surfaceReleasePending &&
								(paused ||
								 surface is null ||
								 (!surfaceChanged &&
								  renderMode == Rendermode.WhenDirty &&
								  !renderRequested))) {
								Monitor.Wait (sync);
							}

							if (exitRequested)
								break;

							releaseSurface = surfaceReleasePending;
							nextSurface = surface;
							nextWidth = width;
							nextHeight = height;
							resize = surfaceChanged;
							surfaceChanged = false;
							renderRequested = false;
						}

						if (releaseSurface) {
							renderer?.ReleaseSurface ();
							stateChanged (renderer?.Context, 0, 0);
							lock (sync) {
								surfaceReleasePending = false;
								Monitor.PulseAll (sync);
							}
							continue;
						}

						if (nextSurface is null)
							continue;

						if (renderer is null) {
							renderer = new VulkanGraphiteRenderer (nextSurface, nextWidth, nextHeight);
							stateChanged (renderer.Context, renderer.Width, renderer.Height);
						} else if (!renderer.HasSurface) {
							renderer.AttachSurface (nextSurface, nextWidth, nextHeight);
							stateChanged (renderer.Context, renderer.Width, renderer.Height);
						} else if (resize) {
							renderer.Resize (nextWidth, nextHeight);
							stateChanged (renderer.Context, renderer.Width, renderer.Height);
						}

						if (renderer.Render (paint)) {
							stateChanged (renderer.Context, renderer.Width, renderer.Height);
						} else {
							lock (sync) {
								renderRequested = true;
								Monitor.PulseAll (sync);
							}
						}
					}
				} catch (Exception ex) {
					failed (ex);
				} finally {
					renderer?.Dispose ();
					stateChanged (null, 0, 0);
					lock (sync) {
						surfaceReleasePending = false;
						Monitor.PulseAll (sync);
					}
				}
			}
		}
	}
}
