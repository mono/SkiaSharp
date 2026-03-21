using System.Text.Json.Serialization;

namespace SkiaSharp.Triage.Models;

// ── Root ─────────────────────────────────────────────────────────

public record SkiaReviewReport(
    [property: JsonPropertyName("schema_version")] string SchemaVersion,
    SkiaReviewMeta Meta,
    string Summary,
    List<string> Recommendations,
    [property: JsonPropertyName("generated_files")] GeneratedFilesCheck GeneratedFiles,
    [property: JsonPropertyName("upstream_integrity")] SourceIntegrity UpstreamIntegrity,
    [property: JsonPropertyName("interop_integrity")] SourceIntegrity InteropIntegrity,
    [property: JsonPropertyName("deps_audit")] DepsAudit DepsAudit,
    [property: JsonPropertyName("risk_assessment")] SkiaRiskAssessment RiskAssessment
);

// ── Meta ─────────────────────────────────────────────────────────

public record SkiaReviewMeta(
    [property: JsonPropertyName("pr_number")] int PrNumber,
    [property: JsonPropertyName("pr_title")] string? PrTitle,
    [property: JsonPropertyName("pr_state")] string? PrState,
    [property: JsonPropertyName("pr_author")] string? PrAuthor,
    [property: JsonPropertyName("old_upstream_branch")] string OldUpstreamBranch,
    [property: JsonPropertyName("new_upstream_branch")] string NewUpstreamBranch,
    [property: JsonPropertyName("base_sha")] string BaseSha,
    [property: JsonPropertyName("pr_head_sha")] string PrHeadSha,
    [property: JsonPropertyName("upstream_sha")] string UpstreamSha,
    DateTime Timestamp,
    [property: JsonPropertyName("companion_pr")] string? CompanionPr = null
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
    [property: JsonPropertyName("diff_summary")] string DiffSummary
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
    [property: JsonPropertyName("old_diff")] string? OldDiff = null,
    [property: JsonPropertyName("new_diff")] string? NewDiff = null,
    [property: JsonPropertyName("patch_diff")] string? PatchDiff = null
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
    [property: JsonPropertyName("old_url")] string? OldUrl = null,
    [property: JsonPropertyName("old_revision")] string? OldRevision = null,
    [property: JsonPropertyName("new_url")] string? NewUrl = null,
    [property: JsonPropertyName("new_revision")] string? NewRevision = null
);

