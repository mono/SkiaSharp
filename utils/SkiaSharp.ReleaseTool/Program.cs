using System.CommandLine;

namespace SkiaSharp.ReleaseTool
{
	/// <summary>
	/// Composition root for <c>skiasharp-release</c>. Slice 0 of the
	/// Python -&gt; C# release CLI migration wires up only the
	/// foundation (errors, process runner, Git repository, release
	/// grammars, and JSON plan artifacts) -- no runtime command is
	/// registered yet, so this root currently only exposes <c>--help</c>
	/// (and the framework-provided <c>--version</c>).
	/// scripts/infra/release/release.py remains the production release
	/// CLI until a later slice ports its commands here.
	/// </summary>
	public static class Program
	{
		public static int Main(string[] args)
		{
			var root = new RootCommand("SkiaSharp release automation CLI (foundation only; no commands yet)");
			return root.Parse(args).Invoke();
		}
	}
}
