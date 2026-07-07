namespace SkiaSharpSample;

/// <summary>
/// Shared companion to the platform entry points. Provides <see cref="DefaultPage"/> so the
/// shared <c>MainLayout</c> (used verbatim across the WebAssembly, Server and Hybrid samples)
/// can pick the page to start on. On iOS/MacCatalyst this merges with the platform
/// <c>Program</c> entry point (which is <c>partial</c>); on Android/Windows it stands alone.
/// </summary>
public partial class Program
{
	/// <summary>The page the sample starts on.</summary>
	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;
}
