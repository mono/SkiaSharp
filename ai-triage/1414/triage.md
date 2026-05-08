# Issue Triage Report — #1414

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T19:24:00Z |
| Type | type/feature-request (0.92 (92%)) |
| Area | area/Build (0.88 (88%)) |
| Suggested action | needs-investigation (0.78 (78%)) |

**Issue Summary:** Feature request to integrate the WASM interpreter 'cookie' script into SkiaSharp's CI pipeline to detect P/Invoke interop signatures missing from the Mono WASM tuner's InterpToNativeGenerator.cs.

**Analysis:** Partial implementation exists (CookieDetector + CookieCommand in utils/SkiaSharpGenerator/Cookies/), but CI integration was never completed. The feature targets the old Mono WASM interpreter path; modern SkiaSharp WASM uses Emscripten directly and this mechanism may be obsolete.

**Recommendations:** **needs-investigation** — The CookieDetector tool was implemented but never integrated into CI. Before deciding whether to complete the integration or close as obsolete, someone needs to confirm whether the Mono WASM interpreter path is still exercised in current SkiaSharp builds.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/Build |
| Platforms | os/WASM |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Environment:** WASM / Mono interpreter (pre-.NET 6 approach). Filed 2020-07-16.

**Repository links:**
- https://github.com/mono/mono/blob/174aeaa31c9d80a53c6c7981ba7206825cd1ea4d/mcs/tools/wasm-tuner/InterpToNativeGenerator.cs#L15 — Mono wasm-tuner InterpToNativeGenerator.cs — the source of required interop cookies
- https://gist.github.com/jeromelaban/ac625399c6e79e7f9d40141f7bbf9b3a — Original cookie detection gist by jeromelaban
- https://github.com/mono/SkiaSharp/pull/1333 — PR #1333 — added a console runner to log missing cookies; partial implementation of this request

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | The Mono WASM interpreter approach (wasm-tuner) predates .NET 6 WASM. Current SkiaSharp WASM builds use Emscripten directly without the Mono interpreter, making the InterpToNativeGenerator.cs cookie mechanism obsolete for modern SkiaSharp 3.x. The associated milestone (v2.88.x Planning) is now closed. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | False |
| Confidence | 0.55 (55%) |
| Reason | A CookieDetector and CookieCommand were implemented in utils/SkiaSharpGenerator/Cookies/, addressing the detection tool aspect. However, CI pipeline integration was never completed. Whether this is still needed for the current Emscripten-based WASM build is uncertain. |
| Related PRs | #1333 |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

Partial implementation exists (CookieDetector + CookieCommand in utils/SkiaSharpGenerator/Cookies/), but CI integration was never completed. The feature targets the old Mono WASM interpreter path; modern SkiaSharp WASM uses Emscripten directly and this mechanism may be obsolete.

### Rationale

The issue requests integrating a CI check that detects P/Invoke signatures missing from the Mono wasm-tuner cookies list. A CookieDetector and CookieCommand were created as part of PR #1333 but were never wired into the CI pipeline. The request remains a feature-request rather than a bug because nothing is broken — it is a proactive quality/compatibility check. Given that SkiaSharp's WASM build now uses Emscripten directly (not the Mono interpreter), the relevance of this cookie mechanism to current builds is uncertain and warrants investigation before acting.

### Key Signals

