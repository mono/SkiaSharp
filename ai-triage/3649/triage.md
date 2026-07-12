# Issue Triage Report — #3649

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-12T05:16:00Z |
| Type | type/enhancement (0.92 (92%)) |
| Area | area/Build (0.95 (95%)) |
| Suggested action | keep-open (0.88 (88%)) |

**Issue Summary:** Audit of googlesource.com-hosted Skia/ANGLE dependencies, identifying which can be migrated to GitHub upstreams and which still require mirroring, motivated by a googlesource.com outage.

**Analysis:** Issue #3649 is an infrastructure/build reliability enhancement request triggered by a googlesource.com outage. The author (mattleibow) has performed a structured audit of all enabled DEPS entries in externals/skia and ANGLE submodules used by native/winui-angle/build.cake. Three categories are identified: (1) 12 deps that can be switched to GitHub upstreams immediately because the exact pinned SHAs resolve there; (2) 5 deps where a GitHub upstream exists but the Chromium/AOSP-specific SHA doesn't resolve there, requiring a pin update; (3) 7 deps still hosted on chromium.googlesource.com or similar that need direct mirroring. The issue is well-specified and actionable but requires careful incremental DEPS editing and SHA verification.

**Recommendations:** **keep-open** — Well-specified, self-contained infrastructure audit with a clear action plan. The issue itself serves as the tracking document. It should stay open until all three phases are completed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/Build |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability |
| Perf | — |
| Partner | — |

## Evidence

### Reproduction

## Analysis

### Technical Summary

Issue #3649 is an infrastructure/build reliability enhancement request triggered by a googlesource.com outage. The author (mattleibow) has performed a structured audit of all enabled DEPS entries in externals/skia and ANGLE submodules used by native/winui-angle/build.cake. Three categories are identified: (1) 12 deps that can be switched to GitHub upstreams immediately because the exact pinned SHAs resolve there; (2) 5 deps where a GitHub upstream exists but the Chromium/AOSP-specific SHA doesn't resolve there, requiring a pin update; (3) 7 deps still hosted on chromium.googlesource.com or similar that need direct mirroring. The issue is well-specified and actionable but requires careful incremental DEPS editing and SHA verification.

### Rationale

Classified as type/enhancement (area/Build) because this is a reliability improvement to the build infrastructure — switching dependency URLs to reduce outage risk. It does not fix broken functionality; it improves supply chain resilience. The tenet/reliability label applies because the motivation is outage resistance. suggestedAction is keep-open because the issue is well-specified and actionable but represents a multi-step infrastructure task; it is not a simple fix and should be tracked on the backlog.

### Key Signals

