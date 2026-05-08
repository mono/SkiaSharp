# Issue Triage Report — #2378

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T02:17:00Z |
| Type | type/feature-request (0.90 (90%)) |
| Area | area/libSkiaSharp.native (0.90 (90%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** Reporter requests reduction of SkiaSharp native library size, noting the total build size doubled after adding SkiaSharp (39.7 MB in v2.88 vs 30.1 MB in v1.68) on Android.

**Analysis:** Reporter wants smaller native binaries for SkiaSharp on Android. The ~32% size increase from v1.68 to v2.88 is due to upstream Skia adding more codecs, GPU backends, and features. No mechanism currently exists to build a trimmed native library without certain features (e.g., no GPU/PDF). This is a recurring ask with no easy fix — the native library must contain all Skia features to support the full C# API surface.

**Recommendations:** **keep-open** — Recurring valid request. No trivial fix; requires upstream Skia build configuration work. Should remain open to track interest.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/libSkiaSharp.native |
| Platforms | os/Android |
| Backends | — |
| Tenets | tenet/performance |
| Partner | — |

## Evidence

### Reproduction

**Environment:** Android build; SkiaSharp v2.88 (39.7 MB) vs v1.68 (30.1 MB)

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/174 — Native Library - File Size Reduction? (closed)
- https://github.com/mono/SkiaSharp/issues/1508 — Build without GPU and PDF support (closed)

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88, 1.68 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Native library size concerns persist as Skia has grown with v3.x adding even more features. |

## Analysis

### Technical Summary

Reporter wants smaller native binaries for SkiaSharp on Android. The ~32% size increase from v1.68 to v2.88 is due to upstream Skia adding more codecs, GPU backends, and features. No mechanism currently exists to build a trimmed native library without certain features (e.g., no GPU/PDF). This is a recurring ask with no easy fix — the native library must contain all Skia features to support the full C# API surface.

### Rationale

The issue is a feature request, not a bug. The size increase is expected given Skia upstream added functionality between milestones. Multiple prior issues (#174, #1508) track this same ask. The correct classification is type/feature-request targeting the native library area.

### Key Signals

- "SkiaSharp 2.88 its 39.7MB" — **issue body** (Reporter comparing build output sizes between versions.)
- "with 1.68 its 30.1 MB" — **issue body** (Baseline size for comparison — ~32% increase across major version bump.)
- "I just install skiaSharp and implemented basic resize" — **issue body** (User is doing minimal usage (image resize) but sees full library overhead.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `native/android/` | — | direct | Android native build directory exists with per-ABI native .so compilation. No feature-flag/trim mechanism found — library includes all Skia features. |
| `native/android/build.cake` | — | direct | Build script for Android native library; compiles the full libSkiaSharp.so with all features enabled by default. |

### Workarounds

- Use Android's built-in APK splitting / AAB to only ship the ABI needed for the target device
- Enable Android linker (proguard/r8) to remove unused managed code, though native .so size is fixed
- Consider SkiaSharp.NativeAssets.Android which delivers per-ABI .so files rather than fat binaries

### Next Questions

- Which Android ABI targets does the reporter include? Including all (arm, arm64, x86, x86_64) multiplies the effective native size.
- Is the reporter using AAB (Android App Bundle) which automatically splits by ABI, or APK?
- Would a slimmed/feature-stripped native build be acceptable (breaking full API availability)?

### Resolution Proposals

**Hypothesis:** Size increase is inherent to Skia upstream growth; no single fix exists. A feature-stripped lite build would require significant upstream coordination.

1. **Enable ABI splits / AAB** — workaround, confidence 0.85 (85%), cost/xs, validated=untested
   - Configure Android project to use Android App Bundle (AAB) so app stores only deliver the native .so for the user's ABI, effectively reducing per-device download to ~25% of the total.
2. **Investigate upstream Skia lite build** — investigation, confidence 0.60 (60%), cost/xl, validated=untested
   - Upstream Skia supports GN build flags (e.g., skia_use_libjpeg_turbo=false, skia_use_libwebp=false, skia_enable_pdf=false) that could produce a smaller binary. A separate 'SkiaSharp.Lite' NativeAssets package could be published with a restricted feature set.

**Recommended proposal:** Enable ABI splits / AAB

**Why:** Simplest immediate workaround requiring no SkiaSharp changes. AAB virtually eliminates per-device ABI overhead.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Recurring valid request. No trivial fix; requires upstream Skia build configuration work. Should remain open to track interest. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply feature-request, native, Android, performance labels | labels=type/feature-request, area/libSkiaSharp.native, os/Android, tenet/performance |
| add-comment | medium | 0.85 (85%) | Acknowledge request and suggest AAB/ABI split workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! The increase from v1.68 to v2.88 is expected — upstream Skia has grown significantly with new codecs, GPU backends, and features.

Some immediate workarounds to reduce per-device download size:

1. **Use Android App Bundle (AAB)**: Publish with `.aab` instead of `.apk`. App stores automatically deliver only the native `.so` for the user's device ABI, so the per-device payload is roughly ¼ of the total.
2. **Enable ABI splitting**: In your `csproj`, set `$(AndroidCreatePackagePerAbi)=true` to generate separate APKs per ABI.

A 'lite' native build with unused Skia features stripped (PDF, GPU, certain codecs) has been discussed before (see #174, #1508) and is a valid long-term ask — though it requires significant upstream build configuration work. We're keeping this open to track interest.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2378,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T02:17:00Z"
  },
  "summary": "Reporter requests reduction of SkiaSharp native library size, noting the total build size doubled after adding SkiaSharp (39.7 MB in v2.88 vs 30.1 MB in v1.68) on Android.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.9
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.9
    },
    "platforms": [
      "os/Android"
    ],
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Android build; SkiaSharp v2.88 (39.7 MB) vs v1.68 (30.1 MB)",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/174",
          "description": "Native Library - File Size Reduction? (closed)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1508",
          "description": "Build without GPU and PDF support (closed)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88",
        "1.68"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Native library size concerns persist as Skia has grown with v3.x adding even more features."
    }
  },
  "analysis": {
    "summary": "Reporter wants smaller native binaries for SkiaSharp on Android. The ~32% size increase from v1.68 to v2.88 is due to upstream Skia adding more codecs, GPU backends, and features. No mechanism currently exists to build a trimmed native library without certain features (e.g., no GPU/PDF). This is a recurring ask with no easy fix — the native library must contain all Skia features to support the full C# API surface.",
    "rationale": "The issue is a feature request, not a bug. The size increase is expected given Skia upstream added functionality between milestones. Multiple prior issues (#174, #1508) track this same ask. The correct classification is type/feature-request targeting the native library area.",
    "keySignals": [
      {
        "text": "SkiaSharp 2.88 its 39.7MB",
        "source": "issue body",
        "interpretation": "Reporter comparing build output sizes between versions."
      },
      {
        "text": "with 1.68 its 30.1 MB",
        "source": "issue body",
        "interpretation": "Baseline size for comparison — ~32% increase across major version bump."
      },
      {
        "text": "I just install skiaSharp and implemented basic resize",
        "source": "issue body",
        "interpretation": "User is doing minimal usage (image resize) but sees full library overhead."
      }
    ],
    "codeInvestigation": [
      {
        "file": "native/android/",
        "finding": "Android native build directory exists with per-ABI native .so compilation. No feature-flag/trim mechanism found — library includes all Skia features.",
        "relevance": "direct"
      },
      {
        "file": "native/android/build.cake",
        "finding": "Build script for Android native library; compiles the full libSkiaSharp.so with all features enabled by default.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Use Android's built-in APK splitting / AAB to only ship the ABI needed for the target device",
      "Enable Android linker (proguard/r8) to remove unused managed code, though native .so size is fixed",
      "Consider SkiaSharp.NativeAssets.Android which delivers per-ABI .so files rather than fat binaries"
    ],
    "nextQuestions": [
      "Which Android ABI targets does the reporter include? Including all (arm, arm64, x86, x86_64) multiplies the effective native size.",
      "Is the reporter using AAB (Android App Bundle) which automatically splits by ABI, or APK?",
      "Would a slimmed/feature-stripped native build be acceptable (breaking full API availability)?"
    ],
    "resolution": {
      "hypothesis": "Size increase is inherent to Skia upstream growth; no single fix exists. A feature-stripped lite build would require significant upstream coordination.",
      "proposals": [
        {
          "title": "Enable ABI splits / AAB",
          "description": "Configure Android project to use Android App Bundle (AAB) so app stores only deliver the native .so for the user's ABI, effectively reducing per-device download to ~25% of the total.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate upstream Skia lite build",
          "description": "Upstream Skia supports GN build flags (e.g., skia_use_libjpeg_turbo=false, skia_use_libwebp=false, skia_enable_pdf=false) that could produce a smaller binary. A separate 'SkiaSharp.Lite' NativeAssets package could be published with a restricted feature set.",
          "category": "investigation",
          "confidence": 0.6,
          "effort": "cost/xl",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Enable ABI splits / AAB",
      "recommendedReason": "Simplest immediate workaround requiring no SkiaSharp changes. AAB virtually eliminates per-device ABI overhead."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Recurring valid request. No trivial fix; requires upstream Skia build configuration work. Should remain open to track interest.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, native, Android, performance labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/feature-request",
          "area/libSkiaSharp.native",
          "os/Android",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge request and suggest AAB/ABI split workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the report! The increase from v1.68 to v2.88 is expected — upstream Skia has grown significantly with new codecs, GPU backends, and features.\n\nSome immediate workarounds to reduce per-device download size:\n\n1. **Use Android App Bundle (AAB)**: Publish with `.aab` instead of `.apk`. App stores automatically deliver only the native `.so` for the user's device ABI, so the per-device payload is roughly ¼ of the total.\n2. **Enable ABI splitting**: In your `csproj`, set `$(AndroidCreatePackagePerAbi)=true` to generate separate APKs per ABI.\n\nA 'lite' native build with unused Skia features stripped (PDF, GPU, certain codecs) has been discussed before (see #174, #1508) and is a valid long-term ask — though it requires significant upstream build configuration work. We're keeping this open to track interest."
      }
    ]
  }
}
```

</details>
