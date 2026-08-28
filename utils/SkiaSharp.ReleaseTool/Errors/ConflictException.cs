namespace SkiaSharp.ReleaseTool.Errors
{
	/// <summary>Live state conflicts with a plan in a way that must never be forced.</summary>
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
