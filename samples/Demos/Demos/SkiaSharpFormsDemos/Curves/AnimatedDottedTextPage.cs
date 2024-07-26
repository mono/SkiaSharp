using System;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public class AnimatedDottedTextPage : ContentPage
    {
        const string text = "DOTTED";
        const float strokeWidth = 10;
        static readonly float[] dashArray = { 0, 2 * strokeWidth };

        SKCanvasView canvasView;
        bool pageIsActive;

        public AnimatedDottedTextPage()
        {
            Title = "Animated Dotted Text";

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

            // Create an SKPaint object to display the text
            using (SKPaint textPaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = strokeWidth,
                    StrokeCap = SKStrokeCap.Round,
                    Color = SKColors.Blue,
                })
            {
                // Adjust TextSize property so text is 95% of screen width
                float textWidth = textPaint.MeasureText(text);
                textPaint.TextSize *= 0.95f * info.Width / textWidth;

                // Find the text bounds
                SKRect textBounds = new SKRect();
                textPaint.MeasureText(text, ref textBounds);

                // Calculate offsets to center the text on the screen
                float xText = info.Width / 2 - textBounds.MidX;
                float yText = info.Height / 2 - textBounds.MidY;

                // Animate the phase; t is 0 to 1 every second
                TimeSpan timeSpan = new TimeSpan(DateTime.Now.Ticks);
                float t = (float)(timeSpan.TotalSeconds % 1 / 1);
                float phase = -t * 2 * strokeWidth;

                // Create dotted line effect based on dash array and phase
                using (SKPathEffect dashEffect = SKPathEffect.CreateDash(dashArray, phase))
                {
                    // Set it to the paint object
                    textPaint.PathEffect = dashEffect;

                    // And draw the text
                    canvas.DrawText(text, xText, yText, textPaint);
                }
            }
        }
    }
}
