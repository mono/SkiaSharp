namespace SkiaSharp.ReleaseTool.Errors
{
	public sealed class MilestoneException : ReleaseToolException
	{
		public MilestoneException(string message)
			: base(message)
		{
		}

		public MilestoneException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
