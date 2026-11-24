using System.Reflection;
using BenchmarkDotNet.Running;

namespace SkiaSharp.Benchmarks;

public class Program
{
	public static void Main(string[] args)
	{
		// Use the switcher so a specific benchmark can be selected via args, e.g.:
		//   dotnet run -c Release -- --filter *SurfaceCanvasBenchmark*
		BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);
	}
}
