namespace ReleaseChecklist.Core;

/// <summary>Records an unexpected failure separately from desired-state status.</summary>
/// <param name="Phase">The phase in which the failure occurred.</param>
/// <param name="Message">The human-readable failure message.</param>
/// <param name="Exception">The exception that caused the failure.</param>
public sealed record ExecutionError(
	ChecklistPhase Phase,
	string Message,
	Exception Exception);
