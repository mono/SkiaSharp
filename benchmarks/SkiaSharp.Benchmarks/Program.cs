using BenchmarkDotNet.Running;

namespace SkiaSharp.Benchmarks;

public class Program
{
	public static void Main(string[] args)
	{
		var summary = BenchmarkRunner.Run<SKData_Direct_Delegates>();
	}
}
