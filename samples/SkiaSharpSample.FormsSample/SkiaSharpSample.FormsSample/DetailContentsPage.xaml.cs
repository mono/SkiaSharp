using System;
using Xamarin.Forms;

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

				sample.Init(canvas.InvalidateSurface);

				Title = sample.Title;
				canvas.InvalidateSurface();
			}
		}

		public void SwitchBackend(SampleBackends backend)
		{
			DisplayAlert("Configure Backend", "This functionality is not yet implemented.", "OK");
		}

		private void OnTapSample(object sender, EventArgs e)
		{
			Sample?.Tap();
		}

		private void OnPaintSample(object sender, SKPaintSurfaceEventArgs e)
		{
			Sample?.DrawSample(e.Surface.Canvas, e.Info.Width, e.Info.Height);
		}
	}
}
