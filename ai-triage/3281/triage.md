# Issue Triage Report — #3281

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T11:25:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/libSkiaSharp.native (0.90 (90%)) |
| Suggested action | close-as-duplicate (0.95 (95%)) |

**Issue Summary:** Reporter requests that Control Flow Guard (CFG) be enabled in libSkiaSharp.dll, libHarfBuzzSharp.dll, and SkiaSharp.Views.WinUI.Native.dll on Windows, citing BinSkim rule BA2008; this is a duplicate of #3218 which covers the same CFG gap.

**Analysis:** Duplicate of #3218. Reporter identified missing CFG security flag across three Windows native DLLs using BinSkim BA2008. Code investigation confirms CFG has already been added to all three build configurations in the current main branch: /guard:cf in native/windows/build.cake for libSkiaSharp.dll, and ControlFlowGuard=Guard in the .vcxproj files for libHarfBuzzSharp.dll and SkiaSharp.Views.WinUI.Native.dll.

**Recommendations:** **close-as-duplicate** — Identical BinSkim BA2008 finding as #3218, same version range, same fix suggestion. CFG is already enabled in the current main branch for all three DLLs. Duplicate discussion should continue on #3218.

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

**Environment:** Windows, Visual Studio, SkiaSharp 3.116.0

**Related issues:** #3218

**Repository links:**
- https://github.com/microsoft/binskim/blob/main/docs/BinSkimRules.md#rule-BA2008EnableControlFlowGuard — BinSkim BA2008 rule documentation
- https://github.com/mono/SkiaSharp/issues/3218 — Duplicate issue #3218 — same CFG complaint filed 2025-03-28, already triaged

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | BinSkim BA2008: libSkiaSharp.dll, libHarfBuzzSharp.dll, SkiaSharp.Views.WinUI.Native.dll do not enable Control Flow Guard |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | unlikely |
| Relevance reason | Code investigation shows /guard:cf is now in extra_cflags/extra_ldflags in native/windows/build.cake and ControlFlowGuard=Guard is set in libHarfBuzzSharp.vcxproj and SkiaSharp.Views.WinUI.Native.vcxproj — the fix has been applied in the current main branch. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.85 (85%) |
| Reason | All three DLL build configurations in the current main branch include CFG compiler and linker flags. However, the parent issue #3218 is still open, suggesting a release has not yet been cut. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

Duplicate of #3218. Reporter identified missing CFG security flag across three Windows native DLLs using BinSkim BA2008. Code investigation confirms CFG has already been added to all three build configurations in the current main branch: /guard:cf in native/windows/build.cake for libSkiaSharp.dll, and ControlFlowGuard=Guard in the .vcxproj files for libHarfBuzzSharp.dll and SkiaSharp.Views.WinUI.Native.dll.

### Rationale

Issue #3281 is materially identical to #3218 (98% similarity per bot, same BinSkim rule, same version range, same fix suggestion). The current codebase already has CFG enabled in all three DLL build configs. Closing as duplicate concentrates discussion on the original tracking issue.

### Key Signals

