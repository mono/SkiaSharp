# Issue Triage Report — #3511

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:16:08Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/Build (0.90 (90%)) |
| Suggested action | ready-to-fix (0.92 (92%)) |

**Issue Summary:** libmicrohttpd is still listed in External-Dependency-Info.txt (lines 1475–1988) even though PR #3463 removed the dependency from Skia; the cgmanifest.json was already cleaned up but the notices file and CredScanSuppressions.json were not updated.

**Analysis:** The External-Dependency-Info.txt compliance/notices file was not updated when PR #3463 removed libmicrohttpd from Skia. The libmicrohttpd section (514 lines of LGPL text, lines 1475–1988) is still present. The cgmanifest.json is already clean (no microhttpd entry), but External-Dependency-Info.txt and scripts/guardian/CredScanSuppressions.json (6 entries referencing microhttpd submodule paths) were overlooked. The fix is to remove the stale section from both files.

**Recommendations:** **ready-to-fix** — Root cause is clear: stale libmicrohttpd section in External-Dependency-Info.txt was not cleaned up when PR #3463 removed the dependency. Fix is a simple file edit with well-defined scope. cgmanifest.json confirms the dependency is genuinely gone.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/Build |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Open External-Dependency-Info.txt in the release/3.119.3-preview.1 branch
2. Search for 'libmicrohttpd'
3. Observe the section starting at line 1475 containing 514 lines of LGPL license text
4. Cross-reference with cgmanifest.json — libmicrohttpd entry is absent there
5. Cross-reference with PR #3463 which removed libmicrohttpd from Skia

**Environment:** SkiaSharp 3.119.3-preview.1 (release branch); libmicrohttpd was removed in PR #3463

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/3463 — PR #3463: removed libmicrohttpd dependency from Skia (skiaserve tool removed)
- https://github.com/mono/SkiaSharp/blob/release/3.119.3-preview.1/External-Dependency-Info.txt#L1475 — Stale libmicrohttpd section in External-Dependency-Info.txt

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | libmicrohttpd section (lines 1475–1988) still present in External-Dependency-Info.txt after dependency removal via PR #3463 |
| Repro quality | complete |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.119.3-preview.1, 3.116.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The libmicrohttpd section is still present in the current main branch External-Dependency-Info.txt at lines 1475–1988, confirmed by direct grep. |

## Analysis

### Technical Summary

The External-Dependency-Info.txt compliance/notices file was not updated when PR #3463 removed libmicrohttpd from Skia. The libmicrohttpd section (514 lines of LGPL text, lines 1475–1988) is still present. The cgmanifest.json is already clean (no microhttpd entry), but External-Dependency-Info.txt and scripts/guardian/CredScanSuppressions.json (6 entries referencing microhttpd submodule paths) were overlooked. The fix is to remove the stale section from both files.

### Rationale

This is a clear documentation/compliance bug: the dependency tracking file is out of sync with the actual dependency state. cgmanifest.json was correctly cleaned up but External-Dependency-Info.txt was missed. The fix path is unambiguous — remove the libmicrohttpd section from External-Dependency-Info.txt and the stale suppressions from CredScanSuppressions.json. Classified as type/bug area/Build because these are release build artifacts that are factually incorrect.

### Key Signals

