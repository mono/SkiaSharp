using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace DocsSamplesApp.Transforms
{
    public partial class BitmapScatterViewPage : ContentPage
    {
        List<TouchManipulationBitmap>? bitmapCollection;

        Dictionary<long, TouchManipulationBitmap> bitmapDictionary =
            new Dictionary<long, TouchManipulationBitmap>();

        public BitmapScatterViewPage()
        {
            InitializeComponent();

            _ = LoadBitmapsAsync();
        }

        async Task LoadBitmapsAsync()
        {
            bitmapCollection = new List<TouchManipulationBitmap>();
            string[] imageFiles = BitmapExtensions.GetImageFileNames();
            SKPoint position = new SKPoint();

            foreach (string fileName in imageFiles)
            {
                using Stream stream = await FileSystem.OpenAppPackageFileAsync(fileName);
                SKBitmap bitmap = SKBitmap.Decode(stream);
                bitmapCollection.Add(new TouchManipulationBitmap(bitmap)
                {
                    Matrix = SKMatrix.CreateTranslation(position.X, position.Y),
                });
                position.X += 100;
                position.Y += 100;
            }
            canvasView.InvalidateSurface();
        }

        void OnTouch(object? sender, SKTouchEventArgs e)
        {
            if (bitmapCollection is null)
                return;

            // Location is already in pixels with built-in touch
            SKPoint point = e.Location;

            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:
                    for (int i = bitmapCollection.Count - 1; i >= 0; i--)
                    {
                        TouchManipulationBitmap bitmap = bitmapCollection[i];

                        if (bitmap.HitTest(point))
                        {
                            // Move bitmap to end of collection
                            bitmapCollection.Remove(bitmap);
                            bitmapCollection.Add(bitmap);

                            // Do the touch processing
                            bitmapDictionary.Add(e.Id, bitmap);
                            bitmap.ProcessTouchEvent(e.Id, e.ActionType, point);
                            canvasView.InvalidateSurface();
                            break;
                        }
                    }
                    break;

                case SKTouchAction.Moved:
                    if (bitmapDictionary.ContainsKey(e.Id))
                    {
                        TouchManipulationBitmap bitmap = bitmapDictionary[e.Id];
                        bitmap.ProcessTouchEvent(e.Id, e.ActionType, point);
                        canvasView.InvalidateSurface();
                    }
                    break;

                case SKTouchAction.Released:
                case SKTouchAction.Cancelled:
                    if (bitmapDictionary.ContainsKey(e.Id))
                    {
                        TouchManipulationBitmap bitmap = bitmapDictionary[e.Id];
                        bitmap.ProcessTouchEvent(e.Id, e.ActionType, point);
                        bitmapDictionary.Remove(e.Id);
                        canvasView.InvalidateSurface();
                    }
                    break;
            }

            e.Handled = true;
        }

        void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs args)
        {
            SKCanvas canvas = args.Surface.Canvas;
            canvas.Clear();

            if (bitmapCollection is null)
                return;

            foreach (TouchManipulationBitmap bitmap in bitmapCollection)
            {
                bitmap.Paint(canvas);
            }
        }
    }
}
