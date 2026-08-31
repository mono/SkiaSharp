namespace ReleaseChecklist.Core;

/// <summary>Provides an ordered collection of child nodes for a structural checklist node.</summary>
public abstract class ChecklistContainer : ChecklistNode
{
	/// <summary>Called from constructors in derived classes to initialize the <see cref="ChecklistContainer" /> class.</summary>
	/// <param name="id">The stable identifier used in reports.</param>
	/// <param name="title">The human-readable title.</param>
	/// <param name="condition">The applicability condition, or <see langword="null" /> to always apply.</param>
	/// <param name="children">The child nodes.</param>
	protected ChecklistContainer(
		string id,
		string title,
		IChecklistCondition? condition,
		IReadOnlyList<ChecklistNode> children)
		: base(id, title, condition)
	{
		Children = children;
	}

	/// <summary>Gets the child nodes.</summary>
	/// <value>The child nodes in declaration order.</value>
	public IReadOnlyList<ChecklistNode> Children { get; }
}
