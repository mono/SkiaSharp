namespace SkiaSharp.Triage.Models;

// ── Root ─────────────────────────────────────────────────────────

public record FixResult(
    FixMeta Meta,
    FixStatus Status,
    string Summary,
    FixRootCause RootCause,
    FixChanges Changes,
    FixTests Tests,
    FixVerification Verification,
    FixInputs? Inputs = null,
    FixPR? Pr = null,
    FixFeedback? Feedback = null,
    List<int>? RelatedIssues = null
);

// ── Meta ─────────────────────────────────────────────────────────

public record FixMeta(
    string SchemaVersion,
    int Number,
    string Repo,
    DateTime AnalyzedAt
);

// ── Inputs ───────────────────────────────────────────────────────

public record FixInputs(
    string? TriageFile = null,
    string? ReproFile = null
);

// ── Root Cause ───────────────────────────────────────────────────

public record FixRootCause(
    RootCauseCategory Category,
    RootCauseArea Area,
    string Description,
    List<string>? AffectedFiles = null
);

// ── Changes ──────────────────────────────────────────────────────

public record FixChanges(
    List<FixFileChange> Files,
    bool BreakingChange,
    RiskLevel Risk
);

public record FixFileChange(
    string Path,
    ChangeType ChangeType,
    string Summary
);

// ── Tests ────────────────────────────────────────────────────────

public record FixTests(
    bool RegressionTestAdded,
    TestResult Result,
    List<FixTestAdded>? TestsAdded = null,
    string? Command = null
);

public record FixTestAdded(
    string File,
    string Name
);

// ── Verification ─────────────────────────────────────────────────

public record FixVerification(
    ReproScenarioResult ReproScenario,
    string? Notes = null
);

// ── PR ───────────────────────────────────────────────────────────

public record FixPR(
    string Url,
    PRStatus Status,
    int? Number = null
);

// ── Feedback ─────────────────────────────────────────────────────

public record FixFeedback(
    List<FixCorrection>? Corrections = null
);

public record FixCorrection(
    CorrectionSource Source,
    string Topic,
    string Upstream,
    string Corrected
);
