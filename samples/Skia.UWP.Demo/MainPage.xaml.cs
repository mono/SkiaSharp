using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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

			if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent(typeof(Windows.UI.ViewManagement.StatusBar).FullName))
			{
				var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
				statusBar.BackgroundColor = Color.FromArgb(0xff, 0x34, 0x98, 0xdb);
				statusBar.ForegroundColor = Colors.White;
				statusBar.BackgroundOpacity = 1;
			}

			string fontName = "content-font.ttf";
			var install = Windows.ApplicationModel.Package.Current.InstalledLocation;
			SkiaSharp.Demos.CustomFontPath = Path.Combine(install.Path, fontName);
			SkiaSharp.Demos.WorkingDirectory = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
			SkiaSharp.Demos.OpenFileDelegate =
				async name =>
				{
					var file = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(name);
					await Windows.System.Launcher.LaunchFileAsync(file);
				};

			var items = SkiaSharp.Demos.SamplesForPlatform(SkiaSharp.Demos.Platform.UWP);
			foreach (var item in items)
			{
				comboBox.Items.Add(item);
			}

			comboBox.SelectionChanged += (sender, e) =>
			{
				skiaView.Sample = SkiaSharp.Demos.GetSample((string)comboBox.SelectedItem);
			};

			comboBox.SelectedIndex = 0;
		}
	}
}
