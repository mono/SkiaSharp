namespace ReleaseChecklist.Core;

/// <summary>Specifies how closely a checklist node matches its desired state.</summary>
public enum ChecklistStatus
{
	/// <summary>The desired state is satisfied.</summary>
	Done,
	/// <summary>The desired state is not satisfied, but no conflicting state exists.</summary>
	NotDone,
	/// <summary>Existing state conflicts with the desired state.</summary>
	Blocked,
	/// <summary>The node does not apply because its condition is false.</summary>
	Skipped,
}
