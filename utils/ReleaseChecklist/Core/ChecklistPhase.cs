namespace ReleaseChecklist.Core;

/// <summary>Identifies a recorded phase of node execution.</summary>
public enum ChecklistPhase
{
	/// <summary>The node condition was evaluated.</summary>
	Condition,
	/// <summary>The desired state was checked.</summary>
	Precheck,
	/// <summary>The desired state was checked again immediately before an action.</summary>
	PreMutation,
	/// <summary>The action was attempted.</summary>
	Action,
	/// <summary>The desired state was checked after an action.</summary>
	Postcheck,
}
