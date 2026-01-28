using System;
using System.Collections.Generic;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;

namespace DocsSamplesApp.Bitmaps
{
    public partial class FingerPaintSavePage : ContentPage
    {
        Dictionary<long, SKPath> inProgressPaths = new Dictionary<long, SKPath>();
        List<SKPath> completedPaths = new List<SKPath>();

        SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Blue,
            StrokeWidth = 10,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };

        SKBitmap saveBitmap;

        public FingerPaintSavePage ()
        {
            InitializeComponent ();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            // Create bitmap the size of the display surface
            if (saveBitmap == null)
            {
                saveBitmap = new SKBitmap(info.Width, info.Height);
            }
            // Or create new bitmap for a new size of display surface
            else if (saveBitmap.Width < info.Width || saveBitmap.Height < info.Height)
            {
                SKBitmap newBitmap = new SKBitmap(Math.Max(saveBitmap.Width, info.Width),
                                                  Math.Max(saveBitmap.Height, info.Height));

                using (SKCanvas newCanvas = new SKCanvas(newBitmap))
                {
                    newCanvas.Clear();
                    newCanvas.DrawBitmap(saveBitmap, 0, 0);
                }

                saveBitmap = newBitmap;
            }

            // Render the bitmap
            canvas.Clear();
            canvas.DrawBitmap(saveBitmap, 0, 0);
        }

        void OnTouch(object sender, SKTouchEventArgs e)
        {
            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:
                    if (!inProgressPaths.ContainsKey(e.Id))
                    {
                        SKPath path = new SKPath();
                        path.MoveTo(e.Location);
                        inProgressPaths.Add(e.Id, path);
                        UpdateBitmap();
                    }
                    break;

                case SKTouchAction.Moved:
                    if (inProgressPaths.ContainsKey(e.Id))
                    {
                        SKPath path = inProgressPaths[e.Id];
                        path.LineTo(e.Location);
                        UpdateBitmap();
                    }
                    break;

                case SKTouchAction.Released:
                    if (inProgressPaths.ContainsKey(e.Id))
                    {
                        completedPaths.Add(inProgressPaths[e.Id]);
                        inProgressPaths.Remove(e.Id);
                        UpdateBitmap();
                    }
                    break;

                case SKTouchAction.Cancelled:
                    if (inProgressPaths.ContainsKey(e.Id))
                    {
                        inProgressPaths.Remove(e.Id);
                        UpdateBitmap();
                    }
                    break;
            }

            e.Handled = true;
        }

        void UpdateBitmap()
        {
            using (SKCanvas saveBitmapCanvas = new SKCanvas(saveBitmap))
            {
                saveBitmapCanvas.Clear();

                foreach (SKPath path in completedPaths)
                {
                    saveBitmapCanvas.DrawPath(path, paint);
                }

                foreach (SKPath path in inProgressPaths.Values)
                {
                    saveBitmapCanvas.DrawPath(path, paint);
                }
            }

            canvasView.InvalidateSurface();
        }

        void OnClearButtonClicked(object sender, EventArgs args)
        {
            completedPaths.Clear();
            inProgressPaths.Clear();
            UpdateBitmap();
            canvasView.InvalidateSurface();
        }

        async void OnSaveButtonClicked(object sender, EventArgs args)
        {
            using (SKImage image = SKImage.FromBitmap(saveBitmap))
            {
                SKData data = image.Encode();
                DateTime dt = DateTime.Now;
                string filename = String.Format("FingerPaint-{0:D4}{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}{6:D3}.png",
                                                dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);

                IPhotoLibrary photoLibrary = DependencyService.Get<IPhotoLibrary>();
                bool result = await photoLibrary.SavePhotoAsync(data.ToArray(), "FingerPaint", filename);

                if (!result)
                {
                    await DisplayAlert("FingerPaint", "Artwork could not be saved. Sorry!", "OK");
                }
            }
        }
    }
}