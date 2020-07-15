using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SkiaSharp;
using SkiaSharp.Views.UWP;

namespace SkiaSharpSample
{
	public sealed partial class MainPage : Page
	{
		private static Color XamarinLightBlue = Color.FromArgb(0xff, 0x34, 0x98, 0xdb);
		private static Color XamarinDarkBlue = Color.FromArgb(0xff, 0x2c, 0x3e, 0x50);

		private CancellationTokenSource cancellations;
		private IList<SampleBase> samples;
		private SampleBase sample;

		public MainPage()
		{
			InitializeComponent();

			samples = SamplesManager.GetSamples(SamplePlatforms.UWP)
				.OrderBy(s => s.Category == SampleCategories.Showcases ? string.Empty : s.Title)
				.ToList();

			SamplesInitializer.Init();

			listView.ItemsSource = samples;

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
						var lastSample = samples.First();
						while (!token.IsCancellationRequested)
						{
							// display the sample
							await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SetSample(lastSample));

							// wait a bit
							await Task.Delay(3000, token);

							// select the next one
							var idx = samples.IndexOf(lastSample) + 1;
							if (idx >= samples.Count)
							{
								idx = 0;
							}
							lastSample = samples[idx];
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
			// clean up the old sample
			if (sample != null)
			{
				sample.RefreshRequested -= OnRefreshRequested;
				sample.Destroy();
			}

			sample = newSample;

			var runtimeMode = string.Empty;
#if __WASM__
			runtimeMode = Environment.GetEnvironmentVariable("UNO_BOOTSTRAP_MONO_RUNTIME_MODE");
			if (runtimeMode.Equals("Interpreter", StringComparison.InvariantCultureIgnoreCase))
				runtimeMode = " (Interpreted)";
			else if (runtimeMode.Equals("FullAOT", StringComparison.InvariantCultureIgnoreCase))
				runtimeMode = " (AOT)";
			else if (runtimeMode.Equals("InterpreterAndAOT", StringComparison.InvariantCultureIgnoreCase))
				runtimeMode = " (Mixed)";
#endif

			// set the title
			titleBar.Text = (sample?.Title ?? $"SkiaSharp for Uno Platform") + runtimeMode;

			// prepare the sample
			if (sample != null)
			{
				sample.RefreshRequested += OnRefreshRequested;
				sample.Init();
			}

			// refresh the view
			OnRefreshRequested(null, null);
		}

		private void OnRefreshRequested(object sender, EventArgs e)
		{
			canvas.Invalidate();
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
}
