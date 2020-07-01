namespace SkiaSharp.Tests
{
	public class Program
	{
		public static int Main()
		{
			var testRunner = new ThreadlessXunitTestRunner();
			var result = testRunner.Run(typeof(Program).Assembly.GetName().Name + ".dll", null);
			return result ? 1 : 0;
		}
	}
}
