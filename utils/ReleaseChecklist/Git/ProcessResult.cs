namespace ReleaseChecklist.Git;

/// <summary>Contains captured output from a child process.</summary>
/// <param name="ExitCode">The process exit code.</param>
/// <param name="StandardOutput">The captured standard output.</param>
/// <param name="StandardError">The captured standard error.</param>
public sealed record ProcessResult(
	int ExitCode,
	string StandardOutput,
	string StandardError);
