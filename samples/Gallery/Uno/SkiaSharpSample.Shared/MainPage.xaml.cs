using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Views.UWP;
#if WINUI
using Windows.UI.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
#else
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
#endif
namespace SkiaSharpSample
{
	public sealed partial class MainPage : Page
	{
		private const int TextOverlayPadding = 8;

		private CancellationTokenSource cancellations;
		private SKPaint textPaint;
		private IList<SampleBase> samples;
		private SampleBase sample;

		public MainPage()
		{
			InitializeComponent();

			textPaint = new SKPaint
			{
				TextSize = 16,
				IsAntialias = true
			};

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

		private void OnBackendSelected(object sender, RoutedEventArgs e)
		{
			var menu = sender as MenuFlyoutItem;

			var backend = (SampleBackends)Enum.Parse(typeof(SampleBackends), menu.Tag.ToString());
			switch (backend)
			{
				case SampleBackends.Memory:
					glview.Visibility = Visibility.Collapsed;
					canvas.Visibility = Visibility.Visible;
					canvas.Invalidate();
					break;
				case SampleBackends.OpenGL:
					glview.Visibility = Visibility.Visible;
					canvas.Visibility = Visibility.Collapsed;
					glview.Invalidate();
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

			var view = sender as SKXamlCanvas;
			DrawOverlayText(view, e.Surface.Canvas, view.CanvasSize, SampleBackends.Memory);
		}

		private void OnPaintGL(object sender, SKPaintGLSurfaceEventArgs e)
		{
			OnPaintSurface(e.Surface.Canvas, e.BackendRenderTarget.Width, e.BackendRenderTarget.Height);

			var view = sender as SKSwapChainPanel;
			DrawOverlayText(view, e.Surface.Canvas, view.CanvasSize, SampleBackends.OpenGL);
		}

		private void DrawOverlayText(FrameworkElement view, SKCanvas canvas, SKSize canvasSize, SampleBackends backend)
		{
			// make sure no previous transforms still apply
			canvas.ResetMatrix();

			// get and apply the current scale
			var scale = canvasSize.Width / (float)view.ActualWidth;
			canvas.Scale(scale);

			var y = (float)view.ActualHeight - TextOverlayPadding;

			var text = $"Current scaling = {scale:0.0}x";
			canvas.DrawText(text, TextOverlayPadding, y, textPaint);

			y -= textPaint.TextSize + TextOverlayPadding;

			text = "SkiaSharp: " + SamplesManager.SkiaSharpVersion;
			canvas.DrawText(text, TextOverlayPadding, y, textPaint);

			y -= textPaint.TextSize + TextOverlayPadding;

			text = "HarfBuzzSharp: " + SamplesManager.HarfBuzzSharpVersion;
			canvas.DrawText(text, TextOverlayPadding, y, textPaint);

			y -= textPaint.TextSize + TextOverlayPadding;

			text = "Backend: " + backend;
			canvas.DrawText(text, TextOverlayPadding, y, textPaint);
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
			if (canvas.Visibility == Visibility.Visible)
				canvas.Invalidate();
			if (glview.Visibility == Visibility.Visible)
				glview.Invalidate();
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
