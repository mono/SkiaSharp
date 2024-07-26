using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class UnicycleHalfPipePage : ContentPage
    {
        SKCanvasView canvasView;
        bool pageIsActive;

        SKPaint strokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3,
            Color = SKColors.Black
        };

        SKPath unicyclePath = SKPath.ParseSvgPathData(
            "M 0 0" + 
            "A 25 25 0 0 0 0 -50" +
            "A 25 25 0 0 0 0 0 Z" +
            "M 0 -25 L 0 -100" +
            "A 15 15 0 0 0 0 -130" +
            "A 15 15 0 0 0 0 -100 Z" +
            "M -25 -85 L 25 -85");

        public UnicycleHalfPipePage()
        {
            Title = "Unicycle Half-Pipe";

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

            using (SKPath pipePath = new SKPath())
            {
                pipePath.MoveTo(50, 50);
                pipePath.CubicTo(0, 1.25f * info.Height, 
                                 info.Width - 0, 1.25f * info.Height,
                                 info.Width - 50, 50);

                canvas.DrawPath(pipePath, strokePaint);

                using (SKPathMeasure pathMeasure = new SKPathMeasure(pipePath))
                {
                    float length = pathMeasure.Length;

                    // Animate t from 0 to 1 every three seconds
                    TimeSpan timeSpan = new TimeSpan(DateTime.Now.Ticks);
                    float t = (float)(timeSpan.TotalSeconds % 5 / 5);

                    // t from 0 to 1 to 0 but slower at beginning and end
                    t = (float)((1 - Math.Cos(t * 2 * Math.PI)) / 2);

                    SKMatrix matrix;
                    pathMeasure.GetMatrix(t * length, out matrix, 
                                          SKPathMeasureMatrixFlags.GetPositionAndTangent);

                    canvas.SetMatrix(matrix);
                    canvas.DrawPath(unicyclePath, strokePaint);
                }
            }
        }
    }
}