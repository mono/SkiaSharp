using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class SquaringTheCirclePage : ContentPage
    {
        SKCanvasView canvasView;
        bool pageIsActive;

        SKPoint[,] points =
        {
            { new SKPoint(   0,  100), new SKPoint(     0,    125), new SKPoint() },
            { new SKPoint(  55,  100), new SKPoint( 62.5f,  62.5f), new SKPoint() },
            { new SKPoint( 100,   55), new SKPoint( 62.5f,  62.5f), new SKPoint() },
            { new SKPoint( 100,    0), new SKPoint(   125,      0), new SKPoint() },
            { new SKPoint( 100,  -55), new SKPoint( 62.5f, -62.5f), new SKPoint() },
            { new SKPoint(  55, -100), new SKPoint( 62.5f, -62.5f), new SKPoint() },
            { new SKPoint(   0, -100), new SKPoint(     0,   -125), new SKPoint() },
            { new SKPoint( -55, -100), new SKPoint(-62.5f, -62.5f), new SKPoint() },
            { new SKPoint(-100,  -55), new SKPoint(-62.5f, -62.5f), new SKPoint() },
            { new SKPoint(-100,    0), new SKPoint(  -125,      0), new SKPoint() },
            { new SKPoint(-100,   55), new SKPoint(-62.5f,  62.5f), new SKPoint() },
            { new SKPoint( -55,  100), new SKPoint(-62.5f,  62.5f), new SKPoint() },
            { new SKPoint(   0,  100), new SKPoint(     0,    125), new SKPoint() }
        };

        SKPaint blueStroke = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Blue,
            StrokeWidth = 10
        };

        SKPaint cyanFill = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Cyan
        };

        public SquaringTheCirclePage()
        {
            Title = "Squaring the Circle";

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            pageIsActive = true;

            Device.StartTimer(TimeSpan.FromSeconds(1f / 60), () =>
            {
                canvasView.InvalidateSurface();
                return pageIsActive;
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            pageIsActive = false;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            canvas.Translate(info.Width / 2, info.Height / 2);
            canvas.Scale(Math.Min(info.Width / 300, info.Height / 300));

            // Interpolate
            TimeSpan timeSpan = new TimeSpan(DateTime.Now.Ticks);
            float t = (float)(timeSpan.TotalSeconds % 3 / 3);   // 0 to 1 every 3 seconds
            t = (1 + (float)Math.Sin(2 * Math.PI * t)) / 2;     // 0 to 1 to 0 sinusoidally

            for (int i = 0; i < 13; i++)
            {
                points[i, 2] = new SKPoint(
                    (1 - t) * points[i, 0].X + t * points[i, 1].X,
                    (1 - t) * points[i, 0].Y + t * points[i, 1].Y);
            }

            // Create the path and draw it
            using (SKPath path = new SKPath())
            {
                path.MoveTo(points[0, 2]);

                for (int i = 1; i < 13; i += 3)
                {
                    path.CubicTo(points[i, 2], points[i + 1, 2], points[i + 2, 2]);
                }
                path.Close();

                canvas.DrawPath(path, cyanFill);
                canvas.DrawPath(path, blueStroke);
            }
        }
    }
}
