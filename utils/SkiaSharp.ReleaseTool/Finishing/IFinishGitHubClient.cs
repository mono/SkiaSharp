namespace SkiaSharp.ReleaseTool.Finishing
{
	internal sealed record FinishGitHubRelease(
		string TagName,
		string Title,
		bool IsDraft,
		bool IsPrerelease,
		string TargetCommitish,
		string Body,
		Uri Url);

	internal interface IFinishGitHubClient
	{
		Task<FinishGitHubRelease?> GetReleaseAsync(
			string tag,
			CancellationToken cancellationToken = default);
	}
}
