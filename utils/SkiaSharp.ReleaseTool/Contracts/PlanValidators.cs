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
