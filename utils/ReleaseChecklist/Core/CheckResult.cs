namespace ReleaseChecklist.Core;

/// <summary>Describes the result of comparing authoritative state with desired state.</summary>
/// <param name="Status">The desired-state status.</param>
/// <param name="Detail">The human-readable result detail.</param>
/// <param name="Observation">The named state values used for drift detection and reporting.</param>
public sealed record CheckResult(
	ChecklistStatus Status,
	string Detail,
	Observation Observation)
{
	/// <summary>Creates a result indicating that desired state is satisfied.</summary>
	/// <param name="detail">The human-readable result detail.</param>
	/// <param name="observation">The observed state, or <see langword="null" /> for an empty observation.</param>
	/// <returns>A done result.</returns>
	public static CheckResult Done(string detail, Observation? observation = null) =>
		new(ChecklistStatus.Done, detail, observation ?? Observation.Empty);

	/// <summary>Creates a result indicating that an action or external change is still needed.</summary>
	/// <param name="detail">The human-readable result detail.</param>
	/// <param name="observation">The observed state, or <see langword="null" /> for an empty observation.</param>
	/// <returns>A not-done result.</returns>
	public static CheckResult NotDone(string detail, Observation? observation = null) =>
		new(ChecklistStatus.NotDone, detail, observation ?? Observation.Empty);

	/// <summary>Creates a result indicating that existing state conflicts with desired state.</summary>
	/// <param name="detail">The human-readable result detail.</param>
	/// <param name="observation">The observed state, or <see langword="null" /> for an empty observation.</param>
	/// <returns>A blocked result.</returns>
	public static CheckResult Blocked(string detail, Observation? observation = null) =>
		new(ChecklistStatus.Blocked, detail, observation ?? Observation.Empty);

	/// <summary>Creates a result indicating that the step does not apply.</summary>
	/// <param name="detail">The human-readable result detail.</param>
	/// <param name="observation">The observed state, or <see langword="null" /> for an empty observation.</param>
	/// <returns>A skipped result.</returns>
	public static CheckResult Skipped(string detail, Observation? observation = null) =>
		new(ChecklistStatus.Skipped, detail, observation ?? Observation.Empty);
}
