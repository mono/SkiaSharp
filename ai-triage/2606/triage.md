# Issue Triage Report — #2606

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T08:20:53Z |
| Type | type/question (0.75 (75%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-not-a-bug (0.80 (80%)) |

**Issue Summary:** SKBitmap.Decode returns null when decoding images from the iOS camera on iOS 16+ because the camera now captures HEIC/HEIF images which SkiaSharp does not support.

**Analysis:** The reporter's iOS camera (iPhone 14 Pro Max, iOS 16.6) produces HEIC/HEIF images (~2MB) by default. SkiaSharp's SKBitmap.Decode calls SKCodec.Create internally; if the codec cannot decode the format, it returns null, which propagates up as a null bitmap. SkiaSharp does not support HEIC/HEIF decoding due to licensing constraints (see #1700, #2887). The reporter needs to convert the HEIC bytes to JPEG using iOS native APIs before passing to SkiaSharp.

**Recommendations:** **close-as-not-a-bug** — SkiaSharp does not support HEIC/HEIF decoding by design. The null return from SKBitmap.Decode is the documented behavior for unsupported formats. Reporter needs to convert to JPEG using native iOS APIs first.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/iOS |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1700 — Closed feature request for HEIF/HEIC support
- https://github.com/mono/SkiaSharp/issues/2887 — Open feature request for HEIF/HEIC file support on Android/iOS

**Code snippets:**

```csharp
using (var imageStream = new System.IO.MemoryStream(image)) { var skData = SKData.Create(imageStream); var BitmapSelected = SKBitmap.Decode(skData); }
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | — |
| Error type | missing-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.x, 2.88.5 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | HEIC support has not been added; behavior is consistent across versions |

## Analysis

### Technical Summary

The reporter's iOS camera (iPhone 14 Pro Max, iOS 16.6) produces HEIC/HEIF images (~2MB) by default. SkiaSharp's SKBitmap.Decode calls SKCodec.Create internally; if the codec cannot decode the format, it returns null, which propagates up as a null bitmap. SkiaSharp does not support HEIC/HEIF decoding due to licensing constraints (see #1700, #2887). The reporter needs to convert the HEIC bytes to JPEG using iOS native APIs before passing to SkiaSharp.

### Rationale

This is classified as type/question because the behavior is by design: SkiaSharp does not support HEIC/HEIF due to licensing, and SKBitmap.Decode returning null is the expected result for unsupported formats. The reporter's situation changed because iOS 16 or the new device captures higher-resolution HEIC files. The fix is a platform-side conversion workaround, not a SkiaSharp code change.

### Key Signals

- "the byte[] that I get from the image is around 2 million bytes. It used to be around 700000 bytes" — **issue body** (Image size increase from ~700KB to ~2MB is consistent with HEIC format from iPhone 14 Pro Max with iOS 16+ (higher resolution photos saved as HEIC))
- "I take a picture with my native iOS camera" — **issue body** (iOS camera defaults to HEIC format since iPhone 11; SkiaSharp cannot decode HEIC)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 513-524 | direct | SKBitmap.Decode(SKData) calls SKCodec.Create(data); if it returns null (unsupported format), Decode returns null immediately. |
| `binding/SkiaSharp/SKData.cs` | 109-121 | related | SKData.Create(Stream) for seekable streams reads stream.Length and creates data with that length. MemoryStream is seekable, so this path is taken. |

### Workarounds

- Use UIKit on iOS to convert the HEIC image to JPEG before passing to SkiaSharp: use UIImage(data:) then JPEG representation.
- Configure iOS camera to capture in 'Most Compatible' mode (Settings > Camera > Formats > Most Compatible) to use JPEG instead of HEIC.
- Use CGImageSource on iOS to convert HEIC to a bitmap CGImage, then pass the raw pixels to SkiaSharp.

### Next Questions

- What is the actual image format of the byte[]? Is it HEIC, JPEG, or another format?
- Does the byte[] come directly from NSData of the camera capture, and was HEIC format verified?

### Resolution Proposals

**Hypothesis:** The byte array from iOS camera is in HEIC format, which SkiaSharp cannot decode. The reporter needs to convert using native iOS APIs.

1. **Convert HEIC to JPEG using native iOS APIs before passing to SkiaSharp** — workaround, cost/s, validated=untested
   - Use UIKit to create a UIImage from the HEIC bytes, then get JPEG representation and pass that to SkiaSharp.

**Recommended proposal:** workaround-1

**Why:** HEIC support is not available in SkiaSharp due to licensing constraints; native iOS conversion is the most reliable approach.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.80 (80%) |
| Reason | SkiaSharp does not support HEIC/HEIF decoding by design. The null return from SKBitmap.Decode is the documented behavior for unsupported formats. Reporter needs to convert to JPEG using native iOS APIs first. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Add area/SkiaSharp and os/iOS labels | labels=area/SkiaSharp, os/iOS, tenet/compatibility |
| add-comment | medium | 0.80 (80%) | Explain that iOS camera produces HEIC images which SkiaSharp cannot decode, and suggest workaround | — |
| link-related | low | 0.95 (95%) | Link to HEIC feature request #2887 | linkedIssue=#2887 |
| link-related | low | 0.95 (95%) | Link to closed HEIC feature request #1700 | linkedIssue=#1700 |

**Comment draft for `add-comment`:**

```markdown
Thank you for reporting this!

The behavior you're seeing — `SKBitmap.Decode` returning `null` — is because iOS camera apps on iPhone 11 and later default to capturing images in **HEIC/HEIF format**, especially on iOS 16+. SkiaSharp currently cannot decode HEIC/HEIF images due to licensing constraints (see #1700 and #2887).

The increase in file size you noticed (~700KB → ~2MB) is consistent with a higher-resolution HEIC capture on iPhone 14 Pro Max.

**Workaround:** Convert the image to JPEG on the iOS side before passing it to SkiaSharp:

```csharp
// On iOS, convert HEIC bytes to JPEG first
var uiImage = new UIImage(NSData.FromArray(heicBytes));
var jpegData = uiImage.AsJPEG(0.9f); // 0.9 quality
byte[] jpegBytes = jpegData.ToArray();

// Now decode with SkiaSharp
using var bitmap = SKBitmap.Decode(jpegBytes);
```

Alternatively, ask users to change their iOS camera format to **"Most Compatible"** (Settings → Camera → Formats → Most Compatible), which saves images as JPEG.

If HEIC support is important to you, please 👍 the feature request at #2887.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2606,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T08:20:53Z"
  },
  "summary": "SKBitmap.Decode returns null when decoding images from the iOS camera on iOS 16+ because the camera now captures HEIC/HEIF images which SkiaSharp does not support.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.75
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/iOS"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "using (var imageStream = new System.IO.MemoryStream(image)) { var skData = SKData.Create(imageStream); var BitmapSelected = SKBitmap.Decode(skData); }"
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1700",
          "description": "Closed feature request for HEIF/HEIC support"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2887",
          "description": "Open feature request for HEIF/HEIC file support on Android/iOS"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.x",
        "2.88.5"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "HEIC support has not been added; behavior is consistent across versions"
    },
    "bugSignals": {
      "severity": "medium",
      "reproQuality": "partial",
      "errorType": "missing-output"
    }
  },
  "analysis": {
    "summary": "The reporter's iOS camera (iPhone 14 Pro Max, iOS 16.6) produces HEIC/HEIF images (~2MB) by default. SkiaSharp's SKBitmap.Decode calls SKCodec.Create internally; if the codec cannot decode the format, it returns null, which propagates up as a null bitmap. SkiaSharp does not support HEIC/HEIF decoding due to licensing constraints (see #1700, #2887). The reporter needs to convert the HEIC bytes to JPEG using iOS native APIs before passing to SkiaSharp.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "finding": "SKBitmap.Decode(SKData) calls SKCodec.Create(data); if it returns null (unsupported format), Decode returns null immediately.",
        "relevance": "direct",
        "lines": "513-524"
      },
      {
        "file": "binding/SkiaSharp/SKData.cs",
        "finding": "SKData.Create(Stream) for seekable streams reads stream.Length and creates data with that length. MemoryStream is seekable, so this path is taken.",
        "relevance": "related",
        "lines": "109-121"
      }
    ],
    "keySignals": [
      {
        "text": "the byte[] that I get from the image is around 2 million bytes. It used to be around 700000 bytes",
        "source": "issue body",
        "interpretation": "Image size increase from ~700KB to ~2MB is consistent with HEIC format from iPhone 14 Pro Max with iOS 16+ (higher resolution photos saved as HEIC)"
      },
      {
        "text": "I take a picture with my native iOS camera",
        "source": "issue body",
        "interpretation": "iOS camera defaults to HEIC format since iPhone 11; SkiaSharp cannot decode HEIC"
      }
    ],
    "rationale": "This is classified as type/question because the behavior is by design: SkiaSharp does not support HEIC/HEIF due to licensing, and SKBitmap.Decode returning null is the expected result for unsupported formats. The reporter's situation changed because iOS 16 or the new device captures higher-resolution HEIC files. The fix is a platform-side conversion workaround, not a SkiaSharp code change.",
    "workarounds": [
      "Use UIKit on iOS to convert the HEIC image to JPEG before passing to SkiaSharp: use UIImage(data:) then JPEG representation.",
      "Configure iOS camera to capture in 'Most Compatible' mode (Settings > Camera > Formats > Most Compatible) to use JPEG instead of HEIC.",
      "Use CGImageSource on iOS to convert HEIC to a bitmap CGImage, then pass the raw pixels to SkiaSharp."
    ],
    "nextQuestions": [
      "What is the actual image format of the byte[]? Is it HEIC, JPEG, or another format?",
      "Does the byte[] come directly from NSData of the camera capture, and was HEIC format verified?"
    ],
    "resolution": {
      "hypothesis": "The byte array from iOS camera is in HEIC format, which SkiaSharp cannot decode. The reporter needs to convert using native iOS APIs.",
      "proposals": [
        {
          "title": "Convert HEIC to JPEG using native iOS APIs before passing to SkiaSharp",
          "description": "Use UIKit to create a UIImage from the HEIC bytes, then get JPEG representation and pass that to SkiaSharp.",
          "category": "workaround",
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "workaround-1",
      "recommendedReason": "HEIC support is not available in SkiaSharp due to licensing constraints; native iOS conversion is the most reliable approach."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.8,
      "reason": "SkiaSharp does not support HEIC/HEIF decoding by design. The null return from SKBitmap.Decode is the documented behavior for unsupported formats. Reporter needs to convert to JPEG using native iOS APIs first.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Add area/SkiaSharp and os/iOS labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "area/SkiaSharp",
          "os/iOS",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain that iOS camera produces HEIC images which SkiaSharp cannot decode, and suggest workaround",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thank you for reporting this!\n\nThe behavior you're seeing — `SKBitmap.Decode` returning `null` — is because iOS camera apps on iPhone 11 and later default to capturing images in **HEIC/HEIF format**, especially on iOS 16+. SkiaSharp currently cannot decode HEIC/HEIF images due to licensing constraints (see #1700 and #2887).\n\nThe increase in file size you noticed (~700KB → ~2MB) is consistent with a higher-resolution HEIC capture on iPhone 14 Pro Max.\n\n**Workaround:** Convert the image to JPEG on the iOS side before passing it to SkiaSharp:\n\n```csharp\n// On iOS, convert HEIC bytes to JPEG first\nvar uiImage = new UIImage(NSData.FromArray(heicBytes));\nvar jpegData = uiImage.AsJPEG(0.9f); // 0.9 quality\nbyte[] jpegBytes = jpegData.ToArray();\n\n// Now decode with SkiaSharp\nusing var bitmap = SKBitmap.Decode(jpegBytes);\n```\n\nAlternatively, ask users to change their iOS camera format to **\"Most Compatible\"** (Settings → Camera → Formats → Most Compatible), which saves images as JPEG.\n\nIf HEIC support is important to you, please 👍 the feature request at #2887."
      },
      {
        "type": "link-related",
        "description": "Link to HEIC feature request #2887",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 2887
      },
      {
        "type": "link-related",
        "description": "Link to closed HEIC feature request #1700",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 1700
      }
    ]
  }
}
```

</details>
