using System;
using System.Windows;
using System.Windows.Controls;

namespace SkiaSharpSample;

public partial class MainWindow : Window
{
	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

	public MainWindow()
	{
		InitializeComponent();
		TabNav.SelectedIndex = (int)DefaultPage;
	}

	private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (TabNav?.SelectedItem is not TabItem tab)
			return;

		if (tab.Content != null)
			return;

		tab.Content = tab.Tag?.ToString() switch
		{
			"cpu" => new CpuPage(),
			"gpu" => new GpuPage(),
			"drawing" => new DrawingPage(),
			_ => new CpuPage(),
		};
	}
}
