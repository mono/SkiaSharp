namespace SkiaSharp.ReleaseTool.Errors
{
	/// <summary>
	/// Base class for every error raised by the release automation CLI.
	/// Mirrors Python's <c>release_common.ReleaseToolError</c>: a single,
	/// concrete exception type callers can catch to mean "the release
	/// tooling refused to proceed", distinguished by message and by the
	/// handful of narrower subclasses below.
	/// </summary>
	public class ReleaseToolException : Exception
	{
		public ReleaseToolException(string message)
			: base(message)
		{
		}

		public ReleaseToolException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
