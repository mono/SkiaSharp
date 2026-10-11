using System;
using CoreGraphics;
using Foundation;
using Metal;
using MetalKit;
using UIKit;

namespace SkiaSharp.Views.iOS
{
	/// <summary>A Graphite drawing container with a private presentation backend.</summary>
	/// <remarks>Currently uses native Metal on iOS and Mac Catalyst. Callbacks run on the UI thread.</remarks>
	[Register (nameof (SKGraphiteView))]
	public class SKGraphiteView : UIView, ISKGraphiteView
	{
		private Presenter? presenter;
		private GraphiteSession? retired;
		private NSTimer? retirementTimer;
		private bool hasRenderLoop;
		/// <summary>Creates an empty Graphite container.</summary>
		public SKGraphiteView () : this (CGRect.Empty) { }
		/// <summary>Creates a Graphite container.</summary>
		/// <param name="frame">The container frame in points.</param>
		public SKGraphiteView (CGRect frame) : base (frame) => Initialize ();
		/// <summary>Wraps a native Graphite container.</summary>
		/// <param name="handle">The native handle.</param>
		public SKGraphiteView (IntPtr handle) : base (handle) => Initialize ();
		private void Initialize () { Opaque = false; ClipsToBounds = true; BackgroundColor = UIColor.Clear; }
		/// <inheritdoc />
		public SKSize CanvasSize { get; private set; }
		/// <inheritdoc />
		public SKGraphiteContext? GraphiteContext => presenter?.Session?.Context;
		/// <inheritdoc />
		public bool IgnorePixelScaling { get; set; }
		/// <inheritdoc />
		public bool HasRenderLoop {
			get => hasRenderLoop;
			set { hasRenderLoop = value; UpdateLoop (); }
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
		public SKGraphiteRecorder CreateRecorder () => GetSession ().CreateRecorder ();
		/// <inheritdoc />
		public SKGraphiteRecording SnapRecording (SKGraphiteRecorder recorder) => GetSession ().SnapRecording (recorder);
		/// <inheritdoc />
		public SKGraphiteInsertStatus InsertRecording (SKGraphiteRecording recording) => GetSession ().InsertRecording (recording);
		private GraphiteSession GetSession () => presenter?.Session ??
			throw new InvalidOperationException ("The Graphite context is not active.");
		/// <inheritdoc />
		public void InvalidateSurface ()
		{
			CheckThread ();
			if (Window is null) return;
			try {
				if (!FinishRetirement ())
					throw new InvalidOperationException ("Dispose caller-owned recorders from the previous context before retrying.");
				if (presenter is null) {
					presenter = new Presenter (this) { Frame = Bounds, UserInteractionEnabled = false };
					AddSubview (presenter);
				}
				UpdateLoop ();
				presenter.SetNeedsDisplay ();
			} catch (Exception ex) { Fail (ex); }
		}
		/// <inheritdoc />
		public void Disconnect ()
		{
			CheckThread ();
			if (presenter is not { } old) return;
			old.Paused = true;
			try {
				if (old.Session is not null) ContextDestroying?.Invoke (this, EventArgs.Empty);
			} finally {
				presenter = null;
				retired = old.Retire ();
				old.RemoveFromSuperview ();
				old.Dispose ();
				CanvasSize = SKSize.Empty;
				ContextDestroyed?.Invoke (this, EventArgs.Empty);
				if (!FinishRetirement ()) {
					// One retired generation per host; cleanup stays on the original owner thread.
					retirementTimer ??= NSTimer.CreateRepeatingScheduledTimer (TimeSpan.FromMilliseconds (16), _ => {
						FinishRetirement ();
					});
				}
			}
		}
		private bool FinishRetirement ()
		{
			if (retired is not null && !retired.TryFinishClose ()) return false;
			retired = null;
			retirementTimer?.Invalidate ();
			retirementTimer?.Dispose ();
			retirementTimer = null;
			return true;
		}
		private void FailPresenter (Presenter failedPresenter, Exception exception)
		{
			if (!ReferenceEquals (presenter, failedPresenter)) {
				Console.Error.WriteLine ($"Retired Graphite generation failed: {exception}");
				return;
			}
			try { Fail (exception); }
			finally { if (ReferenceEquals (presenter, failedPresenter)) Disconnect (); }
		}
		/// <inheritdoc />
		public override void MovedToWindow ()
		{
			base.MovedToWindow ();
			if (Window is null) Disconnect (); else InvalidateSurface ();
		}
		/// <inheritdoc />
		public override void LayoutSubviews () { base.LayoutSubviews (); if (presenter is not null) presenter.Frame = Bounds; }
		/// <inheritdoc />
		protected override void Dispose (bool disposing) { if (disposing) Disconnect (); base.Dispose (disposing); }
		private static void CheckThread ()
		{
			if (!NSThread.IsMain) throw new InvalidOperationException ("Graphite container operations require the UI thread.");
		}
		private void UpdateLoop ()
		{
			if (presenter is null) return;
			presenter.EnableSetNeedsDisplay = !hasRenderLoop;
			presenter.Paused = !hasRenderLoop;
		}
		private void Fail (Exception exception)
		{
			if (presenter is not null) presenter.Paused = true;
			if (RenderFailed is { } failed) failed (this, new SKGpuRenderFailedEventArgs (exception));
			else Console.Error.WriteLine ($"Graphite rendering failed: {exception}");
		}
		private sealed class Presenter : MTKView, IMTKViewDelegate
		{
			private readonly SKGraphiteView owner;
			private IMTLCommandQueue? queue;
			private SKGraphiteMtlBackendContext? backend;
			private NSTimer? completionTimer;
			private ulong frameId;
			private int generation;
			private NSObject? inactiveObserver, activeObserver;
			private bool active = true;
			public GraphiteSession? Session { get; private set; }
			public Presenter (SKGraphiteView owner) : base (CGRect.Empty, null)
			{
				this.owner = owner;
				Device = MTLDevice.SystemDefault ?? throw new PlatformNotSupportedException ("Metal is unavailable.");
				ColorPixelFormat = MTLPixelFormat.BGRA8Unorm;
				DepthStencilPixelFormat = MTLPixelFormat.Invalid;
				SampleCount = 1;
				FramebufferOnly = false;
				Opaque = false;
				BackgroundColor = UIColor.Clear;
				Paused = true;
				EnableSetNeedsDisplay = true;
				Delegate = this;
				inactiveObserver = UIApplication.Notifications.ObserveWillResignActive ((_, _) => {
					active = false; Paused = true; ReleaseDrawables ();
				});
				activeObserver = UIApplication.Notifications.ObserveDidBecomeActive ((_, _) => {
					active = true; owner.UpdateLoop (); SetNeedsDisplay ();
				});
			}
			private void EnsureSession ()
			{
				if (Session is not null) return;
#if __MACCATALYST__
				if (!OperatingSystem.IsMacCatalystVersionAtLeast (13, 1))
					throw new PlatformNotSupportedException ("Graphite Metal requires Mac Catalyst 13.1 or later.");
				if (!Device!.SupportsFamily ((MTLGpuFamily)1007) && !Device.SupportsFamily ((MTLGpuFamily)2002))
#else
				if (!OperatingSystem.IsIOSVersionAtLeast (13))
					throw new PlatformNotSupportedException ("Graphite Metal requires iOS 13 or later.");
				if (ObjCRuntime.Runtime.Arch != ObjCRuntime.Arch.SIMULATOR &&
					!Device!.SupportsFamily ((MTLGpuFamily)1002) && !Device.SupportsFamily ((MTLGpuFamily)2002))
#endif
					throw new PlatformNotSupportedException ("The Metal device does not support a Graphite GPU family.");
				queue = Device!.CreateCommandQueue () ?? throw new PlatformNotSupportedException ("Metal queue creation failed.");
				backend = new SKGraphiteMtlBackendContext { Device = Device, Queue = queue };
				var context = SKGraphiteContext.CreateMetal (backend);
				if (context is null) {
					backend.Dispose (); backend = null;
					queue.Dispose (); queue = null;
					throw new PlatformNotSupportedException ("This device cannot create a Graphite Metal context.");
				}
				var ownedBackend = backend;
				var ownedQueue = queue;
				Session = new GraphiteSession (context, () => { ownedBackend.Dispose (); ownedQueue.Dispose (); });
				owner.ContextCreated?.Invoke (owner, EventArgs.Empty);
				completionTimer = NSTimer.CreateRepeatingScheduledTimer (TimeSpan.FromMilliseconds (16), _ => {
					if (Session is null || !active) return;
					try { Session.Context.CheckAsyncWorkCompletion (); }
					catch (Exception ex) { owner.FailPresenter (this, ex); }
				});
			}
			void IMTKViewDelegate.DrawableSizeWillChange (MTKView view, CGSize size) { if (active) SetNeedsDisplay (); }
			void IMTKViewDelegate.Draw (MTKView view)
			{
				if (!active || owner.Window is null) return;
				SKPaintGraphiteSurfaceEventArgs? frame = null;
				try {
					using var pool = new NSAutoreleasePool ();
					var width = (int)DrawableSize.Width;
					var height = (int)DrawableSize.Height;
					if (width <= 0 || height <= 0) return;
					var drawable = CurrentDrawable;
					if (drawable?.Texture is null) return;
					EnsureSession ();
					var session = Session!;
					using var texture = SKGraphiteBackendTexture.CreateMetal (width, height, drawable.Texture.Handle) ??
						throw new InvalidOperationException ("Graphite drawable wrapping failed.");
					using var surface = SKSurface.Create (session.Recorder!, texture, SKColorType.Bgra8888) ??
						throw new InvalidOperationException ("Graphite surface creation failed.");
					var raw = new SKImageInfo (width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
					var info = owner.IgnorePixelScaling ? raw.WithSize ((int)owner.Bounds.Width, (int)owner.Bounds.Height) : raw;
					owner.CanvasSize = info.Size;
					using (var restore = new SKAutoCanvasRestore (surface.Canvas, true)) {
						surface.Canvas.Clear (SKColors.Transparent);
						if (owner.IgnorePixelScaling)
							surface.Canvas.Scale (width / (float)Math.Max (1, info.Width), height / (float)Math.Max (1, info.Height));
						frame = new SKPaintGraphiteSurfaceEventArgs (surface, texture, session.Context, info, raw);
						owner.PaintSurface?.Invoke (owner, frame);
					}
					using var recording = session.Recorder!.Snap () ?? throw new InvalidOperationException ("Graphite snap failed.");
					session.InsertPrimaryRecording (recording);
					frame.CompleteReadbacks (true);
					if (!session.Context.Submit (new SKGraphiteSubmitInfo { Sync = false, MarkBoundary = true, FrameID = ++frameId }))
						throw new InvalidOperationException ("Graphite submission failed.");
					using var buffer = queue!.CommandBuffer () ?? throw new InvalidOperationException ("Metal presentation buffer creation failed.");
					var submittedGeneration = generation;
					buffer.AddCompletedHandler (completed => {
						if (completed.Status != MTLCommandBufferStatus.Error) return;
						var message = completed.Error?.LocalizedDescription ?? "Metal presentation failed.";
						owner.BeginInvokeOnMainThread (() => {
							if (Session is not null && generation == submittedGeneration)
								owner.FailPresenter (this, new InvalidOperationException (message));
						});
					});
					buffer.PresentDrawable (drawable);
					buffer.Commit ();
				} catch (Exception ex) {
					owner.FailPresenter (this, ex);
				} finally {
					try { frame?.CompleteReadbacks (false); }
					catch (Exception ex) { owner.FailPresenter (this, ex); }
				}
			}
			public GraphiteSession? Retire ()
			{
				Paused = true;
				generation++;
				completionTimer?.Invalidate (); completionTimer?.Dispose (); completionTimer = null;
				var session = Session;
				Session = null;
				try { session?.Close (); }
				catch (Exception ex) { owner.Fail (ex); }
				finally {
					if (session is null) { backend?.Dispose (); queue?.Dispose (); }
					queue = null; backend = null;
				}
				return session;
			}
			protected override void Dispose (bool disposing)
			{
				if (disposing) {
					Delegate = null;
					inactiveObserver?.Dispose (); inactiveObserver = null;
					activeObserver?.Dispose (); activeObserver = null;
				}
				base.Dispose (disposing);
			}
		}
	}
}
