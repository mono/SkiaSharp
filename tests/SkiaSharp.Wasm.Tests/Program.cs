using System;
using System.Threading.Tasks;

namespace SkiaSharp.Tests
{
	public class Program
	{
		public static async Task<int> Main(string[] args)
		{
			Console.WriteLine($"args: {string.Join(" ", args)}");

			var excludedTraits = new[] {
				$"{BaseTest.CategoryKey}={BaseTest.GpuCategory}"
			};

			var testRunner = new ThreadlessXunitTestRunner();
			var result = testRunner.Run(typeof(Program).Assembly.GetName().Name + ".dll", excludedTraits);
			return result ? 1 : 0;
		}
	}
}
