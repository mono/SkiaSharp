using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SkiaSharpSample.Services;

namespace SkiaSharpSample;

public partial class App : Application
{
    private readonly SampleService service;

    public App(SampleService service)
    {
        this.service = service;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var windowSettings = new GalleryWindowSettings(service);
        var navigation = new NavigationPage(new GalleryPage(service, windowSettings))
        {
            BarBackgroundColor = Color.FromArgb("#1A237E"),
            BarTextColor = Colors.White
        };
        return new Window(navigation) { Title = "SkiaSharp Gallery" };
    }
}
