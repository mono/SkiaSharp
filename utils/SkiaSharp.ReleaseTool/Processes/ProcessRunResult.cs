namespace SkiaSharp.ReleaseTool.Processes
{
	/// <summary>The captured result of one argv invocation.</summary>
	public sealed record ProcessRunResult(
		IReadOnlyList<string> Arguments,
		int ExitCode,
		string StandardOutput,
		string StandardError)
	{
		public bool Success => ExitCode == 0;
	}
}
