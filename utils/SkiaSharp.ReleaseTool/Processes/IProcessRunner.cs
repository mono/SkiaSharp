namespace SkiaSharp.ReleaseTool.Processes
{
	/// <summary>
	/// Runs argv lists directly -- never a shell string. Mirrors Python's
	/// <c>release_common.CommandRunner</c>: kept as a small injectable
	/// interface so tests can substitute a recording/fake runner when they
	/// need to observe or fail a specific invocation, while the real CLI
	/// always uses <see cref="ProcessRunner"/>.
	/// </summary>
	public interface IProcessRunner
	{
		/// <summary>
		/// Runs <paramref name="arguments"/> (the executable followed by
		/// its argv) in <paramref name="workingDirectory"/>.
		/// </summary>
		/// <param name="checkExitCode">
		/// When <see langword="true"/> (the default), a non-zero exit
		/// code raises <see cref="Errors.ReleaseToolException"/> instead
		/// of being returned for the caller to inspect.
		/// </param>
		/// <param name="timeout">
		/// Wall-clock budget for the whole invocation. Defaults to
		/// <see cref="ProcessRunner.DefaultTimeout"/>. Exceeding it kills
		/// the process (tree) and raises
		/// <see cref="Errors.ReleaseToolException"/> -- never a raw
		/// <see cref="TimeoutException"/> or platform exception.
		/// </param>
		/// <param name="standardInput">
		/// When given, written to the process's standard input and then
		/// closed before waiting for exit.
		/// </param>
		/// <param name="cancellationToken">
		/// Cooperative cancellation distinct from <paramref name="timeout"/>:
		/// cancelling this token also kills the process (tree), but
		/// surfaces as <see cref="OperationCanceledException"/> rather
		/// than a tooling error, since the caller asked for it.
		/// </param>
		ProcessRunResult Run(
			IReadOnlyList<string> arguments,
			string workingDirectory,
			bool checkExitCode = true,
			TimeSpan? timeout = null,
			string? standardInput = null,
			CancellationToken cancellationToken = default);
	}
}
