using System.Windows.Forms;

using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace SkiaSharpSample
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void skiaView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			// the the canvas and properties
			var canvas = e.Surface.Canvas;

			// make sure the canvas is blank
			canvas.Clear(SKColors.White);

			// draw some text
			using var paint = new SKPaint
			{
				Color = SKColors.Black,
				IsAntialias = true,
				Style = SKPaintStyle.Fill
			};
			using var font = new SKFont
			{
				Size = 24
			};
			var coord = new SKPoint(e.Info.Width / 2, (e.Info.Height + font.Size) / 2);
			canvas.DrawText("SkiaSharp", coord, SKTextAlign.Center, font, paint);
		}
	}
}
