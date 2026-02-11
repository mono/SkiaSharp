namespace SkiaSharp.Triage.Models;

// ── Root ─────────────────────────────────────────────────────────

public record TriagedIssue(
    TriageMeta Meta,
    string Summary,
    TriageClassification Classification,
    TriageEvidence Evidence,
    TriageAnalysis Analysis,
    TriageOutput Output,
    string? Url = null
);

// ── Meta ─────────────────────────────────────────────────────────

public record TriageMeta(
    string SchemaVersion,
    int Number,
    string Repo,
    DateTime AnalyzedAt,
    List<string>? CurrentLabels = null,
    string? State = null
);

// ── Classification ───────────────────────────────────────────────

public record TriageClassification(
    ClassifiedField Type,
    ClassifiedField Area,
    List<ClassifiedField>? Backends = null,
    List<ClassifiedField>? Platforms = null,
    List<ClassifiedField>? Tenets = null,
    ClassifiedField? Partner = null
);

public record ClassifiedField(
    string Value,
    double Confidence
);

// ── Evidence ─────────────────────────────────────────────────────

public record TriageEvidence(
    BugSignals? BugSignals = null,
    ReproEvidence? ReproEvidence = null,
    VersionAnalysis? VersionAnalysis = null,
    RegressionInfo? Regression = null,
    FixStatusInfo? FixStatus = null
);

public record BugSignals(
    bool HasCrash,
    bool HasStackTrace,
    ReproQuality ReproQuality,
    BugSeverity Severity,
    string SeverityReason,
    bool? HasWorkaround = null,
    string? WorkaroundSummary = null,
    List<string>? TargetFrameworks = null
);

public record ReproEvidence(
    List<Screenshot>? Screenshots = null,
    List<Attachment>? Attachments = null,
    List<RepoLink>? RepoLinks = null,
    List<int>? RelatedIssues = null,
    List<string>? StepsToReproduce = null,
    List<CodeSnippet>? CodeSnippets = null,
    string? EnvironmentDetails = null
);

public record Screenshot(string Url, string? Context = null, string? Source = null);
public record Attachment(string Url, string Filename, AttachmentType? Type = null, string? Source = null);
public record RepoLink(string Url, string? Description = null);
public record CodeSnippet(string Code, string? Language = null, string? Context = null);

public record VersionAnalysis(
    string Reason,
    List<string>? MentionedVersions = null,
    CurrentRelevance? CurrentRelevance = null,
    string? MigrationPath = null
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
    string? FixedInVersion = null,
    VerificationStatus? VerificationStatus = null
);

// ── Analysis ─────────────────────────────────────────────────────

public record TriageAnalysis(
    string Summary,
    List<KeySignal> KeySignals,
    List<FieldRationale> FieldRationales,
    List<string>? Uncertainties = null,
    List<string>? Assumptions = null,
    ResolutionAnalysis? Resolution = null,
    List<CodeInvestigationEntry>? CodeInvestigation = null
);

public record KeySignal(
    string Text,
    string Source,
    string Interpretation
);

public record CodeInvestigationEntry(
    string File,
    string Lines,
    string Relevance
);

public record FieldRationale(
    string Field,
    string Chosen,
    string ExpandedReason,
    List<Alternative>? Alternatives = null
);

public record Alternative(string Value, string WhyRejected);

public record ResolutionAnalysis(
    string Hypothesis,
    List<ResolutionProposal> Proposals,
    string RecommendedProposal,
    string RecommendedReason
);

public record ResolutionProposal(
    string Title,
    string Description,
    double Confidence,
    ProposalEffort Effort,
    ProposalCategory? Category = null,
    string? CodeSnippet = null,
    ProposalValidation? Validated = null
);

// ── Output ───────────────────────────────────────────────────────

public record TriageOutput(
    Actionability Actionability,
    List<TriageAction>? Actions = null
);

public record Actionability(
    SuggestedAction SuggestedAction,
    double Confidence,
    string Reason,
    bool? RequiresHumanReview = null,
    List<MissingInfoKind>? MissingInfo = null
);
