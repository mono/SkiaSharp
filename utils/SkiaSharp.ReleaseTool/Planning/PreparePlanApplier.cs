using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Model;

namespace SkiaSharp.ReleaseTool.Planning
{
	public sealed class PreparePlanApplier
	{
		private readonly IReleaseRepository repository;
		private readonly IPrepareGitHubClient github;

		public PreparePlanApplier(
			IReleaseRepository repository,
			IPrepareGitHubClient github)
		{
			this.repository = repository;
			this.github = github;
		}

		public async Task<PrepareApplyResult> ApplyAsync(
			PreparePlan plan,
			Guid expectedPlanId,
			CancellationToken cancellationToken = default,
			IReadOnlyList<string>? allowedUntrackedPaths = null)
		{
			PreparePlanValidator.Validate(plan);
			if (plan.PlanId != expectedPlanId)
				throw new ValidationException($"planId '{plan.PlanId}' does not match expected correlation id '{expectedPlanId}'");

			await repository.FetchAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
			await repository.RequireCleanAsync(
				allowedUntrackedPaths,
				cancellationToken).ConfigureAwait(false);
			await RevalidatePlanBindingAsync(plan, cancellationToken).ConfigureAwait(false);

			var operations = new List<PrepareApplyOperationResult>();
			operations.Add(await ApplyMaintenanceBranchAsync(plan, cancellationToken).ConfigureAwait(false));
			operations.Add(await ApplySkiaRefAsync(plan, cancellationToken).ConfigureAwait(false));
			operations.Add(await ApplyReleaseBranchAsync(plan, cancellationToken).ConfigureAwait(false));

			Uri? pullRequestUrl = null;
			var nextAction = PrepareNextAction.Done;
			if (plan.StableBump is { } stableBump)
			{
				var stableResult = await ApplyStableBumpAsync(
					plan,
					stableBump,
					cancellationToken).ConfigureAwait(false);
				operations.Add(stableResult.Operation);
				pullRequestUrl = stableResult.PullRequestUrl;
				nextAction = stableResult.NextAction;
			}

			var result = new PrepareApplyResult(
				SchemaVersion: 1,
				PlanId: plan.PlanId,
				ToolingSha: plan.ToolingSha,
				NextAction: nextAction,
				Release: plan.Release,
				Operations: operations,
				StableBumpPullRequestUrl: pullRequestUrl,
				Warnings: plan.Warnings);
			PrepareApplyResultValidator.Validate(result);
			return result;
		}

		private async Task RevalidatePlanBindingAsync(
			PreparePlan plan,
			CancellationToken cancellationToken)
		{
			var toolingSha = await repository.ResolveAsync(
				plan.ToolingSha,
				cancellationToken).ConfigureAwait(false);
			if (toolingSha != plan.ToolingSha)
			{
				throw new ConflictException(
					$"approved tooling commit {plan.ToolingSha} no longer resolves exactly");
			}

			if (!await repository.RefExistsAsync(plan.Base.Ref, cancellationToken).ConfigureAwait(false))
				throw new ConflictException($"approved base ref {plan.Base.Ref} no longer exists");
			var liveBaseSha = await repository.ResolveAsync(
				plan.Base.Ref,
				cancellationToken).ConfigureAwait(false);
			if (liveBaseSha != plan.Base.Sha)
			{
				throw new ConflictException(
					$"base ref {plan.Base.Ref} moved from {plan.Base.Sha} to {liveBaseSha}; regenerate the plan");
			}

			if (plan.MaintenanceBranch.Action == MaintenanceBranchAction.Create)
			{
				var state = await ReadVersionStateAsync(
					plan.MaintenanceBranch.BaseSha!,
					cancellationToken).ConfigureAwait(false);
				var identity = SkiaSharpReleaseIdentity.Parse(plan.Release.Identity);
				if (state.Skia.ToNormalizedString() != identity.Numeric ||
					state.Label != "preview.0")
				{
					throw new ConflictException(
						$"maintenance create point {plan.MaintenanceBranch.BaseSha} is not {identity.Numeric} preview.0");
				}
			}
		}

