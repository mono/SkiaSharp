using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace SkiaSharpSample
{
	public partial class Form1 : Form
	{
		private SampleBase sample;
		private CancellationTokenSource cancellations;
		private IList<SampleBase> samples;
		private IList<GroupedSamples> sampleGroups;

		public Form1()
		{
			InitializeComponent();

			samples = SamplesManager.GetSamples(SamplePlatforms.WindowsDesktop).ToList();
			sampleGroups = Enum.GetValues(typeof(SampleCategories))
				.Cast<SampleCategories>()
				.Select(c => new GroupedSamples(c, samples.Where(s => s.Category.HasFlag(c))))
				.Where(g => g.Samples.Count > 0)
				.OrderBy(g => g.Category == SampleCategories.Showcases ? string.Empty : g.Name)
				.ToList();

			SamplesInitializer.Init();

			foreach (var category in sampleGroups)
			{
				// create the menu item
				var menu = new ToolStripMenuItem(category.Name);
				foreach (var sample in category.Samples)
				{
					// create the sample item
					var menuItem = new ToolStripMenuItem(sample.Title, null, OnSampleSelected) { Tag = sample };
					menu.DropDownItems.Add(menuItem);
				}
				// add to the menu bar
				samplesMenu.DropDownItems.Add(menu);
			}

			SetSample(samples.First(s => s.Category.HasFlag(SampleCategories.Showcases)));
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			cancellations?.Cancel();
			cancellations = null;
		}

		private void OnSampleSelected(object sender, EventArgs e)
		{
			var menu = sender as ToolStripMenuItem;
			var sample = menu.Tag as SampleBase;
			SetSample(sample);
		}

		private void OnExitClicked(object sender, EventArgs e)
		{
			Close();
		}

		private void OnBackendChanged(object sender, EventArgs e)
		{
			var menu = sender as ToolStripMenuItem;
			var backend = (SampleBackends)Enum.Parse(typeof(SampleBackends), menu.Tag.ToString());
			switch (backend)
			{
				case SampleBackends.Memory:
					glview.Visible = false;
					canvas.Visible = true;
					break;
				case SampleBackends.OpenGL:
					glview.Visible = true;
					canvas.Visible = false;
					break;
				default:
					MessageBox.Show(
						"This functionality is not yet implemented.",
						"Configure Backend",
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
					break;
			}
		}

		private void OnToggleSlideshow(object sender, EventArgs e)
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
						var sortedSamples = sampleGroups.SelectMany(g => g.Samples).Distinct().ToList();
						var lastSample = sortedSamples.First();
						while (!token.IsCancellationRequested)
						{
							// display the sample
							Invoke(new Action(() => SetSample(lastSample)));

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

		private void OnPaintGL(object sender, SKPaintGLSurfaceEventArgs e)
		{
			OnPaintSurface(e.Surface.Canvas, e.RenderTarget.Width, e.RenderTarget.Height);
		}

		private void OnPaintCanvas(object sender, SKPaintSurfaceEventArgs e)
		{
			OnPaintSurface(e.Surface.Canvas, e.Info.Width, e.Info.Height);
		}

		private void OnSampleClicked(object sender, EventArgs e)
		{
			sample?.Tap();
		}

		private void SetSample(SampleBase newSample)
		{
			// clean up the old sample
			if (sample != null)
			{
				sample.RefreshRequested -= OnRefreshRequested;
				sample.Destroy();
			}

			sample = newSample;

			// set the title
			Text = sample?.Title ?? "SkiaSharp for Windows";

			// prepare the sample
			if (sample != null)
			{
				sample.RefreshRequested += OnRefreshRequested;
				sample.Init();
			}

			// refresh the view
			OnRefreshRequested(null, null);
		}

		private void OnRefreshRequested(object sender, EventArgs e)
		{
			canvas.Invalidate();
			glview.Invalidate();
		}

		private void OnPaintSurface(SKCanvas canvas, int width, int height)
		{
			sample?.DrawSample(canvas, width, height);
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
