namespace ReleaseChecklist.Core;

/// <summary>Creates applicability conditions for checklist nodes.</summary>
public static class Condition
{
	/// <summary>Gets a condition that always applies.</summary>
	/// <value>The always-true condition.</value>
	public static IChecklistCondition Always { get; } = new DelegateCondition(
		static _ => ValueTask.FromResult(true));

	/// <summary>Creates a synchronous condition.</summary>
	/// <param name="condition">The callback that determines whether the node applies.</param>
	/// <returns>A checklist condition that invokes the callback.</returns>
	public static IChecklistCondition From(Func<bool> condition) =>
		new DelegateCondition(_ => ValueTask.FromResult(condition()));

	/// <summary>Creates a synchronous condition that receives a cancellation token.</summary>
	/// <param name="condition">The callback that determines whether the node applies.</param>
	/// <returns>A checklist condition that invokes the callback.</returns>
	public static IChecklistCondition From(Func<CancellationToken, bool> condition) =>
		new DelegateCondition(token => ValueTask.FromResult(condition(token)));

	/// <summary>Creates an asynchronous condition.</summary>
	/// <param name="condition">The callback that determines whether the node applies.</param>
	/// <returns>A checklist condition that invokes the callback.</returns>
	public static IChecklistCondition From(
		Func<CancellationToken, ValueTask<bool>> condition) =>
		new DelegateCondition(condition);

	private sealed class DelegateCondition(
		Func<CancellationToken, ValueTask<bool>> condition) : IChecklistCondition
	{
		public ValueTask<bool> EvaluateAsync(CancellationToken cancellationToken) =>
			condition(cancellationToken);
	}
}
