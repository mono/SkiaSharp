namespace SkiaSharp.ReleaseTool.Errors
{
	/// <summary>
	/// Live state conflicts with a plan in a way that must never be forced.
	/// Mirrors Python's <c>release_common.ConflictError</c>: raised for
	/// anything the plan explicitly forbids recovering from automatically
	/// (a moved tag, a mismatched existing release, a diverged branch,
	/// etc). Callers must stop and report recovery instructions; nothing
	/// reacts to this by forcing state.
	/// </summary>
	public sealed class ConflictException : ReleaseToolException
	{
		public ConflictException(string message)
			: base(message)
		{
		}

		public ConflictException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
