# Issue Triage Report — #3240

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T17:21:11Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/libSkiaSharp.native (0.95 (95%)) |
| Suggested action | ready-to-fix (0.93 (93%)) |

**Issue Summary:** Windows native build passes /DEBUG:FULL to the linker without re-enabling /OPT:REF and /OPT:ICF, causing larger release binaries due to disabled dead-code elimination and identical-code folding.

**Analysis:** The Windows Cake build script passes /DEBUG:FULL in linker flags which, per MSVC documentation, overrides /OPT defaults from REF to NOREF and from ICF to NOICF. This means release builds of libSkiaSharp.dll skip dead-code elimination (/OPT:REF) and identical-code folding (/OPT:ICF), resulting in unnecessarily large binaries. The fix is straightforward: append /OPT:REF,ICF to extra_ldflags.

**Recommendations:** **ready-to-fix** — Root cause is confirmed in source code. Fix is a one-line change adding /OPT:REF,ICF to extra_ldflags in native/windows/build.cake.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/performance |
| Partner | — |

## Evidence

### Reproduction

**Code snippets:**

```csharp
extra_ldflags=[ '/DEBUG:FULL', '/DEBUGTYPE:CV,FIXUP', '/guard:cf', ... ]
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | performance |
| Error message | — |
| Repro quality | complete |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.118.0-preview.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The build script line 80 confirmed to still contain /DEBUG:FULL without /OPT:REF or /OPT:ICF in extra_ldflags. |

## Analysis

### Technical Summary

The Windows Cake build script passes /DEBUG:FULL in linker flags which, per MSVC documentation, overrides /OPT defaults from REF to NOREF and from ICF to NOICF. This means release builds of libSkiaSharp.dll skip dead-code elimination (/OPT:REF) and identical-code folding (/OPT:ICF), resulting in unnecessarily large binaries. The fix is straightforward: append /OPT:REF,ICF to extra_ldflags.

### Rationale

The issue is a clear, verifiable build configuration bug. The MSVC linker documentation confirms the behavior. The code investigation found the exact line where the fix is needed. No repro ambiguity; the fix is to add /OPT:REF,ICF to extra_ldflags in native/windows/build.cake.

### Key Signals

- "/DEBUG changes the defaults for the /OPT option from REF to NOREF and from ICF to NOICF" — **issue body** (MSVC linker documentation confirms that /DEBUG:FULL disables dead-code removal and identical-code folding unless explicitly re-enabled.)
- "extra_ldflags=[ '/DEBUG:FULL', '/DEBUGTYPE:CV,FIXUP', '/guard:cf', ... ]" — **native/windows/build.cake line 80** (Confirmed: /OPT:REF and /OPT:ICF are absent from the linker flags list.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `native/windows/build.cake` | 80 | direct | extra_ldflags contains '/DEBUG:FULL' but does not include '/OPT:REF' or '/OPT:ICF'. According to MSVC docs, /DEBUG changes defaults to NOREF and NOICF. |

### Resolution Proposals

**Hypothesis:** Adding /OPT:REF,ICF after /DEBUG:FULL in extra_ldflags will restore linker optimizations for release builds.

1. **Add /OPT:REF,ICF to extra_ldflags in native/windows/build.cake** — fix, cost/xs, validated=untested
   - Append '/OPT:REF,ICF' to the extra_ldflags array after '/DEBUG:FULL' in native/windows/build.cake line 80 to re-enable dead-code elimination and identical-code folding for release builds.

**Recommended proposal:** fix-1

**Why:** Straightforward one-line change in the build script, directly matches what the reporter requests, and is confirmed by MSVC documentation.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.93 (93%) |
| Reason | Root cause is confirmed in source code. Fix is a one-line change adding /OPT:REF,ICF to extra_ldflags in native/windows/build.cake. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/libSkiaSharp.native, os/Windows-Classic, tenet/performance | labels=type/bug, area/libSkiaSharp.native, os/Windows-Classic, tenet/performance |
| add-comment | medium | 0.90 (90%) | Acknowledge the valid bug report and indicate the fix path. | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report! This is a valid build configuration issue.

The MSVC linker documentation confirms that `/DEBUG` overrides `/OPT` defaults from `REF` to `NOREF` and `ICF` to `NOICF`. As a result, release builds of `libSkiaSharp.dll` skip dead-code elimination and identical-code folding, producing unnecessarily large binaries.

The fix is to add `/OPT:REF,ICF` to the `extra_ldflags` in `native/windows/build.cake` (line 80).
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3240,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T17:21:11Z"
  },
  "summary": "Windows native build passes /DEBUG:FULL to the linker without re-enabling /OPT:REF and /OPT:ICF, causing larger release binaries due to disabled dead-code elimination and identical-code folding.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "low",
      "regressionClaimed": false,
      "errorType": "performance",
      "reproQuality": "complete"
    },
    "reproEvidence": {
      "codeSnippets": [
        "extra_ldflags=[ '/DEBUG:FULL', '/DEBUGTYPE:CV,FIXUP', '/guard:cf', ... ]"
      ],
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.118.0-preview.2"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The build script line 80 confirmed to still contain /DEBUG:FULL without /OPT:REF or /OPT:ICF in extra_ldflags."
    }
  },
  "analysis": {
    "summary": "The Windows Cake build script passes /DEBUG:FULL in linker flags which, per MSVC documentation, overrides /OPT defaults from REF to NOREF and from ICF to NOICF. This means release builds of libSkiaSharp.dll skip dead-code elimination (/OPT:REF) and identical-code folding (/OPT:ICF), resulting in unnecessarily large binaries. The fix is straightforward: append /OPT:REF,ICF to extra_ldflags.",
    "codeInvestigation": [
      {
        "file": "native/windows/build.cake",
        "lines": "80",
        "finding": "extra_ldflags contains '/DEBUG:FULL' but does not include '/OPT:REF' or '/OPT:ICF'. According to MSVC docs, /DEBUG changes defaults to NOREF and NOICF.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "/DEBUG changes the defaults for the /OPT option from REF to NOREF and from ICF to NOICF",
        "source": "issue body",
        "interpretation": "MSVC linker documentation confirms that /DEBUG:FULL disables dead-code removal and identical-code folding unless explicitly re-enabled."
      },
      {
        "text": "extra_ldflags=[ '/DEBUG:FULL', '/DEBUGTYPE:CV,FIXUP', '/guard:cf', ... ]",
        "source": "native/windows/build.cake line 80",
        "interpretation": "Confirmed: /OPT:REF and /OPT:ICF are absent from the linker flags list."
      }
    ],
    "rationale": "The issue is a clear, verifiable build configuration bug. The MSVC linker documentation confirms the behavior. The code investigation found the exact line where the fix is needed. No repro ambiguity; the fix is to add /OPT:REF,ICF to extra_ldflags in native/windows/build.cake.",
    "resolution": {
      "hypothesis": "Adding /OPT:REF,ICF after /DEBUG:FULL in extra_ldflags will restore linker optimizations for release builds.",
      "proposals": [
        {
          "title": "Add /OPT:REF,ICF to extra_ldflags in native/windows/build.cake",
          "category": "fix",
          "effort": "cost/xs",
          "validated": "untested",
          "description": "Append '/OPT:REF,ICF' to the extra_ldflags array after '/DEBUG:FULL' in native/windows/build.cake line 80 to re-enable dead-code elimination and identical-code folding for release builds."
        }
      ],
      "recommendedProposal": "fix-1",
      "recommendedReason": "Straightforward one-line change in the build script, directly matches what the reporter requests, and is confirmed by MSVC documentation."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.93,
      "reason": "Root cause is confirmed in source code. Fix is a one-line change adding /OPT:REF,ICF to extra_ldflags in native/windows/build.cake.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/libSkiaSharp.native, os/Windows-Classic, tenet/performance",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Windows-Classic",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the valid bug report and indicate the fix path.",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed report! This is a valid build configuration issue.\n\nThe MSVC linker documentation confirms that `/DEBUG` overrides `/OPT` defaults from `REF` to `NOREF` and `ICF` to `NOICF`. As a result, release builds of `libSkiaSharp.dll` skip dead-code elimination and identical-code folding, producing unnecessarily large binaries.\n\nThe fix is to add `/OPT:REF,ICF` to the `extra_ldflags` in `native/windows/build.cake` (line 80)."
      }
    ]
  }
}
```

</details>
