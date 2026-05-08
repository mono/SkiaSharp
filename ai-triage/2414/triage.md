# Issue Triage Report — #2414

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T12:39:41Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | close-as-fixed (0.82 (82%)) |

**Issue Summary:** Linker warning MT0182 is emitted when building a .NET MAUI app on Mac targeting MacCatalyst because SkiaSharp.Views.iOS.dll references the OpenGLES framework, which is not available on MacCatalyst.

**Analysis:** The linker warning arises because SkiaSharp.Views.iOS.dll is compiled for both iOS and MacCatalyst, but the OpenGLES P/Invoke code was not excluded from MacCatalyst builds in the 2.88.x series. The current 3.x codebase adds #if !__MACCATALYST__ / !MACCATALYST guards to SKGLView.cs, SKGLLayer.cs, and Gles.cs so the framework reference no longer appears in MacCatalyst builds.

**Recommendations:** **close-as-fixed** — Code investigation confirms MACCATALYST guards are present in the current 3.x codebase and community confirms no reproduction in 3.116.1. Issue only affects 2.88.x users who should upgrade.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/macOS |
| Backends | backend/OpenGL |
| Tenets | tenet/compatibility |
| Partner | partner/maui |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a .NET MAUI project targeting MacCatalyst (Mac)
2. Add a reference to SkiaSharp.Views.Maui.Controls 2.88.x
3. Build on Mac — observe MT0182 warning about OpenGLES not available on MacCatalyst

**Environment:** .NET MAUI on Mac (MacCatalyst), SkiaSharp 2.88.7–2.88.9. Not reproducible in SkiaSharp 3.116.1.

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2414#issuecomment-2612021860 — Community member confirms MT0182 not reproducible in 3.116.1 but reproducible in 2.88.7–2.88.9

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | build-error |
| Error message | ILLINK : warning MT0182: Not linking with the framework OpenGLES (referenced by a module reference in SkiaSharp.Views.iOS.dll) because it's not available on the current platform (MacCatalyst). |
| Repro quality | partial |
| Target frameworks | net8.0-maccatalyst, net8.0-ios |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.7, 2.88.8, 2.88.9, 3.116.1 |
| Worked in | 3.116.1 |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | Current code has #if !__MACCATALYST__ guards on all OpenGLES files. Community confirms no reproduction in 3.116.1. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.88 (88%) |
| Reason | Code investigation shows SKGLView.cs, SKGLLayer.cs, and Gles.cs all have MACCATALYST exclusion guards in the current codebase. Community member confirmed the warning is gone in 3.116.1. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | 3.x (exact version not determined, but not present in 3.116.1) |

## Analysis

### Technical Summary

The linker warning arises because SkiaSharp.Views.iOS.dll is compiled for both iOS and MacCatalyst, but the OpenGLES P/Invoke code was not excluded from MacCatalyst builds in the 2.88.x series. The current 3.x codebase adds #if !__MACCATALYST__ / !MACCATALYST guards to SKGLView.cs, SKGLLayer.cs, and Gles.cs so the framework reference no longer appears in MacCatalyst builds.

### Rationale

This is a real build warning caused by including OpenGLES-dependent code in MacCatalyst assemblies where the framework is unavailable. The fix (adding MACCATALYST preprocessor guards) is already present in the 3.x codebase. The issue is effectively resolved for users on 3.x; users still on 2.88.x should upgrade.

### Key Signals

- "warning MT0182: Not linking with the framework OpenGLES (referenced by a module reference in SkiaSharp.Views.iOS.dll) because it's not available on the current platform (MacCatalyst)." — **issue body** (Linker detects an OpenGLES module reference in the iOS DLL when building for MacCatalyst, where the framework does not exist.)
- "I think this is because I am including the code to use Open GL - even though I don't really use it. I probably should investigate things like this where we can exclude code for platforms that don't / can't use it." — **comment #1699721024 (maintainer mattleibow)** (Maintainer confirmed the root cause and noted a fix was needed.)
- "I cannot reproduce the warning using SkiaSharp 3.116.1 version. Could you please share what is the exact SkiaSharp version you use in the .NET MAUI app? For example, I can reproduce the warning in 2.88.7, 2.88.8, 2.88.9 versions" — **comment #2612021860** (Community confirms fix is present in 3.116.1, confirming the issue was resolved in the 3.x upgrade.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLView.cs` | 1 | direct | File is guarded with #if !__MACCATALYST__ at line 1, ensuring OpenGLES/GLKit imports are excluded from MacCatalyst builds in 3.x. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLLayer.cs` | 1 | direct | File is guarded with #if !__MACCATALYST__ at line 1, excluding EAGLContext / OpenGLES references from MacCatalyst assemblies in 3.x. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Shared/GlesInterop/Gles.cs` | 1 | direct | File is guarded with !MACCATALYST in the preprocessor condition, so the DllImport to /System/Library/Frameworks/OpenGLES.framework/OpenGLES is omitted on MacCatalyst in 3.x. |
| `source/SkiaSharp.Views/SkiaSharp.Views/SkiaSharp.Views.csproj` | 7-10 | related | Both -ios and -maccatalyst TFMs share the assembly name SkiaSharp.Views.iOS, confirming why the MacCatalyst build could emit an iOS-named DLL containing OpenGLES references in older versions. |

