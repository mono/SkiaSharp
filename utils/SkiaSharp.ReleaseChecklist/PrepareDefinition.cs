using ReleaseChecklist.Core;
using ReleaseChecklist.Git;
using ReleaseChecklist.GitHub;

namespace SkiaSharp.ReleaseChecklist;

/// <summary>Composes the SkiaSharp repository preparation checklist.</summary>
public static class PrepareDefinition
{
	/// <summary>Builds the Prepare definition from frozen discovery inputs.</summary>
	/// <param name="found">The immutable release discovery result.</param>
	/// <param name="local">The local SkiaSharp repository.</param>
	/// <param name="github">The GitHub repository client.</param>
	/// <returns>The validated Prepare checklist definition.</returns>
	public static ChecklistDefinition Build(
		ReleaseDiscoveryResult found,
		GitRepository local,
		IGitHubRepositoryClient github)
	{
		var skiaRepository = new GitHubRepositoryIdentity("mono", "skia");
		var skiaSharpRepository = new GitHubRepositoryIdentity("mono", "SkiaSharp");

		return new ChecklistBuilder().Sequence("prepare", $"Prepare SkiaSharp {found.Identity.Raw}", prepare =>
		{
			prepare.Step(new StepOptions(
				"validate-tooling",
				"Validate the trusted tooling revision")
			{
				Check = Check.From(async token =>
				{
					var tooling = await local.ResolveAsync("HEAD", token).ConfigureAwait(false);
					var trusted = await local.IsAncestorAsync(
						tooling,
						$"refs/remotes/{local.Remote}/main",
						token).ConfigureAwait(false);
					var observation = new ObservationBuilder()
						.Add("repository", local.RepositoryIdentity)
						.Add("tooling", tooling)
						.Add("trusted-main", trusted)
						.Build();
					return trusted
						? CheckResult.Done("Tooling is pinned to default-branch history.", observation)
						: CheckResult.Blocked(
							"Tooling revision is not on trusted default-branch history.",
							observation);
				}),
			});

			prepare.Step(new StepOptions(
				"validate-base",
				"Validate the selected release base")
			{
				Check = Check.From(token => ValidateBaseAsync(local, found, token)),
			});

			var maintenance = prepare.GitRemoteBranch(new GitRemoteBranchOptions
			{
				Id = "maintenance-branch",
				Title = "Create or verify the maintenance branch",
				Repository = local,
				Branch = found.MaintenanceBranch,
				StartPoint = found.MaintenanceExpectedSha,
				ExpectedTarget = found.MaintenanceExpectedSha,
				AcceptExisting = (state, token) =>
					ValidateMaintenanceAsync(local, found, state, token),
				CommitMessage = $"Create {found.MaintenanceBranch}",
				When = Condition.From(_ => !found.Identity.IsHotfix),
			});

			Step? skia = null;
			Step? skiaSharp = null;
			prepare.Parallel(
				"exact-release-branches",
				"Create or verify exact release branches",
				parallel =>
				{
					skia = parallel.GitHubBranch(new GitHubBranchOptions
					{
						Id = "mono-skia-release-branch",
						Title = "Create or verify the mono/skia branch",
						Client = github,
						Repository = skiaRepository,
						Branch = found.ReleaseBranch,
						ExpectedSha = found.SkiaSha,
					});
					skiaSharp = parallel.GitRemoteBranch(new GitRemoteBranchOptions
					{
						Id = "skiasharp-release-branch",
						Title = "Create or verify the SkiaSharp branch",
						Repository = local,
						Branch = found.ReleaseBranch,
						StartPoint = found.ReleaseBaseSha,
						ExpectedTarget = found.ReleaseBaseSha,
						AcceptExisting = (state, token) =>
							ValidateReleaseBranchAsync(local, found, state, token),
						ConfigureCommit = (repository, token) =>
							VersionFiles.ConfigureReleaseAsync(repository, found.Identity, token),
						CommitMessage = $"Create release branch for {found.Identity.Raw}",
					});
				});

			prepare.Step(new StepOptions(
				"release-source-ready",
				"Authoritatively verify release source state")
			{
				Check = Check.All(
					maintenance.DesiredState!,
					skia!.DesiredState!,
					skiaSharp!.DesiredState!),
			});

			if (found.StableBump.Required)
				AddStableBump(prepare, found, local, github, skiaSharpRepository);
		});
	}

