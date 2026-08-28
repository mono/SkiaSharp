namespace SkiaSharp.ReleaseTool.Errors
{
	/// <summary>
	/// A plan or a plan file failed schema, shape, or digest validation.
	/// Mirrors Python's <c>release_common.ValidationError</c>.
	/// </summary>
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
