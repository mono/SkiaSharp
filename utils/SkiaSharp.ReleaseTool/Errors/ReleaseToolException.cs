namespace SkiaSharp.ReleaseTool.Errors
{
	/// <summary>Base class for errors raised by the release automation CLI.</summary>
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
