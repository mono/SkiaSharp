namespace ReleaseChecklist.Core;

/// <summary>Runs independent child branches concurrently and waits for every branch.</summary>
public sealed class Parallel : ChecklistContainer
{
	internal Parallel(
		string id,
		string title,
		IChecklistCondition? condition,
		IReadOnlyList<ChecklistNode> children)
		: base(id, title, condition, children)
	{
	}
}
