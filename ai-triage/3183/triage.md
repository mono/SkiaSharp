# Issue Triage Report — #3183

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T19:40:00Z |
| Type | type/question (0.88 (88%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** Reporter observes JPEG output from SKBitmap.Encode is larger than input when using quality=100, misunderstanding JPEG lossy re-encoding behavior.

**Analysis:** The reporter is using quality=100 to re-encode a JPEG that was originally compressed at a lower quality setting. JPEG is a lossy format that does not preserve encoder settings; re-encoding at quality=100 (maximum) will always produce a file larger than one originally compressed at quality ~75-85. This is correct behavior and not a SkiaSharp defect. Multiple community comments (including one with upvote) have already explained this.

**Recommendations:** **close-as-not-a-bug** — This is expected JPEG encoding behavior. Using quality=100 on a re-encoded JPEG will produce a larger file than the original. Multiple community members have already correctly explained this. No SkiaSharp defect exists.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Load a JPEG file as bytes
2. Create SKCodec from SKMemoryStream
3. Decode to SKBitmap via SKBitmap.Decode(codec)
4. Re-encode with SKBitmap.Encode(outputStream, codec.EncodedFormat, 100)
5. Observe outputStream.Length > input imageData.Length

**Environment:** SkiaSharp 3.116.0, Windows 11 Pro, Visual Studio

**Repository links:**
- https://github.com/user-attachments/files/19095189/SkiaSharpIssue.zip — Reporter's sample project

**Code snippets:**

```csharp
originalImage.Encode(outputStream, codec.EncodedFormat, 100);
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | This is expected JPEG encoding behavior, not a defect in SkiaSharp. The behavior has always been this way. |

## Analysis

### Technical Summary

The reporter is using quality=100 to re-encode a JPEG that was originally compressed at a lower quality setting. JPEG is a lossy format that does not preserve encoder settings; re-encoding at quality=100 (maximum) will always produce a file larger than one originally compressed at quality ~75-85. This is correct behavior and not a SkiaSharp defect. Multiple community comments (including one with upvote) have already explained this.

### Rationale

This is a question/misunderstanding about JPEG encoding, not a bug. The API correctly passes quality=100 to the Skia JPEG encoder. Community members already correctly identified the root cause. The appropriate resolution is to clarify expected behavior and suggest using a lower quality value.

### Key Signals

- "originalImage.Encode(outputStream, codec.EncodedFormat, 100)" — **issue body** (Quality=100 (maximum) will produce larger output than the typical original JPEG quality of 75-85.)
- "JPEG is a lossy format, which means that information disappears and/or changes when you re-encode a JPEG image." — **comment by TommiGustafsson-HMP (1 upvote)** (Community already identified the root cause correctly.)
- "The size likely increases simply because of your encoder settings. This doesn't sound like a bug" — **comment by molesmoke** (Community agrees this is not a bug.)
- "I have utilized Magick.NET for the same purpose, and it works flawlessly" — **comment by reporter** (Magick.NET likely uses a different default quality or re-uses metadata — not comparable to re-encoding with quality=100.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 738-741 | direct | SKBitmap.Encode(Stream dst, SKEncodedImageFormat format, int quality) wraps the stream and delegates to SKPixmap.Encode. The quality parameter is passed as-is to the Skia JPEG encoder. There is no bug here — quality=100 is correctly forwarded to the native encoder. |
| `binding/SkiaSharp/SKBitmap.cs` | 744-751 | direct | SKBitmap.Encode(SKWStream dst, ...) delegates to SKPixmap.Encode — no special logic, no quality clamping or preservation of original quality. The encoding quality is entirely determined by the caller-supplied integer. |

### Workarounds

- Use a lower quality value (e.g., 75-85) to produce output files closer in size to typical JPEG source files.
- If preserving original visual quality is the goal, use quality=85 as a reasonable default that balances size and quality.
- If the goal is to avoid re-encoding entirely (e.g., just rotate metadata), consider using a JPEG-specific lossless transform library (not available in SkiaSharp).

### Resolution Proposals

**Hypothesis:** The reporter expects round-trip JPEG encoding to preserve file size, but JPEG does not store or expose the encoder quality setting used to create the original file. Re-encoding at quality=100 produces a higher-quality (larger) file than the original.

1. **Use lower quality setting** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Encode with quality=85 (or 75-90 range) to produce output closer in size to typical JPEGs. Quality=100 is the maximum and will almost always produce files larger than typical originals.

```csharp
originalImage.Encode(outputStream, codec.EncodedFormat, 85);
```
2. **Accept that JPEG round-trip increases file size** — alternative, confidence 0.95 (95%), cost/xs, validated=untested
   - This is fundamental JPEG behavior. The only way to guarantee the same or smaller file size is to use lossless formats (PNG) or use a library that supports lossless JPEG transformations (not available in SkiaSharp/Skia).

**Recommended proposal:** Use lower quality setting

**Why:** Simplest actionable change. Quality=85 is a well-accepted JPEG quality that produces files similar in size to typical JPEG sources while maintaining good visual fidelity.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | This is expected JPEG encoding behavior. Using quality=100 on a re-encoded JPEG will produce a larger file than the original. Multiple community members have already correctly explained this. No SkiaSharp defect exists. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Correct label from type/bug to type/question and add area/SkiaSharp and os/Windows-Classic | labels=type/question, area/SkiaSharp, os/Windows-Classic |
| add-comment | high | 0.88 (88%) | Explain JPEG encoding behavior and suggest lower quality value | — |
| close-issue | medium | 0.85 (85%) | Close as not a bug — expected JPEG behavior | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report! This is expected behavior of the JPEG format rather than a bug in SkiaSharp.

JPEG is a lossy format and **does not store the quality setting** used when the original image was created. When you decode a JPEG to a raw bitmap and re-encode it, the output quality is determined entirely by the `quality` parameter you pass to `Encode()`. Since you're passing `100` (maximum quality), the output will almost always be **larger** than the original, which was likely compressed at a typical quality of 75–90.

To produce output closer in size to the original, use a lower quality value:

```csharp
originalImage.Encode(outputStream, codec.EncodedFormat, 85);
```

A quality of 85 is a common choice that balances visual fidelity and file size. You can tune this value based on your requirements.

Note that every JPEG encode/decode cycle introduces some additional quality loss (generation loss). If you need to avoid this entirely, consider:
- Keeping the original bytes if you don't actually modify the image
- Using a lossless format (PNG) when you need to re-encode
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3183,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T19:40:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter observes JPEG output from SKBitmap.Encode is larger than input when using quality=100, misunderstanding JPEG lossy re-encoding behavior.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Classic"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Load a JPEG file as bytes",
        "Create SKCodec from SKMemoryStream",
        "Decode to SKBitmap via SKBitmap.Decode(codec)",
        "Re-encode with SKBitmap.Encode(outputStream, codec.EncodedFormat, 100)",
        "Observe outputStream.Length > input imageData.Length"
      ],
      "codeSnippets": [
        "originalImage.Encode(outputStream, codec.EncodedFormat, 100);"
      ],
      "environmentDetails": "SkiaSharp 3.116.0, Windows 11 Pro, Visual Studio",
      "repoLinks": [
        {
          "url": "https://github.com/user-attachments/files/19095189/SkiaSharpIssue.zip",
          "description": "Reporter's sample project"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "This is expected JPEG encoding behavior, not a defect in SkiaSharp. The behavior has always been this way."
    }
  },
  "analysis": {
    "summary": "The reporter is using quality=100 to re-encode a JPEG that was originally compressed at a lower quality setting. JPEG is a lossy format that does not preserve encoder settings; re-encoding at quality=100 (maximum) will always produce a file larger than one originally compressed at quality ~75-85. This is correct behavior and not a SkiaSharp defect. Multiple community comments (including one with upvote) have already explained this.",
    "rationale": "This is a question/misunderstanding about JPEG encoding, not a bug. The API correctly passes quality=100 to the Skia JPEG encoder. Community members already correctly identified the root cause. The appropriate resolution is to clarify expected behavior and suggest using a lower quality value.",
    "keySignals": [
      {
        "text": "originalImage.Encode(outputStream, codec.EncodedFormat, 100)",
        "source": "issue body",
        "interpretation": "Quality=100 (maximum) will produce larger output than the typical original JPEG quality of 75-85."
      },
      {
        "text": "JPEG is a lossy format, which means that information disappears and/or changes when you re-encode a JPEG image.",
        "source": "comment by TommiGustafsson-HMP (1 upvote)",
        "interpretation": "Community already identified the root cause correctly."
      },
      {
        "text": "The size likely increases simply because of your encoder settings. This doesn't sound like a bug",
        "source": "comment by molesmoke",
        "interpretation": "Community agrees this is not a bug."
      },
      {
        "text": "I have utilized Magick.NET for the same purpose, and it works flawlessly",
        "source": "comment by reporter",
        "interpretation": "Magick.NET likely uses a different default quality or re-uses metadata — not comparable to re-encoding with quality=100."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "738-741",
        "finding": "SKBitmap.Encode(Stream dst, SKEncodedImageFormat format, int quality) wraps the stream and delegates to SKPixmap.Encode. The quality parameter is passed as-is to the Skia JPEG encoder. There is no bug here — quality=100 is correctly forwarded to the native encoder.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "744-751",
        "finding": "SKBitmap.Encode(SKWStream dst, ...) delegates to SKPixmap.Encode — no special logic, no quality clamping or preservation of original quality. The encoding quality is entirely determined by the caller-supplied integer.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Use a lower quality value (e.g., 75-85) to produce output files closer in size to typical JPEG source files.",
      "If preserving original visual quality is the goal, use quality=85 as a reasonable default that balances size and quality.",
      "If the goal is to avoid re-encoding entirely (e.g., just rotate metadata), consider using a JPEG-specific lossless transform library (not available in SkiaSharp)."
    ],
    "resolution": {
      "hypothesis": "The reporter expects round-trip JPEG encoding to preserve file size, but JPEG does not store or expose the encoder quality setting used to create the original file. Re-encoding at quality=100 produces a higher-quality (larger) file than the original.",
      "proposals": [
        {
          "title": "Use lower quality setting",
          "description": "Encode with quality=85 (or 75-90 range) to produce output closer in size to typical JPEGs. Quality=100 is the maximum and will almost always produce files larger than typical originals.",
          "category": "workaround",
          "codeSnippet": "originalImage.Encode(outputStream, codec.EncodedFormat, 85);",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Accept that JPEG round-trip increases file size",
          "description": "This is fundamental JPEG behavior. The only way to guarantee the same or smaller file size is to use lossless formats (PNG) or use a library that supports lossless JPEG transformations (not available in SkiaSharp/Skia).",
          "category": "alternative",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use lower quality setting",
      "recommendedReason": "Simplest actionable change. Quality=85 is a well-accepted JPEG quality that produces files similar in size to typical JPEG sources while maintaining good visual fidelity."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "This is expected JPEG encoding behavior. Using quality=100 on a re-encoded JPEG will produce a larger file than the original. Multiple community members have already correctly explained this. No SkiaSharp defect exists.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct label from type/bug to type/question and add area/SkiaSharp and os/Windows-Classic",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/Windows-Classic"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain JPEG encoding behavior and suggest lower quality value",
        "risk": "high",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report! This is expected behavior of the JPEG format rather than a bug in SkiaSharp.\n\nJPEG is a lossy format and **does not store the quality setting** used when the original image was created. When you decode a JPEG to a raw bitmap and re-encode it, the output quality is determined entirely by the `quality` parameter you pass to `Encode()`. Since you're passing `100` (maximum quality), the output will almost always be **larger** than the original, which was likely compressed at a typical quality of 75–90.\n\nTo produce output closer in size to the original, use a lower quality value:\n\n```csharp\noriginalImage.Encode(outputStream, codec.EncodedFormat, 85);\n```\n\nA quality of 85 is a common choice that balances visual fidelity and file size. You can tune this value based on your requirements.\n\nNote that every JPEG encode/decode cycle introduces some additional quality loss (generation loss). If you need to avoid this entirely, consider:\n- Keeping the original bytes if you don't actually modify the image\n- Using a lossless format (PNG) when you need to re-encode"
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — expected JPEG behavior",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