### Workarounds

- Upgrade to SkiaSharp 3.116.1 or later — the OpenGLES MACCATALYST guard is present in 3.x and the warning no longer appears.
- If staying on 2.88.x, the warning is informational only and does not prevent the app from running (the app works despite the warning per reporter).

### Next Questions

- Is there a 2.88.x maintenance branch that should receive a backport of the MACCATALYST guards?
- Was the danies8 user (January 2025 comment saying they cannot build) actually on 2.88.x? The comment says 'The app work before' — could be a regression with a 3.x version they did not specify.

### Resolution Proposals

**Hypothesis:** The fix was already applied in the 3.x series by adding #if !__MACCATALYST__ guards around the OpenGLES code in SKGLView.cs, SKGLLayer.cs, and Gles.cs.

1. **Upgrade to SkiaSharp 3.x** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Users experiencing this warning on 2.88.x should upgrade to SkiaSharp 3.116.1 or later. The MACCATALYST guards are already in place and the warning is confirmed absent in 3.116.1.

**Recommended proposal:** Upgrade to SkiaSharp 3.x

**Why:** The fix is already present in 3.x. Upgrading is the simplest path to resolution.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.82 (82%) |
| Reason | Code investigation confirms MACCATALYST guards are present in the current 3.x codebase and community confirms no reproduction in 3.116.1. Issue only affects 2.88.x users who should upgrade. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, views, macOS, OpenGL, compatibility, and MAUI partner labels | labels=type/bug, area/SkiaSharp.Views, os/macOS, backend/OpenGL, tenet/compatibility, partner/maui |
| add-comment | medium | 0.82 (82%) | Inform reporters that the issue is fixed in 3.x and recommend upgrading | — |
| close-issue | medium | 0.82 (82%) | Close as fixed in 3.x | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! This linker warning (MT0182 about OpenGLES on MacCatalyst) was caused by OpenGLES-dependent code in `SkiaSharp.Views.iOS.dll` not being properly excluded from MacCatalyst builds in the 2.88.x series.

This has been fixed in SkiaSharp 3.x by adding `#if !__MACCATALYST__` preprocessor guards to `SKGLView.cs`, `SKGLLayer.cs`, and `Gles.cs`. A community member has confirmed the warning is absent in **SkiaSharp 3.116.1**.

**Recommended action:** Upgrade to SkiaSharp 3.116.1 (or the latest 3.x release). The warning should disappear after the upgrade.

