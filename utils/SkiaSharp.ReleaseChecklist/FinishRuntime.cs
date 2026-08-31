using System.Security.Cryptography;
using System.Text;
using ReleaseChecklist.Core;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Finishing;
using SkiaSharp.ReleaseTool.Milestones;
using SkiaSharp.ReleaseTool.NuGet;
using OldGitRepository = SkiaSharp.ReleaseTool.Git.GitRepository;

namespace SkiaSharp.ReleaseChecklist;

internal sealed class FinishRuntime : IFinishRuntime, IDisposable
{
	private readonly OldGitRepository repository;
	private readonly OctokitFinishGitHubClient github;
	private readonly NuGetOrgPackageSource packageSource;
	private readonly TimeProvider timeProvider;

	private FinishRuntime(
		OldGitRepository repository,
		OctokitFinishGitHubClient github,
		NuGetOrgPackageSource packageSource,
		TimeProvider timeProvider,
		FinishPlan? plan,
		string? pendingDetail)
	{
		this.repository = repository;
		this.github = github;
		this.packageSource = packageSource;
		this.timeProvider = timeProvider;
		Plan = plan;
		PendingDetail = pendingDetail;
	}

	public FinishPlan? Plan { get; }

	public string? PendingDetail { get; }

	public FinishCloseoutResult? CloseoutResult { get; private set; }

	public static async Task<FinishRuntime> CreateAsync(
		string repositoryPath,
		string publicVersion,
		CancellationToken cancellationToken)
	{
		var repository = await OldGitRepository.DiscoverAsync(
			repositoryPath,
			cancellationToken: cancellationToken).ConfigureAwait(false);
		await repository.FetchAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
		var toolingSha = await repository.ResolveAsync("HEAD", cancellationToken).ConfigureAwait(false);
		var github = new OctokitFinishGitHubClient();
		var packageSource = new NuGetOrgPackageSource();
		var timeProvider = TimeProvider.System;
		try
		{
			var verifier = new PublicReceiptVerifier(
				packageSource,
				new NuGetPackageSignatureVerifier());
			var builder = new FinishPlanBuilder(
				repository,
				verifier,
				github,
				ReleasePolicies.Load(repository.Root),
				timeProvider,
				Guid.NewGuid);
			var plan = await builder.BuildAsync(
				new FinishPlanRequest(publicVersion, toolingSha),
				cancellationToken).ConfigureAwait(false);
			return new FinishRuntime(
				repository,
				github,
				packageSource,
				timeProvider,
				plan,
				pendingDetail: null);
		}
		catch (PackagesPendingException ex)
		{
			return new FinishRuntime(
				repository,
				github,
				packageSource,
				timeProvider,
				plan: null,
				ex.Message);
		}
		catch
		{
			packageSource.Dispose();
			throw;
		}
	}

	public async ValueTask<CheckResult> CheckToolingAsync(CancellationToken cancellationToken)
	{
		var plan = RequirePlan();
		var exists = await repository.CommitExistsAsync(
			plan.ToolingSha,
			cancellationToken).ConfigureAwait(false);
		var trusted = exists && await repository.IsAncestorAsync(
			plan.ToolingSha,
			"refs/remotes/origin/main",
			cancellationToken).ConfigureAwait(false);
		var observation = new ObservationBuilder()
			.Add("tooling", plan.ToolingSha)
			.Add("exists", exists)
			.Add("trusted-main", trusted)
			.Build();
		return trusted
			? CheckResult.Done("Tooling is pinned to default-branch history.", observation)
			: CheckResult.Blocked(
				"Tooling revision is not on trusted default-branch history.",
				observation);
	}

	public async ValueTask<CheckResult> CheckTagAsync(CancellationToken cancellationToken)
	{
		var plan = RequirePlan();
		var tags = await repository.RemoteTagsAsync(
			pattern: $"refs/tags/{plan.Tag.Name}",
			cancellationToken: cancellationToken).ConfigureAwait(false);
		var actual = tags.GetValueOrDefault(plan.Tag.Name);
		var observation = new ObservationBuilder()
			.Add("tag", plan.Tag.Name)
			.Add("expected", plan.Receipt.SourceCommit)
			.Add("actual", actual ?? "")
			.Build();
		return actual switch
		{
			null => CheckResult.NotDone($"Tag {plan.Tag.Name} is missing.", observation),
			_ when actual == plan.Receipt.SourceCommit =>
				CheckResult.Done($"Tag {plan.Tag.Name} targets the package source.", observation),
			_ => CheckResult.Blocked($"Tag {plan.Tag.Name} targets another commit.", observation),
		};
	}