- "A recent googlesource.com outage exposed that we still depend on Google-hosted repos for the current Skia / ANGLE setup." — **issue body** (Clear motivation: build outage risk due to external infrastructure dependency.)
- "12 deps can switch to upstream GitHub now (exact pinned SHA resolves there)" — **issue body - section 1** (Immediately actionable subset requiring only DEPS URL changes.)
- "5 deps have a GitHub upstream but current pin is Google/Chromium-specific" — **issue body - section 2** (Requires both URL and SHA update — more risk, needs coordinated dependency bump.)
- "7 deps still Google-hosted / should be mirrored directly" — **issue body - section 3** (Requires either setting up mirrors or accepting continued googlesource dependency for these.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `native/winui-angle/build.cake` | 1-40 | direct | sync-ANGLE task clones ANGLE from github.com/google/angle and then syncs 7 submodules: build, testing, third_party/zlib, third_party/jsoncpp, third_party/vulkan-deps, third_party/astc-encoder/src, tools/clang. These submodule URLs resolve to chromium.googlesource.com, which is the source of the outage risk. |
| `scripts/VERSIONS.txt` | — | direct | Skia is pinned to milestone m151 and ANGLE to chromium/6275. These milestones determine which SHAs are embedded in externals/skia/DEPS and the ANGLE clone. Any dep migration must be validated against these specific milestone pins. |
| `externals/skia/DEPS` | — | context | File is not checked out in this environment (externals/skia submodule not initialized). However, the issue body provides an authoritative breakdown of the enabled entries and their current SHA pins, which is sufficient for triage. |

### Resolution Proposals

**Hypothesis:** Migrate eligible DEPS entries from googlesource.com to GitHub upstreams in multiple phases: first the 12 immediately-switchable entries, then the 5 requiring a pin update, and finally plan mirroring for the 7 remaining Google-hosted entries.

1. **Phase 1: Switch 12 deps to GitHub upstreams (URL-only change)** — fix, cost/m, validated=untested
   - Update externals/skia/DEPS to replace the 12 googlesource-mirrored entries with their GitHub upstream URLs (same SHAs, just different base URL). This is the lowest-risk change.
2. **Phase 2: Bump 5 Chromium/AOSP-specific dep pins to GitHub-resolvable SHAs** — fix, cost/l, validated=untested
   - For the 5 deps where the current SHA is Chromium/AOSP-specific, coordinate a minor dependency version bump to a SHA that exists in the official GitHub repo, then update DEPS URLs.
3. **Phase 3: Investigate mirroring for 7 remaining Google-hosted deps** — investigation, cost/l, validated=untested
   - Evaluate hosting options for the 7 remaining chromium.googlesource.com-only deps (depot_tools, Skia buildtools, dng_sdk, ANGLE build/testing/vulkan-deps/tools/clang). Options include mirrors in mono/ org or accepting continued dependency on Google infrastructure.

**Recommended proposal:** Phase 1 switch for the 12 immediately-actionable deps as a first PR, then track phases 2 and 3 as follow-up issues.

**Why:** Reduces the most dependencies with the least risk in a single PR. Phase 1 requires no SHA changes — only URL substitutions.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.88 (88%) |
| Reason | Well-specified, self-contained infrastructure audit with a clear action plan. The issue itself serves as the tracking document. It should stay open until all three phases are completed. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply type/enhancement, area/Build, tenet/reliability | labels=type/enhancement, area/Build, tenet/reliability |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3649,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-12T05:16:00Z"
  },
  "summary": "Audit of googlesource.com-hosted Skia/ANGLE dependencies, identifying which can be migrated to GitHub upstreams and which still require mirroring, motivated by a googlesource.com outage.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.92
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [],
      "attachments": [],
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "Issue #3649 is an infrastructure/build reliability enhancement request triggered by a googlesource.com outage. The author (mattleibow) has performed a structured audit of all enabled DEPS entries in externals/skia and ANGLE submodules used by native/winui-angle/build.cake. Three categories are identified: (1) 12 deps that can be switched to GitHub upstreams immediately because the exact pinned SHAs resolve there; (2) 5 deps where a GitHub upstream exists but the Chromium/AOSP-specific SHA doesn't resolve there, requiring a pin update; (3) 7 deps still hosted on chromium.googlesource.com or similar that need direct mirroring. The issue is well-specified and actionable but requires careful incremental DEPS editing and SHA verification.",
    "codeInvestigation": [
      {
        "file": "native/winui-angle/build.cake",
        "finding": "sync-ANGLE task clones ANGLE from github.com/google/angle and then syncs 7 submodules: build, testing, third_party/zlib, third_party/jsoncpp, third_party/vulkan-deps, third_party/astc-encoder/src, tools/clang. These submodule URLs resolve to chromium.googlesource.com, which is the source of the outage risk.",
        "relevance": "direct",
        "lines": "1-40"
      },
      {
        "file": "scripts/VERSIONS.txt",
        "finding": "Skia is pinned to milestone m151 and ANGLE to chromium/6275. These milestones determine which SHAs are embedded in externals/skia/DEPS and the ANGLE clone. Any dep migration must be validated against these specific milestone pins.",
        "relevance": "direct"
      },
      {
        "file": "externals/skia/DEPS",
        "finding": "File is not checked out in this environment (externals/skia submodule not initialized). However, the issue body provides an authoritative breakdown of the enabled entries and their current SHA pins, which is sufficient for triage.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "A recent googlesource.com outage exposed that we still depend on Google-hosted repos for the current Skia / ANGLE setup.",
        "source": "issue body",
        "interpretation": "Clear motivation: build outage risk due to external infrastructure dependency."
      },
      {
        "text": "12 deps can switch to upstream GitHub now (exact pinned SHA resolves there)",
        "source": "issue body - section 1",
        "interpretation": "Immediately actionable subset requiring only DEPS URL changes."
      },
      {
        "text": "5 deps have a GitHub upstream but current pin is Google/Chromium-specific",
        "source": "issue body - section 2",
        "interpretation": "Requires both URL and SHA update — more risk, needs coordinated dependency bump."
      },
      {
        "text": "7 deps still Google-hosted / should be mirrored directly",
        "source": "issue body - section 3",
        "interpretation": "Requires either setting up mirrors or accepting continued googlesource dependency for these."
      }
    ],
    "rationale": "Classified as type/enhancement (area/Build) because this is a reliability improvement to the build infrastructure — switching dependency URLs to reduce outage risk. It does not fix broken functionality; it improves supply chain resilience. The tenet/reliability label applies because the motivation is outage resistance. suggestedAction is keep-open because the issue is well-specified and actionable but represents a multi-step infrastructure task; it is not a simple fix and should be tracked on the backlog.",
    "resolution": {
      "hypothesis": "Migrate eligible DEPS entries from googlesource.com to GitHub upstreams in multiple phases: first the 12 immediately-switchable entries, then the 5 requiring a pin update, and finally plan mirroring for the 7 remaining Google-hosted entries.",
      "proposals": [
        {
          "title": "Phase 1: Switch 12 deps to GitHub upstreams (URL-only change)",
          "category": "fix",
          "description": "Update externals/skia/DEPS to replace the 12 googlesource-mirrored entries with their GitHub upstream URLs (same SHAs, just different base URL). This is the lowest-risk change.",
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Phase 2: Bump 5 Chromium/AOSP-specific dep pins to GitHub-resolvable SHAs",
          "category": "fix",
          "description": "For the 5 deps where the current SHA is Chromium/AOSP-specific, coordinate a minor dependency version bump to a SHA that exists in the official GitHub repo, then update DEPS URLs.",
          "effort": "cost/l",
          "validated": "untested"
        },
        {
          "title": "Phase 3: Investigate mirroring for 7 remaining Google-hosted deps",
          "category": "investigation",
          "description": "Evaluate hosting options for the 7 remaining chromium.googlesource.com-only deps (depot_tools, Skia buildtools, dng_sdk, ANGLE build/testing/vulkan-deps/tools/clang). Options include mirrors in mono/ org or accepting continued dependency on Google infrastructure.",
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Phase 1 switch for the 12 immediately-actionable deps as a first PR, then track phases 2 and 3 as follow-up issues.",
      "recommendedReason": "Reduces the most dependencies with the least risk in a single PR. Phase 1 requires no SHA changes — only URL substitutions."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.88,
      "reason": "Well-specified, self-contained infrastructure audit with a clear action plan. The issue itself serves as the tracking document. It should stay open until all three phases are completed.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/enhancement, area/Build, tenet/reliability",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/enhancement",
          "area/Build",
          "tenet/reliability"
        ]
      }
    ]
  }
}
```

</details>
