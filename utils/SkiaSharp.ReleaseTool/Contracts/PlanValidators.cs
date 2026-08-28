using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Git;
using SkiaSharp.ReleaseTool.Model;

namespace SkiaSharp.ReleaseTool.Contracts
{
	public static class PreparePlanValidator
	{
		public static void Validate(PreparePlan plan)
		{
			try
			{
				ValidateCore(plan);
			}
			catch (PlanException ex)
			{
				throw new ValidationException(ex.Message, ex);
			}
		}

		private static void ValidateCore(PreparePlan plan)
		{
			PlanValidation.ValidateHeader(
				plan.SchemaVersion,
				plan.Operation,
				ReleaseOperation.Prepare,
				plan.PlanId,
				plan.GeneratedAt,
				plan.ToolingSha);

			var identity = SkiaSharpReleaseIdentity.Parse(plan.Release.Identity);
			PlanValidation.Require(plan.Release.Version == identity.Raw, "release.version must equal release.identity");
			PlanValidation.Require(plan.Release.Branch == identity.ReleaseBranch, "release.branch does not match release.identity");

			var normalizedTarget = ReleaseVersionPolicy.NormalizeIntegrationBranch(plan.Input.IntegrationTarget);
			PlanValidation.Require(
				plan.Input.IntegrationTarget == normalizedTarget,
				"input.integrationTarget must already be normalized");
			PlanValidation.Require(
				normalizedTarget == "main" || normalizedTarget == identity.IntegrationBranch,
				"input.integrationTarget must be main or release.integrationBranch");
			if (plan.Input.RequestedVersion is not null)
				PlanValidation.Require(plan.Input.RequestedVersion == identity.Raw, "input.requestedVersion does not match release.identity");
			if (plan.Input.ApprovedBase is not null)
			{
				PlanValidation.Require(
					GitReferencePolicy.IsFullyQualified(plan.Input.ApprovedBase),
					"input.approvedBase must be a fully-qualified, well-formed refs/... name");
			}

			PlanValidation.Require(!string.IsNullOrWhiteSpace(plan.Base.Ref), "base.ref must not be empty");
			ValidateBaseRef(plan, identity);
			PlanValidation.ValidateSha(plan.Base.Sha, "base.sha");
			PlanValidation.ValidateSha(plan.Skia.Sha, "skia.sha");

			PlanValidation.Require(
				plan.MaintenanceBranch.Name == identity.IntegrationBranch,
				"maintenanceBranch.name does not match release.integrationBranch");
			if (plan.MaintenanceBranch.BaseSha is not null)
				PlanValidation.ValidateSha(plan.MaintenanceBranch.BaseSha, "maintenanceBranch.baseSha");
			if (plan.MaintenanceBranch.Action == MaintenanceBranchAction.Create)
			{
				PlanValidation.Require(!plan.MaintenanceBranch.Exists, "a maintenance branch marked for creation cannot already exist");
				PlanValidation.Require(
					plan.MaintenanceBranch.BaseSha is not null,
					"maintenanceBranch.baseSha is required when creating the branch");
			}
			else
			{
				PlanValidation.Require(
					plan.MaintenanceBranch.Exists || identity.IsHotfix,
					"maintenanceBranch.action 'none' requires an existing branch except for hotfix releases");
				PlanValidation.Require(
					plan.MaintenanceBranch.BaseSha is null,
					"maintenanceBranch.baseSha must be null when action is 'none'");
			}

			ValidateOperations(plan);
			ValidateStableBump(plan, identity);
			PlanValidation.ValidateStrings(plan.Warnings, "warnings");
		}

