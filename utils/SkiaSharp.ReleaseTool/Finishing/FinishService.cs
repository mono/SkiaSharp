using System.Security.Cryptography;
using System.Text;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Planning;

namespace SkiaSharp.ReleaseTool.Finishing
{
	internal sealed class FinishService(
		IReleaseRepository repository,
		IFinishGitHubClient github,
		TimeProvider timeProvider,
		Func<Guid> newPublicationPlanId)
	{
		public async Task<FinishCreateDraftResult> CreateDraftAsync(
			FinishPlan plan,
			Guid expectedPlanId,
			CancellationToken cancellationToken = default,
			IReadOnlyList<string>? allowedUntrackedPaths = null)
		{
			ValidatePlanCorrelation(plan, expectedPlanId);
			if (plan.NextAction != FinishNextAction.CreateDraft)
				throw new ValidationException("finish create-draft requires a plan whose nextAction is create-draft");
			var writeGitHub = RequireWriteClient();

			await repository.FetchAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
			await repository.RequireCleanAsync(
				allowedUntrackedPaths,
				cancellationToken).ConfigureAwait(false);
			var tagStatus = await RevalidateSourceAndTagAsync(
				plan,
				requireTag: false,
				cancellationToken).ConfigureAwait(false);
			if (tagStatus is null)
			{
				try
				{
					await repository.PushTagAsync(
						plan.Release.Tag,
						plan.Receipt.SourceCommit,
						cancellationToken: cancellationToken).ConfigureAwait(false);
				}
				catch (ReleaseToolException ex)
				{
					var raced = await ReadExactTagAsync(plan, cancellationToken).ConfigureAwait(false);
					if (raced != plan.Receipt.SourceCommit)
					{
						throw new ConflictException(
							$"tag {plan.Release.Tag} was not created at package source commit {plan.Receipt.SourceCommit}",
							ex);
					}
				}
				var verifiedTag = await ReadExactTagAsync(plan, cancellationToken).ConfigureAwait(false);
				if (verifiedTag != plan.Receipt.SourceCommit)
				{
					throw new ConflictException(
						$"tag {plan.Release.Tag} did not verify at package source commit {plan.Receipt.SourceCommit}");
				}
				tagStatus = FinishWriteStatus.Created;
			}

			var existing = await github.GetReleaseAsync(
				plan.Release.Tag,
				cancellationToken).ConfigureAwait(false);
			if (existing is not null)
			{
				ValidateRelease(plan, existing);
				if (!existing.IsDraft)
				{
					return CreateDraftResult(
						plan,
						existing,
						tagStatus.Value,
						FinishWriteStatus.AlreadyPublished,
						FinishNextAction.Closeout);
				}

				var markerState = ManagedReleaseMarkers.Inspect(existing.Body);
				if (markerState == ManagedMarkerState.Complete)
				{
					if (!ManagedReleaseMarkers.HasGeneratedNotes(existing.Body))
					{
						var regeneratedNotes = await GenerateRequiredNotesAsync(
							plan,
							writeGitHub,
							cancellationToken).ConfigureAwait(false);
						var repairedBody = ManagedReleaseMarkers.ReplaceGeneratedNotes(
							existing.Body,
							regeneratedNotes);
						var repaired = await writeGitHub.UpdateDraftBodyAsync(
							existing,
							repairedBody,
							cancellationToken).ConfigureAwait(false);
						var repairedReloaded = await RequireReloadedReleaseAsync(
							plan,
							repaired.Id,
							repairedBody,
							isDraft: true,
							cancellationToken).ConfigureAwait(false);
						RequireGeneratedNotes(repairedReloaded);
						return CreateDraftResult(
							plan,
							repairedReloaded,
							tagStatus.Value,
							FinishWriteStatus.Migrated,
							FinishNextAction.PlanPublication);
					}
					return CreateDraftResult(
						plan,
						existing,
						tagStatus.Value,
						FinishWriteStatus.Existing,
						FinishNextAction.PlanPublication);
				}

				var migratedNotes = string.IsNullOrWhiteSpace(existing.Body)
					? await GenerateRequiredNotesAsync(
						plan,
						writeGitHub,
						cancellationToken).ConfigureAwait(false)
					: existing.Body;
				var migratedBody = ManagedReleaseMarkers.BuildInitialBody(migratedNotes);
				RequireGeneratedNotes(migratedBody, plan.Release.Tag);
				var updated = await writeGitHub.UpdateDraftBodyAsync(
					existing,
					migratedBody,
					cancellationToken).ConfigureAwait(false);
				var reloaded = await RequireReloadedReleaseAsync(
					plan,
					updated.Id,
					migratedBody,
					isDraft: true,
					cancellationToken).ConfigureAwait(false);
				_ = RequireCompleteMarkers(reloaded);
				return CreateDraftResult(
					plan,
					reloaded,
					tagStatus.Value,
					FinishWriteStatus.Migrated,
					FinishNextAction.PlanPublication);
			}

			var generatedNotes = await GenerateRequiredNotesAsync(
				plan,
				writeGitHub,
				cancellationToken).ConfigureAwait(false);
			var body = ManagedReleaseMarkers.BuildInitialBody(generatedNotes);
			var created = await writeGitHub.CreateDraftAsync(
				plan.Release.Tag,
				plan.Release.Title,
				plan.Receipt.SourceCommit,
				body,
				prerelease: !plan.Release.Stable,
				cancellationToken).ConfigureAwait(false);
			var verified = await RequireReloadedReleaseAsync(
				plan,
				created.Id,
				body,
				isDraft: true,
				cancellationToken).ConfigureAwait(false);
			_ = RequireCompleteMarkers(verified);
			return CreateDraftResult(
				plan,
				verified,
				tagStatus.Value,
				FinishWriteStatus.Created,
				FinishNextAction.PlanPublication);
		}

		public async Task<FinishPublicationPlan> PlanPublicationAsync(
			FinishPlan plan,
			Guid expectedPlanId,
			CancellationToken cancellationToken = default)
		{
			ValidatePlanCorrelation(plan, expectedPlanId);
			await repository.FetchAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
			_ = await RevalidateSourceAndTagAsync(
				plan,
				requireTag: true,
				cancellationToken).ConfigureAwait(false);

			var existing = await github.GetReleaseAsync(
				plan.Release.Tag,
				cancellationToken).ConfigureAwait(false)
				?? throw new ConflictException(
					$"no draft or release exists for {plan.Release.Tag}; run finish create-draft first");
			ValidateRelease(plan, existing);

			var markerState = ManagedReleaseMarkers.Inspect(existing.Body);
			var hasGeneratedNotes =
				markerState == ManagedMarkerState.Complete &&
				ManagedReleaseMarkers.HasGeneratedNotes(existing.Body);
			if (existing.IsDraft)
			{
				if (markerState != ManagedMarkerState.Complete)
				throw new ConflictException($"draft {plan.Release.Tag} does not have complete managed markers");
				if (!hasGeneratedNotes)
					throw new ConflictException($"draft {plan.Release.Tag} has an empty generated-notes region");
			}

			var publicationPlanId = newPublicationPlanId();
			var result = new FinishPublicationPlan(
				SchemaVersion: 1,
				Operation: FinishArtifactOperation.PlanPublication,
				PlanId: plan.PlanId,
				PublicationPlanId: publicationPlanId,
				GeneratedAt: timeProvider.GetUtcNow(),
				ToolingSha: plan.ToolingSha,
				NextAction: existing.IsDraft
					? FinishNextAction.Publish
					: FinishNextAction.Closeout,
				Release: plan.Release,
				SourceCommit: plan.Receipt.SourceCommit,
				ReleaseId: existing.Id,
				ReleaseUrl: existing.Url,
				IsDraft: existing.IsDraft,
				IsPublished: !existing.IsDraft,
				MarkerState: markerState,
				HasGeneratedNotes: hasGeneratedNotes,
				ReadyToPublish: existing.IsDraft,
				BodyHashAlgorithm: BodyHashAlgorithm.Sha256,
				BodyHash: BodyHash(existing.Body));
			FinishPublicationPlanValidator.Validate(result);
			return result;
		}

		public async Task<FinishPublishResult> PublishAsync(
			FinishPlan plan,
			Guid expectedPlanId,
			FinishPublicationPlan publication,
			Guid expectedPublicationPlanId,
			CancellationToken cancellationToken = default,
			IReadOnlyList<string>? allowedUntrackedPaths = null)
		{
			ValidatePlanCorrelation(plan, expectedPlanId);
			ValidatePublicationBinding(
				plan,
				publication,
				expectedPublicationPlanId);

			await repository.FetchAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
			await repository.RequireCleanAsync(
				allowedUntrackedPaths,
				cancellationToken).ConfigureAwait(false);
			_ = await RevalidateSourceAndTagAsync(
				plan,
				requireTag: true,
				cancellationToken).ConfigureAwait(false);

			var existing = await github.GetReleaseAsync(
				plan.Release.Tag,
				cancellationToken).ConfigureAwait(false)
				?? throw new ConflictException($"no draft exists for {plan.Release.Tag}");
			ValidateRelease(plan, existing);

			if (!existing.IsDraft)
			{
				if (existing.Id != publication.ReleaseId)
				throw new ConflictException("published release identity changed after publication approval");
				return PublishResult(
					plan,
					publication,
					existing,
					FinishWriteStatus.AlreadyPublished);
			}
			ValidateApprovedRelease(publication, existing);
			if (publication.NextAction != FinishNextAction.Publish ||
				!publication.ReadyToPublish)
			{
				throw new ValidationException(
					$"publication plan {publication.PublicationPlanId} is not approved for publish");
			}
			_ = RequireCompleteMarkers(existing);
			if (!ManagedReleaseMarkers.HasGeneratedNotes(existing.Body))
				throw new ConflictException($"draft {plan.Release.Tag} has an empty generated-notes region");

			var writeGitHub = RequireWriteClient();
			var published = await writeGitHub.PublishDraftAsync(
				existing,
				cancellationToken).ConfigureAwait(false);
			var reloaded = await RequireReloadedReleaseAsync(
				plan,
				published.Id,
				existing.Body,
				isDraft: false,
				cancellationToken).ConfigureAwait(false);
			ValidateApprovedRelease(publication, reloaded);
			return PublishResult(
				plan,
				publication,
				reloaded,
				FinishWriteStatus.Published);
		}

		private async Task<FinishWriteStatus?> RevalidateSourceAndTagAsync(
			FinishPlan plan,
			bool requireTag,
			CancellationToken cancellationToken)
		{
			if (!await repository.CommitExistsAsync(
				plan.ToolingSha,
				cancellationToken).ConfigureAwait(false))
			{
				throw new ConflictException($"tooling commit {plan.ToolingSha} no longer exists");
			}
			if (!await repository.CommitExistsAsync(
				plan.Receipt.SourceCommit,
				cancellationToken).ConfigureAwait(false))
			{
				throw new ConflictException(
					$"package source commit {plan.Receipt.SourceCommit} does not exist");
			}

			var branchRef = $"refs/remotes/origin/{plan.Release.Branch}";
			if (!await repository.RefExistsAsync(
				branchRef,
				cancellationToken).ConfigureAwait(false) ||
				!await repository.IsAncestorAsync(
					plan.Receipt.SourceCommit,
					branchRef,
					cancellationToken).ConfigureAwait(false))
			{
				throw new ConflictException(
					$"exact release branch {plan.Release.Branch} does not contain package source commit {plan.Receipt.SourceCommit}");
			}

			var tagSha = await ReadExactTagAsync(plan, cancellationToken).ConfigureAwait(false);
			if (tagSha is not null && tagSha != plan.Receipt.SourceCommit)
			{
				throw new ConflictException(
					$"tag {plan.Release.Tag} points to {tagSha}, not package source commit {plan.Receipt.SourceCommit}; tags are never moved");
			}
			if (requireTag && tagSha is null)
				throw new ConflictException($"authoritative tag {plan.Release.Tag} does not exist");
			return tagSha is null ? null : FinishWriteStatus.Existing;
		}

		private async Task<string?> ReadExactTagAsync(
			FinishPlan plan,
			CancellationToken cancellationToken)
		{
			var tags = await repository.RemoteTagsAsync(
				cancellationToken: cancellationToken).ConfigureAwait(false);
			return tags.TryGetValue(plan.Release.Tag, out var sha) ? sha : null;
		}

		private async Task<FinishGitHubRelease> RequireReloadedReleaseAsync(
			FinishPlan plan,
			long expectedId,
			string expectedBody,
			bool isDraft,
			CancellationToken cancellationToken)
		{
			var release = await github.GetReleaseAsync(
				plan.Release.Tag,
				cancellationToken).ConfigureAwait(false)
				?? throw new ConflictException(
					$"GitHub release {plan.Release.Tag} could not be re-read after write");
			ValidateRelease(plan, release);
			if (release.Id != expectedId ||
				release.IsDraft != isDraft ||
				release.Body != expectedBody)
			{
				throw new ConflictException(
					$"GitHub release {plan.Release.Tag} did not retain the exact state written");
			}
			return release;
		}

		private FinishCreateDraftResult CreateDraftResult(
			FinishPlan plan,
			FinishGitHubRelease release,
			FinishWriteStatus tagStatus,
			FinishWriteStatus draftStatus,
			FinishNextAction nextAction)
		{
			var result = new FinishCreateDraftResult(
				SchemaVersion: 1,
				Operation: FinishArtifactOperation.CreateDraft,
				PlanId: plan.PlanId,
				GeneratedAt: timeProvider.GetUtcNow(),
				ToolingSha: plan.ToolingSha,
				NextAction: nextAction,
				Release: plan.Release,
				SourceCommit: plan.Receipt.SourceCommit,
				ReleaseId: release.Id,
				ReleaseUrl: release.Url,
				BodyHashAlgorithm: BodyHashAlgorithm.Sha256,
				BodyHash: BodyHash(release.Body),
				Operations:
				[
					new(FinishOperationId.CreateTag, tagStatus),
					new(FinishOperationId.CreateDraft, draftStatus),
				]);
			FinishCreateDraftResultValidator.Validate(result);
			return result;
		}

		private FinishPublishResult PublishResult(
			FinishPlan plan,
			FinishPublicationPlan publication,
			FinishGitHubRelease release,
			FinishWriteStatus status)
		{
			var result = new FinishPublishResult(
				SchemaVersion: 1,
				Operation: FinishArtifactOperation.Publish,
				PlanId: plan.PlanId,
				PublicationPlanId: publication.PublicationPlanId,
				GeneratedAt: timeProvider.GetUtcNow(),
				ToolingSha: plan.ToolingSha,
				NextAction: FinishNextAction.Closeout,
				Release: plan.Release,
				SourceCommit: plan.Receipt.SourceCommit,
				ReleaseId: release.Id,
				ReleaseUrl: release.Url,
				BodyHashAlgorithm: BodyHashAlgorithm.Sha256,
				BodyHash: BodyHash(release.Body),
				Operations:
				[
					new(FinishOperationId.PublishRelease, status),
				]);
			FinishPublishResultValidator.Validate(result);
			return result;
		}

		private static void ValidatePlanCorrelation(FinishPlan plan, Guid expectedPlanId)
		{
			FinishPlanValidator.Validate(plan);
			if (plan.PlanId != expectedPlanId)
			{
				throw new ValidationException(
					$"planId '{plan.PlanId}' does not match expected correlation id '{expectedPlanId}'");
			}
		}

		private static void ValidatePublicationBinding(
			FinishPlan plan,
			FinishPublicationPlan publication,
			Guid expectedPublicationPlanId)
		{
			FinishPublicationPlanValidator.Validate(publication);
			if (publication.PublicationPlanId != expectedPublicationPlanId)
			{
				throw new ValidationException(
					$"publicationPlanId '{publication.PublicationPlanId}' does not match expected correlation id '{expectedPublicationPlanId}'");
			}
			if (publication.PlanId != plan.PlanId)
				throw new ConflictException("publication plan was generated from a different Finish PlanId");
			if (publication.ToolingSha != plan.ToolingSha)
				throw new ConflictException("publication plan tooling SHA does not match the Finish Plan");
			if (publication.Release != plan.Release ||
				publication.SourceCommit != plan.Receipt.SourceCommit)
			{
				throw new ConflictException(
					"publication plan release identity does not match the Finish Plan");
			}
		}

		private static void ValidateApprovedRelease(
			FinishPublicationPlan publication,
			FinishGitHubRelease release)
		{
			if (release.Id != publication.ReleaseId)
			{
				throw new ConflictException(
					$"release identity changed from {publication.ReleaseId} to {release.Id} after publication approval");
			}
			if (publication.ReadyToPublish &&
				release.TargetCommitish != publication.SourceCommit)
			{
				throw new ConflictException(
					$"release {release.TagName} target changed after publication approval");
			}
			var bodyHash = BodyHash(release.Body);
			if (bodyHash != publication.BodyHash)
			{
				throw new ConflictException(
					$"draft {release.TagName} body changed after publication approval");
			}
		}

		private static void ValidateRelease(
			FinishPlan plan,
			FinishGitHubRelease release)
		{
			var mismatches = new List<string>();
			if (release.Id <= 0)
				mismatches.Add($"id '{release.Id}' is invalid");
			if (release.TagName != plan.Release.Tag)
				mismatches.Add($"tag '{release.TagName}' != '{plan.Release.Tag}'");
			if (release.Title != plan.Release.Title)
				mismatches.Add($"title '{release.Title}' != '{plan.Release.Title}'");
			if (release.IsPrerelease == plan.Release.Stable)
				mismatches.Add($"prerelease '{release.IsPrerelease}' is inconsistent");
			if (release.TargetCommitish != plan.Receipt.SourceCommit)
			{
				var exactSha =
					release.TargetCommitish.Length == 40 &&
					release.TargetCommitish.All(static character =>
						character is >= '0' and <= '9' or >= 'a' and <= 'f');
				var acceptedLegacyTarget =
					!release.IsDraft &&
					!exactSha &&
					(release.TargetCommitish is "main" ||
						release.TargetCommitish == plan.Release.Branch);
				if (!acceptedLegacyTarget)
				{
					mismatches.Add(
						$"target '{release.TargetCommitish}' != '{plan.Receipt.SourceCommit}'");
				}
			}
			if (!release.Url.IsAbsoluteUri)
				mismatches.Add("URL is not absolute");
			if (mismatches.Count > 0)
			{
				throw new ConflictException(
					$"GitHub release {plan.Release.Tag} conflicts with the Finish Plan: " +
					string.Join("; ", mismatches));
			}
		}

		private static ManagedMarkerState RequireCompleteMarkers(
			FinishGitHubRelease release)
		{
			var markerState = ManagedReleaseMarkers.Inspect(release.Body);
			if (markerState != ManagedMarkerState.Complete)
				throw new ConflictException($"draft {release.TagName} does not have complete managed markers");
			return markerState;
		}

		private async Task<string> GenerateRequiredNotesAsync(
			FinishPlan plan,
			IFinishGitHubWriteClient writeGitHub,
			CancellationToken cancellationToken)
		{
			var notes = await writeGitHub.GenerateReleaseNotesAsync(
				plan.Release.Tag,
				plan.Receipt.SourceCommit,
				plan.PreviousTag,
				cancellationToken).ConfigureAwait(false);
			if (string.IsNullOrWhiteSpace(notes))
				throw new ConflictException($"GitHub generated no release notes for {plan.Release.Tag}");
			return notes;
		}

		private static void RequireGeneratedNotes(FinishGitHubRelease release) =>
			RequireGeneratedNotes(release.Body, release.TagName);

		private static void RequireGeneratedNotes(string body, string tag)
		{
			_ = ManagedReleaseMarkers.Inspect(body);
			if (!ManagedReleaseMarkers.HasGeneratedNotes(body))
				throw new ConflictException($"draft {tag} has an empty generated-notes region");
		}

		private IFinishGitHubWriteClient RequireWriteClient() =>
			github as IFinishGitHubWriteClient
				?? throw new ValidationException("finish write command requires a write-capable GitHub gateway");

		internal static string BodyHash(string body) =>
			Convert.ToHexString(
				SHA256.HashData(Encoding.UTF8.GetBytes(body)))
				.ToLowerInvariant();
	}
}
