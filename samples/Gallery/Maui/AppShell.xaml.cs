using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;
using SkiaSharpSample.Pages;

namespace SkiaSharpSample
{
	public partial class AppShell : Shell
	{
		private static readonly Regex EnumSplitRegex = new("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])");

		public AppShell()
		{
			InitializeComponent();

			Routing.RegisterRoute(nameof(SampleDetailPage), typeof(SampleDetailPage));

			PopulateFlyout();
		}

		private void PopulateFlyout()
		{
			var samples = SamplesManager.GetSamples(SamplePlatforms.MAUI).ToList();
			var categories = Enum.GetValues(typeof(SampleCategories))
				.Cast<SampleCategories>()
				.Select(c => new
				{
					Category = c,
					Name = EnumSplitRegex.Replace(c.ToString(), " $1"),
					Samples = samples.Where(s => s.Category.HasFlag(c)).OrderBy(s => s.Title).ToList()
				})
				.Where(g => g.Samples.Count > 0)
				.OrderBy(g => g.Category == SampleCategories.Showcases ? string.Empty : g.Name);

			foreach (var group in categories)
			{
				var flyoutItem = new FlyoutItem
				{
					Title = group.Name,
					FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems,
				};

				foreach (var sample in group.Samples)
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
