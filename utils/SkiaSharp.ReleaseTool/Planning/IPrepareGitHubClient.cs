namespace SkiaSharp.ReleaseTool.Planning
{
	public sealed record PullRequestInfo(int Number, Uri Url);

	public interface IPrepareGitHubClient
	{
		Task<string?> GetRefShaAsync(
			string repository,
			string reference,
			CancellationToken cancellationToken = default);

		Task<PullRequestInfo?> FindOpenPullRequestAsync(
			string head,
			string @base,
			CancellationToken cancellationToken = default);

		Task CreateRefAsync(
			string repository,
			string reference,
			string sha,
			CancellationToken cancellationToken = default);

		Task<PullRequestInfo> CreatePullRequestAsync(
			string head,
			string @base,
			string title,
			string body,
			CancellationToken cancellationToken = default);
	}
}
