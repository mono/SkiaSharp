using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Paths
{
    public partial class MultipleLinesPage : ContentPage
    {
        public MultipleLinesPage()
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

            // Create an array of points scattered through the page
            SKPoint[] points = new SKPoint[10];

            for (int i = 0; i < 2; i++)
            {
                float x = (0.1f + 0.8f * i) * info.Width;

                for (int j = 0; j < 5; j++)
                {
                    float y = (0.1f + 0.2f * j) * info.Height;
                    points[2 * j + i] = new SKPoint(x, y);
                }
            }

            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.DarkOrchid,
                StrokeWidth = 50,
                StrokeCap = (SKStrokeCap)strokeCapPicker.SelectedItem
            };

            // Render the points by calling DrawPoints
            SKPointMode pointMode = (SKPointMode)pointModePicker.SelectedItem;
            canvas.DrawPoints(pointMode, points, paint);
        }
    }
}