		private static void ValidateOperations(PreparePlan plan)
		{
			var planOperations = plan.Operations
				?? throw new ValidationException("operations must not be null");
			var operations = new Dictionary<PlanOperationId, PlanOperation>();
			foreach (var operation in planOperations)
			{
				if (operation is null)
					throw new ValidationException("operations must not contain null values");
				PlanValidation.Require(
					operations.TryAdd(operation.Id, operation),
					$"operations contains duplicate id '{operation.Id}'");
				var expectedKind = operation.Id switch
				{
					PlanOperationId.CreateMaintenanceBranch or PlanOperationId.CreateReleaseBranch => PlanOperationKind.GitRef,
					PlanOperationId.CreateSkiaRef => PlanOperationKind.GitHubRef,
					PlanOperationId.OpenStableBumpPullRequest => PlanOperationKind.GitHubPullRequest,
					_ => throw new ValidationException($"unsupported operation id '{operation.Id}'"),
				};
				PlanValidation.Require(operation.Kind == expectedKind, $"operation '{operation.Id}' has the wrong kind");
			}

			var expectedStatuses = new Dictionary<PlanOperationId, PlanOperationStatus>
			{
				[PlanOperationId.CreateMaintenanceBranch] =
					plan.MaintenanceBranch.Action == MaintenanceBranchAction.Create
						? PlanOperationStatus.Pending
						: plan.MaintenanceBranch.Exists
							? PlanOperationStatus.Done
							: PlanOperationStatus.Skipped,
				[PlanOperationId.CreateSkiaRef] = StatusFor(plan.Skia.RemoteState),
				[PlanOperationId.CreateReleaseBranch] = StatusFor(plan.SkiaSharpRemoteState),
			};
			if (plan.StableBump is not null)
				expectedStatuses[PlanOperationId.OpenStableBumpPullRequest] = plan.StableBump.Status;

			PlanValidation.Require(
				operations.Count == expectedStatuses.Count,
				"operations must contain exactly the steps represented by the plan");
			foreach (var expected in expectedStatuses)
			{
				if (!operations.TryGetValue(expected.Key, out var operation))
					throw new ValidationException($"operations is missing '{expected.Key}'");
				PlanValidation.Require(
					operation.Status == expected.Value,
					$"operation '{expected.Key}' status is inconsistent");
			}

			var statuses = planOperations.Select(static operation => operation.Status).ToHashSet();
			var expectedAction = statuses.Contains(PlanOperationStatus.Blocked)
				? PrepareNextAction.Blocked
				: statuses.Contains(PlanOperationStatus.Pending)
					? PrepareNextAction.Apply
					: statuses.Contains(PlanOperationStatus.AwaitingUser)
						? PrepareNextAction.AwaitMerge
						: PrepareNextAction.Done;
			PlanValidation.Require(plan.NextAction == expectedAction, "nextAction does not match operation statuses");
		}

		private static PlanOperationStatus StatusFor(RemoteState state) => state switch
		{
			RemoteState.Matching => PlanOperationStatus.Done,
			RemoteState.Missing => PlanOperationStatus.Pending,
			RemoteState.Conflict => PlanOperationStatus.Blocked,
			_ => throw new ValidationException($"unsupported remote state '{state}'"),
		};

