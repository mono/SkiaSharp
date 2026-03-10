using Microsoft.Maui.Controls;

namespace SkiaSharpSample;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		if (App.DefaultPage != SamplePage.Cpu)
			CurrentItem = Items[(int)App.DefaultPage];
	}
}
