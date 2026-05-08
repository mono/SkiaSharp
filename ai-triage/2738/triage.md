# Issue Triage Report — #2738

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-27T22:46:24Z |
| Type | type/feature-request (0.90 (90%)) |
| Area | area/libSkiaSharp.native (0.92 (92%)) |
| Suggested action | keep-open (0.82 (82%)) |

**Issue Summary:** Reporter requests that SkiaSharp automatically include SkiaSharp.NativeAssets.Linux as a dependency so that .NET 8 apps running on Linux/Docker do not require manual package addition.

**Analysis:** SkiaSharp intentionally does not auto-include any Linux NativeAssets package because there are multiple Linux variants (SkiaSharp.NativeAssets.Linux requires fontconfig; SkiaSharp.NativeAssets.Linux.NoDependencies is for containers/minimal images), and non-platform TFMs like net8.0 or netstandard2.1 give no signal about the deployment target. The reporter's workaround — manually adding SkiaSharp.NativeAssets.Linux — is the current documented approach. A comment from April 2025 notes the same issue affects HarfBuzzSharp on Windows in a similar scenario.

**Recommendations:** **keep-open** — Valid feature request to reduce Linux deployment friction. The design decision not to auto-include Linux NativeAssets is intentional due to multiple variants, but the request warrants design discussion. Related issues #2440 and #2653 confirm this is a recurring pain point.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/libSkiaSharp.native |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/feature-request |

## Evidence

### Reproduction

1. Create a .NET 8 project targeting netstandard2.1 or net8.0
2. Add SkiaSharp NuGet package
3. Run on Linux/Docker container
4. Observe that native library is missing — no Linux NativeAssets are auto-included

**Environment:** .NET 8, Docker, Linux, netstandard2.1

**Related issues:** #2440, #2653

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SkiaSharp.csproj still excludes Linux from auto-include conditions; the design has not changed. |

## Analysis

### Technical Summary

SkiaSharp intentionally does not auto-include any Linux NativeAssets package because there are multiple Linux variants (SkiaSharp.NativeAssets.Linux requires fontconfig; SkiaSharp.NativeAssets.Linux.NoDependencies is for containers/minimal images), and non-platform TFMs like net8.0 or netstandard2.1 give no signal about the deployment target. The reporter's workaround — manually adding SkiaSharp.NativeAssets.Linux — is the current documented approach. A comment from April 2025 notes the same issue affects HarfBuzzSharp on Windows in a similar scenario.

### Rationale

Classified as type/feature-request because the behavior is by-design (Linux NativeAssets are intentionally not auto-included), the reporter correctly self-diagnosed and resolved it, and the request to change this requires a design decision about which Linux variant to recommend. The area is libSkiaSharp.native because it is a native asset packaging question. The os/Linux and tenet/compatibility labels apply because the missing auto-include only manifests on Linux deployments and represents a compatibility friction point for new users.

### Key Signals

