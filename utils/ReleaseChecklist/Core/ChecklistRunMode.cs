namespace ReleaseChecklist.Core;

/// <summary>Specifies whether a checklist reports or applies pending actions.</summary>
public enum ChecklistRunMode
{
	/// <summary>Checks state and reports pending actions without running them.</summary>
	DryRun,
	/// <summary>Checks state and runs actions needed to reach the desired state.</summary>
	Apply,
}
