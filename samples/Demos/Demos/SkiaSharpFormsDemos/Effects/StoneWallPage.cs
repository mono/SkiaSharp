using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
    public class StoneWallPage : ContentPage
    {
        SKBitmap bitmap = BitmapExtensions.LoadBitmapResource(
                            typeof(StoneWallPage),
                            "SkiaSharpFormsDemos.Media.StoneWallTile.jpg");

        public StoneWallPage()
        {
            Title = "Stone Wall";

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

            using (SKPaint paint = new SKPaint())
            {
                // Create scale transform
                SKMatrix matrix = SKMatrix.MakeScale(0.5f, 0.5f);

                // Create bitmap tiling
                paint.Shader = SKShader.CreateBitmap(bitmap,
                                                     SKShaderTileMode.Mirror,
                                                     SKShaderTileMode.Mirror,
                                                     matrix);
                // Draw background
                canvas.DrawRect(info.Rect, paint);
            }
        }
    }
}
