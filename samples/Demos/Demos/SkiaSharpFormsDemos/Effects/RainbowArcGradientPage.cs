using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
	public class RainbowArcGradientPage : ContentPage
	{
		public RainbowArcGradientPage ()
		{
            Title = "Rainbow Arc Gradient";

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            using (SKPaint paint = new SKPaint())
            {
                float rainbowWidth = Math.Min(info.Width, info.Height) / 4f;

                // Center of arc and gradient is lower-right corner
                SKPoint center = new SKPoint(info.Width, info.Height);

                // Find outer, inner, and middle radius
                float outerRadius = Math.Min(info.Width, info.Height);
                float innerRadius = outerRadius - rainbowWidth;
                float radius = outerRadius - rainbowWidth / 2;

                // Calculate the colors and positions
                SKColor[] colors = new SKColor[8];
                float[] positions = new float[8];

                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = SKColor.FromHsl(i * 360f / 7, 100, 50);
                    positions[i] = (i + (7f - i) * innerRadius / outerRadius) / 7f;
                }

                // Create sweep gradient based on center and outer radius
                paint.Shader = SKShader.CreateRadialGradient(center, 
                                                             outerRadius, 
                                                             colors, 
                                                             positions, 
                                                             SKShaderTileMode.Clamp);
                // Draw a circle with a wide line
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = rainbowWidth;

                canvas.DrawCircle(center, radius, paint);
            }
        }
    }
}