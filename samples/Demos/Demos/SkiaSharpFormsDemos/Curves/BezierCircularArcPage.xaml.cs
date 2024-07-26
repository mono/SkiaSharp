using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public partial class BezierCircularArcPage : ContentPage
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

        public BezierCircularArcPage()
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
            float radius = Math.Min(info.Width, info.Height) / 3;
            canvas.DrawCircle(0, 0, radius, blackStroke);

            // Get the value of the Slider
            float angle = (float)angleSlider.Value;

            // Calculate length of control point line
            float length = radius * 4 * (float)Math.Tan(Math.PI * angle / 180 / 4) / 3;

            // Calculate sin and cosine for half that angle
            float sin = (float)Math.Sin(Math.PI * angle / 180 / 2);
            float cos = (float)Math.Cos(Math.PI * angle / 180 / 2);

            // Find the end points 
            SKPoint point0 = new SKPoint(-radius * sin, radius * cos);
            SKPoint point3 = new SKPoint(radius * sin, radius * cos);

            // Find the control points
            SKPoint point0Normalized = Normalize(point0);
            SKPoint point1 = point0 + new SKPoint(length * point0Normalized.Y,
                                                    -length * point0Normalized.X);

            SKPoint point3Normalized = Normalize(point3);
            SKPoint point2 = point3 + new SKPoint(-length * point3Normalized.Y,
                                                    length * point3Normalized.X);

            // Draw the points
            canvas.DrawCircle(point0.X, point0.Y, 10, blackFill);
            canvas.DrawCircle(point1.X, point1.Y, 10, blackFill);
            canvas.DrawCircle(point2.X, point2.Y, 10, blackFill);
            canvas.DrawCircle(point3.X, point3.Y, 10, blackFill);

            // Draw the tangent lines
            canvas.DrawLine(point0.X, point0.Y, point1.X, point1.Y, dottedStroke);
            canvas.DrawLine(point3.X, point3.Y, point2.X, point2.Y, dottedStroke);

            // Draw the Bezier curve
            using (SKPath path = new SKPath())
            {
                path.MoveTo(point0);
                path.CubicTo(point1, point2, point3);
                canvas.DrawPath(path, redStroke);
            }
        }

        // Vector methods
        SKPoint Normalize(SKPoint v)
        {
            float magnitude = Magnitude(v);
            return new SKPoint(v.X / magnitude, v.Y / magnitude);
        }

        float Magnitude(SKPoint v)
        {
            return (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
        }
    }
}
