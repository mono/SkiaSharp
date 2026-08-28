namespace SkiaSharp.ReleaseTool.Errors
{
	/// <summary>
	/// A plan could not be constructed because inputs or state are invalid.
	/// Mirrors Python's <c>release_common.PlanError</c>.
	/// </summary>
	public sealed class PlanException : ReleaseToolException
	{
		public PlanException(string message)
			: base(message)
		{
		}

		public PlanException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
