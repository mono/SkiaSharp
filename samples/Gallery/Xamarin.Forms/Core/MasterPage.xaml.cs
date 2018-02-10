using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SkiaSharpSample
{
	public partial class MasterPage : ContentPage
	{
		private static readonly IEnumerable<SampleCategories> categories;

		private CancellationTokenSource cancellations;
		private SampleBase[] samples;

		static MasterPage()
		{
			categories = Enum.GetValues(typeof(SampleCategories)).Cast<SampleCategories>();
		}

		public MasterPage(SampleBase[] samples)
		{
			InitializeComponent();

			this.samples = samples;

			Samples = new ObservableCollection<GroupedSamples>(GetFilteredSamples(string.Empty));

			BindingContext = this;
		}

		public ObservableCollection<GroupedSamples> Samples { get; private set; }

		public event Action<SampleBase> SampleSelected;

		public void CycleSamples()
		{
			if (cancellations != null)
			{
				// cancel the old loop
				cancellations.Cancel();
				cancellations = null;
			}
			else
			{
				// start a new loop
				cancellations = new CancellationTokenSource();
				var token = cancellations.Token;
				Task.Run(async delegate
				{
					try
					{
						// get the samples in a list
						var distinct = GetFilteredSamples(string.Empty).SelectMany(g => g).Distinct().ToList();
						var lastSample = distinct.First();
						while (!token.IsCancellationRequested)
						{
							// display the sample
							Device.BeginInvokeOnMainThread(() => OnSampleSelected(lastSample));

							// wait a bit
							await Task.Delay(3000, token);

							// select the next one
							var idx = distinct.IndexOf(lastSample) + 1;
							if (idx >= distinct.Count)
							{
								idx = 0;
							}
							lastSample = distinct[idx];
						}
					}
					catch (TaskCanceledException)
					{
						// we are expecting this
					}
				});
			}
		}

		private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
		{
			Samples.Clear();
			foreach (var sample in GetFilteredSamples(e.NewTextValue))
			{
				Samples.Add(sample);
			}
		}

		private IEnumerable<GroupedSamples> GetFilteredSamples(string searchText)
		{
			var filteredSamples = samples.Where(s => s.MatchesFilter(searchText));

			return categories
				.Select(c => new GroupedSamples(c, filteredSamples.Where(s => s.Category.HasFlag(c))))
				.Where(g => g.Any())
				.OrderBy(g => g.Category == SampleCategories.Showcases ? string.Empty : g.Name);
		}

		private void OnSampleSelected(object sender, SelectedItemChangedEventArgs e)
		{
			// deselect the menu item
			if (e.SelectedItem == null)
			{
				return;
			}
			((ListView)sender).SelectedItem = null;

			// display the selected demo
			var sample = (SampleBase)e.SelectedItem;
			OnSampleSelected(sample);
		}

		private void OnSampleSelected(SampleBase sample)
		{
			SampleSelected?.Invoke(sample);
		}

		public class GroupedSamples : ObservableCollection<SampleBase>
		{
			private static readonly Regex EnumSplitRexeg = new Regex("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])");

			public GroupedSamples(SampleCategories category, IEnumerable<SampleBase> samples)
			{
				Category = category;

				Name = EnumSplitRexeg.Replace(category.ToString(), " $1");

				foreach (var sample in samples.OrderBy(s => s.Title))
				{
					Add(sample);
				}
			}

			public SampleCategories Category { get; private set; }

			public string Name { get; private set; }
		}
	}
}