- "libSkiaSharp.dll, libHarfBuzzSharp.dll, and SkiaSharp.Views.WinUI.Native.dll do not enable the control flow guard (CFG)" — **issue title** (Reporter is using BinSkim to audit native Windows DLLs and found CFG not set)
- "We've found some similar issues: #3218, similarity score: 98%" — **similar-issues-ai[bot] comment** (High-confidence duplicate signal — #3218 covers the identical CFG gap)
- "Last Known Good Version: 2.88.9 (Previous)" — **issue body** (Reporter believes CFG was present in 2.88.x, which aligns with the 3.x native build refactor being the likely introduction point)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `native/windows/build.cake` | 79-80 | direct | libSkiaSharp.dll build uses extra_cflags with '/guard:cf' and extra_ldflags with '/guard:cf' — CFG is enabled for libSkiaSharp.dll in the current main branch |
| `native/windows/libHarfBuzzSharp/libHarfBuzzSharp.vcxproj` | 148,154 | direct | All Release/Debug x86, x64, ARM64 configurations include <ControlFlowGuard>Guard</ControlFlowGuard> in ClCompile and <ControlFlowGuard>true</ControlFlowGuard> in Link — CFG is enabled for libHarfBuzzSharp.dll |
| `native/winui/SkiaSharp.Views.WinUI.Native/SkiaSharp.Views.WinUI.Native/SkiaSharp.Views.WinUI.Native.vcxproj` | 115,122 | direct | ClCompile includes <ControlFlowGuard>Guard</ControlFlowGuard> and Link includes <ControlFlowGuard>true</ControlFlowGuard> — CFG is enabled for SkiaSharp.Views.WinUI.Native.dll |

### Next Questions

- When will the CFG fix in main be included in a released NuGet package?

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-duplicate |
| Confidence | 0.95 (95%) |
| Reason | Identical BinSkim BA2008 finding as #3218, same version range, same fix suggestion. CFG is already enabled in the current main branch for all three DLLs. Duplicate discussion should continue on #3218. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/libSkiaSharp.native, os/Windows-Classic, tenet/reliability labels | labels=type/bug, area/libSkiaSharp.native, os/Windows-Classic, tenet/reliability |
| link-duplicate | medium | 0.95 (95%) | Mark as duplicate of #3218 (same CFG issue, filed earlier, already triaged) | linkedIssue=#3218 |
| close-issue | medium | 0.90 (90%) | Close as duplicate of #3218 | stateReason=not_planned |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3281,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T11:25:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter requests that Control Flow Guard (CFG) be enabled in libSkiaSharp.dll, libHarfBuzzSharp.dll, and SkiaSharp.Views.WinUI.Native.dll on Windows, citing BinSkim rule BA2008; this is a duplicate of #3218 which covers the same CFG gap.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.9
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
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "BinSkim BA2008: libSkiaSharp.dll, libHarfBuzzSharp.dll, SkiaSharp.Views.WinUI.Native.dll do not enable Control Flow Guard",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "environmentDetails": "Windows, Visual Studio, SkiaSharp 3.116.0",
      "relatedIssues": [
        3218
      ],
      "repoLinks": [
        {
          "url": "https://github.com/microsoft/binskim/blob/main/docs/BinSkimRules.md#rule-BA2008EnableControlFlowGuard",
          "description": "BinSkim BA2008 rule documentation"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3218",
          "description": "Duplicate issue #3218 — same CFG complaint filed 2025-03-28, already triaged"
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
      "relevanceReason": "Code investigation shows /guard:cf is now in extra_cflags/extra_ldflags in native/windows/build.cake and ControlFlowGuard=Guard is set in libHarfBuzzSharp.vcxproj and SkiaSharp.Views.WinUI.Native.vcxproj — the fix has been applied in the current main branch."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.85,
      "reason": "All three DLL build configurations in the current main branch include CFG compiler and linker flags. However, the parent issue #3218 is still open, suggesting a release has not yet been cut.",
      "relatedPRs": []
    }
  },
  "analysis": {
    "summary": "Duplicate of #3218. Reporter identified missing CFG security flag across three Windows native DLLs using BinSkim BA2008. Code investigation confirms CFG has already been added to all three build configurations in the current main branch: /guard:cf in native/windows/build.cake for libSkiaSharp.dll, and ControlFlowGuard=Guard in the .vcxproj files for libHarfBuzzSharp.dll and SkiaSharp.Views.WinUI.Native.dll.",
    "rationale": "Issue #3281 is materially identical to #3218 (98% similarity per bot, same BinSkim rule, same version range, same fix suggestion). The current codebase already has CFG enabled in all three DLL build configs. Closing as duplicate concentrates discussion on the original tracking issue.",
    "codeInvestigation": [
      {
        "file": "native/windows/build.cake",
        "lines": "79-80",
        "finding": "libSkiaSharp.dll build uses extra_cflags with '/guard:cf' and extra_ldflags with '/guard:cf' — CFG is enabled for libSkiaSharp.dll in the current main branch",
        "relevance": "direct"
      },
      {
        "file": "native/windows/libHarfBuzzSharp/libHarfBuzzSharp.vcxproj",
        "lines": "148,154",
        "finding": "All Release/Debug x86, x64, ARM64 configurations include <ControlFlowGuard>Guard</ControlFlowGuard> in ClCompile and <ControlFlowGuard>true</ControlFlowGuard> in Link — CFG is enabled for libHarfBuzzSharp.dll",
        "relevance": "direct"
      },
      {
        "file": "native/winui/SkiaSharp.Views.WinUI.Native/SkiaSharp.Views.WinUI.Native/SkiaSharp.Views.WinUI.Native.vcxproj",
        "lines": "115,122",
        "finding": "ClCompile includes <ControlFlowGuard>Guard</ControlFlowGuard> and Link includes <ControlFlowGuard>true</ControlFlowGuard> — CFG is enabled for SkiaSharp.Views.WinUI.Native.dll",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "libSkiaSharp.dll, libHarfBuzzSharp.dll, and SkiaSharp.Views.WinUI.Native.dll do not enable the control flow guard (CFG)",
        "source": "issue title",
        "interpretation": "Reporter is using BinSkim to audit native Windows DLLs and found CFG not set"
      },
      {
        "text": "We've found some similar issues: #3218, similarity score: 98%",
        "source": "similar-issues-ai[bot] comment",
        "interpretation": "High-confidence duplicate signal — #3218 covers the identical CFG gap"
      },
      {
        "text": "Last Known Good Version: 2.88.9 (Previous)",
        "source": "issue body",
        "interpretation": "Reporter believes CFG was present in 2.88.x, which aligns with the 3.x native build refactor being the likely introduction point"
      }
    ],
    "nextQuestions": [
      "When will the CFG fix in main be included in a released NuGet package?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-duplicate",
      "confidence": 0.95,
      "reason": "Identical BinSkim BA2008 finding as #3218, same version range, same fix suggestion. CFG is already enabled in the current main branch for all three DLLs. Duplicate discussion should continue on #3218.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/libSkiaSharp.native, os/Windows-Classic, tenet/reliability labels",
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
        "type": "link-duplicate",
        "description": "Mark as duplicate of #3218 (same CFG issue, filed earlier, already triaged)",
        "risk": "medium",
        "confidence": 0.95,
        "linkedIssue": 3218
      },
      {
        "type": "close-issue",
        "description": "Close as duplicate of #3218",
        "risk": "medium",
        "confidence": 0.9,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
