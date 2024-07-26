using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
	public class ChainLinkFencePage : ContentPage
	{
        SKBitmap monkeyBitmap = BitmapExtensions.LoadBitmapResource(
            typeof(ChainLinkFencePage), "SkiaSharpFormsDemos.Media.SeatedMonkey.jpg");

        SKBitmap tileBitmap;

		public ChainLinkFencePage ()
		{
            Title = "Chain-Link Fence";

            // Create bitmap for chain-link tiling
            int tileSize = Device.Idiom == TargetIdiom.Desktop ? 64 : 128;
            tileBitmap = CreateChainLinkTile(tileSize);

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
		}

        SKBitmap CreateChainLinkTile(int tileSize)
        {
            tileBitmap = new SKBitmap(tileSize, tileSize);
            float wireThickness = tileSize / 12f;

            using (SKCanvas canvas = new SKCanvas(tileBitmap))
            using (SKPaint paint = new SKPaint())
            {
                canvas.Clear();
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = wireThickness;
                paint.IsAntialias = true;

                // Draw straight wires first
                paint.Shader = SKShader.CreateLinearGradient(new SKPoint(0, 0),
                                                             new SKPoint(0, tileSize),
                                                             new SKColor[] { SKColors.Silver, SKColors.Black },
                                                             new float[] { 0.4f, 0.6f },
                                                             SKShaderTileMode.Clamp);

                canvas.DrawLine(0, tileSize / 2,
                                tileSize / 2, tileSize / 2 - wireThickness / 2, paint);

                canvas.DrawLine(tileSize, tileSize / 2,
                                tileSize / 2, tileSize / 2 + wireThickness / 2, paint);

                // Draw curved wires
                using (SKPath path = new SKPath())
                {
                    path.MoveTo(tileSize / 2, 0);
                    path.LineTo(tileSize / 2 - wireThickness / 2, tileSize / 2);
                    path.ArcTo(wireThickness / 2, wireThickness / 2,
                               0,
                               SKPathArcSize.Small,
                               SKPathDirection.CounterClockwise,
                               tileSize / 2, tileSize / 2 + wireThickness / 2);

                    paint.Shader = SKShader.CreateLinearGradient(new SKPoint(0, 0),
                                                                 new SKPoint(0, tileSize),
                                                                 new SKColor[] { SKColors.Silver, SKColors.Black },
                                                                 null,
                                                                 SKShaderTileMode.Clamp);
                    canvas.DrawPath(path, paint);

                    path.Reset();
                    path.MoveTo(tileSize / 2, tileSize);
                    path.LineTo(tileSize / 2 + wireThickness / 2, tileSize / 2);
                    path.ArcTo(wireThickness / 2, wireThickness / 2,
                               0,
                               SKPathArcSize.Small,
                               SKPathDirection.CounterClockwise,
                               tileSize / 2, tileSize / 2 - wireThickness / 2);

                    paint.Shader = SKShader.CreateLinearGradient(new SKPoint(0, 0),
                                                                 new SKPoint(0, tileSize),
                                                                 new SKColor[] { SKColors.White, SKColors.Silver },
                                                                 null,
                                                                 SKShaderTileMode.Clamp);
                    canvas.DrawPath(path, paint);
                }
                return tileBitmap;
            }
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            canvas.DrawBitmap(monkeyBitmap, info.Rect, BitmapStretch.UniformToFill, 
                              BitmapAlignment.Center, BitmapAlignment.Start);

            using (SKPaint paint = new SKPaint())
            {
                paint.Shader = SKShader.CreateBitmap(tileBitmap, 
                                                     SKShaderTileMode.Repeat,
                                                     SKShaderTileMode.Repeat,
                                                     SKMatrix.MakeRotationDegrees(45));
                canvas.DrawRect(info.Rect, paint);
            }
        }
    }
}