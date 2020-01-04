using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using OpenTK.Graphics.OpenGL;

namespace SkiaSharpSample
{
	public partial class MainPage : ContentPage
	{
		public SKPoint Pos;
		public bool Down;

		public MainPage()
		{
			InitializeComponent();

		//	glView.OnDisplay = OnDisplay;
		//	glView.HasRenderLoop = true;
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			// the the canvas and properties
			var canvas = e.Surface.Canvas;

			// get the screen density for scaling
			var scale = (float)(e.Info.Width / skiaView.Width);

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
			var coord = new SKPoint((float)skiaView.Width / 2, ((float)skiaView.Height + paint.TextSize) / 2);
			canvas.DrawText($"SkiaSharp {Pos} ({Down})", coord, paint);
		}

		private void OnCLick(object sender, EventArgs e)
		{
			skiaView.InvalidateSurface();
		}

		private void OnTouched(object sender, SKTouchEventArgs e)
		{
			Pos = e.Location;
			Down = e.InContact;

			Console.WriteLine(e);

			skiaView.InvalidateSurface();
			//skiaGLView.InvalidateSurface();

			//e.Handled = true;
		}


		//private const SKColorType colorType = SKColorType.Rgba8888;
		//private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

		//private bool designMode;

		//private GRContext grContext;
		//private GRBackendRenderTarget renderTarget;
		//private SKSurface surface;


		//private void OnDisplay(Rectangle rect)
		//{
		//	//GL.ClearColor(1, 0, 0, 1);
		//	//GL.Clear(ClearBufferMask.ColorBufferBit);

		//	// create the contexts if not done already
		//	if (grContext == null)
		//	{
		//		var glInterface = GRGlInterface.CreateNativeGlInterface();
		//		grContext = GRContext.CreateGl(glInterface);
		//	}

		//	// manage the drawing surface
		//	if (renderTarget == null || surface == null || renderTarget.Width != (int)glView.Width || renderTarget.Height != (int)glView.Height)
		//	{
		//		// create or update the dimensions
		//		renderTarget?.Dispose();
		//		GL.GetInteger(GetPName.FramebufferBinding, out var framebuffer);
		//		GL.GetInteger(GetPName.StencilBits, out var stencil);
		//		GL.GetInteger(GetPName.Samples, out var samples);
		//		//var framebuffer = 1;
		//		//var stencil = 0;
		//		//var samples = 0;
		//		var maxSamples = grContext.GetMaxSurfaceSampleCount(colorType);
		//		if (samples > maxSamples)
		//			samples = maxSamples;
		//		var glInfo = new GRGlFramebufferInfo((uint)framebuffer, colorType.ToGlSizedFormat());
		//		renderTarget = new GRBackendRenderTarget((int)glView.Width, (int)glView.Height, samples, stencil, glInfo);

		//		// create the surface
		//		surface?.Dispose();
		//		surface = SKSurface.Create(grContext, renderTarget, surfaceOrigin, colorType);
		//	}

		//	using (new SKAutoCanvasRestore(surface.Canvas, true))
		//	{
		//		surface.Canvas.Clear(SKColors.Red);

		//		// start drawing
		//		//OnPaintSurface(new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType));
		//	}

		//	// update the control
		//	surface.Canvas.Flush();
		//}

		private void OnPaintGLSurface(object sender, SKPaintGLSurfaceEventArgs e)
		{
			// the the canvas and properties
			var canvas = e.Surface.Canvas;

			// get the screen density for scaling
			var scale = (float)(e.BackendRenderTarget.Width / skiaGLView.Width);
			scale = 1;

			// handle the device screen density
			canvas.Scale(scale);

			// make sure the canvas is blank
			canvas.Clear(SKColors.Red);

			// draw some text
			var paint = new SKPaint
			{
				Color = SKColors.Black,
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				TextAlign = SKTextAlign.Center,
				TextSize = 24
			};
			var coord = new SKPoint((float)skiaGLView.Width / 2, ((float)skiaGLView.Height + paint.TextSize) / 2);
			canvas.DrawText($"SkiaSharp {Pos} ({Down})", coord, paint);

			var w = e.BackendRenderTarget.Width;
			var h = e.BackendRenderTarget.Height;
			var p = new SKPaint {
				Color = SKColors.Green,
				Style = SKPaintStyle.Fill,
				Shader = SKShader.CreateLinearGradient(SKPoint.Empty, new SKPoint(0, w), new[] { SKColors.Blue, SKColors.Green }, SKShaderTileMode.Repeat)
			};
			canvas.DrawRect(0, 0, w, h, p);
		}
	}
}
