using System.Diagnostics;

namespace SkiaSharp.Tests.Shared;

public static class ImageCapture
{
	public static async Task<T> RetryAsync<T>(
		Func<Task<T>> capture,
		TimeSpan limit,
		Func<T, bool, bool> compare,
		CancellationToken cancellationToken)
	{
		if (capture == null) throw new ArgumentNullException(nameof(capture));
		if (compare == null) throw new ArgumentNullException(nameof(compare));
		if (limit <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(limit));

		var clock = Stopwatch.StartNew();
		var hasActual = false;
		T actual = default!;

		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var remaining = limit - clock.Elapsed;
			if (remaining <= TimeSpan.Zero)
			{
				if (!hasActual)
					throw new TimeoutException("The image capture deadline expired before capture completed.");
				compare(actual, true);
				return actual;
			}

			actual = await WaitAsync(capture(), remaining, cancellationToken);
			hasActual = true;
			if (compare(actual, false))
				return actual;

			remaining = limit - clock.Elapsed;
			if (remaining > TimeSpan.Zero)
				await Task.Delay(
					remaining < TimeSpan.FromMilliseconds(100) ? remaining : TimeSpan.FromMilliseconds(100),
					cancellationToken);
		}
	}

	private static async Task<T> WaitAsync<T>(Task<T> task, TimeSpan timeout, CancellationToken cancellationToken)
	{
		using var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		var delay = Task.Delay(timeout, timeoutCancellation.Token);
		if (await Task.WhenAny(task, delay) != task)
		{
			cancellationToken.ThrowIfCancellationRequested();
			throw new TimeoutException("The image capture deadline expired.");
		}

		timeoutCancellation.Cancel();
		return await task;
	}
}
