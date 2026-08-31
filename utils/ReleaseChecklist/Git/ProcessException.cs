namespace ReleaseChecklist.Git;

/// <summary>Represents a child process that exited unsuccessfully.</summary>
/// <param name="message">The message that describes the process failure.</param>
public sealed class ProcessException(string message) : Exception(message);
