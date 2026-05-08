# Issue Triage Report — #3144

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T04:26:00Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | close-as-duplicate (0.82 (82%)) |

**Issue Summary:** Building a .NET 8 MAUI app for MacCatalyst produces linker warning MT0182: Not linking with framework OpenGLES (referenced by SkiaSharp.Views.iOS.dll) because OpenGLES is unavailable on MacCatalyst, preventing the app from building.

**Analysis:** The MT0182 linker warning is generated because SkiaSharp.Views.iOS.dll (used for both ios and maccatalyst targets) contains a P/Invoke DllImport referencing the OpenGLES framework. The Gles.cs shared file uses `!MACCATALYST` as its compile guard while the iOS-platform files (SKGLView.cs, SKGLLayer.cs) use `!__MACCATALYST__`; if either symbol definition differs at package build time, Gles.cs may be included in the maccatalyst assembly, producing an OpenGLES module reference that the Apple linker rejects.

**Recommendations:** **close-as-duplicate** — This is the same MT0182 / OpenGLES / MacCatalyst warning as #2414 which is still open and tracks the underlying root cause. The reporter confirmed reproduction without Telerik controls using plain SkiaSharp.Views.Maui.Controls, identical to #2414 findings. Upgrading to 3.116.1 resolves the reporter's immediate symptom.

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

1. Create a .NET 8 MAUI project on macOS
2. Add SkiaSharp.Views.Maui.Controls and Compatibility references
3. Call .UseSkiaSharp() in MauiProgram.cs
4. Build/deploy the app targeting MacCatalyst

**Environment:** macOS, .NET 8, SkiaSharp 3.116.0, Visual Studio Code, reproduces without Telerik controls

**Related issues:** #2414

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2414 — Original tracking issue for same MT0182 OpenGLES/MacCatalyst warning, still open

**Attachments:**
- 8b82398c-4c31-4ec5-b53f-2b4a2e5c5ba3_archive-net8.zip — https://github.com/user-attachments/files/18503349/8b82398c-4c31-4ec5-b53f-2b4a2e5c5ba3_archive-net8.zip — Sample MAUI app reproducing the MT0182 warning with Telerik controls

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | build-error |
| Error message | warning MT0182: Not linking with the framework OpenGLES (referenced by a module reference in SkiaSharp.Views.iOS.dll) because it's not available on the current platform (MacCatalyst). |
| Repro quality | partial |
| Target frameworks | net8.0-maccatalyst |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | unlikely |
| Relevance reason | A Telerik contributor confirmed the warning is absent in SkiaSharp 3.116.1; may have been introduced in 3.116.0 and fixed in 3.116.1. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.65 (65%) |
| Reason | Reporter states the app worked on 2.88.9 and broke on 3.116.0. However other data points suggest 2.88.x also has the same warning; the version relationship is unclear. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.72 (72%) |
| Reason | Telerik MAUI contributor confirmed in May 2025 that upgrading to SkiaSharp 3.116.1 eliminates the warning. The parent issue #2414 was reproduced on 2.88.7-9 but could not be reproduced on 3.116.1. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

The MT0182 linker warning is generated because SkiaSharp.Views.iOS.dll (used for both ios and maccatalyst targets) contains a P/Invoke DllImport referencing the OpenGLES framework. The Gles.cs shared file uses `!MACCATALYST` as its compile guard while the iOS-platform files (SKGLView.cs, SKGLLayer.cs) use `!__MACCATALYST__`; if either symbol definition differs at package build time, Gles.cs may be included in the maccatalyst assembly, producing an OpenGLES module reference that the Apple linker rejects.

### Rationale

This is a duplicate/near-duplicate of #2414 which tracks the identical warning and root cause. The maintainer commented in #2414 that the OpenGL code is included in the iOS-shared assembly even though it is not needed on MacCatalyst. Upgrading to 3.116.1 appears to resolve the immediate symptom per a Telerik contributor comment. The underlying root cause (conditional compilation inconsistency) is tracked in #2414.

### Key Signals

