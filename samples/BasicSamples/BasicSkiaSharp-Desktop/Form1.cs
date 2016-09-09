using System.Windows.Forms;
using SkiaSharp;
using SkiaSharp.Views;

namespace BasicSkiaSharp
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		// the real draw method
		private static void Draw(SKSurface surface, SKSize size)
		{
			const int stroke = 4;
			const int curve = 20;
			const int textSize = 60;
			const int shrink = stroke / -2;

			var canvas = surface.Canvas;

			canvas.Clear(SKColors.Transparent);

			using (var paint = new SKPaint())
			{
				paint.IsAntialias = true;
				paint.TextSize = textSize;

				paint.Color = SKColors.Orchid;
				var r = SKRect.Create(SKPoint.Empty, size);
				canvas.DrawRoundRect(r, curve, curve, paint);

				paint.Color = SKColors.GreenYellow;
				canvas.DrawText("Hello WinForms World!", 30, textSize + 10, paint);

				paint.Color = SKColors.Orange.WithAlpha(100);
				canvas.DrawOval(SKRect.Create(50, 50, 100, 100), paint);

				paint.IsStroke = true;
				paint.StrokeWidth = stroke;
				paint.Color = SKColors.Black;
				r.Inflate(shrink, shrink);
				canvas.DrawRoundRect(r, curve - stroke, curve - stroke, paint);
			}
		}

		// from the software view
		private void softwareControl_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			Draw(e.Surface, e.Info.Size);
		}

		// from the software view
		private void hardwareControl_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
		{
			Draw(e.Surface, new SKSize(e.RenderTarget.Width, e.RenderTarget.Height));
		}
	}
}
