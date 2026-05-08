# Issue Triage Report — #1337

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T04:05:00Z |
| Type | type/enhancement (0.85 (85%)) |
| Area | area/libSkiaSharp.native (0.92 (92%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** Reporter identified that libSkiaSharp.so on Android does not enforce full RELRO (Relocation Read-Only) memory protection; the Skia upstream team confirmed this is SkiaSharp's decision, and the fix requires adding -Wl,-z,relro,-z,now to the Android build linker flags.

**Analysis:** The Android native build of libSkiaSharp.so omits -Wl,-z,relro,-z,now linker flags, which would enforce full RELRO memory protection. Security scanners flag this as a hardening gap. The Skia upstream team confirmed this is a SkiaSharp build decision. The fix is a one-line change to native/android/build.cake, but it introduces a startup performance cost that has not been benchmarked.

**Recommendations:** **keep-open** — Valid security hardening enhancement with a known one-line implementation. Requires maintainer decision on performance trade-off and whether to apply full vs partial RELRO. Issue should stay open until explicitly accepted or declined.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/libSkiaSharp.native |
| Platforms | os/Android |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a Xamarin.Forms project (XF 3.3 or 4.5+)
2. Add SkiaSharp 1.68
3. Build an Android APK with Xamarin.Android 9.2
4. Test the APK with the Ostorlab.co security scanner

**Environment:** Xamarin.Forms 3.3/4.5+, SkiaSharp 1.68, Xamarin.Android 9.2

**Repository links:**
- https://bugs.chromium.org/p/skia/issues/detail?id=10388 — Skia upstream issue — team responded that RELRO enforcement is SkiaSharp's decision
- https://github.com/mono/mono/issues/19976 — Similar RELRO issue reported against the mono runtime library

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | native/android/build.cake extra_ldflags still only contains -Wl,-z,max-page-size=16384 and does not include RELRO flags in the current codebase. |

## Analysis

### Technical Summary

The Android native build of libSkiaSharp.so omits -Wl,-z,relro,-z,now linker flags, which would enforce full RELRO memory protection. Security scanners flag this as a hardening gap. The Skia upstream team confirmed this is a SkiaSharp build decision. The fix is a one-line change to native/android/build.cake, but it introduces a startup performance cost that has not been benchmarked.

### Rationale

Classified as type/enhancement because the library functions correctly — RELRO is an optional security hardening feature, not a correctness requirement. The implementation path is well-understood (add linker flags), but the performance trade-off warrants a maintainer decision before merging. The Skia team explicitly deferred this to SkiaSharp.

### Key Signals

- "libSkiaSharp.so does not enforce full RELRO" — **issue title and Ostorlab security scan screenshot** (Confirmed missing hardening flag in Android shared library build.)
- "This is the sort of thing that's up to SkiaSharp to decide for itself" — **Skia upstream team response at bugs.chromium.org/p/skia/issues/detail?id=10388 (comment #9, reported by LamijaV)** (Upstream Skia explicitly delegated the RELRO decision to SkiaSharp maintainers.)
- "Just add `-Wl,-z,relro,-z,now` to native/android/build.cake#L40 and use your own build" — **comment #issuecomment-644594804 by @Gillibald** (The fix is a known one-liner; performance cost at startup is the only trade-off.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `native/android/build.cake` | 42 | direct | extra_ldflags currently only contains '-Wl,-z,max-page-size=16384'; no RELRO or BIND_NOW flags are present. Adding '-Wl,-z,relro,-z,now' would enable full RELRO for all Android architectures built in this task. |

### Workarounds

- Build SkiaSharp from source and add '-Wl,-z,relro,-z,now' to extra_ldflags in native/android/build.cake. This is not suitable as a long-term solution using official releases but can satisfy a pen-test requirement.

### Next Questions

- What is the startup-time performance impact of enabling full RELRO (BIND_NOW) on each Android ABI? Benchmark data needed before committing to the change.
- Should RELRO flags also be applied to libHarfBuzzSharp.so built via NDK in the same build.cake?

### Resolution Proposals

**Hypothesis:** Adding '-Wl,-z,relro,-z,now' to extra_ldflags in native/android/build.cake will enable full RELRO on libSkiaSharp.so for all Android architectures.

1. **Add RELRO linker flags to Android build** — fix, confidence 0.85 (85%), cost/xs, validated=untested
   - Append '-Wl,-z,relro,-z,now' to the extra_ldflags array in native/android/build.cake. This makes the GOT read-only after startup (full RELRO). A startup-time benchmark should be run before merging to quantify the performance impact.
2. **Enable partial RELRO only** — alternative, confidence 0.70 (70%), cost/xs, validated=untested
   - Use '-Wl,-z,relro' without '-z,now' for partial RELRO. This avoids forcing all symbol resolution at load time and reduces the performance hit, while still protecting read-only data segments. Many security scanners accept partial RELRO.

**Recommended proposal:** Add RELRO linker flags to Android build

**Why:** One-line fix with known implementation. Skia upstream team delegated the decision to SkiaSharp. Full RELRO (-z,relro,-z,now) provides the strongest hardening, though partial RELRO is a lower-risk starting point.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Valid security hardening enhancement with a known one-line implementation. Requires maintainer decision on performance trade-off and whether to apply full vs partial RELRO. Issue should stay open until explicitly accepted or declined. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Apply enhancement, native library, Android platform, and reliability tenet labels | labels=type/enhancement, area/libSkiaSharp.native, os/Android, tenet/reliability |
| add-comment | medium | 0.80 (80%) | Acknowledge the security concern, explain the current state, and communicate the decision being tracked | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for investigating this and following up with the Skia team.

This is a valid security hardening request. The fix is well-understood: adding `-Wl,-z,relro,-z,now` to `extra_ldflags` in `native/android/build.cake` would enable full RELRO on `libSkiaSharp.so`. The Skia upstream team confirmed this is SkiaSharp's decision to make.

The main consideration before enabling this by default is the startup-time performance cost — RELRO with `BIND_NOW` forces all symbol resolution at load time rather than lazily. We haven't benchmarked this impact on the Android targets we ship.

As a short-term workaround, you can build SkiaSharp from source and add the flags yourself. We're tracking this as an enhancement to evaluate and implement with proper benchmarking.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1337,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T04:05:00Z"
  },
  "summary": "Reporter identified that libSkiaSharp.so on Android does not enforce full RELRO (Relocation Read-Only) memory protection; the Skia upstream team confirmed this is SkiaSharp's decision, and the fix requires adding -Wl,-z,relro,-z,now to the Android build linker flags.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.85
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.92
    },
    "platforms": [
      "os/Android"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Forms project (XF 3.3 or 4.5+)",
        "Add SkiaSharp 1.68",
        "Build an Android APK with Xamarin.Android 9.2",
        "Test the APK with the Ostorlab.co security scanner"
      ],
      "environmentDetails": "Xamarin.Forms 3.3/4.5+, SkiaSharp 1.68, Xamarin.Android 9.2",
      "repoLinks": [
        {
          "url": "https://bugs.chromium.org/p/skia/issues/detail?id=10388",
          "description": "Skia upstream issue — team responded that RELRO enforcement is SkiaSharp's decision"
        },
        {
          "url": "https://github.com/mono/mono/issues/19976",
          "description": "Similar RELRO issue reported against the mono runtime library"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "native/android/build.cake extra_ldflags still only contains -Wl,-z,max-page-size=16384 and does not include RELRO flags in the current codebase."
    }
  },
  "analysis": {
    "summary": "The Android native build of libSkiaSharp.so omits -Wl,-z,relro,-z,now linker flags, which would enforce full RELRO memory protection. Security scanners flag this as a hardening gap. The Skia upstream team confirmed this is a SkiaSharp build decision. The fix is a one-line change to native/android/build.cake, but it introduces a startup performance cost that has not been benchmarked.",
    "rationale": "Classified as type/enhancement because the library functions correctly — RELRO is an optional security hardening feature, not a correctness requirement. The implementation path is well-understood (add linker flags), but the performance trade-off warrants a maintainer decision before merging. The Skia team explicitly deferred this to SkiaSharp.",
    "codeInvestigation": [
      {
        "file": "native/android/build.cake",
        "lines": "42",
        "finding": "extra_ldflags currently only contains '-Wl,-z,max-page-size=16384'; no RELRO or BIND_NOW flags are present. Adding '-Wl,-z,relro,-z,now' would enable full RELRO for all Android architectures built in this task.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "libSkiaSharp.so does not enforce full RELRO",
        "source": "issue title and Ostorlab security scan screenshot",
        "interpretation": "Confirmed missing hardening flag in Android shared library build."
      },
      {
        "text": "This is the sort of thing that's up to SkiaSharp to decide for itself",
        "source": "Skia upstream team response at bugs.chromium.org/p/skia/issues/detail?id=10388 (comment #9, reported by LamijaV)",
        "interpretation": "Upstream Skia explicitly delegated the RELRO decision to SkiaSharp maintainers."
      },
      {
        "text": "Just add `-Wl,-z,relro,-z,now` to native/android/build.cake#L40 and use your own build",
        "source": "comment #issuecomment-644594804 by @Gillibald",
        "interpretation": "The fix is a known one-liner; performance cost at startup is the only trade-off."
      }
    ],
    "workarounds": [
      "Build SkiaSharp from source and add '-Wl,-z,relro,-z,now' to extra_ldflags in native/android/build.cake. This is not suitable as a long-term solution using official releases but can satisfy a pen-test requirement."
    ],
    "nextQuestions": [
      "What is the startup-time performance impact of enabling full RELRO (BIND_NOW) on each Android ABI? Benchmark data needed before committing to the change.",
      "Should RELRO flags also be applied to libHarfBuzzSharp.so built via NDK in the same build.cake?"
    ],
    "resolution": {
      "hypothesis": "Adding '-Wl,-z,relro,-z,now' to extra_ldflags in native/android/build.cake will enable full RELRO on libSkiaSharp.so for all Android architectures.",
      "proposals": [
        {
          "title": "Add RELRO linker flags to Android build",
          "description": "Append '-Wl,-z,relro,-z,now' to the extra_ldflags array in native/android/build.cake. This makes the GOT read-only after startup (full RELRO). A startup-time benchmark should be run before merging to quantify the performance impact.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Enable partial RELRO only",
          "description": "Use '-Wl,-z,relro' without '-z,now' for partial RELRO. This avoids forcing all symbol resolution at load time and reduces the performance hit, while still protecting read-only data segments. Many security scanners accept partial RELRO.",
          "category": "alternative",
          "confidence": 0.7,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add RELRO linker flags to Android build",
      "recommendedReason": "One-line fix with known implementation. Skia upstream team delegated the decision to SkiaSharp. Full RELRO (-z,relro,-z,now) provides the strongest hardening, though partial RELRO is a lower-risk starting point."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Valid security hardening enhancement with a known one-line implementation. Requires maintainer decision on performance trade-off and whether to apply full vs partial RELRO. Issue should stay open until explicitly accepted or declined.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, native library, Android platform, and reliability tenet labels",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/enhancement",
          "area/libSkiaSharp.native",
          "os/Android",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the security concern, explain the current state, and communicate the decision being tracked",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thank you for investigating this and following up with the Skia team.\n\nThis is a valid security hardening request. The fix is well-understood: adding `-Wl,-z,relro,-z,now` to `extra_ldflags` in `native/android/build.cake` would enable full RELRO on `libSkiaSharp.so`. The Skia upstream team confirmed this is SkiaSharp's decision to make.\n\nThe main consideration before enabling this by default is the startup-time performance cost — RELRO with `BIND_NOW` forces all symbol resolution at load time rather than lazily. We haven't benchmarked this impact on the Android targets we ship.\n\nAs a short-term workaround, you can build SkiaSharp from source and add the flags yourself. We're tracking this as an enhancement to evaluate and implement with proper benchmarking."
      }
    ]
  }
}
```

</details>
