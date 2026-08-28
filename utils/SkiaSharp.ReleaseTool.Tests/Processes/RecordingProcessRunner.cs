using SkiaSharp.ReleaseTool.Processes;

namespace SkiaSharp.ReleaseTool.Tests.Processes
{
	/// <summary>
	/// A recording, in-memory <see cref="IProcessRunner"/> fake: never
	/// spawns a real process. Records every invocation (for argv-shape
	/// assertions in higher-layer tests, e.g. <c>GitRepository</c>) and
	/// replays pre-programmed responses in call order.
	/// </summary>
	internal sealed class RecordingProcessRunner : IProcessRunner
	{
		private readonly Queue<Func<RecordedInvocation, ProcessRunResult>> responses = new();

		public List<RecordedInvocation> Invocations { get; } = [];

		public void Enqueue(ProcessRunResult result) => responses.Enqueue(_ => result);

		public void Enqueue(Func<RecordedInvocation, ProcessRunResult> handler) => responses.Enqueue(handler);

		public ProcessRunResult Run(
			IReadOnlyList<string> arguments,
			string workingDirectory,
			bool checkExitCode = true,
			TimeSpan? timeout = null,
			string? standardInput = null,
			CancellationToken cancellationToken = default)
		{
			var invocation = new RecordedInvocation(
				[.. arguments], workingDirectory, checkExitCode, timeout, standardInput);
			Invocations.Add(invocation);

			if (responses.Count == 0)
				throw new InvalidOperationException(
					$"RecordingProcessRunner has no more responses queued for: {string.Join(' ', arguments)}");
			return responses.Dequeue()(invocation);
		}
	}

	internal sealed record RecordedInvocation(
		IReadOnlyList<string> Arguments,
		string WorkingDirectory,
		bool CheckExitCode,
		TimeSpan? Timeout,
		string? StandardInput);
}
