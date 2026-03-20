namespace SkiaSharp.Triage.Models;

public record SkiaUpdatesIndex(
    DateTime GeneratedAt,
    int TotalCount,
    SkiaUpdatesSummary Summary,
    List<SkiaReviewIndexEntry> Reviews
);

public record SkiaUpdatesSummary(
    int Total,
    int HighRisk,
    int MediumRisk,
    int LowRisk,
    int GeneratedFilesFailed,
    int UpstreamChanges,
    int InteropChanges,
    int DepsChanges
);

public record SkiaReviewIndexEntry(
    int PrNumber,
    string UpstreamBranch,
    string OldUpstreamBranch,
    DateTime AnalyzedAt,
    SkiaRiskAssessment RiskAssessment,
    GeneratedFilesStatus GeneratedFilesStatus,
    SkiaReviewStatus UpstreamIntegrityStatus,
    SkiaReviewStatus InteropIntegrityStatus,
    SkiaReviewStatus DepsAuditStatus,
    int UpstreamAddedCount,
    int UpstreamRemovedCount,
    int UpstreamChangedCount,
    int InteropChangedCount,
    int DepsAddedCount,
    int DepsChangedCount,
    string? PrTitle = null,
    string? PrState = null,
    string? PrUrl = null,
    int? SkiasharpPrNumber = null
);
