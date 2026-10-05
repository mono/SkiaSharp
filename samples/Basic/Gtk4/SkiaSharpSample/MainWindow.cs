using System;
using System.IO;
using Gtk;

namespace SkiaSharpSample;

public class MainWindow : ApplicationWindow
{
	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;
	private uint themeCallback;

	public MainWindow(Application app)
		: base(new GObject.ConstructArgument[] { })
	{
		Application = app;
		Title = "SkiaSharp on Gtk4";
		SetDefaultSize(1024, 768);
		SystemTheme.Apply();

		OnMap += (sender, args) =>
		{
			long lastCheck = 0;
			themeCallback = AddTickCallback((widget, clock) =>
			{
				var now = clock.GetFrameTime();
				if (now - lastCheck >= 500_000)
				{
					SystemTheme.Apply();
					lastCheck = now;
				}
				return true;
			});
		};
		OnUnmap += (sender, args) =>
		{
			if (themeCallback != 0)
			{
				RemoveTickCallback(themeCallback);
				themeCallback = 0;
			}
		};

		var builder = LoadBuilder("MainWindow.ui");
		var rootBox = (Box)builder.GetObject("rootBox");
		Child = rootBox;

		var contentStack = (Stack)builder.GetObject("contentStack");

		// Add pages
		var cpuPage = new CpuPage();
		contentStack.AddTitled(cpuPage, "cpu", "CPU Canvas");

		if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsWindows())
		{
			var gpuPage = new GpuPage();
			contentStack.AddTitled(gpuPage, "gpu", "GPU Canvas");
		}
		else if (DefaultPage == SamplePage.Gpu)
		{
			throw new PlatformNotSupportedException("The GTK4 GPU page requires Linux, macOS or Windows.");
		}

		var drawingPage = new DrawingPage();
		contentStack.AddTitled(drawingPage, "drawing", "Drawing");

		contentStack.SetVisibleChildName(DefaultPage switch
		{
			SamplePage.Gpu => "gpu",
			SamplePage.Drawing => "drawing",
			_ => "cpu",
		});
	}

	public static Builder LoadBuilder(string filename)
	{
		var path = Path.Combine(AppContext.BaseDirectory, filename);
		return Builder.NewFromFile(path);
	}

	public override void Dispose()
	{
		if (themeCallback != 0)
		{
			RemoveTickCallback(themeCallback);
			themeCallback = 0;
		}
		base.Dispose();
	}
}
