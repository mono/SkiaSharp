# Issue Triage Report — #2542

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T07:09:00Z |
| Type | type/enhancement (0.92 (92%)) |
| Area | area/libSkiaSharp.native (0.95 (95%)) |
| Suggested action | keep-open (0.88 (88%)) |

**Issue Summary:** Maintainer-created tracking issue to review and remove obsolete/compatibility types (SkColorTable, SkXMLWriter, SkMask, SkCompatPaint) that were added to the mono/skia native fork over the years.

**Analysis:** This is an internal maintenance tracking issue filed by the project maintainer (mattleibow) to audit and remove obsolete compatibility shim types that were added to the mono/skia native library fork. Three of four named items (SkColorTable, SkXMLWriter, SkMask) are marked done; SkCompatPaint was found to not be obsolete. One open item remains: running a diff to find any other compatibility additions.

**Recommendations:** **keep-open** — Maintainer tracking issue with one open action item remaining (diff to find additional compat types). Not ready to close until the diff is complete.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/libSkiaSharp.native |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Review compatibility types added to mono/skia over the years
2. Remove or retain each one based on whether it is still needed

**Environment:** mono/skia native fork; no specific platform

## Analysis

### Technical Summary

This is an internal maintenance tracking issue filed by the project maintainer (mattleibow) to audit and remove obsolete compatibility shim types that were added to the mono/skia native library fork. Three of four named items (SkColorTable, SkXMLWriter, SkMask) are marked done; SkCompatPaint was found to not be obsolete. One open item remains: running a diff to find any other compatibility additions.

### Rationale

Filed by the maintainer as a cleanup tracking issue. Not a user-reported bug or external request. Classification as type/enhancement reflects intentional removal of technical debt. Area is libSkiaSharp.native since the types reside in the C++ fork. The tenet/compatibility applies because removing these shims may affect downstream consumers if not handled carefully.

### Key Signals

- "- [x] SkColorTable
- [x] SkXMLWriter
- [x] SkMask
- [ ] do a diff and see what was added" — **issue body** (Three named items resolved; one open action item remains (comprehensive diff to discover any other compatibility shims).)
- "~`SkCompatPaint`~ _(it was not obsolete)_" — **issue body** (Maintainer investigated SkCompatPaint and determined it should be kept — confirms careful per-type analysis is being done.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPaint.cs` | 45 | direct | Comment references SkCompatPaint in context of font state copying — SkCompatPaint is still referenced in C# binding comments, indicating the type was retained (it was not obsolete as noted in the issue checklist). |
| `externals/skia` | — | direct | Skia submodule is not checked out in this workspace (empty). The compatibility types (SkColorTable, SkXMLWriter, SkMask) listed in the checklist reside in the native C++ fork. The checklist shows these three are already resolved as of 2023-08-16. |

### Next Questions

- Has the diff been run to identify any remaining compatibility types beyond the four named ones?
- Are there any C# binding references that would break if additional types are removed?

### Resolution Proposals

**Hypothesis:** The majority of named compatibility types have already been reviewed and removed. The remaining open item is a comprehensive diff to catch any others.

1. **Run diff on mono/skia to find remaining compat additions** — investigation, confidence 0.90 (90%), cost/s, validated=untested
   - Diff the mono/skia fork against upstream Skia to enumerate all compatibility shims added beyond those already reviewed.

**Recommended proposal:** Run diff on mono/skia to find remaining compat additions

**Why:** This is the only open checklist item and is required before the issue can be closed.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.88 (88%) |
| Reason | Maintainer tracking issue with one open action item remaining (diff to find additional compat types). Not ready to close until the diff is complete. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply enhancement and native labels | labels=type/enhancement, area/libSkiaSharp.native, tenet/compatibility |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2542,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T07:09:00Z"
  },
  "summary": "Maintainer-created tracking issue to review and remove obsolete/compatibility types (SkColorTable, SkXMLWriter, SkMask, SkCompatPaint) that were added to the mono/skia native fork over the years.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.92
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Review compatibility types added to mono/skia over the years",
        "Remove or retain each one based on whether it is still needed"
      ],
      "environmentDetails": "mono/skia native fork; no specific platform"
    }
  },
  "analysis": {
    "summary": "This is an internal maintenance tracking issue filed by the project maintainer (mattleibow) to audit and remove obsolete compatibility shim types that were added to the mono/skia native library fork. Three of four named items (SkColorTable, SkXMLWriter, SkMask) are marked done; SkCompatPaint was found to not be obsolete. One open item remains: running a diff to find any other compatibility additions.",
    "rationale": "Filed by the maintainer as a cleanup tracking issue. Not a user-reported bug or external request. Classification as type/enhancement reflects intentional removal of technical debt. Area is libSkiaSharp.native since the types reside in the C++ fork. The tenet/compatibility applies because removing these shims may affect downstream consumers if not handled carefully.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "lines": "45",
        "finding": "Comment references SkCompatPaint in context of font state copying — SkCompatPaint is still referenced in C# binding comments, indicating the type was retained (it was not obsolete as noted in the issue checklist).",
        "relevance": "direct"
      },
      {
        "file": "externals/skia",
        "finding": "Skia submodule is not checked out in this workspace (empty). The compatibility types (SkColorTable, SkXMLWriter, SkMask) listed in the checklist reside in the native C++ fork. The checklist shows these three are already resolved as of 2023-08-16.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "- [x] SkColorTable\n- [x] SkXMLWriter\n- [x] SkMask\n- [ ] do a diff and see what was added",
        "source": "issue body",
        "interpretation": "Three named items resolved; one open action item remains (comprehensive diff to discover any other compatibility shims)."
      },
      {
        "text": "~`SkCompatPaint`~ _(it was not obsolete)_",
        "source": "issue body",
        "interpretation": "Maintainer investigated SkCompatPaint and determined it should be kept — confirms careful per-type analysis is being done."
      }
    ],
    "nextQuestions": [
      "Has the diff been run to identify any remaining compatibility types beyond the four named ones?",
      "Are there any C# binding references that would break if additional types are removed?"
    ],
    "resolution": {
      "hypothesis": "The majority of named compatibility types have already been reviewed and removed. The remaining open item is a comprehensive diff to catch any others.",
      "proposals": [
        {
          "title": "Run diff on mono/skia to find remaining compat additions",
          "description": "Diff the mono/skia fork against upstream Skia to enumerate all compatibility shims added beyond those already reviewed.",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Run diff on mono/skia to find remaining compat additions",
      "recommendedReason": "This is the only open checklist item and is required before the issue can be closed."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.88,
      "reason": "Maintainer tracking issue with one open action item remaining (diff to find additional compat types). Not ready to close until the diff is complete.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement and native labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/enhancement",
          "area/libSkiaSharp.native",
          "tenet/compatibility"
        ]
      }
    ]
  }
}
```

</details>
