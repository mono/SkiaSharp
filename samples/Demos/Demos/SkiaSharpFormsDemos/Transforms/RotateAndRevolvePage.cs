using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Transforms
{
    public class RotateAndRevolvePage : ContentPage
    {
        SKCanvasView canvasView;
        float revolveDegrees, rotateDegrees;

        public RotateAndRevolvePage()
        {
            Title = "Rotate and Revolve";

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            new Animation((value) => revolveDegrees = 360 * (float)value).
                Commit(this, "revolveAnimation", length: 10000, repeat: () => true);

            new Animation((value) =>
            {
                rotateDegrees = 360 * (float)value;
                canvasView.InvalidateSurface();
            }).Commit(this, "rotateAnimation", length: 1000, repeat: () => true);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            this.AbortAnimation("revolveAnimation");
            this.AbortAnimation("rotateAnimation");
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            using (SKPaint fillPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Red
            })
            {
                // Translate to center of canvas
                canvas.Translate(info.Width / 2, info.Height / 2);

                // Rotate around center of canvas
                canvas.RotateDegrees(revolveDegrees);

                // Translate horizontally
                float radius = Math.Min(info.Width, info.Height) / 3;
                canvas.Translate(radius, 0);

                // Rotate around center of object
                canvas.RotateDegrees(rotateDegrees);

                // Draw a square
                canvas.DrawRect(new SKRect(-50, -50, 50, 50), fillPaint);
            }
        }
    }
}
