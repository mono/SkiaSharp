using System;
using Microsoft.Maui.Controls;

namespace SkiaSharpSample;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		graphiteItem.IsVisible =
#if IOS
			OperatingSystem.IsIOSVersionAtLeast(13);
#elif MACCATALYST
			true;
#else
			false;
#endif

		if (App.DefaultPage == SamplePage.Graphite && !graphiteItem.IsVisible)
			throw new PlatformNotSupportedException("Graphite presentation requires a supported Apple host.");

		if (App.DefaultPage != SamplePage.Cpu)
			CurrentItem = Items[(int)App.DefaultPage];
	}
}
