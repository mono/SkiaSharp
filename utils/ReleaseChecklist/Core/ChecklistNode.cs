namespace ReleaseChecklist.Core;

/// <summary>Provides identity and applicability metadata shared by checklist nodes.</summary>
public abstract class ChecklistNode
{
	/// <summary>Called from constructors in derived classes to initialize the <see cref="ChecklistNode" /> class.</summary>
	/// <param name="id">The stable identifier used in reports.</param>
	/// <param name="title">The human-readable title.</param>
	/// <param name="condition">The applicability condition, or <see langword="null" /> to always apply.</param>
	protected ChecklistNode(
		string id,
		string title,
		IChecklistCondition? condition)
	{
		Id = id;
		Title = title;
		Condition = condition ?? Core.Condition.Always;
	}

	/// <summary>Gets the stable identifier used in reports.</summary>
	/// <value>The stable identifier.</value>
	public string Id { get; }

	/// <summary>Gets the human-readable title.</summary>
	/// <value>The title.</value>
	public string Title { get; }

	/// <summary>Gets the condition that determines whether the node applies.</summary>
	/// <value>The applicability condition.</value>
	public IChecklistCondition Condition { get; }
}
