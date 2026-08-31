namespace ReleaseChecklist.Core;

/// <summary>Builds child nodes within a sequence or parallel container.</summary>
public interface IChecklistChildren
{
	/// <summary>Adds a desired-state step.</summary>
	/// <param name="options">The step configuration.</param>
	/// <returns>The added step.</returns>
	Step Step(StepOptions options);

	/// <summary>Adds an ordered child sequence.</summary>
	/// <param name="id">The stable identifier used in reports.</param>
	/// <param name="title">The human-readable title.</param>
	/// <param name="children">A callback that declares the child nodes.</param>
	/// <param name="when">The applicability condition, or <see langword="null" /> to always run.</param>
	/// <returns>The added sequence.</returns>
	Sequence Sequence(
		string id,
		string title,
		Action<IChecklistChildren> children,
		IChecklistCondition? when = null);

	/// <summary>Adds a concurrent set of independent child branches.</summary>
	/// <param name="id">The stable identifier used in reports.</param>
	/// <param name="title">The human-readable title.</param>
	/// <param name="children">A callback that declares the child branches.</param>
	/// <param name="when">The applicability condition, or <see langword="null" /> to always run.</param>
	/// <returns>The added parallel container.</returns>
	Parallel Parallel(
		string id,
		string title,
		Action<IChecklistChildren> children,
		IChecklistCondition? when = null);
}
