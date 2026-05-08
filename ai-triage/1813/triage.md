# Issue Triage Report — #1813

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T22:46:00Z |
| Type | type/feature-request (0.95 (95%)) |
| Area | area/libSkiaSharp.native (0.90 (90%)) |
| Suggested action | close-as-fixed (0.88 (88%)) |

**Issue Summary:** Feature request asking for Alpine Linux arm64 (linux-musl-arm64) native library support, which has since been implemented in the SkiaSharp.NativeAssets.Linux.NoDependencies package.

**Analysis:** Alpine Linux arm64 native support has been implemented. The SkiaSharp.NativeAssets.Linux.NoDependencies package includes the linux-musl-arm64 runtime identifier, and the CI build infrastructure builds both alpine and alpine-nodeps arm64 variants.

**Recommendations:** **close-as-fixed** — The requested Alpine arm64 native library support has been implemented in SkiaSharp.NativeAssets.Linux.NoDependencies since the issue was filed in 2021.

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

## Evidence

### Reproduction

**Environment:** Alpine Linux arm64 — musl libc

**Related issues:** #3097, #453

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3097 — Related 2024 feature request: Prebuilt ARM libraries for Alpine Linux (still open)
- https://github.com/mono/SkiaSharp/issues/453 — Earlier closed request: More Pre-Built Linux Libraries

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.90 (90%) |
| Reason | The NativeAssets.Linux.NoDependencies package now includes linux-musl-arm64 runtime, and CI pipelines build native_linux_arm64_alpine targets. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

Alpine Linux arm64 native support has been implemented. The SkiaSharp.NativeAssets.Linux.NoDependencies package includes the linux-musl-arm64 runtime identifier, and the CI build infrastructure builds both alpine and alpine-nodeps arm64 variants.

### Rationale

This 2021 feature request asked for Alpine (musl libc) arm64 native binaries. Code investigation confirms linux-musl-arm64 is now included in SkiaSharp.NativeAssets.Linux.NoDependencies, and CI pipeline definitions confirm native_linux_arm64_alpine builds are produced. The feature is implemented and the issue can be closed as fixed.

### Key Signals

