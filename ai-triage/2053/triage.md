# Issue Triage Report — #2053

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T11:44:30Z |
| Type | type/question (0.85 (85%)) |
| Area | area/libSkiaSharp.native (0.90 (90%)) |
| Suggested action | needs-info (0.75 (75%)) |

**Issue Summary:** Question about deploying SkiaSharp with QuestPDF on Azure Function V3 (netcore3.1) without Visual Studio Publish — TypeInitializationException from SKAbstractManagedWStream at runtime on Azure, indicating missing Linux native library.

**Analysis:** The TypeInitializationException from SKAbstractManagedWStream indicates the native libSkiaSharp binary is not available at runtime on Azure Functions. Azure Functions runs on Linux; on Linux, native assets are NOT auto-included and require explicit addition of SkiaSharp.NativeAssets.Linux.NoDependencies in the application project. The static constructor of SKAbstractManagedWStream P/Invokes into libSkiaSharp via sk_managedwstream_set_procs — if the native library is absent, a DllNotFoundException is thrown and wrapped as TypeInitializationException.

**Recommendations:** **needs-info** — Maintainer asked for net6 results in 2022 but reporter never followed up. A clear workaround exists (add NativeAssets.Linux.NoDependencies), but confirmation of whether it resolves the issue is pending.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/libSkiaSharp.native |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

**Related issues:** #1001

**Code snippets:**

```csharp
SkiaSharp.SKAbstractManagedWStream' threw an exception
   at SkiaSharp.SKAbstractManagedWStream..ctor(Boolean owns)
   at SkiaSharp.SKDocument.CreatePdf(Stream stream, SKDocumentPdfMetadata metadata)
   at QuestPDF.Drawing.DocumentGenerator.GeneratePdf(Stream stream, IDocument document)
   at QuestPDF.Fluent.GenerateExtensions.GeneratePdf(IDocument document, Stream stream)
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | netcore3.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | netcore3.1 is end-of-life and Azure Functions V3 has been superseded by V4 on net6+. |

## Analysis

### Technical Summary

The TypeInitializationException from SKAbstractManagedWStream indicates the native libSkiaSharp binary is not available at runtime on Azure Functions. Azure Functions runs on Linux; on Linux, native assets are NOT auto-included and require explicit addition of SkiaSharp.NativeAssets.Linux.NoDependencies in the application project. The static constructor of SKAbstractManagedWStream P/Invokes into libSkiaSharp via sk_managedwstream_set_procs — if the native library is absent, a DllNotFoundException is thrown and wrapped as TypeInitializationException.

### Rationale

Classified as type/question because the title explicitly uses [QUESTION] and the reporter asks 'is there any way to implement SkiaSharp with QuestPDF on Azure Function V3 without using the publish function from VS?'. The issue is a deployment/packaging question. Azure Functions runs on Linux; Linux native assets must be explicitly referenced (unlike Windows/macOS which are auto-included). The workaround is adding SkiaSharp.NativeAssets.Linux.NoDependencies in the application project. Maintainer already asked about net6 results in 2022 but reporter never responded, so needs-info is the appropriate action.

### Key Signals

- "SkiaSharp.SKAbstractManagedWStream' threw an exception at SkiaSharp.SKAbstractManagedWStream..ctor(Boolean owns)" — **Issue body** (TypeInitializationException from static constructor — indicates native library P/Invoke fails during initialization. Strongly suggests libSkiaSharp native binary is not available in the Azure environment.)
- "When I try to execute the same code on Visual Studio it does work with no problem at all." — **Issue body** (Classic 'works locally, fails in deployment' pattern for native library deployment. Locally, VS deploys native binaries automatically; Azure Functions deployment on Linux requires explicit packaging of SkiaSharp.NativeAssets.Linux.NoDependencies.)
- "I assume it's because System.Memory.dll is not updated." — **Comment by reporter (brandonvqz)** (Reporter may be misattributing root cause. System.Memory version mismatch is unlikely to cause a TypeInitializationException at SkiaSharp initialization. The root cause is the missing Linux native binary.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKAbstractManagedWStream.cs` | 16-26 | direct | The static constructor (lines 16-26) calls SkiaApi.sk_managedwstream_set_procs(delegates) which P/Invokes into libSkiaSharp. If the native library is not found or fails to load, a DllNotFoundException is thrown here, which the CLR wraps in a TypeInitializationException — exactly matching the reported exception. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 17221-17237 | direct | sk_managedwstream_set_procs is resolved via dynamic symbol lookup (GetSymbol). This is the actual P/Invoke entry point into libSkiaSharp native binary. Confirms that failing to find libSkiaSharp.so on Linux would cause TypeInitializationException at first use. |

