using System.Runtime.ExceptionServices;

namespace SkiaSharp.Tests.Samples.Utils;

internal static class FailureHandling
{
	internal static async Task Attempt(List<Exception> failures, Func<Task> action)
	{
		try
		{
			await action();
		}
		catch (Exception error)
		{
			failures.Add(error);
		}
	}

	internal static void ThrowFailures(List<Exception> failures)
	{
		if (failures.Count == 1)
		{
			ExceptionDispatchInfo.Capture(failures[0]).Throw();
		}

		if (failures.Count > 1)
		{
			throw new AggregateException("Multiple operations failed; all failures were retained.", failures);
		}
	}
}
