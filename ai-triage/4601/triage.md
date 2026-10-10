# Issue Triage Report — #4601

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-10-10T04:28:51Z |
| Type | type/enhancement (0.99 (99%)) |
| Area | area/libSkiaSharp.native (0.93 (93%)) |
| Suggested action | keep-open (0.97 (97%)) |

**Issue Summary:** SkiaSharp does not expose Skia VulkanBackendContext device-lost callback fields, so Vulkan consumers cannot receive Skia device-loss notifications or diagnostics through GRVkBackendContext.

**Analysis:** The managed GRVkBackendContext maps existing Vulkan fields into GRVkBackendContextNative, including the get-procedure callback proxy, but neither the public wrapper nor generated native struct contains a device-lost callback/context field. The requested additive C-shim and managed delegate plumbing is therefore still absent in the checked-out managed code. The native Skia submodule is unavailable in this workspace, so the issue's cited native line ranges could not be independently opened; the linked open PR is the implementation vehicle.

**Recommendations:** **keep-open** — The API gap is valid and an open linked PR is implementing it; retain the issue until that PR merges and ships.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/libSkiaSharp.native |
| Platforms | — |
| Backends | backend/Vulkan |
| Tenets | tenet/reliability |
| Perf | — |
| Partner | — |
| Current labels | type/enhancement |

## Evidence

### Reproduction

**Environment:** Vulkan GPU applications using GRVkBackendContext; no version, platform, runtime failure, or minimal reproduction was supplied because this is an API-surface gap.

**Related issues:** #4575, #4600, #4953

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/4655 — Open implementation PR linked by GitHub as closing this issue.
- https://github.com/mono/SkiaSharp/issues/4600 — Related GRVkBackendContext Vulkan API-version issue found in repository search.
- https://github.com/mono/SkiaSharp/issues/4953 — Related GRVkBackendContext delegate-lifetime issue found in repository search.

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | False |
| Confidence | 0.98 (98%) |
| Reason | GitHub links open PR #4655 as a closing pull request; it has not yet merged or shipped. |
| Related PRs | #4655 |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

The managed GRVkBackendContext maps existing Vulkan fields into GRVkBackendContextNative, including the get-procedure callback proxy, but neither the public wrapper nor generated native struct contains a device-lost callback/context field. The requested additive C-shim and managed delegate plumbing is therefore still absent in the checked-out managed code. The native Skia submodule is unavailable in this workspace, so the issue's cited native line ranges could not be independently opened; the linked open PR is the implementation vehicle.

### Rationale

This is an enhancement because the optional device-lost callback is missing rather than a regression or current runtime failure. It is Vulkan-specific and spans the native C shim and managed GRVkBackendContext mapping; the reliability tenet applies because the callback provides device-loss recovery diagnostics. The explicit open implementation PR means the issue should remain open until that work merges rather than be reproduced or closed.

### Key Signals

