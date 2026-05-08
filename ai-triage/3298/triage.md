# Issue Triage Report — #3298

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:36:00Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/libSkiaSharp.native (0.90 (90%)) |
| Suggested action | close-as-duplicate (0.88 (88%)) |

**Issue Summary:** Xamarin UWP app crashes at startup with exit code 0xc0000135 (dependency DLL not found) and MCG0007 P/Invoke warning for libEGL.dll after upgrading from SkiaSharp 2.88.9 to 3.116.0 because UWP support was dropped in SkiaSharp 3.x.

**Analysis:** SkiaSharp 3.x dropped UWP (Universal Windows Platform) support. The SkiaSharp.NativeAssets.UWP package is marked obsolete with no v3 equivalent. The ANGLE DLLs (libEGL.dll, libGLESv2.dll) are no longer deployed for UWP apps, causing the startup crash with 0xc0000135. MCG0007 is expected because SkiaSharp's AngleLoader class uses non-UWP P/Invoke signatures for libEGL.dll without the UWP build flag. This is a known issue tracked by #2746.

**Recommendations:** **close-as-duplicate** — This is the same root issue as #2746 (UWP not supported in SkiaSharp 3.x). Closing as duplicate points the reporter to the canonical tracking issue.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Windows-Universal-UWP |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a Xamarin project with a UWP head
2. Install SkiaSharp 3.116.0 NuGet package
3. Build the UWP project — MCG0007 warning appears for libEGL.dll!eglGetProcAddress
4. Run the UWP app — crashes immediately with exit code 0xc0000135

**Environment:** Xamarin UWP, SkiaSharp 3.116.0, Visual Studio on Windows

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2746 — Feature request: Support UWP for SkiaSharp 3.0 — canonical open issue for missing UWP support in v3

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | crash |
| Error message | The program [4484] TEST.UWP.exe exit with code 3221225781 (0xc0000135) 'dependency DLL not found' |
| Repro quality | partial |
| Target frameworks | uap10.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | UWP support was removed in SkiaSharp 3.x; no UWP-compatible native assets package exists in v3. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.95 (95%) |
| Reason | Reporter explicitly states it worked in SkiaSharp 2.88.9 and broke after upgrading to 3.116.0. UWP native assets package (SkiaSharp.NativeAssets.UWP) was marked obsolete with no v3 replacement. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

SkiaSharp 3.x dropped UWP (Universal Windows Platform) support. The SkiaSharp.NativeAssets.UWP package is marked obsolete with no v3 equivalent. The ANGLE DLLs (libEGL.dll, libGLESv2.dll) are no longer deployed for UWP apps, causing the startup crash with 0xc0000135. MCG0007 is expected because SkiaSharp's AngleLoader class uses non-UWP P/Invoke signatures for libEGL.dll without the UWP build flag. This is a known issue tracked by #2746.

### Rationale

The crash is a direct consequence of UWP support being dropped in v3. The SkiaSharp.NativeAssets.UWP package is listed as obsolete in documentation with no v3 replacement. Issue #2746 is the canonical open feature request for restoring UWP support. The MCG0007 warning confirms the code is being compiled for UWP without the WINDOWS_UWP preprocessor flag that enables the correct P/Invoke path. The runtime crash is because libEGL.dll is not present in the UWP app package.

### Key Signals

