using AppKit;
using CoreAnimation;
using CoreGraphics;

namespace SkiaSharpSample;

internal sealed class PerformanceViewController : NSViewController
{
	private static readonly int[] Complexities =
	{
		100,
		250,
		500,
		1_000,
		2_000,
		4_000,
		8_000,
	};

	private static readonly int[] FrameRates = { 15, 30, 60, 120 };

	private readonly RenderSettingsStore settings = new();
	private readonly NSView controlBar;
	private readonly RendererPanel ganeshPanel;
	private readonly RendererPanel graphitePanel;
	private readonly NSPopUpButton displayPopup;
	private readonly NSPopUpButton workloadPopup;
	private readonly NSSlider workersSlider;
	private readonly NSSlider complexitySlider;
	private readonly NSSlider frameRateSlider;
	private readonly NSButton animateButton;
	private readonly NSTextField workersValue;
	private readonly NSTextField complexityValue;
	private readonly NSTextField frameRateValue;
	private DisplayMode displayMode;
	private System.Threading.Timer? abTimer;
	private bool abGraphiteActive;
	private bool disposed;

	public PerformanceViewController()
	{
		View = new NSView(new CGRect(0, 0, 1440, 900))
		{
			WantsLayer = true,
		};
		View.Layer!.BackgroundColor = new CGColor(0.035f, 0.043f, 0.075f);

		controlBar = new NSView
		{
			WantsLayer = true,
		};
		controlBar.Layer!.BackgroundColor = new CGColor(0.07f, 0.08f, 0.13f);
		View.AddSubview(controlBar);

		var ganeshView = new GaneshMetalPerformanceView(CGRect.Empty, settings);
		var graphiteView = new GraphiteMetalPerformanceView(CGRect.Empty, settings);
		ganeshPanel = new RendererPanel(
			"GANESH / METAL",
			"Serial command preparation + onscreen composite",
			ganeshView);
		graphitePanel = new RendererPanel(
			"GRAPHITE / METAL",
			"Worker Recorders + onscreen composite",
			graphiteView);
		View.AddSubview(ganeshPanel);
		View.AddSubview(graphitePanel);

		displayPopup = CreatePopup(
			"View",
			new[]
			{
				"Isolated A/B",
				"Live side by side",
				"Ganesh only",
				"Graphite only",
			},
			0,
			0);
		displayPopup.Activated += (_, _) =>
		{
			displayMode = (DisplayMode)(int)displayPopup.IndexOfSelectedItem;
			ApplyDisplayMode();
			View.NeedsLayout = true;
		};

		workloadPopup = CreatePopup(
			"Workload",
			new[]
			{
				"UI dashboard",
				"Vector tiles",
				"Sprite atlas",
				"Text grid",
			},
			0,
			0);
		workloadPopup.Activated += (_, _) =>
			settings.Update(current => current with
			{
				Workload = (WorkloadKind)(int)workloadPopup.IndexOfSelectedItem,
			});

		var initial = settings.Current;
		workersSlider = CreateSlider(
			"Tiles / Graphite workers",
			1,
			8,
			initial.WorkerCount);
		workersValue = CreateValueLabel($"{initial.WorkerCount}");
		workersSlider.Activated += (_, _) =>
		{
			var count = (int)Math.Round(workersSlider.DoubleValue);
			workersValue.StringValue = count.ToString();
			settings.Update(current => current with { WorkerCount = count });
		};

		var complexityIndex = Array.IndexOf(Complexities, initial.Complexity);
		complexitySlider = CreateSlider(
			"Items per tile",
			0,
			Complexities.Length - 1,
			complexityIndex);
		complexityValue = CreateValueLabel($"{initial.Complexity:N0}");
		complexitySlider.Activated += (_, _) =>
		{
			var index = (int)Math.Round(complexitySlider.DoubleValue);
			var complexity = Complexities[index];
			complexityValue.StringValue = $"{complexity:N0}";
			settings.Update(current => current with { Complexity = complexity });
		};

		var frameRateIndex = Array.IndexOf(FrameRates, initial.FramesPerSecond);
		frameRateSlider = CreateSlider(
			"Target frame rate",
			0,
			FrameRates.Length - 1,
			frameRateIndex);
		frameRateValue = CreateValueLabel($"{initial.FramesPerSecond} Hz");
		frameRateSlider.Activated += (_, _) =>
		{
			var index = (int)Math.Round(frameRateSlider.DoubleValue);
			var frameRate = FrameRates[index];
			frameRateValue.StringValue = $"{frameRate} Hz";
			settings.Update(current => current with
			{
				FramesPerSecond = frameRate,
			});
		};

		animateButton = new NSButton
		{
			Title = "Animate scene",
			State = initial.Animate
				? NSCellStateValue.On
				: NSCellStateValue.Off,
		};
		animateButton.SetButtonType(NSButtonType.Switch);
		animateButton.Activated += (_, _) =>
			settings.Update(current => current with
			{
				Animate = animateButton.State == NSCellStateValue.On,
			});
		controlBar.AddSubview(animateButton);

		var note = CreateTextField(
			"Both panels present real MTKView drawables. Graphite records independent " +
			"tiles on worker Recorders while the UI keeps presenting the last completed set. " +
			"Select one backend with View for isolated numbers.",
			12,
			SecondaryTextColor);
		note.MaximumNumberOfLines = 3;
		note.LineBreakMode = NSLineBreakMode.ByWordWrapping;
		controlBar.AddSubview(note);
		NoteLabel = note;

		ganeshView.MetricsUpdated += (_, metrics) =>
			BeginInvokeOnMainThread(() =>
				ganeshPanel.SetMetrics(FormatMetrics(metrics, parallel: false)));
		graphiteView.MetricsUpdated += (_, metrics) =>
			BeginInvokeOnMainThread(() =>
				graphitePanel.SetMetrics(FormatMetrics(metrics, parallel: true)));
		ganeshView.RenderFailed += (_, exception) =>
			BeginInvokeOnMainThread(() =>
				ganeshPanel.SetError(exception.Message));
		graphiteView.RenderFailed += (_, exception) =>
			BeginInvokeOnMainThread(() =>
				graphitePanel.SetError(exception.Message));

		displayMode = DisplayMode.IsolatedAB;
		ApplyDisplayMode();
	}