### Workarounds

- Add SkiaSharp.NativeAssets.Linux.NoDependencies as a direct PackageReference in the Azure Functions project (the executable project, not a library).
- Run 'dotnet publish --runtime linux-x64 --self-contained false' to ensure native binaries are included in publish output.
- Upgrade to Azure Functions V4 on .NET 6+ for better native library deployment support.

### Next Questions

- Has the reporter tried net6 Azure Functions as the maintainer requested?
- Is SkiaSharp.NativeAssets.Linux or SkiaSharp.NativeAssets.Linux.NoDependencies referenced directly in the Functions project?
- What is the full inner exception beneath the TypeInitializationException?

### Resolution Proposals

**Hypothesis:** The Azure Functions deployment is missing the Linux native binary for SkiaSharp. Adding SkiaSharp.NativeAssets.Linux.NoDependencies as a direct PackageReference in the Functions project should resolve the TypeInitializationException.

1. **Add Linux native assets package to the Functions project** — workaround, cost/xs, validated=yes
   - Add SkiaSharp.NativeAssets.Linux.NoDependencies as a direct PackageReference in the Azure Functions .csproj. On Linux, this package must be referenced in the executable project to ensure the native .so is deployed.

```csharp
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="2.*" />
```
2. **Upgrade to Azure Functions V4 on .NET 6+** — alternative, cost/m, validated=untested
   - Azure Functions V3 on netcore3.1 is end-of-life. Upgrading to V4 on .NET 6+ provides better native library deployment support and is the recommended upgrade path.

**Recommended proposal:** Add Linux native assets package to the Functions project

**Why:** Single-line fix that directly addresses the missing native binary. Works without upgrading the runtime or changing deployment tooling.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.75 (75%) |
| Reason | Maintainer asked for net6 results in 2022 but reporter never followed up. A clear workaround exists (add NativeAssets.Linux.NoDependencies), but confirmation of whether it resolves the issue is pending. |
| Suggested repro platform | linux |

### Missing Info

- Results of testing on net6 Azure Functions as requested by maintainer
- Whether SkiaSharp.NativeAssets.Linux.NoDependencies is referenced directly in the Functions project
- Full inner exception details below the TypeInitializationException

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply classification labels: type/question, area/libSkiaSharp.native, os/Linux, tenet/reliability | labels=type/question, area/libSkiaSharp.native, os/Linux, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Provide workaround for missing Linux native assets in Azure Functions deployment | — |

**Comment draft for `add-comment`:**

