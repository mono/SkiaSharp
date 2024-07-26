using System;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
	public partial class ConicalGradientPage : InteractivePage
	{
        const int RADIUS1 = 50;
        const int RADIUS2 = 100;

		public ConicalGradientPage ()
		{
            touchPoints = new TouchPoint[2];

            touchPoints[0] = new TouchPoint
            {
                Center = new SKPoint(100, 100),
                Radius = RADIUS1
            };

            touchPoints[1] = new TouchPoint
            {
                Center = new SKPoint(300, 300),
                Radius = RADIUS2
            };

            InitializeComponent();
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
                paint.Shader = SKShader.CreateTwoPointConicalGradient(touchPoints[0].Center,
                                                                      RADIUS1,
                                                                      touchPoints[1].Center,
                                                                      RADIUS2,
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
            }
        }
    }
}