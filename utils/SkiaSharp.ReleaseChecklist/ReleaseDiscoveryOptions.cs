namespace SkiaSharp.ReleaseChecklist;

/// <summary>Configures SkiaSharp release discovery.</summary>
public sealed record ReleaseDiscoveryOptions
{
	/// <summary>Gets the source branch.</summary>
	/// <value>The source branch, or <see langword="null" /> to use the current branch.</value>
	public string? Branch { get; init; }

	/// <summary>Gets the exact release identity.</summary>
	/// <value>The release identity, or <see langword="null" /> to infer an eligible preview.</value>
	public string? Release { get; init; }

	/// <summary>Gets the reviewed maintenance creation point.</summary>
	/// <value>A remote ref or published SHA, or <see langword="null" /> when maintenance exists or the source is a safe creation point.</value>
	public string? MaintenanceBase { get; init; }
}
