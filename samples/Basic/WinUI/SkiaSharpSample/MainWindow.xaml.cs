using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace SkiaSharpSample
{
	public sealed partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			NavView.Loaded += OnNavViewLoaded;
		}

		private void OnNavViewLoaded(object sender, RoutedEventArgs e)
		{
			NavView.SelectedItem = NavView.MenuItems[0];
			ContentFrame.Navigate(typeof(CpuPage));
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
}