```markdown
The `TypeInitializationException` from `SKAbstractManagedWStream` happens when SkiaSharp cannot load its native library (`libSkiaSharp`). Azure Functions runs on **Linux**, and on Linux the native assets are **not** automatically included — you need to add them explicitly.

**Fix:** Add this to your Azure Functions `.csproj`:

```xml
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="2.*" />
```

This package provides the Linux native binary with no external system dependencies, which works well in Azure Functions containers.

**Important notes:**
- This `PackageReference` must be in the **application project** (the Azure Functions project itself), not in a class library
- Run `dotnet publish --runtime linux-x64` or use a proper publish profile to ensure the binary is included
- `netcore3.1` is end-of-life — consider upgrading to Azure Functions V4 on .NET 6+ for better native library support

This is the same underlying issue as #1001. Does adding the `NativeAssets.Linux.NoDependencies` package resolve it?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2053,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T11:44:30Z"
  },
  "summary": "Question about deploying SkiaSharp with QuestPDF on Azure Function V3 (netcore3.1) without Visual Studio Publish — TypeInitializationException from SKAbstractManagedWStream at runtime on Azure, indicating missing Linux native library.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.85
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.9
    },
    "platforms": [
      "os/Linux"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "SkiaSharp.SKAbstractManagedWStream' threw an exception\n   at SkiaSharp.SKAbstractManagedWStream..ctor(Boolean owns)\n   at SkiaSharp.SKDocument.CreatePdf(Stream stream, SKDocumentPdfMetadata metadata)\n   at QuestPDF.Drawing.DocumentGenerator.GeneratePdf(Stream stream, IDocument document)\n   at QuestPDF.Fluent.GenerateExtensions.GeneratePdf(IDocument document, Stream stream)"
      ],
      "relatedIssues": [
        1001
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "netcore3.1"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "netcore3.1 is end-of-life and Azure Functions V3 has been superseded by V4 on net6+."
    }
  },
  "analysis": {
    "summary": "The TypeInitializationException from SKAbstractManagedWStream indicates the native libSkiaSharp binary is not available at runtime on Azure Functions. Azure Functions runs on Linux; on Linux, native assets are NOT auto-included and require explicit addition of SkiaSharp.NativeAssets.Linux.NoDependencies in the application project. The static constructor of SKAbstractManagedWStream P/Invokes into libSkiaSharp via sk_managedwstream_set_procs — if the native library is absent, a DllNotFoundException is thrown and wrapped as TypeInitializationException.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKAbstractManagedWStream.cs",
        "finding": "The static constructor (lines 16-26) calls SkiaApi.sk_managedwstream_set_procs(delegates) which P/Invokes into libSkiaSharp. If the native library is not found or fails to load, a DllNotFoundException is thrown here, which the CLR wraps in a TypeInitializationException — exactly matching the reported exception.",
        "relevance": "direct",
        "lines": "16-26"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "sk_managedwstream_set_procs is resolved via dynamic symbol lookup (GetSymbol). This is the actual P/Invoke entry point into libSkiaSharp native binary. Confirms that failing to find libSkiaSharp.so on Linux would cause TypeInitializationException at first use.",
        "relevance": "direct",
        "lines": "17221-17237"
      }
    ],
    "keySignals": [
      {
        "text": "SkiaSharp.SKAbstractManagedWStream' threw an exception at SkiaSharp.SKAbstractManagedWStream..ctor(Boolean owns)",
        "source": "Issue body",
        "interpretation": "TypeInitializationException from static constructor — indicates native library P/Invoke fails during initialization. Strongly suggests libSkiaSharp native binary is not available in the Azure environment."
      },
      {
        "text": "When I try to execute the same code on Visual Studio it does work with no problem at all.",
        "source": "Issue body",
        "interpretation": "Classic 'works locally, fails in deployment' pattern for native library deployment. Locally, VS deploys native binaries automatically; Azure Functions deployment on Linux requires explicit packaging of SkiaSharp.NativeAssets.Linux.NoDependencies."
      },
      {
        "text": "I assume it's because System.Memory.dll is not updated.",
        "source": "Comment by reporter (brandonvqz)",
        "interpretation": "Reporter may be misattributing root cause. System.Memory version mismatch is unlikely to cause a TypeInitializationException at SkiaSharp initialization. The root cause is the missing Linux native binary."
      }
    ],
    "rationale": "Classified as type/question because the title explicitly uses [QUESTION] and the reporter asks 'is there any way to implement SkiaSharp with QuestPDF on Azure Function V3 without using the publish function from VS?'. The issue is a deployment/packaging question. Azure Functions runs on Linux; Linux native assets must be explicitly referenced (unlike Windows/macOS which are auto-included). The workaround is adding SkiaSharp.NativeAssets.Linux.NoDependencies in the application project. Maintainer already asked about net6 results in 2022 but reporter never responded, so needs-info is the appropriate action.",
    "workarounds": [
      "Add SkiaSharp.NativeAssets.Linux.NoDependencies as a direct PackageReference in the Azure Functions project (the executable project, not a library).",
      "Run 'dotnet publish --runtime linux-x64 --self-contained false' to ensure native binaries are included in publish output.",
      "Upgrade to Azure Functions V4 on .NET 6+ for better native library deployment support."
    ],
    "nextQuestions": [
      "Has the reporter tried net6 Azure Functions as the maintainer requested?",
      "Is SkiaSharp.NativeAssets.Linux or SkiaSharp.NativeAssets.Linux.NoDependencies referenced directly in the Functions project?",
      "What is the full inner exception beneath the TypeInitializationException?"
    ],
    "resolution": {
      "hypothesis": "The Azure Functions deployment is missing the Linux native binary for SkiaSharp. Adding SkiaSharp.NativeAssets.Linux.NoDependencies as a direct PackageReference in the Functions project should resolve the TypeInitializationException.",
      "proposals": [
        {
          "title": "Add Linux native assets package to the Functions project",
          "description": "Add SkiaSharp.NativeAssets.Linux.NoDependencies as a direct PackageReference in the Azure Functions .csproj. On Linux, this package must be referenced in the executable project to ensure the native .so is deployed.",
          "category": "workaround",
          "effort": "cost/xs",
          "validated": "yes",
          "codeSnippet": "<PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"2.*\" />"
        },
        {
          "title": "Upgrade to Azure Functions V4 on .NET 6+",
          "description": "Azure Functions V3 on netcore3.1 is end-of-life. Upgrading to V4 on .NET 6+ provides better native library deployment support and is the recommended upgrade path.",
          "category": "alternative",
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add Linux native assets package to the Functions project",
      "recommendedReason": "Single-line fix that directly addresses the missing native binary. Works without upgrading the runtime or changing deployment tooling."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.75,
      "reason": "Maintainer asked for net6 results in 2022 but reporter never followed up. A clear workaround exists (add NativeAssets.Linux.NoDependencies), but confirmation of whether it resolves the issue is pending.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Results of testing on net6 Azure Functions as requested by maintainer",
      "Whether SkiaSharp.NativeAssets.Linux.NoDependencies is referenced directly in the Functions project",
      "Full inner exception details below the TypeInitializationException"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply classification labels: type/question, area/libSkiaSharp.native, os/Linux, tenet/reliability",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/libSkiaSharp.native",
          "os/Linux",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Provide workaround for missing Linux native assets in Azure Functions deployment",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "The `TypeInitializationException` from `SKAbstractManagedWStream` happens when SkiaSharp cannot load its native library (`libSkiaSharp`). Azure Functions runs on **Linux**, and on Linux the native assets are **not** automatically included — you need to add them explicitly.\n\n**Fix:** Add this to your Azure Functions `.csproj`:\n\n```xml\n<PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"2.*\" />\n```\n\nThis package provides the Linux native binary with no external system dependencies, which works well in Azure Functions containers.\n\n**Important notes:**\n- This `PackageReference` must be in the **application project** (the Azure Functions project itself), not in a class library\n- Run `dotnet publish --runtime linux-x64` or use a proper publish profile to ensure the binary is included\n- `netcore3.1` is end-of-life — consider upgrading to Azure Functions V4 on .NET 6+ for better native library support\n\nThis is the same underlying issue as #1001. Does adding the `NativeAssets.Linux.NoDependencies` package resolve it?"
      }
    ]
  }
}
```

</details>
