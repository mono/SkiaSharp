using System.Diagnostics;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Processes;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Processes
{
	public sealed class ProcessRunnerTests
	{
		private static string[] SleepCommand(int seconds) => OperatingSystem.IsWindows()
			? ["ping", "127.0.0.1", "-n", (seconds + 1).ToString()]
			: ["sleep", seconds.ToString()];

		[Fact]
		public async Task Captures_machine_output()
		{
			var result = await new ProcessRunner().RunAsync(
				["git", "--version"],
				Environment.CurrentDirectory,
				cancellationToken: TestContext.Current.CancellationToken);

			Assert.True(result.Success);
			Assert.Contains("git version", result.StandardOutput);
		}

		[Fact]
		public async Task Writes_standard_input_asynchronously()
		{
			var result = await new ProcessRunner().RunAsync(
				["git", "hash-object", "--stdin"],
				Environment.CurrentDirectory,
				standardInput: "release input\n",
				cancellationToken: TestContext.Current.CancellationToken);

			Assert.Equal(40, result.StandardOutput.Trim().Length);
		}

		[Fact]
		public async Task Maps_nonzero_exit_to_release_tool_exception()
		{
			var runner = new ProcessRunner();

			var uncheckedResult = await runner.RunAsync(
				["git", "not-a-real-git-subcommand"],
				Environment.CurrentDirectory,
				checkExitCode: false,
				cancellationToken: TestContext.Current.CancellationToken);
			Assert.False(uncheckedResult.Success);

			var exception = await Assert.ThrowsAsync<ReleaseToolException>(
				() => runner.RunAsync(
					["git", "not-a-real-git-subcommand"],
					Environment.CurrentDirectory,
					cancellationToken: TestContext.Current.CancellationToken));
			Assert.Contains("command failed", exception.Message);
		}

		[Fact]
		public async Task Timeout_covers_process_and_stream_draining()
		{
			var stopwatch = Stopwatch.StartNew();
			var exception = await Assert.ThrowsAsync<ReleaseToolException>(
				() => new ProcessRunner().RunAsync(
					SleepCommand(30),
					Environment.CurrentDirectory,
					timeout: TimeSpan.FromMilliseconds(250),
					cancellationToken: TestContext.Current.CancellationToken));

			Assert.Contains("timed out", exception.Message);
			Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5));
		}

		[Fact]
		public async Task Cancellation_kills_the_process_tree_and_remains_cancellation()
		{
			using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(
				TestContext.Current.CancellationToken);
			cancellation.CancelAfter(TimeSpan.FromMilliseconds(200));

			await Assert.ThrowsAnyAsync<OperationCanceledException>(
				() => new ProcessRunner().RunAsync(
					SleepCommand(30),
					Environment.CurrentDirectory,
					cancellationToken: cancellation.Token));
		}

		[Fact]
		public async Task Large_input_to_a_non_reader_is_bounded_by_timeout()
		{
			var input = new string('x', 4 * 1024 * 1024);
			var stopwatch = Stopwatch.StartNew();

			await Assert.ThrowsAsync<ReleaseToolException>(
				() => new ProcessRunner().RunAsync(
					SleepCommand(30),
					Environment.CurrentDirectory,
					timeout: TimeSpan.FromMilliseconds(300),
					standardInput: input,
					cancellationToken: TestContext.Current.CancellationToken));

			Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5));
		}

		[Fact]
		public async Task Successful_child_exit_wins_over_broken_stdin_pipe()
		{
			if (OperatingSystem.IsWindows())
				return;

			var input = new string('x', 4 * 1024 * 1024);
			var result = await new ProcessRunner().RunAsync(
				["/usr/bin/true"],
				Environment.CurrentDirectory,
				standardInput: input,
				cancellationToken: TestContext.Current.CancellationToken);

			Assert.True(result.Success);
		}

		[Fact]
		public async Task Partial_read_child_exit_wins_over_broken_stdin_pipe()
		{
			if (OperatingSystem.IsWindows())
				return;

			var input = new string('x', 4 * 1024 * 1024);
			var result = await new ProcessRunner().RunAsync(
				["/bin/sh", "-c", "dd bs=1 count=1 of=/dev/null 2>/dev/null"],
				Environment.CurrentDirectory,
				standardInput: input,
				cancellationToken: TestContext.Current.CancellationToken);

			Assert.True(result.Success);
		}

		[Fact]
		public async Task Cancellation_bounds_a_blocked_stdin_write()
		{
			using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(
				TestContext.Current.CancellationToken);
			cancellation.CancelAfter(TimeSpan.FromMilliseconds(200));

			await Assert.ThrowsAnyAsync<OperationCanceledException>(
				() => new ProcessRunner().RunAsync(
					SleepCommand(30),
					Environment.CurrentDirectory,
					standardInput: new string('x', 4 * 1024 * 1024),
					cancellationToken: cancellation.Token));
		}

		[Fact]
		public async Task Recording_runner_honors_check_exit_code()
		{
			var runner = new RecordingProcessRunner();
			var failure = new ProcessRunResult(["git", "status"], 128, "", "fatal");
			runner.Enqueue(failure);
			await Assert.ThrowsAsync<ReleaseToolException>(
				() => runner.RunAsync(
					["git", "status"],
					"/repo",
					cancellationToken: TestContext.Current.CancellationToken));

			runner.Enqueue(failure);
			var result = await runner.RunAsync(
				["git", "status"],
				"/repo",
				checkExitCode: false,
				cancellationToken: TestContext.Current.CancellationToken);
			Assert.Equal(128, result.ExitCode);
			Assert.False(Assert.Single(runner.Invocations.Skip(1)).CheckExitCode);
		}

		[Fact]
		public async Task Rejects_empty_arguments_and_nonpositive_timeout()
		{
			var runner = new ProcessRunner();
			await Assert.ThrowsAsync<ArgumentException>(
				() => runner.RunAsync(
					[],
					Environment.CurrentDirectory,
					cancellationToken: TestContext.Current.CancellationToken));
			await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
				() => runner.RunAsync(
					["git", "--version"],
					Environment.CurrentDirectory,
					timeout: TimeSpan.Zero,
					cancellationToken: TestContext.Current.CancellationToken));
		}
	}
}
