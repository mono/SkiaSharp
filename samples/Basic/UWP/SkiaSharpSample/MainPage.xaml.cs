using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SkiaSharpSample;

public sealed partial class MainPage : Page
{
	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

	public MainPage()
	{
		InitializeComponent();
		NavView.Loaded += OnNavViewLoaded;
	}

	private void OnNavViewLoaded(object sender, RoutedEventArgs e)
	{
		NavView.SelectedItem = NavView.MenuItems[(int)DefaultPage];
	}

	private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
	{
		// Use the typed container: under NativeAOT, SelectedItem (object) does not resolve to NavigationViewItem.
		if (args.SelectedItemContainer is NavigationViewItemBase item)
		{
			Type pageType = item.Tag?.ToString() switch
			{
				"cpu" => typeof(CpuPage),
				"gpu" => typeof(GpuPage),
				"drawing" => typeof(DrawingPage),
				_ => typeof(CpuPage),
			};
			ContentFrame.Navigate(pageType);

			// Clear back stack so previous pages get Unloaded
			// (stops GPU render loop when switching tabs)
			ContentFrame.BackStack.Clear();
		}
	}
}
