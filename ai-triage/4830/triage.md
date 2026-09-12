# Issue Triage Report — #4830

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-09-12T04:25:06Z |
| Type | type/feature-request (0.98 (98%)) |
| Area | area/libSkiaSharp.native (0.90 (90%)) |
| Suggested action | ready-to-fix (0.88 (88%)) |

**Issue Summary:** SkiaSharp and HarfBuzzSharp do not publish Linux ppc64le native assets, preventing an Avalonia-based v2rayN application from running on that architecture despite an available .NET runtime.

**Analysis:** This is a compatibility feature request for a missing Linux RID asset, not a rendering defect. Current Linux native package manifests and the native CI matrix omit ppc64le, while the repository's shared .NET cross-build infrastructure recognizes that architecture. The linked open SkiaSharp and mono/skia PRs provide a concrete, reportedly tested implementation path.

**Recommendations:** **ready-to-fix** — The missing RID assets and CI coverage are confirmed in source, and linked open PRs already define the cross-repository implementation path.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/libSkiaSharp.native |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/compatibility |
| Perf | — |
| Partner | — |
| Current labels | type/feature-request |

## Evidence

### Reproduction

1. Run an application that depends on SkiaSharp and HarfBuzzSharp on Linux ppc64le.
2. Restore the official packages.
3. Observe that no ppc64le native runtime assets are available.

**Environment:** Linux ppc64le (PowerPC 64-bit Little Endian); the reporter cites an IBM .NET 10.0.10 runtime.

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/4824 — Open SkiaSharp implementation PR for ppc64le build and packaging support.
- https://github.com/mono/skia/pull/347 — Open native Skia implementation PR required by the SkiaSharp change.
- https://github.com/autorepobot/SkiaSharp-ppc64le/releases/tag/v4.151.1 — Community ppc64le package release reported as working in the issue comments.
- https://github.com/autorepobot/SkiaSharp-ppc64le/releases/tag/v3.119.4 — Community ppc64le package release reported as working in the issue comments.
- https://github.com/mono/SkiaSharp/issues/1584 — Related open request for an unsupported Linux native architecture (mips64).
- https://github.com/mono/SkiaSharp/issues/90 — Closed historical Linux support issue.
- https://github.com/mono/SkiaSharp/issues/365 — Closed historical Linux ARM support issue.

## Analysis

### Technical Summary

This is a compatibility feature request for a missing Linux RID asset, not a rendering defect. Current Linux native package manifests and the native CI matrix omit ppc64le, while the repository's shared .NET cross-build infrastructure recognizes that architecture. The linked open SkiaSharp and mono/skia PRs provide a concrete, reportedly tested implementation path.

### Rationale

The reporter requests a new ppc64le runtime flavor rather than reporting incorrect behavior from a supported runtime. Source inspection confirms that both Linux asset packages enumerate supported RIDs without ppc64le and that the CI matrix does not build it, establishing the current gap. ppc64le is already recognized by shared cross-build support, making the existing linked implementation PRs the appropriate route for resolution.

### Key Signals

