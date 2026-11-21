using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace SkiaSharpSample;

public sealed partial class GpuPage : Page
{
	/// <summary>
	/// Change this to start the GPU page on a different backend.
	/// </summary>
	public static GpuBackend DefaultGpuBackend
	{
		get => MainWindow.DefaultGpuBackend;
		set => MainWindow.DefaultGpuBackend = value;
	}

	public GpuPage()
	{
		InitializeComponent();

		foreach (var backend in Enum.GetValues<GpuBackend>())
		{
			GpuNavView.MenuItems.Add(new NavigationViewItem
			{
				Content = backend.ToString(),
				Tag = backend,
			});
		}

		GpuNavView.Loaded += OnGpuNavViewLoaded;
	}

	private void OnGpuNavViewLoaded(object sender, RoutedEventArgs e)
	{
		var index = (int)DefaultGpuBackend;
		if (index >= 0 && index < GpuNavView.MenuItems.Count)
			GpuNavView.SelectedItem = GpuNavView.MenuItems[index];
	}

	private void GpuNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
	{
		if (args.SelectedItem is NavigationViewItem { Tag: GpuBackend backend })
		{
			Type pageType = backend switch
			{
				GpuBackend.SwapChain => typeof(GpuSwapChainPage),
				_ => typeof(GpuSwapChainPage),
			};
			GpuContentFrame.Navigate(pageType);

			// Clear back stack so previous GPU backend pages get Unloaded
			GpuContentFrame.BackStack.Clear();
		}
	}
}
