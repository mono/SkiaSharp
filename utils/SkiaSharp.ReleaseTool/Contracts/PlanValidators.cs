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

			var integrationTarget = ReleaseVersionPolicy.NormalizeIntegrationBranch(plan.Input.IntegrationTarget);
			PlanValidation.Require(
				integrationTarget == plan.Input.IntegrationTarget &&
				(integrationTarget == "main" || integrationTarget == identity.IntegrationBranch),
				"input.integrationTarget must be normalized main or release.integrationBranch");
			if (plan.Input.RequestedVersion is not null)
				PlanValidation.Require(plan.Input.RequestedVersion == identity.Raw, "input.requestedVersion does not match release.identity");
			if (plan.Input.ApprovedBase is not null)
			{
				PlanValidation.Require(
					GitReferencePolicy.IsFullyQualified(plan.Input.ApprovedBase),
					"input.approvedBase must be a fully-qualified refs/... name");
			}

			ValidateBaseRef(plan, identity);
			PlanValidation.ValidateSha(plan.Base.Sha, "base.sha");
			PlanValidation.ValidateSha(plan.Skia.Sha, "skia.sha");
			PlanValidation.Require(
				plan.MaintenanceBranch.Name == identity.IntegrationBranch,
				"maintenanceBranch.name does not match release.integrationBranch");
			if (plan.MaintenanceBranch.Action == MaintenanceBranchAction.Create)
			{
				PlanValidation.Require(!plan.MaintenanceBranch.Exists, "maintenance branch create cannot target an existing branch");
				PlanValidation.ValidateSha(
					plan.MaintenanceBranch.BaseSha ??
						throw new ValidationException("maintenanceBranch.baseSha is required for create"),
					"maintenanceBranch.baseSha");
			}
			else
			{
				PlanValidation.Require(
					plan.MaintenanceBranch.BaseSha is null,
					"maintenanceBranch.baseSha is only valid for create");
			}

			ValidateStableBump(plan, identity);
			PlanValidation.ValidateRequiredCollection(plan.Operations, "operations");
			PlanValidation.ValidateStrings(plan.Warnings, "warnings");
		}

		private static void ValidateBaseRef(
			PreparePlan plan,
			SkiaSharpReleaseIdentity identity)
		{
			var baseRef = plan.Base.Ref;
			if (plan.Input.ApprovedBase == baseRef &&
				plan.MaintenanceBranch.Action == MaintenanceBranchAction.Create)
			{
				return;
			}

			const string tagPrefix = "refs/tags/";
			if (baseRef.StartsWith(tagPrefix, StringComparison.Ordinal))
			{
				PlanValidation.Require(
					identity.IsHotfix && !identity.Stable,
					"base.ref may be a tag only for a hotfix prerelease");
				var parent = SkiaSharpReleaseIdentity.ParseTag(baseRef[tagPrefix.Length..]);
				var expectedParent = identity.Version.Version.ToString(3);
				PlanValidation.Require(
					parent.Stable &&
					!parent.IsHotfix &&
					parent.Numeric == expectedParent &&
					baseRef == $"refs/tags/v{expectedParent}",
					"base.ref must be the exact stable parent tag for a hotfix prerelease");
				return;
			}

			const string remotePrefix = "refs/remotes/origin/";
			PlanValidation.Require(
				baseRef.StartsWith(remotePrefix, StringComparison.Ordinal),
				"base.ref must be an origin remote ref or approved recovery ref");
			var branch = baseRef[remotePrefix.Length..];
			if (branch == "main" || branch == identity.IntegrationBranch)
				return;

			var previous = SkiaSharpReleaseIdentity.ParseBranch(branch);
			PlanValidation.Require(
				previous.Numeric == identity.Numeric &&
				(previous.CompareTo(identity) < 0 ||
					(previous.Raw == identity.Raw &&
					 plan.SkiaSharpRemoteState == RemoteState.Matching)),
				"base.ref recovery release must precede the requested release on the same version");
		}

		private static void ValidateStableBump(
			PreparePlan plan,
			SkiaSharpReleaseIdentity identity)
		{
			var required = identity.Stable && !identity.IsHotfix;
			PlanValidation.Require(
				(plan.StableBump is not null) == required,
				"stableBump must be present only for a non-hotfix stable release");
			if (plan.StableBump is not { } bump)
				return;
			if (identity.Version.Patch == int.MaxValue)
				throw new ValidationException("stableBump cannot advance the maximum patch");
			var next = new Version(
				identity.Version.Major,
				identity.Version.Minor,
				identity.Version.Patch + 1).ToString(3);
			PlanValidation.Require(bump.IntegrationBranch == identity.IntegrationBranch, "stableBump.integrationBranch is inconsistent");
			PlanValidation.Require(bump.SkiaSharpVersion == next, "stableBump.skiaSharpVersion is not the next patch");
			ReleaseVersionPolicy.ParseStableVersion(bump.HarfBuzzSharpVersion, "stableBump.harfBuzzSharpVersion", 3, 4);
			PlanValidation.Require(bump.BumpBranch == $"bump-version-{next}", "stableBump.bumpBranch is inconsistent");
			PlanValidation.Require(
				bump.Title == $"Bump to the next version ({next}) after release",
				"stableBump.title is inconsistent");
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
			var requested = PublicReleaseVersion.Parse(plan.Input.RequestedVersion);
			var identity = requested.Identity;
			PlanValidation.Require(plan.Receipt.SkiaSharpVersion == requested.Text, "receipt.skiaSharpVersion must match input.requestedVersion");
			PlanValidation.Require(plan.Receipt.Base == requested.Base, "receipt.base does not match the public version");
			PlanValidation.Require(plan.Receipt.Label == identity.Label, "receipt.label does not match the release");
			PlanValidation.Require(plan.Receipt.BuildRevision == requested.BuildRevision, "receipt.buildRevision does not match the release");
			PlanValidation.ValidateSha(plan.Receipt.SourceCommit, "receipt.sourceCommit");
			PlanValidation.Require(plan.Receipt.SourceBranch == identity.ReleaseBranch, "receipt.sourceBranch must be the exact release branch");
			_ = NuGetVersion.Parse(plan.Receipt.HarfBuzzSharpVersion);
			ValidatePackages(plan, requested);
			ValidateRelease(plan.Release, requested, plan.Receipt.SourceBranch);

			PlanValidation.Require(plan.Tag.Name == identity.Tag, "tag.name is inconsistent");
			PlanValidation.Require(plan.Tag.TargetCommit == plan.Receipt.SourceCommit, "tag.targetCommit is inconsistent");
			if (plan.Tag.ExistingSha is not null)
				PlanValidation.ValidateSha(plan.Tag.ExistingSha, "tag.existingSha");
			if (plan.PreviousTag is not null)
			{
				var previous = SkiaSharpReleaseIdentity.ParseTag(plan.PreviousTag);
				PlanValidation.Require(previous.CompareTo(identity) < 0, "previousTag must sort before the release");
			}

			ValidateDraftAndRouting(plan);
			PlanValidation.ValidateRequiredCollection(plan.Operations, "operations");
			PlanValidation.ValidateStrings(plan.Warnings, "warnings");
		}

		private static void ValidatePackages(FinishPlan plan, PublicReleaseVersion requested)
		{
			var packages = PlanValidation.ValidateRequiredCollection(
				plan.Receipt.Packages,
				"receipt.packages");
			var ids = new HashSet<string>(StringComparer.Ordinal);
			(string Commit, string Branch)? harfBuzzSource = null;
			foreach (var package in packages)
			{
				PlanValidation.Require(
					!string.IsNullOrWhiteSpace(package.Id) && ids.Add(package.Id),
					"receipt package IDs must be non-empty and unique");
				PlanValidation.ValidateSha(package.SourceCommit, $"receipt package {package.Id} sourceCommit");
				_ = SkiaSharpReleaseIdentity.ParseBranch(package.SourceBranch);
				_ = NuGetVersion.Parse(package.Version);
				if (package.Id.StartsWith("SkiaSharp", StringComparison.Ordinal))
				{
					PlanValidation.Require(
						package.Version == requested.Text &&
						package.SourceCommit == plan.Receipt.SourceCommit &&
						package.SourceBranch == plan.Receipt.SourceBranch,
						$"{package.Id} does not match the exact SkiaSharp receipt");
				}
				else if (package.Id.StartsWith("HarfBuzzSharp", StringComparison.Ordinal))
				{
					PlanValidation.Require(
						package.Version == plan.Receipt.HarfBuzzSharpVersion,
						$"{package.Id} version does not match the HarfBuzzSharp receipt");
					harfBuzzSource ??= (package.SourceCommit, package.SourceBranch);
					PlanValidation.Require(
						harfBuzzSource == (package.SourceCommit, package.SourceBranch),
						"HarfBuzzSharp package source must be family-consistent");
				}
				else
				{
					throw new ValidationException($"unsupported package family member '{package.Id}'");
				}
			}
			PlanValidation.Require(ids.Contains("SkiaSharp"), "receipt must contain its SkiaSharp source anchor");
		}

		private static void ValidateDraftAndRouting(FinishPlan plan)
		{
			var draft = plan.Draft;
			if (!draft.Exists)
			{
				PlanValidation.Require(
					!draft.IsPublished &&
					draft.TargetCommitish is null &&
					draft.Url is null &&
					draft.Body is null,
					"absent draft cannot contain published release data");
			}
			else
			{
				PlanValidation.Require(draft.Url is { IsAbsoluteUri: true }, "draft.url must be absolute");
				PlanValidation.Require(draft.Body is not null, "existing draft must include its body");
				PlanValidation.Require(!string.IsNullOrWhiteSpace(draft.TargetCommitish), "existing draft must include targetCommitish");
				if (!draft.IsPublished)
				{
					PlanValidation.Require(
						draft.TargetCommitish == plan.Receipt.SourceCommit,
						"unpublished draft targetCommitish must be the package source commit");
				}
			}

			var expectedAction = draft switch
			{
				{ IsPublished: true } => FinishNextAction.Closeout,
				{ Exists: true, MarkerState: ManagedMarkerState.Complete }
					when Finishing.ManagedReleaseMarkers.HasGeneratedNotes(draft.Body!) =>
					FinishNextAction.PlanPublication,
				_ => FinishNextAction.CreateDraft,
			};
			PlanValidation.Require(plan.NextAction == expectedAction, "nextAction does not match release routing state");
		}

		internal static void ValidateRelease(
			FinishReleaseInfo release,
			PublicReleaseVersion requested,
			string sourceBranch)
		{
			var identity = requested.Identity;
			PlanValidation.Require(release.Identity == identity.Raw, "release.identity is inconsistent");
			PlanValidation.Require(release.Version == requested.Text, "release.version is inconsistent");
			PlanValidation.Require(release.Branch == sourceBranch, "release.branch is inconsistent");
			PlanValidation.Require(release.Raw == identity.Raw, "release.raw is inconsistent");
			PlanValidation.Require(release.Numeric == identity.Numeric, "release.numeric is inconsistent");
			PlanValidation.Require(release.Label == identity.Label, "release.label is inconsistent");
			PlanValidation.Require(release.ReleaseType == identity.ReleaseType, "release.releaseType is inconsistent");
			PlanValidation.Require(release.Stable == identity.Stable, "release.stable is inconsistent");
			PlanValidation.Require(release.Title == identity.Title, "release.title is inconsistent");
			PlanValidation.Require(release.Tag == identity.Tag, "release.tag is inconsistent");
		}
	}

	public static class FinishPublicationPlanValidator
	{
		public static void Validate(FinishPublicationPlan plan)
		{
			PlanValidation.Require(plan.SchemaVersion == 1, "schemaVersion must be 1");
			PlanValidation.Require(
				plan.Operation == FinishArtifactOperation.PlanPublication,
				"operation must be finish-plan-publication");
			PlanValidation.Require(plan.PlanId != Guid.Empty, "planId must not be empty");
			PlanValidation.Require(
				plan.PublicationPlanId != Guid.Empty &&
				plan.PublicationPlanId != plan.PlanId,
				"publicationPlanId must be non-empty and distinct from planId");
			PlanValidation.Require(
				plan.GeneratedAt != default && plan.GeneratedAt.Offset == TimeSpan.Zero,
				"generatedAt must be a UTC timestamp");
			PlanValidation.ValidateSha(plan.ToolingSha, "toolingSha");
			var requested = PublicReleaseVersion.Parse(plan.Release.Version);
			FinishPlanValidator.ValidateRelease(plan.Release, requested, plan.Release.Branch);
			PlanValidation.ValidateSha(plan.SourceCommit, "sourceCommit");
			PlanValidation.Require(plan.ReleaseId > 0, "releaseId must be positive");
			PlanValidation.Require(plan.ReleaseUrl is { IsAbsoluteUri: true }, "releaseUrl must be absolute");
			PlanValidation.Require(plan.BodyHashAlgorithm == BodyHashAlgorithm.Sha256, "bodyHashAlgorithm must be SHA256");
			PlanValidation.ValidateSha256(plan.BodyHash, "bodyHash");

			var publish = plan.NextAction == FinishNextAction.Publish;
			var closeout = plan.NextAction == FinishNextAction.Closeout;
			PlanValidation.Require(publish || closeout, "nextAction must be publish or closeout");
			PlanValidation.Require(plan.IsDraft != plan.IsPublished, "exactly one of isDraft and isPublished must be true");
			PlanValidation.Require(
				closeout
					? plan.IsPublished
					: plan.IsDraft &&
						plan.ReadyToPublish &&
						plan.MarkerState == ManagedMarkerState.Complete &&
						plan.HasGeneratedNotes,
				"publication state is not safe for the requested next action");
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
			Require(
				value is not null &&
				value.Length == 40 &&
				value.All(static character =>
					character is >= '0' and <= '9' or >= 'a' and <= 'f'),
				$"{name} must be a lowercase 40-hex SHA");

		public static void ValidateSha256(string value, string name) =>
			Require(
				value is not null &&
				value.Length == 64 &&
				value.All(static character =>
					character is >= '0' and <= '9' or >= 'a' and <= 'f'),
				$"{name} must be a lowercase 64-hex SHA256");

		public static IReadOnlyList<T> ValidateRequiredCollection<T>(
			IReadOnlyList<T> values,
			string name)
			where T : class
		{
			var collection = values ?? throw new ValidationException($"{name} must not be null");
			Require(collection.All(static value => value is not null), $"{name} must not contain null values");
			return collection;
		}

		public static void ValidateStrings(IReadOnlyList<string> values, string name) =>
			_ = ValidateRequiredCollection(values, name);

		public static void Require(bool condition, string message)
		{
			if (!condition)
				throw new ValidationException(message);
		}
	}
}