- "there is currently no ppc64le runtime available for SkiaSharp and HarfBuzzSharp" — **issue body** (The requested capability is absent from the published native assets.)
- "I've opened PRs implementing this: mono/skia#347; mono/SkiaSharp#4824" — **issue body** (A concrete native and packaging implementation is already awaiting review.)
- "I built the NuGet package for ppc64le, and it looks good." — **issue comment** (A community build provides limited validation of feasibility, but not release-quality CI validation.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.Linux/SkiaSharp.NativeAssets.Linux.csproj` | 9-24 | direct | The glibc and musl PackageFile entries cover x64, x86, arm64, arm, riscv64, and loongarch64, but no linux-ppc64le libSkiaSharp asset. |
| `binding/HarfBuzzSharp.NativeAssets.Linux/HarfBuzzSharp.NativeAssets.Linux.csproj` | 9-24 | direct | The Linux HarfBuzzSharp package mirrors the existing architecture set and likewise has no linux-ppc64le libHarfBuzzSharp asset. |
| `scripts/azure-templates-stages-native-linux.yml` | 46-68 | direct | The glibc native Linux CI matrix builds arm64, x86, arm, x64, riscv64, and loongarch64; ppc64le is not scheduled. |
| `scripts/infra/native/linux/docker/glibc/Dockerfile` | 19,27-31 | direct | The glibc cross-image selects only arm, arm64, x64, riscv64, and loongarch64 and explicitly requires each architecture's .NET cross image and libc++ headers; this is the build-environment gap the linked PR addresses. |
| `eng/common/cross/build-rootfs.sh` | 8,199-202 | related | Shared cross-build rootfs handling recognizes ppc64le, so architecture normalization is not absent across the repository. |

### Workarounds

- Use the community ppc64le NuGet packages linked in the issue comment until official packages are published, after independently reviewing their provenance and compatibility.
- Build native libSkiaSharp and libHarfBuzzSharp from the linked implementation branches when a controlled build environment is available.

### Next Questions

- Can the linked PRs be rebased onto the current native build infrastructure and verified by official Linux ppc64le CI?
- Should the official package support only the requested glibc RID initially, with musl deferred because a suitable toolchain is unavailable?

### Resolution Proposals

**Hypothesis:** Adding ppc64le requires coordinated native cross-image support, CI matrix coverage, and Linux package entries for both libSkiaSharp and libHarfBuzzSharp.

1. **Review and update the linked ppc64le implementation** — fix, confidence 0.88 (88%), cost/m, validated=untested
   - Review mono/skia#347 and mono/SkiaSharp#4824, update them for the current native build infrastructure, then add official glibc ppc64le CI and package validation.
2. **Use community-built ppc64le packages temporarily** — workaround, confidence 0.65 (65%), cost/xs, validated=untested
   - For immediate deployment, use the linked community ppc64le package releases only after assessing their source and version compatibility; this does not replace official package support.

**Recommended proposal:** Review and update the linked ppc64le implementation

**Why:** It closes the verified package and CI gaps using an existing coordinated native and managed implementation, rather than relying on unofficial artifacts.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.88 (88%) |
| Reason | The missing RID assets and CI coverage are confirmed in source, and linked open PRs already define the cross-repository implementation path. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.98 (98%) | Apply feature-request, native library, Linux, and compatibility labels. | labels=type/feature-request, area/libSkiaSharp.native, os/Linux, tenet/compatibility |
| link-related | low | 0.80 (80%) | Cross-reference the related unsupported Linux architecture request. | linkedIssue=#1584 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4830,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-09-12T04:25:06Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "SkiaSharp and HarfBuzzSharp do not publish Linux ppc64le native assets, preventing an Avalonia-based v2rayN application from running on that architecture despite an available .NET runtime.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.98
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
      "stepsToReproduce": [
        "Run an application that depends on SkiaSharp and HarfBuzzSharp on Linux ppc64le.",
        "Restore the official packages.",
        "Observe that no ppc64le native runtime assets are available."
      ],
      "environmentDetails": "Linux ppc64le (PowerPC 64-bit Little Endian); the reporter cites an IBM .NET 10.0.10 runtime.",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/4824",
          "description": "Open SkiaSharp implementation PR for ppc64le build and packaging support."
        },
        {
          "url": "https://github.com/mono/skia/pull/347",
          "description": "Open native Skia implementation PR required by the SkiaSharp change."
        },
        {
          "url": "https://github.com/autorepobot/SkiaSharp-ppc64le/releases/tag/v4.151.1",
          "description": "Community ppc64le package release reported as working in the issue comments."
        },
        {
          "url": "https://github.com/autorepobot/SkiaSharp-ppc64le/releases/tag/v3.119.4",
          "description": "Community ppc64le package release reported as working in the issue comments."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1584",
          "description": "Related open request for an unsupported Linux native architecture (mips64)."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/90",
          "description": "Closed historical Linux support issue."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/365",
          "description": "Closed historical Linux ARM support issue."
        }
      ]
    }
  },
  "analysis": {
    "summary": "This is a compatibility feature request for a missing Linux RID asset, not a rendering defect. Current Linux native package manifests and the native CI matrix omit ppc64le, while the repository's shared .NET cross-build infrastructure recognizes that architecture. The linked open SkiaSharp and mono/skia PRs provide a concrete, reportedly tested implementation path.",
    "rationale": "The reporter requests a new ppc64le runtime flavor rather than reporting incorrect behavior from a supported runtime. Source inspection confirms that both Linux asset packages enumerate supported RIDs without ppc64le and that the CI matrix does not build it, establishing the current gap. ppc64le is already recognized by shared cross-build support, making the existing linked implementation PRs the appropriate route for resolution.",
    "keySignals": [
      {
        "text": "there is currently no ppc64le runtime available for SkiaSharp and HarfBuzzSharp",
        "source": "issue body",
        "interpretation": "The requested capability is absent from the published native assets."
      },
      {
        "text": "I've opened PRs implementing this: mono/skia#347; mono/SkiaSharp#4824",
        "source": "issue body",
        "interpretation": "A concrete native and packaging implementation is already awaiting review."
      },
      {
        "text": "I built the NuGet package for ppc64le, and it looks good.",
        "source": "issue comment",
        "interpretation": "A community build provides limited validation of feasibility, but not release-quality CI validation."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.Linux/SkiaSharp.NativeAssets.Linux.csproj",
        "lines": "9-24",
        "finding": "The glibc and musl PackageFile entries cover x64, x86, arm64, arm, riscv64, and loongarch64, but no linux-ppc64le libSkiaSharp asset.",
        "relevance": "direct"
      },
      {
        "file": "binding/HarfBuzzSharp.NativeAssets.Linux/HarfBuzzSharp.NativeAssets.Linux.csproj",
        "lines": "9-24",
        "finding": "The Linux HarfBuzzSharp package mirrors the existing architecture set and likewise has no linux-ppc64le libHarfBuzzSharp asset.",
        "relevance": "direct"
      },
      {
        "file": "scripts/azure-templates-stages-native-linux.yml",
        "lines": "46-68",
        "finding": "The glibc native Linux CI matrix builds arm64, x86, arm, x64, riscv64, and loongarch64; ppc64le is not scheduled.",
        "relevance": "direct"
      },
      {
        "file": "scripts/infra/native/linux/docker/glibc/Dockerfile",
        "lines": "19,27-31",
        "finding": "The glibc cross-image selects only arm, arm64, x64, riscv64, and loongarch64 and explicitly requires each architecture's .NET cross image and libc++ headers; this is the build-environment gap the linked PR addresses.",
        "relevance": "direct"
      },
      {
        "file": "eng/common/cross/build-rootfs.sh",
        "lines": "8,199-202",
        "finding": "Shared cross-build rootfs handling recognizes ppc64le, so architecture normalization is not absent across the repository.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use the community ppc64le NuGet packages linked in the issue comment until official packages are published, after independently reviewing their provenance and compatibility.",
      "Build native libSkiaSharp and libHarfBuzzSharp from the linked implementation branches when a controlled build environment is available."
    ],
    "nextQuestions": [
      "Can the linked PRs be rebased onto the current native build infrastructure and verified by official Linux ppc64le CI?",
      "Should the official package support only the requested glibc RID initially, with musl deferred because a suitable toolchain is unavailable?"
    ],
    "resolution": {
      "hypothesis": "Adding ppc64le requires coordinated native cross-image support, CI matrix coverage, and Linux package entries for both libSkiaSharp and libHarfBuzzSharp.",
      "proposals": [
        {
          "title": "Review and update the linked ppc64le implementation",
          "description": "Review mono/skia#347 and mono/SkiaSharp#4824, update them for the current native build infrastructure, then add official glibc ppc64le CI and package validation.",
          "category": "fix",
          "confidence": 0.88,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Use community-built ppc64le packages temporarily",
          "description": "For immediate deployment, use the linked community ppc64le package releases only after assessing their source and version compatibility; this does not replace official package support.",
          "category": "workaround",
          "confidence": 0.65,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Review and update the linked ppc64le implementation",
      "recommendedReason": "It closes the verified package and CI gaps using an existing coordinated native and managed implementation, rather than relying on unofficial artifacts."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.88,
      "reason": "The missing RID assets and CI coverage are confirmed in source, and linked open PRs already define the cross-repository implementation path.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, native library, Linux, and compatibility labels.",
        "risk": "low",
        "confidence": 0.98,
        "labels": [
          "type/feature-request",
          "area/libSkiaSharp.native",
          "os/Linux",
          "tenet/compatibility"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference the related unsupported Linux architecture request.",
        "risk": "low",
        "confidence": 0.8,
        "linkedIssue": 1584
      }
    ]
  }
}
```

</details>
