using SkiaSharp.ReleaseTool.Processes;

namespace SkiaSharp.ReleaseTool.Tests.Processes
{
	internal sealed class RecordingProcessRunner : IProcessRunner
	{
		private readonly Queue<Func<RecordedInvocation, ProcessRunResult>> responses = new();

		public List<RecordedInvocation> Invocations { get; } = [];

		public void Enqueue(ProcessRunResult result) => responses.Enqueue(_ => result);

		public void Enqueue(Func<RecordedInvocation, ProcessRunResult> handler) => responses.Enqueue(handler);

		public Task<ProcessRunResult> RunAsync(
			IReadOnlyList<string> arguments,
			string workingDirectory,
			bool checkExitCode = true,
			TimeSpan? timeout = null,
			string? standardInput = null,
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var invocation = new RecordedInvocation(
				[.. arguments], workingDirectory, checkExitCode, timeout, standardInput);
			Invocations.Add(invocation);
			if (responses.Count == 0)
			{
				throw new InvalidOperationException(
					$"RecordingProcessRunner has no response for: {string.Join(' ', arguments)}");
			}

			var result = responses.Dequeue()(invocation);
			ProcessRunner.EnsureSuccess(result, checkExitCode);
			return Task.FromResult(result);
		}
	}

	internal sealed record RecordedInvocation(
		IReadOnlyList<string> Arguments,
		string WorkingDirectory,
		bool CheckExitCode,
		TimeSpan? Timeout,
		string? StandardInput);
}
