using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Model;

namespace SkiaSharp.ReleaseTool.Planning
{
	public sealed record PreparePlanRequest(
		string IntegrationTarget,
		string? RequestedVersion,
		string? ApprovedBase,
		string ToolingSha);

	public sealed class PreparePlanBuilder
	{
		internal const string VariablesPath = "scripts/azure-templates-variables.yml";
		internal const string VersionsPath = "scripts/VERSIONS.txt";
		internal const string SkiaSubmodulePath = "externals/skia";

		private readonly IPrepareRepository repository;
		private readonly IPrepareGitHubClient github;
		private readonly TimeProvider timeProvider;
		private readonly Func<Guid> newPlanId;

		public PreparePlanBuilder(
			IPrepareRepository repository,
			IPrepareGitHubClient github,
			TimeProvider? timeProvider = null,
			Func<Guid>? newPlanId = null)
		{
			this.repository = repository;
			this.github = github;
			this.timeProvider = timeProvider ?? TimeProvider.System;
			this.newPlanId = newPlanId ?? Guid.NewGuid;
		}

		public async Task<PreparePlan> BuildAsync(
			PreparePlanRequest request,
			CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var integrationTarget = ReleaseVersionPolicy.NormalizeIntegrationBranch(request.IntegrationTarget);
			var targetRef = $"refs/remotes/origin/{integrationTarget}";
			if (!await repository.RefExistsAsync(targetRef, cancellationToken).ConfigureAwait(false))
				throw new PlanException($"integration target {integrationTarget} does not exist on origin");

			var releaseBranches = await repository.ReleaseBranchesAsync(
				cancellationToken: cancellationToken).ConfigureAwait(false);
			var identity = request.RequestedVersion is null
				? await DetectNextPreviewAsync(targetRef, releaseBranches, cancellationToken).ConfigureAwait(false)
				: SkiaSharpReleaseIdentity.Parse(request.RequestedVersion);
			if (integrationTarget != "main" && integrationTarget != identity.IntegrationBranch)
			{
				throw new PlanException(
					$"integration target {integrationTarget} does not match release line {identity.IntegrationBranch}");
			}

			var baseSelection = await SelectBaseAsync(
				identity,
				releaseBranches,
				integrationTarget,
				request.ApprovedBase,
				cancellationToken).ConfigureAwait(false);
			var baseState = await ReadVersionStateAsync(baseSelection.Ref, cancellationToken).ConfigureAwait(false);
			var requiresPackageBump =
				baseState.SkiaText != identity.Numeric;
			var skiaSha = await repository.ReadGitlinkAsync(
				baseSelection.Ref,
				SkiaSubmodulePath,
				cancellationToken).ConfigureAwait(false);

			var releaseRemoteState = await GetReleaseRemoteStateAsync(
				identity,
				baseSelection,
				cancellationToken).ConfigureAwait(false);
			var skiaRemoteState = await GetSkiaRemoteStateAsync(
				identity,
				skiaSha,
				cancellationToken).ConfigureAwait(false);

			var warnings = new List<string>();
			if (baseSelection.Action == MaintenanceBranchAction.Create && !baseSelection.Exists)
			{
				warnings.Add(
					$"maintenance branch {identity.IntegrationBranch} does not exist and will be created from {baseSelection.MaintenanceBaseSha}");
			}

			var stableBump = identity.Stable && !identity.IsHotfix
				? await BuildStableBumpAsync(
					identity,
					baseSelection,
					baseState,
					cancellationToken).ConfigureAwait(false)
				: null;
			var operations = BuildOperations(
				identity,
				baseSelection,
				skiaSha,
				skiaRemoteState,
				releaseRemoteState,
				stableBump);
			var plan = new PreparePlan(
				SchemaVersion: 1,
				Operation: ReleaseOperation.Prepare,
				PlanId: newPlanId(),
				GeneratedAt: timeProvider.GetUtcNow(),
				ToolingSha: request.ToolingSha,
				NextAction: NextAction(operations),
				Input: new PrepareInput(
					integrationTarget,
					request.RequestedVersion,
					request.ApprovedBase),
				Release: new PrepareReleaseInfo(
					identity.Raw,
					identity.Raw,
					identity.ReleaseBranch),
				Base: new PrepareBaseInfo(baseSelection.Ref, baseSelection.Sha),
				MaintenanceBranch: new MaintenanceBranchInfo(
					identity.IntegrationBranch,
					baseSelection.Exists,
					baseSelection.Action,
					baseSelection.MaintenanceBaseSha),
				Skia: new PrepareSkiaInfo(skiaSha, skiaRemoteState),
				SkiaSharpRemoteState: releaseRemoteState,
				Versions: new PrepareVersionsInfo(requiresPackageBump),
				Operations: operations,
				StableBump: stableBump,
				Warnings: warnings);
			PreparePlanValidator.Validate(plan);
			return plan;
		}

