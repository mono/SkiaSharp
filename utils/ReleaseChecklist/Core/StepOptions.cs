namespace ReleaseChecklist.Core;

/// <summary>Configures a checklist step.</summary>
/// <param name="Id">The stable identifier used in reports.</param>
/// <param name="Title">The human-readable title.</param>
public sealed record StepOptions(
	string Id,
	string Title)
{
	/// <summary>Gets the desired-state check.</summary>
	/// <value>The check, or <see langword="null" /> for an action-only step that runs on every apply.</value>
	public IChecklistCheck? Check { get; init; }

	/// <summary>Gets the optional action that satisfies the check.</summary>
	/// <value>The action, or <see langword="null" /> for a read-only step.</value>
	public IChecklistAction? Action { get; init; }

	/// <summary>Gets the optional applicability condition.</summary>
	/// <value>The condition, or <see langword="null" /> to always run the step.</value>
	public IChecklistCondition? When { get; init; }
}
