using SkiaSharp;
using SkiaSharp.Views;
using Windows.UI.Xaml.Controls;

namespace BasicSkiaSharp
{
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			InitializeComponent();

			swapChainPanel.CompositionScaleChanged += (sender, e) => System.Diagnostics.Debug.WriteLine(
				$"{swapChainPanel.CompositionScaleX}x{swapChainPanel.CompositionScaleY}");
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
				canvas.DrawText("Hello UWP World!", 30, textSize + 10, paint);

				paint.Color = SKColors.Orange.WithAlpha(100);
				canvas.DrawOval(SKRect.Create(50, 50, 100, 100), paint);

				paint.IsStroke = true;
				paint.StrokeWidth = stroke;
				paint.Color = SKColors.Black;
				r.Inflate(shrink, shrink);
				canvas.DrawRoundRect(r, curve - stroke, curve - stroke, paint);
			}
		}

		// from the hardware panel
		private void swapChainPanel_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
		{
			Draw(e.Surface, new SKSize(e.RenderTarget.Width, e.RenderTarget.Height));
		}
	}
}
