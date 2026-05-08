# Issue Triage Report — #2280

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T23:00:00Z |
| Type | type/question (0.88 (88%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | close-as-not-a-bug (0.85 (85%)) |

**Issue Summary:** Reporter using SkiaSharp to resize images finds that some images appear rotated after processing; root cause (confirmed by community) is EXIF orientation metadata (Orientation: Rotate 90 CW) in the source JPEG — SkiaSharp returns raw pixel data and does not auto-apply EXIF orientation on decode.

**Analysis:** This is a usage question about EXIF orientation handling, not a SkiaSharp defect. The source JPEG has EXIF metadata (Orientation: Rotate 90 CW) that instructs viewers to display the image rotated. SkiaSharp decodes raw pixel data without auto-applying EXIF orientation, and does not preserve EXIF metadata when re-encoding. The reporter needs to use SKCodec.EncodedOrigin to read the orientation and apply the appropriate rotation transform manually. A community member already confirmed this in the comments and the reporter accepted the explanation.

**Recommendations:** **close-as-not-a-bug** — Behavior is by design — SkiaSharp does not auto-apply EXIF orientation. Root cause identified by community, accepted by reporter. Workaround via SKCodec.EncodedOrigin is available.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic, os/Linux |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Take a JPEG image with EXIF Orientation: Rotate 90 CW (e.g., a portrait photo from a camera phone stored as landscape pixels with rotation tag)
2. Use SkiaSharp to decode and resize the image, then re-encode as JPEG
3. Observe that the output image orientation differs from how the source appeared in viewers that honor EXIF

**Environment:** Tested on Windows and Linux, all versions

**Related issues:** #836, #1517

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/836 — Open feature request: Image auto orientation method (AutoOrient())
- https://github.com/mono/SkiaSharp/issues/1517 — Closed issue: SkEncodedOrigin wrong for one JPEG but not another
- https://stackoverflow.com/questions/44181914/iphone-image-orientation-wrong-when-resizing-with-skiasharp — Stack Overflow: iPhone image orientation wrong when resizing with SkiaSharp

**Attachments:**
- random.jpeg — https://user-images.githubusercontent.com/81751123/194540687-d2b259c9-a53f-4079-a70a-754198de8ef9.jpeg — Sample JPEG with EXIF Orientation: Rotate 90 CW attached by reporter

## Analysis

### Technical Summary

This is a usage question about EXIF orientation handling, not a SkiaSharp defect. The source JPEG has EXIF metadata (Orientation: Rotate 90 CW) that instructs viewers to display the image rotated. SkiaSharp decodes raw pixel data without auto-applying EXIF orientation, and does not preserve EXIF metadata when re-encoding. The reporter needs to use SKCodec.EncodedOrigin to read the orientation and apply the appropriate rotation transform manually. A community member already confirmed this in the comments and the reporter accepted the explanation.

### Rationale

Classified as type/question because the behavior is by design: SkiaSharp does not auto-apply EXIF orientation. A community member confirmed the root cause (EXIF orientation metadata) and pointed to SKCodec.EncodedOrigin as the solution. The reporter acknowledged the answer. The related open issue #836 is a feature request for an AutoOrient() helper that would simplify this common pattern, but no such API exists yet.

### Key Signals

- "This image is actually a landscape image with some EXIF metadata saying that it should be rotated. Orientation: Rotate 90 CW" — **comment #1 by ojb500** (Community member identified root cause: EXIF orientation metadata, not a SkiaSharp bug.)
- "you need to either handle this metadata (SKCodec.EncodedOrigin) or preserve it in your output jpeg" — **comment #1 by ojb500** (The workaround is already documented in the thread — use SKCodec.EncodedOrigin and apply the transform.)
- "thank you so much for clarification. Now I understand that some photos have this intrinsic metadata." — **comment #2 by reporter** (Reporter accepted the explanation — confirmed as a usage question, not a SkiaSharp defect.)
- "Since it happens for all version on all operating systems (we have tested Windows and Linux)" — **issue body** (Cross-platform behavior, consistent with SkiaSharp's design to return raw decoded pixels without auto-rotating.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCodec.cs` | 36-37 | direct | SKCodec.EncodedOrigin property exposes the EXIF orientation by calling sk_codec_get_origin — this is the API callers must use to read and handle EXIF orientation manually; there is no auto-apply behavior |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 20650-20669 | related | SKEncodedOrigin enum defines: TopLeft/Default (no rotation), RightTop (90 CW), BottomRight (180), LeftBottom (90 CCW), and mirror variants — consumer code must map these to canvas transforms |

### Workarounds

- Read EXIF orientation with SKCodec.EncodedOrigin and apply the corresponding rotation/flip transform when drawing the bitmap to an SKCanvas before re-encoding
- Preserve EXIF metadata in the output JPEG if the image dimensions are unchanged (some encoders support passing through EXIF)

### Resolution Proposals

**Hypothesis:** The reporter is unaware of EXIF orientation metadata semantics. SkiaSharp provides SKCodec.EncodedOrigin to read the orientation but requires the caller to apply the transformation manually using canvas operations.

1. **Handle EXIF orientation using SKCodec.EncodedOrigin** — workaround, confidence 0.92 (92%), cost/s, validated=untested
   - After decoding with SKCodec, check codec.EncodedOrigin and apply the appropriate rotation transform (e.g., using SKMatrix or SKCanvas.RotateDegrees) before producing the resized output. RightTop = 90 CW, BottomRight = 180, LeftBottom = 90 CCW.
2. **Follow feature request #836 for an AutoOrient() helper** — alternative, confidence 0.80 (80%), cost/xs, validated=untested
   - Issue #836 requests an AutoOrient() method that would automatically apply EXIF orientation. If implemented, this would be the simplest solution. Track that issue for updates.

**Recommended proposal:** Handle EXIF orientation using SKCodec.EncodedOrigin

**Why:** Immediately actionable with the existing API. SKCodec.EncodedOrigin is available in all current SkiaSharp versions and the community comment already pointed the reporter to this solution.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.85 (85%) |
| Reason | Behavior is by design — SkiaSharp does not auto-apply EXIF orientation. Root cause identified by community, accepted by reporter. Workaround via SKCodec.EncodedOrigin is available. |
| Suggested repro platform | — |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Apply question type, SkiaSharp area, and platform labels | labels=type/question, area/SkiaSharp, os/Windows-Classic, os/Linux |
| add-comment | high | 0.85 (85%) | Post answer explaining EXIF orientation and how to handle it with SKCodec.EncodedOrigin | — |
| close-issue | medium | 0.80 (80%) | Close as not a bug — by-design behavior, answer already provided in thread | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
As @ojb500 explained above, this is caused by EXIF orientation metadata in the source JPEG (`Orientation: Rotate 90 CW`). SkiaSharp decodes raw pixel data without automatically applying EXIF orientation, and does not preserve EXIF metadata when re-encoding.

To handle orientation correctly, use `SKCodec.EncodedOrigin` to read the orientation value, then apply the appropriate rotation/flip transform on an `SKCanvas` before drawing the decoded bitmap to your output surface.

The relevant API is `SKCodec.EncodedOrigin` which returns an `SKEncodedOrigin` value (e.g., `RightTop` = 90° CW, `BottomRight` = 180°, `LeftBottom` = 90° CCW, `Default`/`TopLeft` = no rotation needed).

See also the open feature request in #836 for an `AutoOrient()` helper that would handle this automatically in a future SkiaSharp version.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2280,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T23:00:00Z"
  },
  "summary": "Reporter using SkiaSharp to resize images finds that some images appear rotated after processing; root cause (confirmed by community) is EXIF orientation metadata (Orientation: Rotate 90 CW) in the source JPEG — SkiaSharp returns raw pixel data and does not auto-apply EXIF orientation on decode.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    },
    "platforms": [
      "os/Windows-Classic",
      "os/Linux"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Take a JPEG image with EXIF Orientation: Rotate 90 CW (e.g., a portrait photo from a camera phone stored as landscape pixels with rotation tag)",
        "Use SkiaSharp to decode and resize the image, then re-encode as JPEG",
        "Observe that the output image orientation differs from how the source appeared in viewers that honor EXIF"
      ],
      "environmentDetails": "Tested on Windows and Linux, all versions",
      "attachments": [
        {
          "url": "https://user-images.githubusercontent.com/81751123/194540687-d2b259c9-a53f-4079-a70a-754198de8ef9.jpeg",
          "filename": "random.jpeg",
          "description": "Sample JPEG with EXIF Orientation: Rotate 90 CW attached by reporter"
        }
      ],
      "relatedIssues": [
        836,
        1517
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/836",
          "description": "Open feature request: Image auto orientation method (AutoOrient())"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1517",
          "description": "Closed issue: SkEncodedOrigin wrong for one JPEG but not another"
        },
        {
          "url": "https://stackoverflow.com/questions/44181914/iphone-image-orientation-wrong-when-resizing-with-skiasharp",
          "description": "Stack Overflow: iPhone image orientation wrong when resizing with SkiaSharp"
        }
      ]
    }
  },
  "analysis": {
    "summary": "This is a usage question about EXIF orientation handling, not a SkiaSharp defect. The source JPEG has EXIF metadata (Orientation: Rotate 90 CW) that instructs viewers to display the image rotated. SkiaSharp decodes raw pixel data without auto-applying EXIF orientation, and does not preserve EXIF metadata when re-encoding. The reporter needs to use SKCodec.EncodedOrigin to read the orientation and apply the appropriate rotation transform manually. A community member already confirmed this in the comments and the reporter accepted the explanation.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "36-37",
        "finding": "SKCodec.EncodedOrigin property exposes the EXIF orientation by calling sk_codec_get_origin — this is the API callers must use to read and handle EXIF orientation manually; there is no auto-apply behavior",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "20650-20669",
        "finding": "SKEncodedOrigin enum defines: TopLeft/Default (no rotation), RightTop (90 CW), BottomRight (180), LeftBottom (90 CCW), and mirror variants — consumer code must map these to canvas transforms",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "This image is actually a landscape image with some EXIF metadata saying that it should be rotated. Orientation: Rotate 90 CW",
        "source": "comment #1 by ojb500",
        "interpretation": "Community member identified root cause: EXIF orientation metadata, not a SkiaSharp bug."
      },
      {
        "text": "you need to either handle this metadata (SKCodec.EncodedOrigin) or preserve it in your output jpeg",
        "source": "comment #1 by ojb500",
        "interpretation": "The workaround is already documented in the thread — use SKCodec.EncodedOrigin and apply the transform."
      },
      {
        "text": "thank you so much for clarification. Now I understand that some photos have this intrinsic metadata.",
        "source": "comment #2 by reporter",
        "interpretation": "Reporter accepted the explanation — confirmed as a usage question, not a SkiaSharp defect."
      },
      {
        "text": "Since it happens for all version on all operating systems (we have tested Windows and Linux)",
        "source": "issue body",
        "interpretation": "Cross-platform behavior, consistent with SkiaSharp's design to return raw decoded pixels without auto-rotating."
      }
    ],
    "rationale": "Classified as type/question because the behavior is by design: SkiaSharp does not auto-apply EXIF orientation. A community member confirmed the root cause (EXIF orientation metadata) and pointed to SKCodec.EncodedOrigin as the solution. The reporter acknowledged the answer. The related open issue #836 is a feature request for an AutoOrient() helper that would simplify this common pattern, but no such API exists yet.",
    "workarounds": [
      "Read EXIF orientation with SKCodec.EncodedOrigin and apply the corresponding rotation/flip transform when drawing the bitmap to an SKCanvas before re-encoding",
      "Preserve EXIF metadata in the output JPEG if the image dimensions are unchanged (some encoders support passing through EXIF)"
    ],
    "resolution": {
      "hypothesis": "The reporter is unaware of EXIF orientation metadata semantics. SkiaSharp provides SKCodec.EncodedOrigin to read the orientation but requires the caller to apply the transformation manually using canvas operations.",
      "proposals": [
        {
          "title": "Handle EXIF orientation using SKCodec.EncodedOrigin",
          "description": "After decoding with SKCodec, check codec.EncodedOrigin and apply the appropriate rotation transform (e.g., using SKMatrix or SKCanvas.RotateDegrees) before producing the resized output. RightTop = 90 CW, BottomRight = 180, LeftBottom = 90 CCW.",
          "category": "workaround",
          "confidence": 0.92,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Follow feature request #836 for an AutoOrient() helper",
          "description": "Issue #836 requests an AutoOrient() method that would automatically apply EXIF orientation. If implemented, this would be the simplest solution. Track that issue for updates.",
          "category": "alternative",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Handle EXIF orientation using SKCodec.EncodedOrigin",
      "recommendedReason": "Immediately actionable with the existing API. SKCodec.EncodedOrigin is available in all current SkiaSharp versions and the community comment already pointed the reporter to this solution."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.85,
      "reason": "Behavior is by design — SkiaSharp does not auto-apply EXIF orientation. Root cause identified by community, accepted by reporter. Workaround via SKCodec.EncodedOrigin is available."
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question type, SkiaSharp area, and platform labels",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "os/Linux"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post answer explaining EXIF orientation and how to handle it with SKCodec.EncodedOrigin",
        "risk": "high",
        "confidence": 0.85,
        "comment": "As @ojb500 explained above, this is caused by EXIF orientation metadata in the source JPEG (`Orientation: Rotate 90 CW`). SkiaSharp decodes raw pixel data without automatically applying EXIF orientation, and does not preserve EXIF metadata when re-encoding.\n\nTo handle orientation correctly, use `SKCodec.EncodedOrigin` to read the orientation value, then apply the appropriate rotation/flip transform on an `SKCanvas` before drawing the decoded bitmap to your output surface.\n\nThe relevant API is `SKCodec.EncodedOrigin` which returns an `SKEncodedOrigin` value (e.g., `RightTop` = 90° CW, `BottomRight` = 180°, `LeftBottom` = 90° CCW, `Default`/`TopLeft` = no rotation needed).\n\nSee also the open feature request in #836 for an `AutoOrient()` helper that would handle this automatically in a future SkiaSharp version."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — by-design behavior, answer already provided in thread",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
