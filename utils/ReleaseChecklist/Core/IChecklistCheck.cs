namespace ReleaseChecklist.Core;

/// <summary>Reads authoritative state and compares it with a desired state.</summary>
public interface IChecklistCheck
{
	/// <summary>Asynchronously evaluates the desired state.</summary>
	/// <param name="cancellationToken">A token that cancels the evaluation.</param>
	/// <returns>The observed status, detail, and state fields.</returns>
	ValueTask<CheckResult> EvaluateAsync(
		CancellationToken cancellationToken);
}