- "Both fields are optional, so this is purely additive and ABI-safe." — **issue body** (The report requests new API surface rather than correcting existing behavior.)
- "On VK_ERROR_DEVICE_LOST, Skia's callback never reaches managed code." — **issue body** (The missing surface affects Vulkan device-loss diagnostics and recovery handling.)
- "GitHub identifies open PR #4655 as closing this issue." — **GitHub issue metadata** (An implementation is in progress but has not yet shipped.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/GRVkBackendContext.cs` | 18-144 | direct | The wrapper owns only get-procedure delegate state and ToNative maps fGetProcUserData/fGetProc; it exposes no device-lost delegate or context property. |
| `binding/SkiaSharp/Generated/GRVkBackendContextNative.generated.cs` | 13-67 | direct | The generated sequential native struct ends with fProtectedContext after the existing procedure callback fields and contains no device-lost fields. |
| `binding/Binding.Shared/DelegateProxies.shared.cs` | 22-32 | related | DelegateProxies.Create allocates a GCHandle and context pointer, confirming the existing managed proxy pattern cited by the issue. |

### Workarounds

- No managed SkiaSharp device-lost callback workaround exists today; applications can only monitor Vulkan device-loss through their own Vulkan integration outside GRVkBackendContext.

### Resolution Proposals

**Hypothesis:** The C shim and managed wrapper predate Skia's optional device-lost fields, so the callback cannot cross the native/managed boundary.

1. **Review and merge the linked device-lost binding PR** — fix, confidence 0.95 (95%), cost/m, validated=untested
   - Review the additive C-shim fields, native mapping, managed delegate lifetime handling, and delegate round-trip test in #4655; merge when native and managed ABI checks pass.

**Recommended proposal:** Review and merge the linked device-lost binding PR

**Why:** The issue has a concrete, linked implementation path; no separate reproduction is needed for an intentional API addition.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.97 (97%) |
| Reason | The API gap is valid and an open linked PR is implementing it; retain the issue until that PR merges and ships. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.99 (99%) | Apply the enhancement, native, Vulkan, and reliability classification labels. | labels=type/enhancement, area/libSkiaSharp.native, backend/Vulkan, tenet/reliability |
| link-related | low | 0.83 (83%) | Keep the related Vulkan backend-context issue #4600 available for context. | linkedIssue=#4600 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4601,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-10-10T04:28:51Z",
    "currentLabels": [
      "type/enhancement"
    ]
  },
  "summary": "SkiaSharp does not expose Skia VulkanBackendContext device-lost callback fields, so Vulkan consumers cannot receive Skia device-loss notifications or diagnostics through GRVkBackendContext.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.99
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.93
    },
    "backends": [
      "backend/Vulkan"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Vulkan GPU applications using GRVkBackendContext; no version, platform, runtime failure, or minimal reproduction was supplied because this is an API-surface gap.",
      "relatedIssues": [
        4575,
        4600,
        4953
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/4655",
          "description": "Open implementation PR linked by GitHub as closing this issue."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/4600",
          "description": "Related GRVkBackendContext Vulkan API-version issue found in repository search."
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/4953",
          "description": "Related GRVkBackendContext delegate-lifetime issue found in repository search."
        }
      ]
    },
    "fixStatus": {
      "likelyFixed": false,
      "confidence": 0.98,
      "reason": "GitHub links open PR #4655 as a closing pull request; it has not yet merged or shipped.",
      "relatedPRs": [
        4655
      ]
    }
  },
  "analysis": {
    "summary": "The managed GRVkBackendContext maps existing Vulkan fields into GRVkBackendContextNative, including the get-procedure callback proxy, but neither the public wrapper nor generated native struct contains a device-lost callback/context field. The requested additive C-shim and managed delegate plumbing is therefore still absent in the checked-out managed code. The native Skia submodule is unavailable in this workspace, so the issue's cited native line ranges could not be independently opened; the linked open PR is the implementation vehicle.",
    "rationale": "This is an enhancement because the optional device-lost callback is missing rather than a regression or current runtime failure. It is Vulkan-specific and spans the native C shim and managed GRVkBackendContext mapping; the reliability tenet applies because the callback provides device-loss recovery diagnostics. The explicit open implementation PR means the issue should remain open until that work merges rather than be reproduced or closed.",
    "keySignals": [
      {
        "text": "Both fields are optional, so this is purely additive and ABI-safe.",
        "source": "issue body",
        "interpretation": "The report requests new API surface rather than correcting existing behavior."
      },
      {
        "text": "On VK_ERROR_DEVICE_LOST, Skia's callback never reaches managed code.",
        "source": "issue body",
        "interpretation": "The missing surface affects Vulkan device-loss diagnostics and recovery handling."
      },
      {
        "text": "GitHub identifies open PR #4655 as closing this issue.",
        "source": "GitHub issue metadata",
        "interpretation": "An implementation is in progress but has not yet shipped."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/GRVkBackendContext.cs",
        "lines": "18-144",
        "finding": "The wrapper owns only get-procedure delegate state and ToNative maps fGetProcUserData/fGetProc; it exposes no device-lost delegate or context property.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Generated/GRVkBackendContextNative.generated.cs",
        "lines": "13-67",
        "finding": "The generated sequential native struct ends with fProtectedContext after the existing procedure callback fields and contains no device-lost fields.",
        "relevance": "direct"
      },
      {
        "file": "binding/Binding.Shared/DelegateProxies.shared.cs",
        "lines": "22-32",
        "finding": "DelegateProxies.Create allocates a GCHandle and context pointer, confirming the existing managed proxy pattern cited by the issue.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "No managed SkiaSharp device-lost callback workaround exists today; applications can only monitor Vulkan device-loss through their own Vulkan integration outside GRVkBackendContext."
    ],
    "resolution": {
      "hypothesis": "The C shim and managed wrapper predate Skia's optional device-lost fields, so the callback cannot cross the native/managed boundary.",
      "proposals": [
        {
          "title": "Review and merge the linked device-lost binding PR",
          "description": "Review the additive C-shim fields, native mapping, managed delegate lifetime handling, and delegate round-trip test in #4655; merge when native and managed ABI checks pass.",
          "category": "fix",
          "validated": "untested",
          "confidence": 0.95,
          "effort": "cost/m"
        }
      ],
      "recommendedProposal": "Review and merge the linked device-lost binding PR",
      "recommendedReason": "The issue has a concrete, linked implementation path; no separate reproduction is needed for an intentional API addition."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.97,
      "reason": "The API gap is valid and an open linked PR is implementing it; retain the issue until that PR merges and ships.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply the enhancement, native, Vulkan, and reliability classification labels.",
        "risk": "low",
        "confidence": 0.99,
        "labels": [
          "type/enhancement",
          "area/libSkiaSharp.native",
          "backend/Vulkan",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-related",
        "description": "Keep the related Vulkan backend-context issue #4600 available for context.",
        "risk": "low",
        "confidence": 0.83,
        "linkedIssue": 4600
      }
    ]
  }
}
```

</details>