	private static void AddStableBump(
		IChecklistChildren prepare,
		ReleaseDiscoveryResult found,
		GitRepository local,
		IGitHubRepositoryClient github,
		GitHubRepositoryIdentity repository)
	{
		var bumpBranch = found.StableBump.Branch!;
		prepare.Sequence("stable-non-hotfix", "Complete stable non-hotfix preparation", stable =>
		{
			stable.GitRemoteBranch(new GitRemoteBranchOptions
			{
				Id = "stable-bump-branch",
				Title = "Create or verify the stable bump branch",
				Repository = local,
				Branch = bumpBranch,
				StartPoint = found.MaintenanceExpectedSha,
				ExpectedTarget = found.MaintenanceExpectedSha,
				AcceptExisting = (state, token) => ValidateBumpAsync(local, found, state, token),
				ConfigureCommit = (git, token) => VersionFiles.ConfigureNextPreviewAsync(
					git,
					found.StableBump.NextSkia!,
					found.StableBump.NextHarfBuzz!,
					token),
				CommitMessage = $"Bump to the next version ({found.StableBump.NextSkia}) after release",
			});

			stable.GitHubPullRequest(new GitHubPullRequestOptions
			{
				Id = "stable-bump-pull-request",
				Title = "Open or verify the stable bump pull request",
				Client = github,
				Repository = repository,
				Head = bumpBranch,
				Base = found.MaintenanceBranch,
				PullRequestTitle =
					$"Bump to the next version ({found.StableBump.NextSkia}) after release",
				Body = StablePullRequestBody(found),
			});

			stable.Step(new StepOptions(
				"stable-bump-pull-request-merged",
				"Report whether a maintainer merged the bump")
			{
				Check = Check.From(token => CheckMergedAsync(
					github,
					repository,
					bumpBranch,
					found.MaintenanceBranch,
					token)),
			});
		});
	}

	private static async ValueTask<CheckResult> ValidateBaseAsync(
		GitRepository repository,
		ReleaseDiscoveryResult found,
		CancellationToken cancellationToken)
	{
		var actual = await repository.ResolveAsync(found.SourceRef, cancellationToken).ConfigureAwait(false);
		var state = await VersionFiles.ReadAsync(repository, found.SourceRef, cancellationToken)
			.ConfigureAwait(false);
		var skia = await repository.ReadGitlinkAsync(
			found.ReleaseBaseRef, "externals/skia", cancellationToken).ConfigureAwait(false);
		var observation = new ObservationBuilder()
			.Add("source-ref", found.SourceRef)
			.Add("expected-sha", found.SourceSha)
			.Add("actual-sha", actual)
			.Add("version", state.IdentityText)
			.Add("skia", skia)
			.Build();
		if (actual != found.SourceSha ||
			state != found.SourceVersion ||
			skia != found.SkiaSha)
		{
			return CheckResult.Blocked("Frozen source snapshot no longer matches discovery.", observation);
		}
		return CheckResult.Done("Selected release base is unchanged.", observation);
	}

