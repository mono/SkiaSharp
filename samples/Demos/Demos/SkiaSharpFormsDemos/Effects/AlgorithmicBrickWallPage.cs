using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
	public class AlgorithmicBrickWallPage : ContentPage
	{
        static AlgorithmicBrickWallPage()
        {
            const int brickWidth = 64;
            const int brickHeight = 24;
            const int morterThickness = 6;
            const int bitmapWidth = brickWidth + morterThickness;
            const int bitmapHeight = 2 * (brickHeight + morterThickness);

            SKBitmap bitmap = new SKBitmap(bitmapWidth, bitmapHeight);

            using (SKCanvas canvas = new SKCanvas(bitmap))
            using (SKPaint brickPaint = new SKPaint())
            {
                brickPaint.Color = new SKColor(0xB2, 0x22, 0x22);

                canvas.Clear(new SKColor(0xF0, 0xEA, 0xD6));
                canvas.DrawRect(new SKRect(morterThickness / 2,
                                           morterThickness / 2,
                                           morterThickness / 2 + brickWidth,
                                           morterThickness / 2 + brickHeight),
                                           brickPaint);

                int ySecondBrick = 3 * morterThickness / 2 + brickHeight;

                canvas.DrawRect(new SKRect(0,
                                           ySecondBrick,
                                           bitmapWidth / 2 - morterThickness / 2,
                                           ySecondBrick + brickHeight),
                                           brickPaint);

                canvas.DrawRect(new SKRect(bitmapWidth / 2 + morterThickness / 2,
                                           ySecondBrick,
                                           bitmapWidth,
                                           ySecondBrick + brickHeight),
                                           brickPaint);
            }

            // Save as public property for other programs
            BrickWallTile = bitmap;
        }

        public static SKBitmap BrickWallTile { private set; get; }

        public AlgorithmicBrickWallPage ()
		{
            Title = "Algorithmic Brick Wall";

            // Create SKCanvasView
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
                // Create bitmap tiling
                paint.Shader = SKShader.CreateBitmap(BrickWallTile,
                                                     SKShaderTileMode.Repeat,
                                                     SKShaderTileMode.Repeat);
                // Draw background
                canvas.DrawRect(info.Rect, paint);
            }
        }
    }
}