		private static void ValidateBaseRef(
			PreparePlan plan,
			SkiaSharpReleaseIdentity identity)
		{
			var baseRef = plan.Base.Ref;
			if (plan.Input.ApprovedBase == baseRef &&
				plan.MaintenanceBranch.Action == MaintenanceBranchAction.Create)
			{
				PlanValidation.Require(
					!plan.MaintenanceBranch.Exists,
					"approved base recovery requires maintenance branch creation");
				return;
			}

			const string tagPrefix = "refs/tags/";
			if (baseRef.StartsWith(tagPrefix, StringComparison.Ordinal))
			{
				PlanValidation.Require(
					identity.IsHotfix && !identity.Stable,
					"base.ref may be a tag only for a hotfix prerelease");
				SkiaSharpReleaseIdentity parent;
				try
				{
					parent = SkiaSharpReleaseIdentity.ParseTag(baseRef[tagPrefix.Length..]);
				}
				catch (PlanException ex)
				{
					throw new ValidationException("base.ref hotfix tag is invalid", ex);
				}
				var expectedParent = identity.Version.Version.ToString(3);
				PlanValidation.Require(
					parent.Stable &&
					!parent.IsHotfix &&
					parent.Numeric == expectedParent &&
					baseRef == $"refs/tags/v{expectedParent}",
					"base.ref must be the exact stable three-part parent tag for a hotfix prerelease");
				return;
			}

			const string prefix = "refs/remotes/origin/";
			PlanValidation.Require(
				baseRef.StartsWith(prefix, StringComparison.Ordinal),
				"base.ref must be an origin remote ref or the approved recovery ref");
			var branch = baseRef[prefix.Length..];
			if (branch == "main" || branch == identity.IntegrationBranch)
				return;

			SkiaSharpReleaseIdentity previous;
			try
			{
				previous = SkiaSharpReleaseIdentity.ParseBranch(branch);
			}
			catch (PlanException ex)
			{
				throw new ValidationException("base.ref must identify main, the maintenance line, or an earlier release", ex);
			}
			PlanValidation.Require(
				previous.Numeric == identity.Numeric &&
				(previous.CompareTo(identity) < 0 ||
					(previous.Raw == identity.Raw &&
						plan.SkiaSharpRemoteState == RemoteState.Matching)),
				"base.ref recovery release must precede or be the matching requested release on the same numeric version");
		}

		private static void ValidateStableBump(PreparePlan plan, SkiaSharpReleaseIdentity identity)
		{
			var requiresStableBump = identity.Stable && !identity.IsHotfix;
			PlanValidation.Require(
				(plan.StableBump is not null) == requiresStableBump,
				"stableBump must be present only for a non-hotfix stable release");
			if (plan.StableBump is not { } bump)
				return;

			if (identity.Version.Patch == int.MaxValue)
				throw new ValidationException("stableBump.skiaSharpVersion cannot advance because the patch is at its maximum value");
			var nextSkia = new Version(
				identity.Version.Major,
				identity.Version.Minor,
				identity.Version.Patch + 1).ToString(3);
			PlanValidation.Require(bump.IntegrationBranch == identity.IntegrationBranch, "stableBump.integrationBranch is inconsistent");
			PlanValidation.Require(bump.SkiaSharpVersion == nextSkia, "stableBump.skiaSharpVersion is not the next patch");
			ReleaseVersionPolicy.ParseStableVersion(bump.HarfBuzzSharpVersion, "stableBump.harfBuzzSharpVersion", 3, 4);
			PlanValidation.Require(bump.BumpBranch == $"bump-version-{nextSkia}", "stableBump.bumpBranch is inconsistent");
			PlanValidation.Require(
				bump.Title == $"Bump to the next version ({nextSkia}) after release",
				"stableBump.title is inconsistent");
			PlanValidation.Require(
				bump.Status is PlanOperationStatus.Done or PlanOperationStatus.Pending or PlanOperationStatus.AwaitingUser,
				"stableBump.status is invalid");
			if (bump.Status == PlanOperationStatus.AwaitingUser)
				PlanValidation.Require(bump.PullRequestUrl is not null, "stableBump.pullRequestUrl is required while awaiting a user");
			if (bump.PullRequestUrl is not null)
				PlanValidation.Require(bump.PullRequestUrl.IsAbsoluteUri, "stableBump.pullRequestUrl must be absolute");
		}
	}

