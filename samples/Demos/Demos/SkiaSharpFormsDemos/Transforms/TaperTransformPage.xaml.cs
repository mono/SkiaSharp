using System;
using System.IO;
using System.Reflection;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Transforms
{
    public partial class TaperTransformPage : ContentPage
    {
        SKBitmap bitmap;

        MatrixDisplay matrixDisplay = new MatrixDisplay
        {
            PerspectiveFormat = "F5"
        };

        public TaperTransformPage()
        {
            InitializeComponent();

            string resourceID = "SkiaSharpFormsDemos.Media.FacePalm.jpg";
            Assembly assembly = GetType().GetTypeInfo().Assembly;

            using (Stream stream = assembly.GetManifestResourceStream(resourceID))
            {
                bitmap = SKBitmap.Decode(stream);
            }

            taperFractionSlider.Value = 1;
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

            SKMatrix matrix = SKMatrix.MakeTranslation(-x, -y);
            SKMatrix.PostConcat(ref matrix, taperMatrix);
            SKMatrix.PostConcat(ref matrix, SKMatrix.MakeTranslation(x, y));

            canvas.SetMatrix(matrix);
            canvas.DrawBitmap(bitmap, x, y);
        }
    }
}
