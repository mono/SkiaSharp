using System.Text;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Finishing;
using SkiaSharp.ReleaseTool.Git;
using SkiaSharp.ReleaseTool.Processes;
using SkiaSharp.ReleaseTool.Tests.Git;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Finishing
{
	public sealed class FinishServiceTests
	{
		[Fact]
		public async Task Missing_tag_and_draft_are_created_and_verified()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-create");

			var result = await fixture.Service.CreateDraftAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);

			var tags = await fixture.Repository.RemoteTagsAsync(
				pattern: $"refs/tags/{FinishTestFixture.Tag}",
				cancellationToken: TestContext.Current.CancellationToken);
			Assert.Equal(fixture.SourceCommit, tags[FinishTestFixture.Tag]);
			Assert.Equal(FinishNextAction.PlanPublication, result.NextAction);
			Assert.Equal(fixture.Plan.PlanId, result.PlanId);
			Assert.Equal(fixture.Plan.ToolingSha, result.ToolingSha);
			Assert.Equal(
				FinishWriteStatus.Created,
				Assert.Single(
					result.Operations,
					operation => operation.Id == FinishOperationId.CreateTag).Status);
			Assert.Equal(
				FinishWriteStatus.Created,
				Assert.Single(
					result.Operations,
					operation => operation.Id == FinishOperationId.CreateDraft).Status);
			Assert.Equal(
				(FinishTestFixture.Tag, fixture.SourceCommit, "v4.151.1"),
				fixture.GitHub.GeneratedRequest);
			Assert.Equal(
				ManagedReleaseMarkers.BuildInitialBody(fixture.GitHub.GeneratedNotes),
				fixture.GitHub.Release!.Body);
			Assert.Equal(FinishService.BodyHash(fixture.GitHub.Release.Body), result.BodyHash);
		}

		[Fact]
		public async Task Matching_tag_and_marked_draft_are_preserved_on_retry()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-retry");
			await fixture.EnsureTagAsync();
			var body = ManagedReleaseMarkers.BuildInitialBody("reviewed bytes\n");
			fixture.SetRelease(body);

			var result = await fixture.Service.CreateDraftAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Equal(FinishWriteStatus.Existing, result.Operations[0].Status);
			Assert.Equal(FinishWriteStatus.Existing, result.Operations[1].Status);
			Assert.Equal(body, fixture.GitHub.Release!.Body);
			Assert.Equal(0, fixture.GitHub.GenerateCount);
			Assert.Equal(0, fixture.GitHub.CreateCount);
			Assert.Equal(0, fixture.GitHub.UpdateCount);
		}

		[Fact]
		public async Task Markerless_draft_is_migrated_without_losing_bytes()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-migrate");
			await fixture.EnsureTagAsync();
			const string legacy = "## What's Changed\n* hand-authored \u2603\n";
			fixture.SetRelease(legacy);

			var result = await fixture.Service.CreateDraftAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Equal(FinishWriteStatus.Migrated, result.Operations[1].Status);
			Assert.Equal(1, fixture.GitHub.UpdateCount);
			Assert.Equal(
				ManagedReleaseMarkers.BuildInitialBody(legacy),
				fixture.GitHub.Release!.Body);
			Assert.Contains(legacy.Trim(), fixture.GitHub.Release.Body);
		}

		[Fact]
		public async Task Empty_markerless_draft_is_repaired_with_generated_notes()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-migrate-empty");
			await fixture.EnsureTagAsync();
			fixture.SetRelease(" \n ");

			var result = await fixture.Service.CreateDraftAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Equal(FinishWriteStatus.Migrated, result.Operations[1].Status);
			Assert.True(ManagedReleaseMarkers.HasGeneratedNotes(fixture.GitHub.Release!.Body));
			Assert.Equal(1, fixture.GitHub.GenerateCount);
			Assert.Equal(1, fixture.GitHub.UpdateCount);
		}

		[Fact]
		public async Task Marked_draft_with_empty_notes_is_repaired_without_changing_summary()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-repair-empty");
			await fixture.EnsureTagAsync();
			var body =
				$"{ManagedReleaseMarkers.SummaryStart}\nreviewed summary\n{ManagedReleaseMarkers.SummaryEnd}\n\n" +
				$"{ManagedReleaseMarkers.GeneratedNotesStart}\n \n{ManagedReleaseMarkers.GeneratedNotesEnd}\n";
			fixture.SetRelease(body);

			var result = await fixture.Service.CreateDraftAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Equal(FinishWriteStatus.Migrated, result.Operations[1].Status);
			Assert.Contains("reviewed summary", fixture.GitHub.Release!.Body);
			Assert.Contains(fixture.GitHub.GeneratedNotes, fixture.GitHub.Release.Body);
		}

		[Fact]
		public async Task Empty_generated_notes_fail_before_creating_a_draft()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-generated-empty");
			fixture.GitHub.GeneratedNotes = " \n ";

			await Assert.ThrowsAsync<ConflictException>(() =>
				fixture.Service.CreateDraftAsync(
					fixture.Plan,
					fixture.Plan.PlanId,
					TestContext.Current.CancellationToken));

			Assert.Equal(0, fixture.GitHub.CreateCount);
			Assert.Null(fixture.GitHub.Release);
		}

		[Fact]
		public async Task Marked_draft_summary_and_generated_notes_are_never_rewritten()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-preserve");
			await fixture.EnsureTagAsync();
			var body =
				$"{ManagedReleaseMarkers.SummaryStart}\nreviewed summary\n{ManagedReleaseMarkers.SummaryEnd}\n\n" +
				$"{ManagedReleaseMarkers.GeneratedNotesStart}\nGitHub notes\n{ManagedReleaseMarkers.GeneratedNotesEnd}\n";
			fixture.SetRelease(body);

			_ = await fixture.Service.CreateDraftAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Equal(body, fixture.GitHub.Release!.Body);
			Assert.Equal(0, fixture.GitHub.UpdateCount);
		}

		[Theory]
		[InlineData("<!-- SKIASHARP:RELEASE-SUMMARY:START -->")]
		[InlineData("<!-- SKIASHARP:RELEASE-SUMMARY:START --><!-- SKIASHARP:RELEASE-SUMMARY:END --><!-- SKIASHARP:GITHUB-GENERATED-NOTES:END --><!-- SKIASHARP:GITHUB-GENERATED-NOTES:START -->")]
		public async Task Malformed_markers_block_without_updating(string body)
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-malformed");
			await fixture.EnsureTagAsync();
			fixture.SetRelease(body);

			await Assert.ThrowsAsync<GitHubException>(() =>
				fixture.Service.CreateDraftAsync(
					fixture.Plan,
					fixture.Plan.PlanId,
					TestContext.Current.CancellationToken));
			Assert.Equal(0, fixture.GitHub.UpdateCount);
		}

		[Fact]
		public async Task Already_published_release_is_idempotent_and_untouched()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-published");
			await fixture.EnsureTagAsync();
			const string body = "legacy published body";
			fixture.SetRelease(body, isDraft: false, target: "main");

			var result = await fixture.Service.CreateDraftAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Equal(FinishNextAction.Closeout, result.NextAction);
			Assert.Equal(FinishWriteStatus.AlreadyPublished, result.Operations[1].Status);
			Assert.Equal(body, fixture.GitHub.Release!.Body);
			Assert.Equal(0, fixture.GitHub.UpdateCount);
		}

		[Fact]
		public async Task Conflicting_remote_tag_blocks_before_GitHub_writes()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-tag-conflict");
			File.WriteAllText(
				Path.Combine(fixture.Repository.Root, "file.txt"),
				"different\n");
			var other = await GitRepoTestHelper.CommitAllAsync(
				fixture.Repository.Root,
				"other",
				TestContext.Current.CancellationToken);
			await fixture.Repository.PushTagAsync(
				FinishTestFixture.Tag,
				other,
				cancellationToken: TestContext.Current.CancellationToken);

			await Assert.ThrowsAsync<ConflictException>(() =>
				fixture.Service.CreateDraftAsync(
					fixture.Plan,
					fixture.Plan.PlanId,
					TestContext.Current.CancellationToken));
			Assert.Equal(0, fixture.GitHub.GetCount);
			Assert.Equal(0, fixture.GitHub.CreateCount);
		}

		[Fact]
		public async Task Tag_push_failure_accepts_an_exact_concurrent_tag()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-tag-race-exact");
			var racingRepository = new GitRepository(
				fixture.Repository.Root,
				new TagPushRaceRunner(fixture.SourceCommit));
			var service = new FinishService(
				racingRepository,
				fixture.GitHub,
				fixture.TimeProvider,
				() => FinishTestFixture.PublicationPlanId);

			var result = await service.CreateDraftAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Equal(FinishNextAction.PlanPublication, result.NextAction);
			Assert.Equal(1, fixture.GitHub.CreateCount);
		}

		[Fact]
		public async Task Tag_push_failure_rejects_a_mismatching_concurrent_tag()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-tag-race-wrong");
			File.WriteAllText(
				Path.Combine(fixture.Repository.Root, "other.txt"),
				"other\n");
			var other = await GitRepoTestHelper.CommitAllAsync(
				fixture.Repository.Root,
				"other",
				TestContext.Current.CancellationToken);
			var racingRepository = new GitRepository(
				fixture.Repository.Root,
				new TagPushRaceRunner(other));
			var service = new FinishService(
				racingRepository,
				fixture.GitHub,
				fixture.TimeProvider,
				() => FinishTestFixture.PublicationPlanId);

			await Assert.ThrowsAsync<ConflictException>(() =>
				service.CreateDraftAsync(
					fixture.Plan,
					fixture.Plan.PlanId,
					TestContext.Current.CancellationToken));
			Assert.Equal(0, fixture.GitHub.GetCount);
		}

		[Fact]
		public async Task PlanId_mismatch_is_rejected_before_fetch_or_GitHub_access()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-correlation");

			await Assert.ThrowsAsync<ValidationException>(() =>
				fixture.Service.CreateDraftAsync(
					fixture.Plan,
					Guid.NewGuid(),
					TestContext.Current.CancellationToken));

			Assert.Equal(0, fixture.GitHub.GetCount);
			Assert.Empty(
				await fixture.Repository.RemoteTagsAsync(
					pattern: $"refs/tags/{FinishTestFixture.Tag}",
					cancellationToken: TestContext.Current.CancellationToken));
		}

		[Fact]
		public async Task Missing_exact_release_branch_blocks_before_tag_or_draft_writes()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-source-branch");
			_ = await fixture.Repository.GitAsync(
				["push", "origin", $":refs/heads/{FinishTestFixture.Branch}"],
				cancellationToken: TestContext.Current.CancellationToken);

			await Assert.ThrowsAsync<ConflictException>(() =>
				fixture.Service.CreateDraftAsync(
					fixture.Plan,
					fixture.Plan.PlanId,
					TestContext.Current.CancellationToken));

			Assert.Equal(0, fixture.GitHub.GetCount);
			Assert.Empty(
				await fixture.Repository.RemoteTagsAsync(
					pattern: $"refs/tags/{FinishTestFixture.Tag}",
					cancellationToken: TestContext.Current.CancellationToken));
		}

		[Fact]
		public async Task Create_draft_requires_create_draft_next_action()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-action");
			var invalid = fixture.Plan with { NextAction = FinishNextAction.PlanPublication };

			await Assert.ThrowsAsync<ValidationException>(() =>
				fixture.Service.CreateDraftAsync(
					invalid,
					invalid.PlanId,
					TestContext.Current.CancellationToken));
			Assert.Equal(0, fixture.GitHub.GetCount);
		}

		[Fact]
		public async Task Create_reread_failure_is_typed()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-reread");
			fixture.GitHub.ThrowOnGetCount = 2;

			await Assert.ThrowsAsync<GitHubException>(() =>
				fixture.Service.CreateDraftAsync(
					fixture.Plan,
					fixture.Plan.PlanId,
					TestContext.Current.CancellationToken));
		}

		[Fact]
		public async Task Publication_plan_binds_exact_UTF8_body_hash_and_identity()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-publication");
			await fixture.EnsureTagAsync();
			var body = ManagedReleaseMarkers.BuildInitialBody("notes \u2603 \U0001F680");
			var release = fixture.SetRelease(body);

			var publication = await fixture.Service.PlanPublicationAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);

			var expected = Convert.ToHexString(
				System.Security.Cryptography.SHA256.HashData(
					Encoding.UTF8.GetBytes(body))).ToLowerInvariant();
			Assert.Equal(expected, publication.BodyHash);
			Assert.Equal("SHA256", publication.BodyHashAlgorithm switch
			{
				BodyHashAlgorithm.Sha256 => "SHA256",
				_ => throw new InvalidOperationException(),
			});
			Assert.Equal(FinishTestFixture.PublicationPlanId, publication.PublicationPlanId);
			Assert.Equal(release.Id, publication.ReleaseId);
			Assert.Equal(FinishNextAction.Publish, publication.NextAction);
			Assert.True(publication.ReadyToPublish);
			FinishPublicationPlanValidator.Validate(publication);
		}

		[Fact]
		public async Task Publication_plan_rejects_empty_generated_notes()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-empty-notes");
			await fixture.EnsureTagAsync();
			fixture.SetRelease(ManagedReleaseMarkers.BuildInitialBody(" \n "));

			await Assert.ThrowsAsync<ConflictException>(() =>
				fixture.Service.PlanPublicationAsync(
					fixture.Plan,
					fixture.Plan.PlanId,
					TestContext.Current.CancellationToken));
		}

		[Theory]
		[InlineData("wrong-title")]
		[InlineData("wrong-target")]
		[InlineData("wrong-prerelease")]
		public async Task Publication_plan_rejects_wrong_live_release_fields(string mismatch)
		{
			using var fixture = await FinishTestFixture.CreateAsync($"finish-{mismatch}");
			await fixture.EnsureTagAsync();
			var body = ManagedReleaseMarkers.BuildInitialBody("notes");
			fixture.SetRelease(
				body,
				title: mismatch == "wrong-title" ? "Wrong" : FinishTestFixture.Title,
				target: mismatch == "wrong-target" ? new string('f', 40) : fixture.SourceCommit,
				prerelease: mismatch == "wrong-prerelease");

			await Assert.ThrowsAsync<ConflictException>(() =>
				fixture.Service.PlanPublicationAsync(
					fixture.Plan,
					fixture.Plan.PlanId,
					TestContext.Current.CancellationToken));
		}

		[Fact]
		public async Task Published_publication_recovery_routes_to_closeout()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-publication-published");
			await fixture.EnsureTagAsync();
			fixture.SetRelease("legacy published", isDraft: false, target: FinishTestFixture.Branch);

			var publication = await fixture.Service.PlanPublicationAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Equal(FinishNextAction.Closeout, publication.NextAction);
			Assert.True(publication.IsPublished);
			Assert.False(publication.ReadyToPublish);
		}

		[Fact]
		public async Task Publish_sends_the_existing_body_unchanged_and_verifies_it()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-publish");
			await fixture.EnsureTagAsync();
			var body = ManagedReleaseMarkers.BuildInitialBody("notes\nwith exact whitespace  ");
			fixture.SetRelease(body);
			var publication = await fixture.Service.PlanPublicationAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);

			var result = await fixture.Service.PublishAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				publication,
				publication.PublicationPlanId,
				TestContext.Current.CancellationToken);

			Assert.Equal(body, fixture.GitHub.PublishedBody);
			Assert.False(fixture.GitHub.Release!.IsDraft);
			Assert.Equal(FinishWriteStatus.Published, Assert.Single(result.Operations).Status);
			Assert.Equal(FinishNextAction.Closeout, result.NextAction);
			Assert.Equal(publication.PublicationPlanId, result.PublicationPlanId);
			Assert.Equal(publication.BodyHash, result.BodyHash);
		}

		[Fact]
		public async Task Body_edit_after_approval_blocks_before_publish()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-body-edit");
			await fixture.EnsureTagAsync();
			fixture.SetRelease(ManagedReleaseMarkers.BuildInitialBody("approved"));
			var publication = await fixture.Service.PlanPublicationAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);
			fixture.GitHub.Release = fixture.GitHub.Release! with
			{
				Body = ManagedReleaseMarkers.BuildInitialBody("edited"),
			};

			await Assert.ThrowsAsync<ConflictException>(() =>
				fixture.Service.PublishAsync(
					fixture.Plan,
					fixture.Plan.PlanId,
					publication,
					publication.PublicationPlanId,
					TestContext.Current.CancellationToken));
			Assert.Equal(0, fixture.GitHub.PublishCount);
		}

		[Fact]
		public async Task Published_between_approval_and_apply_succeeds_only_when_exact()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-publish-race");
			await fixture.EnsureTagAsync();
			fixture.SetRelease(ManagedReleaseMarkers.BuildInitialBody("approved"));
			var publication = await fixture.Service.PlanPublicationAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);
			fixture.GitHub.Release = fixture.GitHub.Release! with { IsDraft = false };

			var result = await fixture.Service.PublishAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				publication,
				publication.PublicationPlanId,
				TestContext.Current.CancellationToken);

			Assert.Equal(FinishWriteStatus.AlreadyPublished, Assert.Single(result.Operations).Status);
			Assert.Equal(0, fixture.GitHub.PublishCount);

			fixture.GitHub.Release = fixture.GitHub.Release with { Body = "changed" };
			var editedPublished = await fixture.Service.PublishAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				publication,
				publication.PublicationPlanId,
				TestContext.Current.CancellationToken);
			Assert.Equal(FinishWriteStatus.AlreadyPublished, Assert.Single(editedPublished.Operations).Status);
		}

		[Fact]
		public async Task Published_retry_accepts_legacy_branch_target_but_rejects_wrong_exact_SHA()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-published-legacy");
			await fixture.EnsureTagAsync();
			fixture.SetRelease(ManagedReleaseMarkers.BuildInitialBody("approved"));
			var publication = await fixture.Service.PlanPublicationAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);
			fixture.GitHub.Release = fixture.GitHub.Release! with
			{
				IsDraft = false,
				TargetCommitish = "main",
				Body = "published body edited later",
			};

			var result = await fixture.Service.PublishAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				publication,
				publication.PublicationPlanId,
				TestContext.Current.CancellationToken);
			Assert.Equal(FinishWriteStatus.AlreadyPublished, Assert.Single(result.Operations).Status);

			fixture.GitHub.Release = fixture.GitHub.Release with
			{
				TargetCommitish = new string('f', 40),
			};
			await Assert.ThrowsAsync<ConflictException>(() =>
				fixture.Service.PublishAsync(
					fixture.Plan,
					fixture.Plan.PlanId,
					publication,
					publication.PublicationPlanId,
					TestContext.Current.CancellationToken));
		}

		[Fact]
		public async Task Publish_rejects_wrong_publication_correlations_before_GitHub_access()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-publish-correlation");
			await fixture.EnsureTagAsync();
			fixture.SetRelease(ManagedReleaseMarkers.BuildInitialBody("approved"));
			var publication = await fixture.Service.PlanPublicationAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);
			var reads = fixture.GitHub.GetCount;

			await Assert.ThrowsAsync<ValidationException>(() =>
				fixture.Service.PublishAsync(
					fixture.Plan,
					fixture.Plan.PlanId,
					publication,
					Guid.NewGuid(),
					TestContext.Current.CancellationToken));
			Assert.Equal(reads, fixture.GitHub.GetCount);

			var otherPlan = fixture.Plan with { PlanId = Guid.NewGuid() };
			await Assert.ThrowsAsync<ConflictException>(() =>
				fixture.Service.PublishAsync(
					otherPlan,
					otherPlan.PlanId,
					publication,
					publication.PublicationPlanId,
					TestContext.Current.CancellationToken));
			Assert.Equal(reads, fixture.GitHub.GetCount);
		}

		[Fact]
		public async Task Publish_rejects_recreated_release_identity()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-release-id");
			await fixture.EnsureTagAsync();
			fixture.SetRelease(ManagedReleaseMarkers.BuildInitialBody("approved"));
			var publication = await fixture.Service.PlanPublicationAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);
			fixture.GitHub.Release = fixture.GitHub.Release! with { Id = 999 };

			await Assert.ThrowsAsync<ConflictException>(() =>
				fixture.Service.PublishAsync(
					fixture.Plan,
					fixture.Plan.PlanId,
					publication,
					publication.PublicationPlanId,
					TestContext.Current.CancellationToken));
		}

		private sealed class TagPushRaceRunner(string raceSha) : IProcessRunner
		{
			private readonly ProcessRunner inner = new();
			private bool raced;

			public async Task<ProcessRunResult> RunAsync(
				IReadOnlyList<string> arguments,
				string workingDirectory,
				bool checkExitCode = true,
				TimeSpan? timeout = null,
				string? standardInput = null,
				CancellationToken cancellationToken = default)
			{
				if (!raced &&
					arguments.Count == 4 &&
					arguments[0] == "git" &&
					arguments[1] == "push" &&
					arguments[3].Contains(":refs/tags/", StringComparison.Ordinal))
				{
					raced = true;
					var separator = arguments[3].IndexOf(':');
					_ = await inner.RunAsync(
						[
							"git",
							"push",
							arguments[2],
							$"{raceSha}{arguments[3][separator..]}",
						],
						workingDirectory,
						cancellationToken: cancellationToken);
					throw new GitException("simulated tag push race");
				}
				return await inner.RunAsync(
					arguments,
					workingDirectory,
					checkExitCode,
					timeout,
					standardInput,
					cancellationToken);
			}
		}
	}
}
