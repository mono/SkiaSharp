using System.Text.Json.Serialization;

namespace SkiaSharp.Triage.Models;

// ── Root ─────────────────────────────────────────────────────────

public record SkiaReviewReport(
    SkiaReviewMeta Meta,
    string Summary,
    List<string> Recommendations,
    GeneratedFilesCheck GeneratedFiles,
    SourceIntegrity UpstreamIntegrity,
    SourceIntegrity InteropIntegrity,
    DepsAudit DepsAudit,
    SkiaRiskAssessment RiskAssessment,
    CompanionPr? CompanionPr = null
);

// ── Meta ─────────────────────────────────────────────────────────

public record SkiaReviewMeta(
    string SchemaVersion,
    int SkiaPrNumber,
    int? SkiasharpPrNumber,
    string Repo,
    string UpstreamBranch,
    string OldUpstreamBranch,
    DateTime AnalyzedAt,
    SkiaReviewShas Shas
);

public record SkiaReviewShas(
    string PrHead,
    string Base,
    string Upstream
);

// ── Generated Files ──────────────────────────────────────────────

public record GeneratedFilesCheck(
    GeneratedFilesStatus Status,
    string Summary,
    List<string> Recommendations,
    List<string> Checked,
    List<GeneratedFileMismatch>? Mismatches = null
);

public record GeneratedFileMismatch(
    string File,
    string DiffSummary
);

// ── Source Integrity (upstream + interop share the same shape) ────

public record SourceIntegrity(
    SkiaReviewStatus Status,
    string Summary,
    List<string> Recommendations,
    List<SourceFileAdded> Added,
    List<SourceFileRemoved> Removed,
    List<SourceFileChanged> Changed,
    int Unchanged
);

public record SourceFileAdded(
    string Path,
    string Summary,
    string? Diff = null
);

public record SourceFileRemoved(
    string Path,
    string Summary,
    string? Diff = null
);

public record SourceFileChanged(
    string Path,
    string Summary,
    string? OldDiff = null,
    string? NewDiff = null,
    string? PatchDiff = null
);

// ── DEPS Audit ───────────────────────────────────────────────────

public record DepsAudit(
    SkiaReviewStatus Status,
    string Summary,
    List<string> Recommendations,
    List<DepAdded> Added,
    List<DepRemoved> Removed,
    List<DepChanged> Changed,
    int Unchanged
);

public record DepAdded(
    string Name,
    string Summary,
    string? Url = null,
    string? Revision = null
);

public record DepRemoved(
    string Name,
    string Summary,
    string? Url = null,
    string? Revision = null
);

public record DepChanged(
    string Name,
    string Summary,
    string? OldUrl = null,
    string? OldRevision = null,
    string? NewUrl = null,
    string? NewRevision = null
);

// ── Companion PR ─────────────────────────────────────────────────

public record CompanionPr(
    int PrNumber,
    int? FilesChanged = null,
    List<string>? GeneratedFilesSkipped = null,
    List<CompanionPrFinding>? Findings = null
);

public record CompanionPrFinding(
    string File,
    string Finding
);
