# Issue Triage Report — #2746

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T22:18:00Z |
| Type | type/feature-request (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | keep-open (0.88 (88%)) |

**Issue Summary:** Feature request to restore UWP (uap10.0) support in SkiaSharp.Views 3.x, which was dropped in the 3.0 platform reduction; community has since made partial progress with CPU rendering under .NET 9 UWP but GPU (EGL surface creation) remains unresolved.

**Analysis:** SkiaSharp 3.0 explicitly removed UWP (uap10.0) from its supported platform list as part of its 'Platform Reduction' in favour of modern .NET-only targets. The SkiaSharp.Views.WinUI project targets net-windows (Windows App SDK) only, not the legacy uap10.0 TFM. Maintainer (mattleibow) has acknowledged the request and suggested a path: reuse win32 DLLs (which already pass UWP app certification per dotMorten) and add a uap10.0 TFM to the Views NuGet. Community effort in Nov 2025 achieved CPU rendering under .NET 9 UWP but failed on GPU due to EGL_BAD_SURFACE when creating an EGL surface with both wasdk=true and wasdk=false ANGLE builds.

**Recommendations:** **keep-open** — Valid feature request with maintainer engagement, status/up-for-grabs label, and active community progress. CPU rendering under .NET 9 UWP is partially working; GPU (EGL) is the remaining blocker. Worth tracking until a contributor delivers a complete implementation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Universal-UWP |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | status/up-for-grabs, type/feature-request |

## Evidence

### Reproduction

1. Create a UWP app targeting uap10.0.17763
2. Add NuGet reference to SkiaSharp.Views 3.0.0-preview.x
3. Observe NU1202: Package SkiaSharp.Views 3.0.0-preview is not compatible with uap10.0.17763

**Environment:** UWP uap10.0.17763, win10-x86-aot, SkiaSharp.Views 3.0.0-preview.1.8

**Repository links:**
- https://github.com/mono/SkiaSharp/blob/main/changelogs/SkiaSharp/3.0.0/SkiaSharp.humanreadable.md — SkiaSharp 3.0 changelog listing Platform Reduction (UWP removed)
- https://devblogs.microsoft.com/ifdef-windows/preview-uwp-support-for-dotnet-9-native-aot/ — Microsoft announcement of .NET 9 UWP preview (referenced in comments)

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.0.0-preview.1.8, 2.x |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SkiaSharp.Views 3.x still targets net7.0-windows / net8.0-windows only (WinUI 3 / Windows App SDK), with no uap10.0 TFM. UWP is intentionally excluded from the 3.x platform matrix. |

## Analysis

### Technical Summary

SkiaSharp 3.0 explicitly removed UWP (uap10.0) from its supported platform list as part of its 'Platform Reduction' in favour of modern .NET-only targets. The SkiaSharp.Views.WinUI project targets net-windows (Windows App SDK) only, not the legacy uap10.0 TFM. Maintainer (mattleibow) has acknowledged the request and suggested a path: reuse win32 DLLs (which already pass UWP app certification per dotMorten) and add a uap10.0 TFM to the Views NuGet. Community effort in Nov 2025 achieved CPU rendering under .NET 9 UWP but failed on GPU due to EGL_BAD_SURFACE when creating an EGL surface with both wasdk=true and wasdk=false ANGLE builds.

### Rationale

Issue is a valid feature request: UWP was intentionally excluded from SkiaSharp 3.x and users with UWP projects that cannot migrate to WinUI 3 need this. The maintainer has indicated feasibility and the issue has a status/up-for-grabs label signalling it is welcome as a community contribution. Recent activity shows real progress toward a solution, making this worth keeping open and tracking.

### Key Signals

- "NU1202: Package SkiaSharp.Views 3.0.0-preview.1.8 is not compatible with uap10.0.17763" — **issue body** (Confirms SkiaSharp.Views 3.x ships no uap10.0 TFM — UWP support was intentionally dropped.)
- "we no longer build Skia specifically for uwp but use standard win32 dlls in our uwp apps and they still pass app certification" — **comment by dotMorten** (Key feasibility signal: win32 native assets can be reused for UWP, no separate native build needed — reduces implementation scope.)
- "So, this just means we need to add the UWP TFM back into the views nuget and we might be safe." — **comment by mattleibow** (Maintainer confirmation that the fix is primarily adding a uap10.0 TFM to the Views package and wiring in win32 DLLs.)
- "We have got this to work for CPU but not GPU. For GPU we get an exception Failed to create EGL surface ... EGL_BAD_SURFACE" — **comment by charlesroddie (Nov 2025)** (Recent community progress: CPU rendering works under .NET 9 UWP; GPU blocked by ANGLE/EGL surface creation failure — open investigation item.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `changelogs/SkiaSharp/3.0.0/SkiaSharp.humanreadable.md` | — | direct | Explicitly lists UWP as removed under 'Platform Reduction': 3.x supports .NET Standard 2.0+, .NET Framework 4.6.2+ and .NET 7+ platform targets — uap10.0 is not included. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SkiaSharp.Views.WinUI.csproj` | — | direct | Targets $(WindowsTargetFrameworks) (net-windows via WinUI 3 / Windows App SDK), includes Microsoft.WindowsAppSDK 1.4 reference — confirms no uap10.0 TFM exists in the views project. |
| `source/SkiaSharp.Build.props` | — | related | WindowsTargetFrameworks is set to $(TFMCurrent)-windows$(TPVWindowsCurrent) only when $(IsWindows) — no uap TFM variant defined anywhere in the build properties. |
| `build.cake` | — | related | References SkiaSharp.NativeAssets.UWP and SkiaSharp.Views.NativeAssets.UWP only at version 2.80.0 (legacy upgrade paths), confirming the 3.x build system does not produce UWP packages. |

### Workarounds

- Stay on SkiaSharp 2.x which still supports UWP (SkiaSharp.NativeAssets.UWP 2.88.x)
- For CPU-only rendering: community has demonstrated .NET 9 UWP CPU path works — see comments on this issue for partial steps
- Migrate to WinUI 3 / Windows App SDK and use SkiaSharp.Views.WinUI 3.x

### Next Questions

- What ANGLE build flags are needed to create a valid EGL surface under UWP (.NET 9)?
- Does the CPU path work with the current 3.x SkiaSharp.Views source or only with custom builds?
- Is there a plan to officially support .NET 9 UWP in a future SkiaSharp release?

### Resolution Proposals

**Hypothesis:** UWP Views support requires: (1) add uap10.0 / net9.0-windows TFM to SkiaSharp.Views, (2) wire in win32 native DLLs as runtime assets, (3) fix ANGLE/EGL surface creation for GPU path under UWP.

1. **Add UWP TFM to SkiaSharp.Views NuGet using win32 DLLs** — fix, confidence 0.70 (70%), cost/l, validated=untested
   - Per dotMorten's guidance: place native libs in runtimes/win-xXX/native, add uap10.0 (or net9.0-windows) TFM to the managed assembly with multi-targeting, run through App Certification Kit.
2. **Remain on SkiaSharp 2.x until UWP support lands in 3.x** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - SkiaSharp 2.88.x continues to support UWP. Users blocked by WinUI 3 migration can pin to 2.x in the interim.

**Recommended proposal:** Add UWP TFM to SkiaSharp.Views NuGet using win32 DLLs

**Why:** Aligns with maintainer guidance, community has already proven feasibility for CPU path; GPU path is the remaining open investigation.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.88 (88%) |
| Reason | Valid feature request with maintainer engagement, status/up-for-grabs label, and active community progress. CPU rendering under .NET 9 UWP is partially working; GPU (EGL) is the remaining blocker. Worth tracking until a contributor delivers a complete implementation. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply feature-request, views, and UWP platform labels | labels=type/feature-request, area/SkiaSharp.Views, os/Windows-Universal-UWP, tenet/compatibility |
| add-comment | medium | 0.80 (80%) | Summarise current state for community: CPU path partially working, GPU/EGL blocked, link to relevant comments | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the update on the .NET 9 UWP progress! To summarise where things stand:

- **CPU rendering** works under .NET 9 UWP (confirmed by recent community testing)
- **GPU rendering** is blocked by `EGL_BAD_SURFACE` when creating an EGL surface — this occurs with both `wasdk=true` and `wasdk=false` ANGLE builds

The original guidance from @dotMorten still stands: win32 DLLs work in UWP and pass app cert. The implementation path is:
1. Add a UWP/net9-windows TFM to `SkiaSharp.Views`
2. Wire in win32 native libs under `runtimes/win-xXX/native/`
3. Resolve the ANGLE/EGL surface creation issue for GPU acceleration

This issue is tagged **up-for-grabs**. If you can share more details on the EGL failure (e.g. which surface type / attributes you're passing, UWP CoreWindow vs SwapChainPanel), that would help unblock the GPU path.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2746,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T22:18:00Z",
    "currentLabels": [
      "status/up-for-grabs",
      "type/feature-request"
    ]
  },
  "summary": "Feature request to restore UWP (uap10.0) support in SkiaSharp.Views 3.x, which was dropped in the 3.0 platform reduction; community has since made partial progress with CPU rendering under .NET 9 UWP but GPU (EGL surface creation) remains unresolved.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Universal-UWP"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a UWP app targeting uap10.0.17763",
        "Add NuGet reference to SkiaSharp.Views 3.0.0-preview.x",
        "Observe NU1202: Package SkiaSharp.Views 3.0.0-preview is not compatible with uap10.0.17763"
      ],
      "environmentDetails": "UWP uap10.0.17763, win10-x86-aot, SkiaSharp.Views 3.0.0-preview.1.8",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/blob/main/changelogs/SkiaSharp/3.0.0/SkiaSharp.humanreadable.md",
          "description": "SkiaSharp 3.0 changelog listing Platform Reduction (UWP removed)"
        },
        {
          "url": "https://devblogs.microsoft.com/ifdef-windows/preview-uwp-support-for-dotnet-9-native-aot/",
          "description": "Microsoft announcement of .NET 9 UWP preview (referenced in comments)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.0.0-preview.1.8",
        "2.x"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SkiaSharp.Views 3.x still targets net7.0-windows / net8.0-windows only (WinUI 3 / Windows App SDK), with no uap10.0 TFM. UWP is intentionally excluded from the 3.x platform matrix."
    }
  },
  "analysis": {
    "summary": "SkiaSharp 3.0 explicitly removed UWP (uap10.0) from its supported platform list as part of its 'Platform Reduction' in favour of modern .NET-only targets. The SkiaSharp.Views.WinUI project targets net-windows (Windows App SDK) only, not the legacy uap10.0 TFM. Maintainer (mattleibow) has acknowledged the request and suggested a path: reuse win32 DLLs (which already pass UWP app certification per dotMorten) and add a uap10.0 TFM to the Views NuGet. Community effort in Nov 2025 achieved CPU rendering under .NET 9 UWP but failed on GPU due to EGL_BAD_SURFACE when creating an EGL surface with both wasdk=true and wasdk=false ANGLE builds.",
    "rationale": "Issue is a valid feature request: UWP was intentionally excluded from SkiaSharp 3.x and users with UWP projects that cannot migrate to WinUI 3 need this. The maintainer has indicated feasibility and the issue has a status/up-for-grabs label signalling it is welcome as a community contribution. Recent activity shows real progress toward a solution, making this worth keeping open and tracking.",
    "codeInvestigation": [
      {
        "file": "changelogs/SkiaSharp/3.0.0/SkiaSharp.humanreadable.md",
        "finding": "Explicitly lists UWP as removed under 'Platform Reduction': 3.x supports .NET Standard 2.0+, .NET Framework 4.6.2+ and .NET 7+ platform targets — uap10.0 is not included.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SkiaSharp.Views.WinUI.csproj",
        "finding": "Targets $(WindowsTargetFrameworks) (net-windows via WinUI 3 / Windows App SDK), includes Microsoft.WindowsAppSDK 1.4 reference — confirms no uap10.0 TFM exists in the views project.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Build.props",
        "finding": "WindowsTargetFrameworks is set to $(TFMCurrent)-windows$(TPVWindowsCurrent) only when $(IsWindows) — no uap TFM variant defined anywhere in the build properties.",
        "relevance": "related"
      },
      {
        "file": "build.cake",
        "finding": "References SkiaSharp.NativeAssets.UWP and SkiaSharp.Views.NativeAssets.UWP only at version 2.80.0 (legacy upgrade paths), confirming the 3.x build system does not produce UWP packages.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "NU1202: Package SkiaSharp.Views 3.0.0-preview.1.8 is not compatible with uap10.0.17763",
        "source": "issue body",
        "interpretation": "Confirms SkiaSharp.Views 3.x ships no uap10.0 TFM — UWP support was intentionally dropped."
      },
      {
        "text": "we no longer build Skia specifically for uwp but use standard win32 dlls in our uwp apps and they still pass app certification",
        "source": "comment by dotMorten",
        "interpretation": "Key feasibility signal: win32 native assets can be reused for UWP, no separate native build needed — reduces implementation scope."
      },
      {
        "text": "So, this just means we need to add the UWP TFM back into the views nuget and we might be safe.",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer confirmation that the fix is primarily adding a uap10.0 TFM to the Views package and wiring in win32 DLLs."
      },
      {
        "text": "We have got this to work for CPU but not GPU. For GPU we get an exception Failed to create EGL surface ... EGL_BAD_SURFACE",
        "source": "comment by charlesroddie (Nov 2025)",
        "interpretation": "Recent community progress: CPU rendering works under .NET 9 UWP; GPU blocked by ANGLE/EGL surface creation failure — open investigation item."
      }
    ],
    "workarounds": [
      "Stay on SkiaSharp 2.x which still supports UWP (SkiaSharp.NativeAssets.UWP 2.88.x)",
      "For CPU-only rendering: community has demonstrated .NET 9 UWP CPU path works — see comments on this issue for partial steps",
      "Migrate to WinUI 3 / Windows App SDK and use SkiaSharp.Views.WinUI 3.x"
    ],
    "nextQuestions": [
      "What ANGLE build flags are needed to create a valid EGL surface under UWP (.NET 9)?",
      "Does the CPU path work with the current 3.x SkiaSharp.Views source or only with custom builds?",
      "Is there a plan to officially support .NET 9 UWP in a future SkiaSharp release?"
    ],
    "resolution": {
      "hypothesis": "UWP Views support requires: (1) add uap10.0 / net9.0-windows TFM to SkiaSharp.Views, (2) wire in win32 native DLLs as runtime assets, (3) fix ANGLE/EGL surface creation for GPU path under UWP.",
      "proposals": [
        {
          "title": "Add UWP TFM to SkiaSharp.Views NuGet using win32 DLLs",
          "description": "Per dotMorten's guidance: place native libs in runtimes/win-xXX/native, add uap10.0 (or net9.0-windows) TFM to the managed assembly with multi-targeting, run through App Certification Kit.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/l",
          "validated": "untested"
        },
        {
          "title": "Remain on SkiaSharp 2.x until UWP support lands in 3.x",
          "description": "SkiaSharp 2.88.x continues to support UWP. Users blocked by WinUI 3 migration can pin to 2.x in the interim.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add UWP TFM to SkiaSharp.Views NuGet using win32 DLLs",
      "recommendedReason": "Aligns with maintainer guidance, community has already proven feasibility for CPU path; GPU path is the remaining open investigation."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.88,
      "reason": "Valid feature request with maintainer engagement, status/up-for-grabs label, and active community progress. CPU rendering under .NET 9 UWP is partially working; GPU (EGL) is the remaining blocker. Worth tracking until a contributor delivers a complete implementation.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, views, and UWP platform labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp.Views",
          "os/Windows-Universal-UWP",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Summarise current state for community: CPU path partially working, GPU/EGL blocked, link to relevant comments",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the update on the .NET 9 UWP progress! To summarise where things stand:\n\n- **CPU rendering** works under .NET 9 UWP (confirmed by recent community testing)\n- **GPU rendering** is blocked by `EGL_BAD_SURFACE` when creating an EGL surface — this occurs with both `wasdk=true` and `wasdk=false` ANGLE builds\n\nThe original guidance from @dotMorten still stands: win32 DLLs work in UWP and pass app cert. The implementation path is:\n1. Add a UWP/net9-windows TFM to `SkiaSharp.Views`\n2. Wire in win32 native libs under `runtimes/win-xXX/native/`\n3. Resolve the ANGLE/EGL surface creation issue for GPU acceleration\n\nThis issue is tagged **up-for-grabs**. If you can share more details on the EGL failure (e.g. which surface type / attributes you're passing, UWP CoreWindow vs SwapChainPanel), that would help unblock the GPU path."
      }
    ]
  }
}
```

</details>
