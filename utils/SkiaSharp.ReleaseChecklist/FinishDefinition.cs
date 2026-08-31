using ReleaseChecklist.Core;
using SkiaSharp.ReleaseTool.Contracts;

namespace SkiaSharp.ReleaseChecklist;

internal static class FinishDefinition
{
	public static ChecklistDefinition Build(IFinishRuntime runtime)
	{
		var canPublish = Condition.From(() =>
			runtime.Plan?.NextAction != FinishNextAction.CreateDraft);
		return new ChecklistBuilder().Sequence(
			"finish",
			runtime.Plan is null
				? "Finish SkiaSharp release"
				: $"Finish SkiaSharp {runtime.Plan.Release.Version}",
			finish =>
			{
				finish.Step(new StepOptions(
					"validate-tooling",
					"Validate the trusted tooling revision")
				{
					Check = Check.From(token => runtime.Plan is null
						? ValueTask.FromResult(CheckResult.Skipped(
							"Tooling validation waits until the public receipt is available."))
						: runtime.CheckToolingAsync(token)),
				});

				finish.Step(new StepOptions(
					"exact-public-nuget-package",
					"Find the exact public SkiaSharp package")
				{
					Check = Check.From(() => runtime.Plan is null
						? CheckResult.NotDone(
							runtime.PendingDetail ??
							"The exact public package is not yet available.")
						: CheckResult.Done(
							$"SkiaSharp {runtime.Plan.Receipt.SkiaSharpVersion} is publicly verifiable.",
							new ObservationBuilder()
								.Add("version", runtime.Plan.Receipt.SkiaSharpVersion)
								.Add("source-branch", runtime.Plan.Receipt.SourceBranch)
								.Add("source-commit", runtime.Plan.Receipt.SourceCommit)
								.Build())),
				});

				finish.Step(new StepOptions(
					"complete-public-receipt",
					"Verify the complete public package receipt")
				{
					Check = Check.From(() =>
					{
						var plan = runtime.Plan!;
						return CheckResult.Done(
							$"{plan.Receipt.Packages.Count} package receipts are verified.",
							new ObservationBuilder()
								.Add("packages", plan.Receipt.Packages.Count)
								.Add("harfbuzz-version", plan.Receipt.HarfBuzzSharpVersion)
								.Add("source-commit", plan.Receipt.SourceCommit)
								.Build());
					}),
				});

				finish.Step(new StepOptions(
					"effective-and-previous-tags",
					"Select and freeze effective and previous release tags")
				{
					Check = Check.From(() =>
					{
						var plan = runtime.Plan!;
						return CheckResult.Done(
							plan.PreviousTag is null
								? $"No previous tag precedes {plan.Tag.Name}."
								: $"{plan.PreviousTag} precedes {plan.Tag.Name}.",
							new ObservationBuilder()
								.Add("tag", plan.Tag.Name)
								.Add("previous", plan.PreviousTag ?? "")
								.Add("target", plan.Tag.TargetCommit)
								.Build());
					}),
				});

				finish.Step(new StepOptions(
					"release-tag",
					"Create or verify the immutable release tag")
				{
					Check = Check.From(runtime.CheckTagAsync),
					Action = ChecklistBuilder.Action(runtime.CreateTagAsync),
				});

				finish.Step(new StepOptions(
					"release-draft",
					"Create or verify the GitHub Release draft")
				{
					Check = Check.From(runtime.CheckDraftAsync),
					Action = ChecklistBuilder.Action(runtime.CreateDraftAsync),
				});

				finish.Step(new StepOptions(
					"publish-release",
					"Publish the reviewed GitHub Release draft")
				{
					Check = Check.From(runtime.CheckPublishedAsync),
					Action = ChecklistBuilder.Action(runtime.PublishAsync),
					When = canPublish,
				});

				finish.Parallel(
					"post-publication",
					"Converge closeout and report reviewed summary",
					postPublication =>
					{
						postPublication.Sequence(
							"closeout",
							"Converge public repository state after publication",
							closeout =>
							{
								closeout.Step(new StepOptions(
									"shipped-state-gate",
									"Verify exact shipped tag and release state")
								{
									Check = Check.From(runtime.CheckShippedAsync),
								});

								closeout.Step(new StepOptions(
									"apply-public-closeout",
									"Maintain milestones, reconcile work, close shipped milestones, and dispatch follow-up")
								{
									Action = ChecklistBuilder.Action(runtime.CloseoutAsync),
								});

								closeout.Parallel(
									"closeout-results",
									"Report public closeout results",
									results =>
									{
										results.Step(new StepOptions(
											"public-schedule",
											"Maintain the public milestone schedule")
										{
											Check = Check.From(runtime.CheckScheduleResult),
										});
										results.Step(new StepOptions(
											"shipped-reconciliation",
											"Build the shipped range and reconcile pull requests and issues")
										{
											Check = Check.From(runtime.CheckReconciliationResult),
										});
										results.Step(new StepOptions(
											"milestone-closure",
											"Roll open work and close shipped milestones")
										{
											Check = Check.From(runtime.CheckClosureResult),
										});
										results.Step(new StepOptions(
											"follow-up-dispatch",
											"Dispatch release notes and stable issue-template follow-up")
										{
											Check = Check.From(runtime.CheckDispatchResult),
										});
									},
									Condition.From(() => runtime.CloseoutResult is not null));
							});

						postPublication.Step(new StepOptions(
							"reviewed-summary",
							"Report reviewed GitHub Release summary convergence")
						{
							Check = Check.From(runtime.CheckReviewedSummaryAsync),
						});
					},
					canPublish);
			});
	}
}
