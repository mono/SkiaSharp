namespace ReleaseChecklist.Core;

/// <summary>Runs child nodes in declaration order and stops when a child cannot continue.</summary>
public sealed class Sequence : ChecklistContainer
{
	internal Sequence(
		string id,
		string title,
		IChecklistCondition? condition,
		IReadOnlyList<ChecklistNode> children)
		: base(id, title, condition, children)
	{
	}
}
