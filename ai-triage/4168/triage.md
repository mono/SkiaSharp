# Issue Triage Report — #4168

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-25T05:13:00Z |
| Type | type/enhancement (0.95 (95%)) |
| Area | area/Build (0.97 (97%)) |
| Suggested action | keep-open (0.92 (92%)) |

**Issue Summary:** Migrate Apple native library builds (macOS, iOS, tvOS, Mac Catalyst) from hand-maintained Xcode project files to the GN/ninja build system already used by all other platforms.

**Analysis:** The Apple native libraries (libSkiaSharp and libHarfBuzzSharp for macOS, iOS, tvOS, Mac Catalyst) are built via hand-maintained Xcode .xcodeproj files and a Cake.XCode addin, while all other platforms (Linux, Windows, Android, WASM) build directly from GN/ninja targets. This enhancement proposes retiring the bespoke Apple/Xcode tooling and producing identical artifacts from GN instead, improving consistency and reducing maintenance burden.

**Recommendations:** **keep-open** — Well-scoped enhancement filed by the maintainer with a linked PR already in progress. No user action needed — track until the PR lands.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/Build |
| Platforms | os/macOS, os/iOS, os/tvOS |
| Backends | — |
| Tenets | — |
| Perf | — |
| Partner | — |

## Evidence

### Reproduction

**Environment:** Apple platforms (macOS, iOS, tvOS, Mac Catalyst) — build-system change only, no runtime behavior change

## Analysis

### Technical Summary

The Apple native libraries (libSkiaSharp and libHarfBuzzSharp for macOS, iOS, tvOS, Mac Catalyst) are built via hand-maintained Xcode .xcodeproj files and a Cake.XCode addin, while all other platforms (Linux, Windows, Android, WASM) build directly from GN/ninja targets. This enhancement proposes retiring the bespoke Apple/Xcode tooling and producing identical artifacts from GN instead, improving consistency and reducing maintenance burden.

### Rationale

This is clearly a build-system enhancement — no public API changes, no behavioral change for end users. The author (mattleibow, the maintainer) filed this as a tracked design issue with an associated PR. The Xcode project files confirmed in the native/ directory and the xcode.cake script confirm the current state matches the issue description.

### Key Signals

