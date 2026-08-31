namespace ReleaseChecklist.Git;

/// <summary>Represents invalid or conflicting Git state.</summary>
/// <param name="message">The message that describes the Git error.</param>
public sealed class GitException(string message) : Exception(message);
