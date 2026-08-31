namespace SkiaSharp.ReleaseChecklist;

/// <summary>Represents release input or repository state that violates SkiaSharp release policy.</summary>
/// <param name="message">The message that describes the policy violation.</param>
public sealed class ReleasePolicyException(string message) : Exception(message);
