using Microsoft.Maui.Controls;

namespace SkiaSharpSample;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		var pageIndex = (int)App.DefaultPage;
		var removeGraphite = false;
#if WINDOWS
		removeGraphite = true;
#elif ANDROID
		removeGraphite =
			Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.N;
#endif
		if (removeGraphite)
		{
			Items.Remove(graphiteItem);
			if (App.DefaultPage == SamplePage.Graphite)
				pageIndex = (int)SamplePage.Gpu;
			else if (App.DefaultPage > SamplePage.Graphite)
				pageIndex--;
		}
		if (pageIndex != (int)SamplePage.Cpu)
			CurrentItem = Items[pageIndex];
	}
}
