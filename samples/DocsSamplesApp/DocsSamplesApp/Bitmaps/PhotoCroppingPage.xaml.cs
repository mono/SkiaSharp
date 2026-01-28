using System;
using System.IO;
using System.Threading.Tasks;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace DocsSamplesApp.Bitmaps
{
    public partial class PhotoCroppingPage : ContentPage
    {
        PhotoCropperCanvasView? photoCropper;
        SKBitmap? croppedBitmap;

        public PhotoCroppingPage ()
        {
            InitializeComponent ();
            _ = LoadBitmapAsync();
        }

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("MountainClimbers.jpg");
            var bitmap = SKBitmap.Decode(stream);
            photoCropper = new PhotoCropperCanvasView(bitmap);
            canvasViewHost.Children.Add(photoCropper);
        }

        void OnDoneButtonClicked(object? sender, EventArgs args)
        {
            if (photoCropper is null)
                return;

            croppedBitmap = photoCropper.CroppedBitmap;

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();
            canvas.DrawBitmap(croppedBitmap, info.Rect, BitmapStretch.Uniform);
        }
    }
}