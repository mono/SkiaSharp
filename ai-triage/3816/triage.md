# Issue Triage Report — #3816

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-21T05:13:53Z |
| Type | type/feature-request (0.90 (90%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-duplicate (0.88 (88%)) |

**Issue Summary:** Reporter requests AVIF image encoding support in SkiaSharp, noting that SKImage.Encode(SKEncodedImageFormat.Avif, 75) returns null even in preview versions.

**Analysis:** AVIF encoding is not supported because the SKPixmap.Encode() switch statement only handles Jpeg, Png, and Webp formats, returning false for all others including Avif. Skia's native library does not expose an AVIF encoder, so this would require integration of a third-party codec (e.g., libavif). This is a duplicate of #2718, filed by the same reporter who also commented on #2718 the same day.

**Recommendations:** **close-as-duplicate** — Issue #2718 covers the same missing AVIF encoding functionality, is still open, and was even commented on by this same reporter the same day this issue was filed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Perf | — |
| Partner | — |
| Current labels | type/feature-request |

## Evidence

### Reproduction

1. Call SKImage.Encode(SKEncodedImageFormat.Avif, 75) on any image
2. Observe that the return value is null

**Environment:** .NET 10, SkiaSharp 4.147.0-preview.1

**Related issues:** #2718, #3142

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2718 — Bug report: AVIF encoding is silently unsupported (same issue, open since 2024)
- https://github.com/mono/SkiaSharp/issues/3142 — Related: SKData null when encoding to AVIF, HEIF, and other formats
- https://stackoverflow.com/questions/77669075/how-to-make-avif-codec-a-dependency-of-asp-net-web-project-with-skiasharp — StackOverflow discussion on AVIF and SkiaSharp

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 4.147.0-preview.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | AVIF encoding is still not implemented in the SKPixmap.Encode() switch statement as of the latest code. |

## Analysis

### Technical Summary

AVIF encoding is not supported because the SKPixmap.Encode() switch statement only handles Jpeg, Png, and Webp formats, returning false for all others including Avif. Skia's native library does not expose an AVIF encoder, so this would require integration of a third-party codec (e.g., libavif). This is a duplicate of #2718, filed by the same reporter who also commented on #2718 the same day.

### Rationale

SKEncodedImageFormat.Avif exists as enum value 12, but the Encode() dispatch in SKPixmap.cs does not handle it — falls through to the default returning false. The upstream Skia library does not include an AVIF encoder in its encode headers. This is the same issue as #2718 (which is open and labeled as a bug). The reporter is the same person who commented on #2718 on the same day as filing this issue.

### Key Signals

- "scaledImage.Encode(SKEncodedImageFormat.Avif, 75) returned null" — **issue body** (Confirms AVIF encoding silently fails — the switch default returns false causing null SKData.)
- "Issue still present in 3.0.0preview2.1, but Skia does not support AVIF Encoding anyway" — **comment on #2718** (Upstream Skia native library lacks AVIF encoder support, making this a deeper dependency gap.)
- "Is this still an issue? AVIF encoding with .Net 10 is not working with latest Release 4.147.0-preview.1" — **comment on #2718 by same reporter (drewid)** (Same reporter confirmed the issue persists on 4.147.0-preview.1 and then filed this new issue.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPixmap.cs` | 294-305 | direct | The SKPixmap.Encode(SKWStream, SKEncodedImageFormat, int) switch only dispatches for Jpeg, Png, and Webp. All other formats including Avif fall through to `_ => false`, which causes the caller to return null. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 21228-21229 | related | SKEncodedImageFormat.Avif = 12 exists as a valid enum value, indicating it was planned but encoding was never implemented. |

### Workarounds

- Use LibavifSharp (https://github.com/Spacefish/libavifsharp / NuGet: LibavifSharp) which wraps libavif and can accept SKImage/SKBitmap pixels for AVIF encoding.

### Resolution Proposals

**Hypothesis:** AVIF encoding is not supported in the upstream Skia native library. Full AVIF encoding support would require integrating libavif or waiting for upstream Skia to add AVIF encoder support.

1. **Use LibavifSharp as workaround** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - Use the community-maintained LibavifSharp NuGet package which wraps libavif and accepts SKImage/SKBitmap for AVIF encoding.
2. **Integrate libavif into SkiaSharp native builds** — fix, confidence 0.60 (60%), cost/xl, validated=untested
   - Add libavif as a native dependency and implement an AVIF encoder wrapper in the C API layer, following the pattern used for JPEG/PNG/WebP.

**Recommended proposal:** Use LibavifSharp as workaround

**Why:** Immediate relief for the reporter. Long-term fix requires significant native work and depends on upstream Skia decisions.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-duplicate |
| Confidence | 0.88 (88%) |
| Reason | Issue #2718 covers the same missing AVIF encoding functionality, is still open, and was even commented on by this same reporter the same day this issue was filed. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Retain type/feature-request; add area/SkiaSharp and triage/triaged | labels=type/feature-request, area/SkiaSharp, triage/triaged |
| link-duplicate | medium | 0.88 (88%) | Mark as duplicate of #2718 which tracks AVIF encoding being silently unsupported | linkedIssue=#2718 |
| add-comment | medium | 0.88 (88%) | Inform reporter of duplicate issue and workaround via LibavifSharp | — |
| close-issue | medium | 0.88 (88%) | Close as duplicate of #2718 | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! AVIF encoding is not yet supported in SkiaSharp — `SKEncodedImageFormat.Avif` exists as an enum value but the encoder is not implemented because upstream Skia does not include an AVIF encoder. This is tracked in #2718.

As a workaround, you can use the community-maintained [LibavifSharp](https://github.com/Spacefish/libavifsharp) package ([NuGet: LibavifSharp](https://www.nuget.org/packages/LibavifSharp)) which wraps libavif and can accept SkiaSharp bitmap data for AVIF encoding.

Closing as a duplicate of #2718 — please subscribe to that issue for updates.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3816,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-21T05:13:53Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "Reporter requests AVIF image encoding support in SkiaSharp, noting that SKImage.Encode(SKEncodedImageFormat.Avif, 75) returns null even in preview versions.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    }
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Call SKImage.Encode(SKEncodedImageFormat.Avif, 75) on any image",
        "Observe that the return value is null"
      ],
      "environmentDetails": ".NET 10, SkiaSharp 4.147.0-preview.1",
      "relatedIssues": [
        2718,
        3142
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2718",
          "description": "Bug report: AVIF encoding is silently unsupported (same issue, open since 2024)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3142",
          "description": "Related: SKData null when encoding to AVIF, HEIF, and other formats"
        },
        {
          "url": "https://stackoverflow.com/questions/77669075/how-to-make-avif-codec-a-dependency-of-asp-net-web-project-with-skiasharp",
          "description": "StackOverflow discussion on AVIF and SkiaSharp"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "4.147.0-preview.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "AVIF encoding is still not implemented in the SKPixmap.Encode() switch statement as of the latest code."
    }
  },
  "analysis": {
    "summary": "AVIF encoding is not supported because the SKPixmap.Encode() switch statement only handles Jpeg, Png, and Webp formats, returning false for all others including Avif. Skia's native library does not expose an AVIF encoder, so this would require integration of a third-party codec (e.g., libavif). This is a duplicate of #2718, filed by the same reporter who also commented on #2718 the same day.",
    "rationale": "SKEncodedImageFormat.Avif exists as enum value 12, but the Encode() dispatch in SKPixmap.cs does not handle it — falls through to the default returning false. The upstream Skia library does not include an AVIF encoder in its encode headers. This is the same issue as #2718 (which is open and labeled as a bug). The reporter is the same person who commented on #2718 on the same day as filing this issue.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPixmap.cs",
        "lines": "294-305",
        "finding": "The SKPixmap.Encode(SKWStream, SKEncodedImageFormat, int) switch only dispatches for Jpeg, Png, and Webp. All other formats including Avif fall through to `_ => false`, which causes the caller to return null.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "21228-21229",
        "finding": "SKEncodedImageFormat.Avif = 12 exists as a valid enum value, indicating it was planned but encoding was never implemented.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "scaledImage.Encode(SKEncodedImageFormat.Avif, 75) returned null",
        "source": "issue body",
        "interpretation": "Confirms AVIF encoding silently fails — the switch default returns false causing null SKData."
      },
      {
        "text": "Issue still present in 3.0.0preview2.1, but Skia does not support AVIF Encoding anyway",
        "source": "comment on #2718",
        "interpretation": "Upstream Skia native library lacks AVIF encoder support, making this a deeper dependency gap."
      },
      {
        "text": "Is this still an issue? AVIF encoding with .Net 10 is not working with latest Release 4.147.0-preview.1",
        "source": "comment on #2718 by same reporter (drewid)",
        "interpretation": "Same reporter confirmed the issue persists on 4.147.0-preview.1 and then filed this new issue."
      }
    ],
    "workarounds": [
      "Use LibavifSharp (https://github.com/Spacefish/libavifsharp / NuGet: LibavifSharp) which wraps libavif and can accept SKImage/SKBitmap pixels for AVIF encoding."
    ],
    "resolution": {
      "hypothesis": "AVIF encoding is not supported in the upstream Skia native library. Full AVIF encoding support would require integrating libavif or waiting for upstream Skia to add AVIF encoder support.",
      "proposals": [
        {
          "title": "Use LibavifSharp as workaround",
          "description": "Use the community-maintained LibavifSharp NuGet package which wraps libavif and accepts SKImage/SKBitmap for AVIF encoding.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Integrate libavif into SkiaSharp native builds",
          "description": "Add libavif as a native dependency and implement an AVIF encoder wrapper in the C API layer, following the pattern used for JPEG/PNG/WebP.",
          "category": "fix",
          "confidence": 0.6,
          "effort": "cost/xl",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use LibavifSharp as workaround",
      "recommendedReason": "Immediate relief for the reporter. Long-term fix requires significant native work and depends on upstream Skia decisions."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-duplicate",
      "confidence": 0.88,
      "reason": "Issue #2718 covers the same missing AVIF encoding functionality, is still open, and was even commented on by this same reporter the same day this issue was filed.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Retain type/feature-request; add area/SkiaSharp and triage/triaged",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "triage/triaged"
        ]
      },
      {
        "type": "link-duplicate",
        "description": "Mark as duplicate of #2718 which tracks AVIF encoding being silently unsupported",
        "risk": "medium",
        "confidence": 0.88,
        "linkedIssue": 2718
      },
      {
        "type": "add-comment",
        "description": "Inform reporter of duplicate issue and workaround via LibavifSharp",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the report! AVIF encoding is not yet supported in SkiaSharp — `SKEncodedImageFormat.Avif` exists as an enum value but the encoder is not implemented because upstream Skia does not include an AVIF encoder. This is tracked in #2718.\n\nAs a workaround, you can use the community-maintained [LibavifSharp](https://github.com/Spacefish/libavifsharp) package ([NuGet: LibavifSharp](https://www.nuget.org/packages/LibavifSharp)) which wraps libavif and can accept SkiaSharp bitmap data for AVIF encoding.\n\nClosing as a duplicate of #2718 — please subscribe to that issue for updates."
      },
      {
        "type": "close-issue",
        "description": "Close as duplicate of #2718",
        "risk": "medium",
        "confidence": 0.88,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