		private async Task<SkiaSharpReleaseIdentity> DetectNextPreviewAsync(
			string targetRef,
			IReadOnlyList<string> releaseBranches,
			CancellationToken cancellationToken)
		{
			var state = await ReadVersionStateAsync(targetRef, cancellationToken).ConfigureAwait(false);
			if (state.Label != "preview.0")
			{
				throw new PlanException(
					$"{targetRef["refs/remotes/origin/".Length..]} PREVIEW_LABEL is '{state.Label}'; cannot calculate the next preview automatically");
			}

			var current = state.SkiaText;
			if (releaseBranches.Contains($"release/{current}", StringComparer.Ordinal))
				throw new PlanException($"stable branch release/{current} already exists");
			if (releaseBranches.Any(branch =>
				branch.StartsWith($"release/{current}-rc.", StringComparison.Ordinal)))
			{
				throw new PlanException($"an RC branch for {current} already exists");
			}

			var maxIteration = 0;
			var prefix = $"release/{current}-preview.";
			foreach (var branch in releaseBranches)
			{
				if (branch.StartsWith(prefix, StringComparison.Ordinal) &&
					int.TryParse(branch[prefix.Length..], out var iteration) &&
					iteration > maxIteration)
				{
					maxIteration = iteration;
				}
			}
			if (maxIteration == int.MaxValue)
				throw new PlanException($"cannot calculate the next preview for {current}: iteration overflow");
			return SkiaSharpReleaseIdentity.Parse($"{current}-preview.{maxIteration + 1}");
		}

		private async Task<BaseSelection> SelectBaseAsync(
			SkiaSharpReleaseIdentity identity,
			IReadOnlyList<string> releaseBranches,
			string integrationTarget,
			string? approvedBase,
			CancellationToken cancellationToken)
		{
			var integrationRef = $"refs/remotes/origin/{identity.IntegrationBranch}";
			var integrationExists = await repository.RefExistsAsync(
				integrationRef,
				cancellationToken).ConfigureAwait(false);
			if (identity.IsHotfix)
			{
				if (!identity.Stable)
				{
					var parent = identity.Version.Version.ToString(3);
					var tagRef = $"refs/tags/v{parent}";
					if (!await repository.RefExistsAsync(tagRef, cancellationToken).ConfigureAwait(false))
						throw new PlanException($"hotfix base tag {tagRef} does not exist");
					return new BaseSelection(
						tagRef,
						await repository.ResolveAsync(tagRef, cancellationToken).ConfigureAwait(false),
						integrationExists,
						MaintenanceBranchAction.None,
						null,
						null);
				}

				var hotfixCandidate = LatestPrereleaseBranch(releaseBranches, identity.Numeric)
					?? throw new PlanException($"no prerelease branch found to base hotfix {identity.Raw} on");
				var hotfixRef = $"refs/remotes/origin/{hotfixCandidate}";
				return new BaseSelection(
					hotfixRef,
					await repository.ResolveAsync(hotfixRef, cancellationToken).ConfigureAwait(false),
					integrationExists,
					MaintenanceBranchAction.None,
					null,
					null);
			}

			if (integrationExists)
			{
				return new BaseSelection(
					integrationRef,
					await repository.ResolveAsync(integrationRef, cancellationToken).ConfigureAwait(false),
					true,
					MaintenanceBranchAction.None,
					null,
					null);
			}

			string maintenanceBaseRef;
			if (approvedBase is not null)
			{
				if (!await repository.RefExistsAsync(approvedBase, cancellationToken).ConfigureAwait(false))
					throw new PlanException($"approved base '{approvedBase}' does not exist");
				maintenanceBaseRef = approvedBase;
			}
			else if (integrationTarget == "main")
			{
				maintenanceBaseRef = "refs/remotes/origin/main";
			}
			else
			{
				throw new PlanException(
					$"maintenance branch {identity.IntegrationBranch} does not exist; pass an explicitly approved preview.0 base to recover");
			}

			var maintenanceState = await ReadVersionStateAsync(
				maintenanceBaseRef,
				cancellationToken).ConfigureAwait(false);
			if (maintenanceState.SkiaText != identity.Numeric ||
				maintenanceState.Label != "preview.0")
			{
				var source = approvedBase is null
					? $"integration target {integrationTarget}"
					: $"approved base '{approvedBase}'";
				throw new PlanException(
					$"{source} is not a safe maintenance base for {identity.Numeric}: expected SkiaSharp {identity.Numeric} with PREVIEW_LABEL 'preview.0'; pass --approved-base with a matching ref");
			}
			var maintenanceBaseSha = await repository.ResolveAsync(
				maintenanceBaseRef,
				cancellationToken).ConfigureAwait(false);

			string releaseBaseRef;
			if (identity.Channel == ReleaseKind.Preview && identity.Iteration == 1)
			{
				releaseBaseRef = maintenanceBaseRef;
			}
			else
			{
				var candidate = LatestPrereleaseBranch(releaseBranches, identity.Numeric);
				releaseBaseRef = candidate is null
					? maintenanceBaseRef
					: $"refs/remotes/origin/{candidate}";
			}
			var releaseBaseSha = releaseBaseRef == maintenanceBaseRef
				? maintenanceBaseSha
				: await repository.ResolveAsync(releaseBaseRef, cancellationToken).ConfigureAwait(false);
			return new BaseSelection(
				releaseBaseRef,
				releaseBaseSha,
				false,
				MaintenanceBranchAction.Create,
				maintenanceBaseRef,
				maintenanceBaseSha);
		}

