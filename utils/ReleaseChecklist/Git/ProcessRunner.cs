using System.Diagnostics;

namespace ReleaseChecklist.Git;

/// <summary>Runs noninteractive child processes with captured output and cancellation.</summary>
public sealed class ProcessRunner
{
	/// <summary>Asynchronously runs a child process.</summary>
	/// <param name="fileName">The executable file name.</param>
	/// <param name="arguments">The ordered argument list.</param>
	/// <param name="workingDirectory">The process working directory.</param>
	/// <param name="checkExitCode"><see langword="true" /> to throw when the exit code is nonzero; otherwise, <see langword="false" />.</param>
	/// <param name="cancellationToken">A token that cancels and terminates the process.</param>
	/// <returns>The captured process result.</returns>
	/// <exception cref="ProcessException">The process exits with a nonzero code and <paramref name="checkExitCode" /> is <see langword="true" />.</exception>
	public async Task<ProcessResult> RunAsync(
		string fileName,
		IReadOnlyList<string> arguments,
		string workingDirectory,
		bool checkExitCode = true,
		CancellationToken cancellationToken = default)
	{
		var start = new ProcessStartInfo(fileName)
		{
			WorkingDirectory = workingDirectory,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true,
		};
		foreach (var argument in arguments)
			start.ArgumentList.Add(argument);

		using var process = new Process { StartInfo = start };
		if (!process.Start())
			throw new InvalidOperationException($"Unable to start process '{fileName}'.");
		var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
		var stderr = process.StandardError.ReadToEndAsync(cancellationToken);
		try
		{
			await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
		}
		catch
		{
			if (!process.HasExited)
				process.Kill(entireProcessTree: true);
			throw;
		}

		var result = new ProcessResult(
			process.ExitCode,
			await stdout.ConfigureAwait(false),
			await stderr.ConfigureAwait(false));
		if (checkExitCode && result.ExitCode != 0)
		{
			throw new ProcessException(
				$"{fileName} exited with code {result.ExitCode}: " +
				FirstNonempty(result.StandardError, result.StandardOutput));
		}
		return result;
	}

	private static string FirstNonempty(params string[] values) =>
		values.FirstOrDefault(static value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? "no output";
}