- "warning MT0182: Not linking with the framework OpenGLES (referenced by a module reference in SkiaSharp.Views.iOS.dll) because it's not available on the current platform (MacCatalyst)." — **issue title and body** (The compiled SkiaSharp.Views.iOS.dll for maccatalyst contains a DllImport to OpenGLES, which is unavailable on MacCatalyst; the Apple linker emits MT0182.)
- "I have reproduced the warning outside of the usage of Telerik controls. Sample maui project just added the SkiaSharp.Views.Maui.Controls and Compatibility reference and called .UseSkiaSharp inside the MauiProgram.cs file." — **issue comment by reporter** (Confirms root cause is in SkiaSharp itself, not a Telerik-specific issue.)
- "In general, the issue happens with Skia 2.88.6/7/8 versions. With Telerik MAUI 11.0.0 release, we have updated the SkiaSharp version we use as a dependency to version 3.116.1. So now the warning message is not presented." — **comment by Telerik contributor @didiyordanova (May 2025)** (Upgrading to SkiaSharp 3.116.1 resolves the warning for Telerik's use case, suggesting a partial fix landed in 3.116.1.)
- "I think this is because I am including the code to use Open GL - even though I don't really use it." — **maintainer @mattleibow comment on #2414** (Maintainer confirmed root cause: OpenGL code (Gles.cs) is compiled into the maccatalyst assembly despite not being needed.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.Shared/GlesInterop/Gles.cs` | 1 | direct | Guard uses `!MACCATALYST` (single underscores) while platform-specific iOS files use `!__MACCATALYST__` (double underscores). If both symbols are not consistently defined at package build time, Gles.cs may be compiled into the maccatalyst assembly, introducing an OpenGLES DllImport reference (line 14: `private const string libGLESv2 = "/System/Library/Frameworks/OpenGLES.framework/OpenGLES"` — active when `__IOS__` is defined, which it is for maccatalyst builds). |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLView.cs` | 1 | direct | Correctly guarded with `#if !__MACCATALYST__` (double underscores) to exclude the entire OpenGL-dependent GLKView subclass from MacCatalyst builds. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLLayer.cs` | 1 | related | Correctly guarded with `#if !__MACCATALYST__` (double underscores) excluding CAEAGLLayer (OpenGL) from MacCatalyst builds. |
| `source/SkiaSharp.Views/SkiaSharp.Views/SkiaSharp.Views.csproj` | 8-11,40-45 | context | Both `-ios` and `-maccatalyst` TFMs share the same assembly name `SkiaSharp.Views.iOS` and both include `Platform/iOS/**/*.cs`. This means all iOS-platform files including Gles.cs from SkiaSharp.Views.Shared are compiled into the maccatalyst build. |

### Workarounds

- Upgrade SkiaSharp to version 3.116.1 or later — Telerik confirmed this eliminates the warning.
- If upgrading is not possible, suppress the specific linker warning with `<MtouchExtraArgs>--ignore-warnings MT0182</MtouchExtraArgs>` in the csproj (warning only, app still functions).

### Next Questions

- Does the warning still reproduce on SkiaSharp 3.116.1 or the latest available version?
- Is the `MACCATALYST` preprocessor symbol defined (without double underscores) when building the SkiaSharp NuGet package? Compare to `__MACCATALYST__`.
- Can the reporter confirm the issue is resolved by upgrading to 3.116.1?

### Resolution Proposals

**Hypothesis:** The `SkiaSharp.Views.iOS.dll` built for the maccatalyst TFM includes a DllImport for OpenGLES from `Gles.cs` because the compile guard `!MACCATALYST` may not match the actual symbol at package build time. The fix in 3.116.1 may have addressed this, or the issue may have been version-specific to 3.116.0.

1. **Upgrade to SkiaSharp 3.116.1 or later** — workaround, confidence 0.75 (75%), cost/xs, validated=untested
   - Update the SkiaSharp NuGet package reference to 3.116.1 or newer. Telerik confirmed the MT0182 warning disappears on 3.116.1.
2. **Suppress the linker warning** — workaround, confidence 0.70 (70%), cost/xs, validated=untested
   - Add `<MtouchExtraArgs>--ignore-warnings MT0182</MtouchExtraArgs>` to the MacCatalyst target in the csproj to suppress the warning. The app still functions since the OpenGLES code path is guarded at runtime and MacCatalyst uses Metal instead.
3. **Fix conditional compilation in Gles.cs** — fix, confidence 0.65 (65%), cost/xs, validated=untested
   - Align the MacCatalyst guard in Gles.cs to use `!__MACCATALYST__` (double underscores) instead of `!MACCATALYST` to be consistent with SKGLView.cs and SKGLLayer.cs, ensuring the OpenGLES code is never compiled into the maccatalyst assembly.

**Recommended proposal:** Upgrade to SkiaSharp 3.116.1 or later

**Why:** Fastest resolution for the reporter. Telerik's investigation confirms 3.116.1 eliminates the warning. The underlying root cause is tracked in the parent issue #2414.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-duplicate |
| Confidence | 0.82 (82%) |
| Reason | This is the same MT0182 / OpenGLES / MacCatalyst warning as #2414 which is still open and tracks the underlying root cause. The reporter confirmed reproduction without Telerik controls using plain SkiaSharp.Views.Maui.Controls, identical to #2414 findings. Upgrading to 3.116.1 resolves the reporter's immediate symptom. |
| Suggested repro platform | macos |

### Missing Info

- Does the issue reproduce on SkiaSharp 3.116.1 or the latest version?
- macOS version and Xcode version are not specified.

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply area/SkiaSharp.Views, os/macOS, backend/OpenGL, tenet/compatibility, partner/maui and triage/triaged labels | labels=type/bug, area/SkiaSharp.Views, os/macOS, backend/OpenGL, tenet/compatibility, partner/maui, triage/triaged |
| link-duplicate | medium | 0.82 (82%) | Mark as duplicate of #2414 which is the original tracking issue for the same MT0182/OpenGLES/MacCatalyst warning | linkedIssue=#2414 |
| add-comment | medium | 0.82 (82%) | Inform reporter of duplicate and workaround | — |
| close-issue | medium | 0.80 (80%) | Close as duplicate of #2414 | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and for reproducing it with a minimal project!

This appears to be a duplicate of #2414, which tracks the same MT0182 OpenGLES warning on MacCatalyst.

As a workaround, a Telerik contributor confirmed that upgrading to **SkiaSharp 3.116.1 or later** eliminates the warning. Could you try upgrading your `SkiaSharp` and `SkiaSharp.Views.Maui.*` packages to 3.116.1 and let us know if that resolves the issue?

If you cannot upgrade right away, you can suppress the warning by adding this to your MacCatalyst build configuration in the `.csproj`:

```xml
<PropertyGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'maccatalyst'">
  <MtouchExtraArgs>--ignore-warnings MT0182</MtouchExtraArgs>
</PropertyGroup>
```

Note: This is a warning (not a hard error) and the app should still function, since MacCatalyst uses Metal rendering rather than OpenGLES. The root cause (OpenGL code inadvertently compiled into the maccatalyst assembly) is being tracked in #2414.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3144,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T04:26:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Building a .NET 8 MAUI app for MacCatalyst produces linker warning MT0182: Not linking with framework OpenGLES (referenced by SkiaSharp.Views.iOS.dll) because OpenGLES is unavailable on MacCatalyst, preventing the app from building.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
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
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "build-error",
      "errorMessage": "warning MT0182: Not linking with the framework OpenGLES (referenced by a module reference in SkiaSharp.Views.iOS.dll) because it's not available on the current platform (MacCatalyst).",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0-maccatalyst"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET 8 MAUI project on macOS",
        "Add SkiaSharp.Views.Maui.Controls and Compatibility references",
        "Call .UseSkiaSharp() in MauiProgram.cs",
        "Build/deploy the app targeting MacCatalyst"
      ],
      "environmentDetails": "macOS, .NET 8, SkiaSharp 3.116.0, Visual Studio Code, reproduces without Telerik controls",
      "attachments": [
        {
          "url": "https://github.com/user-attachments/files/18503349/8b82398c-4c31-4ec5-b53f-2b4a2e5c5ba3_archive-net8.zip",
          "filename": "8b82398c-4c31-4ec5-b53f-2b4a2e5c5ba3_archive-net8.zip",
          "description": "Sample MAUI app reproducing the MT0182 warning with Telerik controls"
        }
      ],
      "relatedIssues": [
        2414
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2414",
          "description": "Original tracking issue for same MT0182 OpenGLES/MacCatalyst warning, still open"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.0",
      "currentRelevance": "unlikely",
      "relevanceReason": "A Telerik contributor confirmed the warning is absent in SkiaSharp 3.116.1; may have been introduced in 3.116.0 and fixed in 3.116.1."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.65,
      "reason": "Reporter states the app worked on 2.88.9 and broke on 3.116.0. However other data points suggest 2.88.x also has the same warning; the version relationship is unclear.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.72,
      "reason": "Telerik MAUI contributor confirmed in May 2025 that upgrading to SkiaSharp 3.116.1 eliminates the warning. The parent issue #2414 was reproduced on 2.88.7-9 but could not be reproduced on 3.116.1."
    }
  },
  "analysis": {
    "summary": "The MT0182 linker warning is generated because SkiaSharp.Views.iOS.dll (used for both ios and maccatalyst targets) contains a P/Invoke DllImport referencing the OpenGLES framework. The Gles.cs shared file uses `!MACCATALYST` as its compile guard while the iOS-platform files (SKGLView.cs, SKGLLayer.cs) use `!__MACCATALYST__`; if either symbol definition differs at package build time, Gles.cs may be included in the maccatalyst assembly, producing an OpenGLES module reference that the Apple linker rejects.",
    "rationale": "This is a duplicate/near-duplicate of #2414 which tracks the identical warning and root cause. The maintainer commented in #2414 that the OpenGL code is included in the iOS-shared assembly even though it is not needed on MacCatalyst. Upgrading to 3.116.1 appears to resolve the immediate symptom per a Telerik contributor comment. The underlying root cause (conditional compilation inconsistency) is tracked in #2414.",
    "keySignals": [
      {
        "text": "warning MT0182: Not linking with the framework OpenGLES (referenced by a module reference in SkiaSharp.Views.iOS.dll) because it's not available on the current platform (MacCatalyst).",
        "source": "issue title and body",
        "interpretation": "The compiled SkiaSharp.Views.iOS.dll for maccatalyst contains a DllImport to OpenGLES, which is unavailable on MacCatalyst; the Apple linker emits MT0182."
      },
      {
        "text": "I have reproduced the warning outside of the usage of Telerik controls. Sample maui project just added the SkiaSharp.Views.Maui.Controls and Compatibility reference and called .UseSkiaSharp inside the MauiProgram.cs file.",
        "source": "issue comment by reporter",
        "interpretation": "Confirms root cause is in SkiaSharp itself, not a Telerik-specific issue."
      },
      {
        "text": "In general, the issue happens with Skia 2.88.6/7/8 versions. With Telerik MAUI 11.0.0 release, we have updated the SkiaSharp version we use as a dependency to version 3.116.1. So now the warning message is not presented.",
        "source": "comment by Telerik contributor @didiyordanova (May 2025)",
        "interpretation": "Upgrading to SkiaSharp 3.116.1 resolves the warning for Telerik's use case, suggesting a partial fix landed in 3.116.1."
      },
      {
        "text": "I think this is because I am including the code to use Open GL - even though I don't really use it.",
        "source": "maintainer @mattleibow comment on #2414",
        "interpretation": "Maintainer confirmed root cause: OpenGL code (Gles.cs) is compiled into the maccatalyst assembly despite not being needed."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Shared/GlesInterop/Gles.cs",
        "lines": "1",
        "finding": "Guard uses `!MACCATALYST` (single underscores) while platform-specific iOS files use `!__MACCATALYST__` (double underscores). If both symbols are not consistently defined at package build time, Gles.cs may be compiled into the maccatalyst assembly, introducing an OpenGLES DllImport reference (line 14: `private const string libGLESv2 = \"/System/Library/Frameworks/OpenGLES.framework/OpenGLES\"` — active when `__IOS__` is defined, which it is for maccatalyst builds).",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLView.cs",
        "lines": "1",
        "finding": "Correctly guarded with `#if !__MACCATALYST__` (double underscores) to exclude the entire OpenGL-dependent GLKView subclass from MacCatalyst builds.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKGLLayer.cs",
        "lines": "1",
        "finding": "Correctly guarded with `#if !__MACCATALYST__` (double underscores) excluding CAEAGLLayer (OpenGL) from MacCatalyst builds.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/SkiaSharp.Views.csproj",
        "lines": "8-11,40-45",
        "finding": "Both `-ios` and `-maccatalyst` TFMs share the same assembly name `SkiaSharp.Views.iOS` and both include `Platform/iOS/**/*.cs`. This means all iOS-platform files including Gles.cs from SkiaSharp.Views.Shared are compiled into the maccatalyst build.",
        "relevance": "context"
      }
    ],
    "nextQuestions": [
      "Does the warning still reproduce on SkiaSharp 3.116.1 or the latest available version?",
      "Is the `MACCATALYST` preprocessor symbol defined (without double underscores) when building the SkiaSharp NuGet package? Compare to `__MACCATALYST__`.",
      "Can the reporter confirm the issue is resolved by upgrading to 3.116.1?"
    ],
    "workarounds": [
      "Upgrade SkiaSharp to version 3.116.1 or later — Telerik confirmed this eliminates the warning.",
      "If upgrading is not possible, suppress the specific linker warning with `<MtouchExtraArgs>--ignore-warnings MT0182</MtouchExtraArgs>` in the csproj (warning only, app still functions)."
    ],
    "resolution": {
      "hypothesis": "The `SkiaSharp.Views.iOS.dll` built for the maccatalyst TFM includes a DllImport for OpenGLES from `Gles.cs` because the compile guard `!MACCATALYST` may not match the actual symbol at package build time. The fix in 3.116.1 may have addressed this, or the issue may have been version-specific to 3.116.0.",
      "proposals": [
        {
          "title": "Upgrade to SkiaSharp 3.116.1 or later",
          "description": "Update the SkiaSharp NuGet package reference to 3.116.1 or newer. Telerik confirmed the MT0182 warning disappears on 3.116.1.",
          "category": "workaround",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Suppress the linker warning",
          "description": "Add `<MtouchExtraArgs>--ignore-warnings MT0182</MtouchExtraArgs>` to the MacCatalyst target in the csproj to suppress the warning. The app still functions since the OpenGLES code path is guarded at runtime and MacCatalyst uses Metal instead.",
          "category": "workaround",
          "confidence": 0.7,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Fix conditional compilation in Gles.cs",
          "description": "Align the MacCatalyst guard in Gles.cs to use `!__MACCATALYST__` (double underscores) instead of `!MACCATALYST` to be consistent with SKGLView.cs and SKGLLayer.cs, ensuring the OpenGLES code is never compiled into the maccatalyst assembly.",
          "category": "fix",
          "confidence": 0.65,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Upgrade to SkiaSharp 3.116.1 or later",
      "recommendedReason": "Fastest resolution for the reporter. Telerik's investigation confirms 3.116.1 eliminates the warning. The underlying root cause is tracked in the parent issue #2414."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-duplicate",
      "confidence": 0.82,
      "reason": "This is the same MT0182 / OpenGLES / MacCatalyst warning as #2414 which is still open and tracks the underlying root cause. The reporter confirmed reproduction without Telerik controls using plain SkiaSharp.Views.Maui.Controls, identical to #2414 findings. Upgrading to 3.116.1 resolves the reporter's immediate symptom.",
      "suggestedReproPlatform": "macos"
    },
    "missingInfo": [
      "Does the issue reproduce on SkiaSharp 3.116.1 or the latest version?",
      "macOS version and Xcode version are not specified."
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply area/SkiaSharp.Views, os/macOS, backend/OpenGL, tenet/compatibility, partner/maui and triage/triaged labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/macOS",
          "backend/OpenGL",
          "tenet/compatibility",
          "partner/maui",
          "triage/triaged"
        ]
      },
      {
        "type": "link-duplicate",
        "description": "Mark as duplicate of #2414 which is the original tracking issue for the same MT0182/OpenGLES/MacCatalyst warning",
        "risk": "medium",
        "confidence": 0.82,
        "linkedIssue": 2414
      },
      {
        "type": "add-comment",
        "description": "Inform reporter of duplicate and workaround",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report and for reproducing it with a minimal project!\n\nThis appears to be a duplicate of #2414, which tracks the same MT0182 OpenGLES warning on MacCatalyst.\n\nAs a workaround, a Telerik contributor confirmed that upgrading to **SkiaSharp 3.116.1 or later** eliminates the warning. Could you try upgrading your `SkiaSharp` and `SkiaSharp.Views.Maui.*` packages to 3.116.1 and let us know if that resolves the issue?\n\nIf you cannot upgrade right away, you can suppress the warning by adding this to your MacCatalyst build configuration in the `.csproj`:\n\n```xml\n<PropertyGroup Condition=\"$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'maccatalyst'\">\n  <MtouchExtraArgs>--ignore-warnings MT0182</MtouchExtraArgs>\n</PropertyGroup>\n```\n\nNote: This is a warning (not a hard error) and the app should still function, since MacCatalyst uses Metal rendering rather than OpenGLES. The root cause (OpenGL code inadvertently compiled into the maccatalyst assembly) is being tracked in #2414."
      },
      {
        "type": "close-issue",
        "description": "Close as duplicate of #2414",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
