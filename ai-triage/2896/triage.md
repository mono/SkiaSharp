# Issue Triage Report — #2896

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T19:00:00Z |
| Type | type/bug (0.93 (93%)) |
| Area | area/SkiaSharp (0.85 (85%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** Fonts containing only TrueType Outlines (no OpenType Layout tables) are embedded as Type3 fonts instead of TTF in PDF output, observed as a regression from SkiaSharp 2.88.2 to 2.88.3.

**Analysis:** Skia's PDF backend decides whether to embed a font as CIDFont/TrueType or fall back to Type3 based on font table availability. Fonts with OpenType Layout tables (GSUB/GPOS) get embedded as TTF, while fonts with TrueType Outlines only are downgraded to Type3. This behavior appears to have changed between SkiaSharp 2.88.2 and 2.88.3 due to an upstream Skia version bump. The upstream Skia issue tracker confirms this is a known Skia PDF backend behavior.

**Recommendations:** **needs-investigation** — Real regression with identified upstream Skia bug. Needs investigation to confirm the exact Skia version change that introduced this and whether SkiaSharp can apply a targeted fix or needs to wait for upstream.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/PDF |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug, os/Windows-Classic, area/SkiaSharp, backend/PDF, tenet/compatibility, triage/triaged |

## Evidence

### Reproduction

1. Use the CreatePdfSample.cs from the gallery
2. Set Typeface to a font with TrueType Outlines only (e.g., Helvetica)
3. Generate the PDF and inspect the embedded font type

**Environment:** SkiaSharp 2.88.3, Windows, Visual Studio

**Repository links:**
- https://issues.skia.org/issues/40031251 — Upstream Skia bug: PDF font Type3 embedding for TrueType-only fonts
- https://github.com/mono/SkiaSharp/blob/master/samples/Gallery/Shared/Samples/CreatePdfSample.cs — Reference sample used to reproduce the issue

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | Font embedded as Type3 instead of TTF in PDF output when font has TrueType Outlines only (no OpenType Layout tables) |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 2.88.2 |
| Worked in | 2.88.2 |
| Broke in | 2.88.3 |
| Current relevance | likely |
| Relevance reason | The changelog for 2.88.3 shows no C# API changes, indicating the regression is in the underlying native Skia library version bump included in 2.88.3. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.80 (80%) |
| Reason | Reporter explicitly states 2.88.2 worked correctly and 2.88.3 broke it. The 2.88.3 C# changelog has no changes, pointing to a Skia native version change. |
| Worked in version | 2.88.2 |
| Broke in version | 2.88.3 |

## Analysis

### Technical Summary

Skia's PDF backend decides whether to embed a font as CIDFont/TrueType or fall back to Type3 based on font table availability. Fonts with OpenType Layout tables (GSUB/GPOS) get embedded as TTF, while fonts with TrueType Outlines only are downgraded to Type3. This behavior appears to have changed between SkiaSharp 2.88.2 and 2.88.3 due to an upstream Skia version bump. The upstream Skia issue tracker confirms this is a known Skia PDF backend behavior.

### Rationale

The issue is clearly a wrong-output bug where PDF font embedding type changed between releases. The reporter provides a concrete reproduction case (Helvetica vs Arial fonts) and identifies the distinguishing characteristic (OpenType Layout vs TrueType Outlines only). The SKDocumentPdfMetadata API has no font embedding controls, confirming SkiaSharp has no mechanism to override this Skia-internal decision. The 2.88.3 changelog is empty (no C# changes), pointing to a native Skia change as root cause.

### Key Signals

- "if the ttf font file contains 'TrueType Outlines' only then it is converted to Type3 font, but if the ttf file also contains 'OpenType Layout' then it is embedded as TTF" — **issue body** (Reporter has already identified the exact discriminating condition — presence of OpenType Layout tables determines embedding method in Skia's PDF backend.)
- "The only related issue we have found in the Skia project is this one https://issues.skia.org/issues/40031251" — **issue body** (Upstream Skia has an acknowledged bug for this behavior, confirming root cause is in the native Skia PDF writer.)
- "Last Known Good Version of SkiaSharp: 2.88.2" — **issue body** (Regression starting in 2.88.3; the C# changelog for 2.88.3 shows no API changes, so the regression is in the native Skia version included in 2.88.3.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKDocument.cs` | 136-173 | direct | SKDocument.CreatePdf passes metadata to sk_document_create_pdf_from_stream_with_metadata. SKDocumentPdfMetadata exposes RasterDpi, PdfA, EncodingQuality, and metadata strings but no font embedding controls. No mechanism exists in SkiaSharp to override Skia's internal font embedding decision. |
| `binding/SkiaSharp/Definitions.cs` | 385-456 | related | SKDocumentPdfMetadata struct contains no font embedding or typeface options — only RasterDpi, PdfA flag, EncodingQuality, and document metadata strings. Font embedding behavior is entirely controlled by Skia's PDF backend. |
| `changelogs/SkiaSharp/2.88.3/SkiaSharp.md` | 1-5 | direct | The 2.88.3 changelog reads 'No changes.' — confirming no C# API modifications. The regression must originate from the native Skia library version bump included in this release. |

### Next Questions

- What Skia commit/version was included in 2.88.3 vs 2.88.2? Did a Skia upstream change affect PDF font embedding logic?
- Does the upstream Skia bug https://issues.skia.org/issues/40031251 have a planned or available fix?
- Is the behavior reproducible on Linux/macOS or is it Windows-specific (Windows font system may influence which tables are present)?

### Resolution Proposals

**Hypothesis:** Skia's PDF backend changed its font embedding decision logic between the Skia versions used in 2.88.2 and 2.88.3, causing TrueType-only fonts to fall back to Type3 embedding. This is an upstream Skia issue with no current SkiaSharp-level workaround.

1. **Track upstream Skia fix** — investigation, confidence 0.80 (80%), cost/s, validated=untested
   - Monitor the upstream Skia bug https://issues.skia.org/issues/40031251 for a fix. When fixed upstream, the fix will flow into SkiaSharp with the next Skia version bump.
2. **Use fonts with OpenType Layout tables as workaround** — workaround, confidence 0.70 (70%), cost/s, validated=untested
   - Until the upstream fix is available, use font files that include OpenType Layout tables (GSUB/GPOS). For example, substitute affected fonts with variants that include OpenType features. Alternatively, use SKTypeface.FromFile() with a font file known to include OpenType tables.

**Recommended proposal:** Track upstream Skia fix

**Why:** The root cause is in upstream Skia; tracking the fix ensures it will be resolved properly. The font substitution workaround may not always be feasible depending on the reporter's PDF requirements.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Real regression with identified upstream Skia bug. Needs investigation to confirm the exact Skia version change that introduced this and whether SkiaSharp can apply a targeted fix or needs to wait for upstream. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/SkiaSharp, os/Windows-Classic, backend/PDF, tenet/compatibility labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/PDF, tenet/compatibility |
| add-comment | medium | 0.82 (82%) | Acknowledge the upstream Skia bug and provide context about the root cause | — |
| link-related | low | 0.90 (90%) | Reference upstream Skia issue in comments for tracking | linkedIssue=#2896 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed investigation! This matches an upstream Skia bug (https://issues.skia.org/issues/40031251) in Skia's PDF backend, which appears to have changed behavior between the Skia versions used in SkiaSharp 2.88.2 and 2.88.3.

The root cause is in Skia's PDF font embedding logic: fonts with only TrueType Outlines (no OpenType Layout tables like GSUB/GPOS) are currently embedded as Type3 fonts instead of as a TTF/CIDFont. Unfortunately, SkiaSharp's `SKDocumentPdfMetadata` API does not expose font embedding controls — this decision is made entirely inside Skia's PDF writer.

**Possible workaround:** If possible, use font files that include OpenType Layout tables, as those are embedded as TTF. The underlying Skia bug will need to be fixed upstream before this is fully resolved in SkiaSharp.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2896,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T19:00:00Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Classic",
      "area/SkiaSharp",
      "backend/PDF",
      "tenet/compatibility",
      "triage/triaged"
    ]
  },
  "summary": "Fonts containing only TrueType Outlines (no OpenType Layout tables) are embedded as Type3 fonts instead of TTF in PDF output, observed as a regression from SkiaSharp 2.88.2 to 2.88.3.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.93
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.85
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/PDF"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "Font embedded as Type3 instead of TTF in PDF output when font has TrueType Outlines only (no OpenType Layout tables)",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Use the CreatePdfSample.cs from the gallery",
        "Set Typeface to a font with TrueType Outlines only (e.g., Helvetica)",
        "Generate the PDF and inspect the embedded font type"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, Windows, Visual Studio",
      "repoLinks": [
        {
          "url": "https://issues.skia.org/issues/40031251",
          "description": "Upstream Skia bug: PDF font Type3 embedding for TrueType-only fonts"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/blob/master/samples/Gallery/Shared/Samples/CreatePdfSample.cs",
          "description": "Reference sample used to reproduce the issue"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "2.88.2"
      ],
      "workedIn": "2.88.2",
      "brokeIn": "2.88.3",
      "currentRelevance": "likely",
      "relevanceReason": "The changelog for 2.88.3 shows no C# API changes, indicating the regression is in the underlying native Skia library version bump included in 2.88.3."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.8,
      "reason": "Reporter explicitly states 2.88.2 worked correctly and 2.88.3 broke it. The 2.88.3 C# changelog has no changes, pointing to a Skia native version change.",
      "workedInVersion": "2.88.2",
      "brokeInVersion": "2.88.3"
    }
  },
  "analysis": {
    "summary": "Skia's PDF backend decides whether to embed a font as CIDFont/TrueType or fall back to Type3 based on font table availability. Fonts with OpenType Layout tables (GSUB/GPOS) get embedded as TTF, while fonts with TrueType Outlines only are downgraded to Type3. This behavior appears to have changed between SkiaSharp 2.88.2 and 2.88.3 due to an upstream Skia version bump. The upstream Skia issue tracker confirms this is a known Skia PDF backend behavior.",
    "rationale": "The issue is clearly a wrong-output bug where PDF font embedding type changed between releases. The reporter provides a concrete reproduction case (Helvetica vs Arial fonts) and identifies the distinguishing characteristic (OpenType Layout vs TrueType Outlines only). The SKDocumentPdfMetadata API has no font embedding controls, confirming SkiaSharp has no mechanism to override this Skia-internal decision. The 2.88.3 changelog is empty (no C# changes), pointing to a native Skia change as root cause.",
    "keySignals": [
      {
        "text": "if the ttf font file contains 'TrueType Outlines' only then it is converted to Type3 font, but if the ttf file also contains 'OpenType Layout' then it is embedded as TTF",
        "source": "issue body",
        "interpretation": "Reporter has already identified the exact discriminating condition — presence of OpenType Layout tables determines embedding method in Skia's PDF backend."
      },
      {
        "text": "The only related issue we have found in the Skia project is this one https://issues.skia.org/issues/40031251",
        "source": "issue body",
        "interpretation": "Upstream Skia has an acknowledged bug for this behavior, confirming root cause is in the native Skia PDF writer."
      },
      {
        "text": "Last Known Good Version of SkiaSharp: 2.88.2",
        "source": "issue body",
        "interpretation": "Regression starting in 2.88.3; the C# changelog for 2.88.3 shows no API changes, so the regression is in the native Skia version included in 2.88.3."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKDocument.cs",
        "lines": "136-173",
        "finding": "SKDocument.CreatePdf passes metadata to sk_document_create_pdf_from_stream_with_metadata. SKDocumentPdfMetadata exposes RasterDpi, PdfA, EncodingQuality, and metadata strings but no font embedding controls. No mechanism exists in SkiaSharp to override Skia's internal font embedding decision.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "385-456",
        "finding": "SKDocumentPdfMetadata struct contains no font embedding or typeface options — only RasterDpi, PdfA flag, EncodingQuality, and document metadata strings. Font embedding behavior is entirely controlled by Skia's PDF backend.",
        "relevance": "related"
      },
      {
        "file": "changelogs/SkiaSharp/2.88.3/SkiaSharp.md",
        "lines": "1-5",
        "finding": "The 2.88.3 changelog reads 'No changes.' — confirming no C# API modifications. The regression must originate from the native Skia library version bump included in this release.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "What Skia commit/version was included in 2.88.3 vs 2.88.2? Did a Skia upstream change affect PDF font embedding logic?",
      "Does the upstream Skia bug https://issues.skia.org/issues/40031251 have a planned or available fix?",
      "Is the behavior reproducible on Linux/macOS or is it Windows-specific (Windows font system may influence which tables are present)?"
    ],
    "resolution": {
      "hypothesis": "Skia's PDF backend changed its font embedding decision logic between the Skia versions used in 2.88.2 and 2.88.3, causing TrueType-only fonts to fall back to Type3 embedding. This is an upstream Skia issue with no current SkiaSharp-level workaround.",
      "proposals": [
        {
          "title": "Track upstream Skia fix",
          "description": "Monitor the upstream Skia bug https://issues.skia.org/issues/40031251 for a fix. When fixed upstream, the fix will flow into SkiaSharp with the next Skia version bump.",
          "category": "investigation",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Use fonts with OpenType Layout tables as workaround",
          "description": "Until the upstream fix is available, use font files that include OpenType Layout tables (GSUB/GPOS). For example, substitute affected fonts with variants that include OpenType features. Alternatively, use SKTypeface.FromFile() with a font file known to include OpenType tables.",
          "category": "workaround",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Track upstream Skia fix",
      "recommendedReason": "The root cause is in upstream Skia; tracking the fix ensures it will be resolved properly. The font substitution workaround may not always be feasible depending on the reporter's PDF requirements."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Real regression with identified upstream Skia bug. Needs investigation to confirm the exact Skia version change that introduced this and whether SkiaSharp can apply a targeted fix or needs to wait for upstream.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/Windows-Classic, backend/PDF, tenet/compatibility labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/PDF",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the upstream Skia bug and provide context about the root cause",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed investigation! This matches an upstream Skia bug (https://issues.skia.org/issues/40031251) in Skia's PDF backend, which appears to have changed behavior between the Skia versions used in SkiaSharp 2.88.2 and 2.88.3.\n\nThe root cause is in Skia's PDF font embedding logic: fonts with only TrueType Outlines (no OpenType Layout tables like GSUB/GPOS) are currently embedded as Type3 fonts instead of as a TTF/CIDFont. Unfortunately, SkiaSharp's `SKDocumentPdfMetadata` API does not expose font embedding controls — this decision is made entirely inside Skia's PDF writer.\n\n**Possible workaround:** If possible, use font files that include OpenType Layout tables, as those are embedded as TTF. The underlying Skia bug will need to be fixed upstream before this is fully resolved in SkiaSharp."
      },
      {
        "type": "link-related",
        "description": "Reference upstream Skia issue in comments for tracking",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 2896
      }
    ]
  }
}
```

</details>