	private static async ValueTask<bool> ValidateMaintenanceAsync(
		GitRepository repository,
		ReleaseDiscoveryResult found,
		GitRemoteBranchState state,
		CancellationToken cancellationToken)
	{
		if (state.Sha is null)
			return false;
		var version = await VersionFiles.ReadAsync(repository, state.Sha, cancellationToken)
			.ConfigureAwait(false);
		if (version.Label != "preview.0" ||
			version.SkiaSharpVersion.Split('.').Length != 3 ||
			!NuGet.Versioning.NuGetVersion.TryParse(version.SkiaSharpVersion, out var actual))
			return false;
		return actual.Major == found.Identity.Version.Major &&
			actual.Minor == found.Identity.Version.Minor &&
			NuGet.Versioning.VersionComparer.VersionRelease.Compare(
				actual, found.Identity.Version) >= 0;
	}

	private static async ValueTask<bool> ValidateReleaseBranchAsync(
		GitRepository repository,
		ReleaseDiscoveryResult found,
		GitRemoteBranchState state,
		CancellationToken cancellationToken)
	{
		if (state.Sha is null ||
			!await repository.IsAncestorAsync(
				found.ReleaseBaseSha, state.Sha, cancellationToken).ConfigureAwait(false))
			return false;
		var version = await VersionFiles.ReadAsync(repository, state.Sha, cancellationToken)
			.ConfigureAwait(false);
		var skia = await repository.ReadGitlinkAsync(
			state.Sha, "externals/skia", cancellationToken).ConfigureAwait(false);
		return version.SkiaSharpVersion == found.Identity.Numeric &&
			version.Label == found.Identity.Label &&
			skia == found.SkiaSha;
	}

	private static async ValueTask<bool> ValidateBumpAsync(
		GitRepository repository,
		ReleaseDiscoveryResult found,
		GitRemoteBranchState state,
		CancellationToken cancellationToken)
	{
		if (state.Sha is null ||
			!await repository.IsAncestorAsync(
				found.MaintenanceExpectedSha, state.Sha, cancellationToken).ConfigureAwait(false))
			return false;
		var version = await VersionFiles.ReadAsync(repository, state.Sha, cancellationToken)
			.ConfigureAwait(false);
		return version.SkiaSharpVersion == found.StableBump.NextSkia &&
			version.HarfBuzzSharpVersion == found.StableBump.NextHarfBuzz &&
			version.Label == "preview.0";
	}

	private static async ValueTask<CheckResult> CheckMergedAsync(
		IGitHubRepositoryClient github,
		GitHubRepositoryIdentity repository,
		string head,
		string @base,
		CancellationToken cancellationToken)
	{
		var matches = await github.FindPullRequestsAsync(
			repository, head, @base, cancellationToken).ConfigureAwait(false);
		var current = matches.Where(static pr => pr.Open || pr.Merged).ToArray();
		var observation = new ObservationBuilder()
			.Add("head", head)
			.Add("base", @base)
			.Add("matches", matches.Count)
			.Add("current-matches", current.Length)
			.Add("merged", current.Length == 1 && current[0].Merged)
			.Build();
		if (current.Length != 1)
			return CheckResult.Blocked("Expected exactly one stable bump pull request.", observation);
		return current[0].Merged
			? CheckResult.Done($"Pull request #{current[0].Number} was merged.", observation)
			: CheckResult.NotDone(
				$"Maintainer must merge pull request #{current[0].Number}; auto-merge is not implemented.",
				observation);
	}

	private static string StablePullRequestBody(ReleaseDiscoveryResult found) =>
		$"""
		## Description

		Advance `{found.MaintenanceBranch}` after cutting {found.Identity.Raw} by returning it to `preview.0` at SkiaSharp {found.StableBump.NextSkia} / HarfBuzzSharp {found.StableBump.NextHarfBuzz}.

		**Related issues**

		N/A.

		**Required skia PR**

		None.

		**Areas affected**

		- [x] Build, packaging, or CI

		## Changes

		None - version metadata only.

		## Testing

		The release checklist authoritatively verifies this version transform.

		## Checklist

		- [x] Tests added or updated (not needed; version-only change verified by release tooling)
		- [x] `Changes` above lists all public API and behavioral changes (None)
		- [x] New/changed public API? N/A
		- [x] Native change? N/A
		""";
}
