using System;
using System.Threading.Tasks;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace DocsSamplesApp.Effects
{
	public partial class GradientTransitionsPage : ContentPage
	{
        SKBitmap? bitmap1;
        SKBitmap? bitmap2;

        enum TransitionMode
        {
            Linear,
            Radial,
            Sweep
        };

        public GradientTransitionsPage ()
		{
			InitializeComponent ();

            foreach (TransitionMode mode in Enum.GetValues(typeof(TransitionMode)))
            {
                transitionPicker.Items.Add(mode.ToString());
            }

            transitionPicker.SelectedIndex = 0;

            _ = LoadBitmap1Async();
            _ = LoadBitmap2Async();
		}

        async Task LoadBitmap1Async()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("SeatedMonkey.jpg");
            bitmap1 = SKBitmap.Decode(stream);
            canvasView.InvalidateSurface();
        }

        async Task LoadBitmap2Async()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("FacePalm.jpg");
            bitmap2 = SKBitmap.Decode(stream);
            canvasView.InvalidateSurface();
        }

        void OnSliderValueChanged(object sender, ValueChangedEventArgs args)
        {
            canvasView.InvalidateSurface();
        }

        void OnPickerSelectedIndexChanged(object sender, EventArgs args)
        {
            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            if (bitmap1 is null || bitmap2 is null)
                return;

            // Assume both bitmaps are square for display rectangle
            float size = Math.Min(info.Width, info.Height);
            SKRect rect = SKRect.Create(size, size);
            float x = (info.Width - size) / 2;
            float y = (info.Height - size) / 2;
            rect.Offset(x, y);

            using (SKPaint paint0 = new SKPaint())
            using (SKPaint paint1 = new SKPaint())
            using (SKPaint paint2 = new SKPaint())
            {
                SKColor[] colors = new SKColor[] { SKColors.Black,
                                                   SKColors.Transparent };

                float progress = (float)progressSlider.Value;

                float[] positions = new float[]{ 1.1f * progress - 0.1f,
                                                 1.1f * progress };

                switch ((TransitionMode)transitionPicker.SelectedIndex)
                {
                    case TransitionMode.Linear:
                        paint0.Shader = SKShader.CreateLinearGradient(
                                           new SKPoint(rect.Left, 0),
                                           new SKPoint(rect.Right, 0),
                                           colors,
                                           positions,
                                           SKShaderTileMode.Clamp);
                        break;

                    case TransitionMode.Radial:
                        paint0.Shader = SKShader.CreateRadialGradient(
                                            new SKPoint(rect.MidX, rect.MidY),
                                            (float)Math.Sqrt(Math.Pow(rect.Width / 2, 2) +
                                                             Math.Pow(rect.Height / 2, 2)),
                                            colors,
                                            positions,
                                            SKShaderTileMode.Clamp);
                        break;

                    case TransitionMode.Sweep:
                        paint0.Shader = SKShader.CreateSweepGradient(
                                            new SKPoint(rect.MidX, rect.MidY),
                                            colors,
                                            positions);
                        break;
                }

                canvas.DrawRect(rect, paint0);

                paint1.BlendMode = SKBlendMode.SrcOut;
                canvas.DrawBitmap(bitmap1, rect, paint1);

                paint2.BlendMode = SKBlendMode.DstOver;
                canvas.DrawBitmap(bitmap2, rect, paint2);
            }
        }
    }
}