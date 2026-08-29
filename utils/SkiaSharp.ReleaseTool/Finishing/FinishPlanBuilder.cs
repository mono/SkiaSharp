using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Model;
using SkiaSharp.ReleaseTool.NuGet;

namespace SkiaSharp.ReleaseTool.Finishing
{
	internal sealed record FinishPlanRequest(string RequestedVersion, string ToolingSha);

	internal sealed class FinishPlanBuilder(
		IFinishRepository repository,
		IPublicReceiptVerifier receiptVerifier,
		IFinishGitHubClient github,
		ReleasePolicies policies,
		TimeProvider timeProvider,
		Func<Guid> newPlanId)
	{
		public async Task<FinishPlan> BuildAsync(
			FinishPlanRequest request,
			CancellationToken cancellationToken = default)
		{
			if (request.ToolingSha.Length != 40 ||
				request.ToolingSha.Any(static character =>
					character is not (>= '0' and <= '9' or >= 'a' and <= 'f')))
			{
				throw new ValidationException("toolingSha must be a lowercase 40-hex SHA");
			}
			var requested = PublicReleaseVersion.Parse(request.RequestedVersion);
			var receipt = await receiptVerifier.VerifyAsync(
				repository,
				requested,
				policies,
				cancellationToken).ConfigureAwait(false);
			var identity = requested.Identity;
			if (receipt.SourceBranch != identity.ReleaseBranch)
				throw new ConflictException("verified package source branch does not match the public release identity");

			var tags = await repository.RemoteTagsAsync(
				cancellationToken: cancellationToken).ConfigureAwait(false);
			tags.TryGetValue(identity.Tag, out var existingTagSha);
			if (existingTagSha is not null && existingTagSha != receipt.SourceCommit)
			{
				throw new ConflictException(
					$"tag {identity.Tag} points to {existingTagSha}, not package source commit {receipt.SourceCommit}");
			}
			var tagState = existingTagSha is null ? FinishState.Pending : FinishState.Done;
			var previousTag = TagOrdering.SelectPreviousTag(identity.Tag, tags.Keys);

			var existingRelease = await github.GetReleaseAsync(
				identity.Tag,
				cancellationToken).ConfigureAwait(false);
			var releaseWarning = ValidateRelease(existingRelease, identity, receipt);
			if (existingRelease is { IsDraft: false } && tagState != FinishState.Done)
			{
				throw new ConflictException(
					$"published GitHub release {identity.Tag} exists without its authoritative remote tag");
			}
			var markers = existingRelease is null
				? ManagedMarkerState.None
				: ManagedReleaseMarkers.Inspect(existingRelease.Body);
			var hasGeneratedNotes = existingRelease is not null &&
				markers == ManagedMarkerState.Complete &&
				ManagedReleaseMarkers.HasGeneratedNotes(existingRelease.Body);
			var nextAction = (tagState, existingRelease) switch
			{
				(_, { IsDraft: false }) => FinishNextAction.Closeout,
				(FinishState.Done, { IsDraft: true })
					when hasGeneratedNotes =>
					FinishNextAction.PlanPublication,
				_ => FinishNextAction.CreateDraft,
			};
			var warnings = receipt.Warnings.ToList();
			if (releaseWarning is not null)
				warnings.Add(releaseWarning);

			var plan = new FinishPlan(
				SchemaVersion: 1,
				Operation: ReleaseOperation.Finish,
				PlanId: newPlanId(),
				GeneratedAt: timeProvider.GetUtcNow(),
				ToolingSha: request.ToolingSha,
				NextAction: nextAction,
				Input: new FinishInput(requested.Text),
				Receipt: new FinishReceiptInfo(
					receipt.SkiaSharpVersion.ToNormalizedString(),
					receipt.Base.ToNormalizedString(),
					receipt.Label,
					receipt.BuildRevision,
					receipt.SourceCommit,
					receipt.SourceBranch,
					receipt.HarfBuzzSharpVersion.ToNormalizedString(),
					receipt.Packages.Select(static package =>
						new FinishPackageReceipt(
							package.Id,
							package.Version.ToNormalizedString(),
							package.SourceCommit,
							package.SourceBranch)).ToArray()),
				Release: new FinishReleaseInfo(
					identity.Raw,
					requested.Text,
					receipt.SourceBranch,
					identity.Raw,
					identity.Numeric,
					identity.Label,
					identity.ReleaseType,
					identity.Stable,
					identity.Title,
					identity.Tag),
				Tag: new FinishTagInfo(
					identity.Tag,
					receipt.SourceCommit,
					existingTagSha,
					tagState),
				PreviousTag: previousTag,
				Draft: new FinishDraftInfo(
					existingRelease is not null,
					existingRelease is { IsDraft: false },
					existingRelease is null ? FinishState.Pending : FinishState.Done,
					markers,
					existingRelease?.TargetCommitish,
					existingRelease?.Url,
					existingRelease?.Body),
				Operations: BuildOperations(
					tagState,
					existingRelease,
					hasGeneratedNotes),
				Warnings: warnings);
			return plan;
		}

		private static string? ValidateRelease(
			FinishGitHubRelease? release,
			SkiaSharpReleaseIdentity identity,
			PublicReleaseReceipt receipt)
		{
			if (release is null)
				return null;
			var mismatches = new List<string>();
			string? warning = null;
			if (release.TagName != identity.Tag)
				mismatches.Add($"tag '{release.TagName}' != '{identity.Tag}'");
			if (release.Title != identity.Title)
				mismatches.Add($"title '{release.Title}' != '{identity.Title}'");
			if (release.IsPrerelease == identity.Stable)
				mismatches.Add($"prerelease {release.IsPrerelease} is inconsistent");
			if (release.TargetCommitish != receipt.SourceCommit)
			{
				var exactSha = release.TargetCommitish.Length == 40 &&
					release.TargetCommitish.All(static character =>
						character is >= '0' and <= '9' or >= 'a' and <= 'f');
				var acceptedLegacyTarget = !release.IsDraft &&
					!exactSha &&
					(release.TargetCommitish is "main" ||
					 release.TargetCommitish == receipt.SourceBranch);
				if (!acceptedLegacyTarget)
					mismatches.Add($"target '{release.TargetCommitish}' != '{receipt.SourceCommit}'");
				else
				{
					warning =
						$"published legacy release {identity.Tag} uses target_commitish '{release.TargetCommitish}'; exact tag {identity.Tag} remains authoritative";
				}
			}
			if (mismatches.Count > 0)
				throw new ConflictException("existing GitHub release conflicts with finish plan: " + string.Join("; ", mismatches));
			return warning;
		}

		private static IReadOnlyList<FinishOperation> BuildOperations(
			FinishState tagState,
			FinishGitHubRelease? release,
			bool hasGeneratedNotes)
		{
			var published = release is { IsDraft: false };
			var draftReady = release is not null && hasGeneratedNotes;
			return
			[
				new(
					FinishOperationId.CreateTag,
					FinishOperationKind.GitTag,
					tagState == FinishState.Done ? PlanOperationStatus.Done : PlanOperationStatus.Pending,
					null),
				new(
					FinishOperationId.CreateDraft,
					FinishOperationKind.GitHubRelease,
					published
						? PlanOperationStatus.Skipped
						: draftReady
							? PlanOperationStatus.Done
							: PlanOperationStatus.Pending,
					null),
				new(
					FinishOperationId.PublishRelease,
					FinishOperationKind.GitHubRelease,
					published
						? PlanOperationStatus.Done
						: draftReady
							? PlanOperationStatus.Pending
							: PlanOperationStatus.Skipped,
					null),
				new(
					FinishOperationId.Closeout,
					FinishOperationKind.ReleaseCloseout,
					published ? PlanOperationStatus.Pending : PlanOperationStatus.Skipped,
					null),
			];
		}
	}
}
