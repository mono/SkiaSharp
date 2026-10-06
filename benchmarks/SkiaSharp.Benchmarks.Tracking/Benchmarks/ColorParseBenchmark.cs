using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks.Tracking;

// Parsing hex colour strings via the public SKColor.TryParse. Pure managed code,
// so its allocation number is a clean, machine-independent regression signal.
public class ColorParseBenchmark
{
	// Only hex forms, which SKColor.TryParse supports on every platform.
	private static readonly string[] Inputs =
	{
		"#F00", "#FF0000", "#80FF0000", "#123456", "#ABCDEF12", "#0a0b0c", "#fff", "#00000000",
	};

	[Params(1000)]
	public int Iterations { get; set; }

	[Benchmark]
	public int Parse()
	{
		var ok = 0;
		for (var i = 0; i < Iterations; i++)
		{
			foreach (var s in Inputs)
			{
				if (SKColor.TryParse(s, out _))
					ok++;
			}
		}
		return ok;
	}
}