		private async Task<RemoteState> GetReleaseRemoteStateAsync(
			SkiaSharpReleaseIdentity identity,
			BaseSelection baseSelection,
			CancellationToken cancellationToken)
		{
			var remoteSha = await repository.RemoteShaAsync(
				identity.ReleaseBranch,
				cancellationToken: cancellationToken).ConfigureAwait(false);
			if (remoteSha is null)
				return RemoteState.Missing;
			if (!await repository.IsAncestorAsync(
				baseSelection.Sha,
				remoteSha,
				cancellationToken).ConfigureAwait(false))
			{
				throw new ConflictException(
					$"existing branch {identity.ReleaseBranch} ({remoteSha}) is not a descendant of the planned base {baseSelection.Sha}");
			}

			await ValidateExistingReleaseBranchAsync(
				identity,
				remoteSha,
				cancellationToken).ConfigureAwait(false);
			return RemoteState.Matching;
		}

		private async Task ValidateExistingReleaseBranchAsync(
			SkiaSharpReleaseIdentity identity,
			string remoteSha,
			CancellationToken cancellationToken)
		{
			var state = await ReadVersionStateAsync(remoteSha, cancellationToken).ConfigureAwait(false);
			if (state.Label != identity.Label)
			{
				throw new ConflictException(
					$"existing release branch {identity.ReleaseBranch} has PREVIEW_LABEL '{state.Label}', expected '{identity.Label}'");
			}
			if (state.SkiaText != identity.Numeric)
			{
				throw new ConflictException(
					$"existing release branch {identity.ReleaseBranch} has SkiaSharp version '{state.Skia}', expected '{identity.Numeric}'");
			}
		}

		private async Task<RemoteState> GetSkiaRemoteStateAsync(
			SkiaSharpReleaseIdentity identity,
			string expectedSha,
			CancellationToken cancellationToken)
		{
			var reference = $"refs/heads/{identity.ReleaseBranch}";
			var actualSha = await github.GetRefShaAsync(
				"mono/skia",
				reference,
				cancellationToken).ConfigureAwait(false);
			if (actualSha is null)
				return RemoteState.Missing;
			if (actualSha != expectedSha)
			{
				throw new ConflictException(
					$"mono/skia branch {identity.ReleaseBranch} already exists at {actualSha}, expected {expectedSha}");
			}
			return RemoteState.Matching;
		}

		private async Task<StableBumpInfo> BuildStableBumpAsync(
			SkiaSharpReleaseIdentity identity,
			BaseSelection baseSelection,
			VersionState baseState,
			CancellationToken cancellationToken)
		{
			var integrationRef = $"refs/remotes/origin/{identity.IntegrationBranch}";
			var stateRef = await repository.RefExistsAsync(
				integrationRef,
				cancellationToken).ConfigureAwait(false)
				? integrationRef
				: baseSelection.MaintenanceBaseRef
					?? throw new PlanException("missing maintenance branch has no validated creation point");
			var state = await ReadVersionStateAsync(stateRef, cancellationToken).ConfigureAwait(false);
			if (state.Label != "preview.0")
			{
				throw new PlanException(
					$"integration branch {identity.IntegrationBranch} PREVIEW_LABEL is '{state.Label}', expected 'preview.0'");
			}

			var (nextSkia, nextHarfBuzz) = HarfBuzzVersioning.CalculateNextVersions(
				identity.Numeric,
				baseState.HarfBuzzText);
			var bumpBranch = $"bump-version-{nextSkia}";
			var pullRequest = await github.FindOpenPullRequestAsync(
				bumpBranch,
				identity.IntegrationBranch,
				cancellationToken).ConfigureAwait(false);
			var nextVersion = NuGetVersion.Parse(nextSkia);
			PlanOperationStatus status;
			if (VersionComparer.VersionRelease.Compare(state.Skia, nextVersion) >= 0)
				status = PlanOperationStatus.Done;
			else if (VersionComparer.VersionRelease.Equals(state.Skia, identity.Version))
				status = pullRequest is null ? PlanOperationStatus.Pending : PlanOperationStatus.AwaitingUser;
			else
			{
				throw new PlanException(
					$"integration branch {identity.IntegrationBranch} SkiaSharp version {state.Skia} is neither the released version {identity.Numeric} nor the next version {nextSkia}");
			}

			return new StableBumpInfo(
				identity.IntegrationBranch,
				bumpBranch,
				nextSkia,
				nextHarfBuzz,
				status,
				pullRequest?.Url,
				$"Bump to the next version ({nextSkia}) after release");
		}