- "When I added nuget to my project that I developed with .net 8 and ran it on Docker, an error was received. The dependencies coming from .net standard 2.1 were not at a sufficient level." — **issue body** (Reporter hit the well-known missing Linux NativeAssets issue when deploying a net8.0/netstandard2.1 app to Docker/Linux.)
- "I solved the problem by adding the package named SkiaSharp.NativeAssets.Linux to the dependencies." — **issue body** (Self-resolved with the documented workaround; the feature request is to remove this manual step.)
- "I ran into this in relation to https://github.com/ScottPlot/ScottPlot/issues/4930 with HarfBuzzSharp on Windows" — **comment by jeroenjanssen-cpp** (Similar discovery friction exists for HarfBuzzSharp on Windows in containers, though Windows NativeAssets are typically auto-included for Win32 TFMs.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaSharp.csproj` | 19-25 | direct | ProjectReference conditions auto-include Win32 and macOS for non-platform TFMs (condition: `!$(TargetFramework.Contains('-'))`), but Linux is absent — no condition auto-includes any Linux NativeAssets variant. This confirms the behavior is intentional. |
| `documentation/dev/packages.md` | — | direct | Explicitly documents that SkiaSharp.NativeAssets.Linux and SkiaSharp.NativeAssets.Linux.NoDependencies must be added manually. Explains that multiple Linux variants exist (glibc+fontconfig vs NoDependencies), and that for containers NoDependencies is recommended. The design rationale is that .NET has no Linux-specific TFM that would allow NuGet to resolve the right variant automatically. |
| `documentation/dev/packages.md` | — | related | Linux Package Selection Guide table maps deployment scenarios to packages: standard Linux server → NativeAssets.Linux; Docker/Alpine containers → NativeAssets.Linux.NoDependencies. This multi-variant choice is why auto-include is not feasible without additional user signal. |

### Workarounds

- Add `<PackageReference Include="SkiaSharp.NativeAssets.Linux" />` to the executable/application project (not library projects).
- For Docker/minimal containers, prefer `SkiaSharp.NativeAssets.Linux.NoDependencies` to avoid fontconfig dependency.
- For Alpine containers, use `SkiaSharp.NativeAssets.Linux.NoDependencies` which ships `linux-musl-*` variants.

### Next Questions

- Is auto-including SkiaSharp.NativeAssets.Linux.NoDependencies for non-platform TFMs (net8.0, netstandard2.1) feasible without breaking existing deployments?
- Could a NuGet build property or MSBuild opt-in be added to auto-select the Linux variant?
- Does HarfBuzzSharp have the same auto-include gap on Linux (separate from the Windows comment in the thread)?

### Resolution Proposals

**Hypothesis:** Auto-including Linux NativeAssets is feasible but requires choosing between the standard (fontconfig) and NoDependencies variant. A reasonable default for containers would be NoDependencies, but server apps need fontconfig for system fonts.

1. **Document the manual step prominently** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - Add a prominent note in the README and NuGet package description that Linux/Docker requires manually adding SkiaSharp.NativeAssets.Linux or NoDependencies. This helps discoverability without breaking existing users.
2. **Add opt-in MSBuild property to auto-include Linux NativeAssets** — fix, confidence 0.70 (70%), cost/m, validated=untested
   - Introduce a SkiaSharpLinuxNativeAssets MSBuild property that users can set to 'Linux' or 'NoDependencies' to have it auto-included. This avoids a forced choice while reducing friction for informed users.
3. **Auto-include NoDependencies for non-platform TFMs** — fix, confidence 0.50 (50%), cost/s, validated=untested
   - Change SkiaSharp.csproj to auto-include SkiaSharp.NativeAssets.Linux.NoDependencies for non-platform TFMs (the same condition used for Win32 and macOS). This would auto-include for net8.0, netstandard2.1, etc. Risk: could conflict with existing manual NativeAssets.Linux references.

**Recommended proposal:** Document the manual step prominently

**Why:** Lowest risk and immediately actionable. The multi-variant Linux packaging makes automatic selection inherently ambiguous, so improved documentation is more reliable than a forced default.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.82 (82%) |
| Reason | Valid feature request to reduce Linux deployment friction. The design decision not to auto-include Linux NativeAssets is intentional due to multiple variants, but the request warrants design discussion. Related issues #2440 and #2653 confirm this is a recurring pain point. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply feature-request, native area, linux, and compatibility tenet labels | labels=type/feature-request, area/libSkiaSharp.native, os/Linux, tenet/compatibility |
| add-comment | medium | 0.88 (88%) | Explain the design reason for not auto-including and provide the correct workarounds including NoDependencies recommendation for Docker | — |
| link-related | low | 0.85 (85%) | Cross-reference related Linux/Docker deployment issue #2653 | linkedIssue=#2653 |
| link-related | low | 0.75 (75%) | Cross-reference related Windows/Docker deployment issue #2440 | linkedIssue=#2440 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! This is a known friction point for Linux/Docker deployments.

SkiaSharp intentionally does **not** auto-include a Linux NativeAssets package because there are two variants with different trade-offs:

| Package | Use when |
|---------|----------|
| `SkiaSharp.NativeAssets.Linux` | Standard Linux server/desktop (requires `fontconfig` for system fonts) |
| `SkiaSharp.NativeAssets.Linux.NoDependencies` | Docker/Alpine/minimal containers (zero system library dependencies) |

For your Docker scenario, the recommended package is `SkiaSharp.NativeAssets.Linux.NoDependencies` (not the regular `.Linux`):

```xml
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="x.x.x" />
```

> ⚠️ Add this to your **application/executable project** (not a shared library project), so the native binary is correctly deployed to your output directory.

For fontconfig-dependent scenarios (system font enumeration), use `SkiaSharp.NativeAssets.Linux` and ensure `libfontconfig.so.1` is installed in your container.

We are tracking the design question of whether to add a more discoverable opt-in mechanism. In the meantime, the manual `PackageReference` is the supported approach.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2738,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-27T22:46:24Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "Reporter requests that SkiaSharp automatically include SkiaSharp.NativeAssets.Linux as a dependency so that .NET 8 apps running on Linux/Docker do not require manual package addition.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.9
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.92
    },
    "platforms": [
      "os/Linux"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET 8 project targeting netstandard2.1 or net8.0",
        "Add SkiaSharp NuGet package",
        "Run on Linux/Docker container",
        "Observe that native library is missing — no Linux NativeAssets are auto-included"
      ],
      "environmentDetails": ".NET 8, Docker, Linux, netstandard2.1",
      "relatedIssues": [
        2440,
        2653
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "The SkiaSharp.csproj still excludes Linux from auto-include conditions; the design has not changed."
    }
  },
  "analysis": {
    "summary": "SkiaSharp intentionally does not auto-include any Linux NativeAssets package because there are multiple Linux variants (SkiaSharp.NativeAssets.Linux requires fontconfig; SkiaSharp.NativeAssets.Linux.NoDependencies is for containers/minimal images), and non-platform TFMs like net8.0 or netstandard2.1 give no signal about the deployment target. The reporter's workaround — manually adding SkiaSharp.NativeAssets.Linux — is the current documented approach. A comment from April 2025 notes the same issue affects HarfBuzzSharp on Windows in a similar scenario.",
    "rationale": "Classified as type/feature-request because the behavior is by-design (Linux NativeAssets are intentionally not auto-included), the reporter correctly self-diagnosed and resolved it, and the request to change this requires a design decision about which Linux variant to recommend. The area is libSkiaSharp.native because it is a native asset packaging question. The os/Linux and tenet/compatibility labels apply because the missing auto-include only manifests on Linux deployments and represents a compatibility friction point for new users.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaSharp.csproj",
        "lines": "19-25",
        "finding": "ProjectReference conditions auto-include Win32 and macOS for non-platform TFMs (condition: `!$(TargetFramework.Contains('-'))`), but Linux is absent — no condition auto-includes any Linux NativeAssets variant. This confirms the behavior is intentional.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "finding": "Explicitly documents that SkiaSharp.NativeAssets.Linux and SkiaSharp.NativeAssets.Linux.NoDependencies must be added manually. Explains that multiple Linux variants exist (glibc+fontconfig vs NoDependencies), and that for containers NoDependencies is recommended. The design rationale is that .NET has no Linux-specific TFM that would allow NuGet to resolve the right variant automatically.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "finding": "Linux Package Selection Guide table maps deployment scenarios to packages: standard Linux server → NativeAssets.Linux; Docker/Alpine containers → NativeAssets.Linux.NoDependencies. This multi-variant choice is why auto-include is not feasible without additional user signal.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "When I added nuget to my project that I developed with .net 8 and ran it on Docker, an error was received. The dependencies coming from .net standard 2.1 were not at a sufficient level.",
        "source": "issue body",
        "interpretation": "Reporter hit the well-known missing Linux NativeAssets issue when deploying a net8.0/netstandard2.1 app to Docker/Linux."
      },
      {
        "text": "I solved the problem by adding the package named SkiaSharp.NativeAssets.Linux to the dependencies.",
        "source": "issue body",
        "interpretation": "Self-resolved with the documented workaround; the feature request is to remove this manual step."
      },
      {
        "text": "I ran into this in relation to https://github.com/ScottPlot/ScottPlot/issues/4930 with HarfBuzzSharp on Windows",
        "source": "comment by jeroenjanssen-cpp",
        "interpretation": "Similar discovery friction exists for HarfBuzzSharp on Windows in containers, though Windows NativeAssets are typically auto-included for Win32 TFMs."
      }
    ],
    "workarounds": [
      "Add `<PackageReference Include=\"SkiaSharp.NativeAssets.Linux\" />` to the executable/application project (not library projects).",
      "For Docker/minimal containers, prefer `SkiaSharp.NativeAssets.Linux.NoDependencies` to avoid fontconfig dependency.",
      "For Alpine containers, use `SkiaSharp.NativeAssets.Linux.NoDependencies` which ships `linux-musl-*` variants."
    ],
    "nextQuestions": [
      "Is auto-including SkiaSharp.NativeAssets.Linux.NoDependencies for non-platform TFMs (net8.0, netstandard2.1) feasible without breaking existing deployments?",
      "Could a NuGet build property or MSBuild opt-in be added to auto-select the Linux variant?",
      "Does HarfBuzzSharp have the same auto-include gap on Linux (separate from the Windows comment in the thread)?"
    ],
    "resolution": {
      "hypothesis": "Auto-including Linux NativeAssets is feasible but requires choosing between the standard (fontconfig) and NoDependencies variant. A reasonable default for containers would be NoDependencies, but server apps need fontconfig for system fonts.",
      "proposals": [
        {
          "title": "Document the manual step prominently",
          "description": "Add a prominent note in the README and NuGet package description that Linux/Docker requires manually adding SkiaSharp.NativeAssets.Linux or NoDependencies. This helps discoverability without breaking existing users.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Add opt-in MSBuild property to auto-include Linux NativeAssets",
          "description": "Introduce a SkiaSharpLinuxNativeAssets MSBuild property that users can set to 'Linux' or 'NoDependencies' to have it auto-included. This avoids a forced choice while reducing friction for informed users.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Auto-include NoDependencies for non-platform TFMs",
          "description": "Change SkiaSharp.csproj to auto-include SkiaSharp.NativeAssets.Linux.NoDependencies for non-platform TFMs (the same condition used for Win32 and macOS). This would auto-include for net8.0, netstandard2.1, etc. Risk: could conflict with existing manual NativeAssets.Linux references.",
          "category": "fix",
          "confidence": 0.5,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Document the manual step prominently",
      "recommendedReason": "Lowest risk and immediately actionable. The multi-variant Linux packaging makes automatic selection inherently ambiguous, so improved documentation is more reliable than a forced default."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.82,
      "reason": "Valid feature request to reduce Linux deployment friction. The design decision not to auto-include Linux NativeAssets is intentional due to multiple variants, but the request warrants design discussion. Related issues #2440 and #2653 confirm this is a recurring pain point.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, native area, linux, and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/feature-request",
          "area/libSkiaSharp.native",
          "os/Linux",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain the design reason for not auto-including and provide the correct workarounds including NoDependencies recommendation for Docker",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the report! This is a known friction point for Linux/Docker deployments.\n\nSkiaSharp intentionally does **not** auto-include a Linux NativeAssets package because there are two variants with different trade-offs:\n\n| Package | Use when |\n|---------|----------|\n| `SkiaSharp.NativeAssets.Linux` | Standard Linux server/desktop (requires `fontconfig` for system fonts) |\n| `SkiaSharp.NativeAssets.Linux.NoDependencies` | Docker/Alpine/minimal containers (zero system library dependencies) |\n\nFor your Docker scenario, the recommended package is `SkiaSharp.NativeAssets.Linux.NoDependencies` (not the regular `.Linux`):\n\n```xml\n<PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"x.x.x\" />\n```\n\n> ⚠️ Add this to your **application/executable project** (not a shared library project), so the native binary is correctly deployed to your output directory.\n\nFor fontconfig-dependent scenarios (system font enumeration), use `SkiaSharp.NativeAssets.Linux` and ensure `libfontconfig.so.1` is installed in your container.\n\nWe are tracking the design question of whether to add a more discoverable opt-in mechanism. In the meantime, the manual `PackageReference` is the supported approach."
      },
      {
        "type": "link-related",
        "description": "Cross-reference related Linux/Docker deployment issue #2653",
        "risk": "low",
        "confidence": 0.85,
        "linkedIssue": 2653
      },
      {
        "type": "link-related",
        "description": "Cross-reference related Windows/Docker deployment issue #2440",
        "risk": "low",
        "confidence": 0.75,
        "linkedIssue": 2440
      }
    ]
  }
}
```

</details>
