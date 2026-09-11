#if __IOS__ || __MACOS__ || __TVOS__
using System;
using System.ComponentModel;
using System.Diagnostics;
using CoreGraphics;
using Foundation;
using Metal;
using MetalKit;

#if __IOS__
namespace SkiaSharp.Views.iOS
#elif __MACOS__
namespace SkiaSharp.Views.Mac
#elif __TVOS__
namespace SkiaSharp.Views.tvOS
#endif
{
	/// <summary>A hardware-accelerated view that uses Metal to render SkiaSharp drawing commands.</summary>
	/// <remarks>This view provides the best performance on devices that support Metal. For devices that do not support Metal, use <see cref="SKCanvasView" /> instead.</remarks>
	[Register(nameof(SKMetalView))]
	[DesignTimeVisible(true)]
	public class SKMetalView : MTKView, IMTKViewDelegate, IComponent
	{
		// for IComponent
#pragma warning disable 67
		private event EventHandler DisposedInternal;
#pragma warning restore 67
		ISite IComponent.Site { get; set; }
		event EventHandler IComponent.Disposed
		{
			add { DisposedInternal += value; }
			remove { DisposedInternal -= value; }
		}

		private bool designMode;

		private bool DepthStencilModePrivate =>
		#if __MACCATALYST__
			false;
		#elif __MACOS__
			true;
		#else
			ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.SIMULATOR;
		#endif

		private GRMtlBackendContext backendContext;
		private GRContext context;

		// created in code
		/// <summary>Initializes a new instance of the <see cref="SKMetalView" /> class.</summary>
		/// <remarks />
		public SKMetalView()
			: this(CGRect.Empty)
		{
		}

		// created in code
		/// <summary>Initializes the <see cref="SKMetalView" /> with the specified frame.</summary>
		/// <param name="frame">The frame used by the view, expressed in points.</param>
		/// <remarks />
		public SKMetalView(CGRect frame)
			: base(frame, null)
		{
			Initialize();
		}

		// created in code
		/// <summary>Initializes the <see cref="SKMetalView" /> with the specified frame and Metal device.</summary>
		/// <param name="frame">The frame used by the view, expressed in points.</param>
		/// <param name="device">The Metal device to use for rendering.</param>
		/// <remarks />
		public SKMetalView(CGRect frame, IMTLDevice device)
			: base(frame, device)
		{
			Initialize();
		}

		// created via designer
		/// <summary>Initializes a new instance of the <see cref="SKMetalView" /> class from a native handle.</summary>
		/// <param name="p">The pointer (handle) to the unmanaged object.</param>
		/// <remarks>This constructor is used by the platform runtime when creating managed representations of unmanaged objects. It is not intended to be called directly from user code.</remarks>
		public SKMetalView(IntPtr p)
			: base(p)
		{
		}

		// created via designer
		/// <summary>Called after the object has been loaded from the nib file. Overriders must call the base method.</summary>
		/// <remarks />
		public override void AwakeFromNib()
		{
			Initialize();
		}

		private void Initialize()
		{
			designMode = ((IComponent)this).Site?.DesignMode == true || !EnvironmentExtensions.IsValidEnvironment;

			if (designMode)
				return;

			var device = MTLDevice.SystemDefault;
			if (device == null)
			{
				Console.WriteLine("Metal is not supported on this device.");
				return;
			}

			ColorPixelFormat = MTLPixelFormat.BGRA8Unorm;
			DepthStencilPixelFormat = MTLPixelFormat.Depth32Float_Stencil8;
			nuint sampling = 1;
#if __IOS__ || __TVOS__
			if (UIKit.UIDevice.CurrentDevice.CheckSystemVersion(16, 0))
			{
				if (DepthStencilModePrivate)
				{
					DepthStencilStorageMode = MTLStorageMode.Private;
					sampling = 4;
				}
				else
				{
					DepthStencilStorageMode = MTLStorageMode.Shared;
				}
			}
#endif
			SampleCount = sampling;
			FramebufferOnly = false;
			Device = device;
			backendContext = new GRMtlBackendContext
			{
				Device = device,
				Queue = device.CreateCommandQueue(),
			};

			// hook up the drawing
			Delegate = this;
		}

		/// <summary>Gets the current canvas size.</summary>
		/// <value>The current canvas size in pixels.</value>
		/// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
		public SKSize CanvasSize { get; private set; }

		/// <summary>Gets the current GPU context.</summary>
		/// <value>The GPU context used by the Metal backend.</value>
		/// <remarks />
		public GRContext GRContext => context;

		/// <summary>Implements the IMTKViewDelegate.DrawableSizeWillChange method to handle size changes.</summary>
		/// <param name="view">The view whose drawable size changed.</param>
		/// <param name="size">The new drawable size.</param>
		/// <remarks />
		void IMTKViewDelegate.DrawableSizeWillChange(MTKView view, CGSize size)
		{
			CanvasSize = size.ToSKSize();

			if (Paused && EnableSetNeedsDisplay)
#if __IOS__ || __TVOS__
				SetNeedsDisplay();
#elif __MACOS__
				NeedsDisplay = true;
#endif
		}

		/// <summary>Implements the IMTKViewDelegate.Draw method to render the SkiaSharp content.</summary>
		/// <param name="view">The view that triggered the draw request.</param>
		/// <remarks />
		void IMTKViewDelegate.Draw(MTKView view)
		{
			if (designMode)
				return;

			if (backendContext.Device == null || backendContext.Queue == null || CurrentDrawable?.Texture == null)
				return;

			CanvasSize = DrawableSize.ToSKSize();

			if (CanvasSize.Width <= 0 || CanvasSize.Height <= 0)
				return;

			// create the contexts if not done already
			context ??= GRContext.CreateMetal(backendContext);

			const SKColorType colorType = SKColorType.Bgra8888;
			const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.TopLeft;

			// create the render target
			var metalInfo = new GRMtlTextureInfo(CurrentDrawable.Texture);
			using var renderTarget = new GRBackendRenderTarget((int)CanvasSize.Width, (int)CanvasSize.Height, metalInfo);

			// create the surface
			using var surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);
			using var canvas = surface.Canvas;

			// start drawing
			var e = new SKPaintMetalSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType);
			OnPaintSurface(e);

			// flush the SkiaSharp contents
			canvas.Flush();
			surface.Flush();
			context.Flush();

			// present
			using var commandBuffer = backendContext.Queue.CommandBuffer();
			commandBuffer.PresentDrawable(CurrentDrawable);
			commandBuffer.Commit();
		}

		/// <summary>Occurs when the surface needs to be redrawn.</summary>
		/// <remarks>There are two ways to draw on this surface: by overriding <see cref="OnPaintSurface" /> or by attaching a handler to <see cref="PaintSurface" />.</remarks>
		public event EventHandler<SKPaintMetalSurfaceEventArgs> PaintSurface;

		/// <summary>Implement this to draw on the canvas.</summary>
		/// <param name="e">The event arguments that contain the drawing surface and information.</param>
		/// <remarks>There are two ways to draw on this surface: by overriding <see cref="OnPaintSurface" /> or by attaching a handler to <see cref="PaintSurface" />. If this method is overridden, then the base must be called, otherwise the event will not be fired.</remarks>
		protected virtual void OnPaintSurface(SKPaintMetalSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}
	}
}
#endif