- "Linux, Windows, WebAssembly and Android all build directly from the GN/ninja targets that Skia already defines. The Apple platforms should do the same." — **issue body** (Maintainer identifying a consistency gap in build infrastructure — Apple is the only outlier.)
- "Build-system change only — no change to the public API or to which symbols are exported." — **issue body** (Confirms this is a pure build/infra enhancement with no user-visible behavioral change.)
- "see the linked PR for the concrete approach" — **issue body** (Implementation is already in progress; this issue is the tracking item.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `native/macos/libSkiaSharp/libSkiaSharp.xcodeproj` | — | direct | Hand-maintained Xcode project for macOS libSkiaSharp — confirms current Apple build uses separate Xcode projects, not GN |
| `native/ios/libSkiaSharp/libSkiaSharp.xcodeproj` | — | direct | Hand-maintained Xcode project for iOS libSkiaSharp — confirms pattern repeats across all Apple targets (6 .xcodeproj files total across tvos/ios/macos) |
| `scripts/infra/native/apple/xcode.cake` | — | direct | Apple-specific Cake script using Cake.XCode addin with RunXCodeBuild helper — unlike linux/android which use GN scripts directly. Confirms bespoke Apple build path described in the issue. |

### Resolution Proposals

**Hypothesis:** Replace xcode.cake and the 6 .xcodeproj files with GN build targets that produce equivalent .dylib (macOS) and .framework bundles (iOS/tvOS/Mac Catalyst) using the same GN/ninja pipeline used by other platforms.

1. **Migrate to GN build targets for Apple platforms** — fix, confidence 0.90 (90%), cost/l, validated=untested
   - Define GN targets for libSkiaSharp and libHarfBuzzSharp on macOS, iOS, tvOS, and Mac Catalyst. Update the Cake build script to invoke GN/ninja instead of xcodebuild. Verify output artifacts (architectures, install names, code signature, framework metadata) match the current released baseline.

**Recommended proposal:** Migrate to GN build targets for Apple platforms

**Why:** This is the stated goal of the issue and the approach described in the linked PR. Eliminates the maintenance burden of 6 hand-maintained Xcode project files and aligns Apple builds with every other platform.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.92 (92%) |
| Reason | Well-scoped enhancement filed by the maintainer with a linked PR already in progress. No user action needed — track until the PR lands. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply enhancement and build labels | labels=type/enhancement, area/Build, os/macOS, os/iOS, os/tvOS |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4168,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-25T05:13:00Z"
  },
  "summary": "Migrate Apple native library builds (macOS, iOS, tvOS, Mac Catalyst) from hand-maintained Xcode project files to the GN/ninja build system already used by all other platforms.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.95
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.97
    },
    "platforms": [
      "os/macOS",
      "os/iOS",
      "os/tvOS"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Apple platforms (macOS, iOS, tvOS, Mac Catalyst) — build-system change only, no runtime behavior change",
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "The Apple native libraries (libSkiaSharp and libHarfBuzzSharp for macOS, iOS, tvOS, Mac Catalyst) are built via hand-maintained Xcode .xcodeproj files and a Cake.XCode addin, while all other platforms (Linux, Windows, Android, WASM) build directly from GN/ninja targets. This enhancement proposes retiring the bespoke Apple/Xcode tooling and producing identical artifacts from GN instead, improving consistency and reducing maintenance burden.",
    "rationale": "This is clearly a build-system enhancement — no public API changes, no behavioral change for end users. The author (mattleibow, the maintainer) filed this as a tracked design issue with an associated PR. The Xcode project files confirmed in the native/ directory and the xcode.cake script confirm the current state matches the issue description.",
    "codeInvestigation": [
      {
        "file": "native/macos/libSkiaSharp/libSkiaSharp.xcodeproj",
        "finding": "Hand-maintained Xcode project for macOS libSkiaSharp — confirms current Apple build uses separate Xcode projects, not GN",
        "relevance": "direct"
      },
      {
        "file": "native/ios/libSkiaSharp/libSkiaSharp.xcodeproj",
        "finding": "Hand-maintained Xcode project for iOS libSkiaSharp — confirms pattern repeats across all Apple targets (6 .xcodeproj files total across tvos/ios/macos)",
        "relevance": "direct"
      },
      {
        "file": "scripts/infra/native/apple/xcode.cake",
        "finding": "Apple-specific Cake script using Cake.XCode addin with RunXCodeBuild helper — unlike linux/android which use GN scripts directly. Confirms bespoke Apple build path described in the issue.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Linux, Windows, WebAssembly and Android all build directly from the GN/ninja targets that Skia already defines. The Apple platforms should do the same.",
        "source": "issue body",
        "interpretation": "Maintainer identifying a consistency gap in build infrastructure — Apple is the only outlier."
      },
      {
        "text": "Build-system change only — no change to the public API or to which symbols are exported.",
        "source": "issue body",
        "interpretation": "Confirms this is a pure build/infra enhancement with no user-visible behavioral change."
      },
      {
        "text": "see the linked PR for the concrete approach",
        "source": "issue body",
        "interpretation": "Implementation is already in progress; this issue is the tracking item."
      }
    ],
    "resolution": {
      "hypothesis": "Replace xcode.cake and the 6 .xcodeproj files with GN build targets that produce equivalent .dylib (macOS) and .framework bundles (iOS/tvOS/Mac Catalyst) using the same GN/ninja pipeline used by other platforms.",
      "proposals": [
        {
          "title": "Migrate to GN build targets for Apple platforms",
          "description": "Define GN targets for libSkiaSharp and libHarfBuzzSharp on macOS, iOS, tvOS, and Mac Catalyst. Update the Cake build script to invoke GN/ninja instead of xcodebuild. Verify output artifacts (architectures, install names, code signature, framework metadata) match the current released baseline.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Migrate to GN build targets for Apple platforms",
      "recommendedReason": "This is the stated goal of the issue and the approach described in the linked PR. Eliminates the maintenance burden of 6 hand-maintained Xcode project files and aligns Apple builds with every other platform."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.92,
      "reason": "Well-scoped enhancement filed by the maintainer with a linked PR already in progress. No user action needed — track until the PR lands.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement and build labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/enhancement",
          "area/Build",
          "os/macOS",
          "os/iOS",
          "os/tvOS"
        ]
      }
    ]
  }
}
```

</details>
