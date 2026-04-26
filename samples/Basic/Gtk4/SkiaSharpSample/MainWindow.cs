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

		var drawingPage = new DrawingPage();
		contentStack.AddTitled(drawingPage, "drawing", "Drawing");

		if (DefaultPage == SamplePage.Drawing)
			contentStack.SetVisibleChildName("drawing");
	}

	public static Builder LoadBuilder(string filename)
	{
		var path = Path.Combine(AppContext.BaseDirectory, filename);
		return Builder.NewFromFile(path);
	}
}
