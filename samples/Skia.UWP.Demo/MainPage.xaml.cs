using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Skia.UWP.Demo
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();

			string fontName = "content-font.ttf";
			var install = Windows.ApplicationModel.Package.Current.InstalledLocation;
			SkiaSharp.Demos.CustomFontPath = Path.Combine(install.Path, fontName);

			var items = SkiaSharp.Demos.SamplesForPlatform(SkiaSharp.Demos.Platform.UWP);
			foreach (var item in items)
			{
				comboBox.Items.Add(item);
			}

			comboBox.SelectionChanged += (sender, e) =>
			{
				skiaView.OnDrawCallback = SkiaSharp.Demos.MethodForSample((string)comboBox.SelectedItem);
			};

			comboBox.SelectedIndex = 0;
		}
	}
}
