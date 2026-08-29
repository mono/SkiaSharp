using System.Net;
using Octokit;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Finishing
{
	internal sealed class OctokitFinishGitHubClient : IFinishGitHubWriteClient
	{
		private const string Owner = "mono";
		private const string Repository = "SkiaSharp";
		private readonly Func<string, CancellationToken, Task<FinishGitHubRelease?>> getPublished;
		private readonly Func<CancellationToken, Task<IReadOnlyList<FinishGitHubRelease>>> listReleases;
		private readonly Func<
			GenerateReleaseNotesRequest,
			CancellationToken,
			Task<string>> generateReleaseNotes;
		private readonly Func<
			NewRelease,
			CancellationToken,
			Task<FinishGitHubRelease>> createRelease;
		private readonly Func<
			long,
			ReleaseUpdate,
			CancellationToken,
			Task<FinishGitHubRelease>> editRelease;

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
			generateReleaseNotes = (request, cancellationToken) =>
				GenerateReleaseNotesAsync(client, request, cancellationToken);
			createRelease = (request, cancellationToken) =>
				CreateReleaseAsync(client, request, cancellationToken);
			editRelease = (releaseId, request, cancellationToken) =>
				EditReleaseAsync(client, releaseId, request, cancellationToken);
		}

		internal OctokitFinishGitHubClient(
			Func<string, CancellationToken, Task<FinishGitHubRelease?>> getPublished,
			Func<CancellationToken, Task<IReadOnlyList<FinishGitHubRelease>>> listReleases)
			: this(
				getPublished,
				listReleases,
				(_, _) => throw new NotSupportedException(),
				(_, _) => throw new NotSupportedException(),
				(_, _, _) => throw new NotSupportedException())
		{
		}

		internal OctokitFinishGitHubClient(
			Func<string, CancellationToken, Task<FinishGitHubRelease?>> getPublished,
			Func<CancellationToken, Task<IReadOnlyList<FinishGitHubRelease>>> listReleases,
			Func<GenerateReleaseNotesRequest, CancellationToken, Task<string>> generateReleaseNotes,
			Func<NewRelease, CancellationToken, Task<FinishGitHubRelease>> createRelease,
			Func<long, ReleaseUpdate, CancellationToken, Task<FinishGitHubRelease>> editRelease)
		{
			this.getPublished = getPublished;
			this.listReleases = listReleases;
			this.generateReleaseNotes = generateReleaseNotes;
			this.createRelease = createRelease;
			this.editRelease = editRelease;
		}

		public async Task<FinishGitHubRelease?> GetReleaseAsync(
			string tag,
			CancellationToken cancellationToken = default)
		{
			try
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
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (ApiException ex)
			{
				throw new GitHubException($"GitHub release lookup failed for exact tag '{tag}'", ex);
			}
		}

		public async Task<string> GenerateReleaseNotesAsync(
			string tag,
			string targetCommitish,
			string? previousTag,
			CancellationToken cancellationToken = default)
		{
			var request = new GenerateReleaseNotesRequest(tag)
			{
				TargetCommitish = targetCommitish,
				PreviousTagName = previousTag,
			};
			try
			{
				return await generateReleaseNotes(request, cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (ApiException ex)
			{
				throw new GitHubException($"GitHub release-note generation failed for exact tag '{tag}'", ex);
			}
		}

		public async Task<FinishGitHubRelease> CreateDraftAsync(
			string tag,
			string title,
			string targetCommitish,
			string body,
			bool prerelease,
			CancellationToken cancellationToken = default)
		{
			var request = new NewRelease(tag)
			{
				Name = title,
				TargetCommitish = targetCommitish,
				Body = body,
				Draft = true,
				Prerelease = prerelease,
			};
			try
			{
				var created = await createRelease(request, cancellationToken).ConfigureAwait(false);
				return RequireExactRelease(
					created,
					tag,
					title,
					targetCommitish,
					body,
					prerelease,
					isDraft: true,
					"creation");
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (ApiException ex) when (IsWriteRace(ex))
			{
				var raced = await GetReleaseAsync(tag, cancellationToken).ConfigureAwait(false);
				if (raced is not null && IsExactRelease(
					raced,
					tag,
					title,
					targetCommitish,
					body,
					prerelease,
					isDraft: true))
				{
					return raced;
				}
				throw new GitHubException(
					$"GitHub rejected draft creation for '{tag}' and no exact draft resulted",
					ex);
			}
			catch (ApiException ex)
			{
				throw new GitHubException($"GitHub draft creation failed for exact tag '{tag}'", ex);
			}
		}

		public Task<FinishGitHubRelease> UpdateDraftBodyAsync(
			FinishGitHubRelease draft,
			string body,
			CancellationToken cancellationToken = default)
		{
			if (!draft.IsDraft)
				throw new GitHubException($"release '{draft.TagName}' is already published");
			return EditAsync(draft, body, isDraft: true, "body update", cancellationToken);
		}

		public Task<FinishGitHubRelease> PublishDraftAsync(
			FinishGitHubRelease draft,
			CancellationToken cancellationToken = default)
		{
			if (!draft.IsDraft)
				throw new GitHubException($"release '{draft.TagName}' is already published");
			return EditAsync(draft, draft.Body, isDraft: false, "publication", cancellationToken);
		}

		private async Task<FinishGitHubRelease> EditAsync(
			FinishGitHubRelease release,
			string body,
			bool isDraft,
			string operation,
			CancellationToken cancellationToken)
		{
			var request = new ReleaseUpdate
			{
				TagName = release.TagName,
				Name = release.Title,
				TargetCommitish = release.TargetCommitish,
				Body = body,
				Draft = isDraft,
				Prerelease = release.IsPrerelease,
			};
			try
			{
				var edited = await editRelease(
					release.Id,
					request,
					cancellationToken).ConfigureAwait(false);
				return RequireExactRelease(
					edited,
					release.TagName,
					release.Title,
					release.TargetCommitish,
					body,
					release.IsPrerelease,
					isDraft,
					operation,
					release.Id);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (ApiException ex) when (IsWriteRace(ex))
			{
				var raced = await GetReleaseAsync(
					release.TagName,
					cancellationToken).ConfigureAwait(false);
				if (raced is not null &&
					raced.Id == release.Id &&
					IsExactRelease(
						raced,
						release.TagName,
						release.Title,
						release.TargetCommitish,
						body,
						release.IsPrerelease,
						isDraft))
				{
					return raced;
				}
				throw new GitHubException(
					$"GitHub rejected release {operation} for '{release.TagName}' and no exact state resulted",
					ex);
			}
			catch (ApiException ex)
			{
				throw new GitHubException($"GitHub release {operation} failed for '{release.TagName}'", ex);
			}
		}

		private static async Task<FinishGitHubRelease?> GetPublishedAsync(
			GitHubClient client,
			string tag,
			CancellationToken cancellationToken)
		{
			try
			{
				var release = await client.Repository.Release
					.Get(Owner, Repository, tag)
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
					.GetAll(
						Owner,
						Repository,
						new ApiOptions
						{
							StartPage = 1,
							PageSize = 100,
						})
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

		private static async Task<string> GenerateReleaseNotesAsync(
			GitHubClient client,
			GenerateReleaseNotesRequest request,
			CancellationToken cancellationToken)
		{
			var notes = await client.Repository.Release
				.GenerateReleaseNotes(Owner, Repository, request)
				.WaitAsync(cancellationToken)
				.ConfigureAwait(false);
			return notes.Body ?? "";
		}

		private static async Task<FinishGitHubRelease> CreateReleaseAsync(
			GitHubClient client,
			NewRelease request,
			CancellationToken cancellationToken) =>
			Map(await client.Repository.Release
				.Create(Owner, Repository, request)
				.WaitAsync(cancellationToken)
				.ConfigureAwait(false));

		private static async Task<FinishGitHubRelease> EditReleaseAsync(
			GitHubClient client,
			long releaseId,
			ReleaseUpdate request,
			CancellationToken cancellationToken) =>
			Map(await client.Repository.Release
				.Edit(Owner, Repository, releaseId, request)
				.WaitAsync(cancellationToken)
				.ConfigureAwait(false));

		private static FinishGitHubRelease RequireExactTag(
			FinishGitHubRelease release,
			string tag)
		{
			if (release.TagName != tag)
				throw new GitHubException($"GitHub returned release tag '{release.TagName}' for exact tag '{tag}'");
			return release;
		}

		private static FinishGitHubRelease RequireExactRelease(
			FinishGitHubRelease release,
			string tag,
			string title,
			string targetCommitish,
			string body,
			bool prerelease,
			bool isDraft,
			string operation,
			long? expectedId = null)
		{
			if ((expectedId is null || release.Id == expectedId) &&
				IsExactRelease(
					release,
					tag,
					title,
					targetCommitish,
					body,
					prerelease,
					isDraft))
			{
				return release;
			}
			throw new GitHubException(
				$"GitHub returned unexpected release state after {operation} for '{tag}'");
		}

		private static bool IsExactRelease(
			FinishGitHubRelease release,
			string tag,
			string title,
			string targetCommitish,
			string body,
			bool prerelease,
			bool isDraft) =>
			release.Id > 0 &&
			release.TagName == tag &&
			release.Title == title &&
			release.TargetCommitish == targetCommitish &&
			release.Body == body &&
			release.IsPrerelease == prerelease &&
			release.IsDraft == isDraft;

		private static bool IsWriteRace(ApiException exception) =>
			exception.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.UnprocessableEntity;

		private static FinishGitHubRelease Map(Release release) =>
			new(
				release.Id,
				release.TagName,
				release.Name,
				release.Draft,
				release.Prerelease,
				release.TargetCommitish,
				release.Body ?? "",
				new Uri(release.HtmlUrl));
	}
}
