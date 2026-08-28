using System.Diagnostics;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Processes
{
	/// <summary>
	/// The real runner used outside of tests. Mirrors Python's
	/// <c>release_common.SubprocessCommandRunner</c>: always argv-only
	/// (<see cref="ProcessStartInfo.ArgumentList"/>, never a shell
	/// string), captures stdout/stderr, and maps both a non-zero exit and
	/// a timed-out process to <see cref="ReleaseToolException"/> --
	/// callers never need to special-case a platform exception type.
	/// </summary>
	public sealed class ProcessRunner : IProcessRunner
	{
		/// <summary>Matches Python's <c>CommandRunner.run</c> default of 120s.</summary>
		public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(120);

		private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(50);

		public ProcessRunResult Run(
			IReadOnlyList<string> arguments,
			string workingDirectory,
			bool checkExitCode = true,
			TimeSpan? timeout = null,
			string? standardInput = null,
			CancellationToken cancellationToken = default)
		{
			if (arguments.Count == 0)
				throw new ArgumentException("arguments must contain at least the executable name.", nameof(arguments));

			var effectiveTimeout = timeout ?? DefaultTimeout;
			var argv = arguments.ToArray();

			var startInfo = new ProcessStartInfo
			{
				FileName = argv[0],
				WorkingDirectory = workingDirectory,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				RedirectStandardInput = standardInput is not null,
				UseShellExecute = false,
				CreateNoWindow = true,
			};
			for (var i = 1; i < argv.Length; i++)
				startInfo.ArgumentList.Add(argv[i]);

			using var process = new Process { StartInfo = startInfo };
			process.Start();

			// Read raw text rather than line-by-line events: `Process`'s
			// line-based OutputDataReceived/ErrorDataReceived normalize
			// away whether the stream actually ended with a trailing
			// newline, which would silently corrupt e.g. `git show` output
			// that has none. Starting both reads immediately (before
			// waiting for exit) avoids the classic redirected-output
			// deadlock if the child writes more than the OS pipe buffer.
			var stdoutTask = process.StandardOutput.ReadToEndAsync();
			var stderrTask = process.StandardError.ReadToEndAsync();

			if (standardInput is not null)
			{
				process.StandardInput.Write(standardInput);
				process.StandardInput.Close();
			}

			WaitWithTimeoutAndCancellation(process, argv, effectiveTimeout, cancellationToken);
			process.WaitForExit();

			var result = new ProcessRunResult(
				argv, process.ExitCode, stdoutTask.GetAwaiter().GetResult(), stderrTask.GetAwaiter().GetResult());
			if (checkExitCode && !result.Success)
			{
				var detail = FirstNonEmpty(result.StandardError, result.StandardOutput) ?? "no output";
				throw new ReleaseToolException(
					$"command failed ({result.ExitCode}): {string.Join(' ', argv)}\n{detail}");
			}
			return result;
		}

		private static void WaitWithTimeoutAndCancellation(
			Process process, string[] argv, TimeSpan timeout, CancellationToken cancellationToken)
		{
			var stopwatch = Stopwatch.StartNew();
			while (!process.WaitForExit((int)PollInterval.TotalMilliseconds))
			{
				if (cancellationToken.IsCancellationRequested)
				{
					KillProcessTree(process);
					cancellationToken.ThrowIfCancellationRequested();
				}
				if (stopwatch.Elapsed >= timeout)
				{
					KillProcessTree(process);
					throw new ReleaseToolException(
						$"command timed out after {(int)timeout.TotalSeconds}s: {string.Join(' ', argv)}");
				}
			}
		}

		private static string? FirstNonEmpty(params string[] candidates)
		{
			foreach (var candidate in candidates)
			{
				var trimmed = candidate.Trim();
				if (trimmed.Length > 0)
					return trimmed;
			}
			return null;
		}

		private static void KillProcessTree(Process process)
		{
			try
			{
				if (!process.HasExited)
					process.Kill(entireProcessTree: true);
			}
			catch (InvalidOperationException)
			{
				// The process already exited between the check above and Kill().
			}
		}
	}
}
