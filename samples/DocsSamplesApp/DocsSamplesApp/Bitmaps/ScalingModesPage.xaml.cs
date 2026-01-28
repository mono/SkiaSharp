using System;
using System.IO;
using System.Threading.Tasks;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Storage;

namespace DocsSamplesApp.Bitmaps
{
    public partial class ScalingModesPage : ContentPage
    {
        SKBitmap? bitmap;

        public ScalingModesPage()
        {
            InitializeComponent();
            _ = LoadBitmapAsync();
        }

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("Banana.jpg");
            bitmap = SKBitmap.Decode(stream);
            canvasView.InvalidateSurface();
        }

        private void OnPickerSelectedIndexChanged(object? sender, EventArgs args)
        {
            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            if (bitmap is null)
                return;

            SKRect dest = new SKRect(0, 0, info.Width, info.Height);

            BitmapStretch stretch = (BitmapStretch)stretchPicker.SelectedItem;
            BitmapAlignment horizontal = (BitmapAlignment)horizontalPicker.SelectedItem;
            BitmapAlignment vertical = (BitmapAlignment)verticalPicker.SelectedItem;

            canvas.DrawBitmap(bitmap, dest, stretch, horizontal, vertical);
        }
    }
}