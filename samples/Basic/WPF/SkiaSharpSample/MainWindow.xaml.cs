using System.Windows;
using System.Windows.Controls;

namespace SkiaSharpSample
{
	public partial class MainWindow : Window
	{
		private readonly UserControl?[] pages = new UserControl?[3];

		public MainWindow()
		{
			InitializeComponent();
		}

		private void OnNavSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (NavList == null || PageContent == null)
				return;

			var index = NavList.SelectedIndex;
			if (index < 0 || index >= pages.Length)
				return;

			pages[index] ??= index switch
			{
				0 => new CpuPage(),
				1 => new GpuPage(),
				2 => new DrawingPage(),
				_ => null
			};

			PageContent.Content = pages[index];
		}
	}
}
