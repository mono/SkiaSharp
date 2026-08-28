using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Git;
using SkiaSharp.ReleaseTool.Model;
using SkiaSharp.ReleaseTool.Planning;
using SkiaSharp.ReleaseTool.Tests.Contracts;
using SkiaSharp.ReleaseTool.Tests.Git;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Planning
{
	public sealed class PreparePlanApplierTests
	{
		private static readonly string SkiaSha = new('b', 40);

		[Fact]
		public async Task First_preview_applies_all_refs_and_second_apply_is_a_no_op()
		{
			using var fixture = await ApplyFixture.CreateAsync("apply-first-preview");
			var plan = await fixture.PlanAsync();
			var applier = new PreparePlanApplier(fixture.Repository, fixture.GitHub);

			var first = await applier.ApplyAsync(
				plan,
				plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Equal(PrepareNextAction.Done, first.NextAction);
			Assert.All(first.Operations, operation => Assert.Equal(ApplyOperationStatus.Done, operation.Status));
			Assert.Equal(
				fixture.MainSha,
				await fixture.Repository.RemoteShaAsync(
					"release/3.119.x",
					cancellationToken: TestContext.Current.CancellationToken));
			Assert.Equal(
				SkiaSha,
				fixture.GitHub.Refs["mono/skia:refs/heads/release/3.119.0-preview.1"]);
			var releaseSha = await fixture.Repository.RemoteShaAsync(
				"release/3.119.0-preview.1",
				cancellationToken: TestContext.Current.CancellationToken);
			Assert.NotNull(releaseSha);
			Assert.Equal(
				("3.119.0", "preview.1"),
				await fixture.ReadSkiaStateAsync(releaseSha));
			var commitMessage = (await fixture.Repository.GitAsync(
				["show", "-s", "--format=%B", releaseSha],
				cancellationToken: TestContext.Current.CancellationToken))
				.StandardOutput;
			Assert.Contains($"Release-Base: {fixture.MainSha}", commitMessage);
			Assert.Contains($"Release-Skia: {SkiaSha}", commitMessage);

			var second = await applier.ApplyAsync(
				plan,
				plan.PlanId,
				TestContext.Current.CancellationToken);
			Assert.Equal(PrepareNextAction.Done, second.NextAction);
			Assert.Single(fixture.GitHub.CreatedRefs);
			Assert.Equal(
				releaseSha,
				await fixture.Repository.RemoteShaAsync(
					"release/3.119.0-preview.1",
					cancellationToken: TestContext.Current.CancellationToken));
		}

		[Fact]
		public async Task Current_RC_resume_creates_only_maintenance_from_preview_zero_main()
		{
			using var fixture = await ApplyFixture.CreateAsync("apply-rc-resume");
			var rcTip = await fixture.CreateAdvancedReleaseAsync(
				"release/3.119.0-rc.1",
				"rc.1");
			fixture.GitHub.Refs["mono/skia:refs/heads/release/3.119.0-rc.1"] = SkiaSha;
			var plan = await fixture.PlanAsync("3.119.0-rc.1");
			Assert.Equal(rcTip, plan.Base.Sha);
			Assert.Equal(fixture.MainSha, plan.MaintenanceBranch.BaseSha);

			var result = await new PreparePlanApplier(
				fixture.Repository,
				fixture.GitHub).ApplyAsync(
					plan,
					plan.PlanId,
					TestContext.Current.CancellationToken);

			Assert.Equal(PrepareNextAction.Done, result.NextAction);
			Assert.Empty(fixture.GitHub.CreatedRefs);
			Assert.Equal(
				fixture.MainSha,
				await fixture.Repository.RemoteShaAsync(
					"release/3.119.x",
					cancellationToken: TestContext.Current.CancellationToken));
			Assert.Equal(
				rcTip,
				await fixture.Repository.RemoteShaAsync(
					"release/3.119.0-rc.1",
					cancellationToken: TestContext.Current.CancellationToken));
		}

		[Fact]
		public async Task Failure_after_maintenance_is_retryable()
		{
			using var fixture = await ApplyFixture.CreateAsync("apply-retry");
			var plan = await fixture.PlanAsync();
			fixture.GitHub.CreateRefFailure = (_, _, _) =>
				new GitHubException("injected ref failure");
			var applier = new PreparePlanApplier(fixture.Repository, fixture.GitHub);

			await Assert.ThrowsAsync<GitHubException>(
				() => applier.ApplyAsync(
					plan,
					plan.PlanId,
					TestContext.Current.CancellationToken));
			Assert.Equal(
				fixture.MainSha,
				await fixture.Repository.RemoteShaAsync(
					"release/3.119.x",
					cancellationToken: TestContext.Current.CancellationToken));
			Assert.Null(await fixture.Repository.RemoteShaAsync(
				"release/3.119.0-preview.1",
				cancellationToken: TestContext.Current.CancellationToken));

			fixture.GitHub.CreateRefFailure = null;
			var result = await applier.ApplyAsync(
				plan,
				plan.PlanId,
				TestContext.Current.CancellationToken);
			Assert.Equal(PrepareNextAction.Done, result.NextAction);
		}

		[Fact]
		public async Task Partial_apply_with_existing_Skia_ref_resumes_release_creation()
		{
			using var fixture = await ApplyFixture.CreateAsync("apply-partial-skia");
			var plan = await fixture.PlanAsync();
			fixture.GitHub.Refs[
				"mono/skia:refs/heads/release/3.119.0-preview.1"] = SkiaSha;

			var result = await new PreparePlanApplier(
				fixture.Repository,
				fixture.GitHub).ApplyAsync(
					plan,
					plan.PlanId,
					TestContext.Current.CancellationToken);

			Assert.Equal(PrepareNextAction.Done, result.NextAction);
			Assert.Empty(fixture.GitHub.CreatedRefs);
			Assert.NotNull(await fixture.Repository.RemoteShaAsync(
				"release/3.119.0-preview.1",
				cancellationToken: TestContext.Current.CancellationToken));
		}

		[Fact]
		public async Task Conflicting_Skia_ref_is_blocking()
		{
			using var fixture = await ApplyFixture.CreateAsync("apply-skia-conflict");
			await fixture.CreateMaintenanceAsync();
			var plan = await fixture.PlanAsync("3.119.0-preview.2");
			fixture.GitHub.Refs[
				"mono/skia:refs/heads/release/3.119.0-preview.2"] = new string('c', 40);

			await Assert.ThrowsAsync<ConflictException>(
				() => new PreparePlanApplier(
					fixture.Repository,
					fixture.GitHub).ApplyAsync(
						plan,
						plan.PlanId,
						TestContext.Current.CancellationToken));
		}

		[Fact]
		public async Task Existing_release_branch_requires_ancestry_and_version_state()
		{
			using var fixture = await ApplyFixture.CreateAsync("apply-release-conflict");
			await fixture.CreateMaintenanceAsync();
			var plan = await fixture.PlanAsync("3.119.0-preview.2");
			await fixture.CreateAdvancedReleaseAsync(
				"release/3.119.0-preview.2",
				"preview.3");
			fixture.GitHub.Refs[
				"mono/skia:refs/heads/release/3.119.0-preview.2"] = SkiaSha;

			await Assert.ThrowsAsync<ConflictException>(
				() => new PreparePlanApplier(
					fixture.Repository,
					fixture.GitHub).ApplyAsync(
						plan,
						plan.PlanId,
						TestContext.Current.CancellationToken));
		}

		[Fact]
		public async Task Existing_unrelated_release_branch_is_rejected()
		{
			using var fixture = await ApplyFixture.CreateAsync("apply-release-ancestry");
			await fixture.CreateMaintenanceAsync();
			var plan = await fixture.PlanAsync("3.119.0-preview.2");
			await fixture.CreateUnrelatedReleaseAsync(
				"release/3.119.0-preview.2");
			fixture.GitHub.Refs[
				"mono/skia:refs/heads/release/3.119.0-preview.2"] = SkiaSha;

			await Assert.ThrowsAsync<ConflictException>(
				() => new PreparePlanApplier(
					fixture.Repository,
					fixture.GitHub).ApplyAsync(
						plan,
						plan.PlanId,
						TestContext.Current.CancellationToken));
		}

		[Fact]
		public async Task Stable_apply_creates_verified_bump_branch_and_template_PR()
		{
			using var fixture = await ApplyFixture.CreateAsync("apply-stable");
			await fixture.CreateMaintenanceAsync();
			var plan = await fixture.PlanAsync("3.119.0");
			var applier = new PreparePlanApplier(fixture.Repository, fixture.GitHub);

			var result = await applier.ApplyAsync(
				plan,
				plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Equal(PrepareNextAction.AwaitMerge, result.NextAction);
			Assert.NotNull(result.StableBumpPullRequestUrl);
			var created = Assert.Single(fixture.GitHub.CreatedPullRequests);
			Assert.Equal("bump-version-3.119.1", created.Head);
			Assert.Equal("release/3.119.x", created.Base);
			Assert.Contains("## Description", created.Body);
			Assert.Contains("Required skia PR: None.", created.Body);
			Assert.Contains("## Changes", created.Body);
			Assert.Contains("## Testing", created.Body);
			Assert.Contains("## Areas Affected", created.Body);
			var bumpSha = await fixture.Repository.RemoteShaAsync(
				"bump-version-3.119.1",
				cancellationToken: TestContext.Current.CancellationToken);
			Assert.Equal(
				("3.119.1", "preview.0"),
				await fixture.ReadSkiaStateAsync(bumpSha!));

			var retry = await applier.ApplyAsync(
				plan,
				plan.PlanId,
				TestContext.Current.CancellationToken);
			Assert.Equal(PrepareNextAction.AwaitMerge, retry.NextAction);
			Assert.Single(fixture.GitHub.CreatedPullRequests);
		}

		[Fact]
		public async Task Existing_stale_bump_branch_is_rejected()
		{
			using var fixture = await ApplyFixture.CreateAsync("apply-stale-bump");
			await fixture.CreateMaintenanceAsync();
			var plan = await fixture.PlanAsync("3.119.0");
			await fixture.Repository.UpdateLocalBranchAsync(
				"bump-version-3.119.1",
				fixture.MainSha,
				TestContext.Current.CancellationToken);
			await fixture.Repository.PushBranchAsync(
				"bump-version-3.119.1",
				cancellationToken: TestContext.Current.CancellationToken);

			await Assert.ThrowsAsync<ConflictException>(
				() => new PreparePlanApplier(
					fixture.Repository,
					fixture.GitHub).ApplyAsync(
						plan,
						plan.PlanId,
						TestContext.Current.CancellationToken));
		}

		[Fact]
		public async Task Already_advanced_integration_completes_without_a_PR()
		{
			var plan = PlanSamples.StablePrepare();
			var repository = new FakePrepareRepository(
				Path.Combine(Environment.CurrentDirectory, "unused"));
			repository.AddRef(
				plan.ToolingSha,
				plan.ToolingSha,
				new TestVersionState("3.119.0", "1.8.8.3", "stable"),
				plan.Skia.Sha);
			repository.AddRef(
				plan.Base.Ref,
				plan.Base.Sha,
				new TestVersionState("3.119.1", "1.8.8.4", "preview.0"),
				plan.Skia.Sha);
			var releaseSha = new string('d', 40);
			repository.AddRemoteRelease(
				plan.Release.Branch,
				releaseSha,
				new TestVersionState("3.119.0", "1.8.8.3", "stable"));
			var github = new FakePrepareGitHubClient();
			github.Refs[
				$"mono/skia:refs/heads/{plan.Release.Branch}"] = plan.Skia.Sha;

			var result = await new PreparePlanApplier(
				repository,
				github).ApplyAsync(
					plan,
					plan.PlanId,
					TestContext.Current.CancellationToken);

			Assert.Equal(PrepareNextAction.Done, result.NextAction);
			Assert.Null(result.StableBumpPullRequestUrl);
			Assert.Empty(github.CreatedPullRequests);
		}

		[Fact]
		public async Task Retry_reuses_PR_created_before_a_result_failure()
		{
			using var fixture = await ApplyFixture.CreateAsync("apply-pr-retry");
			await fixture.CreateMaintenanceAsync();
			var plan = await fixture.PlanAsync("3.119.0");
			fixture.GitHub.CreatePullRequestFailure = (head, @base) =>
			{
				fixture.GitHub.PullRequests[(head, @base)] = new PullRequestInfo(
					42,
					new Uri("https://example.invalid/pr/42"));
				return new GitHubException("injected post-create failure");
			};
			var applier = new PreparePlanApplier(fixture.Repository, fixture.GitHub);

			await Assert.ThrowsAsync<GitHubException>(
				() => applier.ApplyAsync(
					plan,
					plan.PlanId,
					TestContext.Current.CancellationToken));

			fixture.GitHub.CreatePullRequestFailure = null;
			var retry = await applier.ApplyAsync(
				plan,
				plan.PlanId,
				TestContext.Current.CancellationToken);
			Assert.Equal(
				new Uri("https://example.invalid/pr/42"),
				retry.StableBumpPullRequestUrl);
			Assert.Empty(fixture.GitHub.CreatedPullRequests);
		}

		[Fact]
		public async Task Hotfix_apply_skips_maintenance_and_stable_bump()
		{
			using var fixture = await ApplyFixture.CreateAsync("apply-hotfix");
			_ = await fixture.Repository.GitAsync(
				["tag", "v3.119.0", fixture.MainSha],
				cancellationToken: TestContext.Current.CancellationToken);
			var plan = await fixture.PlanAsync("3.119.0.1-preview.1");

			var result = await new PreparePlanApplier(
				fixture.Repository,
				fixture.GitHub).ApplyAsync(
					plan,
					plan.PlanId,
					TestContext.Current.CancellationToken);

			Assert.Equal(
				ApplyOperationStatus.Skipped,
				Assert.Single(
					result.Operations,
					operation => operation.Id == PlanOperationId.CreateMaintenanceBranch).Status);
			Assert.DoesNotContain(
				result.Operations,
				operation => operation.Id == PlanOperationId.OpenStableBumpPullRequest);
			Assert.Equal(PrepareNextAction.Done, result.NextAction);
		}

		[Fact]
		public async Task Plan_id_mismatch_and_dirty_worktree_are_rejected()
		{
			using var fixture = await ApplyFixture.CreateAsync("apply-gates");
			var plan = await fixture.PlanAsync();
			var applier = new PreparePlanApplier(fixture.Repository, fixture.GitHub);

			await Assert.ThrowsAsync<ValidationException>(
				() => applier.ApplyAsync(
					plan,
					Guid.NewGuid(),
					TestContext.Current.CancellationToken));

			File.WriteAllText(Path.Combine(fixture.Repository.Root, "dirty.txt"), "dirty");
			await Assert.ThrowsAsync<GitException>(
				() => applier.ApplyAsync(
					plan,
					plan.PlanId,
					TestContext.Current.CancellationToken));
		}

		internal sealed class ApplyFixture : IDisposable
		{
			private readonly TestDirectory root;

			private ApplyFixture(
				TestDirectory root,
				GitRepository repository,
				FakePrepareGitHubClient github,
				string mainSha)
			{
				this.root = root;
				Repository = repository;
				GitHub = github;
				MainSha = mainSha;
			}

			public GitRepository Repository { get; }
			public FakePrepareGitHubClient GitHub { get; }
			public string MainSha { get; }

			public static async Task<ApplyFixture> CreateAsync(string purpose)
			{
				var root = new TestDirectory(purpose);
				var (_, worktree) = await GitRepoTestHelper.CreateBareAndWorktreeAsync(
					root.Path,
					"repo",
					TestContext.Current.CancellationToken);
				WriteVersionFiles(worktree, "3.119.0", "1.8.8", "preview.0");
				await GitRepoTestHelper.AddGitlinkAsync(
					worktree,
					"externals/skia",
					SkiaSha,
					TestContext.Current.CancellationToken);
				await GitRepoTestHelper.StageAsync(
					worktree,
					TestContext.Current.CancellationToken,
					"scripts",
					".gitmodules");
				var mainSha = await GitRepoTestHelper.CommitStagedAsync(
					worktree,
					"seed main",
					TestContext.Current.CancellationToken);
				await GitRepoTestHelper.PushAsync(
					worktree,
					TestContext.Current.CancellationToken);
				var repository = new GitRepository(worktree);
				await repository.FetchAsync(
					cancellationToken: TestContext.Current.CancellationToken);
				return new ApplyFixture(
					root,
					repository,
					new FakePrepareGitHubClient(),
					mainSha);
			}

			public async Task<PreparePlan> PlanAsync(string? version = null)
			{
				await Repository.SwitchAsync(
					"main",
					TestContext.Current.CancellationToken);
				await Repository.FetchAsync(
					cancellationToken: TestContext.Current.CancellationToken);
				return await new PreparePlanBuilder(
					Repository,
					GitHub,
					new FixedTimeProvider(),
					() => Guid.Parse("6a82a53c-af48-4d2f-a915-b9943398cc34")).BuildAsync(
						new PreparePlanRequest("main", version, null, MainSha),
						TestContext.Current.CancellationToken);
			}

			public async Task CreateMaintenanceAsync()
			{
				await Repository.UpdateLocalBranchAsync(
					"release/3.119.x",
					MainSha,
					TestContext.Current.CancellationToken);
				await Repository.PushBranchAsync(
					"release/3.119.x",
					cancellationToken: TestContext.Current.CancellationToken);
				await Repository.FetchAsync(
					cancellationToken: TestContext.Current.CancellationToken);
			}

			public async Task<string> CreateAdvancedReleaseAsync(
				string branch,
				string label)
			{
				await Repository.SwitchCreateAsync(
					branch,
					MainSha,
					TestContext.Current.CancellationToken);
				var variables = await Repository.ReadWorktreeFileAsync(
					VersionFileEditor.VariablesPath,
					TestContext.Current.CancellationToken);
				var versions = await Repository.ReadWorktreeFileAsync(
					VersionFileEditor.VersionsPath,
					TestContext.Current.CancellationToken);
				var edits = VersionFileEditor.ComputeEdits(variables, versions, label);
				await Repository.WriteWorktreeFileAsync(
					VersionFileEditor.VariablesPath,
					edits.NewVariablesText,
					TestContext.Current.CancellationToken);
				_ = await Repository.CommitAsync(
					$"Bump the version to 3.119.0-{label}",
					edits.ChangedPaths,
					TestContext.Current.CancellationToken);
				File.WriteAllText(
					Path.Combine(Repository.Root, "ci-fix.txt"),
					"follow-up");
				var tip = await Repository.CommitAsync(
					"Merge main and fix CI",
					["ci-fix.txt"],
					TestContext.Current.CancellationToken);
				await Repository.PushBranchAsync(
					branch,
					cancellationToken: TestContext.Current.CancellationToken);
				await Repository.SwitchAsync(
					"main",
					TestContext.Current.CancellationToken);
				await Repository.FetchAsync(
					cancellationToken: TestContext.Current.CancellationToken);
				return tip;
			}

			public async Task CreateUnrelatedReleaseAsync(string branch)
			{
				var tree = (await Repository.GitAsync(
					["rev-parse", $"{MainSha}^{{tree}}"],
					cancellationToken: TestContext.Current.CancellationToken))
					.StandardOutput.Trim();
				var commit = (await Repository.GitAsync(
					["commit-tree", tree, "-m", "unrelated release"],
					cancellationToken: TestContext.Current.CancellationToken))
					.StandardOutput.Trim();
				await Repository.UpdateLocalBranchAsync(
					branch,
					commit,
					TestContext.Current.CancellationToken);
				await Repository.PushBranchAsync(
					branch,
					cancellationToken: TestContext.Current.CancellationToken);
			}

			public async Task<(string Version, string Label)> ReadSkiaStateAsync(
				string reference)
			{
				var variables = await Repository.ReadRefFileAsync(
					reference,
					VersionFileEditor.VariablesPath,
					TestContext.Current.CancellationToken);
				var versions = await Repository.ReadRefFileAsync(
					reference,
					VersionFileEditor.VersionsPath,
					TestContext.Current.CancellationToken);
				var state = VersionStateReader.Parse(variables, versions);
				return (state.Skia.ToNormalizedString(), state.Label);
			}

			public void Dispose() => root.Dispose();

			private static void WriteVersionFiles(
				string root,
				string skia,
				string harfBuzz,
				string label)
			{
				var scripts = Directory.CreateDirectory(Path.Combine(root, "scripts"));
				File.WriteAllText(
					Path.Combine(scripts.FullName, "azure-templates-variables.yml"),
					$"variables:\n  SKIASHARP_VERSION: {skia}\n  PREVIEW_LABEL: '{label}'\n");
				File.WriteAllText(
					Path.Combine(scripts.FullName, "VERSIONS.txt"),
					$"SkiaSharp file {skia}.0\n" +
					$"HarfBuzzSharp file {harfBuzz}\n" +
					$"SkiaSharp nuget {skia}\n" +
					$"SkiaSharp.HarfBuzz nuget {skia}\n" +
					$"HarfBuzzSharp nuget {harfBuzz}\n");
			}

			private sealed class FixedTimeProvider : TimeProvider
			{
				public override DateTimeOffset GetUtcNow() =>
					new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);
			}
		}
	}
}
