using System.IO;
using System.Reflection;
using Gtk;

namespace SkiaSharpSample;

public class MainWindow : Gtk.Window
{
	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

	public MainWindow()
		: base("SkiaSharp on Gtk3")
	{
		SetDefaultSize(1024, 768);
		DeleteEvent += (s, e) => Application.Quit();

		// Load shell layout from Glade
		var builder = LoadBuilder("SkiaSharpSample.MainWindow.glade");
		var rootBox = (Box)builder.GetObject("rootBox");
		var stack = (Stack)builder.GetObject("contentStack");

		// Add pages — each loads its own Glade layout
		var cpuPage = new CpuPage();
		stack.AddTitled(cpuPage, "cpu", "CPU Canvas");

		var drawingPage = new DrawingPage();
		stack.AddTitled(drawingPage, "drawing", "Drawing");

		Add(rootBox);
		ShowAll();

		if (DefaultPage == SamplePage.Drawing)
			stack.VisibleChildName = "drawing";
	}

	internal static Builder LoadBuilder(string resourceName)
	{
		var builder = new Builder();
		var assembly = Assembly.GetExecutingAssembly();
		using var stream = assembly.GetManifestResourceStream(resourceName);
		using var reader = new StreamReader(stream);
		builder.AddFromString(reader.ReadToEnd());
		return builder;
	}
}
