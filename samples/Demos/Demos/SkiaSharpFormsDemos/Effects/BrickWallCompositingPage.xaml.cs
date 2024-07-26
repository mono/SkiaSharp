using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
    public partial class BrickWallCompositingPage : ContentPage
    {
        SKBitmap monkeyBitmap = BitmapExtensions.LoadBitmapResource(
            typeof(BrickWallCompositingPage),
            "SkiaSharpFormsDemos.Media.SeatedMonkey.jpg");

        SKBitmap matteBitmap = BitmapExtensions.LoadBitmapResource(
            typeof(BrickWallCompositingPage),
            "SkiaSharpFormsDemos.Media.SeatedMonkeyMatte.png");

        int step = 0;

        public BrickWallCompositingPage()
        {
            InitializeComponent();
        }

        void OnButtonClicked(object sender, EventArgs args)
        {
            Button btn = (Button)sender;
            step = (step + 1) % 5;

            switch (step)
            {
                case 0: btn.Text = "Show sitting monkey"; break;
                case 1: btn.Text = "Draw matte with DstIn"; break;
                case 2: btn.Text = "Draw sidewalk with DstOver"; break;
                case 3: btn.Text = "Draw brick wall with DstOver"; break;
                case 4: btn.Text = "Reset"; break;
            }

            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            float x = (info.Width - monkeyBitmap.Width) / 2;
            float y = info.Height - monkeyBitmap.Height;

            // Draw monkey bitmap
            if (step >= 1)
            {
                canvas.DrawBitmap(monkeyBitmap, x, y);
            }

            // Draw matte to exclude monkey's surroundings
            if (step >= 2)
            {
                using (SKPaint paint = new SKPaint())
                {
                    paint.BlendMode = SKBlendMode.DstIn;
                    canvas.DrawBitmap(matteBitmap, x, y, paint);
                }
            }

            const float sidewalkHeight = 80;
            SKRect rect = new SKRect(info.Rect.Left, info.Rect.Bottom - sidewalkHeight,
                                     info.Rect.Right, info.Rect.Bottom);

            // Draw gravel sidewalk for monkey to sit on
            if (step >= 3)
            {
                using (SKPaint paint = new SKPaint())
                {
                    paint.Shader = SKShader.CreateCompose(
                                        SKShader.CreateColor(SKColors.SandyBrown),
                                        SKShader.CreatePerlinNoiseTurbulence(0.1f, 0.3f, 1, 9));

                    paint.BlendMode = SKBlendMode.DstOver;
                    canvas.DrawRect(rect, paint);
                }
            }

            // Draw bitmap tiled brick wall behind monkey
            if (step >= 4)
            {
                using (SKPaint paint = new SKPaint())
                {
                    SKBitmap bitmap = AlgorithmicBrickWallPage.BrickWallTile;
                    float yAdjust = (info.Height - sidewalkHeight) % bitmap.Height;

                    paint.Shader = SKShader.CreateBitmap(bitmap,
                                                         SKShaderTileMode.Repeat,
                                                         SKShaderTileMode.Repeat,
                                                         SKMatrix.MakeTranslation(0, yAdjust));
                    paint.BlendMode = SKBlendMode.DstOver;
                    canvas.DrawRect(info.Rect, paint);
                }
            }
        }
    }
}