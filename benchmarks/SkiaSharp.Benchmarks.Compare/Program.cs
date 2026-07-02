using System.Reflection;
using BenchmarkDotNet.Running;

namespace SkiaSharp.Benchmarks;

public class Program
{
	public static void Main(string[] args)
	{
		BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args, BenchmarkConfig.Create());
	}
}