	public static class PrepareApplyResultValidator
	{
		public static void Validate(PrepareApplyResult result)
		{
			PlanValidation.Require(result.SchemaVersion == 1, "schemaVersion must be 1");
			PlanValidation.Require(result.PlanId != Guid.Empty, "planId must not be empty");
			PlanValidation.ValidateSha(result.ToolingSha, "toolingSha");
			PlanValidation.Require(
				result.NextAction is PrepareNextAction.Done or PrepareNextAction.AwaitMerge,
				"nextAction must be done or await-merge");

			var identity = SkiaSharpReleaseIdentity.Parse(result.Release.Identity);
			PlanValidation.Require(result.Release.Version == identity.Raw, "release.version must equal release.identity");
			PlanValidation.Require(result.Release.Branch == identity.ReleaseBranch, "release.branch does not match release.identity");

			var operations = result.Operations
				?? throw new ValidationException("operations must not be null");
			var ids = new HashSet<PlanOperationId>();
			foreach (var operation in operations)
			{
				if (operation is null)
					throw new ValidationException("operations must not contain null values");
				PlanValidation.Require(ids.Add(operation.Id), $"operations contains duplicate id '{operation.Id}'");
				if (operation.Id != PlanOperationId.OpenStableBumpPullRequest)
				{
					PlanValidation.Require(
						operation.PullRequestUrl is null,
						$"operation '{operation.Id}' cannot carry a pull request URL");
				}
				if (operation.PullRequestUrl is not null)
					PlanValidation.Require(operation.PullRequestUrl.IsAbsoluteUri, "pull request URL must be absolute");
			}
			foreach (var required in new[]
			{
				PlanOperationId.CreateMaintenanceBranch,
				PlanOperationId.CreateSkiaRef,
				PlanOperationId.CreateReleaseBranch,
			})
			{
				PlanValidation.Require(ids.Contains(required), $"operations is missing '{required}'");
			}

			var stableOperation = operations.SingleOrDefault(
				static operation => operation.Id == PlanOperationId.OpenStableBumpPullRequest);
			PlanValidation.Require(
				result.StableBumpPullRequestUrl == stableOperation?.PullRequestUrl,
				"stableBumpPullRequestUrl does not match the operation result");
			PlanValidation.Require(
				(result.NextAction == PrepareNextAction.AwaitMerge) ==
					(result.StableBumpPullRequestUrl is not null),
				"await-merge requires a stable bump pull request URL");
			if (result.StableBumpPullRequestUrl is not null)
				PlanValidation.Require(result.StableBumpPullRequestUrl.IsAbsoluteUri, "stable bump pull request URL must be absolute");
			PlanValidation.ValidateStrings(result.Warnings, "warnings");
		}
	}

	public static class FinishPlanValidator
	{
		public static void Validate(FinishPlan plan)
		{
			try
			{
				ValidateCore(plan);
			}
			catch (PlanException ex)
			{
				throw new ValidationException(ex.Message, ex);
			}
		}

