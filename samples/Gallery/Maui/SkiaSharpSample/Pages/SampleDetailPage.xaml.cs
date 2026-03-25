using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace SkiaSharpSample.Pages
{
	[QueryProperty(nameof(SampleTitle), "sampleTitle")]
	public partial class SampleDetailPage : ContentPage
	{
		private SampleBase? sample;

		public SampleDetailPage()
		{
			InitializeComponent();
		}

		public string? SampleTitle
		{
			set
			{
				var title = Uri.UnescapeDataString(value ?? string.Empty);
				var newSample = SamplesManager.GetSample(title);
				SetSample(newSample);
			}
		}

		private void SetSample(SampleBase? newSample)
		{
			// Tear down old sample
			if (sample is not null)
			{
				sample.RefreshRequested -= OnRefreshRequested;
				sample.Destroy();
			}

			sample = newSample;
			Title = sample?.Title ?? "Sample";

			// Set up new sample
			if (sample is not null)
			{
				sample.RefreshRequested += OnRefreshRequested;
				sample.Init();

				UpdateInfoOverlay();
			}

			skiaView.InvalidateSurface();
		}

		private void UpdateInfoOverlay()
		{
			if (sample is null) return;

			var scale = skiaView.CanvasSize.Width > 0
				? skiaView.CanvasSize.Width / (float)skiaView.Width
				: 1f;

			infoLabel.Text =
				$"Scaling: {scale:0.0}x  |  " +
				$"SkiaSharp: {SamplesManager.SkiaSharpVersion}  |  " +
				$"HarfBuzz: {SamplesManager.HarfBuzzSharpVersion}";

			infoOverlay.IsVisible = true;
		}

		private void OnRefreshRequested(object? sender, EventArgs e) =>
			skiaView.InvalidateSurface();

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			e.Surface.Canvas.Clear(SKColors.White);
			sample?.DrawSample(e.Surface.Canvas, e.Info.Width, e.Info.Height);
		}

		private void OnTapSample(object sender, TappedEventArgs e)
		{
			sample?.Tap();
			skiaView.InvalidateSurface();
		}

		private void OnPanSample(object sender, PanUpdatedEventArgs e)
		{
			var scale = skiaView.CanvasSize.Width > 0
				? skiaView.CanvasSize.Width / (float)skiaView.Width
				: 1f;

			sample?.Pan(
				(GestureState)(int)e.StatusType,
				new SKPoint((float)e.TotalX * scale, (float)e.TotalY * scale));

			skiaView.InvalidateSurface();
		}

		private void OnPinchSample(object sender, PinchGestureUpdatedEventArgs e)
		{
			var size = skiaView.CanvasSize;

			sample?.Pinch(
				(GestureState)(int)e.Status,
				(float)e.Scale,
				new SKPoint((float)e.ScaleOrigin.X * size.Width, (float)e.ScaleOrigin.Y * size.Height));

			skiaView.InvalidateSurface();
		}

		private void OnResetMatrixClicked(object? sender, EventArgs e)
		{
			sample?.ResetMatrix();
			skiaView.InvalidateSurface();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			SetSample(null);
		}
	}
}
