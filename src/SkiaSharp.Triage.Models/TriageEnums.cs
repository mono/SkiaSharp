using System.Text.Json.Serialization;

namespace SkiaSharp.Triage.Models;

// ── Bug Signals ──────────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<BugSeverity>))]
public enum BugSeverity
{
    [JsonStringEnumMemberName("critical")] Critical,
    [JsonStringEnumMemberName("high")] High,
    [JsonStringEnumMemberName("medium")] Medium,
    [JsonStringEnumMemberName("low")] Low
}

[JsonConverter(typeof(JsonStringEnumConverter<ErrorType>))]
public enum ErrorType
{
    [JsonStringEnumMemberName("crash")] Crash,
    [JsonStringEnumMemberName("exception")] Exception,
    [JsonStringEnumMemberName("wrong-output")] WrongOutput,
    [JsonStringEnumMemberName("missing-output")] MissingOutput,
    [JsonStringEnumMemberName("performance")] Performance,
    [JsonStringEnumMemberName("build-error")] BuildError,
    [JsonStringEnumMemberName("memory-leak")] MemoryLeak,
    [JsonStringEnumMemberName("platform-specific")] PlatformSpecific,
    [JsonStringEnumMemberName("other")] Other
}

[JsonConverter(typeof(JsonStringEnumConverter<ReproQuality>))]
public enum ReproQuality
{
    [JsonStringEnumMemberName("complete")] Complete,
    [JsonStringEnumMemberName("partial")] Partial,
    [JsonStringEnumMemberName("none")] None
}

// ── Evidence ─────────────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<CurrentRelevance>))]
public enum CurrentRelevance
{
    [JsonStringEnumMemberName("likely")] Likely,
    [JsonStringEnumMemberName("unlikely")] Unlikely,
    [JsonStringEnumMemberName("unknown")] Unknown
}

// ── Resolution ───────────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<ProposalCategory>))]
public enum ProposalCategory
{
    [JsonStringEnumMemberName("workaround")] Workaround,
    [JsonStringEnumMemberName("fix")] Fix,
    [JsonStringEnumMemberName("alternative")] Alternative,
    [JsonStringEnumMemberName("investigation")] Investigation
}

[JsonConverter(typeof(JsonStringEnumConverter<ProposalValidation>))]
public enum ProposalValidation
{
    [JsonStringEnumMemberName("untested")] Untested,
    [JsonStringEnumMemberName("yes")] Yes,
    [JsonStringEnumMemberName("no")] No
}

[JsonConverter(typeof(JsonStringEnumConverter<ProposalEffort>))]
public enum ProposalEffort
{
    [JsonStringEnumMemberName("trivial")] Trivial,
    [JsonStringEnumMemberName("small")] Small,
    [JsonStringEnumMemberName("medium")] Medium,
    [JsonStringEnumMemberName("large")] Large
}

// ── Output / Actionability ───────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<SuggestedAction>))]
public enum SuggestedAction
{
    [JsonStringEnumMemberName("needs-investigation")] NeedsInvestigation,
    [JsonStringEnumMemberName("close-as-fixed")] CloseAsFixed,
    [JsonStringEnumMemberName("close-as-by-design")] CloseAsByDesign,
    [JsonStringEnumMemberName("close-with-docs")] CloseWithDocs,
    [JsonStringEnumMemberName("close-as-duplicate")] CloseAsDuplicate,
    [JsonStringEnumMemberName("convert-to-discussion")] ConvertToDiscussion,
    [JsonStringEnumMemberName("request-info")] RequestInfo,
    [JsonStringEnumMemberName("keep-open")] KeepOpen
}

// ── Repro Platform ───────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<ReproPlatform>))]
public enum ReproPlatform
{
    [JsonStringEnumMemberName("linux")] Linux,
    [JsonStringEnumMemberName("macos")] MacOS,
    [JsonStringEnumMemberName("windows")] Windows
}

// ── Actions ──────────────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<ActionType>))]
public enum ActionType
{
    [JsonStringEnumMemberName("update-labels")] UpdateLabels,
    [JsonStringEnumMemberName("add-comment")] AddComment,
    [JsonStringEnumMemberName("close-issue")] CloseIssue,
    [JsonStringEnumMemberName("convert-to-discussion")] ConvertToDiscussion,
    [JsonStringEnumMemberName("link-related")] LinkRelated,
    [JsonStringEnumMemberName("link-duplicate")] LinkDuplicate,
    [JsonStringEnumMemberName("update-project")] UpdateProject,
    [JsonStringEnumMemberName("set-milestone")] SetMilestone
}

[JsonConverter(typeof(JsonStringEnumConverter<ActionRisk>))]
public enum ActionRisk
{
    [JsonStringEnumMemberName("low")] Low,
    [JsonStringEnumMemberName("medium")] Medium,
    [JsonStringEnumMemberName("high")] High
}

// ── Code Investigation ───────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<CodeRelevance>))]
public enum CodeRelevance
{
    [JsonStringEnumMemberName("direct")] Direct,
    [JsonStringEnumMemberName("related")] Related,
    [JsonStringEnumMemberName("context")] Context
}