- "Added a console runner to at least log the issues: https://github.com/mono/SkiaSharp/pull/1333 — This only really is needed when the C API changes." — **comment by mattleibow (issue #1414)** (Partial implementation via PR #1333; maintainer acknowledged CI integration was deferred. Noted it is only needed during C API changes.)
- "Right now, the mono interpreter requires that all interop methods have their signature declared here" — **issue body** (Scope is the old Mono WASM interpreter path. Modern .NET 6+ WASM via Emscripten does not use this mechanism.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `utils/SkiaSharpGenerator/Cookies/CookieDetector.cs` | 1-198 | direct | Full implementation of cookie detection: downloads InterpToNativeGenerator.cs from mono/mono, parses the cookies array, then uses Mono.Cecil to reflect over a SkiaSharp assembly and compare P/Invoke signatures. Any signatures not in the cookies list are flagged as missing. |
| `utils/SkiaSharpGenerator/Cookies/CookieCommand.cs` | 1-83 | direct | CLI command wrapper for CookieDetector, invoked as 'skiasharp-tools cookie --assembly=<path> --type=<type> --branch=<branch>'. Registered in Program.cs. Not referenced from any .cake build script or CI YAML template. |
| `utils/SkiaSharpGenerator/Program.cs` | 1-22 | related | CookieCommand is registered as a top-level command of the skiasharp-tools CLI alongside GenerateCommand and VerifyCommand. Available but no build/CI integration found. |
| `native/wasm/build.cake` | 1-60 | context | Current WASM build uses Emscripten (emcc/em++/emar). There is no reference to the cookie check, wasm-tuner, or InterpToNativeGenerator in the WASM build scripts or CI YAML templates, confirming that CI integration was never added. |

### Next Questions

- Is the Mono WASM interpreter path (wasm-tuner cookies) still exercised in any current SkiaSharp build or test scenario, or has it been entirely superseded by the Emscripten WASM build?
- Should the CookieDetector be integrated into the CI pipeline for C API changes as the maintainer suggested, or should the feature be closed as no longer applicable?

### Resolution Proposals

**Hypothesis:** The cookie validation tool was implemented but CI integration was not completed. The Mono WASM interpreter approach may now be obsolete for SkiaSharp 3.x, but that needs confirmation before deciding whether to integrate or close.

1. **Investigate relevance of Mono WASM interpreter path** — investigation, confidence 0.85 (85%), cost/xs, validated=untested
   - Check whether any current SkiaSharp consumer still targets the old Mono WASM interpreter (pre-.NET 6) and whether the InterpToNativeGenerator.cs mechanism is exercised. If the Mono interpreter path is no longer supported, close this as superseded.
2. **Integrate CookieCommand into CI (if still relevant)** — fix, confidence 0.70 (70%), cost/s, validated=untested
   - If the Mono interpreter path is still needed, wire the existing 'skiasharp-tools cookie' command into the WASM CI job (scripts/azure-templates-jobs-wasm-matrix.yml or native/wasm/build.cake) to run after each C API change and fail the build if new signatures are missing.

**Recommended proposal:** Investigate relevance of Mono WASM interpreter path

**Why:** The feature targets an old Mono WASM interpreter mechanism. Before investing in CI integration, confirm whether this path is still exercised in current builds. If not relevant, close as superseded.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.78 (78%) |
| Reason | The CookieDetector tool was implemented but never integrated into CI. Before deciding whether to complete the integration or close as obsolete, someone needs to confirm whether the Mono WASM interpreter path is still exercised in current SkiaSharp builds. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply feature-request, build area, WASM platform, and compatibility tenet labels | labels=type/feature-request, area/Build, os/WASM, tenet/compatibility |
| add-comment | medium | 0.78 (78%) | Note that partial implementation exists (CookieDetector/CookieCommand) but CI integration was never completed; ask whether the Mono WASM interpreter path is still relevant | — |

**Comment draft for `add-comment`:**

```markdown
A `CookieDetector` and `CookieCommand` were added to `utils/SkiaSharpGenerator/Cookies/` (referenced from PR #1333) to detect P/Invoke signatures missing from the Mono wasm-tuner `InterpToNativeGenerator.cs`, but this tool was never wired into the CI pipeline.

Given that current SkiaSharp WASM builds use Emscripten directly (not the Mono interpreter), could you confirm whether the Mono WASM interpreter path and the `InterpToNativeGenerator.cs` cookie mechanism are still relevant to any supported SkiaSharp target? If the Mono interpreter path has been superseded, this issue can be closed as no longer applicable. Otherwise, the next step would be integrating `skiasharp-tools cookie` into the WASM CI job.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1414,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T19:24:00Z"
  },
  "summary": "Feature request to integrate the WASM interpreter 'cookie' script into SkiaSharp's CI pipeline to detect P/Invoke interop signatures missing from the Mono WASM tuner's InterpToNativeGenerator.cs.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.92
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.88
    },
    "platforms": [
      "os/WASM"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "WASM / Mono interpreter (pre-.NET 6 approach). Filed 2020-07-16.",
      "repoLinks": [
        {
          "url": "https://github.com/mono/mono/blob/174aeaa31c9d80a53c6c7981ba7206825cd1ea4d/mcs/tools/wasm-tuner/InterpToNativeGenerator.cs#L15",
          "description": "Mono wasm-tuner InterpToNativeGenerator.cs — the source of required interop cookies"
        },
        {
          "url": "https://gist.github.com/jeromelaban/ac625399c6e79e7f9d40141f7bbf9b3a",
          "description": "Original cookie detection gist by jeromelaban"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/1333",
          "description": "PR #1333 — added a console runner to log missing cookies; partial implementation of this request"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "unlikely",
      "relevanceReason": "The Mono WASM interpreter approach (wasm-tuner) predates .NET 6 WASM. Current SkiaSharp WASM builds use Emscripten directly without the Mono interpreter, making the InterpToNativeGenerator.cs cookie mechanism obsolete for modern SkiaSharp 3.x. The associated milestone (v2.88.x Planning) is now closed."
    },
    "fixStatus": {
      "likelyFixed": false,
      "confidence": 0.55,
      "reason": "A CookieDetector and CookieCommand were implemented in utils/SkiaSharpGenerator/Cookies/, addressing the detection tool aspect. However, CI pipeline integration was never completed. Whether this is still needed for the current Emscripten-based WASM build is uncertain.",
      "relatedPRs": [
        1333
      ]
    }
  },
  "analysis": {
    "summary": "Partial implementation exists (CookieDetector + CookieCommand in utils/SkiaSharpGenerator/Cookies/), but CI integration was never completed. The feature targets the old Mono WASM interpreter path; modern SkiaSharp WASM uses Emscripten directly and this mechanism may be obsolete.",
    "rationale": "The issue requests integrating a CI check that detects P/Invoke signatures missing from the Mono wasm-tuner cookies list. A CookieDetector and CookieCommand were created as part of PR #1333 but were never wired into the CI pipeline. The request remains a feature-request rather than a bug because nothing is broken — it is a proactive quality/compatibility check. Given that SkiaSharp's WASM build now uses Emscripten directly (not the Mono interpreter), the relevance of this cookie mechanism to current builds is uncertain and warrants investigation before acting.",
    "codeInvestigation": [
      {
        "file": "utils/SkiaSharpGenerator/Cookies/CookieDetector.cs",
        "lines": "1-198",
        "finding": "Full implementation of cookie detection: downloads InterpToNativeGenerator.cs from mono/mono, parses the cookies array, then uses Mono.Cecil to reflect over a SkiaSharp assembly and compare P/Invoke signatures. Any signatures not in the cookies list are flagged as missing.",
        "relevance": "direct"
      },
      {
        "file": "utils/SkiaSharpGenerator/Cookies/CookieCommand.cs",
        "lines": "1-83",
        "finding": "CLI command wrapper for CookieDetector, invoked as 'skiasharp-tools cookie --assembly=<path> --type=<type> --branch=<branch>'. Registered in Program.cs. Not referenced from any .cake build script or CI YAML template.",
        "relevance": "direct"
      },
      {
        "file": "utils/SkiaSharpGenerator/Program.cs",
        "lines": "1-22",
        "finding": "CookieCommand is registered as a top-level command of the skiasharp-tools CLI alongside GenerateCommand and VerifyCommand. Available but no build/CI integration found.",
        "relevance": "related"
      },
      {
        "file": "native/wasm/build.cake",
        "lines": "1-60",
        "finding": "Current WASM build uses Emscripten (emcc/em++/emar). There is no reference to the cookie check, wasm-tuner, or InterpToNativeGenerator in the WASM build scripts or CI YAML templates, confirming that CI integration was never added.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "Added a console runner to at least log the issues: https://github.com/mono/SkiaSharp/pull/1333 — This only really is needed when the C API changes.",
        "source": "comment by mattleibow (issue #1414)",
        "interpretation": "Partial implementation via PR #1333; maintainer acknowledged CI integration was deferred. Noted it is only needed during C API changes."
      },
      {
        "text": "Right now, the mono interpreter requires that all interop methods have their signature declared here",
        "source": "issue body",
        "interpretation": "Scope is the old Mono WASM interpreter path. Modern .NET 6+ WASM via Emscripten does not use this mechanism."
      }
    ],
    "nextQuestions": [
      "Is the Mono WASM interpreter path (wasm-tuner cookies) still exercised in any current SkiaSharp build or test scenario, or has it been entirely superseded by the Emscripten WASM build?",
      "Should the CookieDetector be integrated into the CI pipeline for C API changes as the maintainer suggested, or should the feature be closed as no longer applicable?"
    ],
    "resolution": {
      "hypothesis": "The cookie validation tool was implemented but CI integration was not completed. The Mono WASM interpreter approach may now be obsolete for SkiaSharp 3.x, but that needs confirmation before deciding whether to integrate or close.",
      "proposals": [
        {
          "title": "Investigate relevance of Mono WASM interpreter path",
          "description": "Check whether any current SkiaSharp consumer still targets the old Mono WASM interpreter (pre-.NET 6) and whether the InterpToNativeGenerator.cs mechanism is exercised. If the Mono interpreter path is no longer supported, close this as superseded.",
          "category": "investigation",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Integrate CookieCommand into CI (if still relevant)",
          "description": "If the Mono interpreter path is still needed, wire the existing 'skiasharp-tools cookie' command into the WASM CI job (scripts/azure-templates-jobs-wasm-matrix.yml or native/wasm/build.cake) to run after each C API change and fail the build if new signatures are missing.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Investigate relevance of Mono WASM interpreter path",
      "recommendedReason": "The feature targets an old Mono WASM interpreter mechanism. Before investing in CI integration, confirm whether this path is still exercised in current builds. If not relevant, close as superseded."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.78,
      "reason": "The CookieDetector tool was implemented but never integrated into CI. Before deciding whether to complete the integration or close as obsolete, someone needs to confirm whether the Mono WASM interpreter path is still exercised in current SkiaSharp builds.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, build area, WASM platform, and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/feature-request",
          "area/Build",
          "os/WASM",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Note that partial implementation exists (CookieDetector/CookieCommand) but CI integration was never completed; ask whether the Mono WASM interpreter path is still relevant",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "A `CookieDetector` and `CookieCommand` were added to `utils/SkiaSharpGenerator/Cookies/` (referenced from PR #1333) to detect P/Invoke signatures missing from the Mono wasm-tuner `InterpToNativeGenerator.cs`, but this tool was never wired into the CI pipeline.\n\nGiven that current SkiaSharp WASM builds use Emscripten directly (not the Mono interpreter), could you confirm whether the Mono WASM interpreter path and the `InterpToNativeGenerator.cs` cookie mechanism are still relevant to any supported SkiaSharp target? If the Mono interpreter path has been superseded, this issue can be closed as no longer applicable. Otherwise, the next step would be integrating `skiasharp-tools cookie` into the WASM CI job."
      }
    ]
  }
}
```

</details>
