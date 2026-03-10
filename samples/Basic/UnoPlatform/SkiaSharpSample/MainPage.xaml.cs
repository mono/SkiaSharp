namespace SkiaSharpSample;

public sealed partial class MainPage : Page
{
	public MainPage()
	{
		InitializeComponent();
		NavView.Loaded += OnNavViewLoaded;
		InitializeVersionsContextMenu();
	}

	private void OnNavViewLoaded(object sender, RoutedEventArgs e)
	{
		// Hide GPU tab on platforms where SKSwapChainPanel is unsupported
		if (!IsGpuSupported())
		{
			NavView.MenuItems.RemoveAt(1);
		}
		NavView.SelectedItem = NavView.MenuItems[0];
	}

	private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
	{
		if (args.SelectedItem is NavigationViewItem item)
		{
			Type pageType = item.Tag?.ToString() switch
			{
				"cpu" => typeof(CpuPage),
				"gpu" => typeof(GpuPage),
				"drawing" => typeof(DrawingPage),
				_ => typeof(CpuPage),
			};
			ContentFrame.Navigate(pageType);
		}
	}

	private static bool IsGpuSupported()
	{
#if __MACCATALYST__
		return false;
#else
		try
		{
			return !SkiaSharp.Views.Windows.SKSwapChainPanel.RaiseOnUnsupported ||
			       OperatingSystem.IsAndroid() || OperatingSystem.IsIOS() ||
			       OperatingSystem.IsWindows();
		}
		catch
		{
			return false;
		}
#endif
	}
}
