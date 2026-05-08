# Issue Triage Report — #2143

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T18:27:25Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/libSkiaSharp.native (0.90 (90%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** EntryPointNotFoundException for 'skottie_animation_make_from_string' and 'skottie_animation_make_from_stream' when using SkiaSharp.Skottie on Linux arm64 (Raspberry Pi 3, aarch64) with SkiaSharp 2.88.1-preview.79 and .NET 6.

**Analysis:** The Linux arm64 (linux-arm64 RID) native binary for libSkiaSharp in version 2.88.1-preview.79 was built without Skottie support, causing EntryPointNotFoundException when any SkiaSharp.Skottie API is called. The older build script (scripts/build-linux.sh) omits 'skia_enable_skottie=true', while the current build system (native/linux/build.cake) does include it and the symbol export map explicitly exports skottie_* symbols.

**Recommendations:** **needs-investigation** — Clear native build bug with complete repro signal. Needs verification that the current stable/preview release ships Skottie symbols in linux-arm64 binary before closing or marking ready-to-fix.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Install SkiaSharp.Skottie 2.88.1-preview.79 on Linux arm64 (aarch64)
2. Call Animation.Parse(jsonString) or Animation.TryCreate(stream, ...)
3. Observe EntryPointNotFoundException for 'skottie_animation_make_from_string'

**Environment:** Raspberry Pi 3, raspberrypi 5.15.32-v8+ #1538 SMP aarch64 GNU/Linux, .NET 6

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | exception |
| Error message | Unable to find an entry point named 'skottie_animation_make_from_string' in shared library 'libSkiaSharp'. |
| Repro quality | complete |
| Target frameworks | net6.0 |

**Stack trace:**

```text
System.EntryPointNotFoundException: Unable to find an entry point named 'skottie_animation_make_from_string' in shared library 'libSkiaSharp'.
   at SkiaSharp.SkottieApi.skottie_animation_make_from_string(String data, Int32 length)
   at SkiaSharp.Skottie.Animation.TryParse(String json, Animation& animation)
   at SkiaSharp.Skottie.Animation.Parse(String json)
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.1-preview.79 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | The current build.cake includes skia_enable_skottie=true for Linux arm64 builds, suggesting this may be fixed in newer releases, but it cannot be confirmed without testing the stable release on arm64. |

## Analysis

### Technical Summary

The Linux arm64 (linux-arm64 RID) native binary for libSkiaSharp in version 2.88.1-preview.79 was built without Skottie support, causing EntryPointNotFoundException when any SkiaSharp.Skottie API is called. The older build script (scripts/build-linux.sh) omits 'skia_enable_skottie=true', while the current build system (native/linux/build.cake) does include it and the symbol export map explicitly exports skottie_* symbols.

### Rationale

This is clearly a native build/packaging bug — the Skottie symbols are simply absent from the linux-arm64 libSkiaSharp.so binary. The C# P/Invoke layer and managed bindings are correct; the problem is the native library. Both skottie_animation_make_from_string and skottie_animation_make_from_stream are missing, confirming the entire Skottie feature was not compiled in for this platform/architecture combination. The reporter correctly diagnoses the issue as a missing native entry point.

### Key Signals

- "Unable to find an entry point named 'skottie_animation_make_from_string' in shared library 'libSkiaSharp'" — **issue body** (Skottie symbols entirely absent from the linux-arm64 native binary — not a signature mismatch but a missing feature compilation.)
- "also 'skottie_animation_make_from_stream' missing" — **comment #1** (Confirms the entire Skottie feature is not compiled into the native binary, not just one function.)
- "raspberrypi 5.15.32-v8+ #1538 SMP PREEMPT aarch64 GNU/Linux" — **issue body** (Linux arm64 (linux-arm64 RID) platform. This is a less common target that may have been less thoroughly tested in preview builds.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `native/linux/build.cake` | 114 | direct | Current Linux build explicitly includes 'skia_enable_skottie=true' in GN build args for all architectures including arm64. This was likely absent in the build that produced 2.88.1-preview.79. |
| `native/linux/libSkiaSharp/libSkiaSharp.map` | 5 | direct | The linker symbol export map explicitly exports 'skottie_*' symbols via version script. If the binary was built without skia_enable_skottie, these symbols would not exist in the .so. |
| `binding/SkiaSharp.Skottie/SkottieApi.generated.cs` | 482-498 | related | P/Invoke declarations for skottie_animation_make_from_string and skottie_animation_make_from_stream are present and correct. The managed binding layer is not the source of the bug. |
| `scripts/build-linux.sh` | 33-55 | direct | The older Linux build script does NOT include 'skia_enable_skottie=true' in GN args, confirming the hypothesis that early/preview Linux arm64 binaries were built without Skottie support. |
| `binding/SkiaSharp.Skottie/Animation.cs` | 40 | context | Animation.TryParse calls skottie_animation_make_from_string — the call site is correct. No wrapper-level bug. |

### Next Questions

- Is Skottie present in the linux-arm64 binary of the current stable release (2.88.x or 3.x)?
- Was skia_enable_skottie=true added to the arm64 cross-compilation pipeline at the same time as x64?

### Resolution Proposals

**Hypothesis:** The linux-arm64 libSkiaSharp.so in preview 79 was built without skia_enable_skottie=true. The current build.cake fixes this for newer builds, so upgrading to a current release may resolve the issue.

1. **Upgrade to current release** — workaround, confidence 0.65 (65%), cost/xs, validated=untested
   - The current native/linux/build.cake includes skia_enable_skottie=true for all architectures. Upgrading from preview 79 to a current stable or preview release may include Skottie symbols in the linux-arm64 binary.
2. **Verify and fix CI pipeline for Linux arm64 Skottie** — fix, confidence 0.80 (80%), cost/s, validated=untested
   - Confirm that the current CI/CD pipeline for linux-arm64 builds uses native/linux/build.cake and includes skia_enable_skottie=true. If it does not, add the flag to ensure all future linux-arm64 packages ship Skottie.

**Recommended proposal:** Verify and fix CI pipeline for Linux arm64 Skottie

**Why:** The root cause needs to be confirmed by checking whether the current release pipeline builds Linux arm64 with Skottie. The build.cake update appears to address this, but verification is required.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Clear native build bug with complete repro signal. Needs verification that the current stable/preview release ships Skottie symbols in linux-arm64 binary before closing or marking ready-to-fix. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, native library, linux, and reliability labels | labels=type/bug, area/libSkiaSharp.native, os/Linux, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Inform reporter about likely root cause and suggest upgrading | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report. This looks like the `linux-arm64` native binary for `libSkiaSharp` in this preview version was compiled without Skottie support (`skia_enable_skottie` was not set in the GN build configuration at that time).

The current build system (`native/linux/build.cake`) includes `skia_enable_skottie=true` for all Linux architectures including arm64. Could you try upgrading to the latest SkiaSharp release and confirm whether the issue persists? That will help us determine if this is already fixed in current releases or needs additional CI pipeline investigation for Linux arm64.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2143,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T18:27:25Z"
  },
  "summary": "EntryPointNotFoundException for 'skottie_animation_make_from_string' and 'skottie_animation_make_from_stream' when using SkiaSharp.Skottie on Linux arm64 (Raspberry Pi 3, aarch64) with SkiaSharp 2.88.1-preview.79 and .NET 6.",
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
      "os/Linux"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "Unable to find an entry point named 'skottie_animation_make_from_string' in shared library 'libSkiaSharp'.",
      "stackTrace": "System.EntryPointNotFoundException: Unable to find an entry point named 'skottie_animation_make_from_string' in shared library 'libSkiaSharp'.\n   at SkiaSharp.SkottieApi.skottie_animation_make_from_string(String data, Int32 length)\n   at SkiaSharp.Skottie.Animation.TryParse(String json, Animation& animation)\n   at SkiaSharp.Skottie.Animation.Parse(String json)",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net6.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Install SkiaSharp.Skottie 2.88.1-preview.79 on Linux arm64 (aarch64)",
        "Call Animation.Parse(jsonString) or Animation.TryCreate(stream, ...)",
        "Observe EntryPointNotFoundException for 'skottie_animation_make_from_string'"
      ],
      "environmentDetails": "Raspberry Pi 3, raspberrypi 5.15.32-v8+ #1538 SMP aarch64 GNU/Linux, .NET 6"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.1-preview.79"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "The current build.cake includes skia_enable_skottie=true for Linux arm64 builds, suggesting this may be fixed in newer releases, but it cannot be confirmed without testing the stable release on arm64."
    }
  },
  "analysis": {
    "summary": "The Linux arm64 (linux-arm64 RID) native binary for libSkiaSharp in version 2.88.1-preview.79 was built without Skottie support, causing EntryPointNotFoundException when any SkiaSharp.Skottie API is called. The older build script (scripts/build-linux.sh) omits 'skia_enable_skottie=true', while the current build system (native/linux/build.cake) does include it and the symbol export map explicitly exports skottie_* symbols.",
    "rationale": "This is clearly a native build/packaging bug — the Skottie symbols are simply absent from the linux-arm64 libSkiaSharp.so binary. The C# P/Invoke layer and managed bindings are correct; the problem is the native library. Both skottie_animation_make_from_string and skottie_animation_make_from_stream are missing, confirming the entire Skottie feature was not compiled in for this platform/architecture combination. The reporter correctly diagnoses the issue as a missing native entry point.",
    "codeInvestigation": [
      {
        "file": "native/linux/build.cake",
        "lines": "114",
        "finding": "Current Linux build explicitly includes 'skia_enable_skottie=true' in GN build args for all architectures including arm64. This was likely absent in the build that produced 2.88.1-preview.79.",
        "relevance": "direct"
      },
      {
        "file": "native/linux/libSkiaSharp/libSkiaSharp.map",
        "lines": "5",
        "finding": "The linker symbol export map explicitly exports 'skottie_*' symbols via version script. If the binary was built without skia_enable_skottie, these symbols would not exist in the .so.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.Skottie/SkottieApi.generated.cs",
        "lines": "482-498",
        "finding": "P/Invoke declarations for skottie_animation_make_from_string and skottie_animation_make_from_stream are present and correct. The managed binding layer is not the source of the bug.",
        "relevance": "related"
      },
      {
        "file": "scripts/build-linux.sh",
        "lines": "33-55",
        "finding": "The older Linux build script does NOT include 'skia_enable_skottie=true' in GN args, confirming the hypothesis that early/preview Linux arm64 binaries were built without Skottie support.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.Skottie/Animation.cs",
        "lines": "40",
        "finding": "Animation.TryParse calls skottie_animation_make_from_string — the call site is correct. No wrapper-level bug.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "Unable to find an entry point named 'skottie_animation_make_from_string' in shared library 'libSkiaSharp'",
        "source": "issue body",
        "interpretation": "Skottie symbols entirely absent from the linux-arm64 native binary — not a signature mismatch but a missing feature compilation."
      },
      {
        "text": "also 'skottie_animation_make_from_stream' missing",
        "source": "comment #1",
        "interpretation": "Confirms the entire Skottie feature is not compiled into the native binary, not just one function."
      },
      {
        "text": "raspberrypi 5.15.32-v8+ #1538 SMP PREEMPT aarch64 GNU/Linux",
        "source": "issue body",
        "interpretation": "Linux arm64 (linux-arm64 RID) platform. This is a less common target that may have been less thoroughly tested in preview builds."
      }
    ],
    "nextQuestions": [
      "Is Skottie present in the linux-arm64 binary of the current stable release (2.88.x or 3.x)?",
      "Was skia_enable_skottie=true added to the arm64 cross-compilation pipeline at the same time as x64?"
    ],
    "resolution": {
      "hypothesis": "The linux-arm64 libSkiaSharp.so in preview 79 was built without skia_enable_skottie=true. The current build.cake fixes this for newer builds, so upgrading to a current release may resolve the issue.",
      "proposals": [
        {
          "title": "Upgrade to current release",
          "description": "The current native/linux/build.cake includes skia_enable_skottie=true for all architectures. Upgrading from preview 79 to a current stable or preview release may include Skottie symbols in the linux-arm64 binary.",
          "category": "workaround",
          "confidence": 0.65,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Verify and fix CI pipeline for Linux arm64 Skottie",
          "description": "Confirm that the current CI/CD pipeline for linux-arm64 builds uses native/linux/build.cake and includes skia_enable_skottie=true. If it does not, add the flag to ensure all future linux-arm64 packages ship Skottie.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Verify and fix CI pipeline for Linux arm64 Skottie",
      "recommendedReason": "The root cause needs to be confirmed by checking whether the current release pipeline builds Linux arm64 with Skottie. The build.cake update appears to address this, but verification is required."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Clear native build bug with complete repro signal. Needs verification that the current stable/preview release ships Skottie symbols in linux-arm64 binary before closing or marking ready-to-fix.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, native library, linux, and reliability labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Linux",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Inform reporter about likely root cause and suggest upgrading",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thank you for the detailed report. This looks like the `linux-arm64` native binary for `libSkiaSharp` in this preview version was compiled without Skottie support (`skia_enable_skottie` was not set in the GN build configuration at that time).\n\nThe current build system (`native/linux/build.cake`) includes `skia_enable_skottie=true` for all Linux architectures including arm64. Could you try upgrading to the latest SkiaSharp release and confirm whether the issue persists? That will help us determine if this is already fixed in current releases or needs additional CI pipeline investigation for Linux arm64."
      }
    ]
  }
}
```

</details>