		private static void ValidateCore(FinishPlan plan)
		{
			PlanValidation.ValidateHeader(
				plan.SchemaVersion,
				plan.Operation,
				ReleaseOperation.Finish,
				plan.PlanId,
				plan.GeneratedAt,
				plan.ToolingSha);
			var requested = PublicReleaseVersion.Parse(plan.Input.RequestedVersion);
			var identity = requested.Identity;
			PlanValidation.Require(plan.Receipt.SkiaSharpVersion == requested.Text, "receipt.skiaSharpVersion must match input.requestedVersion");
			PlanValidation.Require(plan.Receipt.Base == requested.Base, "receipt.base does not match the public version");
			PlanValidation.Require(plan.Receipt.Label == identity.Label, "receipt.label does not match the release identity");
			PlanValidation.Require(plan.Receipt.BuildRevision == requested.BuildRevision, "receipt.buildRevision does not match the public version");
			PlanValidation.ValidateSha(plan.Receipt.SourceCommit, "receipt.sourceCommit");
			PlanValidation.Require(
				plan.Receipt.SourceBranch == identity.ReleaseBranch,
				"receipt.sourceBranch must be the exact release branch");
			_ = NuGetVersion.Parse(plan.Receipt.HarfBuzzSharpVersion);

			var packageIds = new HashSet<string>(StringComparer.Ordinal);
			(string Commit, string Branch)? harfBuzzSource = null;
			foreach (var package in plan.Receipt.Packages ??
				throw new ValidationException("receipt.packages must not be null"))
			{
				PlanValidation.Require(!string.IsNullOrWhiteSpace(package.Id), "receipt package id must not be empty");
				PlanValidation.Require(packageIds.Add(package.Id), $"receipt contains duplicate package '{package.Id}'");
				_ = NuGetVersion.Parse(package.Version);
				PlanValidation.ValidateSha(package.SourceCommit, $"receipt package {package.Id} sourceCommit");
				_ = SkiaSharpReleaseIdentity.ParseBranch(package.SourceBranch);
				if (package.Id.StartsWith("SkiaSharp", StringComparison.Ordinal))
				{
					PlanValidation.Require(package.Version == requested.Text, $"{package.Id} version is inconsistent");
					PlanValidation.Require(package.SourceCommit == plan.Receipt.SourceCommit, $"{package.Id} sourceCommit is inconsistent");
					PlanValidation.Require(package.SourceBranch == plan.Receipt.SourceBranch, $"{package.Id} sourceBranch is inconsistent");
				}
				else if (package.Id.StartsWith("HarfBuzzSharp", StringComparison.Ordinal))
				{
					PlanValidation.Require(package.Version == plan.Receipt.HarfBuzzSharpVersion, $"{package.Id} version is inconsistent");
					harfBuzzSource ??= (package.SourceCommit, package.SourceBranch);
					PlanValidation.Require(
						harfBuzzSource == (package.SourceCommit, package.SourceBranch),
						"HarfBuzzSharp package source commit and branch must be family-consistent");
				}
				else
				{
					throw new ValidationException($"receipt contains unsupported package family member '{package.Id}'");
				}
			}
			foreach (var anchor in new[] { "SkiaSharp", "SkiaSharp.HarfBuzz", "HarfBuzzSharp" })
				PlanValidation.Require(packageIds.Contains(anchor), $"receipt is missing anchor '{anchor}'");

			PlanValidation.Require(plan.Release.Identity == identity.Raw, "release.identity is inconsistent");
			PlanValidation.Require(plan.Release.Version == requested.Text, "release.version is inconsistent");
			PlanValidation.Require(plan.Release.Branch == plan.Receipt.SourceBranch, "release.branch is inconsistent");
			PlanValidation.Require(plan.Release.Raw == identity.Raw, "release.raw is inconsistent");
			PlanValidation.Require(plan.Release.Numeric == identity.Numeric, "release.numeric is inconsistent");
			PlanValidation.Require(plan.Release.Label == identity.Label, "release.label is inconsistent");
			PlanValidation.Require(plan.Release.ReleaseType == identity.ReleaseType, "release.releaseType is inconsistent");
			PlanValidation.Require(plan.Release.Stable == identity.Stable, "release.stable is inconsistent");
			PlanValidation.Require(plan.Release.Title == identity.Title, "release.title is inconsistent");
			PlanValidation.Require(plan.Release.Tag == identity.Tag, "release.tag is inconsistent");

			PlanValidation.Require(plan.Tag.Name == identity.Tag, "tag.name is inconsistent");
			PlanValidation.Require(plan.Tag.TargetCommit == plan.Receipt.SourceCommit, "tag.targetCommit is inconsistent");
			if (plan.Tag.ExistingSha is not null)
				PlanValidation.ValidateSha(plan.Tag.ExistingSha, "tag.existingSha");
			PlanValidation.Require(
				(plan.Tag.Status == FinishState.Done) ==
					(plan.Tag.ExistingSha == plan.Tag.TargetCommit),
				"tag.status does not match existingSha");
			if (plan.Tag.Status == FinishState.Pending)
				PlanValidation.Require(plan.Tag.ExistingSha is null, "a pending tag must be absent");

			if (plan.PreviousTag is not null)
			{
				var previous = SkiaSharpReleaseIdentity.ParseTag(plan.PreviousTag);
				PlanValidation.Require(previous.CompareTo(identity) < 0, "previousTag must sort before the release");
			}

			ValidateDraft(plan);
			ValidateOperations(plan);
			PlanValidation.ValidateStrings(plan.Warnings, "warnings");
		}

