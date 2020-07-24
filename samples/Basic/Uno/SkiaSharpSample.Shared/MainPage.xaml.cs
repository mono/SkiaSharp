using Windows.Graphics.Display;
using Windows.UI.Xaml.Controls;

using SkiaSharp;
using SkiaSharp.Views.UWP;
using System;

namespace SkiaSharpSample
{
	public sealed partial class MainPage : Page
	{
		private readonly FrameCounter fps = new FrameCounter();

		public MainPage()
		{
			InitializeComponent();
		}

		private void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
		{
			// the the canvas and properties
			var canvas = e.Surface.Canvas;

			// get the screen density for scaling
			var display = DisplayInformation.GetForCurrentView();
			var scale = display.LogicalDpi / 96.0f;
			var scaledSize = new SKSize(e.BackendRenderTarget.Width / scale, e.BackendRenderTarget.Height / scale);

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
			var coord = new SKPoint(scaledSize.Width / 2, (scaledSize.Height + paint.TextSize) / 2);
			canvas.DrawText("Skia is Sharp", coord, paint);

			paint.TextAlign = SKTextAlign.Left;
			coord.Y += 50;
			canvas.DrawText("FPS: " + fps.GetCurrentRate().ToString("#.00"), coord, paint);

			//skiaView.Invalidate();
		}
	}

	public class FrameCounter
	{
		private readonly int sampleCount;
		private int index;
		private int sum;
		private readonly int[] samples;

		private int lastTick;

		public FrameCounter(int sampleCount = 100)
		{
			this.sampleCount = sampleCount;
			lastTick = Environment.TickCount;
			samples = new int[sampleCount];
		}

		public float GetCurrentRate()
		{
			var ticks = System.Environment.TickCount;
			var delta = ticks - lastTick;
			lastTick = ticks;

			return 1000f / CalculateAverage(delta);
		}

		private float CalculateAverage(int delta)
		{
			sum -= samples[index];
			sum += delta;
			samples[index] = delta;

			if (++index == sampleCount)
				index = 0;

			return (float)sum / sampleCount;
		}
	}
}
