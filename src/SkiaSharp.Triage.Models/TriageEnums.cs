using System.Text.Json.Serialization;

namespace SkiaSharp.Triage.Models;

// ── Bug Signals ──────────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<ReproQuality>))]
public enum ReproQuality
{
    [JsonStringEnumMemberName("complete")] Complete,
    [JsonStringEnumMemberName("partial")] Partial,
    [JsonStringEnumMemberName("steps-only")] StepsOnly,
    [JsonStringEnumMemberName("none")] None
}

[JsonConverter(typeof(JsonStringEnumConverter<BugSeverity>))]
public enum BugSeverity
{
    [JsonStringEnumMemberName("critical")] Critical,
    [JsonStringEnumMemberName("high")] High,
    [JsonStringEnumMemberName("medium")] Medium,
    [JsonStringEnumMemberName("low")] Low
}

// ── Evidence ─────────────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<VerificationStatus>))]
public enum VerificationStatus
{
    [JsonStringEnumMemberName("unverified")] Unverified,
    [JsonStringEnumMemberName("verified-fixed")] VerifiedFixed,
    [JsonStringEnumMemberName("verified-still-broken")] VerifiedStillBroken,
    [JsonStringEnumMemberName("inconclusive")] Inconclusive
}

[JsonConverter(typeof(JsonStringEnumConverter<CurrentRelevance>))]
public enum CurrentRelevance
{
    [JsonStringEnumMemberName("likely")] Likely,
    [JsonStringEnumMemberName("unlikely")] Unlikely,
    [JsonStringEnumMemberName("unknown")] Unknown
}

[JsonConverter(typeof(JsonStringEnumConverter<AttachmentType>))]
public enum AttachmentType
{
    [JsonStringEnumMemberName("repro-project")] ReproProject,
    [JsonStringEnumMemberName("log-file")] LogFile,
    [JsonStringEnumMemberName("config-file")] ConfigFile,
    [JsonStringEnumMemberName("other")] Other
}

// ── Resolution ───────────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<ProposalEffort>))]
public enum ProposalEffort
{
    [JsonStringEnumMemberName("low")] Low,
    [JsonStringEnumMemberName("medium")] Medium,
    [JsonStringEnumMemberName("high")] High
}

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
    [JsonStringEnumMemberName("yes")] Yes,
    [JsonStringEnumMemberName("no")] No,
    [JsonStringEnumMemberName("untested")] Untested
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

[JsonConverter(typeof(JsonStringEnumConverter<MissingInfoKind>))]
public enum MissingInfoKind
{
    [JsonStringEnumMemberName("reproduction-steps")] ReproductionSteps,
    [JsonStringEnumMemberName("version-number")] VersionNumber,
    [JsonStringEnumMemberName("platform")] Platform,
    [JsonStringEnumMemberName("stack-trace")] StackTrace,
    [JsonStringEnumMemberName("expected-behavior")] ExpectedBehavior,
    [JsonStringEnumMemberName("actual-behavior")] ActualBehavior,
    [JsonStringEnumMemberName("sample-project")] SampleProject,
    [JsonStringEnumMemberName("device-info")] DeviceInfo
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

[JsonConverter(typeof(JsonStringEnumConverter<CommentType>))]
public enum CommentType
{
    [JsonStringEnumMemberName("answer")] Answer,
    [JsonStringEnumMemberName("documentation")] Documentation,
    [JsonStringEnumMemberName("request-info")] RequestInfo,
    [JsonStringEnumMemberName("close-message")] CloseMessage,
    [JsonStringEnumMemberName("duplicate-notice")] DuplicateNotice,
    [JsonStringEnumMemberName("workaround")] Workaround
}

[JsonConverter(typeof(JsonStringEnumConverter<CloseReason>))]
public enum CloseReason
{
    [JsonStringEnumMemberName("completed")] Completed,
    [JsonStringEnumMemberName("not_planned")] NotPlanned
}
