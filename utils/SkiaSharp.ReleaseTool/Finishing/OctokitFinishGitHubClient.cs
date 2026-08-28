using Octokit;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Finishing
{
	internal sealed class OctokitFinishGitHubClient : IFinishGitHubClient
	{
		private readonly Func<string, CancellationToken, Task<FinishGitHubRelease?>> getPublished;
		private readonly Func<CancellationToken, Task<IReadOnlyList<FinishGitHubRelease>>> listReleases;

		public OctokitFinishGitHubClient(string? token = null)
		{
			var client = new GitHubClient(new ProductHeaderValue("SkiaSharp.ReleaseTool"));
			token ??= Planning.OctokitPrepareGitHubClient.ResolveToken(
				Environment.GetEnvironmentVariable);
			if (!string.IsNullOrEmpty(token))
				client.Credentials = new Credentials(token);
			getPublished = (tag, cancellationToken) =>
				GetPublishedAsync(client, tag, cancellationToken);
			listReleases = cancellationToken =>
				ListReleasesAsync(client, cancellationToken);
		}

		internal OctokitFinishGitHubClient(
			Func<string, CancellationToken, Task<FinishGitHubRelease?>> getPublished,
			Func<CancellationToken, Task<IReadOnlyList<FinishGitHubRelease>>> listReleases)
		{
			this.getPublished = getPublished;
			this.listReleases = listReleases;
		}

		public async Task<FinishGitHubRelease?> GetReleaseAsync(
			string tag,
			CancellationToken cancellationToken = default)
		{
			var published = await getPublished(tag, cancellationToken).ConfigureAwait(false);
			if (published is not null)
				return RequireExactTag(published, tag);

			var matches = (await listReleases(cancellationToken).ConfigureAwait(false))
				.Where(release => release.IsDraft && release.TagName == tag)
				.ToArray();
			return matches.Length switch
			{
				0 => null,
				1 => RequireExactTag(matches[0], tag),
				_ => throw new GitHubException(
					$"GitHub returned multiple draft releases for exact tag '{tag}'"),
			};
		}

		private static async Task<FinishGitHubRelease?> GetPublishedAsync(
			GitHubClient client,
			string tag,
			CancellationToken cancellationToken)
		{
			try
			{
				var release = await client.Repository.Release
					.Get("mono", "SkiaSharp", tag)
					.WaitAsync(cancellationToken)
					.ConfigureAwait(false);
				return Map(release);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (NotFoundException)
			{
				return null;
			}
			catch (ApiException ex)
			{
				throw new GitHubException($"GitHub release lookup failed for exact tag '{tag}'", ex);
			}
		}

		private static async Task<IReadOnlyList<FinishGitHubRelease>> ListReleasesAsync(
			GitHubClient client,
			CancellationToken cancellationToken)
		{
			try
			{
				var releases = await client.Repository.Release
					.GetAll("mono", "SkiaSharp")
					.WaitAsync(cancellationToken)
					.ConfigureAwait(false);
				return releases.Select(Map).ToArray();
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (ApiException ex)
			{
				throw new GitHubException("GitHub release listing failed while checking drafts", ex);
			}
		}

		private static FinishGitHubRelease RequireExactTag(
			FinishGitHubRelease release,
			string tag)
		{
			if (release.TagName != tag)
				throw new GitHubException($"GitHub returned release tag '{release.TagName}' for exact tag '{tag}'");
			return release;
		}

		private static FinishGitHubRelease Map(Release release) =>
			new(
				release.TagName,
				release.Name,
				release.Draft,
				release.Prerelease,
				release.TargetCommitish,
				release.Body ?? "",
				new Uri(release.HtmlUrl));
	}
}
