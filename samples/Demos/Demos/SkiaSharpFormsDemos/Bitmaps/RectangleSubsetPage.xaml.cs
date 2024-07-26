using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Bitmaps
{
    public partial class RectangleSubsetPage : ContentPage
    {
        SKBitmap bitmap =
            BitmapExtensions.LoadBitmapResource(typeof(RectangleSubsetPage),
                                                "SkiaSharpFormsDemos.Media.Banana.jpg");

        static readonly SKRect SOURCE = new SKRect(94, 12, 212, 118);

        public RectangleSubsetPage()
        {
            InitializeComponent();
        }

        private void OnPickerSelectedIndexChanged(object sender, EventArgs args)
        {
            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            SKRect dest = new SKRect(0, 0, info.Width, info.Height);

            BitmapStretch stretch = (BitmapStretch)stretchPicker.SelectedItem;
            BitmapAlignment horizontal = (BitmapAlignment)horizontalPicker.SelectedItem;
            BitmapAlignment vertical = (BitmapAlignment)verticalPicker.SelectedItem;

            canvas.DrawBitmap(bitmap, SOURCE, dest, stretch, horizontal, vertical);
        }
    }
}