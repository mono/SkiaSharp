using ReleaseChecklist.Core;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Model;

namespace SkiaSharp.ReleaseChecklist.Tests;

public class FinishDefinitionTests
{
	[Fact]
	public async Task PendingPublicReceiptStopsBeforeRepositoryWrites()
	{
		var runtime = new FakeFinishRuntime(
			plan: null,
			pendingDetail: "Packages are still indexing.");

		var report = await ChecklistRunner.RunAsync(FinishDefinition.Build(runtime));

		Assert.Equal(ChecklistStatus.NotDone, report.Root.Status);
		Assert.Equal(
			ChecklistStatus.NotDone,
			Find(report.Root, "exact-public-nuget-package").Status);
		Assert.Equal(0, runtime.TotalWrites);
	}

	[Fact]
	public async Task CreateDraftRunDoesNotPublishInSameInvocation()
	{
		var runtime = new FakeFinishRuntime(Plan(FinishNextAction.CreateDraft))
		{
			TagDone = false,
			DraftDone = false,
		};

		var report = await ChecklistRunner.RunAsync(
			FinishDefinition.Build(runtime),
			new ChecklistRunOptions { Mode = ChecklistRunMode.Apply });

		Assert.True(report.Successful);
		Assert.Equal(1, runtime.TagWrites);
		Assert.Equal(1, runtime.DraftWrites);
		Assert.Equal(0, runtime.PublishWrites);
		Assert.Equal(0, runtime.CloseoutWrites);
		Assert.Equal(ChecklistStatus.Skipped, Find(report.Root, "publish-release").Status);
	}

	[Fact]
	public async Task ReviewedDraftPublishesAndRunsCloseout()
	{
		var runtime = new FakeFinishRuntime(Plan(FinishNextAction.PlanPublication))
		{
			TagDone = true,
			DraftDone = true,
			Published = false,
			SummaryDone = false,
		};

		var report = await ChecklistRunner.RunAsync(
			FinishDefinition.Build(runtime),
			new ChecklistRunOptions { Mode = ChecklistRunMode.Apply });

		Assert.Equal(1, runtime.PublishWrites);
		Assert.Equal(1, runtime.CloseoutWrites);
		Assert.Equal(ChecklistStatus.Done, Find(report.Root, "public-schedule").Status);
		Assert.Equal(ChecklistStatus.Done, Find(report.Root, "shipped-reconciliation").Status);
		Assert.Equal(ChecklistStatus.Done, Find(report.Root, "milestone-closure").Status);
		Assert.Equal(ChecklistStatus.Done, Find(report.Root, "follow-up-dispatch").Status);
		Assert.Equal(ChecklistStatus.NotDone, Find(report.Root, "reviewed-summary").Status);
	}

	[Fact]
	public async Task PublishedReleaseWithReviewedSummaryConverges()
	{
		var runtime = new FakeFinishRuntime(Plan(FinishNextAction.Closeout))
		{
			TagDone = true,
			DraftDone = true,
			Published = true,
			SummaryDone = true,
		};

		var report = await ChecklistRunner.RunAsync(
			FinishDefinition.Build(runtime),
			new ChecklistRunOptions { Mode = ChecklistRunMode.Apply });

		Assert.True(report.Successful);
		Assert.Equal(0, runtime.PublishWrites);
		Assert.Equal(1, runtime.CloseoutWrites);
		Assert.Equal(ChecklistStatus.Done, Find(report.Root, "reviewed-summary").Status);
	}

	private static FinishPlan Plan(FinishNextAction nextAction) =>
		new(
			SchemaVersion: 1,
			Operation: ReleaseOperation.Finish,
			PlanId: Guid.NewGuid(),
			GeneratedAt: DateTimeOffset.UtcNow,
			ToolingSha: new string('a', 40),
			NextAction: nextAction,
			Input: new FinishInput("4.152.0"),
			Receipt: new FinishReceiptInfo(
				"4.152.0",
				"4.152.0",
				"stable",
				null,
				new string('b', 40),
				"release/4.152.0",
				"14.2.1.200",
				[]),
			Release: new FinishReleaseInfo(
				"4.152.0",
				"4.152.0",
				"release/4.152.0",
				"4.152.0",
				"4.152.0",
				"stable",
				ReleaseKind.Stable,
				true,
				"SkiaSharp 4.152.0",
				"v4.152.0"),
			Tag: new FinishTagInfo(
				"v4.152.0",
				new string('b', 40),
				null,
				FinishState.Pending),
			PreviousTag: "v4.151.0",
			Draft: new FinishDraftInfo(
				false,
				false,
				FinishState.Pending,
				ManagedMarkerState.None,
				null,
				null,
				null),
			Operations: [],
			Warnings: []);

