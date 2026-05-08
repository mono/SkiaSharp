# Issue Triage Report — #1161

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T17:06:30Z |
| Type | type/question (0.90 (90%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.80 (80%)) |

**Issue Summary:** User asking for detailed documentation on SKImageFilter coordinate system and filter pipeline behavior, noting that SKImageFilter.CreateImage fills the whole canvas rather than only the drawn geometry rect.

**Analysis:** Reporter asks for documentation on SKImageFilter coordinate system and pipeline behavior. The confusion about CreateImage(image) filling the whole canvas is by-design Skia behavior: when no src/dst rect is specified, the image filter replaces the entire filter layer output, not just the clip rect of the draw call. The question about coordinate systems is answered in Skia source comments but not exposed in SkiaSharp docs.

**Recommendations:** **close-as-not-a-bug** — This is a usage question about documented Skia behavior. The fill-whole-canvas behavior is by-design (use the src/dst overload to control it). Detailed coordinate system docs are upstream Skia scope. The reporter acknowledges it may belong to the Skia mailing list.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Environment:** No specific platform mentioned; generic SkiaSharp question

**Repository links:**
- https://github.com/wieslawsoltes/Svg.Skia/blob/a362239968ea7201d2e376c8c16471069c1bfe54/src/Svg.Skia/SvgFilterskExtensions.cs#L12 — Community SVG filter implementation using SKImageFilter
- https://www.w3.org/TR/SVG11/filters.html — W3C SVG filter effects specification referenced in comments
- https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/effects/image-filters — SkiaSharp image filter introduction article referenced in issue

**Code snippets:**

```csharp
var paint = new SKPaint { Color = 0xFFFFFFFF, Style = SKPaintStyle.Fill, ImageFilter = SKImageFilter.CreateImage(image) }; canvas.DrawRect(0, 0, 1, 1, paint);
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Documentation question remains valid regardless of version; SKImageFilter API still exists and filter coordinate system behavior is unchanged. |

## Analysis

### Technical Summary

Reporter asks for documentation on SKImageFilter coordinate system and pipeline behavior. The confusion about CreateImage(image) filling the whole canvas is by-design Skia behavior: when no src/dst rect is specified, the image filter replaces the entire filter layer output, not just the clip rect of the draw call. The question about coordinate systems is answered in Skia source comments but not exposed in SkiaSharp docs.

### Rationale

Issue title explicitly marks this as [QUESTION]. Reporter is not claiming broken behavior — they acknowledge the behavior may be by-design and are asking for explanation. The issue is a request for documentation/explanation of SKImageFilter internals. Classified as type/question because the user wants to understand the API, not report a defect. Area is area/SkiaSharp because SKImageFilter is in the core binding.

### Key Signals

- "[QUESTION] SKImageFilter detailed documentation?" — **issue title** (Reporter explicitly flags this as a question, not a bug.)
- "I miss many details... what is coordinate system used in filters?" — **issue body** (Documentation gap — the coordinate system docs exist in Skia C++ headers but aren't surfaced in SkiaSharp API docs.)
- "the following code fills the whole canvas with an image, not just a single pixel" — **issue body** (This is expected Skia behavior: CreateImage without src/dst rects fills the filter layer output bounds, not the draw call geometry bounds.)
- "Maybe it's better to ask these questions in the Google Skia mailing list?" — **issue body** (Reporter themselves acknowledges this may be upstream Skia scope, not SkiaSharp-specific.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImageFilter.cs` | 377-384 | direct | CreateImage(SKImage) with no src/dst calls sk_imagefilter_new_image_simple — the 'simple' overload that renders the image to fill the entire filter output bounds, explaining why it fills the whole canvas instead of just the 1x1 rect. |
| `binding/SkiaSharp/SKImageFilter.cs` | 386-390 | direct | CreateImage(SKImage, SKRect src, SKRect dst, SKSamplingOptions) exists and allows specifying source and destination rectangles for more controlled rendering — this is the answer to the reporter's fill-whole-canvas question. |

### Workarounds

- Use SKImageFilter.CreateImage(image, srcRect, dstRect, sampling) overload to control the exact region the image filter covers.
- Refer to upstream Skia SkImageFilter.h comments for coordinate system explanation: local space of filters matches the drawn geometry's local space.

### Next Questions

- Has SkiaSharp published updated image filter documentation since 2020?
- Are there samples in .docs/docs/docs/ covering SKImageFilter coordinate behavior?

### Resolution Proposals

**Hypothesis:** The reporter needs an explanation of SKImageFilter's filter coordinate system (documented in Skia C++ headers) and should use the src/dst overload of CreateImage to control the fill area.

1. **Use src/dst overload of CreateImage** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - To limit the image to a specific region, use SKImageFilter.CreateImage(image, srcRect, dstRect, sampling) instead of the no-argument overload. The simple overload fills the entire filter layer output.
2. **Point to Skia coordinate system docs** — alternative, confidence 0.85 (85%), cost/xs, validated=untested
   - The coordinate system is documented in the Skia source: filter local space matches the drawn geometry's local space (canvas transform is respected). Upstream Skia docs and comments in SkImageFilter.h are the authoritative source.

**Recommended proposal:** Use src/dst overload of CreateImage

**Why:** Direct actionable answer to the specific fill-whole-canvas behavior. Answers the concrete code question immediately.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.80 (80%) |
| Reason | This is a usage question about documented Skia behavior. The fill-whole-canvas behavior is by-design (use the src/dst overload to control it). Detailed coordinate system docs are upstream Skia scope. The reporter acknowledges it may belong to the Skia mailing list. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply question and SkiaSharp labels | labels=type/question, area/SkiaSharp |
| add-comment | high | 0.80 (80%) | Post answer explaining CreateImage behavior and coordinate system | — |
| close-issue | medium | 0.75 (75%) | Close as answered — by-design behavior explained | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed question!

**Why `CreateImage` fills the whole canvas:**
The overload `SKImageFilter.CreateImage(image)` (without src/dst rectangles) is a 'fill the filter layer' operation — it renders the image to fill the entire filter output bounds, not just the 1x1 rect you drew. To control the exact region, use the overload with explicit src and dst rectangles:

```csharp
var dst = new SKRect(0, 0, 1, 1);
var src = SKRect.Create(image.Width, image.Height);
var paint = new SKPaint
{
    ImageFilter = SKImageFilter.CreateImage(image, src, dst, new SKSamplingOptions(SKCubicResampler.Mitchell))
};
canvas.DrawRect(dst, paint);
```

**Coordinate system:**
As documented in the [Skia source](https://github.com/google/skia/blob/master/include/core/SkImageFilter.h), the local space of image filters matches the local space of the drawn geometry — so canvas transforms (rotation, scale) are respected. The filter operates in the same coordinate space as the draw call.

For deeper exploration of the filter pipeline, the [Skia image filter introduction](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/effects/image-filters) is a good start, and the Skia mailing list / [Skia discuss](https://groups.google.com/g/skia-discuss) can answer lower-level Skia questions.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1161,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T17:06:30Z"
  },
  "summary": "User asking for detailed documentation on SKImageFilter coordinate system and filter pipeline behavior, noting that SKImageFilter.CreateImage fills the whole canvas rather than only the drawn geometry rect.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    }
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "var paint = new SKPaint { Color = 0xFFFFFFFF, Style = SKPaintStyle.Fill, ImageFilter = SKImageFilter.CreateImage(image) }; canvas.DrawRect(0, 0, 1, 1, paint);"
      ],
      "environmentDetails": "No specific platform mentioned; generic SkiaSharp question",
      "repoLinks": [
        {
          "url": "https://github.com/wieslawsoltes/Svg.Skia/blob/a362239968ea7201d2e376c8c16471069c1bfe54/src/Svg.Skia/SvgFilterskExtensions.cs#L12",
          "description": "Community SVG filter implementation using SKImageFilter"
        },
        {
          "url": "https://www.w3.org/TR/SVG11/filters.html",
          "description": "W3C SVG filter effects specification referenced in comments"
        },
        {
          "url": "https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/effects/image-filters",
          "description": "SkiaSharp image filter introduction article referenced in issue"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "Documentation question remains valid regardless of version; SKImageFilter API still exists and filter coordinate system behavior is unchanged."
    }
  },
  "analysis": {
    "summary": "Reporter asks for documentation on SKImageFilter coordinate system and pipeline behavior. The confusion about CreateImage(image) filling the whole canvas is by-design Skia behavior: when no src/dst rect is specified, the image filter replaces the entire filter layer output, not just the clip rect of the draw call. The question about coordinate systems is answered in Skia source comments but not exposed in SkiaSharp docs.",
    "rationale": "Issue title explicitly marks this as [QUESTION]. Reporter is not claiming broken behavior — they acknowledge the behavior may be by-design and are asking for explanation. The issue is a request for documentation/explanation of SKImageFilter internals. Classified as type/question because the user wants to understand the API, not report a defect. Area is area/SkiaSharp because SKImageFilter is in the core binding.",
    "keySignals": [
      {
        "text": "[QUESTION] SKImageFilter detailed documentation?",
        "source": "issue title",
        "interpretation": "Reporter explicitly flags this as a question, not a bug."
      },
      {
        "text": "I miss many details... what is coordinate system used in filters?",
        "source": "issue body",
        "interpretation": "Documentation gap — the coordinate system docs exist in Skia C++ headers but aren't surfaced in SkiaSharp API docs."
      },
      {
        "text": "the following code fills the whole canvas with an image, not just a single pixel",
        "source": "issue body",
        "interpretation": "This is expected Skia behavior: CreateImage without src/dst rects fills the filter layer output bounds, not the draw call geometry bounds."
      },
      {
        "text": "Maybe it's better to ask these questions in the Google Skia mailing list?",
        "source": "issue body",
        "interpretation": "Reporter themselves acknowledges this may be upstream Skia scope, not SkiaSharp-specific."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImageFilter.cs",
        "lines": "377-384",
        "finding": "CreateImage(SKImage) with no src/dst calls sk_imagefilter_new_image_simple — the 'simple' overload that renders the image to fill the entire filter output bounds, explaining why it fills the whole canvas instead of just the 1x1 rect.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImageFilter.cs",
        "lines": "386-390",
        "finding": "CreateImage(SKImage, SKRect src, SKRect dst, SKSamplingOptions) exists and allows specifying source and destination rectangles for more controlled rendering — this is the answer to the reporter's fill-whole-canvas question.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Use SKImageFilter.CreateImage(image, srcRect, dstRect, sampling) overload to control the exact region the image filter covers.",
      "Refer to upstream Skia SkImageFilter.h comments for coordinate system explanation: local space of filters matches the drawn geometry's local space."
    ],
    "nextQuestions": [
      "Has SkiaSharp published updated image filter documentation since 2020?",
      "Are there samples in .docs/docs/docs/ covering SKImageFilter coordinate behavior?"
    ],
    "resolution": {
      "hypothesis": "The reporter needs an explanation of SKImageFilter's filter coordinate system (documented in Skia C++ headers) and should use the src/dst overload of CreateImage to control the fill area.",
      "proposals": [
        {
          "title": "Use src/dst overload of CreateImage",
          "description": "To limit the image to a specific region, use SKImageFilter.CreateImage(image, srcRect, dstRect, sampling) instead of the no-argument overload. The simple overload fills the entire filter layer output.",
          "category": "workaround",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Point to Skia coordinate system docs",
          "description": "The coordinate system is documented in the Skia source: filter local space matches the drawn geometry's local space (canvas transform is respected). Upstream Skia docs and comments in SkImageFilter.h are the authoritative source.",
          "category": "alternative",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use src/dst overload of CreateImage",
      "recommendedReason": "Direct actionable answer to the specific fill-whole-canvas behavior. Answers the concrete code question immediately."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.8,
      "reason": "This is a usage question about documented Skia behavior. The fill-whole-canvas behavior is by-design (use the src/dst overload to control it). Detailed coordinate system docs are upstream Skia scope. The reporter acknowledges it may belong to the Skia mailing list.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and SkiaSharp labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post answer explaining CreateImage behavior and coordinate system",
        "risk": "high",
        "confidence": 0.8,
        "comment": "Thanks for the detailed question!\n\n**Why `CreateImage` fills the whole canvas:**\nThe overload `SKImageFilter.CreateImage(image)` (without src/dst rectangles) is a 'fill the filter layer' operation — it renders the image to fill the entire filter output bounds, not just the 1x1 rect you drew. To control the exact region, use the overload with explicit src and dst rectangles:\n\n```csharp\nvar dst = new SKRect(0, 0, 1, 1);\nvar src = SKRect.Create(image.Width, image.Height);\nvar paint = new SKPaint\n{\n    ImageFilter = SKImageFilter.CreateImage(image, src, dst, new SKSamplingOptions(SKCubicResampler.Mitchell))\n};\ncanvas.DrawRect(dst, paint);\n```\n\n**Coordinate system:**\nAs documented in the [Skia source](https://github.com/google/skia/blob/master/include/core/SkImageFilter.h), the local space of image filters matches the local space of the drawn geometry — so canvas transforms (rotation, scale) are respected. The filter operates in the same coordinate space as the draw call.\n\nFor deeper exploration of the filter pipeline, the [Skia image filter introduction](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/effects/image-filters) is a good start, and the Skia mailing list / [Skia discuss](https://groups.google.com/g/skia-discuss) can answer lower-level Skia questions."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — by-design behavior explained",
        "risk": "medium",
        "confidence": 0.75,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
