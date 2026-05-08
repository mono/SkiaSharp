# Issue Triage Report — #2238

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T12:05:00Z |
| Type | type/bug (0.75 (75%)) |
| Area | area/SkiaSharp.Views.Blazor (0.90 (90%)) |
| Suggested action | needs-info (0.82 (82%)) |

**Issue Summary:** Blazor WASM published app fails with integrity check error for SkiaSharp.Views.Blazor.dll, but works in debug mode.

**Analysis:** Browser integrity check fails for SkiaSharp.Views.Blazor.dll after publish (but not in debug). This is a known class of Blazor WASM publish issue where ILLink or other publish-time transforms modify assemblies after blazor.boot.json integrity hashes are computed. Could be external to SkiaSharp, but may also be specific to how SkiaSharp.Views.Blazor embeds JavaScript interop assets.

**Recommendations:** **needs-info** — No reproduction steps provided, uses preview tooling. Need confirmation with stable tools and newer SkiaSharp versions before investigating further.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Blazor |
| Platforms | os/WASM |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | other |
| Error message | Failed to find a valid digest in the 'integrity' attribute for resource 'http://localhost:5580/_framework/SkiaSharp.Views.Blazor.dll' with computed SHA-256 integrity 'mnnfdAEuQYbTe9JKRu1rVRILNd37rnaKpjZJE8056T4='. The resource has been blocked. |
| Repro quality | none |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Issue was filed using SkiaSharp 2.88.1 and VS 2022 Preview 17.4.0 Preview 1.0. Newer releases exist (2.88.x, 3.x). Unknown if resolved. |

## Analysis

### Technical Summary

Browser integrity check fails for SkiaSharp.Views.Blazor.dll after publish (but not in debug). This is a known class of Blazor WASM publish issue where ILLink or other publish-time transforms modify assemblies after blazor.boot.json integrity hashes are computed. Could be external to SkiaSharp, but may also be specific to how SkiaSharp.Views.Blazor embeds JavaScript interop assets.

### Rationale

Classified as type/bug in area/SkiaSharp.Views.Blazor because the integrity error specifically names SkiaSharp.Views.Blazor.dll, though the root cause may be in the .NET Blazor publish toolchain (ILLink modifying the assembly post-hash). Severity is medium since debug mode works and only publish is affected. Suggested action is needs-info because there are no repro steps, no project file, and the reporter used a preview tooling version — we need to confirm whether this reproduces on stable tooling or newer SkiaSharp versions.

### Key Signals

