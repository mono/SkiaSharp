using System.Text.Json.Serialization;

namespace SkiaSharp.Triage.Models;

// ── Fix Status ───────────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<FixStatus>))]
public enum FixStatus
{
    [JsonStringEnumMemberName("in-progress")] InProgress,
    [JsonStringEnumMemberName("fixed")] Fixed,
    [JsonStringEnumMemberName("cannot-fix")] CannotFix,
    [JsonStringEnumMemberName("needs-info")] NeedsInfo,
    [JsonStringEnumMemberName("duplicate")] Duplicate
}

// ── Root Cause ───────────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<RootCauseCategory>))]
public enum RootCauseCategory
{
    [JsonStringEnumMemberName("logic-error")] LogicError,
    [JsonStringEnumMemberName("memory-safety")] MemorySafety,
    [JsonStringEnumMemberName("threading")] Threading,
    [JsonStringEnumMemberName("api-misuse")] ApiMisuse,
    [JsonStringEnumMemberName("dependency")] Dependency,
    [JsonStringEnumMemberName("upstream-skia")] UpstreamSkia,
    [JsonStringEnumMemberName("missing-feature")] MissingFeature,
    [JsonStringEnumMemberName("other")] Other
}

[JsonConverter(typeof(JsonStringEnumConverter<RootCauseArea>))]
public enum RootCauseArea
{
    [JsonStringEnumMemberName("managed")] Managed,
    [JsonStringEnumMemberName("binding")] Binding,
    [JsonStringEnumMemberName("native")] Native,
    [JsonStringEnumMemberName("build")] Build,
    [JsonStringEnumMemberName("packaging")] Packaging,
    [JsonStringEnumMemberName("tests")] Tests,
    [JsonStringEnumMemberName("docs")] Docs
}

// ── Changes ──────────────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<ChangeType>))]
public enum ChangeType
{
    [JsonStringEnumMemberName("added")] Added,
    [JsonStringEnumMemberName("modified")] Modified,
    [JsonStringEnumMemberName("removed")] Removed
}

[JsonConverter(typeof(JsonStringEnumConverter<RiskLevel>))]
public enum RiskLevel
{
    [JsonStringEnumMemberName("low")] Low,
    [JsonStringEnumMemberName("medium")] Medium,
    [JsonStringEnumMemberName("high")] High
}

// ── Tests ────────────────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<TestResult>))]
public enum TestResult
{
    [JsonStringEnumMemberName("pass")] Pass,
    [JsonStringEnumMemberName("fail")] Fail,
    [JsonStringEnumMemberName("not-run")] NotRun
}

// ── Verification ─────────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<ReproScenarioResult>))]
public enum ReproScenarioResult
{
    [JsonStringEnumMemberName("passed")] Passed,
    [JsonStringEnumMemberName("failed")] Failed,
    [JsonStringEnumMemberName("not-run")] NotRun,
    [JsonStringEnumMemberName("not-applicable")] NotApplicable
}

// ── PR ───────────────────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<PRStatus>))]
public enum PRStatus
{
    [JsonStringEnumMemberName("draft")] Draft,
    [JsonStringEnumMemberName("open")] Open,
    [JsonStringEnumMemberName("merged")] Merged,
    [JsonStringEnumMemberName("closed")] Closed
}

// ── Feedback ─────────────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<CorrectionSource>))]
public enum CorrectionSource
{
    [JsonStringEnumMemberName("triage")] Triage,
    [JsonStringEnumMemberName("repro")] Repro
}
