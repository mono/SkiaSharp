namespace SkiaSharp.ReleaseTool.Errors
{
	public sealed class GitHubException : ReleaseToolException
	{
		public GitHubException(string message)
			: base(message)
		{
		}

		public GitHubException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
