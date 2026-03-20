using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace SkiaSharpSample;

public sealed partial class MainWindow : Window
{
	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

	public MainWindow()
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

			// Clear back stack so previous pages get Unloaded
			// (stops GPU render loop when switching tabs)
			ContentFrame.BackStack.Clear();
		}
	}
}
