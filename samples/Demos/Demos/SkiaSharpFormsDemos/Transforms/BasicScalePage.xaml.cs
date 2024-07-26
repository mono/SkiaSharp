using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Transforms
{
    public partial class BasicScalePage : ContentPage
    {
        public BasicScalePage()
        {
            InitializeComponent();

            xScaleSlider.Value = 1;
            yScaleSlider.Value = 1;
        }

        void sliderValueChanged(object sender, ValueChangedEventArgs args)
        {
            if (canvasView != null)
            {
                canvasView.InvalidateSurface();
            }
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            using (SKPaint strokePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Red,
                StrokeWidth = 3,
                PathEffect = SKPathEffect.CreateDash(new float[] {  7, 7 }, 0)
            })
            using (SKPaint textPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Blue,
                TextSize = 50
            })
            {
                canvas.Scale((float)xScaleSlider.Value,
                             (float)yScaleSlider.Value);

                SKRect textBounds = new SKRect();
                textPaint.MeasureText(Title, ref textBounds);

                float margin = 10;
                SKRect borderRect = SKRect.Create(new SKPoint(margin, margin), textBounds.Size);
                canvas.DrawRoundRect(borderRect, 20, 20, strokePaint);
                canvas.DrawText(Title, margin, -textBounds.Top + margin, textPaint);
            }
        }
    }
}