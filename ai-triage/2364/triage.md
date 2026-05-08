# Issue Triage Report — #2364

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T23:36:22Z |
| Type | type/question (0.88 (88%)) |
| Area | area/Build (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.82 (82%)) |

**Issue Summary:** Reporter asks how to build only the core SkiaSharp NuGet package without the Views/Controls projects, since the Views build fails with NETSDK1147 (missing iOS workload) on their Windows machine.

**Analysis:** Reporter wants to compile and build only the core SkiaSharp library (binding/SkiaSharp/SkiaSharp.csproj) without the Views/Controls libraries on Windows. The solution filter used by build.cake includes Views projects that require iOS workloads, which the reporter does not have installed. Building the binding project directly with dotnet build bypasses this problem.

**Recommendations:** **close-as-not-a-bug** — This is a usage question about building a subset of the repo. The build system works as designed. A clear workaround exists: build binding/SkiaSharp/SkiaSharp.csproj directly.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/Build |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Clone SkiaSharp source
2. Run build.cake on Windows without iOS workload installed
3. Observe NETSDK1147 failure for SkiaSharp.Views.csproj targeting net6.0-ios

**Environment:** .NET SDK 7.0.102 / 7.0.200-preview.22628.1, Windows, no iOS workload installed

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 7.0.102, 7.0.200-preview.22628.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The solution filter still includes Views projects that require mobile workloads. |

## Analysis

### Technical Summary

Reporter wants to compile and build only the core SkiaSharp library (binding/SkiaSharp/SkiaSharp.csproj) without the Views/Controls libraries on Windows. The solution filter used by build.cake includes Views projects that require iOS workloads, which the reporter does not have installed. Building the binding project directly with dotnet build bypasses this problem.

### Rationale

The issue is a usage question about the build system. No broken behavior — the build system works as designed. The reporter simply lacks the iOS workload required to build the Views multi-targeted project. Directly building binding/SkiaSharp/SkiaSharp.csproj is a valid workaround.

### Key Signals

