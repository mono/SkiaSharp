using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
	public class CompositingMaskPage : ContentPage
	{
        SKBitmap bitmap = BitmapExtensions.LoadBitmapResource(
            typeof(CompositingMaskPage),
            "SkiaSharpFormsDemos.Media.MountainClimbers.jpg");

        static readonly SKPoint CENTER = new SKPoint(180, 300);
        static readonly float RADIUS = 120;

        public CompositingMaskPage ()
		{
            Title = "Compositing Mask";

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            // Find rectangle to display bitmap
            float scale = Math.Min((float)info.Width / bitmap.Width,
                                   (float)info.Height / bitmap.Height);

            SKRect rect = SKRect.Create(scale * bitmap.Width, scale * bitmap.Height);

            float x = (info.Width - rect.Width) / 2;
            float y = (info.Height - rect.Height) / 2;
            rect.Offset(x, y);

            // Display bitmap in rectangle
            canvas.DrawBitmap(bitmap, rect);

            // Adjust center and radius for scaled and offset bitmap
            SKPoint center = new SKPoint(scale * CENTER.X + x,
                                         scale * CENTER.Y + y);
            float radius = scale * RADIUS;

            using (SKPaint paint = new SKPaint())
            {
                paint.Shader = SKShader.CreateRadialGradient(
                                    center,
                                    radius,
                                    new SKColor[] { SKColors.Black,
                                                    SKColors.Transparent },
                                    new float[] { 0.6f, 1 },
                                    SKShaderTileMode.Clamp);

                paint.BlendMode = SKBlendMode.DstIn;

                // Display rectangle using that gradient and blend mode
                canvas.DrawRect(rect, paint);
            }

            canvas.DrawColor(SKColors.Pink, SKBlendMode.DstOver);
        }
    }
}
