using System;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
    public partial class InteractiveLinearGradientPage : InteractivePage
    {
	    public InteractiveLinearGradientPage ()
	    {
		    InitializeComponent ();

            touchPoints = new TouchPoint[2];

            for (int i = 0; i < 2; i++)
            { 
                touchPoints[i] = new TouchPoint
                {
                    Center = new SKPoint(100 + i * 200, 100 + i * 200)
                };
            }

            baseCanvasView = canvasView;
        }

        void OnPickerSelectedIndexChanged(object sender, EventArgs args)
        {
            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            SKColor[] colors = { SKColors.Red, SKColors.Green, SKColors.Blue };
            SKShaderTileMode tileMode =
                (SKShaderTileMode)(tileModePicker.SelectedIndex == -1 ?
                                            0 : tileModePicker.SelectedItem);

            using (SKPaint paint = new SKPaint())
            {
                paint.Shader = SKShader.CreateLinearGradient(touchPoints[0].Center,
                                                             touchPoints[1].Center,
                                                             colors,
                                                             null,
                                                             tileMode);
                canvas.DrawRect(info.Rect, paint);
            }

            // Display the touch points here rather than by TouchPoint
            using (SKPaint paint = new SKPaint())
            {
                paint.Style = SKPaintStyle.Stroke;
                paint.Color = SKColors.Black;
                paint.StrokeWidth = 3;

                foreach (TouchPoint touchPoint in touchPoints)
                {
                    canvas.DrawCircle(touchPoint.Center, touchPoint.Radius, paint);
                }

                // Draw gradient line connecting touchpoints
                canvas.DrawLine(touchPoints[0].Center, touchPoints[1].Center, paint);

                // Draw lines perpendicular to the gradient line
                SKPoint vector = touchPoints[1].Center - touchPoints[0].Center;
                float length = (float)Math.Sqrt(Math.Pow(vector.X, 2) +
                                                Math.Pow(vector.Y, 2));
                vector.X /= length;
                vector.Y /= length;
                SKPoint rotate90 = new SKPoint(-vector.Y, vector.X);
                rotate90.X *= 200;
                rotate90.Y *= 200;

                canvas.DrawLine(touchPoints[0].Center, 
                                touchPoints[0].Center + rotate90, 
                                paint);

                canvas.DrawLine(touchPoints[0].Center,
                                touchPoints[0].Center - rotate90,
                                paint);

                canvas.DrawLine(touchPoints[1].Center,
                                touchPoints[1].Center + rotate90,
                                paint);

                canvas.DrawLine(touchPoints[1].Center,
                                touchPoints[1].Center - rotate90,
                                paint);
            }
        }
    }
}