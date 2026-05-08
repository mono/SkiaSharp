# Issue Triage Report — #2115

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T21:35:00Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** Feature request to add a C# binding for Skia's SkMultiPictureDocument utility, which writes multi-picture .mskp files from a collection of SKPicture objects.

**Analysis:** No C# binding exists for Skia's SkMultiPictureDocument utility. The existing SKDocument class covers PDF and XPS but not the .mskp multi-picture serialization format. The upstream C++ API lives in SkMultiPictureDocument.cpp and has no corresponding sk_multipicture_* C shim, and therefore no C# wrapper. This is a genuine missing API gap.

**Recommendations:** **needs-investigation** — Valid feature request with a clear upstream reference. Needs investigation to confirm SkMultiPictureDocument is compiled into the shipped native binary before committing to a binding.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/google/skia/blob/main/src/utils/SkMultiPictureDocument.cpp — Upstream Skia implementation of SkMultiPictureDocument

## Analysis

### Technical Summary

No C# binding exists for Skia's SkMultiPictureDocument utility. The existing SKDocument class covers PDF and XPS but not the .mskp multi-picture serialization format. The upstream C++ API lives in SkMultiPictureDocument.cpp and has no corresponding sk_multipicture_* C shim, and therefore no C# wrapper. This is a genuine missing API gap.

### Rationale

Reporter asks for SKMultiPictureDocument to be wrapped in C#. Code search confirms no sk_multipicture or multi_picture entries in SkiaApi.generated.cs or the binding directory. SKDocument covers PDF and XPS but not .mskp. The upstream Skia C++ header and implementation exist. This is a valid feature request to expose new functionality — not a bug or enhancement of existing code.

### Key Signals

