namespace ReleaseChecklist.Core;

/// <summary>Determines whether a checklist node applies to the current run.</summary>
public interface IChecklistCondition
{
	/// <summary>Asynchronously evaluates the condition.</summary>
	/// <param name="cancellationToken">A token that cancels the evaluation.</param>
	/// <returns><see langword="true" /> when the node applies; otherwise, <see langword="false" />.</returns>
	ValueTask<bool> EvaluateAsync(
		CancellationToken cancellationToken);
}
