using System.Linq;
using Xamarin.Forms;

namespace SkiaSharpSample
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

			if (Device.RuntimePlatform == "Tizen")
			{
				var hamburger = new ToolbarItem
				{
					IconImageSource = "hamburger.png",
					Order = ToolbarItemOrder.Secondary
				};
				hamburger.Clicked += delegate { IsPresented = !IsPresented; };
				detailsPage.ToolbarItems.Add(hamburger);
			}

			Master = masterPage;
			Detail = detailsPage;
		}

		private static SampleBase[] GetPlatformSamples()
		{
			switch (Device.RuntimePlatform)
			{
				case Device.iOS:
					return SamplesManager.GetSamples(SamplePlatforms.iOS).ToArray();
				case Device.Android:
					return SamplesManager.GetSamples(SamplePlatforms.Android).ToArray();
				case Device.UWP:
					return SamplesManager.GetSamples(SamplePlatforms.UWP).ToArray();
				default:
					return SamplesManager.GetSamples(SamplePlatforms.All).ToArray();
			}
		}
	}
}
