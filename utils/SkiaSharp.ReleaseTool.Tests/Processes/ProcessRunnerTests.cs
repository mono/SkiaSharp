using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Processes;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Processes
{
	public class ProcessRunnerTests
	{
		// `sleep` on Windows requires a TTY (or errors on redirected
		// stdin); `ping` is the well-known argv-only cross-platform
		// stand-in that never needs a shell.
		private static string[] SleepCommand(int seconds) => OperatingSystem.IsWindows()
			? ["ping", "127.0.0.1", "-n", (seconds + 1).ToString()]
			: ["sleep", seconds.ToString()];

		[Fact]
		public void Run_captures_stdout_and_succeeds()
		{
			var runner = new ProcessRunner();

			var result = runner.Run(["git", "--version"], Environment.CurrentDirectory, cancellationToken: TestContext.Current.CancellationToken);

			Assert.True(result.Success);
			Assert.Equal(0, result.ExitCode);
			Assert.Contains("git version", result.StandardOutput);
		}

		[Fact]
		public void Run_with_checkExitCode_false_returns_failed_result_without_throwing()
		{
			var runner = new ProcessRunner();

			var result = runner.Run(
				["git", "not-a-real-git-subcommand"], Environment.CurrentDirectory,
				checkExitCode: false, cancellationToken: TestContext.Current.CancellationToken);

			Assert.False(result.Success);
			Assert.NotEqual(0, result.ExitCode);
		}

		[Fact]
		public void Run_with_checkExitCode_true_raises_ReleaseToolException_on_nonzero_exit()
		{
			var runner = new ProcessRunner();

			var ex = Assert.Throws<ReleaseToolException>(
				() => runner.Run(
					["git", "not-a-real-git-subcommand"], Environment.CurrentDirectory,
					cancellationToken: TestContext.Current.CancellationToken));

			Assert.Contains("command failed", ex.Message);
			Assert.Contains("not-a-real-git-subcommand", ex.Message);
		}

		[Fact]
		public void Run_raises_clean_ReleaseToolException_on_timeout()
		{
			var runner = new ProcessRunner();

			var ex = Assert.Throws<ReleaseToolException>(
				() => runner.Run(
					SleepCommand(30), Environment.CurrentDirectory, timeout: TimeSpan.FromSeconds(1),
					cancellationToken: TestContext.Current.CancellationToken));

			Assert.Matches("timed out after 1s", ex.Message);
			// Must never leak (or wrap as) a raw framework exception type --
			// exactly one exception type, ReleaseToolException, regardless
			// of whether the command failed or timed out.
			Assert.IsType<ReleaseToolException>(ex, exactMatch: true);
		}

		[Fact]
		public void Run_honors_cancellation_distinctly_from_timeout()
		{
			var runner = new ProcessRunner();
			using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
			cts.CancelAfter(TimeSpan.FromMilliseconds(200));

			Assert.Throws<OperationCanceledException>(
				() => runner.Run(SleepCommand(30), Environment.CurrentDirectory, cancellationToken: cts.Token));
		}

		[Fact]
		public void Run_throws_ArgumentException_for_empty_arguments()
		{
			var runner = new ProcessRunner();

			Assert.Throws<ArgumentException>(
				() => runner.Run([], Environment.CurrentDirectory, cancellationToken: TestContext.Current.CancellationToken));
		}

		[Fact]
		public void RecordingProcessRunner_records_invocation_and_replays_response()
		{
			var runner = new RecordingProcessRunner();
			runner.Enqueue(new ProcessRunResult(["git", "status"], 0, "clean\n", ""));

			var result = runner.Run(
				["git", "status"], "/repo", timeout: TimeSpan.FromSeconds(5),
				cancellationToken: TestContext.Current.CancellationToken);

			Assert.Equal("clean\n", result.StandardOutput);
			var invocation = Assert.Single(runner.Invocations);
			Assert.Equal(["git", "status"], invocation.Arguments);
			Assert.Equal("/repo", invocation.WorkingDirectory);
			Assert.Equal(TimeSpan.FromSeconds(5), invocation.Timeout);
		}

		[Fact]
		public void RecordingProcessRunner_throws_when_no_response_queued()
		{
			var runner = new RecordingProcessRunner();

			Assert.Throws<InvalidOperationException>(
				() => runner.Run(["git", "status"], "/repo", cancellationToken: TestContext.Current.CancellationToken));
		}
	}
}
