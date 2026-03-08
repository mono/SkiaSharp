using System.Windows;
using System.Windows.Controls;

namespace SkiaSharpSample
{
	public partial class MainWindow : Window
	{
		private readonly UserControl?[] pages = new UserControl?[3];

		public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

		public MainWindow()
		{
			InitializeComponent();
			NavList.SelectedIndex = (int)DefaultPage;
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