- "Currently not available in alpine-arm64" — **issue body** (At the time of filing (2021), no alpine arm64 support existed. This has since changed.)
- "PackageFile Include=...alpinenodeps\arm64..." — **SkiaSharp.NativeAssets.Linux.NoDependencies.csproj** (Explicit arm64 Alpine (no-deps) build output included in the package — feature implemented.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.Linux.NoDependencies/SkiaSharp.NativeAssets.Linux.NoDependencies.csproj` | — | direct | PackageFile entry maps output/native/alpinenodeps/arm64/libSkiaSharp*.so to runtimes/linux-musl-arm64/native — arm64 Alpine support is included in the package. |
| `scripts/azure-templates-stages-native-merge.yml` | — | direct | CI artifact merge stages list native_linux_arm64_alpine_linux and native_linux_arm64_alpine_nodeps_linux — both are built in CI pipelines. |
| `binding/SkiaSharp.NativeAssets.Linux.NoDependencies/buildTransitive/net4/SkiaSharp.targets` | — | related | Build targets reference linux-musl-arm64 runtime path for native library deployment, confirming the package supports arm64 Alpine at build-time. |

### Workarounds

- Use SkiaSharp.NativeAssets.Linux.NoDependencies NuGet package which includes linux-musl-arm64 runtime support for Alpine Linux arm64.

### Resolution Proposals

**Hypothesis:** The requested alpine-arm64 support has been implemented and is distributed via SkiaSharp.NativeAssets.Linux.NoDependencies.

1. **Close as fixed** — fix, confidence 0.90 (90%), cost/xs, validated=untested
   - Close the issue as the requested feature (Alpine arm64 native libraries) is now shipped in SkiaSharp.NativeAssets.Linux.NoDependencies.

**Recommended proposal:** Close as fixed

**Why:** Code evidence confirms linux-musl-arm64 is packaged and CI builds the alpine arm64 native binaries.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.88 (88%) |
| Reason | The requested Alpine arm64 native library support has been implemented in SkiaSharp.NativeAssets.Linux.NoDependencies since the issue was filed in 2021. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply feature-request, native, linux, and compatibility labels | labels=type/feature-request, area/libSkiaSharp.native, os/Linux, tenet/compatibility |
| add-comment | medium | 0.88 (88%) | Inform reporter that Alpine arm64 support has been implemented | — |
| close-issue | medium | 0.88 (88%) | Close as completed since the feature has been implemented | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Good news — Alpine Linux arm64 (`linux-musl-arm64`) support has been implemented since this issue was filed. You can use the [SkiaSharp.NativeAssets.Linux.NoDependencies](https://www.nuget.org/packages/SkiaSharp.NativeAssets.Linux.NoDependencies) NuGet package which includes prebuilt `linux-musl-arm64` native libraries for Alpine arm64 environments.

If you're still experiencing issues with this, please open a new issue with your specific environment details and SkiaSharp version.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1813,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T22:46:00Z"
  },
  "summary": "Feature request asking for Alpine Linux arm64 (linux-musl-arm64) native library support, which has since been implemented in the SkiaSharp.NativeAssets.Linux.NoDependencies package.",
  "classification": {
    "type": {
      "value": "type/feature-request",
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
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Alpine Linux arm64 — musl libc",
      "relatedIssues": [
        3097,
        453
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3097",
          "description": "Related 2024 feature request: Prebuilt ARM libraries for Alpine Linux (still open)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/453",
          "description": "Earlier closed request: More Pre-Built Linux Libraries"
        }
      ]
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.9,
      "reason": "The NativeAssets.Linux.NoDependencies package now includes linux-musl-arm64 runtime, and CI pipelines build native_linux_arm64_alpine targets."
    }
  },
  "analysis": {
    "summary": "Alpine Linux arm64 native support has been implemented. The SkiaSharp.NativeAssets.Linux.NoDependencies package includes the linux-musl-arm64 runtime identifier, and the CI build infrastructure builds both alpine and alpine-nodeps arm64 variants.",
    "rationale": "This 2021 feature request asked for Alpine (musl libc) arm64 native binaries. Code investigation confirms linux-musl-arm64 is now included in SkiaSharp.NativeAssets.Linux.NoDependencies, and CI pipeline definitions confirm native_linux_arm64_alpine builds are produced. The feature is implemented and the issue can be closed as fixed.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.Linux.NoDependencies/SkiaSharp.NativeAssets.Linux.NoDependencies.csproj",
        "finding": "PackageFile entry maps output/native/alpinenodeps/arm64/libSkiaSharp*.so to runtimes/linux-musl-arm64/native — arm64 Alpine support is included in the package.",
        "relevance": "direct"
      },
      {
        "file": "scripts/azure-templates-stages-native-merge.yml",
        "finding": "CI artifact merge stages list native_linux_arm64_alpine_linux and native_linux_arm64_alpine_nodeps_linux — both are built in CI pipelines.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.Linux.NoDependencies/buildTransitive/net4/SkiaSharp.targets",
        "finding": "Build targets reference linux-musl-arm64 runtime path for native library deployment, confirming the package supports arm64 Alpine at build-time.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Currently not available in alpine-arm64",
        "source": "issue body",
        "interpretation": "At the time of filing (2021), no alpine arm64 support existed. This has since changed."
      },
      {
        "text": "PackageFile Include=...alpinenodeps\\arm64...",
        "source": "SkiaSharp.NativeAssets.Linux.NoDependencies.csproj",
        "interpretation": "Explicit arm64 Alpine (no-deps) build output included in the package — feature implemented."
      }
    ],
    "workarounds": [
      "Use SkiaSharp.NativeAssets.Linux.NoDependencies NuGet package which includes linux-musl-arm64 runtime support for Alpine Linux arm64."
    ],
    "resolution": {
      "hypothesis": "The requested alpine-arm64 support has been implemented and is distributed via SkiaSharp.NativeAssets.Linux.NoDependencies.",
      "proposals": [
        {
          "title": "Close as fixed",
          "description": "Close the issue as the requested feature (Alpine arm64 native libraries) is now shipped in SkiaSharp.NativeAssets.Linux.NoDependencies.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Close as fixed",
      "recommendedReason": "Code evidence confirms linux-musl-arm64 is packaged and CI builds the alpine arm64 native binaries."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.88,
      "reason": "The requested Alpine arm64 native library support has been implemented in SkiaSharp.NativeAssets.Linux.NoDependencies since the issue was filed in 2021.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, native, linux, and compatibility labels",
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
        "description": "Inform reporter that Alpine arm64 support has been implemented",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Good news — Alpine Linux arm64 (`linux-musl-arm64`) support has been implemented since this issue was filed. You can use the [SkiaSharp.NativeAssets.Linux.NoDependencies](https://www.nuget.org/packages/SkiaSharp.NativeAssets.Linux.NoDependencies) NuGet package which includes prebuilt `linux-musl-arm64` native libraries for Alpine arm64 environments.\n\nIf you're still experiencing issues with this, please open a new issue with your specific environment details and SkiaSharp version."
      },
      {
        "type": "close-issue",
        "description": "Close as completed since the feature has been implemented",
        "risk": "medium",
        "confidence": 0.88,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
