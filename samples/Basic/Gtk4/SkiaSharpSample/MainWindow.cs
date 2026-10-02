using System;
using System.IO;
using Gtk;

namespace SkiaSharpSample;

public class MainWindow : ApplicationWindow
{
	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

	public MainWindow(Application app)
		: base(new GObject.ConstructArgument[] { })
	{
		Application = app;
		Title = "SkiaSharp on Gtk4";
		SetDefaultSize(1024, 768);

		var builder = LoadBuilder("MainWindow.ui");
		var rootBox = (Box)builder.GetObject("rootBox");
		Child = rootBox;

		var contentStack = (Stack)builder.GetObject("contentStack");

		// Add pages
		var cpuPage = new CpuPage();
		contentStack.AddTitled(cpuPage, "cpu", "CPU Canvas");

		if (OperatingSystem.IsLinux())
		{
			var gpuPage = new GpuPage();
			contentStack.AddTitled(gpuPage, "gpu", "GPU Canvas");
		}
		else if (DefaultPage == SamplePage.Gpu)
		{
			throw new PlatformNotSupportedException("The GTK4 GPU page requires Linux desktop OpenGL.");
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
}
