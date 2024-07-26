using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class GlobularTextPage : ContentPage
    {
        SKPath globePath;

        public GlobularTextPage()
        {
            Title = "Globular Text";

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;

            using (SKPaint textPaint = new SKPaint())
            {
                textPaint.Typeface = SKTypeface.FromFamilyName("Times New Roman");
                textPaint.TextSize = 100;

                using (SKPath textPath = textPaint.GetTextPath("HELLO", 0, 0))
                {
                    SKRect textPathBounds;
                    textPath.GetBounds(out textPathBounds);

                    globePath = textPath.CloneWithTransform((SKPoint pt) =>
                    {
                        double longitude = (Math.PI / textPathBounds.Width) * 
                                                (pt.X - textPathBounds.Left) - Math.PI / 2;
                        double latitude = (Math.PI / textPathBounds.Height) * 
                                                (pt.Y - textPathBounds.Top) - Math.PI / 2;

                        longitude *= 0.75;
                        latitude *= 0.75;

                        float x = (float)(Math.Cos(latitude) * Math.Sin(longitude));
                        float y = (float)Math.Sin(latitude);

                        return new SKPoint(x, y);
                    });
                }
            }
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            using (SKPaint pathPaint = new SKPaint())
            {
                pathPaint.Style = SKPaintStyle.Fill;
                pathPaint.Color = SKColors.Blue;
                pathPaint.StrokeWidth = 3;
                pathPaint.IsAntialias = true;

                canvas.Translate(info.Width / 2, info.Height / 2);
                canvas.Scale(0.45f * Math.Min(info.Width, info.Height));     // radius
                canvas.DrawPath(globePath, pathPaint);
            }
        }
    }
}