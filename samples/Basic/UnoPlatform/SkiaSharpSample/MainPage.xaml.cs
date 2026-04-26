using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
}
