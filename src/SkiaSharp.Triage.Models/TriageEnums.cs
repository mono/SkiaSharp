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
    [JsonStringEnumMemberName("close-with-docs")] CloseWithDocs,
    [JsonStringEnumMemberName("close-as-duplicate")] CloseAsDuplicate,
    [JsonStringEnumMemberName("convert-to-discussion")] ConvertToDiscussion,
    [JsonStringEnumMemberName("request-info")] RequestInfo,
    [JsonStringEnumMemberName("keep-open")] KeepOpen
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
