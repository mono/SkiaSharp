using ReleaseChecklist.Core;
using SkiaSharp.ReleaseTool.Contracts;

namespace SkiaSharp.ReleaseChecklist;

internal interface IFinishRuntime
{
	FinishPlan? Plan { get; }

	string? PendingDetail { get; }

	FinishCloseoutResult? CloseoutResult { get; }

	ValueTask<CheckResult> CheckToolingAsync(CancellationToken cancellationToken);

	ValueTask<CheckResult> CheckTagAsync(CancellationToken cancellationToken);

	ValueTask CreateTagAsync(CancellationToken cancellationToken);

	ValueTask<CheckResult> CheckDraftAsync(CancellationToken cancellationToken);

	ValueTask CreateDraftAsync(CancellationToken cancellationToken);

	ValueTask<CheckResult> CheckPublishedAsync(CancellationToken cancellationToken);

	ValueTask PublishAsync(CancellationToken cancellationToken);

	ValueTask<CheckResult> CheckShippedAsync(CancellationToken cancellationToken);

	ValueTask CloseoutAsync(CancellationToken cancellationToken);

	CheckResult CheckScheduleResult();

	CheckResult CheckReconciliationResult();

	CheckResult CheckClosureResult();

	CheckResult CheckDispatchResult();

	ValueTask<CheckResult> CheckReviewedSummaryAsync(CancellationToken cancellationToken);
}
