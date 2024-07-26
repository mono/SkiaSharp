using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public partial class TangentArcPage : InteractivePage
    {
        public TangentArcPage()
        {
            touchPoints = new TouchPoint[3];

            for (int i = 0; i < 3; i++)
            {
                TouchPoint touchPoint = new TouchPoint
                {
                    Center = new SKPoint(i == 0 ? 100 : 500,
                                         i != 2 ? 100 : 500)
                };
                touchPoints[i] = touchPoint;
            }

            InitializeComponent();

            baseCanvasView = canvasView;
            radiusSlider.Value = 100;
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

            // Draw the two lines that meet at an angle
            using (SKPath path = new SKPath())
            {
                path.MoveTo(touchPoints[0].Center);
                path.LineTo(touchPoints[1].Center);
                path.LineTo(touchPoints[2].Center);
                canvas.DrawPath(path, dottedStrokePaint);
            }

            // Draw the circle that the arc wraps around
            float radius = (float)radiusSlider.Value;

            SKPoint v1 = Normalize(touchPoints[0].Center - touchPoints[1].Center); 
            SKPoint v2 = Normalize(touchPoints[2].Center - touchPoints[1].Center); 

            double dotProduct = v1.X * v2.X + v1.Y * v2.Y;
            double angleBetween = Math.Acos(dotProduct);
            float hypotenuse = radius / (float)Math.Sin(angleBetween / 2);
            SKPoint vMid = Normalize(new SKPoint((v1.X + v2.X) / 2, (v1.Y + v2.Y) / 2));
            SKPoint center = new SKPoint(touchPoints[1].Center.X + vMid.X * hypotenuse,
                                         touchPoints[1].Center.Y + vMid.Y * hypotenuse);

            canvas.DrawCircle(center.X, center.Y, radius, this.strokePaint);

            // Draw the tangent arc
            using (SKPath path = new SKPath())
            {
                path.MoveTo(touchPoints[0].Center);
                path.ArcTo(touchPoints[1].Center, touchPoints[2].Center, radius);
                canvas.DrawPath(path, redStrokePaint);
            }

            foreach (TouchPoint touchPoint in touchPoints)
            {
                touchPoint.Paint(canvas);
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
