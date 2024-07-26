using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Bitmaps
{
    public partial class MonkeyMoustachePage : ContentPage
    {
        SKBitmap monkeyBitmap;

        public MonkeyMoustachePage()
        {
            Title = "Monkey Moustache";

            monkeyBitmap = BitmapExtensions.LoadBitmapResource(GetType(),
                "SkiaSharpFormsDemos.Media.MonkeyFace.png");

            // Create canvas based on bitmap
            using (SKCanvas canvas = new SKCanvas(monkeyBitmap))
            {
                using (SKPaint paint = new SKPaint())
                {
                    paint.Style = SKPaintStyle.Stroke;
                    paint.Color = SKColors.Black;
                    paint.StrokeWidth = 24;
                    paint.StrokeCap = SKStrokeCap.Round;

                    using (SKPath path = new SKPath())
                    {
                        path.MoveTo(380, 390);
                        path.CubicTo(560, 390, 560, 280, 500, 280);

                        path.MoveTo(320, 390);
                        path.CubicTo(140, 390, 140, 280, 200, 280);

                        canvas.DrawPath(path, paint);
                    }
                }
            }

            // Create SKCanvasView to view result
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
            canvas.DrawBitmap(monkeyBitmap, info.Rect, BitmapStretch.Uniform);
        }
    }
}