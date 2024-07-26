using System;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Curves
{
    public partial class OneDimensionalPathEffectPage : ContentPage
    {
        SKPathEffect translatePathEffect =
            SKPathEffect.Create1DPath(SKPath.ParseSvgPathData("M -10 -10 L 10 -10, 10 10, -10 10 Z"),
                                      24, 0, SKPath1DPathEffectStyle.Translate); 

        SKPathEffect rotatePathEffect =
            SKPathEffect.Create1DPath(SKPath.ParseSvgPathData("M -10 0 L 0 -10, 10 0, 0 10 Z"),
                                      20, 0, SKPath1DPathEffectStyle.Rotate);

        SKPathEffect morphPathEffect =
            SKPathEffect.Create1DPath(SKPath.ParseSvgPathData("M -25 -10 L 25 -10, 25 10, -25 10 Z"),
                                      55, 0, SKPath1DPathEffectStyle.Morph);

        SKPaint pathPaint = new SKPaint
        {
            Color = SKColors.Blue
        };

        public OneDimensionalPathEffectPage()
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

            using (SKPath path = new SKPath())
            {
                path.MoveTo(new SKPoint(0, 0));
                path.CubicTo(new SKPoint(2 * info.Width, info.Height),
                             new SKPoint(-info.Width, info.Height),
                             new SKPoint(info.Width, 0));

                switch ((string)effectStylePicker.SelectedItem)
                {
                    case "Translate":
                        pathPaint.PathEffect = translatePathEffect;
                        break;

                    case "Rotate":
                        pathPaint.PathEffect = rotatePathEffect;
                        break;

                    case "Morph":
                        pathPaint.PathEffect = morphPathEffect;
                        break;
                }

                canvas.DrawPath(path, pathPaint);
            }
        }
    }
}
