#nullable enable

#if __IOS__
using System;
using CoreGraphics;
using Foundation;
using Metal;
using MetalKit;
using UIKit;

namespace SkiaSharp.Views.iOS
{
	/// <summary>A Ganesh drawing container whose presentation backend is an implementation detail.</summary>
	/// <remarks>Currently uses Metal. All GPU operations and lifecycle events run on the UI thread.</remarks>
	[Register (nameof (SKGaneshView))]
	public class SKGaneshView : UIView, ISKGaneshView
	{
		private Presenter? presenter;
		private bool hasRenderLoop;
		private bool failed;

		/// <summary>Creates an empty Ganesh container.</summary>
		public SKGaneshView () : this (CGRect.Empty) { }
		/// <summary>Creates a Ganesh container.</summary>
		/// <param name="frame">The frame in points.</param>
		public SKGaneshView (CGRect frame) : base (frame) => Initialize ();
		/// <summary>Creates a managed wrapper for a native container.</summary>
		/// <param name="handle">The native container handle.</param>
		public SKGaneshView (IntPtr handle) : base (handle) => Initialize ();
		private void Initialize ()
		{
			Opaque = false;
			ClipsToBounds = true;
			BackgroundColor = UIColor.Clear;
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
			set { hasRenderLoop = value; UpdateLoop (); }
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
			if (Window is null)
				return;
			if (failed) { Disconnect (); failed = false; }
			EnsurePresenter ();
			UpdateLoop ();
			presenter?.SetNeedsDisplay ();
		}
		/// <inheritdoc />
		public void Disconnect ()
		{
			CheckThread ();
			var old = presenter;
			if (old is null)
				return;
			old.Paused = true;
			try {
				if (old.Context is not null)
					ContextDestroying?.Invoke (this, EventArgs.Empty);
			} finally {
				presenter = null;
				old.RemoveFromSuperview ();
				old.Dispose ();
				CanvasSize = SKSize.Empty;
				ContextDestroyed?.Invoke (this, EventArgs.Empty);
			}
		}
		/// <inheritdoc />
		public override void MovedToWindow ()
		{
			base.MovedToWindow ();
			if (Window is null)
				Disconnect ();
			else
				InvalidateSurface ();
		}
		/// <inheritdoc />
		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			if (presenter is not null)
				presenter.Frame = Bounds;
		}
		/// <inheritdoc />
		protected override void Dispose (bool disposing)
		{
			if (disposing)
				Disconnect ();
			base.Dispose (disposing);
		}
		private void EnsurePresenter ()
		{
			if (presenter is not null)
				return;
			try {
				presenter = new Presenter (this) { Frame = Bounds, UserInteractionEnabled = false };
				AddSubview (presenter);
			} catch (Exception ex) {
				Fail (ex);
			}
		}
		private void UpdateLoop ()
		{
			if (presenter is null)
				return;
			presenter.EnableSetNeedsDisplay = !hasRenderLoop;
			presenter.Paused = failed || !hasRenderLoop;
		}
		private void Fail (Exception ex)
		{
			failed = true;
			if (presenter is not null)
				presenter.Paused = true;
			if (RenderFailed is { } handler)
				handler (this, new SKGpuRenderFailedEventArgs (ex));
			else
				Console.Error.WriteLine ($"Ganesh rendering failed: {ex}");
		}
		private static void CheckThread ()
		{
			if (!NSThread.IsMain)
				throw new InvalidOperationException ("Ganesh view operations require the UI thread.");
		}

		private sealed class Presenter : MTKView, IMTKViewDelegate
		{
			private readonly SKGaneshView owner;
			private readonly GRMtlBackendContext backend;
			private NSObject? inactiveObserver, activeObserver;
			private bool active = true;
			private int generation;
			public GRContext? Context { get; private set; }
			public Presenter (SKGaneshView owner) : base (CGRect.Empty, null)
			{
				this.owner = owner;
				Device = MTLDevice.SystemDefault ??
					throw new PlatformNotSupportedException ("Metal is unavailable.");
				backend = new GRMtlBackendContext {
					Device = Device,
					Queue = Device.CreateCommandQueue () ??
						throw new PlatformNotSupportedException ("A Metal queue could not be created."),
				};
				ColorPixelFormat = MTLPixelFormat.BGRA8Unorm;
				DepthStencilPixelFormat = MTLPixelFormat.Invalid;
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
			void IMTKViewDelegate.DrawableSizeWillChange (MTKView view, CGSize size) => SetNeedsDisplay ();
			void IMTKViewDelegate.Draw (MTKView view)
			{
				if (!active) return;
				try {
					using var pool = new NSAutoreleasePool ();
					var width = (int)DrawableSize.Width;
					var height = (int)DrawableSize.Height;
					if (width <= 0 || height <= 0 || owner.Window is null)
						return;
					var drawable = CurrentDrawable;
					if (drawable?.Texture is null)
						return;
					if (Context is null) {
						Context = GRContext.CreateMetal (backend) ??
							throw new InvalidOperationException ("Ganesh context creation failed.");
						owner.ContextCreated?.Invoke (owner, EventArgs.Empty);
					}
					using var target = new GRBackendRenderTarget (width, height, new GRMtlTextureInfo (drawable.Texture));
					using var surface = SKSurface.Create (Context, target, GRSurfaceOrigin.TopLeft, SKColorType.Bgra8888) ??
						throw new InvalidOperationException ("Ganesh surface creation failed.");
					var raw = new SKImageInfo (width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
					var info = owner.IgnorePixelScaling ? raw.WithSize ((int)owner.Bounds.Width, (int)owner.Bounds.Height) : raw;
					owner.CanvasSize = info.Size;
					using (var restore = new SKAutoCanvasRestore (surface.Canvas, true)) {
						if (owner.IgnorePixelScaling)
							surface.Canvas.Scale (width / (float)Math.Max (1, info.Width), height / (float)Math.Max (1, info.Height));
						owner.PaintSurface?.Invoke (owner, new SKPaintGaneshSurfaceEventArgs (
							surface, Context, target, GRSurfaceOrigin.TopLeft, info, raw));
					}
					surface.Flush ();
					Context.Flush ();
					using var buffer = backend.Queue.CommandBuffer () ??
						throw new InvalidOperationException ("Metal presentation failed.");
					var submittedGeneration = generation;
					buffer.AddCompletedHandler (completed => {
						if (completed.Status != MTLCommandBufferStatus.Error) return;
						var message = completed.Error?.LocalizedDescription ?? "Metal presentation failed.";
						BeginInvokeOnMainThread (() => {
							if (Context is not null && generation == submittedGeneration)
								owner.Fail (new InvalidOperationException (message));
						});
					});
					buffer.PresentDrawable (drawable);
					buffer.Commit ();
				} catch (Exception ex) { owner.Fail (ex); }
			}
			protected override void Dispose (bool disposing)
			{
				if (disposing) {
					Paused = true;
					Delegate = null;
					generation++;
					inactiveObserver?.Dispose (); inactiveObserver = null;
					activeObserver?.Dispose (); activeObserver = null;
					try {
						Context?.Flush ();
						using var completion = backend.Queue?.CommandBuffer ();
						completion?.Commit ();
						completion?.WaitUntilCompleted ();
					} finally {
						Context?.Dispose ();
						Context = null;
						backend.Queue?.Dispose ();
						backend.Dispose ();
					}
				}
				base.Dispose (disposing);
			}
		}
	}
}
#endif
