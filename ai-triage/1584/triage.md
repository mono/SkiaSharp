# Issue Triage Report — #1584

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T06:39:00Z |
| Type | type/feature-request (0.80 (80%)) |
| Area | area/libSkiaSharp.native (0.95 (95%)) |
| Suggested action | needs-info (0.85 (85%)) |

**Issue Summary:** User requests a pre-built mips64 binary of libSkiaSharp, as compiling it themselves produces errors.

**Analysis:** The user wants a libSkiaSharp native library built for the mips64 architecture. Inspection of the build scripts shows that SkiaSharp only supports x64 (and loongarch64) for Linux, and x86/x86_64/arm64-v8a for Android — mips64 is not a supported target. This is a new platform/architecture support request, not a bug in existing functionality. No prior issues or PRs for mips64 support were found. The issue body provides no error details, repro steps, or build configuration, so more information would be needed to assist the user with self-compilation attempts.

**Recommendations:** **needs-info** — The issue provides no error details, target platform context, or SkiaSharp version. mips64 is not a supported architecture. Need to understand if this is Android mips64 (deprecated by Google) or Linux mips64, and what errors the user is encountering.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/libSkiaSharp.native |
| Platforms | os/Linux |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

## Analysis

### Technical Summary

The user wants a libSkiaSharp native library built for the mips64 architecture. Inspection of the build scripts shows that SkiaSharp only supports x64 (and loongarch64) for Linux, and x86/x86_64/arm64-v8a for Android — mips64 is not a supported target. This is a new platform/architecture support request, not a bug in existing functionality. No prior issues or PRs for mips64 support were found. The issue body provides no error details, repro steps, or build configuration, so more information would be needed to assist the user with self-compilation attempts.

### Rationale

Classified as type/feature-request because mips64 is not an officially supported architecture in SkiaSharp's build scripts, and adding it would require new native build infrastructure. The issue lacks build error details, so it cannot be treated as a bug. Area is libSkiaSharp.native as this is purely a native build concern.

### Key Signals

