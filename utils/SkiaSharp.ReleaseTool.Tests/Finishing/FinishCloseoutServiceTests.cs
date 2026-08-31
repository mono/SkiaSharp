using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Finishing;
using SkiaSharp.ReleaseTool.Milestones;
using SkiaSharp.ReleaseTool.Model;
using SkiaSharp.ReleaseTool.Planning;
using SkiaSharp.ReleaseTool.Tests.Contracts;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Finishing
{
	public sealed class FinishCloseoutServiceTests
	{
		[Theory]
		[InlineData("missing-source")]
		[InlineData("missing-tooling")]
		[InlineData("missing-branch")]
		[InlineData("branch-does-not-contain-source")]
		[InlineData("missing-tag")]
		[InlineData("moved-tag")]
		[InlineData("missing-release")]
		[InlineData("draft-release")]
		[InlineData("wrong-release-title")]
		public async Task Shipped_gate_blocks_before_milestone_or_dispatch_access(string failure)
		{
			var setup = Setup();
			switch (failure)
			{
				case "missing-source":
					setup.Repository.Commits.Remove(setup.Plan.Receipt.SourceCommit);
					break;
				case "missing-tooling":
					setup.Repository.Commits.Remove(setup.Plan.ToolingSha);
					break;
				case "missing-branch":
					setup.Repository.RemoteBranches.Clear();
					break;
				case "branch-does-not-contain-source":
					setup.Repository.Ancestor = false;
					break;
				case "missing-tag":
					setup.Repository.Tags.Clear();
					break;
				case "moved-tag":
					setup.Repository.Tags[setup.Plan.Tag.Name] = PlanSamples.Sha('f');
					break;
				case "missing-release":
					setup.GitHub.Release = null;
					break;
				case "draft-release":
					setup.GitHub.Release = setup.GitHub.Release! with { IsDraft = true };
					break;
				case "wrong-release-title":
					setup.GitHub.Release = setup.GitHub.Release! with { Title = "wrong" };
					break;
			}

			await Assert.ThrowsAsync<ConflictException>(() =>
				setup.Service.ApplyAsync(
					setup.Plan,
					setup.Plan.PlanId,
					TestContext.Current.CancellationToken));

			Assert.Equal(0, setup.GitHub.MilestoneAccessCount);
			Assert.Empty(setup.GitHub.Dispatches);
		}

		[Fact]
		public async Task Reconciles_first_parent_pull_request_and_its_closing_issue()
		{
			var setup = Setup();
			setup.GitHub.Milestones.Add(new(1, setup.Plan.Release.Identity, true, null, null));
			setup.Repository.Subjects.Add("Fix the thing (#42)");
			setup.Repository.Subjects.Add("Finish the thing (#43)");
			setup.GitHub.PullRequestMilestones[42] = "4.151.0";
			setup.GitHub.PullRequestMilestones[43] = null;
			setup.GitHub.ClosingIssues[42] = [7];
			setup.GitHub.ClosingIssues[43] = [7];
			setup.GitHub.PullRequestBodies[43] = "Resolves: mono/SkiaSharp#8";
			setup.GitHub.IssueMilestones[7] = null;
			setup.GitHub.IssueMilestones[8] = null;

			var result = await setup.Service.ApplyAsync(
				setup.Plan,
				setup.Plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Equal(4, result.ReconcileResults.Count);
			Assert.Equal(1, setup.GitHub.PullRequestMilestoneNumbers[42]);
			Assert.Equal(1, setup.GitHub.PullRequestMilestoneNumbers[43]);
			Assert.Equal(1, setup.GitHub.IssueMilestoneNumbers[7]);
			Assert.Equal(1, setup.GitHub.IssueMilestoneNumbers[8]);
			Assert.Equal(2, result.ReconcileResults.Count(value => value.Kind == FinishReconcileKind.Issue));
			Assert.Equal(4, setup.GitHub.AssignmentWrites);
		}

		[Fact]
		public async Task Sibling_previous_boundary_is_projected_onto_integration_history()
		{
			var setup = Setup(previousTag: "v4.151.0");
			setup.GitHub.Milestones.Add(new(1, setup.Plan.Release.Identity, true, null, null));
			setup.Repository.Tags["v4.151.0"] = PlanSamples.Sha('e');
			setup.Repository.Commits.Add(PlanSamples.Sha('e'));
			setup.Repository.Subjects.Add("Release branch only (#42)");
			setup.GitHub.PullRequestMilestones[42] = null;

			var result = await setup.Service.ApplyAsync(
				setup.Plan,
				setup.Plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Contains(result.ReconcileResults, item => item.Number == 42);
			Assert.Equal(1, setup.GitHub.PullRequestMilestoneNumbers[42]);
		}

		[Fact]
		public async Task Unshipped_exact_branch_segment_rolls_into_current_release()
		{
			var setup = Setup(previousTag: "v4.151.1");
			setup.GitHub.Milestones.Add(new(1, setup.Plan.Release.Identity, true, null, null));
			setup.Repository.Tags["v4.151.1"] = PlanSamples.Sha('e');
			setup.Repository.Commits.Add(PlanSamples.Sha('e'));
			const string branch = "release/4.152.0-preview.1";
			var endpoint = PlanSamples.Sha('c');
			setup.Repository.ReleaseBranchNames.Add(branch);
			setup.Repository.RemoteBranches[branch] = endpoint;
			setup.Repository.SubjectsBySource[endpoint] = ["Unshipped branch change (#77)"];
			setup.GitHub.PullRequestMilestones[77] = null;

			var result = await setup.Service.ApplyAsync(
				setup.Plan,
				setup.Plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Contains(result.ReconcileResults, item => item.Number == 77);
			Assert.Equal(1, setup.GitHub.PullRequestMilestoneNumbers[77]);
		}

		[Fact]
		public async Task Open_items_without_eligible_rollover_target_block_but_do_not_suppress_dispatch()
		{
			var setup = Setup();
			setup.GitHub.Milestones.Add(new(1, setup.Plan.Release.Identity, true, null, null));
			setup.GitHub.OpenItems[1] = [new(99, "still open", new Uri("https://example.test/99"), false)];

			var result = await setup.Service.ApplyAsync(
				setup.Plan,
				setup.Plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Equal(FinishCloseoutNextAction.Blocked, result.NextAction);
			Assert.Equal(FinishCloseoutStatus.Blocked, Assert.Single(result.ClosureResults).Status);
			Assert.Equal(2, setup.GitHub.Dispatches.Count);
			Assert.Equal(0, setup.GitHub.CloseWrites);
		}

		[Fact]
		public async Task Rollover_moves_open_items_then_closes_shipped_milestone()
		{
			var setup = Setup();
			setup.GitHub.Milestones.AddRange(
			[
				new(1, setup.Plan.Release.Identity, true, null, null),
				new(2, "4.153.0-preview.1", true, null, null),
			]);
			setup.GitHub.OpenItems[1] = [new(99, "still open", new Uri("https://example.test/99"), false)];

			var result = await setup.Service.ApplyAsync(
				setup.Plan,
				setup.Plan.PlanId,
				TestContext.Current.CancellationToken);

			var closure = Assert.Single(result.ClosureResults);
			Assert.Equal(FinishCloseoutStatus.Done, closure.Status);
			Assert.Equal("4.153.0-preview.1", closure.MovedTo);
			Assert.Equal(2, setup.GitHub.ItemMilestoneNumbers[99]);
			Assert.False(setup.GitHub.Milestones.Single(value => value.Number == 1).IsOpen);
		}

		[Fact]
		public async Task Newly_created_schedule_milestone_is_reread_as_rollover_target()
		{
			var setup = Setup();
			setup.GitHub.Milestones.Add(new(1, setup.Plan.Release.Identity, true, null, null));
			setup.GitHub.OpenItems[1] = [new(99, "still open", new Uri("https://example.test/99"), false)];
			setup.Chromium.Schedules[153] =
				SkiaSharp.ReleaseTool.Tests.Milestones.ChromiumScheduleTests.Schedule();

			var result = await setup.Service.ApplyAsync(
				setup.Plan,
				setup.Plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Equal(4, setup.GitHub.ScheduleWrites);
			Assert.Equal(4, result.ScheduleResults.Count);
			Assert.Equal("4.153.0-preview.1", Assert.Single(result.ClosureResults).MovedTo);
			Assert.Contains(setup.GitHub.Milestones, value => value.Title == "4.153.0-preview.1");
		}

		[Fact]
		public async Task Schedule_apply_updates_only_mismatches_and_preserves_noops()
		{
			var setup = Setup();
			var schedule = SkiaSharp.ReleaseTool.Tests.Milestones.ChromiumScheduleTests.Schedule();
			setup.Chromium.Schedules[152] = schedule;
			var desired = ChromiumSchedulePlanner.Desired(schedule, 152, 4);
			setup.GitHub.Milestones.AddRange(
				desired.Select((value, index) => new GitHubMilestone(
					index + 1,
					value.Title,
					true,
					value.DueOn,
					index == 0 ? "stale" : value.Description)));

			var result = await setup.Service.ApplyAsync(
				setup.Plan,
				setup.Plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Equal(1, setup.GitHub.ScheduleWrites);
			Assert.Equal(FinishScheduleAction.Update, result.ScheduleResults[0].Action);
			Assert.All(
				result.ScheduleResults.Skip(1),
				value => Assert.Equal(FinishScheduleAction.None, value.Action));
		}

		[Fact]
		public async Task Idempotent_rerun_does_not_repeat_milestone_writes_but_redispatches()
		{
			var setup = Setup();
			setup.GitHub.Milestones.Add(new(1, setup.Plan.Release.Identity, true, null, null));

			_ = await setup.Service.ApplyAsync(
				setup.Plan,
				setup.Plan.PlanId,
				TestContext.Current.CancellationToken);
			var milestoneWrites = setup.GitHub.CloseWrites + setup.GitHub.AssignmentWrites + setup.GitHub.ScheduleWrites;
			_ = await setup.Service.ApplyAsync(
				setup.Plan,
				setup.Plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Equal(milestoneWrites, setup.GitHub.CloseWrites + setup.GitHub.AssignmentWrites + setup.GitHub.ScheduleWrites);
			Assert.Equal(4, setup.GitHub.Dispatches.Count);
		}

		[Theory]
		[InlineData(true, 2)]
		[InlineData(false, 1)]
		public async Task Dispatch_is_always_release_notes_and_stable_only_issue_refresh(
			bool stable,
			int expectedDispatches)
		{
			var setup = Setup(stable: stable);

			var result = await setup.Service.ApplyAsync(
				setup.Plan,
				setup.Plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Equal(expectedDispatches, result.Dispatches.Count);
			var notes = result.Dispatches[0];
			Assert.Equal("update-release-notes.lock.yml", notes.Workflow);
			Assert.Equal("main", notes.Ref);
			Assert.Equal("main", notes.Inputs["source_branch"]);
			Assert.Equal(setup.Plan.Receipt.Base, notes.Inputs["min_version"]);
			Assert.Equal("false", notes.Inputs["force"]);
			if (stable)
				Assert.Equal("auto-update-issue-template-versions.yml", result.Dispatches[1].Workflow);
		}

		[Fact]
		public async Task Dispatch_failure_is_recoverable_by_rerun()
		{
			var setup = Setup();
			setup.GitHub.Milestones.Add(new(1, setup.Plan.Release.Identity, true, null, null));
			setup.GitHub.FailDispatchAttempt = 2;

			await Assert.ThrowsAsync<GitHubException>(() =>
				setup.Service.ApplyAsync(
					setup.Plan,
					setup.Plan.PlanId,
					TestContext.Current.CancellationToken));
			Assert.Equal(1, setup.GitHub.CloseWrites);
			Assert.Single(setup.GitHub.Dispatches);

			setup.GitHub.FailDispatchAttempt = null;
			var result = await setup.Service.ApplyAsync(
				setup.Plan,
				setup.Plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Equal(1, setup.GitHub.CloseWrites);
			Assert.Equal(2, result.Dispatches.Count);
			Assert.Equal(3, setup.GitHub.Dispatches.Count);
		}

		[Fact]
		public async Task Chromium_failure_is_warning_and_does_not_block_reconciliation_closure_or_dispatch()
		{
			var setup = Setup();
			setup.GitHub.Milestones.Add(new(1, setup.Plan.Release.Identity, true, null, null));
			setup.Repository.Subjects.Add("Merged (#8)");
			setup.GitHub.PullRequestMilestones[8] = null;

			var result = await setup.Service.ApplyAsync(
				setup.Plan,
				setup.Plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Equal(3, result.Warnings.Count(value => value.Contains("Chromium", StringComparison.Ordinal)));
			Assert.Single(result.ReconcileResults);
			Assert.Single(result.ClosureResults);
			Assert.Equal(2, result.Dispatches.Count);
		}

		[Fact]
		public async Task Expected_parent_plan_id_is_revalidated()
		{
			var setup = Setup();

			await Assert.ThrowsAsync<ValidationException>(() =>
				setup.Service.ApplyAsync(
					setup.Plan,
					Guid.NewGuid(),
					TestContext.Current.CancellationToken));
			Assert.Equal(0, setup.GitHub.ReleaseReads);
		}

		private static CloseoutSetup Setup(
			bool stable = true,
			string? previousTag = null)
		{
			var plan = CreatePlan(stable, previousTag);
			var repository = new FakeCloseoutRepository(plan);
			var github = new FakeCloseoutGitHubClient
			{
				Release = new(
					42,
					plan.Tag.Name,
					plan.Release.Title,
					IsDraft: false,
					IsPrerelease: !plan.Release.Stable,
					plan.Receipt.SourceCommit,
					"",
					new Uri($"https://github.com/mono/SkiaSharp/releases/tag/{plan.Tag.Name}")),
			};
			var chromium = new FakeChromiumScheduleClient();
			var service = new FinishCloseoutService(
				repository,
				github,
				chromium,
				new FixedTimeProvider(new DateTimeOffset(2026, 8, 29, 12, 0, 0, TimeSpan.Zero)));
			return new(plan, repository, github, chromium, service);
		}

		internal static FinishPlan CreatePlan(bool stable, string? previousTag)
		{
			var version = stable ? "4.152.0" : "4.152.0-preview.1.12345.1";
			var requested = PublicReleaseVersion.Parse(version);
			var identity = requested.Identity;
			var source = PlanSamples.Sha('a');
			var release = new FinishReleaseInfo(
				identity.Raw,
				version,
				identity.ReleaseBranch,
				identity.Raw,
				identity.Numeric,
				identity.Label,
				identity.ReleaseType,
				identity.Stable,
				identity.Title,
				identity.Tag);
			var plan = new FinishPlan(
				1,
				ReleaseOperation.Finish,
				FinishTestFixture.PlanId,
				new DateTimeOffset(2026, 8, 29, 0, 0, 0, TimeSpan.Zero),
				source,
				FinishNextAction.Closeout,
				new(version),
				new(
					version,
					"4.152.0",
					identity.Label,
					requested.BuildRevision,
					source,
					identity.ReleaseBranch,
					"14.2.1.200",
					[
						new("SkiaSharp", version, source, identity.ReleaseBranch),
						new("SkiaSharp.HarfBuzz", version, source, identity.ReleaseBranch),
						new("HarfBuzzSharp", "14.2.1.200", source, identity.ReleaseBranch),
					]),
				release,
				new(identity.Tag, source, source, FinishState.Done),
				previousTag,
				new(
					true,
					true,
					FinishState.Done,
					ManagedMarkerState.Complete,
					source,
					new Uri($"https://github.com/mono/SkiaSharp/releases/tag/{identity.Tag}"),
					ManagedReleaseMarkers.BuildInitialBody("notes")),
				[
					new(FinishOperationId.CreateTag, FinishOperationKind.GitTag, PlanOperationStatus.Done, null),
					new(FinishOperationId.CreateDraft, FinishOperationKind.GitHubRelease, PlanOperationStatus.Skipped, null),
					new(FinishOperationId.PublishRelease, FinishOperationKind.GitHubRelease, PlanOperationStatus.Done, null),
					new(FinishOperationId.Closeout, FinishOperationKind.ReleaseCloseout, PlanOperationStatus.Pending, null),
				],
				[]);
			FinishPlanValidator.Validate(plan);
			return plan;
		}

		private sealed record CloseoutSetup(
			FinishPlan Plan,
			FakeCloseoutRepository Repository,
			FakeCloseoutGitHubClient GitHub,
			FakeChromiumScheduleClient Chromium,
			FinishCloseoutService Service);
	}

	internal sealed class FakeCloseoutRepository : IReleaseRepository
	{
		public FakeCloseoutRepository(FinishPlan plan, string root = ".")
		{
			Root = root;
			Commits.Add(plan.Receipt.SourceCommit);
			Commits.Add(plan.ToolingSha);
			RemoteBranches[plan.Receipt.SourceBranch] = plan.Receipt.SourceCommit;
			Tags[plan.Tag.Name] = plan.Receipt.SourceCommit;
		}

		public string Root { get; }
		public HashSet<string> Commits { get; } = new(StringComparer.Ordinal);
		public Dictionary<string, string> RemoteBranches { get; } = new(StringComparer.Ordinal);
		public Dictionary<string, string> Tags { get; } = new(StringComparer.Ordinal);
		public List<string> Subjects { get; } = [];
		public List<string> ReleaseBranchNames { get; } = [];
		public Dictionary<string, IReadOnlyList<string>> SubjectsBySource { get; } = [];
		public bool Ancestor { get; set; } = true;
		public HashSet<(string Ancestor, string Descendant)> RejectedAncestry { get; } = [];
		public string VersionsText { get; set; } =
			"""
			# nuget versions
			# SkiaSharp
			SkiaSharp nuget 4.152.0
			# HarfBuzzSharp
			HarfBuzzSharp nuget 14.2.1.200
			SkiaSharp file 4.152.0.0
			HarfBuzzSharp file 14.2.1.200
			libSkiaSharp milestone 152
			""";

		public Task FetchAsync(string remote = "origin", CancellationToken cancellationToken = default) => Task.CompletedTask;
		public Task<bool> RefExistsAsync(string reference, CancellationToken cancellationToken = default) => Task.FromResult(true);
		public Task<string> ResolveAsync(string reference, CancellationToken cancellationToken = default) => Task.FromResult(PlanSamples.Sha('a'));
		public Task<bool> CommitExistsAsync(string commit, CancellationToken cancellationToken = default) => Task.FromResult(Commits.Contains(commit));
		public Task<string> ReadRefFileAsync(string reference, string path, CancellationToken cancellationToken = default)
		{
			Assert.Equal("refs/remotes/origin/main", reference);
			Assert.Equal("scripts/VERSIONS.txt", path);
			return Task.FromResult(VersionsText);
		}
		public Task<string> ReadGitlinkAsync(string reference, string submodulePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
		public Task<string?> RemoteShaAsync(string branch, string remote = "origin", CancellationToken cancellationToken = default) => Task.FromResult(RemoteBranches.GetValueOrDefault(branch));
		public Task<IReadOnlyDictionary<string, string>> RemoteTagsAsync(string remote = "origin", string pattern = "refs/tags/*", CancellationToken cancellationToken = default)
		{
			if (pattern == "refs/tags/*")
				return Task.FromResult<IReadOnlyDictionary<string, string>>(new Dictionary<string, string>(Tags));
			var name = pattern["refs/tags/".Length..];
			var values = Tags.TryGetValue(name, out var sha)
				? new Dictionary<string, string> { [name] = sha }
				: new Dictionary<string, string>();
			return Task.FromResult<IReadOnlyDictionary<string, string>>(values);
		}
		public Task<IReadOnlyList<string>> ReleaseBranchesAsync(string remote = "origin", CancellationToken cancellationToken = default) =>
			Task.FromResult<IReadOnlyList<string>>([.. ReleaseBranchNames]);
		public Task<bool> IsAncestorAsync(string ancestor, string descendant, CancellationToken cancellationToken = default) =>
			Task.FromResult(Ancestor && !RejectedAncestry.Contains((ancestor, descendant)));
		public Task<string> MergeBaseAsync(string left, string right, CancellationToken cancellationToken = default) =>
			Task.FromResult(left);
		public Task<IReadOnlyList<string>> CommitSubjectsFirstParentAsync(string? exclusiveLowerBound, string sourceCommit, CancellationToken cancellationToken = default) =>
			Task.FromResult(
				SubjectsBySource.TryGetValue(sourceCommit, out var values)
					? values
					: (IReadOnlyList<string>)[.. Subjects]);
		public Task RequireCleanAsync(IReadOnlyList<string>? allowedUntrackedPaths = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
		public Task<string> CurrentBranchAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
		public Task UpdateLocalBranchAsync(string branch, string sha, CancellationToken cancellationToken = default) => throw new NotSupportedException();
		public Task SwitchAsync(string branch, CancellationToken cancellationToken = default) => throw new NotSupportedException();
		public Task SwitchCreateAsync(string branch, string startPoint, CancellationToken cancellationToken = default) => throw new NotSupportedException();
		public Task<string> CommitAsync(string message, IReadOnlyList<string>? paths = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
		public Task PushBranchAsync(string branch, string remote = "origin", bool setUpstream = true, CancellationToken cancellationToken = default) => throw new NotSupportedException();
		public Task PushTagAsync(string tag, string sha, string remote = "origin", CancellationToken cancellationToken = default) => throw new NotSupportedException();
		public Task<string> ReadWorktreeFileAsync(string path, CancellationToken cancellationToken = default) => throw new NotSupportedException();
		public Task WriteWorktreeFileAsync(string path, string content, CancellationToken cancellationToken = default) => throw new NotSupportedException();
	}

	internal sealed class FakeChromiumScheduleClient : IChromiumScheduleClient
	{
		public Dictionary<int, ChromiumMilestoneSchedule> Schedules { get; } = [];

		public Task<ChromiumMilestoneSchedule> FetchAsync(int milestone, CancellationToken cancellationToken = default) =>
			Schedules.TryGetValue(milestone, out var schedule)
				? Task.FromResult(schedule)
				: Task.FromException<ChromiumMilestoneSchedule>(
					new MilestoneException($"simulated missing schedule for m{milestone}"));
	}

	internal sealed class FakeCloseoutGitHubClient : ICloseoutGitHubClient
	{
		private int nextMilestone = 100;
		public FinishGitHubRelease? Release { get; set; }
		public List<GitHubMilestone> Milestones { get; } = [];
		public Dictionary<int, List<GitHubMilestoneItem>> OpenItems { get; } = [];
		public Dictionary<int, string?> PullRequestMilestones { get; } = [];
		public Dictionary<int, string?> PullRequestBodies { get; } = [];
		public Dictionary<int, int> PullRequestMilestoneNumbers { get; } = [];
		public Dictionary<int, IReadOnlyList<int>> ClosingIssues { get; } = [];
		public Dictionary<int, string?> IssueMilestones { get; } = [];
		public Dictionary<int, int> IssueMilestoneNumbers { get; } = [];
		public Dictionary<int, int> ItemMilestoneNumbers { get; } = [];
		public List<FinishWorkflowDispatch> Dispatches { get; } = [];
		public int ReleaseReads { get; private set; }
		public int MilestoneAccessCount { get; private set; }
		public int ScheduleWrites { get; private set; }
		public int AssignmentWrites { get; private set; }
		public int CloseWrites { get; private set; }
		public int DispatchAttempts { get; private set; }
		public int? FailDispatchAttempt { get; set; }

		public Task<FinishGitHubRelease?> GetReleaseAsync(string tag, CancellationToken cancellationToken = default)
		{
			ReleaseReads++;
			return Task.FromResult(Release);
		}

		public Task<IReadOnlyList<GitHubMilestone>> GetMilestonesAsync(CancellationToken cancellationToken = default)
		{
			MilestoneAccessCount++;
			return Task.FromResult<IReadOnlyList<GitHubMilestone>>([.. Milestones]);
		}

		public Task<GitHubMilestone> CreateMilestoneAsync(string title, DateTimeOffset dueOn, string description, CancellationToken cancellationToken = default)
		{
			MilestoneAccessCount++;
			ScheduleWrites++;
			var milestone = new GitHubMilestone(nextMilestone++, title, true, dueOn, description);
			Milestones.Add(milestone);
			return Task.FromResult(milestone);
		}

		public Task UpdateMilestoneAsync(int number, DateTimeOffset dueOn, string description, CancellationToken cancellationToken = default)
		{
			MilestoneAccessCount++;
			ScheduleWrites++;
			var index = Milestones.FindIndex(value => value.Number == number);
			Milestones[index] = Milestones[index] with { DueOn = dueOn, Description = description };
			return Task.CompletedTask;
		}

		public Task<IReadOnlyList<GitHubMilestoneItem>> GetOpenMilestoneItemsAsync(int milestoneNumber, CancellationToken cancellationToken = default)
		{
			MilestoneAccessCount++;
			return Task.FromResult<IReadOnlyList<GitHubMilestoneItem>>(
				OpenItems.TryGetValue(milestoneNumber, out var items) ? [.. items] : []);
		}

		public Task<string?> GetPullRequestMilestoneAsync(int pullRequestNumber, CancellationToken cancellationToken = default)
		{
			MilestoneAccessCount++;
			return Task.FromResult(PullRequestMilestones.GetValueOrDefault(pullRequestNumber));
		}

		public Task<string?> GetPullRequestBodyAsync(
			int pullRequestNumber,
			CancellationToken cancellationToken = default)
		{
			MilestoneAccessCount++;
			return Task.FromResult(PullRequestBodies.GetValueOrDefault(pullRequestNumber));
		}

		public Task<IReadOnlyList<int>> GetClosingIssuesAsync(int pullRequestNumber, CancellationToken cancellationToken = default)
		{
			MilestoneAccessCount++;
			return Task.FromResult(ClosingIssues.GetValueOrDefault(pullRequestNumber) ?? []);
		}

		public Task<string?> GetIssueMilestoneAsync(int issueNumber, CancellationToken cancellationToken = default)
		{
			MilestoneAccessCount++;
			return Task.FromResult(IssueMilestones.GetValueOrDefault(issueNumber));
		}

		public Task UpdateItemMilestoneAsync(int itemNumber, int milestoneNumber, CancellationToken cancellationToken = default)
		{
			MilestoneAccessCount++;
			AssignmentWrites++;
			ItemMilestoneNumbers[itemNumber] = milestoneNumber;
			if (PullRequestMilestones.ContainsKey(itemNumber))
			{
				PullRequestMilestones[itemNumber] = Milestones.Single(value => value.Number == milestoneNumber).Title;
				PullRequestMilestoneNumbers[itemNumber] = milestoneNumber;
			}
			if (IssueMilestones.ContainsKey(itemNumber))
			{
				IssueMilestones[itemNumber] = Milestones.Single(value => value.Number == milestoneNumber).Title;
				IssueMilestoneNumbers[itemNumber] = milestoneNumber;
			}
			foreach (var items in OpenItems.Values)
				items.RemoveAll(value => value.Number == itemNumber);
			return Task.CompletedTask;
		}

		public Task CloseMilestoneAsync(int milestoneNumber, CancellationToken cancellationToken = default)
		{
			MilestoneAccessCount++;
			CloseWrites++;
			var index = Milestones.FindIndex(value => value.Number == milestoneNumber);
			Milestones[index] = Milestones[index] with { IsOpen = false };
			return Task.CompletedTask;
		}

		public Task DispatchWorkflowAsync(string workflow, string reference, IReadOnlyDictionary<string, string> inputs, CancellationToken cancellationToken = default)
		{
			DispatchAttempts++;
			if (FailDispatchAttempt == DispatchAttempts)
				throw new GitHubException("simulated dispatch failure");
			Dispatches.Add(new(workflow, reference, inputs, FinishDispatchStatus.Dispatched));
			return Task.CompletedTask;
		}

	}
}
