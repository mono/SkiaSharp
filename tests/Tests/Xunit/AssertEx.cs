using System;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit;

public static class AssertEx
{
	public static async Task Eventually(
		Func<bool> assertion,
		int timeout = 1000,
		int interval = 100,
		string message = "Assertion timed out")
	{
		do
		{
			if (assertion())
			{
				return;
			}

			await Task.Delay(interval);

			timeout -= interval;
		}
		while (timeout >= 0);

		if (!assertion())
		{
			throw new XunitException(message);
		}
	}

	public static async Task EventuallyGC(params WeakReference[] references)
	{
		Assert.NotEmpty(references);

		bool AreReferencesCollected()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();

			foreach (var reference in references)
			{
				Assert.NotNull(reference);
				if (reference.IsAlive)
				{
					return false;
				}
			}

			return true;
		}

		try
		{
			await Eventually(AreReferencesCollected);
		}
		catch (XunitException ex)
		{
			throw new XunitException(ListLivingReferences(references), ex);
		}
	}

	private static string ListLivingReferences(WeakReference[] references)
	{
		var stringBuilder = new StringBuilder();

		foreach (var weakReference in references)
		{
			if (weakReference.IsAlive && weakReference.Target is object x)
			{
				stringBuilder.Append($"Reference to {x} (type {x.GetType()} is still alive.\n");
			}
		}

		return stringBuilder.ToString();
	}
}
