using System.Linq;
using Xamarin.Forms;

namespace SkiaSharpSample.FormsSample
{
	public partial class MainPage : MasterDetailPage
	{
		public MainPage()
		{
			InitializeComponent();

			// get the samples for this platform
			var samples = GetPlatformSamples();
			var showcase = samples.First(s => s.Category.HasFlag(SampleCategories.Showcases));

			var detailContents = new DetailContentsPage(showcase);
			var detailsPage = new DetailsPage(detailContents);
			var masterPage = new MasterPage(samples);

			masterPage.SampleSelected += sample =>
			{
				detailsPage.Sample = sample;

				IsPresented = false;
			};
			detailsPage.PlaySamples += delegate { masterPage.CycleSamples(); };

			Master = masterPage;
			Detail = detailsPage;
		}

		private static SampleBase[] GetPlatformSamples()
		{
			switch (Device.OS)
			{
				case TargetPlatform.iOS:
					return SamplesManager.GetSamples(SamplePlatforms.iOS).ToArray();
				case TargetPlatform.Android:
					return SamplesManager.GetSamples(SamplePlatforms.Android).ToArray();
				case TargetPlatform.Windows:
					return SamplesManager.GetSamples(SamplePlatforms.UWP).ToArray();
				default:
					return SamplesManager.GetSamples(SamplePlatforms.All).ToArray();
			}
		}
	}
}
