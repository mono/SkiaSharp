# Issue Triage Report — #3972

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-15T05:29:22Z |
| Type | type/bug (0.98 (98%)) |
| Area | area/libSkiaSharp.native (0.97 (97%)) |
| Suggested action | close-as-fixed (0.92 (92%)) |

**Issue Summary:** libSkiaSharp.so fails to load on Linux LoongArch64 with undefined symbol png_init_filter_functions_lsx; the fix was already merged into mono/skia (PR #169) and released in 3.119.4-preview.1.1, confirmed working by reporter.

**Analysis:** libSkiaSharp.so on LoongArch64 contained a reference to png_init_filter_functions_lsx (a LoongArch LSX SIMD intrinsic) from the bundled libpng, but the symbol was not present on all LoongArch64 systems. The fix in mono/skia PR #169 (upstream Skia commit 3742756) guards the LSX code path properly. The fix shipped in 3.119.4-preview.1.1 and was confirmed working.

**Recommendations:** **close-as-fixed** — Reporter confirmed issue resolved in 3.119.4-preview.1.1; fix merged in mono/skia PR #169. No action needed on the SkiaSharp side beyond awaiting stable release.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Install SkiaSharp 3.119.2 on Linux LoongArch64 (Debian loong64 / AOSC OS, .NET 10)
2. Run any Avalonia/SkiaSharp application
3. Observe symbol lookup error on libSkiaSharp.so

**Environment:** Linux LoongArch64, Debian 13 / AOSC OS, .NET 10 loongarch64 SDK, SkiaSharp 3.119.2

**Related issues:** #3230

**Repository links:**
- https://github.com/mono/skia/pull/169 — mono/skia PR #169 — fix for LoongArch libpng LSX unresolved symbol (references Skia upstream commit 3742756)
- https://github.com/mono/SkiaSharp/issues/3230 — Related closed feature request: Loongson LSX support merged upstream

**Attachments:**
-  — https://github.com/user-attachments/assets/d1ee05a2-a195-416c-b3e6-d7dafb8feebc — Terminal showing symbol lookup error
-  — https://github.com/user-attachments/assets/359507b4-6cda-4124-a308-e1acd257cea7 — v2rayN running successfully on LoongArch64 after fix

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | symbol lookup error: /opt/v2rayN/libSkiaSharp.so: undefined symbol: png_init_filter_functions_lsx |
| Repro quality | complete |
| Target frameworks | net10.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.119.2, 3.119.4-preview.1.1 |
| Worked in | 3.119.4-preview.1.1 |
| Broke in | 3.119.2 |
| Current relevance | unlikely |
| Relevance reason | Reporter confirmed fix works in 3.119.4-preview.1.1; the stable release of that fix is pending. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.97 (97%) |
| Reason | Reporter explicitly confirmed that upgrading to 3.119.4-preview.1.1 resolved the issue. The fix was merged in mono/skia PR #169. |
| Related PRs | #https://github.com/mono/skia/pull/169 |
| Related commits | — |
| Fixed in version | 3.119.4-preview.1.1 |

## Analysis

### Technical Summary

libSkiaSharp.so on LoongArch64 contained a reference to png_init_filter_functions_lsx (a LoongArch LSX SIMD intrinsic) from the bundled libpng, but the symbol was not present on all LoongArch64 systems. The fix in mono/skia PR #169 (upstream Skia commit 3742756) guards the LSX code path properly. The fix shipped in 3.119.4-preview.1.1 and was confirmed working.

### Rationale

This is clearly a native library bug (area/libSkiaSharp.native) specific to Linux LoongArch64. The undefined symbol prevents the shared library from loading, causing a hard crash on startup. The fix is already available in a preview release and confirmed by the reporter. Suggesting close-as-fixed with a note to await stable release.

### Key Signals

- "symbol lookup error: /opt/v2rayN/libSkiaSharp.so: undefined symbol: png_init_filter_functions_lsx" — **issue body / log output** (Native library fails to load due to unresolved LoongArch LSX symbol from bundled libpng.)
- "This appears to be the same LoongArch libpng LSX issue fixed upstream by Skia commit 3742756 and proposed for mono/skia in PR: https://github.com/mono/skia/pull/169" — **issue body** (Reporter correctly identified the upstream fix and the corresponding mono/skia PR.)
- "Use 3.119.4-preview.1.1, This version solved LSX Problem" — **comment by @4Darmygeometry** (Fix already available in preview; confirms issue is resolved in newer build.)
- "Thanks, the issue has been resolved. v2rayN on LoongArch64 can now run normally." — **comment by original reporter @xujiegb** (Reporter confirmed the fix works, requesting a stable release.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.Linux/SkiaSharp.NativeAssets.Linux.csproj` | — | context | Package includes runtimes/linux-loongarch64/native/libSkiaSharp*.so — confirms LoongArch64 is a supported/shipped platform. |
| `native/linux/build.cake` | 23-25 | related | Build maps 'loongarch64' to GN target 'loong64'. The LoongArch64 native is built and shipped. The undefined symbol originates in bundled libpng within the skia submodule. |

### Next Questions

- When will 3.119.4 stable be released to pick up the libpng LSX fix?
- Should the issue be closed now that the preview fix is confirmed, or kept open until stable ships?

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.92 (92%) |
| Reason | Reporter confirmed issue resolved in 3.119.4-preview.1.1; fix merged in mono/skia PR #169. No action needed on the SkiaSharp side beyond awaiting stable release. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply native area and Linux platform labels | labels=type/bug, area/libSkiaSharp.native, os/Linux, tenet/compatibility |
| add-comment | medium | 0.92 (92%) | Confirm fix is in preview and advise on stable release | — |
| close-issue | medium | 0.90 (90%) | Close as fixed — fix confirmed in preview release | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report and for confirming the fix! The LoongArch64 libpng LSX symbol issue was resolved in [mono/skia PR #169](https://github.com/mono/skia/pull/169) and is available in **3.119.4-preview.1.1**. A stable release incorporating this fix is planned. In the meantime, you can use `3.119.4-preview.1.1` as a workaround.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3972,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-15T05:29:22Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "libSkiaSharp.so fails to load on Linux LoongArch64 with undefined symbol png_init_filter_functions_lsx; the fix was already merged into mono/skia (PR #169) and released in 3.119.4-preview.1.1, confirmed working by reporter.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.98
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.97
    },
    "platforms": [
      "os/Linux"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "symbol lookup error: /opt/v2rayN/libSkiaSharp.so: undefined symbol: png_init_filter_functions_lsx",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net10.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Install SkiaSharp 3.119.2 on Linux LoongArch64 (Debian loong64 / AOSC OS, .NET 10)",
        "Run any Avalonia/SkiaSharp application",
        "Observe symbol lookup error on libSkiaSharp.so"
      ],
      "environmentDetails": "Linux LoongArch64, Debian 13 / AOSC OS, .NET 10 loongarch64 SDK, SkiaSharp 3.119.2",
      "attachments": [
        {
          "type": "screenshot",
          "url": "https://github.com/user-attachments/assets/d1ee05a2-a195-416c-b3e6-d7dafb8feebc",
          "description": "Terminal showing symbol lookup error"
        },
        {
          "type": "screenshot",
          "url": "https://github.com/user-attachments/assets/359507b4-6cda-4124-a308-e1acd257cea7",
          "description": "v2rayN running successfully on LoongArch64 after fix"
        }
      ],
      "relatedIssues": [
        3230
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/skia/pull/169",
          "description": "mono/skia PR #169 — fix for LoongArch libpng LSX unresolved symbol (references Skia upstream commit 3742756)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3230",
          "description": "Related closed feature request: Loongson LSX support merged upstream"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.119.2",
        "3.119.4-preview.1.1"
      ],
      "workedIn": "3.119.4-preview.1.1",
      "brokeIn": "3.119.2",
      "currentRelevance": "unlikely",
      "relevanceReason": "Reporter confirmed fix works in 3.119.4-preview.1.1; the stable release of that fix is pending."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.97,
      "reason": "Reporter explicitly confirmed that upgrading to 3.119.4-preview.1.1 resolved the issue. The fix was merged in mono/skia PR #169.",
      "relatedPRs": [
        "https://github.com/mono/skia/pull/169"
      ],
      "fixedInVersion": "3.119.4-preview.1.1"
    }
  },
  "analysis": {
    "summary": "libSkiaSharp.so on LoongArch64 contained a reference to png_init_filter_functions_lsx (a LoongArch LSX SIMD intrinsic) from the bundled libpng, but the symbol was not present on all LoongArch64 systems. The fix in mono/skia PR #169 (upstream Skia commit 3742756) guards the LSX code path properly. The fix shipped in 3.119.4-preview.1.1 and was confirmed working.",
    "rationale": "This is clearly a native library bug (area/libSkiaSharp.native) specific to Linux LoongArch64. The undefined symbol prevents the shared library from loading, causing a hard crash on startup. The fix is already available in a preview release and confirmed by the reporter. Suggesting close-as-fixed with a note to await stable release.",
    "keySignals": [
      {
        "text": "symbol lookup error: /opt/v2rayN/libSkiaSharp.so: undefined symbol: png_init_filter_functions_lsx",
        "source": "issue body / log output",
        "interpretation": "Native library fails to load due to unresolved LoongArch LSX symbol from bundled libpng."
      },
      {
        "text": "This appears to be the same LoongArch libpng LSX issue fixed upstream by Skia commit 3742756 and proposed for mono/skia in PR: https://github.com/mono/skia/pull/169",
        "source": "issue body",
        "interpretation": "Reporter correctly identified the upstream fix and the corresponding mono/skia PR."
      },
      {
        "text": "Use 3.119.4-preview.1.1, This version solved LSX Problem",
        "source": "comment by @4Darmygeometry",
        "interpretation": "Fix already available in preview; confirms issue is resolved in newer build."
      },
      {
        "text": "Thanks, the issue has been resolved. v2rayN on LoongArch64 can now run normally.",
        "source": "comment by original reporter @xujiegb",
        "interpretation": "Reporter confirmed the fix works, requesting a stable release."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.Linux/SkiaSharp.NativeAssets.Linux.csproj",
        "finding": "Package includes runtimes/linux-loongarch64/native/libSkiaSharp*.so — confirms LoongArch64 is a supported/shipped platform.",
        "relevance": "context"
      },
      {
        "file": "native/linux/build.cake",
        "lines": "23-25",
        "finding": "Build maps 'loongarch64' to GN target 'loong64'. The LoongArch64 native is built and shipped. The undefined symbol originates in bundled libpng within the skia submodule.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "When will 3.119.4 stable be released to pick up the libpng LSX fix?",
      "Should the issue be closed now that the preview fix is confirmed, or kept open until stable ships?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.92,
      "reason": "Reporter confirmed issue resolved in 3.119.4-preview.1.1; fix merged in mono/skia PR #169. No action needed on the SkiaSharp side beyond awaiting stable release.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply native area and Linux platform labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Linux",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Confirm fix is in preview and advise on stable release",
        "risk": "medium",
        "confidence": 0.92,
        "comment": "Thanks for the report and for confirming the fix! The LoongArch64 libpng LSX symbol issue was resolved in [mono/skia PR #169](https://github.com/mono/skia/pull/169) and is available in **3.119.4-preview.1.1**. A stable release incorporating this fix is planned. In the meantime, you can use `3.119.4-preview.1.1` as a workaround."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed — fix confirmed in preview release",
        "risk": "medium",
        "confidence": 0.9,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
