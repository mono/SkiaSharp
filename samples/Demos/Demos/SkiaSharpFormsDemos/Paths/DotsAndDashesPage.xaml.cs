using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Paths
{
    public partial class DotsAndDashesPage : ContentPage
    {
        public DotsAndDashesPage()
        {
            InitializeComponent();
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

            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Blue,
                StrokeWidth = 10,
                StrokeCap = (SKStrokeCap)strokeCapPicker.SelectedItem,
                PathEffect = SKPathEffect.CreateDash(GetPickerArray(dashArrayPicker), 20)
            };

            SKPath path = new SKPath();
            path.MoveTo(0.2f * info.Width, 0.2f * info.Height);
            path.LineTo(0.8f * info.Width, 0.8f * info.Height);
            path.LineTo(0.2f * info.Width, 0.8f * info.Height);
            path.LineTo(0.8f * info.Width, 0.2f * info.Height);

            canvas.DrawPath(path, paint); 
        }

        float[] GetPickerArray(Picker picker)
        {
            if (picker.SelectedIndex == -1)
            {
                return new float[0];
            }

            string str = (string)picker.SelectedItem;
            string[] strs = str.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            float[] array = new float[strs.Length];

            for (int i = 0; i < strs.Length; i++)
            {
                array[i] = Convert.ToSingle(strs[i]);
            }
            return array;
        }
    }
}
