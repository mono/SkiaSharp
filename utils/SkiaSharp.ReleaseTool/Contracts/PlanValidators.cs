using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Errors;
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
			PlanValidation.Require(plan.Release.Numeric == identity.Numeric, "release.numeric does not match release.identity");
			PlanValidation.Require(plan.Release.Label == identity.Label, "release.label does not match release.identity");
			PlanValidation.Require(plan.Release.ReleaseType == identity.ReleaseType, "release.releaseType does not match release.identity");
			PlanValidation.Require(plan.Release.Branch == identity.ReleaseBranch, "release.branch does not match release.identity");
			PlanValidation.Require(
				plan.Release.IntegrationBranch == identity.IntegrationBranch,
				"release.integrationBranch does not match release.identity");
			PlanValidation.Require(plan.Release.IsHotfix == identity.IsHotfix, "release.isHotfix does not match release.identity");
			PlanValidation.Require(plan.Release.Stable == identity.Stable, "release.stable does not match release.identity");

			ReleaseVersionPolicy.NormalizeIntegrationBranch(plan.Input.IntegrationTarget);
			if (plan.Input.RequestedVersion is not null)
				PlanValidation.Require(plan.Input.RequestedVersion == identity.Raw, "input.requestedVersion does not match release.identity");

			PlanValidation.Require(!string.IsNullOrWhiteSpace(plan.Base.Ref), "base.ref must not be empty");
			ValidateBaseRef(plan.Base.Ref, identity);
			PlanValidation.ValidateSha(plan.Base.Sha, "base.sha");
			PlanValidation.ValidateSha(plan.Skia.Sha, "skia.sha");
			PlanValidation.Require(plan.Skia.ReleaseBranch == identity.ReleaseBranch, "skia.releaseBranch does not match release.branch");

			PlanValidation.Require(
				plan.MaintenanceBranch.Name == identity.IntegrationBranch,
				"maintenanceBranch.name does not match release.integrationBranch");
			if (plan.MaintenanceBranch.BaseSha is not null)
				PlanValidation.ValidateSha(plan.MaintenanceBranch.BaseSha, "maintenanceBranch.baseSha");
			if (plan.MaintenanceBranch.Action == MaintenanceBranchAction.Create)
			{
				PlanValidation.Require(!plan.MaintenanceBranch.Exists, "a maintenance branch marked for creation cannot already exist");
				PlanValidation.Require(
					plan.MaintenanceBranch.BaseSha == plan.Base.Sha,
					"maintenanceBranch.baseSha must match base.sha when creating the branch");
			}
			else
			{
				PlanValidation.Require(plan.MaintenanceBranch.Exists, "maintenanceBranch.action 'none' requires an existing branch");
			}

			var versionState = ReleaseVersionPolicy.ParseStableVersion(
				plan.Versions.SkiaSharp, "versions.skiaSharp", identity.ComponentCount);
			PlanValidation.Require(
				Equals(versionState.Version, identity.Version.Version),
				"versions.skiaSharp does not match release.numeric");

			ValidateOperations(plan);
			ValidateStableBump(plan, identity);
			PlanValidation.ValidateStrings(plan.Warnings, "warnings");
		}

		private static void ValidateOperations(PreparePlan plan)
		{
			var operations = new Dictionary<PlanOperationId, PlanOperation>();
			foreach (var operation in plan.Operations)
			{
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
						: PlanOperationStatus.Done,
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

			var statuses = plan.Operations.Select(static operation => operation.Status).ToHashSet();
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

		private static void ValidateBaseRef(string baseRef, SkiaSharpReleaseIdentity identity)
		{
			const string prefix = "refs/remotes/origin/";
			PlanValidation.Require(baseRef.StartsWith(prefix, StringComparison.Ordinal), "base.ref must be an origin remote ref");
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
				previous.CompareTo(identity) < 0,
				"base.ref recovery release must precede the requested release on the same numeric version");
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

			var identity = SkiaSharpReleaseIdentity.Parse(plan.Release.Identity);
			PlanValidation.Require(plan.Release.Raw == identity.Raw, "release.raw does not match release.identity");
			PlanValidation.Require(plan.Release.Numeric == identity.Numeric, "release.numeric does not match release.identity");
			PlanValidation.Require(plan.Release.Label == identity.Label, "release.label does not match release.identity");
			PlanValidation.Require(plan.Release.ReleaseType == identity.ReleaseType, "release.releaseType does not match release.identity");
			PlanValidation.Require(plan.Release.Stable == identity.Stable, "release.stable does not match release.identity");
			PlanValidation.Require(plan.Release.Title == identity.Title, "release.title does not match release.identity");
			PlanValidation.Require(plan.Release.Tag == identity.Tag, "release.tag does not match release.identity");
			PlanValidation.Require(plan.Release.Branch == identity.ReleaseBranch, "release.branch does not match release.identity");

			var (numeric, buildRevision) = identity.ValidatePublicVersion(plan.Release.Version);
			PlanValidation.Require(plan.Input.RequestedVersion == plan.Release.Version, "input.requestedVersion does not match release.version");
			PlanValidation.Require(plan.Receipt.SkiaSharpVersion == plan.Release.Version, "receipt.skiaSharpVersion does not match release.version");
			PlanValidation.Require(plan.Receipt.Base == numeric, "receipt.base does not match release.numeric");
			PlanValidation.Require(plan.Receipt.Label == identity.Label, "receipt.label does not match release.label");
			PlanValidation.Require(plan.Receipt.BuildRevision == buildRevision, "receipt.buildRevision is inconsistent");
			PlanValidation.Require(plan.Receipt.SourceBranch == identity.ReleaseBranch, "receipt.sourceBranch does not match release.branch");
			PlanValidation.ValidateSha(plan.Receipt.SourceCommit, "receipt.sourceCommit");
			ReleaseVersionPolicy.ParseStableVersion(
				plan.Receipt.HarfBuzzSharpVersion, "receipt.harfBuzzSharpVersion", 3, 4);
			ValidatePackages(plan);
			ValidateTag(plan, identity);
			ValidateDraft(plan);
			PlanValidation.ValidateStrings(plan.Warnings, "warnings");
		}

		private static void ValidatePackages(FinishPlan plan)
		{
			var packages = new Dictionary<string, PackageReceipt>(StringComparer.Ordinal);
			foreach (var package in plan.Receipt.Packages)
			{
				PlanValidation.Require(!string.IsNullOrWhiteSpace(package.Id), "package id must not be empty");
				PlanValidation.Require(packages.TryAdd(package.Id, package), $"receipt.packages contains duplicate id '{package.Id}'");
				PlanValidation.Require(package.SourceCommit == plan.Receipt.SourceCommit, $"package '{package.Id}' sourceCommit is inconsistent");
				PlanValidation.Require(package.SourceBranch == plan.Receipt.SourceBranch, $"package '{package.Id}' sourceBranch is inconsistent");
				PlanValidation.Require(
					NuGetVersion.TryParse(package.Version, out var packageVersion) && !packageVersion.HasMetadata,
					$"package '{package.Id}' has an invalid version");
			}

			PlanValidation.Require(
				packages.TryGetValue("SkiaSharp", out var skiaSharp) &&
				skiaSharp.Version == plan.Receipt.SkiaSharpVersion,
				"receipt.packages must contain the matching SkiaSharp package");
			PlanValidation.Require(
				packages.TryGetValue("HarfBuzzSharp", out var harfBuzz) &&
				harfBuzz.Version == plan.Receipt.HarfBuzzSharpVersion,
				"receipt.packages must contain the matching HarfBuzzSharp package");
		}

		private static void ValidateTag(FinishPlan plan, SkiaSharpReleaseIdentity identity)
		{
			PlanValidation.Require(plan.Tag.Name == identity.Tag, "tag.name does not match release.tag");
			PlanValidation.Require(plan.Tag.TargetCommit == plan.Receipt.SourceCommit, "tag.targetCommit does not match receipt.sourceCommit");
			if (plan.Tag.ExistingSha is null)
				PlanValidation.Require(plan.Tag.Status == CompletionStatus.Pending, "a missing tag must be pending");
			else
			{
				PlanValidation.ValidateSha(plan.Tag.ExistingSha, "tag.existingSha");
				PlanValidation.Require(plan.Tag.ExistingSha == plan.Tag.TargetCommit, "tag.existingSha conflicts with tag.targetCommit");
				PlanValidation.Require(plan.Tag.Status == CompletionStatus.Done, "an existing matching tag must be done");
			}

			if (plan.PreviousTag is not null)
			{
				var previous = SkiaSharpReleaseIdentity.ParseTag(plan.PreviousTag);
				PlanValidation.Require(
					VersionComparer.VersionRelease.Compare(previous.Version, identity.Version) < 0,
					"previousTag must precede the current release tag");
			}
		}

		private static void ValidateDraft(FinishPlan plan)
		{
			PlanValidation.Require(
				plan.Draft.Status == (plan.Draft.Exists ? CompletionStatus.Done : CompletionStatus.Pending),
				"draft.status does not match draft.exists");
			PlanValidation.Require(!plan.Draft.IsPublished || plan.Draft.Exists, "a published draft must exist");
			PlanValidation.Require(!plan.Draft.HasManagedMarkers || plan.Draft.Exists, "managed markers require an existing draft");

			var expectedAction = plan.Draft.IsPublished
				? FinishNextAction.Closeout
				: plan.Draft.Exists && plan.Draft.HasManagedMarkers
					? FinishNextAction.PlanPublication
					: FinishNextAction.CreateDraft;
			PlanValidation.Require(plan.NextAction == expectedAction, "nextAction does not match draft state");
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
			Require(value.Length == 40 && value.All(IsHex), $"{name} must be a 40-hex SHA");

		public static void ValidateStrings(IReadOnlyList<string> values, string name)
		{
			foreach (var value in values)
				Require(value is not null, $"{name} must not contain null values");
		}

		public static void Require(bool condition, string message)
		{
			if (!condition)
				throw new ValidationException(message);
		}

		private static bool IsHex(char value) =>
			value is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
	}
}
