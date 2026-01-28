using System;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;


namespace DocsSamplesApp.Basics
{
    public class SurfaceSizePage : ContentPage
    {
        SKCanvasView canvasView;

        public SurfaceSizePage()
        {
            Title = "Surface Size";

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            SKPaint paint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 40
            };

            float fontSpacing = paint.FontSpacing;
            float x = 20;               // left margin
            float y = fontSpacing;      // first baseline
            float indent = 100;

            canvas.DrawText("SKCanvasView Height and Width:", x, y, paint);
            y += fontSpacing;
            canvas.DrawText(String.Format("{0:F2} x {1:F2}", 
                                          canvasView.Width, canvasView.Height), 
                            x + indent, y, paint);
            y += fontSpacing * 2;
            canvas.DrawText("SKCanvasView CanvasSize:", x, y, paint);
            y += fontSpacing;
            canvas.DrawText(canvasView.CanvasSize.ToString(), x + indent, y, paint);
            y += fontSpacing * 2;
            canvas.DrawText("SKImageInfo Size:", x, y, paint);
            y += fontSpacing;
            canvas.DrawText(info.Size.ToString(), x + indent, y, paint);
        }
    }
}
