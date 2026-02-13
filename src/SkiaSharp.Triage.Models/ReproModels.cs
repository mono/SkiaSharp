namespace SkiaSharp.Triage.Models;

// ── Root ─────────────────────────────────────────────────────────

public record ReproResult(
    ReproMeta Meta,
    ReproConclusion Conclusion,
    string Notes,
    List<ReproStep> ReproductionSteps,
    ReproEnvironment Environment,
    ReproInputs? Inputs = null,
    string? Assessment = null,
    string? Scope = null,
    string? ReproductionTime = null,
    List<ReproVersionResult>? VersionResults = null,
    ReproProject? ReproProject = null,
    List<ReproArtifact>? Artifacts = null,
    ReproErrorMessages? ErrorMessages = null,
    List<string>? Blockers = null,
    ReproFeedback? Feedback = null,
    ReproOutput? Output = null
);

// ── Output ───────────────────────────────────────────────────────

public record ReproOutput(
    ReproProposedResponse ProposedResponse,
    Actionability Actionability,
    List<string>? Workarounds = null,
    List<TriageAction>? Actions = null,
    List<string>? MissingInfo = null
);

public record ReproProposedResponse(
    ResponseStatus Status,
    string Body,
    string? Summary = null
);

// ── Meta ─────────────────────────────────────────────────────────

public record ReproMeta(
    string SchemaVersion,
    int Number,
    string Repo,
    DateTime AnalyzedAt
);

// ── Inputs ───────────────────────────────────────────────────────

public record ReproInputs(
    string? TriageFile = null
);

// ── Steps ────────────────────────────────────────────────────────

public record ReproStep(
    int StepNumber,
    string Description,
    StepLayer Layer,
    string? Command = null,
    int? ExitCode = null,
    string? Output = null,
    List<ReproFileCreated>? FilesCreated = null,
    StepResult? Result = null
);

public record ReproFileCreated(
    string Filename,
    string Description,
    string? Content = null
);

// ── Version Results ──────────────────────────────────────────────

public record ReproVersionResult(
    string Version,
    VersionSource Source,
    VersionTestResult Result,
    string? Notes = null,
    string? Platform = null
);

// ── Environment ──────────────────────────────────────────────────

public record ReproEnvironment(
    string Os,
    string Arch,
    string DotnetVersion,
    string SkiaSharpVersion,
    bool DockerUsed,
    string? DotnetSdkVersion = null,
    string? WasmToolsVersion = null,
    ReproGpu? Gpu = null
);

public record ReproGpu(
    bool Available,
    string? Backend = null
);

// ── Artifacts ────────────────────────────────────────────────────

public record ReproArtifact(
    string Filename,
    string Description,
    ArtifactSource Source,
    bool Available,
    string? Url = null
);

// ── Project ──────────────────────────────────────────────────────

public record ReproProject(
    ReproProjectType Type,
    string Tfm,
    List<ReproPackage> Packages
);

public record ReproPackage(
    string Name,
    string Version
);

// ── Error Messages ───────────────────────────────────────────────

public record ReproErrorMessages(
    string? PrimaryError = null,
    string? StackTrace = null,
    List<string>? AdditionalErrors = null
);

// ── Feedback ─────────────────────────────────────────────────────

public record ReproFeedback(
    List<TriageCorrection>? TriageCorrections = null
);

public record TriageCorrection(
    string Topic,
    string Upstream,
    string Corrected
);
