namespace ReleaseChecklist.Core;

/// <summary>Represents one desired-state check and its optional corrective action.</summary>
public sealed class Step : ChecklistNode
{
	internal Step(
		string id,
		string title,
		IChecklistCondition? condition,
		IChecklistCheck? check,
		IChecklistAction? action)
		: base(id, title, condition)
	{
		Check = check;
		Action = action;
		DesiredState = check is null ? null : new ConditionAwareCheck(this);
	}

	/// <summary>Gets the desired-state check.</summary>
	/// <value>The check run before and after an action, or <see langword="null" /> for an action-only step.</value>
	public IChecklistCheck? Check { get; }

	/// <summary>Gets the optional corrective action.</summary>
	/// <value>The action, or <see langword="null" /> for a read-only step.</value>
	public IChecklistAction? Action { get; }

	/// <summary>Gets a check that combines this step's condition and desired-state check.</summary>
	/// <value>A condition-aware check suitable for explicit joins, or <see langword="null" /> for an action-only step.</value>
	public IChecklistCheck? DesiredState { get; }

	private sealed class ConditionAwareCheck(Step step) : IChecklistCheck
	{
		public async ValueTask<CheckResult> EvaluateAsync(CancellationToken cancellationToken)
		{
			if (!await step.Condition.EvaluateAsync(cancellationToken).ConfigureAwait(false))
				return CheckResult.Skipped($"Condition for '{step.Id}' is false.");
			return await step.Check!.EvaluateAsync(cancellationToken).ConfigureAwait(false);
		}
	}
}