	public async ValueTask CreateTagAsync(CancellationToken cancellationToken)
	{
		var plan = RequirePlan();
		try
		{
			await repository.PushTagAsync(
				plan.Tag.Name,
				plan.Receipt.SourceCommit,
				cancellationToken: cancellationToken).ConfigureAwait(false);
		}
		catch (ReleaseToolException)
		{
			if ((await CheckTagAsync(cancellationToken).ConfigureAwait(false)).Status != ChecklistStatus.Done)
				throw;
		}
	}

	public async ValueTask<CheckResult> CheckDraftAsync(CancellationToken cancellationToken)
	{
		var plan = RequirePlan();
		var release = await github.GetReleaseAsync(plan.Tag.Name, cancellationToken).ConfigureAwait(false);
		if (release is null)
			return CheckResult.NotDone($"No GitHub Release draft exists for {plan.Tag.Name}.");
		var metadata = ValidateRelease(plan, release);
		if (metadata is not null)
			return metadata;
		if (!release.IsDraft)
			return CheckResult.Done($"GitHub Release {plan.Tag.Name} is already published.", ObserveRelease(release));
		try
		{
			var markers = ManagedReleaseMarkers.Inspect(release.Body);
			if (markers != ManagedMarkerState.Complete ||
				!ManagedReleaseMarkers.HasGeneratedNotes(release.Body))
			{
				return CheckResult.NotDone(
					$"Draft {plan.Tag.Name} needs managed generated notes.",
					ObserveRelease(release));
			}
			return CheckResult.Done($"Draft {plan.Tag.Name} is ready for review.", ObserveRelease(release));
		}
		catch (GitHubException ex)
		{
			return CheckResult.Blocked(ex.Message, ObserveRelease(release));
		}
	}

