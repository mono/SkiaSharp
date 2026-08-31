using ReleaseChecklist.Core;

namespace ReleaseChecklist.FileSystem;

/// <summary>Configures a convergent text-file transformation step.</summary>
public sealed record FileContentsOptions
{
	/// <summary>Gets the stable step identifier.</summary>
	/// <value>The identifier used in reports.</value>
	public required string Id { get; init; }

	/// <summary>Gets the human-readable step title.</summary>
	/// <value>The title shown in reports.</value>
	public required string Title { get; init; }

	/// <summary>Gets the file path.</summary>
	/// <value>The absolute or relative file path.</value>
	public required string Path { get; init; }

	/// <summary>Gets the deterministic text transformation.</summary>
	/// <value>A function that receives current content, or <see langword="null" /> for a missing file, and returns desired content.</value>
	public required Func<string?, string> Transform { get; init; }

	/// <summary>Gets the optional applicability condition.</summary>
	/// <value>The condition, or <see langword="null" /> to always include the step.</value>
	public IChecklistCondition? When { get; init; }
}