		private async Task<PrepareApplyOperationResult> ApplyMaintenanceBranchAsync(
			PreparePlan plan,
			CancellationToken cancellationToken)
		{
			var maintenance = plan.MaintenanceBranch;
			if (maintenance.Action == MaintenanceBranchAction.None)
			{
				return new PrepareApplyOperationResult(
					PlanOperationId.CreateMaintenanceBranch,
					maintenance.Exists ? ApplyOperationStatus.Done : ApplyOperationStatus.Skipped,
					null);
			}

			var existing = await repository.RemoteShaAsync(
				maintenance.Name,
				cancellationToken: cancellationToken).ConfigureAwait(false);
			if (existing is not null && existing != maintenance.BaseSha)
			{
				throw new ConflictException(
					$"maintenance branch {maintenance.Name} already exists at {existing}, expected {maintenance.BaseSha}");
			}
			if (existing is null)
			{
				var currentBranch = await repository.CurrentBranchAsync(
					cancellationToken).ConfigureAwait(false);
				if (currentBranch == maintenance.Name)
				{
					var currentSha = await repository.ResolveAsync(
						"HEAD",
						cancellationToken).ConfigureAwait(false);
					if (currentSha != maintenance.BaseSha)
					{
						throw new ConflictException(
							$"checked-out maintenance branch {maintenance.Name} is at {currentSha}, expected {maintenance.BaseSha}");
					}
				}
				else
				{
					await repository.UpdateLocalBranchAsync(
						maintenance.Name,
						maintenance.BaseSha!,
						cancellationToken).ConfigureAwait(false);
				}
				try
				{
					await repository.PushBranchAsync(
						maintenance.Name,
						cancellationToken: cancellationToken).ConfigureAwait(false);
				}
				catch (ReleaseToolException)
				{
					var raced = await repository.RemoteShaAsync(
						maintenance.Name,
						cancellationToken: cancellationToken).ConfigureAwait(false);
					if (raced != maintenance.BaseSha)
						throw;
				}
			}

			return new PrepareApplyOperationResult(
				PlanOperationId.CreateMaintenanceBranch,
				ApplyOperationStatus.Done,
				null);
		}

		private async Task<PrepareApplyOperationResult> ApplySkiaRefAsync(
			PreparePlan plan,
			CancellationToken cancellationToken)
		{
			var reference = $"refs/heads/{plan.Release.Branch}";
			var existing = await github.GetRefShaAsync(
				"mono/skia",
				reference,
				cancellationToken).ConfigureAwait(false);
			if (existing is null)
			{
				await github.CreateRefAsync(
					"mono/skia",
					reference,
					plan.Skia.Sha,
					cancellationToken).ConfigureAwait(false);
				existing = await github.GetRefShaAsync(
					"mono/skia",
					reference,
					cancellationToken).ConfigureAwait(false);
			}
			if (existing != plan.Skia.Sha)
			{
				throw new ConflictException(
					$"mono/skia branch {plan.Release.Branch} already exists at {existing}, expected {plan.Skia.Sha}");
			}

			return new PrepareApplyOperationResult(
				PlanOperationId.CreateSkiaRef,
				ApplyOperationStatus.Done,
				null);
		}

