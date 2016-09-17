using System;
using System.Linq;
using Xamarin.Forms;

namespace SkiaSharpSample.FormsSample
{
	public partial class DetailsPage : NavigationPage
	{
		private SampleBase sample;

		public DetailsPage(DetailContentsPage root)
			: base(root)
		{
			InitializeComponent();
		}

		public SampleBase Sample
		{
			get { return sample; }
			set
			{
				sample = value;

				((DetailContentsPage)CurrentPage).Sample = value;
				Title = value.Title;
			}
		}

		public event EventHandler PlaySamples;

		private async void OnConfigureBackend(object sender, EventArgs e)
		{
			var items = Enum.GetNames(typeof(SampleBackends)).Except(new[] { nameof(SampleBackends.All) });
			var backendString = await DisplayActionSheet("Select Backend:", "Cancel", null, items.ToArray());

			SampleBackends backend;
			if (Enum.TryParse(backendString, out backend))
			{
				((DetailContentsPage)CurrentPage).SwitchBackend(backend);
			}
		}

		private void OnPlaySamples(object sender, EventArgs e)
		{
			PlaySamples?.Invoke(this, EventArgs.Empty);
		}
	}
}