- "Could you please provide me with a libSkiaSharp that has been compiled on mips64" — **issue body** (User is requesting a pre-compiled mips64 binary, implying they cannot successfully compile it themselves.)
- "I always get all sorts of errors when compiling libSkiaSharp to mips64" — **issue title** (User has attempted to compile for mips64 and encountered unspecified errors.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `native/linux/build.cake` | — | direct | BUILD_ARCH only includes 'x64' (and loongarch64 mapping). No mips64 target is defined for Linux builds. |
| `native/android/build.cake` | — | related | Android native builds target x86, x86_64, and arm64-v8a. mips/mips64 is not present. |

### Next Questions

- What OS/distro is the target mips64 environment (Linux)? Which Android NDK version?
- What specific errors occur when attempting to compile libSkiaSharp for mips64?
- Is this for Android mips64 (which Google deprecated in NDK r17) or a Linux mips64 device?

### Resolution Proposals

1. **Request more information from reporter** — investigation, cost/xs, validated=untested
   - Request more information from the reporter: target platform (Android mips64 vs Linux mips64), specific build errors, and SkiaSharp version. Note that Android NDK dropped mips/mips64 support in NDK r17.

**Recommended proposal:** Request more information from reporter

**Why:** The issue lacks enough detail to take action. Gathering more info is the first step; mips64 Android support was dropped by Google so this may be a dead end for Android targets.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.85 (85%) |
| Reason | The issue provides no error details, target platform context, or SkiaSharp version. mips64 is not a supported architecture. Need to understand if this is Android mips64 (deprecated by Google) or Linux mips64, and what errors the user is encountering. |
| Suggested repro platform | linux |

### Missing Info

- Target platform: Android mips64 or Linux mips64?
- Specific compilation errors encountered
- SkiaSharp version being used
- Build environment / toolchain details

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/feature-request, area/libSkiaSharp.native, os/Linux labels | labels=type/feature-request, area/libSkiaSharp.native, os/Linux |
| add-comment | medium | 0.85 (85%) | Ask reporter for more details: target platform, specific errors, SkiaSharp version, and note Android mips64 deprecation | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the report! To help you better, could you provide the following details?

1. **Target platform**: Is this for an Android mips64 device, or a Linux mips64 system?
2. **Specific errors**: What errors do you encounter when trying to compile libSkiaSharp for mips64? (Please paste the full error output)
3. **SkiaSharp version**: Which version of SkiaSharp are you targeting?
4. **Toolchain**: What compiler/toolchain/NDK version are you using?

> **Note for Android mips64**: Google officially dropped mips and mips64 support in Android NDK r17 (2018). If you're targeting Android mips64, it may not be possible to use a current NDK, and SkiaSharp does not ship mips64 Android binaries as this architecture is no longer supported by the Android ecosystem.

mips64 is currently not an officially supported build target for SkiaSharp. Supporting it would require significant build infrastructure changes.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1584,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T06:39:00Z"
  },
  "summary": "User requests a pre-built mips64 binary of libSkiaSharp, as compiling it themselves produces errors.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.8
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.95
    },
    "platforms": [
      "os/Linux"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "The user wants a libSkiaSharp native library built for the mips64 architecture. Inspection of the build scripts shows that SkiaSharp only supports x64 (and loongarch64) for Linux, and x86/x86_64/arm64-v8a for Android — mips64 is not a supported target. This is a new platform/architecture support request, not a bug in existing functionality. No prior issues or PRs for mips64 support were found. The issue body provides no error details, repro steps, or build configuration, so more information would be needed to assist the user with self-compilation attempts.",
    "codeInvestigation": [
      {
        "file": "native/linux/build.cake",
        "finding": "BUILD_ARCH only includes 'x64' (and loongarch64 mapping). No mips64 target is defined for Linux builds.",
        "relevance": "direct"
      },
      {
        "file": "native/android/build.cake",
        "finding": "Android native builds target x86, x86_64, and arm64-v8a. mips/mips64 is not present.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Could you please provide me with a libSkiaSharp that has been compiled on mips64",
        "source": "issue body",
        "interpretation": "User is requesting a pre-compiled mips64 binary, implying they cannot successfully compile it themselves."
      },
      {
        "text": "I always get all sorts of errors when compiling libSkiaSharp to mips64",
        "source": "issue title",
        "interpretation": "User has attempted to compile for mips64 and encountered unspecified errors."
      }
    ],
    "rationale": "Classified as type/feature-request because mips64 is not an officially supported architecture in SkiaSharp's build scripts, and adding it would require new native build infrastructure. The issue lacks build error details, so it cannot be treated as a bug. Area is libSkiaSharp.native as this is purely a native build concern.",
    "nextQuestions": [
      "What OS/distro is the target mips64 environment (Linux)? Which Android NDK version?",
      "What specific errors occur when attempting to compile libSkiaSharp for mips64?",
      "Is this for Android mips64 (which Google deprecated in NDK r17) or a Linux mips64 device?"
    ],
    "resolution": {
      "proposals": [
        {
          "title": "Request more information from reporter",
          "category": "investigation",
          "description": "Request more information from the reporter: target platform (Android mips64 vs Linux mips64), specific build errors, and SkiaSharp version. Note that Android NDK dropped mips/mips64 support in NDK r17.",
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Request more information from reporter",
      "recommendedReason": "The issue lacks enough detail to take action. Gathering more info is the first step; mips64 Android support was dropped by Google so this may be a dead end for Android targets."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.85,
      "reason": "The issue provides no error details, target platform context, or SkiaSharp version. mips64 is not a supported architecture. Need to understand if this is Android mips64 (deprecated by Google) or Linux mips64, and what errors the user is encountering.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Target platform: Android mips64 or Linux mips64?",
      "Specific compilation errors encountered",
      "SkiaSharp version being used",
      "Build environment / toolchain details"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/feature-request, area/libSkiaSharp.native, os/Linux labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/feature-request",
          "area/libSkiaSharp.native",
          "os/Linux"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask reporter for more details: target platform, specific errors, SkiaSharp version, and note Android mips64 deprecation",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thank you for the report! To help you better, could you provide the following details?\n\n1. **Target platform**: Is this for an Android mips64 device, or a Linux mips64 system?\n2. **Specific errors**: What errors do you encounter when trying to compile libSkiaSharp for mips64? (Please paste the full error output)\n3. **SkiaSharp version**: Which version of SkiaSharp are you targeting?\n4. **Toolchain**: What compiler/toolchain/NDK version are you using?\n\n> **Note for Android mips64**: Google officially dropped mips and mips64 support in Android NDK r17 (2018). If you're targeting Android mips64, it may not be possible to use a current NDK, and SkiaSharp does not ship mips64 Android binaries as this architecture is no longer supported by the Android ecosystem.\n\nmips64 is currently not an officially supported build target for SkiaSharp. Supporting it would require significant build infrastructure changes."
      }
    ]
  }
}
```

</details>