- "External-Dependency-Info.txt still has '# libmicrohttpd' at line 1475 through '# END: libmicrohttpd' at line 1988" — **direct file inspection** (Over 500 lines of stale LGPL license text still present; the section was not removed when the dependency was dropped.)
- "cgmanifest.json contains no libmicrohttpd entry" — **direct file inspection** (The component manifest was correctly cleaned up, confirming the dependency is gone — External-Dependency-Info.txt just wasn't updated in the same pass.)
- "scripts/guardian/CredScanSuppressions.json has 6 entries referencing externals\skia\third_party\externals\microhttpd\... paths" — **direct file inspection** (Stale credential-scan suppressions for the removed dependency also need cleanup.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `External-Dependency-Info.txt` | 1475-1988 | direct | Full libmicrohttpd section present: begins with '# libmicrohttpd' header and ends with '# END: libmicrohttpd'. Contains 514 lines of LGPL v2.1 license text. Should be removed as the dependency is no longer used. |
| `cgmanifest.json` | — | direct | No libmicrohttpd registration found — the component governance manifest was already cleaned up correctly. This confirms the dependency is gone. |
| `scripts/guardian/CredScanSuppressions.json` | — | related | 6 suppression entries still reference microhttpd paths under externals/skia/third_party/externals/microhttpd/. These should be removed along with the External-Dependency-Info.txt cleanup. |

### Next Questions

- Does the skia submodule (externals/skia) still contain the microhttpd directory? If so, the CredScanSuppressions.json entries may still be necessary until the submodule is updated.
- Is there an automated process that regenerates External-Dependency-Info.txt, or is it maintained manually?

### Resolution Proposals

**Hypothesis:** The External-Dependency-Info.txt and CredScanSuppressions.json files were not updated as part of the same PR (#3463) that removed the libmicrohttpd dependency, leaving stale entries in two compliance-related files.

1. **Remove libmicrohttpd section from External-Dependency-Info.txt** — fix, confidence 0.95 (95%), cost/xs, validated=untested
   - Delete lines 1475–1988 (the entire '# libmicrohttpd' through '# END: libmicrohttpd' block, including the surrounding separator lines) from External-Dependency-Info.txt.
2. **Remove microhttpd entries from CredScanSuppressions.json** — fix, confidence 0.80 (80%), cost/xs, validated=untested
   - Remove the 6 suppression entries in scripts/guardian/CredScanSuppressions.json that reference externals/skia/third_party/externals/microhttpd/ paths, provided the submodule no longer includes that directory.

**Recommended proposal:** Remove libmicrohttpd section from External-Dependency-Info.txt

**Why:** The primary reported issue. Simple single-file edit with high confidence. The CredScanSuppressions.json cleanup should accompany it but depends on confirming the submodule state.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.92 (92%) |
| Reason | Root cause is clear: stale libmicrohttpd section in External-Dependency-Info.txt was not cleaned up when PR #3463 removed the dependency. Fix is a simple file edit with well-defined scope. cgmanifest.json confirms the dependency is genuinely gone. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug and Build area labels | labels=type/bug, area/Build |
| add-comment | medium | 0.92 (92%) | Confirm the issue and describe the fix needed | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for reporting this! Confirmed: the `libmicrohttpd` section (lines 1475–1988) is still present in `External-Dependency-Info.txt` even though `cgmanifest.json` was already cleaned up.

The fix involves:
1. Removing the `# libmicrohttpd` through `# END: libmicrohttpd` block (lines 1475–1988) from `External-Dependency-Info.txt`.
2. Optionally removing the 6 stale entries referencing `externals\skia\third_party\externals\microhttpd\` from `scripts/guardian/CredScanSuppressions.json` (once confirmed the submodule directory is also gone).

This is a straightforward cleanup — the root cause is that `External-Dependency-Info.txt` was not updated as part of the same PR that removed the dependency.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3511,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:16:08Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "libmicrohttpd is still listed in External-Dependency-Info.txt (lines 1475–1988) even though PR #3463 removed the dependency from Skia; the cgmanifest.json was already cleaned up but the notices file and CredScanSuppressions.json were not updated.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.9
    }
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "libmicrohttpd section (lines 1475–1988) still present in External-Dependency-Info.txt after dependency removal via PR #3463",
      "reproQuality": "complete"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Open External-Dependency-Info.txt in the release/3.119.3-preview.1 branch",
        "Search for 'libmicrohttpd'",
        "Observe the section starting at line 1475 containing 514 lines of LGPL license text",
        "Cross-reference with cgmanifest.json — libmicrohttpd entry is absent there",
        "Cross-reference with PR #3463 which removed libmicrohttpd from Skia"
      ],
      "environmentDetails": "SkiaSharp 3.119.3-preview.1 (release branch); libmicrohttpd was removed in PR #3463",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/3463",
          "description": "PR #3463: removed libmicrohttpd dependency from Skia (skiaserve tool removed)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/blob/release/3.119.3-preview.1/External-Dependency-Info.txt#L1475",
          "description": "Stale libmicrohttpd section in External-Dependency-Info.txt"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.119.3-preview.1",
        "3.116.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The libmicrohttpd section is still present in the current main branch External-Dependency-Info.txt at lines 1475–1988, confirmed by direct grep."
    }
  },
  "analysis": {
    "summary": "The External-Dependency-Info.txt compliance/notices file was not updated when PR #3463 removed libmicrohttpd from Skia. The libmicrohttpd section (514 lines of LGPL text, lines 1475–1988) is still present. The cgmanifest.json is already clean (no microhttpd entry), but External-Dependency-Info.txt and scripts/guardian/CredScanSuppressions.json (6 entries referencing microhttpd submodule paths) were overlooked. The fix is to remove the stale section from both files.",
    "rationale": "This is a clear documentation/compliance bug: the dependency tracking file is out of sync with the actual dependency state. cgmanifest.json was correctly cleaned up but External-Dependency-Info.txt was missed. The fix path is unambiguous — remove the libmicrohttpd section from External-Dependency-Info.txt and the stale suppressions from CredScanSuppressions.json. Classified as type/bug area/Build because these are release build artifacts that are factually incorrect.",
    "keySignals": [
      {
        "text": "External-Dependency-Info.txt still has '# libmicrohttpd' at line 1475 through '# END: libmicrohttpd' at line 1988",
        "source": "direct file inspection",
        "interpretation": "Over 500 lines of stale LGPL license text still present; the section was not removed when the dependency was dropped."
      },
      {
        "text": "cgmanifest.json contains no libmicrohttpd entry",
        "source": "direct file inspection",
        "interpretation": "The component manifest was correctly cleaned up, confirming the dependency is gone — External-Dependency-Info.txt just wasn't updated in the same pass."
      },
      {
        "text": "scripts/guardian/CredScanSuppressions.json has 6 entries referencing externals\\skia\\third_party\\externals\\microhttpd\\... paths",
        "source": "direct file inspection",
        "interpretation": "Stale credential-scan suppressions for the removed dependency also need cleanup."
      }
    ],
    "codeInvestigation": [
      {
        "file": "External-Dependency-Info.txt",
        "lines": "1475-1988",
        "finding": "Full libmicrohttpd section present: begins with '# libmicrohttpd' header and ends with '# END: libmicrohttpd'. Contains 514 lines of LGPL v2.1 license text. Should be removed as the dependency is no longer used.",
        "relevance": "direct"
      },
      {
        "file": "cgmanifest.json",
        "finding": "No libmicrohttpd registration found — the component governance manifest was already cleaned up correctly. This confirms the dependency is gone.",
        "relevance": "direct"
      },
      {
        "file": "scripts/guardian/CredScanSuppressions.json",
        "finding": "6 suppression entries still reference microhttpd paths under externals/skia/third_party/externals/microhttpd/. These should be removed along with the External-Dependency-Info.txt cleanup.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Does the skia submodule (externals/skia) still contain the microhttpd directory? If so, the CredScanSuppressions.json entries may still be necessary until the submodule is updated.",
      "Is there an automated process that regenerates External-Dependency-Info.txt, or is it maintained manually?"
    ],
    "resolution": {
      "hypothesis": "The External-Dependency-Info.txt and CredScanSuppressions.json files were not updated as part of the same PR (#3463) that removed the libmicrohttpd dependency, leaving stale entries in two compliance-related files.",
      "proposals": [
        {
          "title": "Remove libmicrohttpd section from External-Dependency-Info.txt",
          "description": "Delete lines 1475–1988 (the entire '# libmicrohttpd' through '# END: libmicrohttpd' block, including the surrounding separator lines) from External-Dependency-Info.txt.",
          "category": "fix",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Remove microhttpd entries from CredScanSuppressions.json",
          "description": "Remove the 6 suppression entries in scripts/guardian/CredScanSuppressions.json that reference externals/skia/third_party/externals/microhttpd/ paths, provided the submodule no longer includes that directory.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Remove libmicrohttpd section from External-Dependency-Info.txt",
      "recommendedReason": "The primary reported issue. Simple single-file edit with high confidence. The CredScanSuppressions.json cleanup should accompany it but depends on confirming the submodule state."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.92,
      "reason": "Root cause is clear: stale libmicrohttpd section in External-Dependency-Info.txt was not cleaned up when PR #3463 removed the dependency. Fix is a simple file edit with well-defined scope. cgmanifest.json confirms the dependency is genuinely gone.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug and Build area labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/Build"
        ]
      },
      {
        "type": "add-comment",
        "description": "Confirm the issue and describe the fix needed",
        "risk": "medium",
        "confidence": 0.92,
        "comment": "Thanks for reporting this! Confirmed: the `libmicrohttpd` section (lines 1475–1988) is still present in `External-Dependency-Info.txt` even though `cgmanifest.json` was already cleaned up.\n\nThe fix involves:\n1. Removing the `# libmicrohttpd` through `# END: libmicrohttpd` block (lines 1475–1988) from `External-Dependency-Info.txt`.\n2. Optionally removing the 6 stale entries referencing `externals\\skia\\third_party\\externals\\microhttpd\\` from `scripts/guardian/CredScanSuppressions.json` (once confirmed the submodule directory is also gone).\n\nThis is a straightforward cleanup — the root cause is that `External-Dependency-Info.txt` was not updated as part of the same PR that removed the dependency."
      }
    ]
  }
}
```

</details>
