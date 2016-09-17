using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SkiaSharpSample.FormsSample
{
	public partial class MasterPage : ContentPage
	{
		private CancellationTokenSource cancellations;

		public MasterPage(SampleBase[] samples)
		{
			InitializeComponent();

			var groups = Enum.GetValues(typeof(SampleCategories))
				.Cast<SampleCategories>()
				.Select(c => new GroupedSamples(c, samples.Where(s => s.Category.HasFlag(c))))
				.OrderBy(g => g.Category == SampleCategories.Showcases ? string.Empty : g.Name);
			Samples = new ObservableCollection<GroupedSamples>(groups);

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
						var samples = Samples.SelectMany(g => g).Distinct().ToList();
						var lastSample = samples.First();
						while (!token.IsCancellationRequested)
						{
							// display the sample
							Device.BeginInvokeOnMainThread(() => OnSampleSelected(lastSample));

							// wait a bit
							await Task.Delay(3000, token);

							// select the next one
							var idx = samples.IndexOf(lastSample) + 1;
							if (idx >= samples.Count)
							{
								idx = 0;
							}
							lastSample = samples[idx];
						}
					}
					catch (TaskCanceledException)
					{
						// we are expecting this
					}
				});
			}
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
