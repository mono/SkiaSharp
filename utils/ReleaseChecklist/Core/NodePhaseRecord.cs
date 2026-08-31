namespace ReleaseChecklist.Core;

/// <summary>Records one phase of node execution.</summary>
/// <param name="Phase">The execution phase.</param>
/// <param name="Status">The desired-state status observed during the phase, if applicable.</param>
/// <param name="Detail">The human-readable phase detail.</param>
/// <param name="Observation">The observed state, if the phase evaluated a check.</param>
public sealed record NodePhaseRecord(
	ChecklistPhase Phase,
	ChecklistStatus? Status,
	string Detail,
	Observation? Observation = null);