- "warning MCG0007: P/Invoke-Methode 'libEGL.dll!eglGetProcAddress' for method 'System.IntPtr SkiaSharp.GRGlInterface.AngleLoader.eglGetProcAddress(System.String)'" — **issue body** (MCG0007 is a UWP managed code generator warning — it flags P/Invoke calls that don't target a UWP API contract. In SkiaSharp 3.x the WINDOWS_UWP build path is not used for Xamarin UWP, so the standard Kernel32/libEGL DllImport code is compiled in.)
- "exit with code 3221225781 (0xc0000135) 'dependency DLL not found'" — **issue body** (0xc0000135 means a required DLL was not found in the app package. libEGL.dll (ANGLE) is not deployed because SkiaSharp.NativeAssets.UWP was dropped in v3.)
- "below Version 3 everything is fine" — **issue body** (Confirms regression introduced in v3 — SkiaSharp 2.88.9 had SkiaSharp.NativeAssets.UWP which supplied libEGL.dll and libGLESv2.dll.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/GRGlInterface.cs` | 124-199 | direct | AngleLoader class has #if WINDOWS_UWP path (lines 130-139) using LoadPackagedLibrary from api-ms-win-core-libraryloader-l2-1-0.dll, but the default non-UWP path uses Kernel32.dll LoadLibrary which is not in the UWP API contract. In SkiaSharp 3.x, WINDOWS_UWP is no longer defined for Xamarin UWP projects, causing MCG0007 warning and incorrect P/Invoke path. |
| `documentation/dev/packages.md` | 163-165 | direct | SkiaSharp.NativeAssets.UWP is listed as OBSOLETE with replacement SkiaSharp.NativeAssets.WinUI (for WinUI3, not UWP). No UWP-compatible native assets exist in SkiaSharp 3.x. This confirms UWP support was intentionally removed. |

### Next Questions

- Is the user using Xamarin.Forms or native Xamarin UWP? (Both are affected but the symptom may differ slightly.)
- Is the user aware that UWP support was dropped in v3, or did they upgrade expecting full compatibility?

### Resolution Proposals

**Hypothesis:** UWP support was dropped in SkiaSharp 3.x. The SkiaSharp.NativeAssets.UWP package is obsolete with no v3 replacement. The only path forward is to stay on SkiaSharp 2.88.x or migrate the UWP app to WinUI 3.

1. **Stay on SkiaSharp 2.88.x for UWP** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - Downgrade (or do not upgrade) the SkiaSharp package to the latest 2.88.x release for the UWP project head. This is supported and receives security/critical fixes.
2. **Migrate UWP app to WinUI 3** — alternative, confidence 0.85 (85%), cost/xl, validated=untested
   - Migrate the UWP project head to WinUI 3, then use SkiaSharp.Views.WinUI and SkiaSharp.NativeAssets.WinUI which are fully supported in SkiaSharp 3.x. This requires Windows App SDK.

**Recommended proposal:** Stay on SkiaSharp 2.88.x for UWP

**Why:** Lowest effort and no application rewrite required. UWP users should stay on 2.88.x until or unless UWP support is restored in v3 (tracked in #2746).

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-duplicate |
| Confidence | 0.88 (88%) |
| Reason | This is the same root issue as #2746 (UWP not supported in SkiaSharp 3.x). Closing as duplicate points the reporter to the canonical tracking issue. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, native assets, UWP platform and compatibility tenet labels | labels=type/bug, area/libSkiaSharp.native, os/Windows-Universal-UWP, tenet/compatibility |
| link-duplicate | medium | 0.88 (88%) | Mark as duplicate of #2746 (Support UWP for SkiaSharp 3.0) | linkedIssue=#2746 |
| add-comment | medium | 0.88 (88%) | Explain that UWP was dropped in v3 and suggest workarounds | — |
| close-issue | medium | 0.85 (85%) | Close as duplicate of #2746 | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! This crash is caused by SkiaSharp 3.x dropping UWP (Universal Windows Platform) support. The `SkiaSharp.NativeAssets.UWP` package that supplied `libEGL.dll` and `libGLESv2.dll` in v2 is now obsolete and has no equivalent for v3. The MCG0007 warning and 0xc0000135 crash are both symptoms of the missing ANGLE DLLs.

This is a known issue tracked in #2746.

**Workarounds:**
1. **Stay on SkiaSharp 2.88.x** — Pin the SkiaSharp package version to `2.88.*` for your UWP project head. This is fully supported and still receives critical fixes.
2. **Migrate to WinUI 3** — If migration is feasible, WinUI 3 is fully supported in SkiaSharp 3.x via `SkiaSharp.Views.WinUI` and `SkiaSharp.NativeAssets.WinUI`.

Closing this as a duplicate of #2746 where the UWP v3 support request is being tracked.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3298,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:36:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Xamarin UWP app crashes at startup with exit code 0xc0000135 (dependency DLL not found) and MCG0007 P/Invoke warning for libEGL.dll after upgrading from SkiaSharp 2.88.9 to 3.116.0 because UWP support was dropped in SkiaSharp 3.x.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/libSkiaSharp.native",
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
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "crash",
      "errorMessage": "The program [4484] TEST.UWP.exe exit with code 3221225781 (0xc0000135) 'dependency DLL not found'",
      "reproQuality": "partial",
      "targetFrameworks": [
        "uap10.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin project with a UWP head",
        "Install SkiaSharp 3.116.0 NuGet package",
        "Build the UWP project — MCG0007 warning appears for libEGL.dll!eglGetProcAddress",
        "Run the UWP app — crashes immediately with exit code 0xc0000135"
      ],
      "environmentDetails": "Xamarin UWP, SkiaSharp 3.116.0, Visual Studio on Windows",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2746",
          "description": "Feature request: Support UWP for SkiaSharp 3.0 — canonical open issue for missing UWP support in v3"
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
      "currentRelevance": "likely",
      "relevanceReason": "UWP support was removed in SkiaSharp 3.x; no UWP-compatible native assets package exists in v3."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.95,
      "reason": "Reporter explicitly states it worked in SkiaSharp 2.88.9 and broke after upgrading to 3.116.0. UWP native assets package (SkiaSharp.NativeAssets.UWP) was marked obsolete with no v3 replacement.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "SkiaSharp 3.x dropped UWP (Universal Windows Platform) support. The SkiaSharp.NativeAssets.UWP package is marked obsolete with no v3 equivalent. The ANGLE DLLs (libEGL.dll, libGLESv2.dll) are no longer deployed for UWP apps, causing the startup crash with 0xc0000135. MCG0007 is expected because SkiaSharp's AngleLoader class uses non-UWP P/Invoke signatures for libEGL.dll without the UWP build flag. This is a known issue tracked by #2746.",
    "rationale": "The crash is a direct consequence of UWP support being dropped in v3. The SkiaSharp.NativeAssets.UWP package is listed as obsolete in documentation with no v3 replacement. Issue #2746 is the canonical open feature request for restoring UWP support. The MCG0007 warning confirms the code is being compiled for UWP without the WINDOWS_UWP preprocessor flag that enables the correct P/Invoke path. The runtime crash is because libEGL.dll is not present in the UWP app package.",
    "keySignals": [
      {
        "text": "warning MCG0007: P/Invoke-Methode 'libEGL.dll!eglGetProcAddress' for method 'System.IntPtr SkiaSharp.GRGlInterface.AngleLoader.eglGetProcAddress(System.String)'",
        "source": "issue body",
        "interpretation": "MCG0007 is a UWP managed code generator warning — it flags P/Invoke calls that don't target a UWP API contract. In SkiaSharp 3.x the WINDOWS_UWP build path is not used for Xamarin UWP, so the standard Kernel32/libEGL DllImport code is compiled in."
      },
      {
        "text": "exit with code 3221225781 (0xc0000135) 'dependency DLL not found'",
        "source": "issue body",
        "interpretation": "0xc0000135 means a required DLL was not found in the app package. libEGL.dll (ANGLE) is not deployed because SkiaSharp.NativeAssets.UWP was dropped in v3."
      },
      {
        "text": "below Version 3 everything is fine",
        "source": "issue body",
        "interpretation": "Confirms regression introduced in v3 — SkiaSharp 2.88.9 had SkiaSharp.NativeAssets.UWP which supplied libEGL.dll and libGLESv2.dll."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/GRGlInterface.cs",
        "lines": "124-199",
        "finding": "AngleLoader class has #if WINDOWS_UWP path (lines 130-139) using LoadPackagedLibrary from api-ms-win-core-libraryloader-l2-1-0.dll, but the default non-UWP path uses Kernel32.dll LoadLibrary which is not in the UWP API contract. In SkiaSharp 3.x, WINDOWS_UWP is no longer defined for Xamarin UWP projects, causing MCG0007 warning and incorrect P/Invoke path.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "lines": "163-165",
        "finding": "SkiaSharp.NativeAssets.UWP is listed as OBSOLETE with replacement SkiaSharp.NativeAssets.WinUI (for WinUI3, not UWP). No UWP-compatible native assets exist in SkiaSharp 3.x. This confirms UWP support was intentionally removed.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "Is the user using Xamarin.Forms or native Xamarin UWP? (Both are affected but the symptom may differ slightly.)",
      "Is the user aware that UWP support was dropped in v3, or did they upgrade expecting full compatibility?"
    ],
    "resolution": {
      "hypothesis": "UWP support was dropped in SkiaSharp 3.x. The SkiaSharp.NativeAssets.UWP package is obsolete with no v3 replacement. The only path forward is to stay on SkiaSharp 2.88.x or migrate the UWP app to WinUI 3.",
      "proposals": [
        {
          "title": "Stay on SkiaSharp 2.88.x for UWP",
          "description": "Downgrade (or do not upgrade) the SkiaSharp package to the latest 2.88.x release for the UWP project head. This is supported and receives security/critical fixes.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Migrate UWP app to WinUI 3",
          "description": "Migrate the UWP project head to WinUI 3, then use SkiaSharp.Views.WinUI and SkiaSharp.NativeAssets.WinUI which are fully supported in SkiaSharp 3.x. This requires Windows App SDK.",
          "category": "alternative",
          "confidence": 0.85,
          "effort": "cost/xl",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Stay on SkiaSharp 2.88.x for UWP",
      "recommendedReason": "Lowest effort and no application rewrite required. UWP users should stay on 2.88.x until or unless UWP support is restored in v3 (tracked in #2746)."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-duplicate",
      "confidence": 0.88,
      "reason": "This is the same root issue as #2746 (UWP not supported in SkiaSharp 3.x). Closing as duplicate points the reporter to the canonical tracking issue.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, native assets, UWP platform and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Windows-Universal-UWP",
          "tenet/compatibility"
        ]
      },
      {
        "type": "link-duplicate",
        "description": "Mark as duplicate of #2746 (Support UWP for SkiaSharp 3.0)",
        "risk": "medium",
        "confidence": 0.88,
        "linkedIssue": 2746
      },
      {
        "type": "add-comment",
        "description": "Explain that UWP was dropped in v3 and suggest workarounds",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the report! This crash is caused by SkiaSharp 3.x dropping UWP (Universal Windows Platform) support. The `SkiaSharp.NativeAssets.UWP` package that supplied `libEGL.dll` and `libGLESv2.dll` in v2 is now obsolete and has no equivalent for v3. The MCG0007 warning and 0xc0000135 crash are both symptoms of the missing ANGLE DLLs.\n\nThis is a known issue tracked in #2746.\n\n**Workarounds:**\n1. **Stay on SkiaSharp 2.88.x** — Pin the SkiaSharp package version to `2.88.*` for your UWP project head. This is fully supported and still receives critical fixes.\n2. **Migrate to WinUI 3** — If migration is feasible, WinUI 3 is fully supported in SkiaSharp 3.x via `SkiaSharp.Views.WinUI` and `SkiaSharp.NativeAssets.WinUI`.\n\nClosing this as a duplicate of #2746 where the UWP v3 support request is being tracked."
      },
      {
        "type": "close-issue",
        "description": "Close as duplicate of #2746",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
