using System.Text.Json.Serialization;

namespace SkiaSharp.Triage.Models;

[JsonConverter(typeof(JsonStringEnumConverter<SkiaRiskAssessment>))]
public enum SkiaRiskAssessment
{
    [JsonStringEnumMemberName("LOW")] Low,
    [JsonStringEnumMemberName("MEDIUM")] Medium,
    [JsonStringEnumMemberName("HIGH")] High
}

[JsonConverter(typeof(JsonStringEnumConverter<SkiaReviewStatus>))]
public enum SkiaReviewStatus
{
    [JsonStringEnumMemberName("PASS")] Pass,
    [JsonStringEnumMemberName("REVIEW_REQUIRED")] ReviewRequired
}

[JsonConverter(typeof(JsonStringEnumConverter<GeneratedFilesStatus>))]
public enum GeneratedFilesStatus
{
    [JsonStringEnumMemberName("PASS")] Pass,
    [JsonStringEnumMemberName("FAIL")] Fail
}
