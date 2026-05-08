# Issue Triage Report — #3097

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T23:25:04Z |
| Type | type/feature-request (0.90 (90%)) |
| Area | area/libSkiaSharp.native (0.95 (95%)) |
| Suggested action | close-as-fixed (0.93 (93%)) |

**Issue Summary:** Feature request for ARM (armv7/arm64) Alpine Linux prebuilt libraries, motivated by Jellyfin users — but these RIDs (linux-musl-arm64, linux-musl-arm) already ship in SkiaSharp.NativeAssets.Linux and SkiaSharp.NativeAssets.Linux.NoDependencies.

**Analysis:** The reporter believes SkiaSharp only provides x86_64 Alpine libraries, but SkiaSharp.NativeAssets.Linux (and .NoDependencies) already include linux-musl-arm64 and linux-musl-arm RIDs — covering both arm64 and armv7 Alpine use cases.

**Recommendations:** **close-as-fixed** — SkiaSharp.NativeAssets.Linux already includes linux-musl-arm64 and linux-musl-arm RIDs built from Alpine output. The requested feature exists in current releases.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/libSkiaSharp.native |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/feature-request |

## Evidence

### Reproduction

**Environment:** Alpine Linux ARM (armv7/arm64), Jellyfin server

**Repository links:**
- https://www.nuget.org/packages?q=SkiaSharp+Alpine — Two unmaintained third-party NuGet packages for Alpine SkiaSharp mentioned by reporter

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.97 (97%) |
| Reason | The SkiaSharp.NativeAssets.Linux csproj explicitly includes linux-musl-arm64 and linux-musl-arm RIDs sourced from output/native/alpine/arm64 and output/native/alpine/arm. SkiaSharp.NativeAssets.Linux.NoDependencies has the same matrix. The feature already exists. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

The reporter believes SkiaSharp only provides x86_64 Alpine libraries, but SkiaSharp.NativeAssets.Linux (and .NoDependencies) already include linux-musl-arm64 and linux-musl-arm RIDs — covering both arm64 and armv7 Alpine use cases.

### Rationale

Code investigation of the NativeAssets.Linux csproj confirms that linux-musl-arm64 and linux-musl-arm are explicitly packaged from output/native/alpine/. This is exactly what Jellyfin and similar Alpine ARM workloads need. The issue can be closed as already implemented; the reporter was likely looking at an older release or third-party packages.

### Key Signals

