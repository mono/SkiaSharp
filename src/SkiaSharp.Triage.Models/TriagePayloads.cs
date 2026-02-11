using System.Text.Json;

namespace SkiaSharp.Triage.Models;

// ── Action (with typed payload access) ───────────────────────────

public record TriageAction(
    string Id,
    ActionType Type,
    ActionRisk Risk,
    string Description,
    string Reason,
    double Confidence,
    string? DependsOn = null,
    JsonElement? Payload = null
)
{
    public UpdateLabelsPayload? GetLabelsPayload() =>
        Type == ActionType.UpdateLabels && Payload.HasValue
            ? JsonSerializer.Deserialize<UpdateLabelsPayload>(Payload.Value, TriageJsonOptions.Default)
            : null;

    public AddCommentPayload? GetCommentPayload() =>
        Type == ActionType.AddComment && Payload.HasValue
            ? JsonSerializer.Deserialize<AddCommentPayload>(Payload.Value, TriageJsonOptions.Default)
            : null;

    public CloseIssuePayload? GetClosePayload() =>
        Type == ActionType.CloseIssue && Payload.HasValue
            ? JsonSerializer.Deserialize<CloseIssuePayload>(Payload.Value, TriageJsonOptions.Default)
            : null;

    public LinkDuplicatePayload? GetDuplicatePayload() =>
        Type == ActionType.LinkDuplicate && Payload.HasValue
            ? JsonSerializer.Deserialize<LinkDuplicatePayload>(Payload.Value, TriageJsonOptions.Default)
            : null;

    public LinkRelatedPayload? GetRelatedPayload() =>
        Type == ActionType.LinkRelated && Payload.HasValue
            ? JsonSerializer.Deserialize<LinkRelatedPayload>(Payload.Value, TriageJsonOptions.Default)
            : null;

    public ConvertToDiscussionPayload? GetDiscussionPayload() =>
        Type == ActionType.ConvertToDiscussion && Payload.HasValue
            ? JsonSerializer.Deserialize<ConvertToDiscussionPayload>(Payload.Value, TriageJsonOptions.Default)
            : null;

    public UpdateProjectPayload? GetProjectPayload() =>
        Type == ActionType.UpdateProject && Payload.HasValue
            ? JsonSerializer.Deserialize<UpdateProjectPayload>(Payload.Value, TriageJsonOptions.Default)
            : null;

    public SetMilestonePayload? GetMilestonePayload() =>
        Type == ActionType.SetMilestone && Payload.HasValue
            ? JsonSerializer.Deserialize<SetMilestonePayload>(Payload.Value, TriageJsonOptions.Default)
            : null;
}

// ── Payload Types ────────────────────────────────────────────────

public record UpdateLabelsPayload(
    List<string>? LabelsToAdd = null,
    List<string>? LabelsToRemove = null
);

public record AddCommentPayload(
    CommentType CommentType,
    string DraftBody,
    bool RequiresHumanEdit,
    string? FinalBody = null,
    string? DedupeToken = null
);

public record CloseIssuePayload(
    CloseReason Reason,
    string? Comment = null
);

public record LinkRelatedPayload(
    List<int> RelatedIssues,
    string? Comment = null
);

public record LinkDuplicatePayload(
    int DuplicateOf,
    string? Comment = null
);

public record ConvertToDiscussionPayload(
    string CategorySlug,
    string? Comment = null
);

public record UpdateProjectPayload(
    string ProjectId,
    List<ProjectFieldUpdate> FieldUpdates
);

public record ProjectFieldUpdate(
    string FieldName,
    string Value
);

public record SetMilestonePayload(
    string MilestoneTitle
);
