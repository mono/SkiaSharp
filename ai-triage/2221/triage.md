# Issue Triage Report — #2221

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T10:59:44Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views.Blazor (0.90 (90%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** Blazor WASM release-mode deployments intermittently throw EntryPointNotFoundException for sk_paint_set_stroke_width due to the IL trimmer removing P/Invoke delegate entries from the SkiaSharp assembly.

**Analysis:** The SkiaSharp assembly is marked with [assembly: AssemblyMetadata("IsTrimmable", "True")] which allows the IL trimmer to remove unused code. The P/Invoke delegates in SkiaApi.generated.cs (using GetSymbol<T> delegate caching pattern) can be incorrectly eliminated by the trimmer because their usage paths may not be statically analyzable. The issue manifests intermittently in deployed Blazor WASM release builds but not locally, because the Blazor publishing pipeline enables trimming by default. The fix is to ship an ILLink.Descriptors.xml (or use [DynamicDependency] / [RequiresUnreferencedCode] attributes) to preserve P/Invoke entry points.

**Recommendations:** **needs-investigation** — Root cause is clear (missing ILLink descriptors) and a workaround exists, but the fix requires verifying whether newer versions have already addressed this and determining the correct trimming annotation strategy.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Blazor |
| Platforms | os/WASM |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability, tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Code snippets:**

```csharp
var paint = new SKPaint() { Color = SKColor.Parse("#ffffff"), IsStroke = true, StrokeWidth = 2.0f, StrokeCap = SKStrokeCap.Round };
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | exception |
| Error message | System.EntryPointNotFoundException: sk_paint_set_stroke_width at SkiaSharp.SKPaint.set_StrokeWidth(Single) |
| Repro quality | partial |
| Target frameworks | net6.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | No ILLink descriptor file exists in the binding directory as of the current codebase, and the assembly is marked IsTrimmable=True. |

## Analysis

### Technical Summary

The SkiaSharp assembly is marked with [assembly: AssemblyMetadata("IsTrimmable", "True")] which allows the IL trimmer to remove unused code. The P/Invoke delegates in SkiaApi.generated.cs (using GetSymbol<T> delegate caching pattern) can be incorrectly eliminated by the trimmer because their usage paths may not be statically analyzable. The issue manifests intermittently in deployed Blazor WASM release builds but not locally, because the Blazor publishing pipeline enables trimming by default. The fix is to ship an ILLink.Descriptors.xml (or use [DynamicDependency] / [RequiresUnreferencedCode] attributes) to preserve P/Invoke entry points.

### Rationale

Classified as type/bug in area/SkiaSharp.Views.Blazor because the SkiaSharp package opts into trimming via IsTrimmable=True but does not ship trim-safe annotations or an ILLink descriptor, causing the P/Invoke delegate infrastructure to be incorrectly eliminated in published Blazor WASM apps. The issue is reproducible with a provided workaround and the root cause is clear.

### Key Signals

- "Uncaught Error: System.EntryPointNotFoundException: sk_paint_set_stroke_width" — **issue body** (The native symbol lookup fails because the delegate entry was trimmed from the managed assembly.)
- "works locally in Debug and Release mode but intermittently when deployed to Azure App Service" — **issue body** (Local builds may not apply trimming with the same aggressiveness as the Blazor publish pipeline targeting WASM.)
- "Add file ILLink.Descriptors.xml to project root... This will stop the SkiaSharp library from being trimmed" — **issue body - workaround section** (Reporter confirmed the workaround of disabling trimming for SkiaSharp assemblies resolves the issue.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/Properties/SkiaSharpAssemblyInfo.cs` | — | direct | Assembly is marked [assembly: AssemblyMetadata("IsTrimmable", "True")] enabling the IL trimmer to process and remove members. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | direct | P/Invoke delegates use a delegate-caching GetSymbol<T> pattern (e.g., sk_paint_set_stroke_width_delegate). These delegates are stored as private static fields and may be trimmed if the trimmer does not trace all code paths calling them. |
| `binding/SkiaSharp/SKPaint.cs` | — | direct | StrokeWidth setter calls SkiaApi.sk_paint_set_stroke_width(Handle, value) which dispatches through the delegate-cached entry in SkiaApi.generated.cs. |

### Workarounds

- Add ILLink.Descriptors.xml to the project root preserving SkiaSharp and SkiaSharp.Views.Blazor assemblies, and reference it as an EmbeddedResource with LogicalName=ILLink.Descriptors.xml in the .csproj.

### Next Questions

- Has a TrimmerRootDescriptor or [DynamicDependency] approach been considered for the generated P/Invoke delegates?
- Does the issue reproduce on SkiaSharp 3.x (current), or was it addressed in later versions?

### Resolution Proposals

**Hypothesis:** Add an ILLink.Descriptors.xml (or equivalent TrimmerRootDescriptor) to the SkiaSharp NuGet package to prevent the IL trimmer from removing P/Invoke delegate fields and associated infrastructure.

1. **Ship ILLink.Descriptors.xml in SkiaSharp NuGet package** — fix, cost/s, validated=untested
   - Add an ILLink.Descriptors.xml to the SkiaSharp project that preserves all members of the SkiaApi class and related P/Invoke infrastructure so the IL trimmer does not remove them.
2. **Workaround: project-level ILLink.Descriptors.xml** — workaround, cost/xs, validated=yes
   - Users can add their own ILLink.Descriptors.xml preserving the SkiaSharp assemblies until an official fix ships.

**Recommended proposal:** Ship ILLink.Descriptors.xml in SkiaSharp NuGet package

**Why:** Shipping the fix in the package ensures all users are protected without requiring per-project workarounds.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | Root cause is clear (missing ILLink descriptors) and a workaround exists, but the fix requires verifying whether newer versions have already addressed this and determining the correct trimming annotation strategy. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/SkiaSharp.Views.Blazor, os/WASM, backend/OpenGL, tenet/reliability, tenet/compatibility | labels=type/bug, area/SkiaSharp.Views.Blazor, os/WASM, backend/OpenGL, tenet/reliability, tenet/compatibility |
| add-comment | medium | 0.85 (85%) | Acknowledge the bug, confirm the workaround, and outline the fix path. | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for reporting this and for including a workaround!

This is a known trimming issue: the SkiaSharp assembly is opted into IL trimming (`IsTrimmable=True`) but does not currently ship an `ILLink.Descriptors.xml` to protect its P/Invoke delegate infrastructure. When Blazor WASM publishes with trimming enabled, some delegate entries can be incorrectly removed, resulting in `EntryPointNotFoundException` at runtime.

**Workaround (confirmed):** Add an `ILLink.Descriptors.xml` to your project as described above to prevent SkiaSharp from being trimmed.

**Planned fix:** The correct long-term fix is to ship a trim descriptor or use `[DynamicDependency]` annotations inside the SkiaSharp package itself. We'll track this.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2221,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T10:59:44Z"
  },
  "summary": "Blazor WASM release-mode deployments intermittently throw EntryPointNotFoundException for sk_paint_set_stroke_width due to the IL trimmer removing P/Invoke delegate entries from the SkiaSharp assembly.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views.Blazor",
      "confidence": 0.9
    },
    "platforms": [
      "os/WASM"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/reliability",
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "System.EntryPointNotFoundException: sk_paint_set_stroke_width at SkiaSharp.SKPaint.set_StrokeWidth(Single)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0"
      ]
    },
    "reproEvidence": {
      "codeSnippets": [
        "var paint = new SKPaint() { Color = SKColor.Parse(\"#ffffff\"), IsStroke = true, StrokeWidth = 2.0f, StrokeCap = SKStrokeCap.Round };"
      ],
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "No ILLink descriptor file exists in the binding directory as of the current codebase, and the assembly is marked IsTrimmable=True."
    }
  },
  "analysis": {
    "summary": "The SkiaSharp assembly is marked with [assembly: AssemblyMetadata(\"IsTrimmable\", \"True\")] which allows the IL trimmer to remove unused code. The P/Invoke delegates in SkiaApi.generated.cs (using GetSymbol<T> delegate caching pattern) can be incorrectly eliminated by the trimmer because their usage paths may not be statically analyzable. The issue manifests intermittently in deployed Blazor WASM release builds but not locally, because the Blazor publishing pipeline enables trimming by default. The fix is to ship an ILLink.Descriptors.xml (or use [DynamicDependency] / [RequiresUnreferencedCode] attributes) to preserve P/Invoke entry points.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/Properties/SkiaSharpAssemblyInfo.cs",
        "finding": "Assembly is marked [assembly: AssemblyMetadata(\"IsTrimmable\", \"True\")] enabling the IL trimmer to process and remove members.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "P/Invoke delegates use a delegate-caching GetSymbol<T> pattern (e.g., sk_paint_set_stroke_width_delegate). These delegates are stored as private static fields and may be trimmed if the trimmer does not trace all code paths calling them.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "finding": "StrokeWidth setter calls SkiaApi.sk_paint_set_stroke_width(Handle, value) which dispatches through the delegate-cached entry in SkiaApi.generated.cs.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Uncaught Error: System.EntryPointNotFoundException: sk_paint_set_stroke_width",
        "source": "issue body",
        "interpretation": "The native symbol lookup fails because the delegate entry was trimmed from the managed assembly."
      },
      {
        "text": "works locally in Debug and Release mode but intermittently when deployed to Azure App Service",
        "source": "issue body",
        "interpretation": "Local builds may not apply trimming with the same aggressiveness as the Blazor publish pipeline targeting WASM."
      },
      {
        "text": "Add file ILLink.Descriptors.xml to project root... This will stop the SkiaSharp library from being trimmed",
        "source": "issue body - workaround section",
        "interpretation": "Reporter confirmed the workaround of disabling trimming for SkiaSharp assemblies resolves the issue."
      }
    ],
    "rationale": "Classified as type/bug in area/SkiaSharp.Views.Blazor because the SkiaSharp package opts into trimming via IsTrimmable=True but does not ship trim-safe annotations or an ILLink descriptor, causing the P/Invoke delegate infrastructure to be incorrectly eliminated in published Blazor WASM apps. The issue is reproducible with a provided workaround and the root cause is clear.",
    "workarounds": [
      "Add ILLink.Descriptors.xml to the project root preserving SkiaSharp and SkiaSharp.Views.Blazor assemblies, and reference it as an EmbeddedResource with LogicalName=ILLink.Descriptors.xml in the .csproj."
    ],
    "nextQuestions": [
      "Has a TrimmerRootDescriptor or [DynamicDependency] approach been considered for the generated P/Invoke delegates?",
      "Does the issue reproduce on SkiaSharp 3.x (current), or was it addressed in later versions?"
    ],
    "resolution": {
      "hypothesis": "Add an ILLink.Descriptors.xml (or equivalent TrimmerRootDescriptor) to the SkiaSharp NuGet package to prevent the IL trimmer from removing P/Invoke delegate fields and associated infrastructure.",
      "proposals": [
        {
          "title": "Ship ILLink.Descriptors.xml in SkiaSharp NuGet package",
          "category": "fix",
          "description": "Add an ILLink.Descriptors.xml to the SkiaSharp project that preserves all members of the SkiaApi class and related P/Invoke infrastructure so the IL trimmer does not remove them.",
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Workaround: project-level ILLink.Descriptors.xml",
          "category": "workaround",
          "description": "Users can add their own ILLink.Descriptors.xml preserving the SkiaSharp assemblies until an official fix ships.",
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Ship ILLink.Descriptors.xml in SkiaSharp NuGet package",
      "recommendedReason": "Shipping the fix in the package ensures all users are protected without requiring per-project workarounds."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Root cause is clear (missing ILLink descriptors) and a workaround exists, but the fix requires verifying whether newer versions have already addressed this and determining the correct trimming annotation strategy.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views.Blazor, os/WASM, backend/OpenGL, tenet/reliability, tenet/compatibility",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Blazor",
          "os/WASM",
          "backend/OpenGL",
          "tenet/reliability",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the bug, confirm the workaround, and outline the fix path.",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thank you for reporting this and for including a workaround!\n\nThis is a known trimming issue: the SkiaSharp assembly is opted into IL trimming (`IsTrimmable=True`) but does not currently ship an `ILLink.Descriptors.xml` to protect its P/Invoke delegate infrastructure. When Blazor WASM publishes with trimming enabled, some delegate entries can be incorrectly removed, resulting in `EntryPointNotFoundException` at runtime.\n\n**Workaround (confirmed):** Add an `ILLink.Descriptors.xml` to your project as described above to prevent SkiaSharp from being trimmed.\n\n**Planned fix:** The correct long-term fix is to ship a trim descriptor or use `[DynamicDependency]` annotations inside the SkiaSharp package itself. We'll track this."
      }
    ]
  }
}
```

</details>
