namespace ReleaseChecklist.Core;

/// <summary>Represents an invalid checklist definition.</summary>
/// <param name="message">The message that describes the definition error.</param>
public sealed class ChecklistDefinitionException(string message) : Exception(message);
