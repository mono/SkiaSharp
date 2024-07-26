using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Bitmaps
{
    public class PixelizedImagePage : ContentPage
    {
        SKBitmap pixelizedBitmap;

        public PixelizedImagePage ()
        {
            Title = "Pixelize Image";

            SKBitmap originalBitmap = BitmapExtensions.LoadBitmapResource(GetType(),
                "SkiaSharpFormsDemos.Media.MountainClimbers.jpg");

            // Create tiny bitmap for pixelized face
            SKBitmap faceBitmap = new SKBitmap(9, 9);

            // Copy subset of original bitmap to that
            using (SKCanvas canvas = new SKCanvas(faceBitmap))
            {
                canvas.Clear();
                canvas.DrawBitmap(originalBitmap,
                                  new SKRect(112, 238, 184, 310),   // source
                                  new SKRect(0, 0, 9, 9));          // destination

            }

            // Create full-sized bitmap for copy
            pixelizedBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height);

            using (SKCanvas canvas = new SKCanvas(pixelizedBitmap))
            {
                canvas.Clear();

                // Draw original in full size
                canvas.DrawBitmap(originalBitmap, new SKPoint());

                // Draw tiny bitmap to cover face
                canvas.DrawBitmap(faceBitmap, 
                                  new SKRect(112, 238, 184, 310));  // destination
            }

            // Create SKCanvasView to view result
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
            canvas.DrawBitmap(pixelizedBitmap, info.Rect, BitmapStretch.Uniform);
        }
    }
}