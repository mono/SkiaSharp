using SkiaSharp.Triage.Models;

namespace SkiaSharp.Triage.Dashboard.Helpers;

public static class TriageFormatHelper
{
    public static string FormatLabel(string value) =>
        string.Join(' ', StripPrefix(value).Split('-').Select(w =>
            w.Length > 0 ? char.ToUpper(w[0]) + w[1..] : w));

    public static string StripPrefix(string value)
    {
        var slashIndex = value.IndexOf('/');
        return slashIndex >= 0 ? value[(slashIndex + 1)..] : value;
    }

    public static string GetConfidenceClass(double c) => c switch
    {
        >= 0.85 => "confidence-high",
        >= 0.70 => "confidence-medium",
        _ => "confidence-low"
    };

    public static int SeverityOrder(string? severity) => severity switch
    {
        "critical" => 4, "high" => 3, "medium" => 2, "low" => 1, _ => 0
    };

    public static string GetRiskStyle(ActionRisk risk) => risk switch
    {
        ActionRisk.Low => "background: #28a745; color: white;",
        ActionRisk.Medium => "background: #ffc107; color: #212529;",
        ActionRisk.High => "background: #dc3545; color: white;",
        _ => ""
    };

    public static string GetRiskStyle(RiskLevel risk) => risk switch
    {
        RiskLevel.Low => "background: #28a745; color: white;",
        RiskLevel.Medium => "background: #ffc107; color: #212529;",
        RiskLevel.High => "background: #dc3545; color: white;",
        _ => ""
    };

    public static string GetReproTooltip(ReproQuality quality) => quality switch
    {
        ReproQuality.Complete => "Full reproducible project provided — easiest to diagnose",
        ReproQuality.Partial => "Code snippets provided — requires assembling into repro",
        ReproQuality.None => "No reproduction material — diagnosis will be difficult",
        _ => "Reproduction quality assessment"
    };

    public static string GetRiskTooltip(ActionRisk risk) => risk switch
    {
        ActionRisk.Low => "Reversible action, cosmetic change — safe to auto-apply",
        ActionRisk.Medium => "State change that requires moderate caution",
        ActionRisk.High => "Irreversible or reputation-sensitive — needs human review",
        _ => "Action risk level"
    };

    public static string GetActionTypeTooltip(ActionType type) => type switch
    {
        ActionType.UpdateLabels => "Add or remove GitHub labels on the issue",
        ActionType.AddComment => "Post a comment on the issue thread",
        ActionType.CloseIssue => "Close the issue with a reason",
        ActionType.LinkDuplicate => "Mark as duplicate of another issue",
        ActionType.LinkRelated => "Cross-reference related issues",
        ActionType.ConvertToDiscussion => "Convert issue to a GitHub Discussion",
        ActionType.UpdateProject => "Update GitHub project fields",
        ActionType.SetMilestone => "Set milestone on the issue",
        _ => $"Action: {type}"
    };

    public static string GetConclusionBadgeClass(string conclusion) => conclusion.ToLowerInvariant() switch
    {
        "reproduced" => "badge-repro-success",
        "not-reproduced" => "badge-repro-failure",
        "partial" => "badge-repro-partial",
        "needs-hardware" => "badge-repro-blocked",
        "needs-info" => "badge-repro-blocked",
        "blocked" => "badge-repro-blocked",
        _ => "badge-repro-unknown"
    };

    public static string GetFileUrl(string repo, CodeInvestigationEntry entry)
    {
        var file = entry.File.TrimEnd('/');
        var url = $"https://github.com/{repo}/blob/main/{file}";
        if (!string.IsNullOrEmpty(entry.Lines) && entry.Lines != "N/A" && entry.Lines.Contains('-'))
        {
            var parts = entry.Lines.Split('-');
            if (parts.Length == 2)
                url += $"#L{parts[0]}-L{parts[1]}";
        }
        else if (!string.IsNullOrEmpty(entry.Lines) && entry.Lines != "N/A")
        {
            url += $"#L{entry.Lines}";
        }
        return url;
    }
}
