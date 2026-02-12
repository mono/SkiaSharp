namespace SkiaSharp.Triage.Models;

// ── Root ─────────────────────────────────────────────────────────

public record TriagedIssue(
    TriageMeta Meta,
    string Summary,
    TriageClassification Classification,
    TriageEvidence Evidence,
    TriageAnalysis Analysis,
    TriageOutput Output
);

// ── Meta ─────────────────────────────────────────────────────────

public record TriageMeta(
    string SchemaVersion,
    int Number,
    string Repo,
    DateTime AnalyzedAt,
    List<string>? CurrentLabels = null
);

// ── Classification ───────────────────────────────────────────────

public record TriageClassification(
    ClassifiedField Type,
    ClassifiedField? Area = null,
    List<string>? Platforms = null,
    List<string>? Backends = null,
    List<string>? Tenets = null,
    string? Partner = null
);

public record ClassifiedField(
    string Value,
    double Confidence
);

// ── Evidence ─────────────────────────────────────────────────────

public record TriageEvidence(
    ReproEvidence? ReproEvidence = null,
    BugSignals? BugSignals = null,
    VersionAnalysis? VersionAnalysis = null,
    RegressionInfo? Regression = null,
    FixStatusInfo? FixStatus = null
);

public record BugSignals(
    BugSeverity? Severity = null,
    bool? IsRegression = null,
    string? ErrorType = null,
    string? ErrorMessage = null,
    string? StackTrace = null,
    ReproQuality? ReproQuality = null,
    List<string>? TargetFrameworks = null
);

public record ReproEvidence(
    List<string>? StepsToReproduce = null,
    List<string>? CodeSnippets = null,
    List<Screenshot>? Screenshots = null,
    List<Attachment>? Attachments = null,
    string? EnvironmentDetails = null,
    List<int>? RelatedIssues = null,
    List<RepoLink>? RepoLinks = null
);

public record Screenshot(string Url, string? Description = null);
public record Attachment(string Url, string Filename, string? Description = null);
public record RepoLink(string Url, string? Description = null);

public record VersionAnalysis(
    List<string>? MentionedVersions = null,
    string? WorkedIn = null,
    string? BrokeIn = null,
    CurrentRelevance? CurrentRelevance = null,
    string? RelevanceReason = null
);

public record RegressionInfo(
    bool IsRegression,
    double Confidence,
    string Reason,
    string? WorkedInVersion = null,
    string? BrokeInVersion = null
);

public record FixStatusInfo(
    bool LikelyFixed,
    double Confidence,
    string Reason,
    List<int>? RelatedPRs = null,
    List<string>? RelatedCommits = null,
    string? FixedInVersion = null
);

// ── Analysis ─────────────────────────────────────────────────────

public record TriageAnalysis(
    string Summary,
    List<CodeInvestigationEntry> CodeInvestigation,
    string? Rationale = null,
    List<KeySignal>? KeySignals = null,
    string? ErrorFingerprint = null,
    List<string>? Workarounds = null,
    List<string>? NextQuestions = null,
    ResolutionAnalysis? Resolution = null
);

public record KeySignal(
    string Text,
    string Source,
    string? Interpretation = null
);

public record CodeInvestigationEntry(
    string File,
    string Finding,
    CodeRelevance Relevance,
    string? Lines = null
);

public record ResolutionAnalysis(
    string? Hypothesis = null,
    List<ResolutionProposal>? Proposals = null,
    string? RecommendedProposal = null,
    string? RecommendedReason = null
);

public record ResolutionProposal(
    string Description,
    string? Title = null,
    string? CodeSnippet = null,
    double? Confidence = null,
    ProposalEffort? Effort = null
);

// ── Output ───────────────────────────────────────────────────────

public record TriageOutput(
    Actionability Actionability,
    List<TriageAction> Actions,
    List<string>? MissingInfo = null
);

public record Actionability(
    SuggestedAction SuggestedAction,
    double Confidence,
    string Reason
);

public record TriageAction(
    ActionType Type,
    string Description,
    ActionRisk? Risk = null,
    double? Confidence = null,
    List<string>? Labels = null,
    string? Comment = null,
    int? LinkedIssue = null
);