		private async Task<VersionState> ReadVersionStateAsync(
			string reference,
			CancellationToken cancellationToken)
		{
			var variables = await repository.ReadRefFileAsync(
				reference,
				VariablesPath,
				cancellationToken).ConfigureAwait(false);
			var versions = await repository.ReadRefFileAsync(
				reference,
				VersionsPath,
				cancellationToken).ConfigureAwait(false);
			return VersionStateReader.Parse(variables, versions);
		}

		private static string? LatestPrereleaseBranch(
			IReadOnlyList<string> branches,
			string numeric)
		{
			return branches
				.Select(TryParseReleaseBranch)
				.Where(candidate =>
					candidate.Identity is { Stable: false } &&
					candidate.Identity.Numeric == numeric)
				.OrderBy(candidate => candidate.Identity!.Channel == ReleaseKind.ReleaseCandidate ? 1 : 0)
				.ThenBy(candidate => candidate.Identity!.Iteration)
				.Select(candidate => candidate.Branch)
				.LastOrDefault();
		}

		private static (string Branch, SkiaSharpReleaseIdentity? Identity) TryParseReleaseBranch(
			string branch)
		{
			try
			{
				return (branch, SkiaSharpReleaseIdentity.ParseBranch(branch));
			}
			catch (PlanException)
			{
				return (branch, null);
			}
		}

		private static IReadOnlyList<PlanOperation> BuildOperations(
			SkiaSharpReleaseIdentity identity,
			BaseSelection baseSelection,
			string skiaSha,
			RemoteState skiaState,
			RemoteState releaseState,
			StableBumpInfo? stableBump)
		{
			var operations = new List<PlanOperation>
			{
				new(
					PlanOperationId.CreateMaintenanceBranch,
					PlanOperationKind.GitRef,
					baseSelection.Action == MaintenanceBranchAction.Create
						? PlanOperationStatus.Pending
						: baseSelection.Exists
							? PlanOperationStatus.Done
							: PlanOperationStatus.Skipped,
					identity.IntegrationBranch),
				new(
					PlanOperationId.CreateSkiaRef,
					PlanOperationKind.GitHubRef,
					StatusFor(skiaState),
					$"mono/skia:{identity.ReleaseBranch}@{skiaSha}"),
				new(
					PlanOperationId.CreateReleaseBranch,
					PlanOperationKind.GitRef,
					StatusFor(releaseState),
					$"mono/SkiaSharp:{identity.ReleaseBranch}"),
			};
			if (stableBump is not null)
			{
				operations.Add(new PlanOperation(
					PlanOperationId.OpenStableBumpPullRequest,
					PlanOperationKind.GitHubPullRequest,
					stableBump.Status,
					stableBump.BumpBranch));
			}
			return operations;
		}

		private static PlanOperationStatus StatusFor(RemoteState state) => state switch
		{
			RemoteState.Matching => PlanOperationStatus.Done,
			RemoteState.Missing => PlanOperationStatus.Pending,
			RemoteState.Conflict => PlanOperationStatus.Blocked,
			_ => throw new ArgumentOutOfRangeException(nameof(state)),
		};

		private static PrepareNextAction NextAction(IReadOnlyList<PlanOperation> operations)
		{
			var statuses = operations.Select(static operation => operation.Status).ToHashSet();
			if (statuses.Contains(PlanOperationStatus.Blocked))
				return PrepareNextAction.Blocked;
			if (statuses.Contains(PlanOperationStatus.Pending))
				return PrepareNextAction.Apply;
			if (statuses.Contains(PlanOperationStatus.AwaitingUser))
				return PrepareNextAction.AwaitMerge;
			return PrepareNextAction.Done;
		}

		private sealed record BaseSelection(
			string Ref,
			string Sha,
			bool Exists,
			MaintenanceBranchAction Action,
			string? MaintenanceBaseRef,
			string? MaintenanceBaseSha);
	}
}