		private static void ValidateDraft(FinishPlan plan)
		{
			var draft = plan.Draft;
			PlanValidation.Require(draft.IsPublished == (draft.Exists && plan.NextAction == FinishNextAction.Closeout), "draft.isPublished is inconsistent");
			if (draft.IsPublished)
				PlanValidation.Require(plan.Tag.Status == FinishState.Done, "published release requires its authoritative tag");
			PlanValidation.Require(
				(draft.Status == FinishState.Done) == draft.Exists,
				"draft.status does not match draft.exists");
			if (!draft.Exists)
			{
				PlanValidation.Require(
					draft.TargetCommitish is null && draft.Url is null && draft.Body is null,
					"absent draft cannot have target, URL, or body");
				PlanValidation.Require(draft.MarkerState == ManagedMarkerState.None, "absent draft cannot have markers");
			}
			else
			{
				PlanValidation.Require(!string.IsNullOrWhiteSpace(draft.TargetCommitish), "existing draft must include targetCommitish");
				PlanValidation.Require(draft.Url is { IsAbsoluteUri: true }, "draft.url must be absolute");
				PlanValidation.Require(draft.Body is not null, "existing draft must include its body");
				ManagedMarkerState actualMarkers;
				try
				{
					actualMarkers = Finishing.ManagedReleaseMarkers.Inspect(draft.Body!);
				}
				catch (GitHubException ex)
				{
					throw new ValidationException(ex.Message, ex);
				}
				PlanValidation.Require(actualMarkers == draft.MarkerState, "draft.markerState does not match draft.body");
				if (!draft.IsPublished)
				{
					PlanValidation.Require(
						draft.TargetCommitish == plan.Receipt.SourceCommit,
						"unpublished draft targetCommitish must be the package source commit");
				}
				else
				{
					var exactSha = draft.TargetCommitish!.Length == 40 &&
						draft.TargetCommitish.All(static character =>
							character is >= '0' and <= '9' or >= 'a' and <= 'f');
					PlanValidation.Require(
						draft.TargetCommitish == plan.Receipt.SourceCommit ||
						(!exactSha &&
							(draft.TargetCommitish is "main" ||
							 draft.TargetCommitish == plan.Receipt.SourceBranch)),
						"published release targetCommitish is not verified");
				}
			}

			var expectedAction = draft switch
			{
				{ IsPublished: true } => FinishNextAction.Closeout,
				{ Exists: true, MarkerState: ManagedMarkerState.Complete }
					when plan.Tag.Status == FinishState.Done =>
					FinishNextAction.PlanPublication,
				_ => FinishNextAction.CreateDraft,
			};
			PlanValidation.Require(plan.NextAction == expectedAction, "nextAction does not match draft state");
		}

		private static void ValidateOperations(FinishPlan plan)
		{
			var operations = plan.Operations ??
				throw new ValidationException("operations must not be null");
			var byId = new Dictionary<FinishOperationId, FinishOperation>();
			foreach (var operation in operations)
			{
				if (operation is null)
					throw new ValidationException("operations must not contain null values");
				PlanValidation.Require(byId.TryAdd(operation.Id, operation), $"operations contains duplicate id '{operation.Id}'");
				var expectedKind = operation.Id switch
				{
					FinishOperationId.CreateTag => FinishOperationKind.GitTag,
					FinishOperationId.CreateDraft or FinishOperationId.PublishRelease =>
						FinishOperationKind.GitHubRelease,
					FinishOperationId.Closeout => FinishOperationKind.ReleaseCloseout,
					_ => throw new ValidationException($"unsupported finish operation '{operation.Id}'"),
				};
				PlanValidation.Require(operation.Kind == expectedKind, $"finish operation '{operation.Id}' has wrong kind");
			}
			PlanValidation.Require(byId.Count == 4, "operations must contain exactly four finish operations");
			foreach (var id in Enum.GetValues<FinishOperationId>())
				PlanValidation.Require(byId.ContainsKey(id), $"operations is missing '{id}'");

			var published = plan.Draft.IsPublished;
			var ready = plan.Draft.Exists && plan.Draft.MarkerState == ManagedMarkerState.Complete;
			var expected = new Dictionary<FinishOperationId, PlanOperationStatus>
			{
				[FinishOperationId.CreateTag] =
					plan.Tag.Status == FinishState.Done ? PlanOperationStatus.Done : PlanOperationStatus.Pending,
				[FinishOperationId.CreateDraft] =
					published ? PlanOperationStatus.Skipped :
					ready ? PlanOperationStatus.Done : PlanOperationStatus.Pending,
				[FinishOperationId.PublishRelease] =
					published ? PlanOperationStatus.Done :
					ready ? PlanOperationStatus.Pending : PlanOperationStatus.Skipped,
				[FinishOperationId.Closeout] =
					published ? PlanOperationStatus.Pending : PlanOperationStatus.Skipped,
			};
			foreach (var pair in expected)
				PlanValidation.Require(byId[pair.Key].Status == pair.Value, $"finish operation '{pair.Key}' status is inconsistent");
		}
	}

