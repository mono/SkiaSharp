using System;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Basics
{
    public partial class ColorExplorePage : ContentPage
    {
        public ColorExplorePage()
        {
            InitializeComponent();

            hueSlider.Value = 0;
            saturationSlider.Value = 100;
            lightnessSlider.Value = 50;
            valueSlider.Value = 100;
        }

        void OnSliderValueChanged(object sender, ValueChangedEventArgs args)
        {
            hslCanvasView.InvalidateSurface();
            hsvCanvasView.InvalidateSurface();
        }

        void OnHslCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKColor color = SKColor.FromHsl((float)hueSlider.Value, 
                                            (float)saturationSlider.Value, 
                                            (float)lightnessSlider.Value);
            args.Surface.Canvas.Clear(color);

            hslLabel.Text = String.Format(" RGB = {0:X2}-{1:X2}-{2:X2} ", 
                                          color.Red, color.Green, color.Blue);
        }

        void OnHsvCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKColor color = SKColor.FromHsv((float)hueSlider.Value, 
                                            (float)saturationSlider.Value,
                                            (float)valueSlider.Value);
            args.Surface.Canvas.Clear(color);

            hsvLabel.Text = String.Format(" RGB = {0:X2}-{1:X2}-{2:X2} ",
                                          color.Red, color.Green, color.Blue);
        }
    }
}
