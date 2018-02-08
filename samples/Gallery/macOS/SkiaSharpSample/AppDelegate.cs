using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace SkiaSharpSample
{
	[Register("AppDelegate")]
	public partial class AppDelegate : NSApplicationDelegate
	{
		private MainViewController controller;
		private CancellationTokenSource cancellations;
		private IList<SampleBase> samples;
		private IList<GroupedSamples> sampleGroups;

		public AppDelegate()
		{
			samples = SamplesManager.GetSamples(SamplePlatforms.OSX).ToList();
			sampleGroups = Enum.GetValues(typeof(SampleCategories))
				.Cast<SampleCategories>()
				.Select(c => new GroupedSamples(c, samples.Where(s => s.Category.HasFlag(c))))
				.Where(g => g.Samples.Count > 0)
				.OrderBy(g => g.Category == SampleCategories.Showcases ? string.Empty : g.Name)
				.ToList();
		}

		public MainViewController Controller
		{
			get { return controller; }
			set
			{
				controller = value;
				controller?.SetSample(samples.First(s => s.Category.HasFlag(SampleCategories.Showcases)));
			}
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;

		public override void DidFinishLaunching(NSNotification notification)
		{
			SamplesInitializer.Init();

			foreach (var category in sampleGroups)
			{
				// create the menu item
				var menu = new NSMenuItem(category.Name);
				menu.Submenu = new NSMenu(category.Name);
				foreach (var sample in category.Samples)
				{
					// create the sample item
					var tag = samples.IndexOf(sample);
					menu.Submenu.AddItem(new NSMenuItem(sample.Title, OnSampleSelected) { Tag = tag });
				}
				// add to the menu bar
				samplesMenu.Submenu.AddItem(menu);
			}
		}

		public override void WillTerminate(NSNotification notification)
		{
			cancellations?.Cancel();
			cancellations = null;
		}

		private void OnSampleSelected(object sender, EventArgs e)
		{
			var menu = sender as NSMenuItem;
			if (menu?.Tag >= 0)
			{
				Controller?.SetSample(samples[(int)menu.Tag]);
			}
		}

		partial void OnBackendChanged(NSObject sender)
		{
			var menu = sender as NSMenuItem;
			Controller?.ChangeBackend((SampleBackends)(int)menu.Tag);
		}

		partial void OnPlaySamples(NSObject sender)
		{
			if (cancellations != null)
			{
				// cancel the old loop
				cancellations.Cancel();
				cancellations = null;
			}
			else if (Controller != null)
			{
				// start a new loop
				cancellations = new CancellationTokenSource();
				var token = cancellations.Token;
				Task.Run(async delegate
				{
					try
					{
						// get the samples in a list
						var sortedSamples = sampleGroups.SelectMany(g => g.Samples).Distinct().ToList();
						var lastSample = sortedSamples.First();
						while (!token.IsCancellationRequested)
						{
							// display the sample
							InvokeOnMainThread(() => Controller?.SetSample(lastSample));

							// wait a bit
							await Task.Delay(3000, token);

							// select the next one
							var idx = sortedSamples.IndexOf(lastSample) + 1;
							if (idx >= sortedSamples.Count)
							{
								idx = 0;
							}
							lastSample = sortedSamples[idx];
						}
					}
					catch (TaskCanceledException)
					{
						// we are expecting this
					}
				});
			}
		}

		public class GroupedSamples
		{
			private static readonly Regex EnumSplitRexeg = new Regex("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])");

			public GroupedSamples(SampleCategories category, IEnumerable<SampleBase> samples)
			{
				Category = category;
				Name = EnumSplitRexeg.Replace(category.ToString(), " $1");
				Samples = samples.OrderBy(s => s.Title).ToList();
			}

			public SampleCategories Category { get; private set; }

			public string Name { get; private set; }

			public IList<SampleBase> Samples { get; private set; }
		}
	}
}
