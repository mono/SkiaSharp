using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Milestones;
using SkiaSharp.ReleaseTool.Model;
using SkiaSharp.ReleaseTool.Planning;

namespace SkiaSharp.ReleaseTool.Finishing
{
	internal sealed class FinishCloseoutService(
		IReleaseRepository repository,
		ICloseoutGitHubClient github,
		IChromiumScheduleClient chromium,
		TimeProvider timeProvider)
	{
		private const int ScheduleMilestoneCount = 3;
		private const string VersionsPath = "scripts/VERSIONS.txt";
		private const string ReleaseNotesWorkflow = "update-release-notes.lock.yml";
		private const string IssueTemplateWorkflow = "auto-update-issue-template-versions.yml";

		public async Task<FinishCloseoutPlan> PlanAsync(
			FinishPlan parent,
			Guid expectedPlanId,
			CancellationToken cancellationToken = default)
		{
			RequireParent(parent, expectedPlanId);
			await repository.FetchAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
			var tags = await RequireShippedAsync(parent, cancellationToken).ConfigureAwait(false);
			var warnings = new List<string>();
			var milestones = await github.GetMilestonesAsync(cancellationToken).ConfigureAwait(false);
			var milestoneMap = MilestonePlanner.Index(milestones);
			var schedule = await PlanScheduleAsync(
				milestoneMap,
				warnings,
				cancellationToken).ConfigureAwait(false);
			var reconciliation = await PlanReconciliationAsync(
				parent,
				milestones,
				tags,
				warnings,
				cancellationToken).ConfigureAwait(false);
			var creatable = schedule
				.Where(operation =>
					operation.Action == FinishScheduleAction.Create &&
					operation.Status == FinishCloseoutStatus.Pending)
				.Select(operation => operation.Title)
				.ToHashSet(StringComparer.Ordinal);
			var closure = await MilestonePlanner.PlanClosureAsync(
				milestones,
				tags,
				github,
				creatable,
				warnings,
				cancellationToken).ConfigureAwait(false);
			var nextAction = closure.Any(operation => operation.Status == FinishCloseoutStatus.Blocked)
				? FinishCloseoutNextAction.Blocked
				: schedule.Any(operation => operation.Status == FinishCloseoutStatus.Pending) ||
					reconciliation.Count > 0 ||
					closure.Any(operation => operation.Status == FinishCloseoutStatus.Pending)
					? FinishCloseoutNextAction.Closeout
					: FinishCloseoutNextAction.Done;
			var result = new FinishCloseoutPlan(
				SchemaVersion: 1,
				Operation: FinishCloseoutOperation.Plan,
				PlanId: parent.PlanId,
				GeneratedAt: timeProvider.GetUtcNow(),
				ToolingSha: parent.ToolingSha,
				NextAction: nextAction,
				Release: parent.Release,
				SourceCommit: parent.Receipt.SourceCommit,
				SourceBranch: parent.Receipt.SourceBranch,
				Tag: parent.Tag.Name,
				ScheduleOperations: schedule,
				ReconcileOperations: reconciliation,
				ClosureOperations: closure,
				Dispatches: BuildDispatches(parent, FinishDispatchStatus.Pending),
				Warnings: warnings);
			FinishCloseoutPlanValidator.Validate(result);
			return result;
		}

		public async Task<FinishCloseoutResult> ApplyAsync(
			FinishPlan parent,
			Guid expectedPlanId,
			CancellationToken cancellationToken = default)
		{
			RequireParent(parent, expectedPlanId);
			await repository.FetchAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
			var tags = await RequireShippedAsync(parent, cancellationToken).ConfigureAwait(false);
			var warnings = new List<string>();
			var milestones = await github.GetMilestonesAsync(cancellationToken).ConfigureAwait(false);
			var schedule = await PlanScheduleAsync(
				MilestonePlanner.Index(milestones),
				warnings,
				cancellationToken).ConfigureAwait(false);
			var scheduleResults = await ApplyScheduleAsync(schedule, cancellationToken).ConfigureAwait(false);

			milestones = await github.GetMilestonesAsync(cancellationToken).ConfigureAwait(false);
			_ = MilestonePlanner.Index(milestones);
			var reconciliation = await PlanReconciliationAsync(
				parent,
				milestones,
				tags,
				warnings,
				cancellationToken).ConfigureAwait(false);
			var reconcileResults = new List<FinishReconcileResult>();
			foreach (var operation in reconciliation)
			{
				await github.UpdateItemMilestoneAsync(
					operation.Number,
					operation.ToMilestoneNumber,
					cancellationToken).ConfigureAwait(false);
				reconcileResults.Add(new(
					operation.Kind,
					operation.Number,
					operation.ViaPullRequest,
					operation.FromMilestone,
					operation.ToMilestone,
					FinishCloseoutStatus.Done));
			}

			var closure = await MilestonePlanner.PlanClosureAsync(
				milestones,
				tags,
				github,
				creatableTitles: null,
				warnings,
				cancellationToken).ConfigureAwait(false);
			var closureResults = await ApplyClosureAsync(closure, cancellationToken).ConfigureAwait(false);

			var dispatched = new List<FinishWorkflowDispatch>();
			foreach (var dispatch in BuildDispatches(parent, FinishDispatchStatus.Pending))
			{
				await github.DispatchWorkflowAsync(
					dispatch.Workflow,
					dispatch.Ref,
					dispatch.Inputs,
					cancellationToken).ConfigureAwait(false);
				dispatched.Add(dispatch with { Status = FinishDispatchStatus.Dispatched });
			}

			var result = new FinishCloseoutResult(
				SchemaVersion: 1,
				Operation: FinishCloseoutOperation.Apply,
				PlanId: parent.PlanId,
				GeneratedAt: timeProvider.GetUtcNow(),
				ToolingSha: parent.ToolingSha,
				NextAction: closureResults.Any(value => value.Status == FinishCloseoutStatus.Blocked)
					? FinishCloseoutNextAction.Blocked
					: FinishCloseoutNextAction.Done,
				Release: parent.Release,
				SourceCommit: parent.Receipt.SourceCommit,
				SourceBranch: parent.Receipt.SourceBranch,
				Tag: parent.Tag.Name,
				ScheduleResults: scheduleResults,
				ReconcileResults: reconcileResults,
				ClosureResults: closureResults,
				Dispatches: dispatched,
				Warnings: warnings);
			FinishCloseoutResultValidator.Validate(result);
			return result;
		}

		private async Task<IReadOnlySet<string>> RequireShippedAsync(
			FinishPlan parent,
			CancellationToken cancellationToken)
		{
			var sourceCommit = parent.Receipt.SourceCommit;
			if (!await repository.CommitExistsAsync(sourceCommit, cancellationToken).ConfigureAwait(false))
				throw new ConflictException($"cannot close out {parent.Tag.Name}: source commit {sourceCommit} does not exist");
			if (!await repository.CommitExistsAsync(parent.ToolingSha, cancellationToken).ConfigureAwait(false))
				throw new ConflictException($"cannot close out {parent.Tag.Name}: tooling commit {parent.ToolingSha} does not exist");
			var branchSha = await repository.RemoteShaAsync(
				parent.Receipt.SourceBranch,
				cancellationToken: cancellationToken).ConfigureAwait(false);
			if (branchSha is null)
			{
				throw new ConflictException(
					$"cannot close out {parent.Tag.Name}: release branch {parent.Receipt.SourceBranch} does not exist on origin");
			}
			if (!await repository.IsAncestorAsync(sourceCommit, branchSha, cancellationToken).ConfigureAwait(false))
			{
				throw new ConflictException(
					$"cannot close out {parent.Tag.Name}: release branch {parent.Receipt.SourceBranch} does not contain source commit {sourceCommit}");
			}
			var remoteTags = await repository.RemoteTagsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
			if (!remoteTags.TryGetValue(parent.Tag.Name, out var tagSha))
				throw new ConflictException($"cannot close out {parent.Tag.Name}: it does not exist on the remote");
			if (tagSha != sourceCommit)
			{
				throw new ConflictException(
					$"cannot close out {parent.Tag.Name}: it points to {tagSha}, expected package source commit {sourceCommit}");
			}

			var release = await github.GetReleaseAsync(parent.Tag.Name, cancellationToken).ConfigureAwait(false)
				?? throw new ConflictException($"cannot close out {parent.Tag.Name}: no GitHub release exists for it");
			if (release.IsDraft)
				throw new ConflictException($"cannot close out {parent.Tag.Name}: its GitHub release is still an unpublished draft");
			if (release.TagName != parent.Tag.Name ||
				release.Title != parent.Release.Title ||
				release.IsPrerelease != !parent.Release.Stable)
			{
				throw new ConflictException($"cannot close out {parent.Tag.Name}: its published GitHub release metadata does not match");
			}
			if (release.TargetCommitish != sourceCommit &&
				release.TargetCommitish != "main" &&
				release.TargetCommitish != parent.Receipt.SourceBranch)
			{
				throw new ConflictException(
					$"cannot close out {parent.Tag.Name}: release target '{release.TargetCommitish}' is not the source commit or an accepted legacy branch");
			}
			return remoteTags.Keys.ToHashSet(StringComparer.Ordinal);
		}

		private async Task<IReadOnlyList<FinishScheduleOperation>> PlanScheduleAsync(
			IReadOnlyDictionary<string, GitHubMilestone> existing,
			ICollection<string> warnings,
			CancellationToken cancellationToken)
		{
			var versions = await repository.ReadRefFileAsync(
				"refs/remotes/origin/main",
				VersionsPath,
				cancellationToken).ConfigureAwait(false);
			var (major, currentMilestone) = VersionsTxt.ParseCurrentMajorAndMilestone(versions);
			var schedules = new List<(int, int, ChromiumMilestoneSchedule)>();
			for (var number = currentMilestone; number < currentMilestone + ScheduleMilestoneCount; number++)
			{
				try
				{
					var schedule = await chromium.FetchAsync(number, cancellationToken).ConfigureAwait(false);
					_ = ChromiumSchedulePlanner.Desired(schedule, number, major);
					schedules.Add((number, major, schedule));
				}
				catch (MilestoneException ex)
				{
					warnings.Add($"could not fetch Chromium schedule for m{number}: {ex.Message}");
				}
			}
			var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
			return ChromiumSchedulePlanner.Plan(schedules, existing, today);
		}

		private async Task<IReadOnlyList<FinishReconcileOperation>> PlanReconciliationAsync(
			FinishPlan parent,
			IReadOnlyList<GitHubMilestone> milestones,
			IReadOnlySet<string> tags,
			ICollection<string> warnings,
			CancellationToken cancellationToken)
		{
			var target = milestones.SingleOrDefault(milestone => milestone.Title == parent.Release.Identity);
			if (target is null)
			{
				warnings.Add(
					$"no milestone titled '{parent.Release.Identity}' exists; skipping PR/issue reconciliation for this release");
				return [];
			}

			string? lowerBound = null;
			if (parent.PreviousTag is not null)
			{
				var remoteTags = await repository.RemoteTagsAsync(
					pattern: $"refs/tags/{parent.PreviousTag}",
					cancellationToken: cancellationToken).ConfigureAwait(false);
				if (!remoteTags.TryGetValue(parent.PreviousTag, out lowerBound))
				{
					throw new ConflictException(
						$"cannot resolve previous tag '{parent.PreviousTag}' to a commit; the release boundary for PR/issue reconciliation is ambiguous");
				}
				if (!await repository.CommitExistsAsync(lowerBound, cancellationToken).ConfigureAwait(false) ||
					!await repository.IsAncestorAsync(
						lowerBound,
						parent.Receipt.SourceCommit,
						cancellationToken).ConfigureAwait(false))
				{
					throw new ConflictException(
						$"previous tag '{parent.PreviousTag}' ({lowerBound}) is not an ancestor of shipped commit {parent.Receipt.SourceCommit}; the release boundary for PR/issue reconciliation is ambiguous");
				}
			}
			var subjects = await repository.CommitSubjectsFirstParentAsync(
				lowerBound,
				parent.Receipt.SourceCommit,
				cancellationToken).ConfigureAwait(false);
			var pullRequests = MilestonePlanner.ExtractMergedPullRequests(subjects);
			return await MilestonePlanner.PlanReconciliationAsync(
				pullRequests,
				target,
				github,
				cancellationToken).ConfigureAwait(false);
		}

		private async Task<IReadOnlyList<FinishScheduleResult>> ApplyScheduleAsync(
			IReadOnlyList<FinishScheduleOperation> operations,
			CancellationToken cancellationToken)
		{
			var results = new List<FinishScheduleResult>();
			foreach (var operation in operations)
			{
				switch (operation.Action)
				{
					case FinishScheduleAction.Create:
						var created = await github.CreateMilestoneAsync(
							operation.Title,
							operation.DueOn,
							operation.Description,
							cancellationToken).ConfigureAwait(false);
						results.Add(new(operation.Title, created.Number, operation.Action, FinishCloseoutStatus.Done));
						break;
					case FinishScheduleAction.Update:
						await github.UpdateMilestoneAsync(
							operation.Number!.Value,
							operation.DueOn,
							operation.Description,
							cancellationToken).ConfigureAwait(false);
						results.Add(new(operation.Title, operation.Number, operation.Action, FinishCloseoutStatus.Done));
						break;
					default:
						results.Add(new(operation.Title, operation.Number, operation.Action, operation.Status));
						break;
				}
			}
			return results;
		}

		private async Task<IReadOnlyList<FinishClosureResult>> ApplyClosureAsync(
			IReadOnlyList<FinishClosureOperation> operations,
			CancellationToken cancellationToken)
		{
			var results = new List<FinishClosureResult>();
			foreach (var operation in operations)
			{
				if (operation.Status == FinishCloseoutStatus.Blocked)
				{
					results.Add(new(
						operation.Milestone,
						FinishCloseoutStatus.Blocked,
						null,
						operation.Detail));
					continue;
				}
				var items = await github.GetOpenMilestoneItemsAsync(
					operation.MilestoneNumber,
					cancellationToken).ConfigureAwait(false);
				if (items.Count > 0 && operation.MoveToNumber is null)
					throw new MilestoneException($"milestone '{operation.Milestone}' has open items but no numbered target");
				foreach (var item in items)
				{
					await github.UpdateItemMilestoneAsync(
						item.Number,
						operation.MoveToNumber!.Value,
						cancellationToken).ConfigureAwait(false);
				}
				var remaining = await github.GetOpenMilestoneItemsAsync(
					operation.MilestoneNumber,
					cancellationToken).ConfigureAwait(false);
				if (remaining.Count > 0)
				{
					throw new MilestoneException(
						$"milestone '{operation.Milestone}' still has open items after moving: [{string.Join(", ", remaining.Select(item => item.Number))}]");
				}
				await github.CloseMilestoneAsync(operation.MilestoneNumber, cancellationToken).ConfigureAwait(false);
				results.Add(new(
					operation.Milestone,
					FinishCloseoutStatus.Done,
					operation.MoveTo,
					null));
			}
			return results;
		}

		private static IReadOnlyList<FinishWorkflowDispatch> BuildDispatches(
			FinishPlan parent,
			FinishDispatchStatus status)
		{
			var dispatches = new List<FinishWorkflowDispatch>
			{
				new(
					ReleaseNotesWorkflow,
					"main",
					new Dictionary<string, string>(StringComparer.Ordinal)
					{
						["source_branch"] = "main",
						["min_version"] = parent.Receipt.Base,
						["max_version"] = parent.Receipt.Base,
						["force"] = "false",
					},
					status),
			};
			if (parent.Release.Stable)
			{
				dispatches.Add(new(
					IssueTemplateWorkflow,
					"main",
					new Dictionary<string, string>(StringComparer.Ordinal),
					status));
			}
			return dispatches;
		}

		private static void RequireParent(FinishPlan parent, Guid expectedPlanId)
		{
			FinishPlanValidator.Validate(parent);
			if (parent.PlanId != expectedPlanId)
			{
				throw new ValidationException(
					$"planId '{parent.PlanId}' does not match expected correlation id '{expectedPlanId}'");
			}
		}
	}
}
