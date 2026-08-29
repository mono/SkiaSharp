namespace SkiaSharp.ReleaseTool.Finishing
{
	internal sealed record FinishGitHubRelease(
		long Id,
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

	internal interface IFinishGitHubWriteClient : IFinishGitHubClient
	{
		Task<string> GenerateReleaseNotesAsync(
			string tag,
			string targetCommitish,
			string? previousTag,
			CancellationToken cancellationToken = default);

		Task<FinishGitHubRelease> CreateDraftAsync(
			string tag,
			string title,
			string targetCommitish,
			string body,
			bool prerelease,
			CancellationToken cancellationToken = default);

		Task<FinishGitHubRelease> UpdateDraftBodyAsync(
			FinishGitHubRelease draft,
			string body,
			CancellationToken cancellationToken = default);

		Task<FinishGitHubRelease> PublishDraftAsync(
			FinishGitHubRelease draft,
			CancellationToken cancellationToken = default);
	}
}
