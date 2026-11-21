using Microsoft.Maui.Controls;

namespace SkiaSharpSample;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		if (App.DefaultGpuBackend != GpuBackend.GL &&
			(int)App.DefaultGpuBackend < gpuTab.Items.Count)
		{
			gpuTab.CurrentItem = gpuTab.Items[(int)App.DefaultGpuBackend];
		}

		if (App.DefaultPage != SamplePage.Cpu)
			CurrentItem = Items[(int)App.DefaultPage];
	}
}
