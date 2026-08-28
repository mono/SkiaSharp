namespace SkiaSharp.ReleaseTool.Errors
{
	/// <summary>A plan or plan file failed shape or semantic validation.</summary>
	public sealed class ValidationException : ReleaseToolException
	{
		public ValidationException(string message)
			: base(message)
		{
		}

		public ValidationException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
