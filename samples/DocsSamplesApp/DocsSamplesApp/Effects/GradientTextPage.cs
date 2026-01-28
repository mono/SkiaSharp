using System;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace DocsSamplesApp.Effects
{
	public class GradientTextPage : ContentPage
	{
        const string TEXT = "GRADIENT";

		public GradientTextPage ()
		{
            Title = "Gradient Text";

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            using (SKPaint paint = new SKPaint())
            using (SKFont font = new SKFont())
            {
                // Create gradient for background
                paint.Shader = SKShader.CreateLinearGradient(
                                    new SKPoint(0, 0),
                                    new SKPoint(info.Width, info.Height),
                                    new SKColor[] { new SKColor(0x40, 0x40, 0x40),
                                                    new SKColor(0xC0, 0xC0, 0xC0) },
                                    null,
                                    SKShaderTileMode.Clamp);

                // Draw background
                canvas.DrawRect(info.Rect, paint);

                // Set TextSize to fill 90% of width
                font.Size = 100;
                float width = font.MeasureText(TEXT);
                float scale = 0.9f * info.Width / width;
                font.Size *= scale;

                // Get text bounds
                SKRect textBounds = new SKRect();
                font.MeasureText(TEXT, out textBounds);

                // Calculate offsets to center the text on the screen
                float xText = info.Width / 2 - textBounds.MidX;
                float yText = info.Height / 2 - textBounds.MidY;

                // Shift textBounds by that amount
                textBounds.Offset(xText, yText);

                // Create gradient for text
                paint.Shader = SKShader.CreateLinearGradient(
                                    new SKPoint(textBounds.Left, textBounds.Top),
                                    new SKPoint(textBounds.Right, textBounds.Bottom),
                                    new SKColor[] { new SKColor(0x40, 0x40, 0x40),
                                                    new SKColor(0xC0, 0xC0, 0xC0) },
                                    null,
                                    SKShaderTileMode.Clamp);

                // Draw text
                canvas.DrawText(TEXT, xText, yText, SKTextAlign.Left, font, paint);
            }
        }
    }
}