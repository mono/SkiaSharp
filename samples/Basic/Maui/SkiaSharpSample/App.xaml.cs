using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SkiaSharpSample;

public partial class App : Application
{
	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState) =>
		new Window
		{
			Title = "SkiaSharp Sample",
			Page = new AppShell()
		};
}