- "Is there a way to only build the skiasharp nuget without all the extra stuff?" — **issue title/body** (Direct question about build subset support — this is a usage question.)
- "error NETSDK1147: To build this project, the following workloads must be installed: ios" — **issue body** (Missing iOS workload prevents building the full solution filter. Not a bug in SkiaSharp — expected when workload is absent.)
- "We were looking to update our custom SkiaSharp to the latest code but cant compile any of it any longer because of these views" — **issue body** (Reporter wants to fork/customize core SkiaSharp and build just that portion.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharpSource.Windows.slnf` | — | direct | Solution filter includes all binding projects (Views, NativeAssets, etc.). No subset filter exists for core-only builds. The Views project requires the iOS workload on Windows. |
| `binding/SkiaSharp/SkiaSharp.csproj` | — | direct | Core SkiaSharp csproj file exists and can be built independently with 'dotnet build binding/SkiaSharp/SkiaSharp.csproj' without requiring Views or mobile workloads. |
| `build.cake` | 199-206 | related | The 'libs' task uses RunDotNetBuild on the platform .slnf file, which includes all projects including Views. No per-project or subset target exists in build.cake. |

### Workarounds

- Build the core project directly: dotnet build binding/SkiaSharp/SkiaSharp.csproj
- Build just the HarfBuzzSharp binding: dotnet build binding/HarfBuzzSharp/HarfBuzzSharp.csproj
- Install the required iOS workload with: dotnet workload install ios macos android

### Resolution Proposals

**Hypothesis:** The reporter needs to build only the core SkiaSharp C# binding without mobile/Views projects. Directly building the csproj is the straightforward workaround.

1. **Build core csproj directly** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Run `dotnet build binding/SkiaSharp/SkiaSharp.csproj` directly instead of using build.cake or the solution filter. This builds the core SkiaSharp library without requiring Views or mobile workloads.
2. **Install required workloads** — alternative, confidence 0.85 (85%), cost/s, validated=untested
   - Install the required workloads via `dotnet workload install ios android macos` to allow full build via build.cake.

**Recommended proposal:** Build core csproj directly

**Why:** Simplest path — reporter only needs the core SkiaSharp package, not Views. Building the csproj directly requires no additional tooling.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.82 (82%) |
| Reason | This is a usage question about building a subset of the repo. The build system works as designed. A clear workaround exists: build binding/SkiaSharp/SkiaSharp.csproj directly. |
| Suggested repro platform | — |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply question, build, and Windows labels | labels=type/question, area/Build, os/Windows-Classic |
| add-comment | high | 0.82 (82%) | Answer the question with the workaround for building core SkiaSharp only | — |
| close-issue | medium | 0.80 (80%) | Close as answered | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for raising this!

Yes, you can build just the core SkiaSharp library without the Views/Controls projects. Instead of using `build.cake` (which builds the full solution filter including Views), build the core project directly:

```bash
dotnet build binding/SkiaSharp/SkiaSharp.csproj
```

This will compile the core SkiaSharp assembly without requiring iOS, Android, or any other mobile workloads.

If you also need HarfBuzzSharp:

```bash
dotnet build binding/HarfBuzzSharp/HarfBuzzSharp.csproj
```

The NETSDK1147 error you're seeing is expected when the iOS workload isn't installed — it's only required for the `SkiaSharp.Views` multi-targeted project.

If you do need the full build at some point, you can install all required workloads with:
```bash
dotnet workload install ios android macos
```
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2364,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T23:36:22Z"
  },
  "summary": "Reporter asks how to build only the core SkiaSharp NuGet package without the Views/Controls projects, since the Views build fails with NETSDK1147 (missing iOS workload) on their Windows machine.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.88
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Clone SkiaSharp source",
        "Run build.cake on Windows without iOS workload installed",
        "Observe NETSDK1147 failure for SkiaSharp.Views.csproj targeting net6.0-ios"
      ],
      "environmentDetails": ".NET SDK 7.0.102 / 7.0.200-preview.22628.1, Windows, no iOS workload installed",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "7.0.102",
        "7.0.200-preview.22628.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The solution filter still includes Views projects that require mobile workloads."
    }
  },
  "analysis": {
    "summary": "Reporter wants to compile and build only the core SkiaSharp library (binding/SkiaSharp/SkiaSharp.csproj) without the Views/Controls libraries on Windows. The solution filter used by build.cake includes Views projects that require iOS workloads, which the reporter does not have installed. Building the binding project directly with dotnet build bypasses this problem.",
    "rationale": "The issue is a usage question about the build system. No broken behavior — the build system works as designed. The reporter simply lacks the iOS workload required to build the Views multi-targeted project. Directly building binding/SkiaSharp/SkiaSharp.csproj is a valid workaround.",
    "keySignals": [
      {
        "text": "Is there a way to only build the skiasharp nuget without all the extra stuff?",
        "source": "issue title/body",
        "interpretation": "Direct question about build subset support — this is a usage question."
      },
      {
        "text": "error NETSDK1147: To build this project, the following workloads must be installed: ios",
        "source": "issue body",
        "interpretation": "Missing iOS workload prevents building the full solution filter. Not a bug in SkiaSharp — expected when workload is absent."
      },
      {
        "text": "We were looking to update our custom SkiaSharp to the latest code but cant compile any of it any longer because of these views",
        "source": "issue body",
        "interpretation": "Reporter wants to fork/customize core SkiaSharp and build just that portion."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharpSource.Windows.slnf",
        "finding": "Solution filter includes all binding projects (Views, NativeAssets, etc.). No subset filter exists for core-only builds. The Views project requires the iOS workload on Windows.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaSharp.csproj",
        "finding": "Core SkiaSharp csproj file exists and can be built independently with 'dotnet build binding/SkiaSharp/SkiaSharp.csproj' without requiring Views or mobile workloads.",
        "relevance": "direct"
      },
      {
        "file": "build.cake",
        "lines": "199-206",
        "finding": "The 'libs' task uses RunDotNetBuild on the platform .slnf file, which includes all projects including Views. No per-project or subset target exists in build.cake.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Build the core project directly: dotnet build binding/SkiaSharp/SkiaSharp.csproj",
      "Build just the HarfBuzzSharp binding: dotnet build binding/HarfBuzzSharp/HarfBuzzSharp.csproj",
      "Install the required iOS workload with: dotnet workload install ios macos android"
    ],
    "resolution": {
      "hypothesis": "The reporter needs to build only the core SkiaSharp C# binding without mobile/Views projects. Directly building the csproj is the straightforward workaround.",
      "proposals": [
        {
          "title": "Build core csproj directly",
          "description": "Run `dotnet build binding/SkiaSharp/SkiaSharp.csproj` directly instead of using build.cake or the solution filter. This builds the core SkiaSharp library without requiring Views or mobile workloads.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Install required workloads",
          "description": "Install the required workloads via `dotnet workload install ios android macos` to allow full build via build.cake.",
          "category": "alternative",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Build core csproj directly",
      "recommendedReason": "Simplest path — reporter only needs the core SkiaSharp package, not Views. Building the csproj directly requires no additional tooling."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.82,
      "reason": "This is a usage question about building a subset of the repo. The build system works as designed. A clear workaround exists: build binding/SkiaSharp/SkiaSharp.csproj directly."
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, build, and Windows labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/Build",
          "os/Windows-Classic"
        ]
      },
      {
        "type": "add-comment",
        "description": "Answer the question with the workaround for building core SkiaSharp only",
        "risk": "high",
        "confidence": 0.82,
        "comment": "Thanks for raising this!\n\nYes, you can build just the core SkiaSharp library without the Views/Controls projects. Instead of using `build.cake` (which builds the full solution filter including Views), build the core project directly:\n\n```bash\ndotnet build binding/SkiaSharp/SkiaSharp.csproj\n```\n\nThis will compile the core SkiaSharp assembly without requiring iOS, Android, or any other mobile workloads.\n\nIf you also need HarfBuzzSharp:\n\n```bash\ndotnet build binding/HarfBuzzSharp/HarfBuzzSharp.csproj\n```\n\nThe NETSDK1147 error you're seeing is expected when the iOS workload isn't installed — it's only required for the `SkiaSharp.Views` multi-targeted project.\n\nIf you do need the full build at some point, you can install all required workloads with:\n```bash\ndotnet workload install ios android macos\n```"
      },
      {
        "type": "close-issue",
        "description": "Close as answered",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
