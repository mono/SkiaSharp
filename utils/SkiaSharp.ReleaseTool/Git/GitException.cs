using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Git
{
	/// <summary>
	/// A Git operation failed or returned an unexpected result. Mirrors
	/// Python's <c>release_git.GitError</c>.
	/// </summary>
	public sealed class GitException : ReleaseToolException
	{
		public GitException(string message)
			: base(message)
		{
		}

		public GitException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
