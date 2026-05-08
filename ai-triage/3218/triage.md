# Issue Triage Report — #3218

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T01:45:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/libSkiaSharp.native (0.95 (95%)) |
| Suggested action | close-as-fixed (0.85 (85%)) |

**Issue Summary:** libSkiaSharp.dll on Windows does not enable Control Flow Guard (CFG), causing BinSkim rule BA2008 failures in security-conscious environments.

**Analysis:** The Windows native build scripts did not pass /guard:cf (Control Flow Guard) on the MSVC compiler and linker, causing BinSkim BA2008 failures. The fix is already present in the current source tree.

**Recommendations:** **close-as-fixed** — CFG flags (/guard:cf) are present in the current native/windows/build.cake and all relevant vcxproj files. Community confirms fix was merged. Issue is fixed in source; stable release is pending.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Download SkiaSharp 3.116.0 NuGet package
2. Run BinSkim on libSkiaSharp.dll
3. Observe BA2008 rule failure: /guard:cf not present

**Environment:** SkiaSharp 3.116.0, Windows, Visual Studio

**Related issues:** #3281

**Repository links:**
- https://github.com/microsoft/binskim/blob/main/docs/BinSkimRules.md#rule-BA2008EnableControlFlowGuard — BinSkim BA2008 rule documentation

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | other |
| Error message | BA2008: EnableControlFlowGuard — /guard:cf missing on compiler and linker command lines |
| Repro quality | partial |
| Target frameworks | net8.0-windows |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 2.88.9 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | native/windows/build.cake already includes /guard:cf in extra_cflags and extra_ldflags; fix is in source but awaiting stable release. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.90 (90%) |
| Reason | native/windows/build.cake now passes /guard:cf on both compiler and linker; libHarfBuzzSharp.vcxproj and SkiaSharp.Views.WinUI.Native.vcxproj also contain ControlFlowGuard=Guard settings. Community comment notes this was fixed by PR #3281. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

The Windows native build scripts did not pass /guard:cf (Control Flow Guard) on the MSVC compiler and linker, causing BinSkim BA2008 failures. The fix is already present in the current source tree.

### Rationale

This is a native build configuration issue affecting Windows binary security compliance. The reporter correctly identified the fix (pass /guard:cf on compiler and linker). Code investigation confirms the fix has since been applied to native/windows/build.cake and the related vcxproj files. The issue is likely fixed in source but not yet in a published stable release as of December 2025.

### Key Signals

