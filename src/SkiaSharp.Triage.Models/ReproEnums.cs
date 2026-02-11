using System.Text.Json.Serialization;

namespace SkiaSharp.Triage.Models;

// ── Reproduction Conclusion ──────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<ReproConclusion>))]
public enum ReproConclusion
{
    [JsonStringEnumMemberName("reproduced")] Reproduced,
    [JsonStringEnumMemberName("not-reproduced")] NotReproduced,
    [JsonStringEnumMemberName("wrong-output")] WrongOutput,
    [JsonStringEnumMemberName("needs-platform")] NeedsPlatform,
    [JsonStringEnumMemberName("needs-hardware")] NeedsHardware,
    [JsonStringEnumMemberName("partial")] Partial,
    [JsonStringEnumMemberName("inconclusive")] Inconclusive
}

// ── Step Result ──────────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<StepResult>))]
public enum StepResult
{
    [JsonStringEnumMemberName("success")] Success,
    [JsonStringEnumMemberName("failure")] Failure,
    [JsonStringEnumMemberName("wrong-output")] WrongOutput,
    [JsonStringEnumMemberName("skip")] Skip
}

// ── Step Layer ───────────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<StepLayer>))]
public enum StepLayer
{
    [JsonStringEnumMemberName("csharp")] CSharp,
    [JsonStringEnumMemberName("c-api")] CApi,
    [JsonStringEnumMemberName("native")] Native,
    [JsonStringEnumMemberName("deployment")] Deployment,
    [JsonStringEnumMemberName("setup")] Setup
}

// ── Artifact Source ──────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<ArtifactSource>))]
public enum ArtifactSource
{
    [JsonStringEnumMemberName("issue-attachment")] IssueAttachment,
    [JsonStringEnumMemberName("test-data")] TestData,
    [JsonStringEnumMemberName("generated")] Generated,
    [JsonStringEnumMemberName("external")] External
}

// ── Version Test Result ──────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<VersionTestResult>))]
public enum VersionTestResult
{
    [JsonStringEnumMemberName("reproduced")] Reproduced,
    [JsonStringEnumMemberName("not-reproduced")] NotReproduced,
    [JsonStringEnumMemberName("wrong-output")] WrongOutput,
    [JsonStringEnumMemberName("error")] Error,
    [JsonStringEnumMemberName("not-tested")] NotTested
}

// ── Repro Project Type ───────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<ReproProjectType>))]
public enum ReproProjectType
{
    [JsonStringEnumMemberName("console")] Console,
    [JsonStringEnumMemberName("test")] Test,
    [JsonStringEnumMemberName("existing")] Existing
}

// ── Version Source ───────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter<VersionSource>))]
public enum VersionSource
{
    [JsonStringEnumMemberName("nuget")] NuGet,
    [JsonStringEnumMemberName("source")] Source
}
