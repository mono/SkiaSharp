using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Views.UWP;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace SkiaSharpSample
{
	public sealed partial class MainPage : Page
	{
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

		private void OnToggleSplitView(object sender, RoutedEventArgs e)
		{
			if (!IsLoaded)
				return;

			var menuButton = (ToggleButton)sender;
			splitView.IsPaneOpen = menuButton.IsChecked == true;
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

		private void OnPaintCanvas(object sender, SKPaintGLSurfaceEventArgs e)
		{
			OnPaintSurface(e.Surface.Canvas, e.BackendRenderTarget.Width, e.BackendRenderTarget.Height);
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
