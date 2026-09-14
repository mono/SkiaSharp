namespace SkiaSharpSample;

public partial class App : Application
{
	/// <summary>The page the sample starts on (mirrors the native MAUI sample).</summary>
	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new MainPage()) { Title = "SkiaSharpSample" };
	}
}
