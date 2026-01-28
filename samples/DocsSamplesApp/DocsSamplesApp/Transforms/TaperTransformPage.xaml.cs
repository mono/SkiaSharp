using System;
using System.IO;
using System.Threading.Tasks;

using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Storage;

namespace DocsSamplesApp.Transforms
{
    public partial class TaperTransformPage : ContentPage
    {
        SKBitmap? bitmap;

        MatrixDisplay matrixDisplay = new MatrixDisplay
        {
            PerspectiveFormat = "F5"
        };

        public TaperTransformPage()
        {
            InitializeComponent();

            _ = LoadBitmapAsync();

            taperFractionSlider.Value = 1;
        }

        async Task LoadBitmapAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("FacePalm.jpg");
            bitmap = SKBitmap.Decode(stream);
            canvasView.InvalidateSurface();
        }

        void OnSliderValueChanged(object sender, ValueChangedEventArgs args)
        {
            if (canvasView != null)
            {
                canvasView.InvalidateSurface();
            }
        }

        void OnPickerSelectedIndexChanged(object sender, EventArgs args)
        {
            if (canvasView != null)
            {
                canvasView.InvalidateSurface();
            }
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            if (bitmap is null)
                return;

            TaperSide taperSide = (TaperSide)taperSidePicker.SelectedItem;
            TaperCorner taperCorner = (TaperCorner)taperCornerPicker.SelectedItem;
            float taperFraction = (float)taperFractionSlider.Value;

            SKMatrix taperMatrix = 
                TaperTransform.Make(new SKSize(bitmap.Width, bitmap.Height),
                                    taperSide, taperCorner, taperFraction);

            // Display the matrix in the lower-right corner
            SKSize matrixSize = matrixDisplay.Measure(taperMatrix);

            matrixDisplay.Paint(canvas, taperMatrix,
                new SKPoint(info.Width - matrixSize.Width,
                            info.Height - matrixSize.Height));

            // Center bitmap on canvas
            float x = (info.Width - bitmap.Width) / 2;
            float y = (info.Height - bitmap.Height) / 2;

            SKMatrix matrix = SKMatrix.CreateTranslation(-x, -y);
            matrix = matrix.PostConcat(taperMatrix);
            matrix = matrix.PostConcat(SKMatrix.CreateTranslation(x, y));

            canvas.SetMatrix(matrix);
            canvas.DrawBitmap(bitmap, x, y);
        }
    }
}
