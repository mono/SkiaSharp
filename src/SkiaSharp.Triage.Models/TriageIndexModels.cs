namespace SkiaSharp.Triage.Models;

// ── Shared ───────────────────────────────────────────────────────

public record LabelCount(string Label, int Count);

// ── Index (lightweight list data) ────────────────────────────────

public record TriageIndex(
    DateTime GeneratedAt,
    int TotalCount,
    TriageIndexSummary Summary,
    List<LabelCount> ByType,
    List<LabelCount> ByArea,
    List<LabelCount> ByAction,
    List<LabelCount> BySeverity,
    List<TriageIndexEntry> Issues
);

public record TriageIndexSummary(
    int NeedsInvestigation,
    int Closeable,
    int QuickWins,
    int NeedsHumanReview,
    int Regressions,
    int WithRepro,
    int Reproduced,
    int NotReproduced
);

public record TriageIndexEntry(
    int Number,
    string Title,
    string? Type,
    string? Area,
    string? Severity,
    string? Action,
    double? Confidence,
    bool HumanReview,
    bool IsRegression,
    bool Closeable,
    string State,
    DateTime AnalyzedAt,
    bool HasTriage,
    bool HasRepro,
    string? ReproConclusion
);
