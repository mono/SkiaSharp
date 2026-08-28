using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Model;
using SkiaSharp.ReleaseTool.Planning;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Planning
{
	public sealed class PreparePlanBuilderTests
	{
		private static readonly DateTimeOffset GeneratedAt =
			new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);
		private static readonly Guid PlanId =
			Guid.Parse("f7c00eec-72b1-46f7-b9ff-0dc424a40528");
		private static readonly string MainSha = new('a', 40);
		private static readonly string SkiaSha = new('b', 40);

		[Fact]
		public async Task New_line_auto_detects_first_preview_and_maintenance_creation()
		{
			using var root = new TestDirectory("prepare-new-line");
			var (repository, github) = NewFixture(root.Path);

			var plan = await BuildAsync(repository, github, null);

			Assert.Equal("3.119.0-preview.1", plan.Release.Version);
			Assert.Equal("refs/remotes/origin/main", plan.Base.Ref);
			Assert.Equal(MaintenanceBranchAction.Create, plan.MaintenanceBranch.Action);
			Assert.False(plan.MaintenanceBranch.Exists);
			Assert.Equal(MainSha, plan.MaintenanceBranch.BaseSha);
			Assert.True(plan.Warnings.Single().Contains("maintenance branch", StringComparison.Ordinal));
			Assert.Equal(PrepareNextAction.Apply, plan.NextAction);
			Assert.Equal(
				PlanOperationStatus.Pending,
				Operation(plan, PlanOperationId.CreateMaintenanceBranch).Status);
			Assert.Equal(
				("mono/skia", "refs/heads/release/3.119.0-preview.1"),
				Assert.Single(github.RefRequests));
		}

		[Fact]
		public async Task Auto_detection_advances_the_highest_existing_preview()
		{
			using var root = new TestDirectory("prepare-next-preview");
			var (repository, github) = NewFixture(root.Path);
			repository.AddRef(
				"refs/remotes/origin/release/3.119.x",
				MainSha,
				new TestVersionState("3.119.0", "1.8.8", "preview.0"),
				SkiaSha);
			repository.ReleaseBranchNames.AddRange(
				[
					"release/3.119.0-preview.1",
					"release/3.119.0-preview.3",
					"release/3.118.0-preview.20",
				]);

			var plan = await new PreparePlanBuilder(
				repository,
				github,
				new FixedTimeProvider(GeneratedAt),
				() => PlanId).BuildAsync(
					new PreparePlanRequest(
						"release/3.119.x",
						null,
						null,
						MainSha),
					TestContext.Current.CancellationToken);

			Assert.Equal("3.119.0-preview.4", plan.Release.Identity);
			Assert.Equal("refs/remotes/origin/release/3.119.x", plan.Base.Ref);
		}

		[Fact]
		public async Task Later_preview_and_rc_use_existing_maintenance_branch()
		{
			using var root = new TestDirectory("prepare-later");
			var (repository, github) = NewFixture(root.Path);
			repository.AddRef(
				"refs/remotes/origin/release/3.119.x",
				MainSha,
				new TestVersionState("3.119.0", "1.8.8", "preview.0"),
				SkiaSha);
			repository.ReleaseBranchNames.Add("release/3.119.0-preview.1");

			var preview = await BuildAsync(repository, github, "3.119.0-preview.2");
			var rc = await BuildAsync(repository, github, "3.119.0-rc.1");

			Assert.Equal("refs/remotes/origin/release/3.119.x", preview.Base.Ref);
			Assert.Equal(MaintenanceBranchAction.None, preview.MaintenanceBranch.Action);
			Assert.True(preview.MaintenanceBranch.Exists);
			Assert.Equal(
				ReleaseKind.ReleaseCandidate,
				SkiaSharpReleaseIdentity.Parse(rc.Release.Identity).ReleaseType);
			Assert.Empty(preview.Warnings);
		}

		[Fact]
		public async Task Stable_plans_next_patch_HarfBuzz_bump_and_open_PR_state()
		{
			using var root = new TestDirectory("prepare-stable");
			var (repository, github) = NewFixture(root.Path);
			repository.AddRef(
				"refs/remotes/origin/release/3.119.x",
				MainSha,
				new TestVersionState("3.119.0", "1.8.8", "preview.0"),
				SkiaSha);
			github.PullRequests[("bump-version-3.119.1", "release/3.119.x")] =
				new PullRequestInfo(7, new Uri("https://example.invalid/pr/7"));

			var plan = await BuildAsync(repository, github, "3.119.0");

			Assert.NotNull(plan.StableBump);
			Assert.Equal("3.119.1", plan.StableBump.SkiaSharpVersion);
			Assert.Equal("1.8.8.1", plan.StableBump.HarfBuzzSharpVersion);
			Assert.Equal(PlanOperationStatus.AwaitingUser, plan.StableBump.Status);
			Assert.Equal(new Uri("https://example.invalid/pr/7"), plan.StableBump.PullRequestUrl);
			Assert.Equal(PrepareNextAction.Apply, plan.NextAction);
		}

		[Fact]
		public async Task Existing_release_and_open_bump_PR_yield_await_merge()
		{
			using var root = new TestDirectory("prepare-await-merge");
			var (repository, github) = NewFixture(root.Path);
			repository.AddRef(
				"refs/remotes/origin/release/3.119.x",
				MainSha,
				new TestVersionState("3.119.0", "1.8.8", "preview.0"),
				SkiaSha);
			var releaseSha = new string('c', 40);
			repository.AddRemoteRelease(
				"release/3.119.0",
				releaseSha,
				new TestVersionState("3.119.0", "1.8.8", "stable"));
			github.Refs["mono/skia:refs/heads/release/3.119.0"] = SkiaSha;
			github.PullRequests[("bump-version-3.119.1", "release/3.119.x")] =
				new PullRequestInfo(7, new Uri("https://example.invalid/pr/7"));

			var plan = await BuildAsync(repository, github, "3.119.0");

			Assert.Equal(PrepareNextAction.AwaitMerge, plan.NextAction);
			Assert.Equal(
				PlanOperationStatus.AwaitingUser,
				Operation(plan, PlanOperationId.OpenStableBumpPullRequest).Status);
		}

		[Fact]
		public async Task Missing_maintenance_recovers_from_latest_matching_prerelease()
		{
			using var root = new TestDirectory("prepare-recovery");
			var (repository, github) = NewFixture(root.Path);
			var rcSha = new string('c', 40);
			repository.AddRef(
				"refs/remotes/origin/release/3.119.0-preview.4",
				new string('d', 40),
				new TestVersionState("3.119.0", "1.8.8", "preview.4"),
				SkiaSha);
			repository.AddRef(
				"refs/remotes/origin/release/3.119.0-rc.1",
				rcSha,
				new TestVersionState("3.119.0", "1.8.8", "rc.1"),
				SkiaSha);
			repository.ReleaseBranchNames.AddRange(
				["release/3.119.0-preview.4", "release/3.119.0-rc.1"]);

			var plan = await BuildAsync(repository, github, "3.119.0");

			Assert.Equal("refs/remotes/origin/release/3.119.0-rc.1", plan.Base.Ref);
			Assert.Equal(rcSha, plan.Base.Sha);
			Assert.Equal(MaintenanceBranchAction.Create, plan.MaintenanceBranch.Action);
			Assert.Equal(MainSha, plan.MaintenanceBranch.BaseSha);
			Assert.Equal(PlanOperationStatus.Pending, plan.StableBump!.Status);
		}

		[Fact]
		public async Task Explicit_approved_base_recovers_when_no_prerelease_exists()
		{
			using var root = new TestDirectory("prepare-approved");
			var (repository, github) = NewFixture(root.Path);
			const string approvedRef = "refs/heads/audited-release-base";
			repository.AddRef(
				approvedRef,
				MainSha,
				new TestVersionState("3.119.0", "1.8.8", "preview.0"),
				SkiaSha);

			var plan = await BuildAsync(
				repository,
				github,
				"3.119.0",
				approvedRef);

			Assert.Equal(MainSha, plan.Base.Sha);
			Assert.Equal(approvedRef, plan.Base.Ref);
			Assert.Equal(approvedRef, plan.Input.ApprovedBase);
			Assert.Equal(MaintenanceBranchAction.Create, plan.MaintenanceBranch.Action);
		}

		[Fact]
		public async Task Hotfix_preview_uses_parent_tag_and_skips_maintenance_creation()
		{
			using var root = new TestDirectory("prepare-hotfix-preview");
			var (repository, github) = NewFixture(root.Path);
			repository.AddRef(
				"refs/tags/v3.119.0",
				MainSha,
				new TestVersionState("3.119.0", "1.8.8", "stable"),
				SkiaSha);

			var plan = await BuildAsync(repository, github, "3.119.0.1-preview.1");

			Assert.Equal("refs/tags/v3.119.0", plan.Base.Ref);
			Assert.True(SkiaSharpReleaseIdentity.Parse(plan.Release.Identity).IsHotfix);
			Assert.False(plan.MaintenanceBranch.Exists);
			Assert.Equal(MaintenanceBranchAction.None, plan.MaintenanceBranch.Action);
			Assert.Equal(
				PlanOperationStatus.Skipped,
				Operation(plan, PlanOperationId.CreateMaintenanceBranch).Status);
			Assert.True(plan.Versions.RequiresPackageBump);
			Assert.Equal(RemoteState.Missing, plan.Skia.RemoteState);
			Assert.Equal(RemoteState.Missing, plan.SkiaSharpRemoteState);
			Assert.Equal(PrepareNextAction.Apply, plan.NextAction);
			Assert.Null(plan.StableBump);
		}

		[Fact]
		public async Task Hotfix_stable_uses_latest_prerelease_and_has_no_stable_bump()
		{
			using var root = new TestDirectory("prepare-hotfix-stable");
			var (repository, github) = NewFixture(root.Path);
			repository.AddRef(
				"refs/remotes/origin/release/3.119.0.1-preview.2",
				MainSha,
				new TestVersionState("3.119.0.1", "1.8.8", "preview.2"),
				SkiaSha);
			repository.AddRef(
				"refs/remotes/origin/release/3.119.0.1-rc.1",
				MainSha,
				new TestVersionState("3.119.0.1", "1.8.8", "rc.1"),
				SkiaSha);
			repository.ReleaseBranchNames.AddRange(
				["release/3.119.0.1-preview.2", "release/3.119.0.1-rc.1"]);

			var plan = await BuildAsync(repository, github, "3.119.0.1");

			Assert.Equal(
				"refs/remotes/origin/release/3.119.0.1-rc.1",
				plan.Base.Ref);
			Assert.Equal("3.119.0.1", plan.Release.Identity);
			Assert.False(plan.MaintenanceBranch.Exists);
			Assert.Equal(MaintenanceBranchAction.None, plan.MaintenanceBranch.Action);
			Assert.Equal(
				PlanOperationStatus.Skipped,
				Operation(plan, PlanOperationId.CreateMaintenanceBranch).Status);
			Assert.False(plan.Versions.RequiresPackageBump);
			Assert.Equal(PrepareNextAction.Apply, plan.NextAction);
			Assert.Null(plan.StableBump);
		}

		[Fact]
		public async Task Existing_advanced_release_branch_is_resumable()
		{
			using var root = new TestDirectory("prepare-existing");
			var (repository, github) = NewFixture(root.Path);
			// This SHA represents the live head after the initial version bump,
			// a main merge, and a later CI/pool fix. The recovery ref and remote
			// branch both resolve to that advanced head.
			var advancedReleaseSha = new string('c', 40);
			repository.AddRef(
				"refs/remotes/origin/release/3.119.0-rc.1",
				advancedReleaseSha,
				new TestVersionState("3.119.0", "1.8.8", "rc.1"),
				SkiaSha);
			repository.AddRemoteRelease(
				"release/3.119.0-rc.1",
				advancedReleaseSha,
				new TestVersionState("3.119.0", "1.8.8", "rc.1"));
			github.Refs[
				"mono/skia:refs/heads/release/3.119.0-rc.1"] = SkiaSha;

			var plan = await BuildAsync(repository, github, "3.119.0-rc.1");
			Assert.Equal(advancedReleaseSha, plan.Base.Sha);
			Assert.Equal(
				"refs/remotes/origin/release/3.119.0-rc.1",
				plan.Base.Ref);
			Assert.Equal(MainSha, plan.MaintenanceBranch.BaseSha);
			Assert.Equal(RemoteState.Matching, plan.SkiaSharpRemoteState);
			Assert.Equal(
				PlanOperationStatus.Done,
				Operation(plan, PlanOperationId.CreateReleaseBranch).Status);
			Assert.Equal(PrepareNextAction.Apply, plan.NextAction);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("refs/remotes/origin/main")]
		public async Task Unsafe_maintenance_base_is_rejected(
			string? approvedBase)
		{
			using var root = new TestDirectory("prepare-unsafe-maintenance");
			var (repository, github) = NewFixture(root.Path);
			repository.AddRef(
				"refs/remotes/origin/main",
				MainSha,
				new TestVersionState("3.119.0", "1.8.8", "rc.1"),
				SkiaSha);

			var exception = await Assert.ThrowsAsync<PlanException>(
				() => BuildAsync(repository, github, "3.119.0", approvedBase));
			Assert.Contains("not a safe maintenance base", exception.Message);
		}

		[Theory]
		[InlineData("3.120.0", "preview.2")]
		[InlineData("3.119.0", "preview.3")]
		public async Task Existing_release_branch_rejects_version_state_drift(
			string liveVersion,
			string liveLabel)
		{
			using var root = new TestDirectory("prepare-existing-drift");
			var (repository, github) = NewFixture(root.Path);
			repository.AddRef(
				"refs/remotes/origin/release/3.119.x",
				MainSha,
				new TestVersionState("3.119.0", "1.8.8", "preview.0"),
				SkiaSha);
			var releaseSha = new string('c', 40);
			repository.AddRemoteRelease(
				"release/3.119.0-preview.2",
				releaseSha,
				new TestVersionState(liveVersion, "1.8.8", liveLabel));

			await Assert.ThrowsAsync<ConflictException>(
				() => BuildAsync(repository, github, "3.119.0-preview.2"));
		}

		[Fact]
		public async Task Branch_and_skia_conflicts_are_rejected()
		{
			using var root = new TestDirectory("prepare-conflicts");
			var (repository, github) = NewFixture(root.Path);
			repository.AddRef(
				"refs/remotes/origin/release/3.119.x",
				MainSha,
				new TestVersionState("3.119.0", "1.8.8", "preview.0"),
				SkiaSha);
			var releaseSha = new string('c', 40);
			repository.AddRemoteRelease(
				"release/3.119.0-preview.2",
				releaseSha,
				new TestVersionState("3.119.0", "1.8.8", "preview.2"));
			repository.RejectAncestry(MainSha, releaseSha);

			await Assert.ThrowsAsync<ConflictException>(
				() => BuildAsync(repository, github, "3.119.0-preview.2"));

			var (skiaRepository, skiaGithub) = NewFixture(root.Path);
			skiaRepository.AddRef(
				"refs/remotes/origin/release/3.119.x",
				MainSha,
				new TestVersionState("3.119.0", "1.8.8", "preview.0"),
				SkiaSha);
			skiaGithub.Refs[
				"mono/skia:refs/heads/release/3.119.0-preview.2"] = new string('d', 40);
			await Assert.ThrowsAsync<ConflictException>(
				() => BuildAsync(skiaRepository, skiaGithub, "3.119.0-preview.2"));
		}

		[Fact]
		public async Task Invalid_target_is_rejected()
		{
			using var root = new TestDirectory("prepare-invalid");
			var (repository, github) = NewFixture(root.Path);

			await Assert.ThrowsAsync<PlanException>(
				() => new PreparePlanBuilder(repository, github).BuildAsync(
					new PreparePlanRequest(
						"release/3.118.x",
						"3.119.0-preview.2",
						null,
						MainSha),
					TestContext.Current.CancellationToken));
		}

		[Fact]
		public async Task Manual_Python_oracle_projection_matches_without_volatile_fields()
		{
			using var root = new TestDirectory("prepare-differential");
			var (repository, github) = NewFixture(root.Path);

			var plan = await BuildAsync(repository, github, null);
			var projection = new
			{
				plan.Operation,
				plan.NextAction,
				plan.Input,
				plan.Release,
				plan.Base,
				plan.MaintenanceBranch,
				plan.Skia,
				plan.SkiaSharpRemoteState,
				plan.Versions,
				Operations = plan.Operations.Select(operation =>
					(operation.Id, operation.Kind, operation.Status, operation.Detail)).ToArray(),
				plan.StableBump,
				plan.Warnings,
			};
			var expected = new
			{
				Operation = ReleaseOperation.Prepare,
				NextAction = PrepareNextAction.Apply,
				Input = new PrepareInput("main", null, null),
				Release = new PrepareReleaseInfo(
					"3.119.0-preview.1",
					"3.119.0-preview.1",
					"release/3.119.0-preview.1"),
				Base = new PrepareBaseInfo("refs/remotes/origin/main", MainSha),
				MaintenanceBranch = new MaintenanceBranchInfo(
					"release/3.119.x",
					false,
					MaintenanceBranchAction.Create,
					MainSha),
				Skia = new PrepareSkiaInfo(
					SkiaSha,
					RemoteState.Missing),
				SkiaSharpRemoteState = RemoteState.Missing,
				Versions = new PrepareVersionsInfo(false),
				Operations = new[]
				{
					(PlanOperationId.CreateMaintenanceBranch, PlanOperationKind.GitRef, PlanOperationStatus.Pending, "release/3.119.x"),
					(PlanOperationId.CreateSkiaRef, PlanOperationKind.GitHubRef, PlanOperationStatus.Pending, $"mono/skia:release/3.119.0-preview.1@{SkiaSha}"),
					(PlanOperationId.CreateReleaseBranch, PlanOperationKind.GitRef, PlanOperationStatus.Pending, "mono/SkiaSharp:release/3.119.0-preview.1"),
				},
				StableBump = (StableBumpInfo?)null,
				Warnings = (IReadOnlyList<string>)
				[$"maintenance branch release/3.119.x does not exist and will be created from {MainSha}"],
			};

			Assert.Equivalent(expected, projection, strict: true);
		}

		private static (FakePrepareRepository Repository, FakePrepareGitHubClient GitHub)
			NewFixture(string root)
		{
			var repository = new FakePrepareRepository(root);
			repository.AddRef(
				"refs/remotes/origin/main",
				MainSha,
				new TestVersionState("3.119.0", "1.8.8", "preview.0"),
				SkiaSha);
			return (repository, new FakePrepareGitHubClient());
		}

		private static Task<PreparePlan> BuildAsync(
			FakePrepareRepository repository,
			FakePrepareGitHubClient github,
			string? version,
			string? approvedBase = null) =>
			new PreparePlanBuilder(
				repository,
				github,
				new FixedTimeProvider(GeneratedAt),
				() => PlanId).BuildAsync(
					new PreparePlanRequest("main", version, approvedBase, MainSha),
					TestContext.Current.CancellationToken);

		private static PlanOperation Operation(PreparePlan plan, PlanOperationId id) =>
			Assert.Single(plan.Operations, operation => operation.Id == id);

		private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
		{
			public override DateTimeOffset GetUtcNow() => value;
		}
	}
}
