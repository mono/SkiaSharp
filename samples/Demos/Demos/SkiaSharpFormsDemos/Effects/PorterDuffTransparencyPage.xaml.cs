using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
    public partial class PorterDuffTransparencyPage : ContentPage
    {
        public PorterDuffTransparencyPage()
        {
            InitializeComponent();
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

            // Make square display rectangle smaller than canvas
            float size = 0.9f * Math.Min(info.Width, info.Height);
            float x = (info.Width - size) / 2;
            float y = (info.Height - size) / 2;
            SKRect rect = new SKRect(x, y, x + size, y + size);

            using (SKPaint paint = new SKPaint())
            {
                // Draw destination
                paint.Shader = SKShader.CreateLinearGradient(
                                    new SKPoint(rect.Right, rect.Top),
                                    new SKPoint(rect.Left, rect.Bottom),
                                    new SKColor[] { new SKColor(0xC0, 0x80, 0x00),
                                                    new SKColor(0xC0, 0x80, 0x00, 0) },
                                    new float[] { 0.4f, 0.6f },
                                    SKShaderTileMode.Clamp);

                canvas.DrawRect(rect, paint);

                // Draw source
                paint.Shader = SKShader.CreateLinearGradient(
                                    new SKPoint(rect.Left, rect.Top),
                                    new SKPoint(rect.Right, rect.Bottom),
                                    new SKColor[] { new SKColor(0x00, 0x80, 0xC0), 
                                                    new SKColor(0x00, 0x80, 0xC0, 0) },
                                    new float[] { 0.4f, 0.6f },
                                    SKShaderTileMode.Clamp);

                // Get the blend mode from the picker
                paint.BlendMode = blendModePicker.SelectedIndex == -1 ? 0 :
                                        (SKBlendMode)blendModePicker.SelectedItem;

                canvas.DrawRect(rect, paint);

                // Stroke surrounding rectangle
                paint.Shader = null;
                paint.BlendMode = SKBlendMode.SrcOver;
                paint.Style = SKPaintStyle.Stroke;
                paint.Color = SKColors.Black;
                paint.StrokeWidth = 3;
                canvas.DrawRect(rect, paint);
            }
        }
    }
}