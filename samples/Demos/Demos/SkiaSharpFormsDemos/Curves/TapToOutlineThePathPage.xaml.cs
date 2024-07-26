using System;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public partial class TapToOutlineThePathPage : ContentPage
    {
        bool outlineThePath = false;

        SKPaint redThickStroke = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Red,
            StrokeWidth = 100
        };

        SKPaint redThinStroke = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Red,
            StrokeWidth = 20
        };

        SKPaint blueFill = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Blue
        };

        public TapToOutlineThePathPage()
        {
            InitializeComponent();
        }

        void OnCanvasViewTapped(object sender, EventArgs args)
        {
            outlineThePath ^= true;
            (sender as SKCanvasView).InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            using (SKPath circlePath = new SKPath())
            {
                circlePath.AddCircle(info.Width / 2, info.Height / 2,
                                     Math.Min(info.Width / 2, info.Height / 2) - 
                                     redThickStroke.StrokeWidth);

                if (!outlineThePath)
                {
                    canvas.DrawPath(circlePath, blueFill);
                    canvas.DrawPath(circlePath, redThickStroke);
                }
                else
                {
                    using (SKPath outlinePath = new SKPath())
                    {
                        redThickStroke.GetFillPath(circlePath, outlinePath);

                        canvas.DrawPath(outlinePath, blueFill);
                        canvas.DrawPath(outlinePath, redThinStroke);
                    }
                }
            }
        }
    }
}