	private static NodeResult Find(NodeResult root, string id)
	{
		if (root.Id == id)
			return root;
		foreach (var child in root.Children)
		{
			try
			{
				return Find(child, id);
			}
			catch (InvalidOperationException)
			{
			}
		}
		throw new InvalidOperationException($"Node '{id}' not found.");
	}

	private sealed class FakeFinishRuntime : IFinishRuntime
	{
		public FakeFinishRuntime(FinishPlan? plan, string? pendingDetail = null)
		{
			Plan = plan;
			PendingDetail = pendingDetail;
		}

		public FinishPlan? Plan { get; }
		public string? PendingDetail { get; }
		public FinishCloseoutResult? CloseoutResult { get; private set; }
		public bool TagDone { get; set; }
		public bool DraftDone { get; set; }
		public bool Published { get; set; }
		public bool SummaryDone { get; set; }
		public int TagWrites { get; private set; }
		public int DraftWrites { get; private set; }
		public int PublishWrites { get; private set; }
		public int CloseoutWrites { get; private set; }
		public int TotalWrites => TagWrites + DraftWrites + PublishWrites + CloseoutWrites;

		public ValueTask<CheckResult> CheckToolingAsync(CancellationToken cancellationToken) =>
			ValueTask.FromResult(CheckResult.Done("tooling done"));

		public ValueTask<CheckResult> CheckTagAsync(CancellationToken cancellationToken) =>
			ValueTask.FromResult(
				TagDone ? CheckResult.Done("tag done") : CheckResult.NotDone("tag pending"));

		public ValueTask CreateTagAsync(CancellationToken cancellationToken)
		{
			TagWrites++;
			TagDone = true;
			return ValueTask.CompletedTask;
		}

		public ValueTask<CheckResult> CheckDraftAsync(CancellationToken cancellationToken) =>
			ValueTask.FromResult(
				DraftDone ? CheckResult.Done("draft done") : CheckResult.NotDone("draft pending"));

		public ValueTask CreateDraftAsync(CancellationToken cancellationToken)
		{
			DraftWrites++;
			DraftDone = true;
			return ValueTask.CompletedTask;
		}

		public ValueTask<CheckResult> CheckPublishedAsync(CancellationToken cancellationToken) =>
			ValueTask.FromResult(
				Published ? CheckResult.Done("published") : CheckResult.NotDone("publish pending"));

		public ValueTask PublishAsync(CancellationToken cancellationToken)
		{
			PublishWrites++;
			Published = true;
			return ValueTask.CompletedTask;
		}

		public ValueTask<CheckResult> CheckShippedAsync(CancellationToken cancellationToken) =>
			ValueTask.FromResult(
				Published && TagDone
					? CheckResult.Done("shipped")
					: CheckResult.NotDone("not shipped"));

		public ValueTask CloseoutAsync(CancellationToken cancellationToken)
		{
			CloseoutWrites++;
			var plan = Plan!;
			CloseoutResult = new FinishCloseoutResult(
				1,
				FinishCloseoutOperation.Apply,
				plan.PlanId,
				DateTimeOffset.UtcNow,
				plan.ToolingSha,
				FinishCloseoutNextAction.Done,
				plan.Release,
				plan.Receipt.SourceCommit,
				plan.Receipt.SourceBranch,
				plan.Tag.Name,
				[],
				[],
				[],
				[],
				[]);
			return ValueTask.CompletedTask;
		}

		public CheckResult CheckScheduleResult() => CheckResult.Done("schedule done");
		public CheckResult CheckReconciliationResult() => CheckResult.Done("reconciliation done");
		public CheckResult CheckClosureResult() => CheckResult.Done("closure done");
		public CheckResult CheckDispatchResult() => CheckResult.Done("dispatch done");

		public ValueTask<CheckResult> CheckReviewedSummaryAsync(CancellationToken cancellationToken) =>
			ValueTask.FromResult(
				SummaryDone ? CheckResult.Done("summary done") : CheckResult.NotDone("summary pending"));
	}
}
