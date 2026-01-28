using System.Collections.Generic;
using System.IO;

using SkiaSharp;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;

namespace DocsSamplesApp.Transforms
{
    public partial class TouchManipulationPage : ContentPage
    {
        TouchManipulationBitmap? bitmap;
        List<long> touchIds = new List<long>();
        MatrixDisplay matrixDisplay = new MatrixDisplay();

        public TouchManipulationPage()
        {
            InitializeComponent();
            _ = LoadBitmapAsync();
        }

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("MountainClimbers.jpg");
            SKBitmap loadedBitmap = SKBitmap.Decode(stream);
            bitmap = new TouchManipulationBitmap(loadedBitmap);
            bitmap.TouchManager.Mode = TouchManipulationMode.ScaleRotate;
            canvasView.InvalidateSurface();
        }

        void OnTouchModePickerSelectedIndexChanged(object sender, EventArgs args)
        {
            if (bitmap is not null)
            {
                Picker picker = (Picker)sender;
                bitmap.TouchManager.Mode = (TouchManipulationMode)picker.SelectedItem;
            }
        }

        void OnTouch(object sender, SKTouchEventArgs e)
        {
            if (bitmap is null)
                return;

            SKPoint point = e.Location;

            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:
                    if (bitmap.HitTest(point))
                    {
                        touchIds.Add(e.Id);
                        bitmap.ProcessTouchEvent(e.Id, e.ActionType, point);
                    }
                    break;

                case SKTouchAction.Moved:
                    if (touchIds.Contains(e.Id))
                    {
                        bitmap.ProcessTouchEvent(e.Id, e.ActionType, point);
                        canvasView.InvalidateSurface();
                    }
                    break;

                case SKTouchAction.Released:
                case SKTouchAction.Cancelled:
                    if (touchIds.Contains(e.Id))
                    {
                        bitmap.ProcessTouchEvent(e.Id, e.ActionType, point);
                        touchIds.Remove(e.Id);
                        canvasView.InvalidateSurface();
                    }
                    break;
            }

            e.Handled = true;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKCanvas canvas = args.Surface.Canvas;
            canvas.Clear();

            if (bitmap is null)
                return;

            bitmap.Paint(canvas);

            SKSize matrixSize = matrixDisplay.Measure(bitmap.Matrix);
            matrixDisplay.Paint(canvas, bitmap.Matrix,
                new SKPoint(args.Info.Width - matrixSize.Width,
                            args.Info.Height - matrixSize.Height));
        }
    }
}
