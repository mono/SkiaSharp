using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.NUI.Components;

namespace SkiaSharpSample;

public class App : NUIApplication
{
	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

	private GpuPage? gpuPage;

	public static void Main(string[] args) => new App().Run(args);

	protected override void OnCreate()
	{
		base.OnCreate();

		GetDefaultWindow().KeyEvent += OnKeyEvent;

		var tabView = new TabView
		{
			WidthSpecification = LayoutParamPolicies.MatchParent,
			HeightSpecification = LayoutParamPolicies.MatchParent,
		};

		tabView.AddTab(new TabButton { Text = "CPU" }, new CpuPage());

		gpuPage = new GpuPage();
		tabView.AddTab(new TabButton { Text = "GPU" }, gpuPage);

		tabView.AddTab(new TabButton { Text = "Drawing" }, new DrawingPage());

		GetDefaultWindow().Add(tabView);
	}

	void OnKeyEvent(object? sender, Window.KeyEventArgs e)
	{
		if (e.Key.State == Key.StateType.Down &&
			(e.Key.KeyPressedName == "XF86Back" || e.Key.KeyPressedName == "Escape"))
		{
			gpuPage?.StopAnimation();
			Exit();
		}
	}
}
