namespace ReleaseChecklist.Core;

/// <summary>Contains the structured output from one checklist run.</summary>
/// <param name="Root">The root node result.</param>
public sealed record ChecklistReport(NodeResult Root)
{
	/// <summary>Gets a value indicating whether the run completed without errors, cancellation, or pending state.</summary>
	/// <value><see langword="true" /> if the root is done or skipped and no errors occurred; otherwise, <see langword="false" />.</value>
	public bool Successful =>
		!Root.HasErrors &&
		!Root.CancellationObserved &&
		Root.Status is ChecklistStatus.Done or ChecklistStatus.Skipped;
}