If you are unable to upgrade and remain on 2.88.x, the warning is informational only — the app should still run correctly despite the warning.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2414,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T12:39:41Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Linker warning MT0182 is emitted when building a .NET MAUI app on Mac targeting MacCatalyst because SkiaSharp.Views.iOS.dll references the OpenGLES framework, which is not available on MacCatalyst.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.9
    },
    "platforms": [
      "os/macOS"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/compatibility"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "low",
      "regressionClaimed": false,
      "errorType": "build-error",
      "errorMessage": "ILLINK : warning MT0182: Not linking with the framework OpenGLES (referenced by a module reference in SkiaSharp.Views.iOS.dll) because it's not available on the current platform (MacCatalyst).",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0-maccatalyst",
        "net8.0-ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET MAUI project targeting MacCatalyst (Mac)",
        "Add a reference to SkiaSharp.Views.Maui.Controls 2.88.x",
        "Build on Mac — observe MT0182 warning about OpenGLES not available on MacCatalyst"
      ],
      "environmentDetails": ".NET MAUI on Mac (MacCatalyst), SkiaSharp 2.88.7–2.88.9. Not reproducible in SkiaSharp 3.116.1.",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2414#issuecomment-2612021860",
          "description": "Community member confirms MT0182 not reproducible in 3.116.1 but reproducible in 2.88.7–2.88.9"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.7",
        "2.88.8",
        "2.88.9",
        "3.116.1"
      ],
      "workedIn": "3.116.1",
      "currentRelevance": "unlikely",
      "relevanceReason": "Current code has #if !__MACCATALYST__ guards on all OpenGLES files. Community confirms no reproduction in 3.116.1."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.88,
      "reason": "Code investigation shows SKGLView.cs, SKGLLayer.cs, and Gles.cs all have MACCATALYST exclusion guards in the current codebase. Community member confirmed the warning is gone in 3.116.1.",
      "fixedInVersion": "3.x (exact version not determined, but not present in 3.116.1)"
    }
  },
  "analysis": {
    "summary": "The linker warning arises because SkiaSharp.Views.iOS.dll is compiled for both iOS and MacCatalyst, but the OpenGLES P/Invoke code was not excluded from MacCatalyst builds in the 2.88.x series. The current 3.x codebase adds #if !__MACCATALYST__ / !MACCATALYST guards to SKGLView.cs, SKGLLayer.cs, and Gles.cs so the framework reference no longer appears in MacCatalyst builds.",
    "rationale": "This is a real build warning caused by including OpenGLES-dependent code in MacCatalyst assemblies where the framework is unavailable. The fix (adding MACCATALYST preprocessor guards) is already present in the 3.x codebase. The issue is effectively resolved for users on 3.x; users still on 2.88.x should upgrade.",
    "keySignals": [
      {
        "text": "warning MT0182: Not linking with the framework OpenGLES (referenced by a module reference in SkiaSharp.Views.iOS.dll) because it's not available on the current platform (MacCatalyst).",
        "source": "issue body",
        "interpretation": "Linker detects an OpenGLES module reference in the iOS DLL when building for MacCatalyst, where the framework does not exist."
      },
      {
        "text": "I think this is because I am including the code to use Open GL - even though I don't really use it. I probably should investigate things like this where we can exclude code for platforms that don't / can't use it.",
        "source": "comment #1699721024 (maintainer mattleibow)",
        "interpretation": "Maintainer confirmed the root cause and noted a fix was needed."
      },
      {
        "text": "I cannot reproduce the warning using SkiaSharp 3.116.1 version. Could you please share what is the exact SkiaSharp version you use in the .NET MAUI app? For example, I can reproduce the warning in 2.88.7, 2.88.8, 2.88.9 versions",
        "source": "comment #2612021860",
        "interpretation": "Community confirms fix is present in 3.116.1, confirming the issue was resolved in the 3.x upgrade."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLView.cs",
        "lines": "1",
        "finding": "File is guarded with #if !__MACCATALYST__ at line 1, ensuring OpenGLES/GLKit imports are excluded from MacCatalyst builds in 3.x.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLLayer.cs",
        "lines": "1",
        "finding": "File is guarded with #if !__MACCATALYST__ at line 1, excluding EAGLContext / OpenGLES references from MacCatalyst assemblies in 3.x.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Shared/GlesInterop/Gles.cs",
        "lines": "1",
        "finding": "File is guarded with !MACCATALYST in the preprocessor condition, so the DllImport to /System/Library/Frameworks/OpenGLES.framework/OpenGLES is omitted on MacCatalyst in 3.x.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/SkiaSharp.Views.csproj",
        "lines": "7-10",
        "finding": "Both -ios and -maccatalyst TFMs share the assembly name SkiaSharp.Views.iOS, confirming why the MacCatalyst build could emit an iOS-named DLL containing OpenGLES references in older versions.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Upgrade to SkiaSharp 3.116.1 or later — the OpenGLES MACCATALYST guard is present in 3.x and the warning no longer appears.",
      "If staying on 2.88.x, the warning is informational only and does not prevent the app from running (the app works despite the warning per reporter)."
    ],
    "nextQuestions": [
      "Is there a 2.88.x maintenance branch that should receive a backport of the MACCATALYST guards?",
      "Was the danies8 user (January 2025 comment saying they cannot build) actually on 2.88.x? The comment says 'The app work before' — could be a regression with a 3.x version they did not specify."
    ],
    "resolution": {
      "hypothesis": "The fix was already applied in the 3.x series by adding #if !__MACCATALYST__ guards around the OpenGLES code in SKGLView.cs, SKGLLayer.cs, and Gles.cs.",
      "proposals": [
        {
          "title": "Upgrade to SkiaSharp 3.x",
          "description": "Users experiencing this warning on 2.88.x should upgrade to SkiaSharp 3.116.1 or later. The MACCATALYST guards are already in place and the warning is confirmed absent in 3.116.1.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Upgrade to SkiaSharp 3.x",
      "recommendedReason": "The fix is already present in 3.x. Upgrading is the simplest path to resolution."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.82,
      "reason": "Code investigation confirms MACCATALYST guards are present in the current 3.x codebase and community confirms no reproduction in 3.116.1. Issue only affects 2.88.x users who should upgrade.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views, macOS, OpenGL, compatibility, and MAUI partner labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/macOS",
          "backend/OpenGL",
          "tenet/compatibility",
          "partner/maui"
        ]
      },
      {
        "type": "add-comment",
        "description": "Inform reporters that the issue is fixed in 3.x and recommend upgrading",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the report! This linker warning (MT0182 about OpenGLES on MacCatalyst) was caused by OpenGLES-dependent code in `SkiaSharp.Views.iOS.dll` not being properly excluded from MacCatalyst builds in the 2.88.x series.\n\nThis has been fixed in SkiaSharp 3.x by adding `#if !__MACCATALYST__` preprocessor guards to `SKGLView.cs`, `SKGLLayer.cs`, and `Gles.cs`. A community member has confirmed the warning is absent in **SkiaSharp 3.116.1**.\n\n**Recommended action:** Upgrade to SkiaSharp 3.116.1 (or the latest 3.x release). The warning should disappear after the upgrade.\n\nIf you are unable to upgrade and remain on 2.88.x, the warning is informational only — the app should still run correctly despite the warning."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed in 3.x",
        "risk": "medium",
        "confidence": 0.82,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