	public static class FinishPendingReportValidator
	{
		public static void Validate(FinishPendingReport report)
		{
			PlanValidation.Require(report.SchemaVersion == 1, "schemaVersion must be 1");
			PlanValidation.Require(
				report.Operation == FinishPendingOperation.FinishPlanPending,
				"operation must be finish-plan-pending");
			PlanValidation.Require(
				report.GeneratedAt != default && report.GeneratedAt.Offset == TimeSpan.Zero,
				"generatedAt must be a UTC timestamp");
			PlanValidation.ValidateSha(report.ToolingSha, "toolingSha");
			PlanValidation.Require(report.NextAction == PendingNextAction.Pending, "nextAction must be pending");
			_ = PublicReleaseVersion.Parse(report.RequestedVersion);
			PlanValidation.Require(report.MissingPackages is { Count: > 0 }, "missingPackages must not be empty");
			foreach (var package in report.MissingPackages)
			{
				PlanValidation.Require(!string.IsNullOrWhiteSpace(package.Id), "pending package id must not be empty");
				_ = NuGetVersion.Parse(package.Version);
			}
			PlanValidation.Require(double.IsFinite(report.ElapsedSeconds) && report.ElapsedSeconds >= 0, "elapsedSeconds must be nonnegative");
			PlanValidation.Require(double.IsFinite(report.DeadlineSeconds) && report.DeadlineSeconds >= 0, "deadlineSeconds must be nonnegative");
			PlanValidation.Require(report.ElapsedSeconds >= report.DeadlineSeconds, "elapsedSeconds must reach deadlineSeconds");
			PlanValidation.Require(!string.IsNullOrWhiteSpace(report.Message), "message must not be empty");
		}
	}

	internal static class PlanValidation
	{
		public static void ValidateHeader(
			int schemaVersion,
			ReleaseOperation operation,
			ReleaseOperation expectedOperation,
			Guid planId,
			DateTimeOffset generatedAt,
			string toolingSha)
		{
			Require(schemaVersion == 1, "schemaVersion must be 1");
			Require(operation == expectedOperation, $"operation must be '{expectedOperation.ToString().ToLowerInvariant()}'");
			Require(planId != Guid.Empty, "planId must not be empty");
			Require(generatedAt != default && generatedAt.Offset == TimeSpan.Zero, "generatedAt must be a UTC timestamp");
			ValidateSha(toolingSha, "toolingSha");
		}

		public static void ValidateSha(string value, string name) =>
			Require(value is not null && value.Length == 40 && value.All(IsHex), $"{name} must be a lowercase 40-hex SHA");

		public static void ValidateStrings(IReadOnlyList<string> values, string name)
		{
			var strings = values ?? throw new ValidationException($"{name} must not be null");
			foreach (var value in strings)
				Require(value is not null, $"{name} must not contain null values");
		}

		public static void Require(bool condition, string message)
		{
			if (!condition)
				throw new ValidationException(message);
		}

		private static bool IsHex(char value) =>
			value is >= '0' and <= '9' or >= 'a' and <= 'f';
	}
}