		private async Task<PrepareApplyOperationResult> ApplyReleaseBranchAsync(
			PreparePlan plan,
			CancellationToken cancellationToken)
		{
			var remoteSha = await repository.RemoteShaAsync(
				plan.Release.Branch,
				cancellationToken: cancellationToken).ConfigureAwait(false);
			if (remoteSha is not null)
			{
				await VerifyReleaseBranchAsync(
					plan,
					remoteSha,
					cancellationToken).ConfigureAwait(false);
				return Done(PlanOperationId.CreateReleaseBranch);
			}

			var localRef = $"refs/heads/{plan.Release.Branch}";
			if (await repository.RefExistsAsync(localRef, cancellationToken).ConfigureAwait(false))
			{
				var localSha = await repository.ResolveAsync(
					localRef,
					cancellationToken).ConfigureAwait(false);
				if (localSha != plan.Base.Sha)
				{
					await VerifyReleaseBranchAsync(
						plan,
						localSha,
						cancellationToken).ConfigureAwait(false);
					await PushReleaseBranchWithRaceRecoveryAsync(
						plan,
						cancellationToken).ConfigureAwait(false);
					return Done(PlanOperationId.CreateReleaseBranch);
				}
				await repository.SwitchAsync(
					plan.Release.Branch,
					cancellationToken).ConfigureAwait(false);
			}
			else
			{
				await repository.SwitchCreateAsync(
					plan.Release.Branch,
					plan.Base.Sha,
					cancellationToken).ConfigureAwait(false);
			}

			var identity = SkiaSharpReleaseIdentity.Parse(plan.Release.Identity);
			var baseState = await ReadVersionStateAsync(
				plan.Base.Ref,
				cancellationToken).ConfigureAwait(false);
			await EditCommitAndPushAsync(
				identity.Label,
				plan.Versions.RequiresPackageBump ? identity.Numeric : null,
				plan.Versions.RequiresPackageBump ? baseState.HarfBuzz.ToNormalizedString() : null,
				$"Bump the version to {identity.Raw}\n\n" +
					$"Release-Base: {plan.Base.Sha}\n" +
					$"Release-Skia: {plan.Skia.Sha}\n",
				async token => await PushReleaseBranchWithRaceRecoveryAsync(plan, token).ConfigureAwait(false),
				cancellationToken).ConfigureAwait(false);
			return Done(PlanOperationId.CreateReleaseBranch);
		}

		private async Task PushReleaseBranchWithRaceRecoveryAsync(
			PreparePlan plan,
			CancellationToken cancellationToken)
		{
			try
			{
				await repository.PushBranchAsync(
					plan.Release.Branch,
					cancellationToken: cancellationToken).ConfigureAwait(false);
			}
			catch (ReleaseToolException)
			{
				var raced = await repository.RemoteShaAsync(
					plan.Release.Branch,
					cancellationToken: cancellationToken).ConfigureAwait(false);
				if (raced is null)
					throw;
				await VerifyReleaseBranchAsync(plan, raced, cancellationToken).ConfigureAwait(false);
			}
		}

		private async Task VerifyReleaseBranchAsync(
			PreparePlan plan,
			string sha,
			CancellationToken cancellationToken)
		{
			if (!await repository.IsAncestorAsync(
				plan.Base.Sha,
				sha,
				cancellationToken).ConfigureAwait(false))
			{
				throw new ConflictException(
					$"existing release branch {plan.Release.Branch} ({sha}) is not a descendant of approved base {plan.Base.Sha}");
			}
			var identity = SkiaSharpReleaseIdentity.Parse(plan.Release.Identity);
			var state = await ReadVersionStateAsync(sha, cancellationToken).ConfigureAwait(false);
			if (state.Label != identity.Label ||
				state.Skia.ToNormalizedString() != identity.Numeric)
			{
				throw new ConflictException(
					$"existing release branch {plan.Release.Branch} has version state {state.Skia}/{state.Label}, expected {identity.Numeric}/{identity.Label}");
			}
		}

