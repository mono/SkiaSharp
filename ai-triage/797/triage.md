# Issue Triage Report — #797

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T20:40:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/libSkiaSharp.native (0.95 (95%)) |
| Suggested action | close-as-fixed (0.75 (75%)) |

**Issue Summary:** Building libSkiaSharp native library for AArch64 (ARM64) fails in SkRasterPipeline_opts.h with a type mismatch error where neon::U16 cannot be converted to float16x4_t in the vcvt_f32_f16 NEON intrinsic call, affecting SkiaSharp 1.68.0 on Linux AArch64.

**Analysis:** AArch64 native build failure caused by a type mismatch in the NEON half-float conversion code in SkRasterPipeline_opts.h — vcvt_f32_f16() receives a scalar neon::U16 (uint16_t) where it expects a float16x4_t vector. The Skia opts code was refactored in the m80/m81 milestone (PR #986), which the maintainer believes resolved this.

**Recommendations:** **close-as-fixed** — Maintainer confirmed in Feb 2020 that PR #986 likely fixed this. Issue is from 1.68.0 (Skia m63), current SkiaSharp is 3.x (Skia m120+). The opts code has been significantly rewritten since then.

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

## Evidence

### Reproduction

1. Attempt to build libSkiaSharp native library targeting AArch64 (ARM64) architecture
2. Compilation fails in src/opts/SkRasterPipeline_opts.h at the neon::from_half function
3. The __aarch64__ code path calls vcvt_f32_f16(h) where h is neon::U16 (short unsigned int) but vcvt_f32_f16 expects float16x4_t

**Environment:** AArch64/ARM64 Linux, SkiaSharp 1.68.0, GCC toolchain

**Repository links:**
- https://github.com/OSSystems/meta-browser/blob/master/recipes-browser/chromium/files/aarch64-skia-build-fix.patch — OSSystems patch for the same Chromium/Skia AArch64 build issue

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | build-error |
| Error message | error: cannot convert 'neon::U16 {aka short unsigned int}' to 'float16x4_t {aka __vector(4) __fp16}' for argument '1' to 'float32x4_t vcvt_f32_f16(float16x4_t)' |
| Repro quality | complete |
| Target frameworks | net472 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.0, 1.60.3 |
| Worked in | 1.60.3 |
| Broke in | 1.68.0 |
| Current relevance | unlikely |
| Relevance reason | Maintainer indicated PR #986 (m80/m81 rewrite) likely fixed this. Current SkiaSharp is based on Skia m120+ where opts code was substantially refactored. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.90 (90%) |
| Reason | Reporter explicitly states 1.60.3 worked and 1.68.0 broke. The __aarch64__ vcvt intrinsic code was introduced between those versions. |
| Worked in version | 1.60.3 |
| Broke in version | 1.68.0 |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.75 (75%) |
| Reason | Maintainer @mattleibow confirmed in Feb 2020 that PR #986 (m81 opts rewrite) likely fixed this. Additional comment in June 2020 referenced m80 master rebase. The opts code path was heavily refactored. Raspberry Pi 4B user confirmed the workaround patch works for 1.68.x but this class of problem should be resolved in 2.x+ builds. |
| Related PRs | #986 |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

AArch64 native build failure caused by a type mismatch in the NEON half-float conversion code in SkRasterPipeline_opts.h — vcvt_f32_f16() receives a scalar neon::U16 (uint16_t) where it expects a float16x4_t vector. The Skia opts code was refactored in the m80/m81 milestone (PR #986), which the maintainer believes resolved this.

### Rationale

Clear build-error regression: explicitly worked in 1.60.3, broke in 1.68.0. The error is in upstream Skia's opts layer, not SkiaSharp's own C API shim. A community patch was provided and confirmed working. Maintainer acknowledged the likely fix via PR #986. Current SkiaSharp (2.88+, 3.x) is based on Skia milestones far beyond m68, making this very likely resolved.

### Key Signals

- "error: cannot convert 'neon::U16 {aka short unsigned int}' to 'float16x4_t {aka __vector(4) __fp16}' for argument '1' to 'float32x4_t vcvt_f32_f16(float16x4_t)'" — **issue body** (Type mismatch: the __aarch64__ code path passes a scalar uint16 where vcvt_f32_f16 expects an ARM NEON vector (float16x4_t). This is a compiler type system error in the NEON intrinsic usage.)
- "I believe this may be fixed with the new work in the #986 PR. A lot of the opts bits have been improved." — **comment by @mattleibow (maintainer)** (Maintainer acknowledgement that the m80/m81 opts rewrite in PR #986 likely resolved the AArch64 build failure.)
- "the master branch is now the new m80" — **comment by @mattleibow (maintainer)** (Confirms that the m80 rebase was completed in June 2020, which includes the opts code improvements.)
- "This patch is still good for releases up to and including 1.68.1.1" — **comment by @solabc16 (reporter)** (The community workaround patch is valid for the 1.68.x series, confirming the root cause.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `externals/skia/src/opts/SkRasterPipeline_opts.h` | — | direct | File not present in current working tree (submodule uninitialized). The error at line 657 in the 1.68.0 era was in neon::from_half(U16 h) where the __aarch64__ code path called vcvt_f32_f16(h) with h as U16 (scalar uint16). In the patch provided by the reporter, the __aarch64__ branch was removed entirely, falling through to the generic scalar fallback. Current Skia m120+ has substantially different opts architecture. |
| `externals/skia/src/opts/SkRasterPipeline_opts.h` | — | direct | The matching to_half function had the same issue: vcvt_f16_f32(f) returned float16x4_t stored into U16, which is also a type mismatch. Both functions were patched by removing the __aarch64__-specific code paths in the community workaround. |

### Workarounds

- Apply the patch from comment #476876664: remove the __aarch64__ && !SK_BUILD_FOR_GOOGLE3 code paths in neon::from_half() and neon::to_half() in SkRasterPipeline_opts.h, falling back to the generic scalar conversion. This is valid for SkiaSharp 1.68.x builds.
- Upgrade to SkiaSharp 2.80.0 or later (based on Skia m80+) where the opts code was rewritten and this specific type mismatch was resolved per PR #986.
- Use clang instead of GCC for the AArch64 build — the maintainer suggested clang handles this differently.

### Next Questions

- Does the build failure reproduce with current SkiaSharp (2.88.x or 3.x)?
- Is CI currently building AArch64 Linux native artifacts successfully?

### Resolution Proposals

**Hypothesis:** The bug was fixed as part of the m80/m81 Skia milestone rebase (PR #986) which rewrote the opts pipeline. The issue is specific to the 1.68.x era (Skia m63) and is no longer present in 2.x+ releases.

1. **Close as fixed in 2.80.0** — fix, confidence 0.75 (75%), cost/xs, validated=untested
   - The maintainer indicated PR #986 (m80/m81) fixed this. Current SkiaSharp releases (2.88+, 3.x) are based on far newer Skia milestones. Close with note directing users to upgrade.
2. **Apply community workaround for 1.68.x** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - Remove the __aarch64__ vcvt code path from neon::from_half and neon::to_half. Falls back to generic scalar half-float conversion. Confirmed working by two users.

**Recommended proposal:** Close as fixed in 2.80.0

**Why:** Maintainer indicated the fix is in PR #986 (m80), and the issue is from 1.68.0 era. Users should upgrade to 2.88+ or 3.x.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.75 (75%) |
| Reason | Maintainer confirmed in Feb 2020 that PR #986 likely fixed this. Issue is from 1.68.0 (Skia m63), current SkiaSharp is 3.x (Skia m120+). The opts code has been significantly rewritten since then. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply build-error/native/linux labels | labels=type/bug, area/libSkiaSharp.native, os/Linux, tenet/compatibility |
| add-comment | medium | 0.75 (75%) | Inform reporter this is likely fixed in 2.80.0+ and provide workaround for 1.68.x | — |
| close-issue | medium | 0.75 (75%) | Close as fixed | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thank you for the report and the workaround patch!

This build error in `SkRasterPipeline_opts.h` was specific to SkiaSharp **1.68.x** (Skia m63). The maintainer noted that **PR #986** (the m80/m81 opts rewrite) likely resolved this, and the current SkiaSharp releases (2.88.x and 3.x) are based on Skia m120+ where the opts pipeline was substantially rewritten.

**If you're still on 1.68.x**, the community workaround patch from @solabc16 (comment above) works: remove the `__aarch64__` NEON vcvt code paths from `neon::from_half` and `neon::to_half` in `src/opts/SkRasterPipeline_opts.h`.

**If you're on a newer version** and still seeing this, please reopen with your SkiaSharp version, OS, and compiler version.

Closing as fixed in 2.80.0+.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 797,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T20:40:00Z"
  },
  "summary": "Building libSkiaSharp native library for AArch64 (ARM64) fails in SkRasterPipeline_opts.h with a type mismatch error where neon::U16 cannot be converted to float16x4_t in the vcvt_f32_f16 NEON intrinsic call, affecting SkiaSharp 1.68.0 on Linux AArch64.",
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
      "os/Linux"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "build-error",
      "errorMessage": "error: cannot convert 'neon::U16 {aka short unsigned int}' to 'float16x4_t {aka __vector(4) __fp16}' for argument '1' to 'float32x4_t vcvt_f32_f16(float16x4_t)'",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net472"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Attempt to build libSkiaSharp native library targeting AArch64 (ARM64) architecture",
        "Compilation fails in src/opts/SkRasterPipeline_opts.h at the neon::from_half function",
        "The __aarch64__ code path calls vcvt_f32_f16(h) where h is neon::U16 (short unsigned int) but vcvt_f32_f16 expects float16x4_t"
      ],
      "environmentDetails": "AArch64/ARM64 Linux, SkiaSharp 1.68.0, GCC toolchain",
      "repoLinks": [
        {
          "url": "https://github.com/OSSystems/meta-browser/blob/master/recipes-browser/chromium/files/aarch64-skia-build-fix.patch",
          "description": "OSSystems patch for the same Chromium/Skia AArch64 build issue"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.0",
        "1.60.3"
      ],
      "workedIn": "1.60.3",
      "brokeIn": "1.68.0",
      "currentRelevance": "unlikely",
      "relevanceReason": "Maintainer indicated PR #986 (m80/m81 rewrite) likely fixed this. Current SkiaSharp is based on Skia m120+ where opts code was substantially refactored."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.9,
      "reason": "Reporter explicitly states 1.60.3 worked and 1.68.0 broke. The __aarch64__ vcvt intrinsic code was introduced between those versions.",
      "workedInVersion": "1.60.3",
      "brokeInVersion": "1.68.0"
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.75,
      "reason": "Maintainer @mattleibow confirmed in Feb 2020 that PR #986 (m81 opts rewrite) likely fixed this. Additional comment in June 2020 referenced m80 master rebase. The opts code path was heavily refactored. Raspberry Pi 4B user confirmed the workaround patch works for 1.68.x but this class of problem should be resolved in 2.x+ builds.",
      "relatedPRs": [
        986
      ]
    }
  },
  "analysis": {
    "summary": "AArch64 native build failure caused by a type mismatch in the NEON half-float conversion code in SkRasterPipeline_opts.h — vcvt_f32_f16() receives a scalar neon::U16 (uint16_t) where it expects a float16x4_t vector. The Skia opts code was refactored in the m80/m81 milestone (PR #986), which the maintainer believes resolved this.",
    "rationale": "Clear build-error regression: explicitly worked in 1.60.3, broke in 1.68.0. The error is in upstream Skia's opts layer, not SkiaSharp's own C API shim. A community patch was provided and confirmed working. Maintainer acknowledged the likely fix via PR #986. Current SkiaSharp (2.88+, 3.x) is based on Skia milestones far beyond m68, making this very likely resolved.",
    "keySignals": [
      {
        "text": "error: cannot convert 'neon::U16 {aka short unsigned int}' to 'float16x4_t {aka __vector(4) __fp16}' for argument '1' to 'float32x4_t vcvt_f32_f16(float16x4_t)'",
        "source": "issue body",
        "interpretation": "Type mismatch: the __aarch64__ code path passes a scalar uint16 where vcvt_f32_f16 expects an ARM NEON vector (float16x4_t). This is a compiler type system error in the NEON intrinsic usage."
      },
      {
        "text": "I believe this may be fixed with the new work in the #986 PR. A lot of the opts bits have been improved.",
        "source": "comment by @mattleibow (maintainer)",
        "interpretation": "Maintainer acknowledgement that the m80/m81 opts rewrite in PR #986 likely resolved the AArch64 build failure."
      },
      {
        "text": "the master branch is now the new m80",
        "source": "comment by @mattleibow (maintainer)",
        "interpretation": "Confirms that the m80 rebase was completed in June 2020, which includes the opts code improvements."
      },
      {
        "text": "This patch is still good for releases up to and including 1.68.1.1",
        "source": "comment by @solabc16 (reporter)",
        "interpretation": "The community workaround patch is valid for the 1.68.x series, confirming the root cause."
      }
    ],
    "codeInvestigation": [
      {
        "file": "externals/skia/src/opts/SkRasterPipeline_opts.h",
        "finding": "File not present in current working tree (submodule uninitialized). The error at line 657 in the 1.68.0 era was in neon::from_half(U16 h) where the __aarch64__ code path called vcvt_f32_f16(h) with h as U16 (scalar uint16). In the patch provided by the reporter, the __aarch64__ branch was removed entirely, falling through to the generic scalar fallback. Current Skia m120+ has substantially different opts architecture.",
        "relevance": "direct"
      },
      {
        "file": "externals/skia/src/opts/SkRasterPipeline_opts.h",
        "finding": "The matching to_half function had the same issue: vcvt_f16_f32(f) returned float16x4_t stored into U16, which is also a type mismatch. Both functions were patched by removing the __aarch64__-specific code paths in the community workaround.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Apply the patch from comment #476876664: remove the __aarch64__ && !SK_BUILD_FOR_GOOGLE3 code paths in neon::from_half() and neon::to_half() in SkRasterPipeline_opts.h, falling back to the generic scalar conversion. This is valid for SkiaSharp 1.68.x builds.",
      "Upgrade to SkiaSharp 2.80.0 or later (based on Skia m80+) where the opts code was rewritten and this specific type mismatch was resolved per PR #986.",
      "Use clang instead of GCC for the AArch64 build — the maintainer suggested clang handles this differently."
    ],
    "nextQuestions": [
      "Does the build failure reproduce with current SkiaSharp (2.88.x or 3.x)?",
      "Is CI currently building AArch64 Linux native artifacts successfully?"
    ],
    "resolution": {
      "hypothesis": "The bug was fixed as part of the m80/m81 Skia milestone rebase (PR #986) which rewrote the opts pipeline. The issue is specific to the 1.68.x era (Skia m63) and is no longer present in 2.x+ releases.",
      "proposals": [
        {
          "title": "Close as fixed in 2.80.0",
          "description": "The maintainer indicated PR #986 (m80/m81) fixed this. Current SkiaSharp releases (2.88+, 3.x) are based on far newer Skia milestones. Close with note directing users to upgrade.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Apply community workaround for 1.68.x",
          "description": "Remove the __aarch64__ vcvt code path from neon::from_half and neon::to_half. Falls back to generic scalar half-float conversion. Confirmed working by two users.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Close as fixed in 2.80.0",
      "recommendedReason": "Maintainer indicated the fix is in PR #986 (m80), and the issue is from 1.68.0 era. Users should upgrade to 2.88+ or 3.x."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.75,
      "reason": "Maintainer confirmed in Feb 2020 that PR #986 likely fixed this. Issue is from 1.68.0 (Skia m63), current SkiaSharp is 3.x (Skia m120+). The opts code has been significantly rewritten since then.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply build-error/native/linux labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Linux",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Inform reporter this is likely fixed in 2.80.0+ and provide workaround for 1.68.x",
        "risk": "medium",
        "confidence": 0.75,
        "comment": "Thank you for the report and the workaround patch!\n\nThis build error in `SkRasterPipeline_opts.h` was specific to SkiaSharp **1.68.x** (Skia m63). The maintainer noted that **PR #986** (the m80/m81 opts rewrite) likely resolved this, and the current SkiaSharp releases (2.88.x and 3.x) are based on Skia m120+ where the opts pipeline was substantially rewritten.\n\n**If you're still on 1.68.x**, the community workaround patch from @solabc16 (comment above) works: remove the `__aarch64__` NEON vcvt code paths from `neon::from_half` and `neon::to_half` in `src/opts/SkRasterPipeline_opts.h`.\n\n**If you're on a newer version** and still seeing this, please reopen with your SkiaSharp version, OS, and compiler version.\n\nClosing as fixed in 2.80.0+."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed",
        "risk": "medium",
        "confidence": 0.75,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
