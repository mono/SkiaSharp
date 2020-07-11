using System;
using UIKit;

using SkiaSharp;
using SkiaSharp.Views.iOS;
using Metal;
using MetalKit;
using CoreGraphics;

namespace SkiaSharpSample
{
	public partial class ViewController : UIViewController, IMTKViewDelegate
	{
		protected ViewController(IntPtr handle)
			: base(handle)
		{
		}

		MTKView mtkView;
		GRContext grContext;
		IMTLDevice metalDevice;
		IMTLCommandQueue metalQueue;

		public override void LoadView()
		{
			View = new MTKView(UIScreen.MainScreen.Bounds, null);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			if (grContext == null)
			{
				metalDevice = MTLDevice.SystemDefault;
				metalQueue = metalDevice.CreateCommandQueue();
				grContext = GRContext.CreateMetal(metalDevice, metalQueue);
			}

			if (View == null || metalDevice == null)
			{
				Console.WriteLine("Metal is not supported on this device.");
				return;
			}

			mtkView = (MTKView)View;
			mtkView.BackgroundColor = UIColor.Black;
			mtkView.ColorPixelFormat = MTLPixelFormat.BGRA8Unorm;
			mtkView.DepthStencilPixelFormat = MTLPixelFormat.Depth32Float_Stencil8;
			mtkView.SampleCount = 1;
			mtkView.Device = metalDevice;
			mtkView.Delegate = this;
		}

		public void DrawableSizeWillChange(MTKView view, CGSize size)
		{
		}

		public void Draw(MTKView view)
		{
			if (grContext == null || view == null)
				return;

			var colorType = SKColorType.Bgra8888;
			var origin = GRSurfaceOrigin.TopLeft;
			var sampleCount = (int)mtkView.SampleCount;
			var size = mtkView.DrawableSize;
			var width = (int)size.Width;
			var height = (int)size.Height;

			var fbInfo = new GRMetalTextureInfo(mtkView.CurrentDrawable.Texture);

			using (var backendRT = new GRBackendRenderTarget(width, height, 1, fbInfo))
			using (var surface = SKSurface.Create(grContext, backendRT, origin, colorType))
			{
				if (surface == null)
				{
					Console.WriteLine("Unable to create SKSurface.");
					return;
				}

				OnPaintSurface(mtkView, new SKPaintGLSurfaceEventArgs(surface, backendRT));

				// Must flush *and* present for this to work!
				surface.Flush();
			}

			var commandBuffer = metalQueue.CommandBuffer();
			commandBuffer.PresentDrawable(view.CurrentDrawable);
			commandBuffer.Commit();
		}

		private void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;

			canvas.Clear(SKColors.Red);
			canvas.DrawRect(SKRect.Create(100, 400, 400, 100), new SKPaint { Color = SKColors.Violet });
			canvas.DrawText("This is METAL! " + DateTime.Now.ToString("mm:ss.fff"), 50, 550, new SKPaint { TextSize = 50 });

		}
	}
}

/*
using System;
using UIKit;

using SkiaSharp;
using SkiaSharp.Views.iOS;

namespace SkiaSharpSample
{
	public partial class ViewController : UIViewController
	{
		protected ViewController(IntPtr handle)
			: base(handle)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			skiaView.PaintSurface += OnPaintSurface;
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			// the the canvas and properties
			var canvas = e.Surface.Canvas;

			// get the screen density for scaling
			var scale = (float)skiaView.ContentScaleFactor;

			// handle the device screen density
			canvas.Scale(scale);

			// make sure the canvas is blank
			canvas.Clear(SKColors.White);

			// draw some text
			var paint = new SKPaint
			{
				Color = SKColors.Black,
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				TextAlign = SKTextAlign.Center,
				TextSize = 24
			};
			var coord = new SKPoint((float)skiaView.Bounds.Width / 2, ((float)skiaView.Bounds.Height + paint.TextSize) / 2);
			canvas.DrawText("SkiaSharp", coord, paint);
		}
	}
}

 */
