namespace ReleaseChecklist.GitHub;

/// <summary>Represents malformed or unsuccessful GitHub protocol state.</summary>
/// <param name="message">The message that describes the protocol error.</param>
public sealed class GitHubProtocolException(string message) : Exception(message);