		private async Task<StableBumpApplyResult> ApplyStableBumpAsync(
			PreparePlan plan,
			StableBumpInfo stableBump,
			CancellationToken cancellationToken)
		{
			var integrationRef = $"refs/remotes/origin/{stableBump.IntegrationBranch}";
			if (!await repository.RefExistsAsync(integrationRef, cancellationToken).ConfigureAwait(false))
				throw new ConflictException($"integration branch {stableBump.IntegrationBranch} does not exist");
			var integrationSha = await repository.ResolveAsync(
				integrationRef,
				cancellationToken).ConfigureAwait(false);
			var integrationState = await ReadVersionStateAsync(
				integrationSha,
				cancellationToken).ConfigureAwait(false);
			var targetSkia = NuGetVersion.Parse(stableBump.SkiaSharpVersion);

			var comparison = VersionComparer.VersionRelease.Compare(
				integrationState.Skia,
				targetSkia);
			if (comparison >= 0)
			{
				if (integrationState.Label != "preview.0" ||
					(comparison == 0 &&
						integrationState.HarfBuzz.ToNormalizedString() != stableBump.HarfBuzzSharpVersion))
				{
					throw new ConflictException("advanced integration branch does not match the stable bump plan");
				}
				return new StableBumpApplyResult(
					Done(PlanOperationId.OpenStableBumpPullRequest),
					null,
					PrepareNextAction.Done);
			}

			var identity = SkiaSharpReleaseIdentity.Parse(plan.Release.Identity);
			if (integrationState.Label != "preview.0" ||
				integrationState.Skia.ToNormalizedString() != identity.Numeric)
			{
				throw new ConflictException(
					$"integration branch {stableBump.IntegrationBranch} moved to {integrationState.Skia}/{integrationState.Label}");
			}

			var remoteSha = await repository.RemoteShaAsync(
				stableBump.BumpBranch,
				cancellationToken: cancellationToken).ConfigureAwait(false);
			if (remoteSha is null)
			{
				await CreateOrReuseLocalBumpBranchAsync(
					stableBump,
					integrationRef,
					integrationSha,
					cancellationToken).ConfigureAwait(false);
			}
			else
			{
				await VerifyBumpBranchAsync(
					stableBump,
					integrationSha,
					remoteSha,
					cancellationToken).ConfigureAwait(false);
			}

			var pullRequest = await github.FindOpenPullRequestAsync(
				stableBump.BumpBranch,
				stableBump.IntegrationBranch,
				cancellationToken).ConfigureAwait(false)
				?? await github.CreatePullRequestAsync(
					stableBump.BumpBranch,
					stableBump.IntegrationBranch,
					stableBump.Title,
					StablePullRequestBody(identity.Raw, stableBump),
					cancellationToken).ConfigureAwait(false);
			var operation = new PrepareApplyOperationResult(
				PlanOperationId.OpenStableBumpPullRequest,
				ApplyOperationStatus.Done,
				pullRequest.Url);
			return new StableBumpApplyResult(
				operation,
				pullRequest.Url,
				PrepareNextAction.AwaitMerge);
		}

		private async Task CreateOrReuseLocalBumpBranchAsync(
			StableBumpInfo stableBump,
			string integrationRef,
			string integrationSha,
			CancellationToken cancellationToken)
		{
			var localRef = $"refs/heads/{stableBump.BumpBranch}";
			if (await repository.RefExistsAsync(localRef, cancellationToken).ConfigureAwait(false))
			{
				var localSha = await repository.ResolveAsync(
					localRef,
					cancellationToken).ConfigureAwait(false);
				if (localSha != integrationSha)
				{
					await VerifyBumpBranchAsync(
						stableBump,
						integrationSha,
						localSha,
						cancellationToken).ConfigureAwait(false);
					await PushBumpBranchWithRaceRecoveryAsync(
						stableBump,
						integrationSha,
						cancellationToken).ConfigureAwait(false);
					return;
				}
				await repository.SwitchAsync(
					stableBump.BumpBranch,
					cancellationToken).ConfigureAwait(false);
			}
			else
			{
				await repository.SwitchCreateAsync(
					stableBump.BumpBranch,
					integrationRef,
					cancellationToken).ConfigureAwait(false);
			}

			await EditCommitAndPushAsync(
				"preview.0",
				stableBump.SkiaSharpVersion,
				stableBump.HarfBuzzSharpVersion,
				stableBump.Title,
				async token => await PushBumpBranchWithRaceRecoveryAsync(
					stableBump,
					integrationSha,
					token).ConfigureAwait(false),
				cancellationToken).ConfigureAwait(false);
		}

		private async Task PushBumpBranchWithRaceRecoveryAsync(
			StableBumpInfo stableBump,
			string integrationSha,
			CancellationToken cancellationToken)
		{
			try
			{
				await repository.PushBranchAsync(
					stableBump.BumpBranch,
					cancellationToken: cancellationToken).ConfigureAwait(false);
			}
			catch (ReleaseToolException)
			{
				var raced = await repository.RemoteShaAsync(
					stableBump.BumpBranch,
					cancellationToken: cancellationToken).ConfigureAwait(false);
				if (raced is null)
					throw;
				await VerifyBumpBranchAsync(
					stableBump,
					integrationSha,
					raced,
					cancellationToken).ConfigureAwait(false);
			}
		}

