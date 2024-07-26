using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
    public class TileAlignmentPage : ContentPage
    {
        bool isAligned;

        public TileAlignmentPage()
        {
            Title = "Tile Alignment";

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;

            // Add tap handler
            TapGestureRecognizer tap = new TapGestureRecognizer();
            tap.Tapped += (sender, args) =>
            {
                isAligned ^= true;
                canvasView.InvalidateSurface();
            };
            canvasView.GestureRecognizers.Add(tap);

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
                SKRect rect = new SKRect(info.Width / 7,
                                            info.Height / 7,
                                            6 * info.Width / 7,
                                            6 * info.Height / 7);

                // Get bitmap from other program
                SKBitmap bitmap = AlgorithmicBrickWallPage.BrickWallTile;

                // Create bitmap tiling
                if (!isAligned)
                {
                    paint.Shader = SKShader.CreateBitmap(bitmap,
                                                         SKShaderTileMode.Repeat,
                                                         SKShaderTileMode.Repeat);
                }
                else
                {
                    SKMatrix matrix = SKMatrix.MakeTranslation(rect.Left, rect.Top);

                    paint.Shader = SKShader.CreateBitmap(bitmap,
                                                         SKShaderTileMode.Repeat,
                                                         SKShaderTileMode.Repeat,
                                                         matrix);
                }

                // Draw rectangle
                canvas.DrawRect(rect, paint);
            }
        }
    }
}