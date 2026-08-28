using System.CommandLine;

namespace SkiaSharp.ReleaseTool
{
	/// <summary>Composition root for the release-tooling foundation.</summary>
	public static class Program
	{
		public static int Main(string[] args)
		{
			var root = new RootCommand("SkiaSharp release automation CLI");
			return root.Parse(args).Invoke();
		}
	}
}