		private async Task VerifyBumpBranchAsync(
			StableBumpInfo stableBump,
			string integrationSha,
			string bumpSha,
			CancellationToken cancellationToken)
		{
			if (!await repository.IsAncestorAsync(
				integrationSha,
				bumpSha,
				cancellationToken).ConfigureAwait(false))
			{
				throw new ConflictException(
					$"existing bump branch {stableBump.BumpBranch} is not descended from integration");
			}
			var state = await ReadVersionStateAsync(
				bumpSha,
				cancellationToken).ConfigureAwait(false);
			if (state.Label != "preview.0" ||
				state.Skia.ToNormalizedString() != stableBump.SkiaSharpVersion ||
				state.HarfBuzz.ToNormalizedString() != stableBump.HarfBuzzSharpVersion)
			{
				throw new ConflictException(
					$"existing bump branch {stableBump.BumpBranch} has stale version state");
			}
		}

		private async Task EditCommitAndPushAsync(
			string previewLabel,
			string? skiaVersion,
			string? harfBuzzVersion,
			string message,
			Func<CancellationToken, Task> push,
			CancellationToken cancellationToken)
		{
			var variables = await repository.ReadWorktreeFileAsync(
				VersionFileEditor.VariablesPath,
				cancellationToken).ConfigureAwait(false);
			var versions = await repository.ReadWorktreeFileAsync(
				VersionFileEditor.VersionsPath,
				cancellationToken).ConfigureAwait(false);
			var edits = VersionFileEditor.ComputeEdits(
				variables,
				versions,
				previewLabel,
				skiaVersion,
				harfBuzzVersion);
			if (edits.ChangedPaths.Contains(VersionFileEditor.VariablesPath))
			{
				await repository.WriteWorktreeFileAsync(
					VersionFileEditor.VariablesPath,
					edits.NewVariablesText,
					cancellationToken).ConfigureAwait(false);
			}
			if (edits.ChangedPaths.Contains(VersionFileEditor.VersionsPath))
			{
				await repository.WriteWorktreeFileAsync(
					VersionFileEditor.VersionsPath,
					edits.NewVersionsText,
					cancellationToken).ConfigureAwait(false);
			}
			_ = await repository.CommitAsync(
				message,
				edits.ChangedPaths,
				cancellationToken).ConfigureAwait(false);
			await push(cancellationToken).ConfigureAwait(false);
		}

		private async Task<VersionState> ReadVersionStateAsync(
			string reference,
			CancellationToken cancellationToken)
		{
			var variables = await repository.ReadRefFileAsync(
				reference,
				VersionFileEditor.VariablesPath,
				cancellationToken).ConfigureAwait(false);
			var versions = await repository.ReadRefFileAsync(
				reference,
				VersionFileEditor.VersionsPath,
				cancellationToken).ConfigureAwait(false);
			return VersionStateReader.Parse(variables, versions);
		}

		private static PrepareApplyOperationResult Done(PlanOperationId id) =>
			new(id, ApplyOperationStatus.Done, null);

		private static string StablePullRequestBody(
			string releasedVersion,
			StableBumpInfo bump) =>
			$"""
			Advance `{bump.IntegrationBranch}` after cutting {releasedVersion}.

			## Description

			Bumps the integration branch back to `preview.0` at SkiaSharp {bump.SkiaSharpVersion} / HarfBuzzSharp {bump.HarfBuzzSharpVersion} so the next preview starts from a clean baseline.

			Related issues: N/A

			Required skia PR: None.

			## Changes

			None - version metadata only (`scripts/azure-templates-variables.yml`, `scripts/VERSIONS.txt`).

			## Testing

			The release automation validated this exact version transform before opening this pull request.

			## Areas Affected

			- [x] Build/infra
			""";

		private sealed record StableBumpApplyResult(
			PrepareApplyOperationResult Operation,
			Uri? PullRequestUrl,
			PrepareNextAction NextAction);
	}
}
