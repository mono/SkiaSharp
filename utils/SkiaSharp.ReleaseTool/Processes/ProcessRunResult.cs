namespace SkiaSharp.ReleaseTool.Processes
{
	/// <summary>
	/// The captured result of one argv invocation. Mirrors Python's
	/// <c>release_common.CommandResult</c>.
	/// </summary>
	public sealed record ProcessRunResult(
		IReadOnlyList<string> Arguments,
		int ExitCode,
		string StandardOutput,
		string StandardError)
	{
		public bool Success => ExitCode == 0;
	}
}
