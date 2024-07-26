using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Paths
{
    public partial class FivePointedStarPage : ContentPage
    {
        public FivePointedStarPage()
        {
            InitializeComponent();
        }

        void OnPickerSelectedIndexChanged(object sender, EventArgs args)
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

            SKPoint center = new SKPoint(info.Width / 2, info.Height / 2);
            float radius = 0.45f * Math.Min(info.Width, info.Height);

            SKPath path = new SKPath
            {
                FillType = (SKPathFillType)fillTypePicker.SelectedItem
            };
            path.MoveTo(info.Width / 2, info.Height / 2 - radius);

            for (int i = 1; i < 5; i++)
            {
                // angle from vertical
                double angle = i * 4 * Math.PI / 5;
                path.LineTo(center + new SKPoint(radius * (float)Math.Sin(angle), 
                                                -radius * (float)Math.Cos(angle)));
            }
            path.Close();

            SKPaint strokePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Red,
                StrokeWidth = 50,
                StrokeJoin = SKStrokeJoin.Round
            };

            SKPaint fillPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Blue
            };

            switch ((string)drawingModePicker.SelectedItem)
            {
                case "Fill only":
                    canvas.DrawPath(path, fillPaint);
                    break;

                case "Stroke only":
                    canvas.DrawPath(path, strokePaint);
                    break;

                case "Stroke then Fill":
                    canvas.DrawPath(path, strokePaint);
                    canvas.DrawPath(path, fillPaint);
                    break;

                case "Fill then Stroke":
                    canvas.DrawPath(path, fillPaint);
                    canvas.DrawPath(path, strokePaint);
                    break;
            }
        }
    }
}