	public async ValueTask CreateDraftAsync(CancellationToken cancellationToken)
	{
		var plan = RequirePlan();
		_ = await new FinishService(
			repository,
			github,
			timeProvider,
			Guid.NewGuid).CreateDraftAsync(
				plan,
				plan.PlanId,
				cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask<CheckResult> CheckPublishedAsync(CancellationToken cancellationToken)
	{
		var plan = RequirePlan();
		var release = await github.GetReleaseAsync(plan.Tag.Name, cancellationToken).ConfigureAwait(false);
		if (release is null)
			return CheckResult.NotDone($"No GitHub Release exists for {plan.Tag.Name}.");
		var metadata = ValidateRelease(plan, release);
		if (metadata is not null)
			return metadata;
		var observation = ObserveRelease(release);
		if (release.IsDraft)
		{
			try
			{
				if (ManagedReleaseMarkers.Inspect(release.Body) != ManagedMarkerState.Complete ||
					!ManagedReleaseMarkers.HasGeneratedNotes(release.Body))
				{
					return CheckResult.Blocked(
						$"Draft {plan.Tag.Name} is not ready to publish.",
						observation);
				}
			}
			catch (GitHubException ex)
			{
				return CheckResult.Blocked(ex.Message, observation);
			}
			return CheckResult.NotDone(
				$"Draft {plan.Tag.Name} is ready to publish.",
				observation);
		}
		return CheckResult.Done($"GitHub Release {plan.Tag.Name} is published.", observation);
	}

	public async ValueTask PublishAsync(CancellationToken cancellationToken)
	{
		var plan = RequirePlan();
		var service = new FinishService(repository, github, timeProvider, Guid.NewGuid);
		var publication = await service.PlanPublicationAsync(
			plan,
			plan.PlanId,
			cancellationToken).ConfigureAwait(false);
		_ = await service.PublishAsync(
			plan,
			plan.PlanId,
			publication,
			publication.PublicationPlanId,
			cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask<CheckResult> CheckShippedAsync(CancellationToken cancellationToken)
	{
		var tag = await CheckTagAsync(cancellationToken).ConfigureAwait(false);
		if (tag.Status != ChecklistStatus.Done)
			return tag;
		var published = await CheckPublishedAsync(cancellationToken).ConfigureAwait(false);
		return published.Status == ChecklistStatus.Done
			? CheckResult.Done("The exact tag and GitHub Release prove shipped state.", published.Observation)
			: published;
	}

	public async ValueTask CloseoutAsync(CancellationToken cancellationToken)
	{
		var plan = RequirePlan();
		CloseoutResult = await new FinishCloseoutService(
			repository,
			new OctokitCloseoutGitHubClient(),
			new HttpChromiumScheduleClient(),
			timeProvider).ApplyAsync(
				plan,
				plan.PlanId,
				cancellationToken).ConfigureAwait(false);
	}

	public CheckResult CheckScheduleResult() =>
		CheckCloseout(
			"Public milestone schedule convergence",
			CloseoutResult?.ScheduleResults.Count ?? 0,
			CloseoutResult?.ScheduleResults.Any(result =>
				result.Status == FinishCloseoutStatus.Blocked) == true);

	public CheckResult CheckReconciliationResult() =>
		CheckCloseout(
			"Shipped pull request and issue reconciliation",
			CloseoutResult?.ReconcileResults.Count ?? 0,
			CloseoutResult?.ReconcileResults.Any(result =>
				result.Status == FinishCloseoutStatus.Blocked) == true);

	public CheckResult CheckClosureResult() =>
		CheckCloseout(
			"Milestone rollover and closure",
			CloseoutResult?.ClosureResults.Count ?? 0,
			CloseoutResult?.ClosureResults.Any(result =>
				result.Status == FinishCloseoutStatus.Blocked) == true);

	public CheckResult CheckDispatchResult() =>
		CheckCloseout(
			"Repository follow-up dispatch",
			CloseoutResult?.Dispatches.Count ?? 0,
			CloseoutResult?.Dispatches.Any(result =>
				result.Status != FinishDispatchStatus.Dispatched) == true);

	public async ValueTask<CheckResult> CheckReviewedSummaryAsync(
		CancellationToken cancellationToken)
	{
		var plan = RequirePlan();
		var release = await github.GetReleaseAsync(plan.Tag.Name, cancellationToken).ConfigureAwait(false);
		if (release is null)
			return CheckResult.Blocked("Published GitHub Release is missing.");
		try
		{
			if (ManagedReleaseMarkers.Inspect(release.Body) != ManagedMarkerState.Complete)
				return CheckResult.Skipped("Published release has no managed summary region.");
		}
		catch (GitHubException ex)
		{
			return CheckResult.Blocked(ex.Message, ObserveRelease(release));
		}
		var start = release.Body.IndexOf(
			ManagedReleaseMarkers.SummaryStart,
			StringComparison.Ordinal) + ManagedReleaseMarkers.SummaryStart.Length;
		var end = release.Body.IndexOf(
			ManagedReleaseMarkers.SummaryEnd,
			start,
			StringComparison.Ordinal);
		return !string.IsNullOrWhiteSpace(release.Body[start..end])
			? CheckResult.Done("The reviewed release summary has converged.", ObserveRelease(release))
			: CheckResult.NotDone(
				"The reviewed summary is waiting for the release-notes review workflow.",
				ObserveRelease(release));

	}

	public void Dispose() => packageSource.Dispose();

	private FinishPlan RequirePlan() =>
		Plan ?? throw new InvalidOperationException(PendingDetail ?? "Finish plan is unavailable.");

	private static CheckResult? ValidateRelease(FinishPlan plan, FinishGitHubRelease release)
	{
		var observation = ObserveRelease(release);
		if (release.TagName != plan.Tag.Name ||
			release.Title != plan.Release.Title ||
			release.IsPrerelease == plan.Release.Stable)
		{
			return CheckResult.Blocked("GitHub Release metadata conflicts with the public receipt.", observation);
		}
		if (release.TargetCommitish != plan.Receipt.SourceCommit &&
			!(release.IsDraft == false &&
				(release.TargetCommitish == "main" ||
				 release.TargetCommitish == plan.Receipt.SourceBranch)))
		{
			return CheckResult.Blocked("GitHub Release target conflicts with the package source.", observation);
		}
		return null;
	}

	private static Observation ObserveRelease(FinishGitHubRelease release) =>
		new ObservationBuilder()
			.Add("release-id", release.Id)
			.Add("tag", release.TagName)
			.Add("target", release.TargetCommitish)
			.Add("draft", release.IsDraft)
			.Add("prerelease", release.IsPrerelease)
			.Add("body-sha256", Convert.ToHexStringLower(
				SHA256.HashData(Encoding.UTF8.GetBytes(release.Body))))
			.Add("url", release.Url.ToString())
			.Build();

	private CheckResult CheckCloseout(string name, int count, bool blocked)
	{
		if (CloseoutResult is null)
			return CheckResult.NotDone($"{name} has not been evaluated in this run.");
		if (blocked)
			return CheckResult.Blocked($"{name} reported a blocked operation.");
		return CheckResult.Done($"{name} completed with {count} operation(s).");
	}
}
