namespace SkiaSharp.Dashboard.Services;

// ── Top-level container ──────────────────────────────────────────

public record TriageData(
    DateTime GeneratedAt,
    int TotalCount,
    TriageSummaryStats Summary,
    List<LabelCount> ByType,
    List<LabelCount> ByArea,
    List<LabelCount> ByAction,
    List<LabelCount> BySeverity,
    List<TriagedIssue> Issues
);

public record TriageSummaryStats(
    int NeedsInvestigation,
    int Closeable,
    int QuickWins,
    int NeedsHumanReview,
    int Regressions,
    int Abandoned
);

// ── Per-issue triage ─────────────────────────────────────────────

public record TriagedIssue(
    int Number,
    string Repo,
    DateTime AnalyzedAt,
    string Summary,
    string Url,
    Classification Type,
    Classification? Area,
    List<Classification>? Backends,
    List<Classification>? Platforms,
    List<Classification>? Tenets,
    Classification? Partner,
    RegressionInfo? Regression,
    FixStatusInfo? FixStatus,
    BugSignals? BugSignals,
    ReproEvidence? ReproEvidence,
    VersionAnalysis? VersionAnalysis,
    Actionability Actionability,
    SuggestedResponse? SuggestedResponse,
    AnalysisNotes? AnalysisNotes,
    ResolutionAnalysis? ResolutionAnalysis
);

// ── Classification (shared shape) ────────────────────────────────

public record Classification(
    string Value,
    double Confidence,
    string Reason
);

// ── Bug signals ──────────────────────────────────────────────────

public record BugSignals(
    bool HasCrash,
    bool HasStackTrace,
    string ReproQuality,
    bool? HasScreenshot,
    bool? HasWorkaround,
    string? WorkaroundSummary,
    List<string>? TargetFrameworks,
    string Severity,
    string SeverityReason
);

// ── Repro evidence ───────────────────────────────────────────────

public record ReproEvidence(
    List<Screenshot>? Screenshots,
    List<Attachment>? Attachments,
    List<RepoLink>? RepoLinks,
    List<int>? RelatedIssues,
    List<string>? StepsToReproduce,
    List<CodeSnippet>? CodeSnippets,
    string? EnvironmentDetails
);

public record Screenshot(string Url, string? Context, string? Source);
public record Attachment(string Url, string Filename, string? Type, string? Source);
public record RepoLink(string Url, string? Description);
public record CodeSnippet(string Code, string? Language, string? Context);

// ── Version analysis ─────────────────────────────────────────────

public record VersionAnalysis(
    List<string>? MentionedVersions,
    string? Era,
    string? CurrentRelevance,
    string Reason,
    string? MigrationPath
);

// ── Regression ───────────────────────────────────────────────────

public record RegressionInfo(
    bool IsRegression,
    double Confidence,
    string Reason,
    string? WorkedInVersion,
    string? BrokeInVersion
);

// ── Fix status ───────────────────────────────────────────────────

public record FixStatusInfo(
    bool LikelyFixed,
    double Confidence,
    string Reason,
    List<int>? RelatedPRs,
    List<string>? RelatedCommits,
    string? FixedInVersion,
    string? VerificationStatus
);

// ── Actionability ────────────────────────────────────────────────

public record Actionability(
    string SuggestedAction,
    double Confidence,
    string Reason,
    bool? RequiresHumanReview,
    bool? Closeable,
    string? CloseReason,
    int? DuplicateOf,
    List<string>? MissingInfo,
    bool? Abandoned,
    string? AbandonedReason
);

// ── Suggested response ───────────────────────────────────────────

public record SuggestedResponse(
    string ResponseType,
    double Confidence,
    string Reason,
    string Draft
);

// ── Analysis notes ───────────────────────────────────────────────

public record AnalysisNotes(
    string Summary,
    List<KeySignal>? KeySignals,
    List<FieldRationale>? FieldRationales,
    List<DocReference>? DocsConsulted,
    string? DocsNotConsulted,
    List<string>? Uncertainties,
    List<string>? Assumptions
);

public record KeySignal(
    string Text,
    string Source,
    string Interpretation,
    List<string>? SupportedFields
);

public record FieldRationale(
    string Field,
    string Chosen,
    string ExpandedReason,
    List<Alternative>? Alternatives
);

public record Alternative(string Value, string WhyRejected);

public record DocReference(
    string Path,
    string Relevance,
    List<string>? UsedFor
);

// ── Resolution analysis ──────────────────────────────────────────

public record ResolutionAnalysis(
    string Hypothesis,
    List<string>? ResearchDone,
    List<ResolutionProposal> Proposals,
    string RecommendedProposal,
    string RecommendedReason
);

public record ResolutionProposal(
    string Title,
    string Description,
    List<string> Steps,
    List<string> Pros,
    List<string> Cons,
    double Confidence,
    string Effort
);
