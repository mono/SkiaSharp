using System;
using System.Linq;
using Microsoft.Maui.Controls;
using SkiaSharpSample.Pages;

namespace SkiaSharpSample
{
	public partial class AppShell : Shell
	{
		public AppShell()
		{
			InitializeComponent();

			Routing.RegisterRoute(nameof(SampleDetailPage), typeof(SampleDetailPage));

			PopulateFlyout();
		}

		private void PopulateFlyout()
		{
			var samples = SamplesManager.GetSamples().ToList();
			var groups = samples
				.GroupBy(s => s.Category)
				.OrderBy(g => g.Key == SampleCategories.Showcases ? "" : g.Key);

			foreach (var group in groups)
			{
				var flyoutItem = new FlyoutItem
				{
					Title = group.Key,
					FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems,
				};

				foreach (var sample in group.OrderBy(s => s.Title))
				{
					var content = new ShellContent
					{
						Title = sample.Title,
						Route = $"sample_{sample.Title.Replace(" ", "_")}",
						ContentTemplate = new DataTemplate(() =>
						{
							var page = new SampleDetailPage();
							page.SampleTitle = Uri.EscapeDataString(sample.Title);
							return page;
						}),
					};
					flyoutItem.Items.Add(new Tab { Items = { content } });
				}

				Items.Add(flyoutItem);
			}
		}
	}
}
