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
	/// <summary>A hardware-accelerated view that renders SkiaSharp drawing commands using the Ganesh GPU backend.</summary>
	/// <remarks>On Apple platforms, this view uses Metal for GPU rendering and presentation.</remarks>
	[Register(nameof(SKGaneshView))]
	public class SKGaneshView : UIView
	{
		private Presenter? presenter;
		private bool ignorePixelScaling;
		private bool hasRenderLoop;

		/// <summary>Initializes a new instance of the <see cref="SKGaneshView" /> class.</summary>
		/// <remarks />
		public SKGaneshView()
			: this(CGRect.Empty)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="SKGaneshView" /> class with the specified frame.</summary>
		/// <param name="frame">The frame used by the view, expressed in points.</param>
		/// <remarks />
		public SKGaneshView(CGRect frame)
			: base(frame)
		{
			Initialize();
		}

		/// <summary>Initializes a new instance of the <see cref="SKGaneshView" /> class from a native handle.</summary>
		/// <param name="handle">The pointer (handle) to the unmanaged object.</param>
		/// <remarks>This constructor is used by the platform runtime when creating managed representations of unmanaged objects. It is not intended to be called directly from user code.</remarks>
		public SKGaneshView(IntPtr handle)
			: base(handle)
		{
			Initialize();
		}

		private void Initialize()
		{
			Opaque = false;
			ClipsToBounds = true;
			BackgroundColor = UIColor.Clear;
		}

		/// <summary>Gets the current canvas size.</summary>
		/// <value>The current canvas size in pixels, or in device-independent units when <see cref="IgnorePixelScaling" /> is <see langword="true" />.</value>
		/// <remarks />
		public SKSize CanvasSize { get; private set; }

		/// <summary>Gets the current Ganesh GPU context.</summary>
		/// <value>The <see cref="SkiaSharp.GRContext" /> currently used for rendering, or <see langword="null" /> if the surface has not been initialized.</value>
		/// <remarks />
		public GRContext? GRContext => presenter?.Context;

		/// <summary>Gets or sets a value indicating whether the drawing canvas should use device-independent point dimensions instead of physical pixels.</summary>
		/// <value><see langword="true" /> to ignore pixel scaling; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				if (ignorePixelScaling == value)
					return;

				ignorePixelScaling = value;
				InvalidateSurface();
			}
		}

		/// <summary>Gets or sets a value indicating whether the view renders continuously on the display refresh loop.</summary>
		/// <value><see langword="true" /> to enable continuous rendering; <see langword="false" /> to render only on demand.</value>
		/// <remarks />
		public bool HasRenderLoop
		{
			get => hasRenderLoop;
			set
			{
				if (hasRenderLoop == value)
					return;

				hasRenderLoop = value;
				if (hasRenderLoop && Window is not null && presenter is null)
				{
					InvalidateSurface();
				}
				else
				{
					UpdateLoop();
				}
			}
		}

		/// <summary>Occurs when the surface needs to be redrawn.</summary>
		/// <remarks>There are two ways to draw on this surface: by overriding <see cref="OnPaintSurface" /> or by attaching a handler to <see cref="PaintSurface" />.</remarks>
		public event EventHandler<SKPaintGaneshSurfaceEventArgs>? PaintSurface;

		/// <summary>Informs the view that its drawing surface needs to be redrawn.</summary>
		/// <remarks />
		public void InvalidateSurface()
		{
			if (Window is null)
				return;

			EnsurePresenter();
			UpdateLoop();
			presenter?.SetNeedsDisplay();
		}

		/// <summary>Implement this to draw on the canvas.</summary>
		/// <param name="e">The event arguments that contain the drawing surface and GPU context information.</param>
		/// <remarks>There are two ways to draw on this surface: by overriding <see cref="OnPaintSurface" /> or by attaching a handler to <see cref="PaintSurface" />. If this method is overridden, then the base must be called, otherwise the event will not be fired.</remarks>
		protected virtual void OnPaintSurface(SKPaintGaneshSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		/// <summary>Called when the view is added to or removed from a window.</summary>
		/// <remarks />
		public override void MovedToWindow()
		{
			base.MovedToWindow();

			if (Window is null)
			{
				DisconnectPresenter();
			}
			else
			{
				InvalidateSurface();
			}
		}

		/// <summary>Lays out subviews.</summary>
		/// <remarks />
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (presenter is not null)
			{
				presenter.Frame = Bounds;
			}
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="SKGaneshView" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DisconnectPresenter();
			}

			base.Dispose(disposing);
		}

		private void EnsurePresenter()
		{
			if (presenter is not null)
				return;

			var device = MTLDevice.SystemDefault;
			if (device is null)
			{
				Console.WriteLine("Metal is not supported on this device.");
				return;
			}

			presenter = new Presenter(this, device)
			{
				Frame = Bounds,
				UserInteractionEnabled = false,
			};

			AddSubview(presenter);
		}

		private void DisconnectPresenter()
		{
			var old = presenter;
			if (old is null)
				return;

			presenter = null;
			old.Paused = true;
			old.RemoveFromSuperview();
			old.Dispose();
			CanvasSize = SKSize.Empty;
		}

		private void UpdateLoop()
		{
			if (presenter is null)
				return;

			presenter.EnableSetNeedsDisplay = !hasRenderLoop;
			presenter.Paused = !hasRenderLoop;
		}

		private sealed class Presenter : MTKView, IMTKViewDelegate
		{
			private readonly SKGaneshView owner;
			private readonly GRMtlBackendContext backend;
			private NSObject? inactiveObserver;
			private NSObject? activeObserver;
			private bool active = true;

			public GRContext? Context { get; private set; }

			public Presenter(SKGaneshView owner, IMTLDevice device)
				: base(CGRect.Empty, device)
			{
				this.owner = owner;
				backend = new GRMtlBackendContext
				{
					Device = device,
					Queue = device.CreateCommandQueue(),
				};

				ColorPixelFormat = MTLPixelFormat.BGRA8Unorm;
				DepthStencilPixelFormat = MTLPixelFormat.Invalid;
				FramebufferOnly = false;
				Opaque = false;
				BackgroundColor = UIColor.Clear;
				Paused = true;
				EnableSetNeedsDisplay = true;
				Delegate = this;

				var weakSelf = new WeakReference<Presenter>(this);

				inactiveObserver = UIApplication.Notifications.ObserveWillResignActive((_, _) =>
				{
					if (weakSelf.TryGetTarget(out var self))
					{
						self.active = false;
						self.Paused = true;
						self.ReleaseDrawables();
					}
				});

				activeObserver = UIApplication.Notifications.ObserveDidBecomeActive((_, _) =>
				{
					if (weakSelf.TryGetTarget(out var self))
					{
						self.active = true;
						self.owner.UpdateLoop();
						self.SetNeedsDisplay();
					}
				});
			}

			void IMTKViewDelegate.DrawableSizeWillChange(MTKView view, CGSize size)
			{
				SetNeedsDisplay();
			}

			void IMTKViewDelegate.Draw(MTKView view)
			{
				if (!active)
					return;

				if (backend.Device is null || backend.Queue is null)
					return;

				using var pool = new NSAutoreleasePool();

				var width = (int)DrawableSize.Width;
				var height = (int)DrawableSize.Height;
				if (width <= 0 || height <= 0 || owner.Window is null)
					return;

				var drawable = CurrentDrawable;
				if (drawable?.Texture is null)
					return;

				Context ??= GRContext.CreateMetal(backend);
				if (Context is null)
					return;

				const SKColorType colorType = SKColorType.Bgra8888;
				const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.TopLeft;

				var metalInfo = new GRMtlTextureInfo(drawable.Texture);
				using var target = new GRBackendRenderTarget(width, height, metalInfo);
				using var surface = SKSurface.Create(Context, target, surfaceOrigin, colorType);
				if (surface is null)
					return;

				var raw = new SKImageInfo(width, height, colorType, SKAlphaType.Premul);
				var info = owner.IgnorePixelScaling
					? raw.WithSize((int)owner.Bounds.Width, (int)owner.Bounds.Height)
					: raw;

				owner.CanvasSize = info.Size;

				using (new SKAutoCanvasRestore(surface.Canvas, true))
				{
					if (owner.IgnorePixelScaling)
					{
						surface.Canvas.Scale(
							width / (float)Math.Max(1, info.Width),
							height / (float)Math.Max(1, info.Height));
					}

					var e = new SKPaintGaneshSurfaceEventArgs(surface, Context, target, surfaceOrigin, info, raw);
					owner.OnPaintSurface(e);
				}

				surface.Flush();
				Context.Flush();

				using var commandBuffer = backend.Queue.CommandBuffer();
				if (commandBuffer is null)
					return;

				commandBuffer.PresentDrawable(drawable);
				commandBuffer.Commit();
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					Paused = true;
					Delegate = null;

					inactiveObserver?.Dispose();
					inactiveObserver = null;

					activeObserver?.Dispose();
					activeObserver = null;

					try
					{
						Context?.Flush();
						using var completion = backend.Queue?.CommandBuffer();
						completion?.Commit();
						completion?.WaitUntilCompleted();
					}
					finally
					{
						Context?.Dispose();
						Context = null;
						backend.Queue?.Dispose();
						backend.Dispose();
					}
				}

				base.Dispose(disposing);
			}
		}
	}
}
#endif