- "SKMultiPictureDocument is missing from the C# binding" — **issue body** (Confirms this is a missing API — reporter has verified the class does not exist in SkiaSharp.)
- "Call MultiPictureDocument with an array of SKPictures" — **issue body** (The desired API shape: a method that accepts multiple SKPicture objects and serializes them to .mskp format.)
- "https://github.com/google/skia/blob/main/src/utils/SkMultiPictureDocument.cpp" — **issue body** (Reporter links the upstream Skia C++ implementation, indicating the feature exists in the native library but has no C shim or C# wrapper.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | direct | No 'multipicture' or 'multi_picture' entries found in the generated C API bindings. Only sk_document_* entries exist for PDF and XPS formats. |
| `binding/SkiaSharp/SKDocument.cs` | — | direct | SKDocument class provides CreatePdf() and CreateXps() factory methods but has no CreateMskp() or any multi-picture equivalent. The class wraps sk_document_t via the C API shim. |
| `binding/SkiaSharp/SKPicture.cs` | — | related | SKPicture class exists with Serialize/Deserialize support, confirming the picture object model is present, but there is no multi-picture document aggregation class. |

### Next Questions

- Does the compiled libSkiaSharp native binary export SkMultiPictureDocument? If it is only in src/utils/ it may not be compiled into the standard build.
- Is there a C API shim (sk_multipicture_*) needed in externals/skia/src/c/ before C# bindings can be added?
- Should this be added to SKDocument as additional factory methods or as a separate SKMultiPictureDocument class?

### Resolution Proposals

**Hypothesis:** SkMultiPictureDocument requires a new C API shim in externals/skia/src/c/ and a new SKMultiPictureDocument C# class (or additions to SKDocument).

1. **Add C shim and C# binding** — fix, confidence 0.75 (75%), cost/m, validated=untested
   - Add sk_multipicture_document_* functions to the C API shim in externals/skia/src/c/, regenerate bindings, then add a SKMultiPictureDocument class to binding/SkiaSharp/.
2. **Extend SKDocument with multi-picture support** — alternative, confidence 0.65 (65%), cost/m, validated=untested
   - Rather than a separate class, add CreateMskp() factory methods on SKDocument mirroring the existing CreatePdf/CreateXps pattern.

**Recommended proposal:** Add C shim and C# binding

**Why:** A dedicated class mirrors the upstream Skia design and is cleaner than mixing multi-picture document methods into SKDocument.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Valid feature request with a clear upstream reference. Needs investigation to confirm SkMultiPictureDocument is compiled into the shipped native binary before committing to a binding. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply feature-request and SkiaSharp area labels | labels=type/feature-request, area/SkiaSharp |
| add-comment | medium | 0.82 (82%) | Acknowledge request and note investigation needed re: native binary inclusion | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the request! `SkMultiPictureDocument` is not currently exposed in SkiaSharp — you're right that it's missing.

Before a binding can be added we need to confirm that `SkMultiPictureDocument` is compiled into the shipped `libSkiaSharp` native binary (it lives in `src/utils/` which may not be included in the default Skia build configuration). If it is available, the work involves:

1. Adding `sk_multipicture_document_*` C API shim functions in the Skia fork
2. Regenerating the C# P/Invoke bindings
3. Adding a `SKMultiPictureDocument` C# wrapper class

We'll track this as a feature request. Contributions welcome!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2115,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T21:35:00Z"
  },
  "summary": "Feature request to add a C# binding for Skia's SkMultiPictureDocument utility, which writes multi-picture .mskp files from a collection of SKPicture objects.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    }
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/google/skia/blob/main/src/utils/SkMultiPictureDocument.cpp",
          "description": "Upstream Skia implementation of SkMultiPictureDocument"
        }
      ]
    }
  },
  "analysis": {
    "summary": "No C# binding exists for Skia's SkMultiPictureDocument utility. The existing SKDocument class covers PDF and XPS but not the .mskp multi-picture serialization format. The upstream C++ API lives in SkMultiPictureDocument.cpp and has no corresponding sk_multipicture_* C shim, and therefore no C# wrapper. This is a genuine missing API gap.",
    "rationale": "Reporter asks for SKMultiPictureDocument to be wrapped in C#. Code search confirms no sk_multipicture or multi_picture entries in SkiaApi.generated.cs or the binding directory. SKDocument covers PDF and XPS but not .mskp. The upstream Skia C++ header and implementation exist. This is a valid feature request to expose new functionality — not a bug or enhancement of existing code.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "No 'multipicture' or 'multi_picture' entries found in the generated C API bindings. Only sk_document_* entries exist for PDF and XPS formats.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKDocument.cs",
        "finding": "SKDocument class provides CreatePdf() and CreateXps() factory methods but has no CreateMskp() or any multi-picture equivalent. The class wraps sk_document_t via the C API shim.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPicture.cs",
        "finding": "SKPicture class exists with Serialize/Deserialize support, confirming the picture object model is present, but there is no multi-picture document aggregation class.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "SKMultiPictureDocument is missing from the C# binding",
        "source": "issue body",
        "interpretation": "Confirms this is a missing API — reporter has verified the class does not exist in SkiaSharp."
      },
      {
        "text": "Call MultiPictureDocument with an array of SKPictures",
        "source": "issue body",
        "interpretation": "The desired API shape: a method that accepts multiple SKPicture objects and serializes them to .mskp format."
      },
      {
        "text": "https://github.com/google/skia/blob/main/src/utils/SkMultiPictureDocument.cpp",
        "source": "issue body",
        "interpretation": "Reporter links the upstream Skia C++ implementation, indicating the feature exists in the native library but has no C shim or C# wrapper."
      }
    ],
    "nextQuestions": [
      "Does the compiled libSkiaSharp native binary export SkMultiPictureDocument? If it is only in src/utils/ it may not be compiled into the standard build.",
      "Is there a C API shim (sk_multipicture_*) needed in externals/skia/src/c/ before C# bindings can be added?",
      "Should this be added to SKDocument as additional factory methods or as a separate SKMultiPictureDocument class?"
    ],
    "resolution": {
      "hypothesis": "SkMultiPictureDocument requires a new C API shim in externals/skia/src/c/ and a new SKMultiPictureDocument C# class (or additions to SKDocument).",
      "proposals": [
        {
          "title": "Add C shim and C# binding",
          "description": "Add sk_multipicture_document_* functions to the C API shim in externals/skia/src/c/, regenerate bindings, then add a SKMultiPictureDocument class to binding/SkiaSharp/.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Extend SKDocument with multi-picture support",
          "description": "Rather than a separate class, add CreateMskp() factory methods on SKDocument mirroring the existing CreatePdf/CreateXps pattern.",
          "category": "alternative",
          "confidence": 0.65,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add C shim and C# binding",
      "recommendedReason": "A dedicated class mirrors the upstream Skia design and is cleaner than mixing multi-picture document methods into SKDocument."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Valid feature request with a clear upstream reference. Needs investigation to confirm SkMultiPictureDocument is compiled into the shipped native binary before committing to a binding.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request and SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge request and note investigation needed re: native binary inclusion",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the request! `SkMultiPictureDocument` is not currently exposed in SkiaSharp — you're right that it's missing.\n\nBefore a binding can be added we need to confirm that `SkMultiPictureDocument` is compiled into the shipped `libSkiaSharp` native binary (it lives in `src/utils/` which may not be included in the default Skia build configuration). If it is available, the work involves:\n\n1. Adding `sk_multipicture_document_*` C API shim functions in the Skia fork\n2. Regenerating the C# P/Invoke bindings\n3. Adding a `SKMultiPictureDocument` C# wrapper class\n\nWe'll track this as a feature request. Contributions welcome!"
      }
    ]
  }
}
```

</details>
