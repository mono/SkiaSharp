using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Skia.WPF.Demo
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			string fontName = "content-font.ttf";
			var dir = Path.Combine(Path.GetTempPath(), "SkiaSharp.Demos", Path.GetRandomFileName());
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
			SkiaSharp.Demos.CustomFontPath = Path.Combine(Path.GetDirectoryName(typeof(MainWindow).Assembly.Location), "Content", fontName);
			SkiaSharp.Demos.WorkingDirectory = dir;
			SkiaSharp.Demos.OpenFileDelegate = path =>
			{
				System.Diagnostics.Process.Start(Path.Combine(dir, path));
			};

			foreach (var sample in SkiaSharp.Demos.SamplesForPlatform(SkiaSharp.Demos.Platform.WindowsDesktop))
			{
				comboBox.Items.Add(sample);
			}
			comboBox.SelectedIndex = 0;
		}

		private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			skiaView.Sample = SkiaSharp.Demos.GetSample((string)comboBox.SelectedItem);
		}
	}
}
