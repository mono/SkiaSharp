using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharpSample
{
	public partial class MainPage : ContentPage
	{
		public SKPoint Pos;
		public bool Down;

		public MainPage()
		{
			InitializeComponent();
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
			skiaGLView.InvalidateSurface();

			//e.Handled = true;
		}

		private void OnPaintGLSurface(object sender, SKPaintGLSurfaceEventArgs e)
		{
			// the the canvas and properties
			var canvas = e.Surface.Canvas;

			// get the screen density for scaling
			var scale = (float)(e.BackendRenderTarget.Width / skiaGLView.Width);

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

			var x = 0;
			var y = 0;
			var w = e.BackendRenderTarget.Width;
			var h = e.BackendRenderTarget.Height;
			var p = new SKPaint
			{
				Color = SKColors.Green,
				Style = SKPaintStyle.Stroke,
				StrokeWidth = 6,
				//Shader = SKShader.CreateLinearGradient(
				//	new SKPoint(0, 0),
				//	new SKPoint(w, 0),
				//	new[] { SKColors.Blue, SKColors.Green },
				//	SKShaderTileMode.Repeat)
			};
			canvas.DrawRect(x, y, w, h, p);
		}
	}
}
