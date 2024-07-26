using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
    public partial class TiledPerlinNoisePage : ContentPage
    {
        const int TILE_SIZE = 200;

        public TiledPerlinNoisePage()
        {
            InitializeComponent();
        }

        void OnStepperValueChanged(object sender, ValueChangedEventArgs args)
        {
            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            // Get seed value from stepper
            float seed = (float)seedStepper.Value;

            SKRect tileRect = new SKRect(0, 0, TILE_SIZE, TILE_SIZE);

            using (SKBitmap bitmap = new SKBitmap(TILE_SIZE, TILE_SIZE))
            {
                using (SKCanvas bitmapCanvas = new SKCanvas(bitmap))
                {
                    bitmapCanvas.Clear();

                    // Draw tiled turbulence noise on bitmap
                    using (SKPaint paint = new SKPaint())
                    {
                        paint.Shader = SKShader.CreatePerlinNoiseTurbulence(
                                            0.02f, 0.02f, 1, seed, 
                                            new SKPointI(TILE_SIZE, TILE_SIZE));

                        bitmapCanvas.DrawRect(tileRect, paint);
                    }
                }

                // Draw tiled bitmap shader on canvas
                using (SKPaint paint = new SKPaint())
                {
                    paint.Shader = SKShader.CreateBitmap(bitmap, 
                                                         SKShaderTileMode.Repeat, 
                                                         SKShaderTileMode.Repeat);
                    canvas.DrawRect(info.Rect, paint);
                }

                // Draw rectangle showing tile 
                using (SKPaint paint = new SKPaint())
                {
                    paint.Style = SKPaintStyle.Stroke;
                    paint.Color = SKColors.Black;
                    paint.StrokeWidth = 2;

                    canvas.DrawRect(tileRect, paint);
                }
            }
        }
    }
}