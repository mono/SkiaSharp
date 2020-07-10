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


		private SKSurface SkMtkViewToSurface(MTKView mtkView)
		{
			if (mtkView?.CurrentDrawable?.Texture == null || grContext == null)
				return null;

			var colorType = SKColorType.Bgra8888;
			var origin = GRSurfaceOrigin.TopLeft;
			var sampleCount = (int)mtkView.SampleCount;
			var size = mtkView.DrawableSize;
			var width = (int)size.Width;
			var height = (int)size.Height;

			var fbInfo = new GRMetalTextureInfo(mtkView.CurrentDrawable.Texture);

			if (sampleCount == 1)
			{
				var backendRT = new GRBackendRenderTarget(width, height, 1, fbInfo);
				return SKSurface.Create(grContext, backendRT, origin, colorType);
			}
			else
			{
				var backendTexture = new GRBackendTexture(width, height, false, fbInfo);
				return SKSurface.Create(grContext, backendTexture, origin, sampleCount, colorType);
			}
		}

		public void DrawableSizeWillChange(MTKView view, CGSize size)
		{
		}

		public void Draw(MTKView view)
		{
			if (grContext == null || view == null)
				return;

			//// Do as much as possible before creating surface.
			//config_paint(&fPaint);
			//float rotation = (float)(180 * 1e-9 * SkTime::GetNSecs());

			// Create surface:
			var surface = SkMtkViewToSurface(view);
			if (surface == null)
			{
				Console.WriteLine("Unable to create SKSurface.");
				return;
			}

			//draw_example(surface.get(), fPaint, rotation);

			// Must flush *and* present for this to work!
			surface.Flush();
			surface = null;

			var commandBuffer = metalQueue.CommandBuffer();
			commandBuffer.PresentDrawable(view.CurrentDrawable);
			commandBuffer.Commit();
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			// the the canvas and properties
			var canvas = e.Surface.Canvas;

			// get the screen density for scaling
			var scale = (float)mtkView.ContentScaleFactor;

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
			var coord = new SKPoint((float)mtkView.Bounds.Width / 2, ((float)mtkView.Bounds.Height + paint.TextSize) / 2);
			canvas.DrawText("SkiaSharp", coord, paint);
		}
	}
}
