using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

namespace SkiaSharpSample.Pages
{
	public partial class SamplesListPage : ContentPage
	{
		private readonly SampleBase[] samples;
		private CancellationTokenSource? cancellations;

		public ObservableCollection<GroupedSamples> Groups { get; } = new();

		public SamplesListPage()
		{
			samples = SamplesManager.GetSamples().ToArray();
			InitializeComponent();
			BindingContext = this;
			RefreshGroups(string.Empty);
		}

		private void RefreshGroups(string searchText)
		{
			Groups.Clear();
			foreach (var group in GetFilteredGroups(searchText))
				Groups.Add(group);
		}

		private IEnumerable<GroupedSamples> GetFilteredGroups(string searchText)
		{
			var filtered = samples.Where(s => s.MatchesFilter(searchText));
			return filtered
				.GroupBy(s => s.Category)
				.Select(g => new GroupedSamples(g.Key, g))
				.Where(g => g.Count > 0)
				.OrderBy(g => g.Name == SampleCategories.Showcases ? string.Empty : g.Name);
		}

		private void OnSearchTextChanged(object sender, TextChangedEventArgs e) =>
			RefreshGroups(e.NewTextValue ?? string.Empty);

		private async void OnSampleSelected(object sender, SelectionChangedEventArgs e)
		{
			if (e.CurrentSelection.FirstOrDefault() is SampleBase sample)
			{
				samplesCollectionView.SelectedItem = null;
				await Shell.Current.GoToAsync(
					$"{nameof(SampleDetailPage)}?sampleTitle={Uri.EscapeDataString(sample.Title)}");
			}
		}

		private void OnCycleSamplesClicked(object sender, EventArgs e)
		{
			if (cancellations != null)
			{
				cancellations.Cancel();
				cancellations = null;
				cycleButton.Text = "▶ Play";
			}
			else
			{
				cancellations = new CancellationTokenSource();
				var token = cancellations.Token;
				cycleButton.Text = "⏹ Stop";

				Task.Run(async () =>
				{
					try
					{
						var allSamples = GetFilteredGroups(string.Empty)
							.SelectMany(g => g)
							.Distinct()
							.ToList();

						if (allSamples.Count == 0) return;

						var current = allSamples[0];
						while (!token.IsCancellationRequested)
						{
							var title = current.Title;
							Dispatcher.Dispatch(async () =>
							{
								await Shell.Current.GoToAsync(
									$"{nameof(SampleDetailPage)}?sampleTitle={Uri.EscapeDataString(title)}");
							});

							await Task.Delay(3000, token);

							var idx = allSamples.IndexOf(current) + 1;
							if (idx >= allSamples.Count) idx = 0;
							current = allSamples[idx];
						}
					}
					catch (TaskCanceledException)
					{
					}
				}, token);
			}
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			cancellations?.Cancel();
			cancellations = null;
		}

		public class GroupedSamples : ObservableCollection<SampleBase>
		{
			public GroupedSamples(string category, IEnumerable<SampleBase> items)
			{
				Name = category;
				foreach (var item in items.OrderBy(s => s.Title))
					Add(item);
			}

			public string Name { get; }
		}
	}
}
