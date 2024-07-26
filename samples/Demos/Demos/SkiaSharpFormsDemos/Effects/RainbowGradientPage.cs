using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
    public class RainbowGradientPage : ContentPage
    {
        public RainbowGradientPage()
        {
            Title = "Rainbow Gradient";

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

            using (SKPath path = new SKPath())
            {
                float rainbowWidth = Math.Min(info.Width, info.Height) / 2f;

                // Create path from upper-left to lower-right corner
                path.MoveTo(0, 0);
                path.LineTo(rainbowWidth / 2, 0);
                path.LineTo(info.Width, info.Height - rainbowWidth / 2);
                path.LineTo(info.Width, info.Height);
                path.LineTo(info.Width - rainbowWidth / 2, info.Height);
                path.LineTo(0, rainbowWidth / 2);
                path.Close();

                using (SKPaint paint = new SKPaint())
                {
                    SKColor[] colors = new SKColor[8];

                    for (int i = 0; i < colors.Length; i++)
                    {
                        colors[i] = SKColor.FromHsl(i * 360f / (colors.Length - 1), 100, 50);
                    }

                    // Vector on lower-left edge, from top to bottom 
                    SKPoint edgeVector = new SKPoint(info.Width - rainbowWidth / 2, info.Height) -
                                            new SKPoint(0, rainbowWidth / 2);

                    // Rotate 90 degrees counter-clockwise:
                    SKPoint gradientVector = new SKPoint(edgeVector.Y, -edgeVector.X);

                    // Normalize
                    float length = (float)Math.Sqrt(Math.Pow(gradientVector.X, 2) +
                                                    Math.Pow(gradientVector.Y, 2));
                    gradientVector.X /= length;
                    gradientVector.Y /= length;

                    // Make it the width of the rainbow
                    gradientVector.X *= rainbowWidth;
                    gradientVector.Y *= rainbowWidth;

                    // Calculate the two points
                    SKPoint point1 = new SKPoint(0, rainbowWidth / 2);
                    SKPoint point2 = point1 + gradientVector;

                    paint.Shader = SKShader.CreateLinearGradient(point1,
                                                                 point2,
                                                                 colors,
                                                                 null,
                                                                 SKShaderTileMode.Repeat);

                    canvas.DrawPath(path, paint);
                }
            }
        }
    }
}