using SkiaSharp;
using SkiaSharp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace SkiaSharpSample.UWPSample
{
	public sealed partial class MainPage : Page
	{
		private static Color XamarinLightBlue = Color.FromArgb(0xff, 0x34, 0x98, 0xdb);
		private static Color XamarinDarkBlue = Color.FromArgb(0xff, 0x2c, 0x3e, 0x50);

		private CancellationTokenSource cancellations;
		private IList<SampleBase> samples;
		private IList<GroupedSamples> sampleGroups;
		private SampleBase sample;

		public MainPage()
		{
			this.InitializeComponent();

			if (ApiInformation.IsTypePresent(typeof(StatusBar).FullName))
			{
				var statusBar = StatusBar.GetForCurrentView();
				statusBar.BackgroundColor = XamarinLightBlue;
				statusBar.ForegroundColor = Colors.White;
				statusBar.BackgroundOpacity = 1;
			}

			if (ApiInformation.IsTypePresent(typeof(ApplicationViewTitleBar).FullName))
			{
				var titlebar = ApplicationView.GetForCurrentView().TitleBar;
				titlebar.BackgroundColor = XamarinLightBlue;
				titlebar.ForegroundColor = Colors.White;
				titlebar.ButtonBackgroundColor = XamarinLightBlue;
				titlebar.ButtonForegroundColor = Colors.White;
				titlebar.ButtonHoverBackgroundColor = XamarinDarkBlue;
				titlebar.ButtonHoverForegroundColor = Colors.White;
			}

			samples = SamplesManager.GetSamples(SamplePlatforms.iOS).ToList();
			sampleGroups = Enum.GetValues(typeof(SampleCategories))
				.Cast<SampleCategories>()
				.Select(c => new GroupedSamples(c, samples.Where(s => s.Category.HasFlag(c))))
				.Where(g => g.Count > 0)
				.OrderBy(g => g.Category == SampleCategories.Showcases ? string.Empty : g.Name)
				.ToList();

			SamplesInitializer.Init();

			samplesViewSource.Source = sampleGroups;

			//foreach (var category in sampleGroups)
			//{
			//	// create the menu item
			//	var menu = new MenuItem { Header = category.Name };
			//	foreach (var sample in category.Samples)
			//	{
			//		// create the sample item
			//		var menuItem = new MenuItem
			//		{
			//			Header = sample.Title,
			//			Tag = sample
			//		};
			//		menuItem.Click += OnSampleSelected;
			//		menu.Items.Add(menuItem);
			//	}
			//	// add to the menu bar
			//	samplesList.Items.Add(menu);
			//}

			SetSample(samples.First(s => s.Category.HasFlag(SampleCategories.Showcases)));
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);

			cancellations?.Cancel();
			cancellations = null;
		}

		private void OnSampleSelected(object sender, SelectionChangedEventArgs e)
		{
			var sample = e.AddedItems?.FirstOrDefault() as SampleBase;
			SetSample(sample);
		}

		private void OnBackendSelected(object sender, RoutedEventArgs e)
		{
			var menu = sender as MenuFlyoutItem;

			var backend = (SampleBackends)Enum.Parse(typeof(SampleBackends), menu.Tag.ToString());
			switch (backend)
			{
				case SampleBackends.Memory:
					//glhost.Visibility = Visibility.Collapsed;
					canvas.Visibility = Visibility.Visible;
					break;
				case SampleBackends.OpenGL:
					//glhost.Visibility = Visibility.Visible;
					canvas.Visibility = Visibility.Collapsed;
					break;
				default:
					var msg = new MessageDialog("This functionality is not yet implemented.", "Configure Backend");
					msg.Commands.Add(new UICommand("OK"));
					msg.ShowAsync();
					break;
			}
		}

		private void OnToggleSlideshow(object sender, RoutedEventArgs e)
		{
			if (cancellations != null)
			{
				// cancel the old loop
				cancellations.Cancel();
				cancellations = null;
			}
			else
			{
				// start a new loop
				cancellations = new CancellationTokenSource();
				var token = cancellations.Token;
				Task.Run(async delegate
				{
					try
					{
						// get the samples in a list
						var sortedSamples = sampleGroups.SelectMany(g => g).Distinct().ToList();
						var lastSample = sortedSamples.First();
						while (!token.IsCancellationRequested)
						{
							// display the sample
							await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SetSample(lastSample));

							// wait a bit
							await Task.Delay(3000, token);

							// select the next one
							var idx = sortedSamples.IndexOf(lastSample) + 1;
							if (idx >= sortedSamples.Count)
							{
								idx = 0;
							}
							lastSample = sortedSamples[idx];
						}
					}
					catch (TaskCanceledException)
					{
						// we are expecting this
					}
				});
			}
		}

		private void OnPaintCanvas(object sender, SKPaintSurfaceEventArgs e)
		{
			OnPaintSurface(e.Surface.Canvas, e.Info.Width, e.Info.Height);
		}

		private void SetSample(SampleBase newSample)
		{
			sample = newSample;

			// set the title
			titleBar.Text = sample?.Title ?? "SkiaSharp for Windows";

			// prepare the sample
			sample?.Init(() =>
			{
				// refresh the view
				canvas.Invalidate();
				//glhost.Child?.Invalidate();
			});

			// refresh the view
			canvas.Invalidate();
			//glhost.Child?.Invalidate();
		}

		private void OnPaintSurface(SKCanvas canvas, int width, int height)
		{
			sample?.DrawSample(canvas, width, height);
		}

		private void OnSampleTapped(object sender, TappedRoutedEventArgs e)
		{
			sample?.Tap();
		}
	}

	public class GroupedSamples : ObservableCollection<SampleBase>
	{
		private static readonly Regex EnumSplitRexeg = new Regex("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])");

		public GroupedSamples(SampleCategories category, IEnumerable<SampleBase> samples)
		{
			Category = category;
			Name = EnumSplitRexeg.Replace(category.ToString(), " $1");
			foreach (var sample in samples.OrderBy(s => s.Title))
			{
				Add(sample);
			}
		}

		public SampleCategories Category { get; private set; }

		public string Name { get; private set; }
	}
}