- "Failed to find a valid digest in the 'integrity' attribute for resource 'http://localhost:5580/_framework/SkiaSharp.Views.Blazor.dll'" — **issue body** (Browser's subresource integrity check fails — the actual SHA-256 of the served DLL does not match the hash in blazor.boot.json. This can happen when publish-time transforms modify assemblies after hashing.)
- "It works fine in debugmode!" — **issue body** (Debug builds skip ILLink/trimming and integrity manifest generation. Release publish enables these, which can cause hash mismatches — a known Blazor WASM SDK issue.)
- "Microsoft Visual Studio Community 2022 (64-bit) - Preview Version 17.4.0 Preview 1.0" — **issue body** (Reporter uses a preview IDE version from 2022, making it harder to confirm whether this is a stable SkiaSharp issue or a preview tooling bug.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/Internal/JSModuleInterop.cs` | — | related | JSModuleInterop imports JavaScript modules dynamically via JSHost.ImportAsync (NET7+) or JS import(). The JS assets embedded in SkiaSharp.Views.Blazor.dll are loaded at runtime. No evidence of post-build modification of the DLL. |

### Workarounds

- Add <BlazorCacheBootResources>false</BlazorCacheBootResources> to the project file to disable integrity checking (workaround, not a fix).

### Next Questions

- Does this reproduce with SkiaSharp 2.88.x or 3.x?
- Does it reproduce with stable VS 2022 (non-preview)?
- Is this a Blazor Server or Blazor WebAssembly project?
- Can you share the .csproj file and publish profile?
- Does it reproduce without GPU.razor (i.e., with just SKCanvasView)?

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.82 (82%) |
| Reason | No reproduction steps provided, uses preview tooling. Need confirmation with stable tools and newer SkiaSharp versions before investigating further. |
| Suggested repro platform | linux |

### Missing Info

- SkiaSharp version tested (newer than 2.88.1?)
- Stable vs preview Visual Studio version
- Project type (Blazor WebAssembly standalone, hosted, etc.)
- .csproj and publish profile
- Whether issue reproduces without GPU-accelerated canvas

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply classification labels | labels=type/bug, area/SkiaSharp.Views.Blazor, os/WASM, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Request missing reproduction info from reporter | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report!

The integrity check error for `SkiaSharp.Views.Blazor.dll` is a known class of Blazor WASM issue that can occur when publish-time tools (like ILLink/trimmer) modify the assembly after `blazor.boot.json` integrity hashes are computed.

To help us investigate, could you provide:

1. **SkiaSharp version** — does this reproduce with a newer version (e.g., 2.88.8 or 3.x)?
2. **Tooling** — can you try with a stable (non-preview) VS 2022 release?
3. **Project type** — is this a standalone Blazor WebAssembly project, or hosted?
4. **Your `.csproj`** — please share the project file and publish profile.
5. **Minimal repro** — does it happen with just `SKCanvasView` (no GPU canvas)?

As a temporary workaround, you can disable integrity checking by adding this to your `.csproj`:

```xml
<PropertyGroup>
  <BlazorCacheBootResources>false</BlazorCacheBootResources>
</PropertyGroup>
```

Note: this disables the browser's integrity check for all framework files and is not recommended for production.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2238,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T12:05:00Z"
  },
  "summary": "Blazor WASM published app fails with integrity check error for SkiaSharp.Views.Blazor.dll, but works in debug mode.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.75
    },
    "area": {
      "value": "area/SkiaSharp.Views.Blazor",
      "confidence": 0.9
    },
    "platforms": [
      "os/WASM"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "other",
      "errorMessage": "Failed to find a valid digest in the 'integrity' attribute for resource 'http://localhost:5580/_framework/SkiaSharp.Views.Blazor.dll' with computed SHA-256 integrity 'mnnfdAEuQYbTe9JKRu1rVRILNd37rnaKpjZJE8056T4='. The resource has been blocked.",
      "reproQuality": "none",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "repoLinks": [],
      "attachments": [],
      "codeSnippets": [],
      "stepsToReproduce": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.1"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Issue was filed using SkiaSharp 2.88.1 and VS 2022 Preview 17.4.0 Preview 1.0. Newer releases exist (2.88.x, 3.x). Unknown if resolved."
    }
  },
  "analysis": {
    "summary": "Browser integrity check fails for SkiaSharp.Views.Blazor.dll after publish (but not in debug). This is a known class of Blazor WASM publish issue where ILLink or other publish-time transforms modify assemblies after blazor.boot.json integrity hashes are computed. Could be external to SkiaSharp, but may also be specific to how SkiaSharp.Views.Blazor embeds JavaScript interop assets.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/Internal/JSModuleInterop.cs",
        "finding": "JSModuleInterop imports JavaScript modules dynamically via JSHost.ImportAsync (NET7+) or JS import(). The JS assets embedded in SkiaSharp.Views.Blazor.dll are loaded at runtime. No evidence of post-build modification of the DLL.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Failed to find a valid digest in the 'integrity' attribute for resource 'http://localhost:5580/_framework/SkiaSharp.Views.Blazor.dll'",
        "source": "issue body",
        "interpretation": "Browser's subresource integrity check fails — the actual SHA-256 of the served DLL does not match the hash in blazor.boot.json. This can happen when publish-time transforms modify assemblies after hashing."
      },
      {
        "text": "It works fine in debugmode!",
        "source": "issue body",
        "interpretation": "Debug builds skip ILLink/trimming and integrity manifest generation. Release publish enables these, which can cause hash mismatches — a known Blazor WASM SDK issue."
      },
      {
        "text": "Microsoft Visual Studio Community 2022 (64-bit) - Preview Version 17.4.0 Preview 1.0",
        "source": "issue body",
        "interpretation": "Reporter uses a preview IDE version from 2022, making it harder to confirm whether this is a stable SkiaSharp issue or a preview tooling bug."
      }
    ],
    "rationale": "Classified as type/bug in area/SkiaSharp.Views.Blazor because the integrity error specifically names SkiaSharp.Views.Blazor.dll, though the root cause may be in the .NET Blazor publish toolchain (ILLink modifying the assembly post-hash). Severity is medium since debug mode works and only publish is affected. Suggested action is needs-info because there are no repro steps, no project file, and the reporter used a preview tooling version — we need to confirm whether this reproduces on stable tooling or newer SkiaSharp versions.",
    "nextQuestions": [
      "Does this reproduce with SkiaSharp 2.88.x or 3.x?",
      "Does it reproduce with stable VS 2022 (non-preview)?",
      "Is this a Blazor Server or Blazor WebAssembly project?",
      "Can you share the .csproj file and publish profile?",
      "Does it reproduce without GPU.razor (i.e., with just SKCanvasView)?"
    ],
    "workarounds": [
      "Add <BlazorCacheBootResources>false</BlazorCacheBootResources> to the project file to disable integrity checking (workaround, not a fix)."
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.82,
      "reason": "No reproduction steps provided, uses preview tooling. Need confirmation with stable tools and newer SkiaSharp versions before investigating further.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "SkiaSharp version tested (newer than 2.88.1?)",
      "Stable vs preview Visual Studio version",
      "Project type (Blazor WebAssembly standalone, hosted, etc.)",
      ".csproj and publish profile",
      "Whether issue reproduces without GPU-accelerated canvas"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply classification labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Blazor",
          "os/WASM",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request missing reproduction info from reporter",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the report!\n\nThe integrity check error for `SkiaSharp.Views.Blazor.dll` is a known class of Blazor WASM issue that can occur when publish-time tools (like ILLink/trimmer) modify the assembly after `blazor.boot.json` integrity hashes are computed.\n\nTo help us investigate, could you provide:\n\n1. **SkiaSharp version** — does this reproduce with a newer version (e.g., 2.88.8 or 3.x)?\n2. **Tooling** — can you try with a stable (non-preview) VS 2022 release?\n3. **Project type** — is this a standalone Blazor WebAssembly project, or hosted?\n4. **Your `.csproj`** — please share the project file and publish profile.\n5. **Minimal repro** — does it happen with just `SKCanvasView` (no GPU canvas)?\n\nAs a temporary workaround, you can disable integrity checking by adding this to your `.csproj`:\n\n```xml\n<PropertyGroup>\n  <BlazorCacheBootResources>false</BlazorCacheBootResources>\n</PropertyGroup>\n```\n\nNote: this disables the browser's integrity check for all framework files and is not recommended for production."
      }
    ]
  }
}
```

</details>
