namespace ReleaseChecklist.Core;

/// <summary>Configures one checklist run.</summary>
public sealed record ChecklistRunOptions
{
	/// <summary>Gets the run mode.</summary>
	/// <value>The run mode. The default is <see cref="ChecklistRunMode.DryRun" />.</value>
	public ChecklistRunMode Mode { get; init; } = ChecklistRunMode.DryRun;

	/// <summary>Gets the maximum time allowed for an action or post-action check.</summary>
	/// <value>The completion timeout. The default is five minutes.</value>
	public TimeSpan CompletionTimeout { get; init; } = TimeSpan.FromMinutes(5);
}