	private NSTextField NoteLabel { get; }

	public override void ViewDidLayout()
	{
		base.ViewDidLayout();
		var bounds = View.Bounds;
		nfloat controlHeight = 128;
		nfloat margin = 12;
		nfloat gap = 12;

		controlBar.Frame = new CGRect(
			0,
			bounds.Height - controlHeight,
			bounds.Width,
			controlHeight);
		LayoutControls(controlBar.Bounds);

		var contentHeight = Math.Max(1, bounds.Height - controlHeight - margin);
		switch (displayMode)
		{
			case DisplayMode.IsolatedAB:
			case DisplayMode.SideBySideContended:
				var panelWidth = Math.Max(1, (bounds.Width - margin * 2 - gap) / 2);
				ganeshPanel.Frame = new CGRect(
					margin,
					margin,
					panelWidth,
					contentHeight - margin);
				graphitePanel.Frame = new CGRect(
					margin + panelWidth + gap,
					margin,
					panelWidth,
					contentHeight - margin);
				break;
			case DisplayMode.Ganesh:
				ganeshPanel.Frame = new CGRect(
					margin,
					margin,
					bounds.Width - margin * 2,
					contentHeight - margin);
				break;
			case DisplayMode.Graphite:
				graphitePanel.Frame = new CGRect(
					margin,
					margin,
					bounds.Width - margin * 2,
					contentHeight - margin);
				break;
		}

		ganeshPanel.LayoutContents();
		graphitePanel.LayoutContents();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			disposed = true;
			abTimer?.Dispose();
			abTimer = null;
			ganeshPanel.RenderView.StopAndDrain();
			graphitePanel.RenderView.StopAndDrain();
		}
		base.Dispose(disposing);
	}

	private void ApplyDisplayMode()
	{
		abTimer?.Dispose();
		abTimer = null;
		ganeshPanel.RenderView.StopAndDrain();
		graphitePanel.RenderView.StopAndDrain();

		switch (displayMode)
		{
			case DisplayMode.IsolatedAB:
				ganeshPanel.Hidden = false;
				graphitePanel.Hidden = false;
				abGraphiteActive = false;
				ApplyABActiveBackend();
				NoteLabel.StringValue =
					"Isolated A/B keeps both last-presented images visible but measures only " +
					"one backend at a time. It switches every four seconds so Ganesh cannot " +
					"starve Graphite on AppKit's shared UI thread.";
				abTimer = new System.Threading.Timer(
					_ => BeginInvokeOnMainThread(SwitchABBackend),
					null,
					TimeSpan.FromSeconds(4),
					TimeSpan.FromSeconds(4));
				break;
			case DisplayMode.SideBySideContended:
				ganeshPanel.Hidden = false;
				graphitePanel.Hidden = false;
				ganeshPanel.RenderView.SetRendering(true);
				graphitePanel.RenderView.SetRendering(true);
				ganeshPanel.SetRunState("LIVE · CONTENDED");
				graphitePanel.SetRunState("LIVE · CONTENDED");
				NoteLabel.StringValue =
					"Live side by side is for visual parity only. Both views share the AppKit " +
					"thread and GPU; a slow Ganesh callback delays Graphite and invalidates " +
					"throughput comparisons.";
				break;
			case DisplayMode.Ganesh:
				ganeshPanel.Hidden = false;
				graphitePanel.Hidden = true;
				ganeshPanel.RenderView.SetRendering(true);
				graphitePanel.RenderView.SetRendering(false);
				ganeshPanel.SetRunState("MEASURING · ISOLATED");
				NoteLabel.StringValue =
					"Ganesh is the only active renderer. Use this mode for an uncontended " +
					"Ganesh frame-rate and UI-thread measurement.";
				break;
			case DisplayMode.Graphite:
				ganeshPanel.Hidden = true;
				graphitePanel.Hidden = false;
				ganeshPanel.RenderView.SetRendering(false);
				graphitePanel.RenderView.SetRendering(true);
				graphitePanel.SetRunState("MEASURING · ISOLATED");
				NoteLabel.StringValue =
					"Graphite is the only active renderer. Use this mode for an uncontended " +
					"Graphite frame-rate and UI-thread measurement.";
				break;
		}
	}

	private void SwitchABBackend()
	{
		if (disposed || displayMode != DisplayMode.IsolatedAB)
			return;

		if (abGraphiteActive)
			graphitePanel.RenderView.StopAndDrain();
		else
			ganeshPanel.RenderView.StopAndDrain();
		abGraphiteActive = !abGraphiteActive;
		ApplyABActiveBackend();
	}

	private void ApplyABActiveBackend()
	{
		ganeshPanel.RenderView.SetRendering(!abGraphiteActive);
		graphitePanel.RenderView.SetRendering(abGraphiteActive);
		ganeshPanel.SetRunState(
			abGraphiteActive
				? "PAUSED SNAPSHOT"
				: "MEASURING · ISOLATED");
		graphitePanel.SetRunState(
			abGraphiteActive
				? "MEASURING · ISOLATED"
				: "PAUSED SNAPSHOT");
	}

	private void LayoutControls(CGRect bounds)
	{
		nfloat top = 76;
		nfloat labelY = 94;

		displayPopup.Frame = new CGRect(16, top, 135, 26);
		DisplayLabel.Frame = new CGRect(16, labelY, 135, 18);

		workloadPopup.Frame = new CGRect(164, top, 145, 26);
		WorkloadLabel.Frame = new CGRect(164, labelY, 145, 18);

		workersSlider.Frame = new CGRect(326, top, 160, 24);
		WorkersLabel.Frame = new CGRect(326, labelY, 160, 18);
		workersValue.Frame = new CGRect(492, top + 1, 42, 22);

		complexitySlider.Frame = new CGRect(548, top, 180, 24);
		ComplexityLabel.Frame = new CGRect(548, labelY, 180, 18);
		complexityValue.Frame = new CGRect(734, top + 1, 65, 22);

		frameRateSlider.Frame = new CGRect(812, top, 142, 24);
		FrameRateLabel.Frame = new CGRect(812, labelY, 142, 18);
		frameRateValue.Frame = new CGRect(960, top + 1, 58, 22);

		animateButton.Frame = new CGRect(1032, top - 1, 130, 26);
		NoteLabel.Frame = new CGRect(
			16,
			10,
			Math.Max(300, bounds.Width - 32),
			48);
	}

	private NSTextField DisplayLabel { get; set; } = null!;

	private NSTextField WorkloadLabel { get; set; } = null!;

	private NSTextField WorkersLabel { get; set; } = null!;

	private NSTextField ComplexityLabel { get; set; } = null!;

	private NSTextField FrameRateLabel { get; set; } = null!;

	private NSPopUpButton CreatePopup(
		string title,
		string[] items,
		nfloat x,
		nfloat y)
	{
		var label = CreateTextField(title, 12, SecondaryTextColor);
		controlBar.AddSubview(label);
		switch (title)
		{
			case "View":
				DisplayLabel = label;
				break;
			case "Workload":
				WorkloadLabel = label;
				break;
		}

		var popup = new NSPopUpButton(new CGRect(x, y, 140, 26), pullsDown: false);
		popup.AddItems(items);
		controlBar.AddSubview(popup);
		return popup;
	}

	private NSSlider CreateSlider(
		string title,
		double minimum,
		double maximum,
		double value)
	{
		var label = CreateTextField(title, 12, SecondaryTextColor);
		controlBar.AddSubview(label);
		switch (title)
		{
			case "Tiles / Graphite workers":
				WorkersLabel = label;
				break;
			case "Items per tile":
				ComplexityLabel = label;
				break;
			case "Target frame rate":
				FrameRateLabel = label;
				break;
		}

		var slider = new NSSlider
		{
			MinValue = minimum,
			MaxValue = maximum,
			DoubleValue = value,
			Continuous = true,
		};
		controlBar.AddSubview(slider);
		return slider;
	}

	private NSTextField CreateValueLabel(string value)
	{
		var label = CreateTextField(value, 12, NSColor.White);
		label.Alignment = NSTextAlignment.Center;
		label.Bezeled = true;
		label.DrawsBackground = true;
		label.BackgroundColor = NSColor.FromRgba(0.12f, 0.13f, 0.19f, 1f);
		controlBar.AddSubview(label);
		return label;
	}

	private static string FormatMetrics(RenderMetrics metrics, bool parallel)
	{
		var tileLabel = parallel ? "worker record" : "tile prepare";
		var uiLabel = parallel ? "UI busy" : "UI serial";
		return
			$"{metrics.FramesPerSecond,5:F1} present    " +
			$"{metrics.ContentUpdatesPerSecond,5:F1} updates/s    " +
			$"CPU frame {metrics.CpuFrameMs,6:F2} ms    " +
			$"{tileLabel} {metrics.TileWallMs,6:F2} ms    " +
			$"{uiLabel} {metrics.UiThreadBusyMs,6:F2} ms";
	}

	private static NSTextField CreateTextField(
		string text,
		nfloat size,
		NSColor color)
	{
		return new NSTextField
		{
			StringValue = text,
			Editable = false,
			Selectable = false,
			Bezeled = false,
			DrawsBackground = false,
			TextColor = color,
			Font = NSFont.SystemFontOfSize(size)!,
		};
	}

	private static NSColor SecondaryTextColor =>
		NSColor.FromRgba(0.68f, 0.70f, 0.76f, 1f);

	private static NSColor MetricsTextColor =>
		NSColor.FromRgba(0.45f, 0.82f, 1f, 1f);

	private sealed class RendererPanel : NSView
	{
		private readonly string subtitleText;
		private readonly NSTextField title;
		private readonly NSTextField subtitle;
		private readonly NSTextField metrics;

		public RendererPanel(
			string titleText,
			string subtitleText,
			PerformanceMetalView renderView)
		{
			this.subtitleText = subtitleText;
			WantsLayer = true;
			Layer!.BackgroundColor = new CGColor(0.055f, 0.065f, 0.105f);
			Layer.CornerRadius = 12;
			Layer.MasksToBounds = true;

			title = CreateTextField(titleText, 18, NSColor.White);
			title.Font = NSFont.SystemFontOfSize(
				18,
				NSFontWeight.Semibold)!;
			subtitle = CreateTextField(
				subtitleText,
				12,
				SecondaryTextColor);
			metrics = CreateTextField(
				"Warming up...",
				12,
				MetricsTextColor);
			RenderView = renderView;
			AddSubview(title);
			AddSubview(subtitle);
			AddSubview(metrics);
			AddSubview(RenderView);
		}

		public PerformanceMetalView RenderView { get; }

		public void SetMetrics(string value)
		{
			metrics.StringValue = value;
			metrics.TextColor = MetricsTextColor;
		}

		public void SetError(string value)
		{
			metrics.StringValue = $"Render failed: {value}";
			metrics.TextColor = NSColor.SystemRed;
		}

		public void SetRunState(string state)
		{
			subtitle.StringValue = $"{subtitleText}  ·  {state}";
		}

		public void LayoutContents()
		{
			var bounds = Bounds;
			title.Frame = new CGRect(14, bounds.Height - 34, bounds.Width - 28, 22);
			subtitle.Frame = new CGRect(
				14,
				bounds.Height - 54,
				bounds.Width - 28,
				18);
			metrics.Frame = new CGRect(
				14,
				bounds.Height - 76,
				bounds.Width - 28,
				18);
			RenderView.Frame = new CGRect(
				0,
				0,
				bounds.Width,
				Math.Max(1, bounds.Height - 84));
		}

	}
}
