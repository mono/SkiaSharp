using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public partial class ConicCircularArcPage : ContentPage
    {
        SKPaint blackFill = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Black
        };

        SKPaint blackStroke = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 3
        };

        SKPaint dottedStroke = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 3,
            PathEffect = SKPathEffect.CreateDash(new float[] { 7, 7 }, 0)
        };

        SKPaint redStroke = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Red,
            StrokeWidth = 6
        };

        public ConicCircularArcPage()
        {
            InitializeComponent();

            angleSlider.Value = 90;
        }

        void OnSliderValueChanged(object sender, ValueChangedEventArgs args)
        {
            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            // Translate to center
            canvas.Translate(info.Width / 2, info.Height / 2);

            // Draw the circle
            float radius = Math.Min(info.Width, info.Height) / 4;
            canvas.DrawCircle(0, 0, radius, blackStroke);

            // Get the value of the Slider
            float angle = (float)angleSlider.Value;

            // Calculate sin and cosine for half that angle
            float sin = (float)Math.Sin(Math.PI * angle / 180 / 2);
            float cos = (float)Math.Cos(Math.PI * angle / 180 / 2);

            // Find the points and weight
            SKPoint point0 = new SKPoint(-radius * sin, radius * cos);
            SKPoint point1 = new SKPoint(0, radius / cos);
            SKPoint point2 = new SKPoint(radius * sin, radius * cos);
            float weight = cos;

            // Draw the points
            canvas.DrawCircle(point0.X, point0.Y, 10, blackFill);
            canvas.DrawCircle(point1.X, point1.Y, 10, blackFill);
            canvas.DrawCircle(point2.X, point2.Y, 10, blackFill);

            // Draw the tangent lines
            canvas.DrawLine(point0.X, point0.Y, point1.X, point1.Y, dottedStroke);
            canvas.DrawLine(point2.X, point2.Y, point1.X, point1.Y, dottedStroke);

            // Draw the conic
            using (SKPath path = new SKPath())
            {
                path.MoveTo(point0);
                path.ConicTo(point1, point2, weight);
                canvas.DrawPath(path, redStroke);
            }
        }
    }
}