- "pass /guard:cf on both the compiler and linker command lines" — **issue body** (Reporter correctly identifies the exact fix needed per BinSkim BA2008 guidance.)
- "It seems like this was fixed by PR #3281, but there is still no published stable version of SkiaSharp with this." — **comment by azchohfi (2025-12-13)** (Community confirms fix was merged but awaits a stable release.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `native/windows/build.cake` | 79-80 | direct | extra_cflags includes '/guard:cf' and extra_ldflags includes '/guard:cf' for the libSkiaSharp.dll GN build — CFG is now enabled at both compile and link time. |
| `native/windows/libHarfBuzzSharp/libHarfBuzzSharp.vcxproj` | — | related | Multiple configurations include ControlFlowGuard=Guard and ControlFlowGuard=true — libHarfBuzzSharp.dll also has CFG enabled. |
| `native/winui/SkiaSharp.Views.WinUI.Native/SkiaSharp.Views.WinUI.Native/SkiaSharp.Views.WinUI.Native.vcxproj` | 115-122 | related | ControlFlowGuard=Guard present in release configuration — WinUI native component has CFG enabled. |

### Next Questions

- Has a preview or stable NuGet package been published with the CFG fix included?
- Does the libSkiaSharp.dll in the latest preview NuGet pass BinSkim BA2008?

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.85 (85%) |
| Reason | CFG flags (/guard:cf) are present in the current native/windows/build.cake and all relevant vcxproj files. Community confirms fix was merged. Issue is fixed in source; stable release is pending. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply native library and Windows labels | labels=type/bug, area/libSkiaSharp.native, os/Windows-Classic, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Inform reporter the fix is in source and pending stable release | — |
| close-issue | medium | 0.85 (85%) | Close as fixed — CFG is now enabled in the build scripts | stateReason=completed |
| link-related | low | 0.90 (90%) | Link related issue #3281 which covers the same CFG problem on additional DLLs | linkedIssue=#3281 |

**Comment draft for `add-comment`:**

```markdown
The Control Flow Guard (`/guard:cf`) flag has been added to the Windows native build configuration (`native/windows/build.cake`) for `libSkiaSharp.dll`, and CFG settings have also been applied to `libHarfBuzzSharp.vcxproj` and `SkiaSharp.Views.WinUI.Native.vcxproj`. The fix should be included in the next stable release. Watch the [releases page](https://github.com/mono/SkiaSharp/releases) for the next published version.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3218,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T01:45:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "libSkiaSharp.dll on Windows does not enable Control Flow Guard (CFG), causing BinSkim rule BA2008 failures in security-conscious environments.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Classic"
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
      "errorMessage": "BA2008: EnableControlFlowGuard — /guard:cf missing on compiler and linker command lines",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0-windows"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Download SkiaSharp 3.116.0 NuGet package",
        "Run BinSkim on libSkiaSharp.dll",
        "Observe BA2008 rule failure: /guard:cf not present"
      ],
      "environmentDetails": "SkiaSharp 3.116.0, Windows, Visual Studio",
      "relatedIssues": [
        3281
      ],
      "repoLinks": [
        {
          "url": "https://github.com/microsoft/binskim/blob/main/docs/BinSkimRules.md#rule-BA2008EnableControlFlowGuard",
          "description": "BinSkim BA2008 rule documentation"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "2.88.9"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "native/windows/build.cake already includes /guard:cf in extra_cflags and extra_ldflags; fix is in source but awaiting stable release."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.9,
      "reason": "native/windows/build.cake now passes /guard:cf on both compiler and linker; libHarfBuzzSharp.vcxproj and SkiaSharp.Views.WinUI.Native.vcxproj also contain ControlFlowGuard=Guard settings. Community comment notes this was fixed by PR #3281."
    }
  },
  "analysis": {
    "summary": "The Windows native build scripts did not pass /guard:cf (Control Flow Guard) on the MSVC compiler and linker, causing BinSkim BA2008 failures. The fix is already present in the current source tree.",
    "rationale": "This is a native build configuration issue affecting Windows binary security compliance. The reporter correctly identified the fix (pass /guard:cf on compiler and linker). Code investigation confirms the fix has since been applied to native/windows/build.cake and the related vcxproj files. The issue is likely fixed in source but not yet in a published stable release as of December 2025.",
    "keySignals": [
      {
        "text": "pass /guard:cf on both the compiler and linker command lines",
        "source": "issue body",
        "interpretation": "Reporter correctly identifies the exact fix needed per BinSkim BA2008 guidance."
      },
      {
        "text": "It seems like this was fixed by PR #3281, but there is still no published stable version of SkiaSharp with this.",
        "source": "comment by azchohfi (2025-12-13)",
        "interpretation": "Community confirms fix was merged but awaits a stable release."
      }
    ],
    "codeInvestigation": [
      {
        "file": "native/windows/build.cake",
        "lines": "79-80",
        "finding": "extra_cflags includes '/guard:cf' and extra_ldflags includes '/guard:cf' for the libSkiaSharp.dll GN build — CFG is now enabled at both compile and link time.",
        "relevance": "direct"
      },
      {
        "file": "native/windows/libHarfBuzzSharp/libHarfBuzzSharp.vcxproj",
        "finding": "Multiple configurations include ControlFlowGuard=Guard and ControlFlowGuard=true — libHarfBuzzSharp.dll also has CFG enabled.",
        "relevance": "related"
      },
      {
        "file": "native/winui/SkiaSharp.Views.WinUI.Native/SkiaSharp.Views.WinUI.Native/SkiaSharp.Views.WinUI.Native.vcxproj",
        "lines": "115-122",
        "finding": "ControlFlowGuard=Guard present in release configuration — WinUI native component has CFG enabled.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Has a preview or stable NuGet package been published with the CFG fix included?",
      "Does the libSkiaSharp.dll in the latest preview NuGet pass BinSkim BA2008?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.85,
      "reason": "CFG flags (/guard:cf) are present in the current native/windows/build.cake and all relevant vcxproj files. Community confirms fix was merged. Issue is fixed in source; stable release is pending.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply native library and Windows labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Inform reporter the fix is in source and pending stable release",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "The Control Flow Guard (`/guard:cf`) flag has been added to the Windows native build configuration (`native/windows/build.cake`) for `libSkiaSharp.dll`, and CFG settings have also been applied to `libHarfBuzzSharp.vcxproj` and `SkiaSharp.Views.WinUI.Native.vcxproj`. The fix should be included in the next stable release. Watch the [releases page](https://github.com/mono/SkiaSharp/releases) for the next published version."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed — CFG is now enabled in the build scripts",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "completed"
      },
      {
        "type": "link-related",
        "description": "Link related issue #3281 which covers the same CFG problem on additional DLLs",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 3281
      }
    ]
  }
}
```

</details>
