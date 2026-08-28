namespace SkiaSharp.ReleaseTool.Processes
{
	public interface IProcessRunner
	{
		Task<ProcessRunResult> RunAsync(
			IReadOnlyList<string> arguments,
			string workingDirectory,
			bool checkExitCode = true,
			TimeSpan? timeout = null,
			string? standardInput = null,
			CancellationToken cancellationToken = default);
	}
}
