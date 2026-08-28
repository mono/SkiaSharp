using System.ComponentModel;
using System.Diagnostics;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Processes
{
	public sealed class ProcessRunner : IProcessRunner
	{
		public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(120);

		public async Task<ProcessRunResult> RunAsync(
			IReadOnlyList<string> arguments,
			string workingDirectory,
			bool checkExitCode = true,
			TimeSpan? timeout = null,
			string? standardInput = null,
			CancellationToken cancellationToken = default)
		{
			if (arguments.Count == 0)
				throw new ArgumentException("Arguments must contain an executable name.", nameof(arguments));
			cancellationToken.ThrowIfCancellationRequested();

			var effectiveTimeout = timeout ?? DefaultTimeout;
			if (effectiveTimeout <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive.");

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
			for (var index = 1; index < argv.Length; index++)
				startInfo.ArgumentList.Add(argv[index]);

			using var process = new Process { StartInfo = startInfo };
			try
			{
				process.Start();
			}
			catch (Exception ex) when (ex is InvalidOperationException or Win32Exception)
			{
				throw new ReleaseToolException($"could not start command: {FormatCommand(argv)}", ex);
			}

			using var timeoutSource = new CancellationTokenSource();
			timeoutSource.CancelAfter(effectiveTimeout);
			using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
				cancellationToken,
				timeoutSource.Token);
			var token = linkedSource.Token;

			var standardOutputTask = process.StandardOutput.ReadToEndAsync(token);
			var standardErrorTask = process.StandardError.ReadToEndAsync(token);
			var standardInputTask = WriteStandardInputAsync(process, standardInput, token);
			var exitTask = process.WaitForExitAsync(token);

			try
			{
				await Task.WhenAll(
					exitTask,
					standardInputTask,
					standardOutputTask,
					standardErrorTask).WaitAsync(token).ConfigureAwait(false);
			}
			catch (OperationCanceledException) when (token.IsCancellationRequested)
			{
				KillProcessTree(process);
				await ObserveAsync(exitTask, standardInputTask, standardOutputTask, standardErrorTask).ConfigureAwait(false);

				if (cancellationToken.IsCancellationRequested)
					throw new OperationCanceledException(cancellationToken);
				throw new ReleaseToolException(
					$"command timed out after {effectiveTimeout.TotalSeconds:g}s: {FormatCommand(argv)}");
			}
			catch (Exception ex) when (ex is IOException or ObjectDisposedException)
			{
				KillProcessTree(process);
				throw new ReleaseToolException($"command I/O failed: {FormatCommand(argv)}", ex);
			}

			var result = new ProcessRunResult(
				argv,
				process.ExitCode,
				await standardOutputTask.ConfigureAwait(false),
				await standardErrorTask.ConfigureAwait(false));
			EnsureSuccess(result, checkExitCode);
			return result;
		}

		internal static void EnsureSuccess(ProcessRunResult result, bool checkExitCode)
		{
			if (!checkExitCode || result.Success)
				return;
			var detail = FirstNonEmpty(result.StandardError, result.StandardOutput) ?? "no output";
			throw new ReleaseToolException(
				$"command failed ({result.ExitCode}): {FormatCommand(result.Arguments)}\n{detail}");
		}

		private static async Task WriteStandardInputAsync(
			Process process,
			string? standardInput,
			CancellationToken cancellationToken)
		{
			if (standardInput is null)
				return;
			try
			{
				await process.StandardInput.WriteAsync(
					standardInput.AsMemory(),
					cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				process.StandardInput.Close();
			}
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
			}
			catch (NotSupportedException)
			{
				try
				{
					if (!process.HasExited)
						process.Kill();
				}
				catch (InvalidOperationException)
				{
				}
			}
		}

		private static async Task ObserveAsync(params Task[] tasks)
		{
			foreach (var task in tasks)
			{
				try
				{
					await task.ConfigureAwait(false);
				}
				catch (Exception ex) when (
					ex is OperationCanceledException or IOException or ObjectDisposedException or InvalidOperationException)
				{
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

		private static string FormatCommand(IEnumerable<string> arguments) =>
			string.Join(' ', arguments);
	}
}
