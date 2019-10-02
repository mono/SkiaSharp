using System;
using System.Linq;
using Xamarin.Forms;

namespace SkiaSharpSample
{
	public partial class DetailsPage : NavigationPage
	{
		private SampleBase sample;

		public DetailsPage(DetailContentsPage root)
			: base(root)
		{
			InitializeComponent();

			if (Device.RuntimePlatform == "Tizen")
			{
				ToolbarItems.Clear();
				var item = new ToolbarItem
				{
					IconImageSource = "more.png"
				};
				item.Clicked += OnMore;
				ToolbarItems.Add(item);
			}
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

		private async void OnMore(object sender, EventArgs e)
		{
			var result = await DisplayActionSheet("More", "Close", null, "Configure Backend", "Toggle Slideshow");
			switch (result)
			{
				case "Toggle Slideshow":
					OnPlaySamples(sender, e);
					break;
				case "Configure Backend":
					OnConfigureBackend(sender, e);
					break;
			}
		}

		private async void OnConfigureBackend(object sender, EventArgs e)
		{
			var items = Enum.GetNames(typeof(SampleBackends)).Except(new[] { nameof(SampleBackends.All) });
			var backendString = await DisplayActionSheet("Select Backend:", "Close", null, items.ToArray());

			if (Enum.TryParse(backendString, out SampleBackends backend))
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
