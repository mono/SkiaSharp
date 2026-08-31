namespace ReleaseChecklist.Core;

/// <summary>Contains a validated checklist node tree.</summary>
/// <param name="Root">The root container.</param>
public sealed record ChecklistDefinition(ChecklistContainer Root);
