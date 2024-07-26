using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Effects
{
    public partial class BitmapTileFlipModesPage : ContentPage
    {
        SKBitmap bitmap;

	    public BitmapTileFlipModesPage ()
	    {
		    InitializeComponent ();

            SKBitmap origBitmap = BitmapExtensions.LoadBitmapResource(
                GetType(), "SkiaSharpFormsDemos.Media.SeatedMonkey.jpg");

            // Define cropping rect
            SKRectI cropRect = new SKRectI(5, 27, 296, 260);

            // Get the cropped bitmap
            SKBitmap croppedBitmap = new SKBitmap(cropRect.Width, cropRect.Height);
            origBitmap.ExtractSubset(croppedBitmap, cropRect);

            // Resize to half the width and height
            SKImageInfo info = new SKImageInfo(cropRect.Width / 2, cropRect.Height / 2);
            bitmap = croppedBitmap.Resize(info, SKBitmapResizeMethod.Box);
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

            // Get tile modes from Pickers
            SKShaderTileMode xTileMode =
                (SKShaderTileMode)(xModePicker.SelectedIndex == -1 ?
                                            0 : xModePicker.SelectedItem);
            SKShaderTileMode yTileMode =
                (SKShaderTileMode)(yModePicker.SelectedIndex == -1 ?
                                            0 : yModePicker.SelectedItem);

            using (SKPaint paint = new SKPaint())
            {
                paint.Shader = SKShader.CreateBitmap(bitmap, xTileMode, yTileMode);
                canvas.DrawRect(info.Rect, paint);
            }
        }
    }
}
