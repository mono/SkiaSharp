namespace ReleaseChecklist.Core;

/// <summary>Contains the structured result for one checklist node.</summary>
public sealed class NodeResult
{
	internal NodeResult(ChecklistNode node)
	{
		Id = node.Id;
		Title = node.Title;
		Kind = node.GetType().Name;
	}

	/// <summary>Gets the node identifier.</summary>
	/// <value>The stable identifier.</value>
	public string Id { get; }

	/// <summary>Gets the node title.</summary>
	/// <value>The human-readable title.</value>
	public string Title { get; }

	/// <summary>Gets the node kind.</summary>
	/// <value>The runtime node type name.</value>
	public string Kind { get; }

	/// <summary>Gets a value indicating whether traversal reached the node.</summary>
	/// <value><see langword="true" /> if the node was reached; otherwise, <see langword="false" />.</value>
	public bool Reached { get; internal set; } = true;

	/// <summary>Gets the aggregate desired-state status.</summary>
	/// <value>The status, or <see langword="null" /> if no check completed.</value>
	public ChecklistStatus? Status { get; internal set; }

	/// <summary>Gets the human-readable result detail.</summary>
	/// <value>The detail, or <see langword="null" /> if none was recorded.</value>
	public string? Detail { get; internal set; }

	/// <summary>Gets the reason traversal did not reach the node.</summary>
	/// <value>The reason, or <see langword="null" /> if the node was reached.</value>
	public string? NotReachedReason { get; internal set; }

	/// <summary>Gets the ordered phase records.</summary>
	/// <value>The phase records.</value>
	public List<NodePhaseRecord> Phases { get; } = [];

	/// <summary>Gets unexpected errors recorded for this node.</summary>
	/// <value>The execution errors.</value>
	public List<ExecutionError> Errors { get; } = [];

	/// <summary>Gets results for child nodes.</summary>
	/// <value>The child results in declaration order.</value>
	public List<NodeResult> Children { get; } = [];

	/// <summary>Gets a value indicating whether a pending action exists.</summary>
	/// <value><see langword="true" /> if an action can address a not-done state; otherwise, <see langword="false" />.</value>
	public bool ActionAvailable { get; internal set; }

	/// <summary>Gets a value indicating whether the runner attempted the action.</summary>
	/// <value><see langword="true" /> if the action was started; otherwise, <see langword="false" />.</value>
	public bool ActionAttempted { get; internal set; }

	/// <summary>Gets a value indicating whether the action returned successfully.</summary>
	/// <value><see langword="true" /> if the action completed without throwing; otherwise, <see langword="false" />.</value>
	public bool ActionCompleted { get; internal set; }

	/// <summary>Gets a value indicating whether caller cancellation was observed.</summary>
	/// <value><see langword="true" /> if cancellation was observed; otherwise, <see langword="false" />.</value>
	public bool CancellationObserved { get; internal set; }

	/// <summary>Gets a value indicating whether this node or a descendant recorded an error.</summary>
	/// <value><see langword="true" /> if an error was recorded; otherwise, <see langword="false" />.</value>
	public bool HasErrors =>
		Errors.Count > 0 || Children.Any(static child => child.HasErrors);
}
