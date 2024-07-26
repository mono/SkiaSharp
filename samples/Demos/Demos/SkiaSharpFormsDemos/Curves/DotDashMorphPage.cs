using System;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class DotDashMorphPage : ContentPage
    {
        const float strokeWidth = 30;
        static readonly float[] dashArray = new float[4];

        SKCanvasView canvasView;
        bool pageIsActive = false;

        SKPaint ellipsePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = strokeWidth,
            StrokeCap = SKStrokeCap.Round,
            Color = SKColors.Blue
        };

        public DotDashMorphPage()
        {
            Title = "Dot / Dash Morph";

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

            // Create elliptical path
            using (SKPath ellipsePath = new SKPath())
            {
                ellipsePath.AddOval(new SKRect(50, 50, info.Width - 50, info.Height - 50));

                // Create animated path effect 
                TimeSpan timeSpan = new TimeSpan(DateTime.Now.Ticks);
                float t = (float)(timeSpan.TotalSeconds % 3 / 3);
                float phase = 0;

                if (t < 0.25f)  // 1, 0, 1, 2 --> 0, 2, 0, 2
                {
                    float tsub = 4 * t;
                    dashArray[0] = strokeWidth * (1 - tsub);
                    dashArray[1] = strokeWidth * 2 * tsub;
                    dashArray[2] = strokeWidth * (1 - tsub);
                    dashArray[3] = strokeWidth * 2;
                }
                else if (t < 0.5f)  // 0, 2, 0, 2 --> 1, 2, 1, 0
                {
                    float tsub = 4 * (t - 0.25f);
                    dashArray[0] = strokeWidth * tsub;
                    dashArray[1] = strokeWidth * 2;
                    dashArray[2] = strokeWidth * tsub;
                    dashArray[3] = strokeWidth * 2 * (1 - tsub);
                    phase = strokeWidth * tsub;
                }
                else if (t < 0.75f) // 1, 2, 1, 0 --> 0, 2, 0, 2
                {
                    float tsub = 4 * (t - 0.5f);
                    dashArray[0] = strokeWidth * (1 - tsub);
                    dashArray[1] = strokeWidth * 2;
                    dashArray[2] = strokeWidth * (1 - tsub);
                    dashArray[3] = strokeWidth * 2 * tsub;
                    phase = strokeWidth * (1 - tsub);
                }
                else               // 0, 2, 0, 2 --> 1, 0, 1, 2
                {
                    float tsub = 4 * (t - 0.75f);
                    dashArray[0] = strokeWidth * tsub;
                    dashArray[1] = strokeWidth * 2 * (1 - tsub);
                    dashArray[2] = strokeWidth * tsub;
                    dashArray[3] = strokeWidth * 2;
                }

                using (SKPathEffect pathEffect = SKPathEffect.CreateDash(dashArray, phase))
                {
                    ellipsePaint.PathEffect = pathEffect;
                    canvas.DrawPath(ellipsePath, ellipsePaint);
                }
            }
        }
    }
}