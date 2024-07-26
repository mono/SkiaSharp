using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
    public partial class RadialGradientPage : ContentPage
    {
        public RadialGradientPage ()
        {
            InitializeComponent ();
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

            SKShaderTileMode tileMode =
                (SKShaderTileMode)(tileModePicker.SelectedIndex == -1 ?
                                            0 : tileModePicker.SelectedItem);

            using (SKPaint paint = new SKPaint())
            {
                paint.Shader = SKShader.CreateRadialGradient(
                                    new SKPoint(info.Rect.MidX, info.Rect.MidY),
                                    100,
                                    new SKColor[] { SKColors.Black, SKColors.White },
                                    null,
                                    tileMode);

                canvas.DrawRect(info.Rect, paint);
            }
        }
    }
}