- "there's currently only x86_64 Alpine libraries provided" — **issue body** (Reporter's premise is incorrect — arm64 and arm musl variants are already included in the official packages.)
- "There's two unmaintained nuget packages" — **issue body** (Reporter was looking at third-party packages, not the official SkiaSharp.NativeAssets.Linux which includes Alpine ARM.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.Linux/SkiaSharp.NativeAssets.Linux.csproj` | — | direct | Package includes linux-musl-arm64 from output/native/alpine/arm64 and linux-musl-arm from output/native/alpine/arm — Alpine ARM and ARM64 are already supported. |
| `documentation/dev/packages.md` | — | direct | Documentation explicitly states: 'Linux (x64, x86, arm, arm64, riscv64, loongarch64). Both glibc and musl (Alpine) variants.' — ARM Alpine is already documented as supported. |

### Workarounds

- Use SkiaSharp.NativeAssets.Linux.NoDependencies for Alpine ARM containers — it ships linux-musl-arm64 and linux-musl-arm with no external dependencies.
- Use SkiaSharp.NativeAssets.Linux if fontconfig-based system font enumeration is needed.

### Resolution Proposals

**Hypothesis:** The feature already exists. The reporter was likely checking third-party/outdated packages or an older SkiaSharp release, not the current official NuGet packages.

1. **Close with explanation** — fix, confidence 0.95 (95%), cost/xs, validated=untested
   - Close the issue explaining that SkiaSharp.NativeAssets.Linux and SkiaSharp.NativeAssets.Linux.NoDependencies already ship linux-musl-arm64 and linux-musl-arm RIDs. Point to the packages.md documentation for Alpine container guidance.

**Recommended proposal:** Close with explanation

**Why:** The csproj file confirms ARM Alpine support is already in the official packages — the issue is already resolved.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.93 (93%) |
| Reason | SkiaSharp.NativeAssets.Linux already includes linux-musl-arm64 and linux-musl-arm RIDs built from Alpine output. The requested feature exists in current releases. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply feature-request, native, Linux, and compatibility labels | labels=type/feature-request, area/libSkiaSharp.native, os/Linux, tenet/compatibility |
| add-comment | high | 0.93 (93%) | Post response explaining ARM Alpine support already exists in official packages | — |
| close-issue | medium | 0.93 (93%) | Close as completed — ARM Alpine support already ships in official packages | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Good news — Alpine ARM support is already available in the official SkiaSharp packages!

Both `SkiaSharp.NativeAssets.Linux` and `SkiaSharp.NativeAssets.Linux.NoDependencies` ship prebuilt binaries for:
- `linux-musl-arm64` — Alpine ARM64
- `linux-musl-arm` — Alpine ARMv7

For Alpine containers (like running Jellyfin on ARM), we recommend:

```xml
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="3.x.x" />
```

The `NoDependencies` variant has zero external dependencies (no fontconfig required), which makes it ideal for minimal Alpine containers. See our [Linux packages documentation](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/packages.md#linux-package-selection-guide) for the full selection guide.

If you're still seeing issues on Alpine ARM, please open a new issue with your specific error, .NET version, and Alpine version.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3097,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T23:25:04Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "Feature request for ARM (armv7/arm64) Alpine Linux prebuilt libraries, motivated by Jellyfin users — but these RIDs (linux-musl-arm64, linux-musl-arm) already ship in SkiaSharp.NativeAssets.Linux and SkiaSharp.NativeAssets.Linux.NoDependencies.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.9
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
    "reproEvidence": {
      "environmentDetails": "Alpine Linux ARM (armv7/arm64), Jellyfin server",
      "repoLinks": [
        {
          "url": "https://www.nuget.org/packages?q=SkiaSharp+Alpine",
          "description": "Two unmaintained third-party NuGet packages for Alpine SkiaSharp mentioned by reporter"
        }
      ]
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.97,
      "reason": "The SkiaSharp.NativeAssets.Linux csproj explicitly includes linux-musl-arm64 and linux-musl-arm RIDs sourced from output/native/alpine/arm64 and output/native/alpine/arm. SkiaSharp.NativeAssets.Linux.NoDependencies has the same matrix. The feature already exists."
    }
  },
  "analysis": {
    "summary": "The reporter believes SkiaSharp only provides x86_64 Alpine libraries, but SkiaSharp.NativeAssets.Linux (and .NoDependencies) already include linux-musl-arm64 and linux-musl-arm RIDs — covering both arm64 and armv7 Alpine use cases.",
    "rationale": "Code investigation of the NativeAssets.Linux csproj confirms that linux-musl-arm64 and linux-musl-arm are explicitly packaged from output/native/alpine/. This is exactly what Jellyfin and similar Alpine ARM workloads need. The issue can be closed as already implemented; the reporter was likely looking at an older release or third-party packages.",
    "keySignals": [
      {
        "text": "there's currently only x86_64 Alpine libraries provided",
        "source": "issue body",
        "interpretation": "Reporter's premise is incorrect — arm64 and arm musl variants are already included in the official packages."
      },
      {
        "text": "There's two unmaintained nuget packages",
        "source": "issue body",
        "interpretation": "Reporter was looking at third-party packages, not the official SkiaSharp.NativeAssets.Linux which includes Alpine ARM."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.Linux/SkiaSharp.NativeAssets.Linux.csproj",
        "finding": "Package includes linux-musl-arm64 from output/native/alpine/arm64 and linux-musl-arm from output/native/alpine/arm — Alpine ARM and ARM64 are already supported.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "finding": "Documentation explicitly states: 'Linux (x64, x86, arm, arm64, riscv64, loongarch64). Both glibc and musl (Alpine) variants.' — ARM Alpine is already documented as supported.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Use SkiaSharp.NativeAssets.Linux.NoDependencies for Alpine ARM containers — it ships linux-musl-arm64 and linux-musl-arm with no external dependencies.",
      "Use SkiaSharp.NativeAssets.Linux if fontconfig-based system font enumeration is needed."
    ],
    "resolution": {
      "hypothesis": "The feature already exists. The reporter was likely checking third-party/outdated packages or an older SkiaSharp release, not the current official NuGet packages.",
      "proposals": [
        {
          "title": "Close with explanation",
          "description": "Close the issue explaining that SkiaSharp.NativeAssets.Linux and SkiaSharp.NativeAssets.Linux.NoDependencies already ship linux-musl-arm64 and linux-musl-arm RIDs. Point to the packages.md documentation for Alpine container guidance.",
          "category": "fix",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Close with explanation",
      "recommendedReason": "The csproj file confirms ARM Alpine support is already in the official packages — the issue is already resolved."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.93,
      "reason": "SkiaSharp.NativeAssets.Linux already includes linux-musl-arm64 and linux-musl-arm RIDs built from Alpine output. The requested feature exists in current releases.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, native, Linux, and compatibility labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/libSkiaSharp.native",
          "os/Linux",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post response explaining ARM Alpine support already exists in official packages",
        "risk": "high",
        "confidence": 0.93,
        "comment": "Good news — Alpine ARM support is already available in the official SkiaSharp packages!\n\nBoth `SkiaSharp.NativeAssets.Linux` and `SkiaSharp.NativeAssets.Linux.NoDependencies` ship prebuilt binaries for:\n- `linux-musl-arm64` — Alpine ARM64\n- `linux-musl-arm` — Alpine ARMv7\n\nFor Alpine containers (like running Jellyfin on ARM), we recommend:\n\n```xml\n<PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"3.x.x\" />\n```\n\nThe `NoDependencies` variant has zero external dependencies (no fontconfig required), which makes it ideal for minimal Alpine containers. See our [Linux packages documentation](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/packages.md#linux-package-selection-guide) for the full selection guide.\n\nIf you're still seeing issues on Alpine ARM, please open a new issue with your specific error, .NET version, and Alpine version."
      },
      {
        "type": "close-issue",
        "description": "Close as completed — ARM Alpine support already ships in official packages",
        "risk": "medium",
        "confidence": 0.93,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
