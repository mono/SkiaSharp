using System;
using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpSample.FormsSample
{
	public partial class DetailContentsPage : ContentPage
	{
		private SampleBase sample;

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
				sample = value;

				sample.Init(RefreshSamples);

				Title = sample.Title;
				RefreshSamples();
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
		}

		private void OnPaintSample(object sender, SKPaintSurfaceEventArgs e)
		{
			Sample?.DrawSample(e.Surface.Canvas, e.Info.Width, e.Info.Height);

			e.Surface.Canvas.ResetMatrix();
			var view = sender as SKCanvasView;
			var paint = new SKPaint { TextSize = 20 };
			e.Surface.Canvas.DrawText($"{view.CanvasSize.Width} / {view.Width} = {view.CanvasSize.Width / view.Width}", 10, 30, paint);
		}

		private void OnPaintGLSample(object sender, SKPaintGLSurfaceEventArgs e)
		{
			Sample?.DrawSample(e.Surface.Canvas, e.RenderTarget.Width, e.RenderTarget.Height);

			e.Surface.Canvas.ResetMatrix();
			var view = sender as SKGLView;
			var paint = new SKPaint { TextSize = 20 };
			e.Surface.Canvas.DrawText($"{view.CanvasSize.Width} / {view.Width} = {view.CanvasSize.Width / view.Width}", 10, 30, paint);
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
