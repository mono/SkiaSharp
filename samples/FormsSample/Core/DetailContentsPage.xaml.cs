using System;
using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpSample.FormsSample
{
	public partial class DetailContentsPage : ContentPage
	{
		private SampleBase sample;
		private SKImage lastImage;

		public DetailContentsPage(SampleBase showcase)
		{
			InitializeComponent();

			Sample = showcase;
			BindingContext = this;
		}

		public SampleBase Sample
		{
			get { return sample; }
			set
			{
				// clean up the old sample
				if (sample != null)
				{
					sample.RefreshRequested -= OnRefreshRequested;
					sample.Destroy();
				}

				sample = value;
				Title = sample?.Title;

				// prepare the sample
				if (sample != null)
				{
					sample.RefreshRequested += OnRefreshRequested;
					sample.Init();
				}

				// refresh the view
				OnRefreshRequested(null, null);
			}
		}

		public void SwitchBackend(SampleBackends backend)
		{
			switch (backend)
			{
				case SampleBackends.Memory:
					canvas.IsVisible = true;
					canvas.InvalidateSurface();
					glview.IsVisible = false;
					canvas.IgnorePixelScaling = !canvas.IgnorePixelScaling;
					break;
				case SampleBackends.OpenGL:
					glview.IsVisible = true;
					glview.InvalidateSurface();
					canvas.IsVisible = false;
					break;
				case SampleBackends.Vulkan:
				default:
					DisplayAlert("Configure Backend", "This functionality is not yet implemented.", "OK");
					break;
			}
		}

		private void OnTapSample(object sender, EventArgs e)
		{
			Sample?.Tap();

			//// mostly for testing, but also useful
			//Navigation.PushAsync(new ContentPage
			//{
			//	Title = "Preview",
			//	Content = new Image
			//	{
			//		Source = new SKImageImageSource
			//		{
			//			Image = lastImage
			//		}
			//	}
			//});
		}

		private void OnPanSample(object sender, PanUpdatedEventArgs e)
		{
			var scale = canvas.CanvasSize.Width / (float)canvas.Width;
			if (glview.IsVisible)
				scale = glview.CanvasSize.Width / (float)glview.Width;
			
			Sample?.Pan(
				(GestureState)(int)e.StatusType,
				new SKPoint((float)e.TotalX * scale, (float)e.TotalY * scale));
			RefreshSamples();
		}

		private void OnPinchSample(object sender, PinchGestureUpdatedEventArgs e)
		{
			var size = canvas.CanvasSize;
			if (glview.IsVisible)
				size = glview.CanvasSize;

			Sample?.Pinch(
				(GestureState)(int)e.Status,
				(float)e.Scale,
				new SKPoint((float)e.ScaleOrigin.X * size.Width, (float)e.ScaleOrigin.Y * size.Height));
			RefreshSamples();
		}

		private void OnPaintSample(object sender, SKPaintSurfaceEventArgs e)
		{
			Sample?.DrawSample(e.Surface.Canvas, e.Info.Width, e.Info.Height);

			lastImage?.Dispose();
			lastImage = e.Surface.Snapshot();

			var view = sender as SKCanvasView;
			DrawScaling(view, e.Surface.Canvas, view.CanvasSize);
		}

		private void OnPaintGLSample(object sender, SKPaintGLSurfaceEventArgs e)
		{
			Sample?.DrawSample(e.Surface.Canvas, e.RenderTarget.Width, e.RenderTarget.Height);

			lastImage?.Dispose();
			lastImage = e.Surface.Snapshot();

			var view = sender as SKGLView;
			DrawScaling(view, e.Surface.Canvas, view.CanvasSize);
		}

		private void DrawScaling(View view, SKCanvas canvas, SKSize canvasSize)
		{
			// make sure no previous transforms still apply
			canvas.ResetMatrix();

			// get the current scale
			var scale = canvasSize.Width / (float)view.Width;

			// write the scale into the bottom left
			using (var paint = new SKPaint())
			{
				paint.IsAntialias = true;
				paint.TextSize = 20 * scale;

				var text = $"Current scaling = {scale:0.0}x";
				var padding = 10 * scale;
				canvas.DrawText(text, padding, canvasSize.Height - padding, paint);
			}
		}

		private void OnRefreshRequested(object sender, EventArgs e)
		{
			RefreshSamples();
		}

		private void RefreshSamples()
		{
			if (canvas.IsVisible)
			{
				canvas.InvalidateSurface();
			}
			if (glview.IsVisible)
			{
				glview.InvalidateSurface();
			}
		}
	}
}
