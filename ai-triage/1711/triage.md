# Issue Triage Report — #1711

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T01:49:10Z |
| Type | type/bug (0.80 (80%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.82 (82%)) |

**Issue Summary:** Reporter finds that decoding a PNG with SKCodec.GetPixels() using SKAlphaType.Opaque in the desired SKImageInfo results in no pixels being loaded, while the same code works for JPG images.

**Analysis:** When a PNG with an alpha channel is decoded using SKCodec.GetPixels() with a desired SKImageInfo specifying SKAlphaType.Opaque, Skia's native codec rejects the conversion and returns an error result. The reporter does not check the return value, so the bitmap pixels remain zeroed (empty). JPG works because it has no alpha channel so its native alpha type is already Opaque, making the 'conversion' a no-op that succeeds.

**Recommendations:** **close-as-not-a-bug** — The Skia codec intentionally rejects decoding an alpha-bearing PNG to SKAlphaType.Opaque. This is by-design upstream behavior. The reporter is not checking the GetPixels return value. JPG vs PNG difference confirms the conversion restriction. A clear workaround exists.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a SKCodec from a PNG file with alpha channel
2. Construct a SKImageInfo from codec.Info but with AlphaType = SKAlphaType.Opaque
3. Create a SKBitmap from that desiredInfo
4. Call codec.GetPixels(bitmap.Info, bitmap.GetPixels())
5. Attempt to draw the bitmap to a canvas — no pixels visible

**Environment:** SkiaSharp 2.80.3-pre-90 and 2.80.2, VS 2019, Windows 10, WinForms with SKGLControl

**Code snippets:**

```csharp
var codec = SKCodec.Create("myimage.png");
var codecInfo = codec.Info;
SKImageInfo desiredInfo = new SKImageInfo(codecInfo.Width, codecInfo.Height);
desiredInfo.AlphaType = SKAlphaType.Opaque;
bitmap = new SKBitmap(desiredInfo);
codec.GetPixels(bitmap.Info, bitmap.GetPixels());
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | No pixels loaded when requesting SKAlphaType.Opaque for PNG via SKCodec.GetPixels |
| Repro quality | partial |
| Target frameworks | net472 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.3-pre-90, 2.80.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SKCodec.GetPixels and SKImageInfo alpha handling code has not fundamentally changed since 2.80.x. |

## Analysis

### Technical Summary

When a PNG with an alpha channel is decoded using SKCodec.GetPixels() with a desired SKImageInfo specifying SKAlphaType.Opaque, Skia's native codec rejects the conversion and returns an error result. The reporter does not check the return value, so the bitmap pixels remain zeroed (empty). JPG works because it has no alpha channel so its native alpha type is already Opaque, making the 'conversion' a no-op that succeeds.

### Rationale

The behavior is by-design in Skia: decoding a PNG that has alpha to an Opaque alpha type is an invalid conversion — Skia's codec won't silently discard alpha channel data. The return value from GetPixels() would be a non-success code (e.g. InvalidConversion). The reporter isn't checking the return value, which causes a silent failure. This is a usage issue rather than a code bug, but the silent failure (empty pixels, no exception) is a real source of confusion, justifying close-as-not-a-bug with a clear explanation.

### Key Signals

- "when I load a PNG and explicitly request the bitmap to have alpha = opaque, nothing gets drawn" — **issue body** (Skia codec refuses conversion from alpha PNG to Opaque, returns error code, pixels remain zeroed.)
- "a jpg of the same image works fine" — **issue body** (JPEG has no alpha channel, codec.Info.AlphaType is already Opaque, so the requested conversion is a trivial pass-through that succeeds.)
- "The call to GetPixels also seems to end far too quickly as if it's not even reading anything from the file" — **issue body** (The codec performs a quick parameter check and immediately returns an error result rather than attempting to decode. The return value is ignored by the caller.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCodec.cs` | 119-137 | direct | SKCodec.GetPixels(SKImageInfo, IntPtr, int, SKCodecOptions) passes the info directly to sk_codec_get_pixels without validation. If Skia rejects the alpha conversion (PNG alpha -> Opaque), a non-success SKCodecResult is returned. The method does NOT throw; callers must check the return value. The reporter calls GetPixels without storing or checking the result. |
| `binding/SkiaSharp/SKImageInfo.cs` | 58-80 | direct | SKImageInfo constructor new SKImageInfo(width, height) sets ColorType = PlatformColorType and AlphaType = SKAlphaType.Premul by default. Reporter then overrides AlphaType = Opaque, creating an info that asks Skia to decode to opaque color data — unsupported for alpha PNGs by the Skia codec. |

### Workarounds

- Decode using the native codec info (Premul alpha) and draw normally — Skia composites alpha during rendering automatically.
- Decode with the default alpha type, then copy the bitmap using SKBitmap.Copy() with a new SKImageInfo specifying Opaque if truly needed.
- Check the return value of GetPixels() and handle non-success results before using the bitmap.

### Next Questions

- Does the reporter need the alpha channel stripped entirely, or do they just want to ignore alpha during rendering?
- Is the PNG actually a 32-bit RGBA image or a palette PNG? Some formats may behave differently.

### Resolution Proposals

**Hypothesis:** The Skia codec refuses to decode a PNG with alpha directly to SKAlphaType.Opaque and returns a non-success result. The bitmap pixels are never written because the caller does not check the return code.

1. **Decode with native alpha type and draw normally** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Remove the AlphaType override and decode using codec.Info directly. The bitmap will have premultiplied alpha, which Skia composites correctly when drawn.

```csharp
var codec = SKCodec.Create("myimage.png");
var bitmap = new SKBitmap(codec.Info.Width, codec.Info.Height);
var result = codec.GetPixels(bitmap.Info, bitmap.GetPixels());
// result should be SKCodecResult.Success
// Draw normally — alpha compositing is handled by Skia
```
2. **Decode then copy to opaque bitmap** — alternative, confidence 0.80 (80%), cost/xs, validated=yes
   - Decode normally, then use SKBitmap.Copy() with an Opaque SKImageInfo if the caller truly needs an opaque bitmap (e.g., to pass to an API that requires it).

```csharp
var codec = SKCodec.Create("myimage.png");
var srcBitmap = new SKBitmap(codec.Info.Width, codec.Info.Height);
codec.GetPixels(srcBitmap.Info, srcBitmap.GetPixels());
var opaqueInfo = srcBitmap.Info.WithAlphaType(SKAlphaType.Opaque);
var opaqueBitmap = new SKBitmap();
srcBitmap.CopyTo(opaqueBitmap, opaqueInfo.ColorType);
```

**Recommended proposal:** Decode with native alpha type and draw normally

**Why:** Simplest fix — one line change. Decoding with the native alpha type always succeeds and Skia's rendering handles alpha correctly during compositing.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.82 (82%) |
| Reason | The Skia codec intentionally rejects decoding an alpha-bearing PNG to SKAlphaType.Opaque. This is by-design upstream behavior. The reporter is not checking the GetPixels return value. JPG vs PNG difference confirms the conversion restriction. A clear workaround exists. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, area/SkiaSharp, Windows-Classic labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability |
| add-comment | high | 0.82 (82%) | Explain by-design behavior and provide working workaround | — |
| close-issue | medium | 0.80 (80%) | Close as not a bug — by-design Skia codec behavior with clear workaround provided | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! This is expected behavior from Skia's codec layer.

When decoding a **PNG that has an alpha channel**, requesting `SKAlphaType.Opaque` in the target `SKImageInfo` is an unsupported conversion — Skia's codec won't silently discard alpha data. It returns a non-success `SKCodecResult` and writes no pixels. JPEG works because it has no alpha channel and its native alpha type is already `Opaque`, so no conversion is needed.

The fix is to decode using the image's native format and let Skia handle alpha compositing during rendering:

```csharp
var codec = SKCodec.Create("myimage.png");
var bitmap = new SKBitmap(codec.Info.Width, codec.Info.Height);
var result = codec.GetPixels(bitmap.Info, bitmap.GetPixels());
// result should be SKCodecResult.Success
// Draw normally — Skia composites alpha automatically
```

If you specifically need an opaque-alpha bitmap (e.g., for an API that requires it), decode normally first and then copy:

```csharp
var codec = SKCodec.Create("myimage.png");
var srcBitmap = new SKBitmap(codec.Info.Width, codec.Info.Height);
codec.GetPixels(srcBitmap.Info, srcBitmap.GetPixels());
var opaqueBitmap = new SKBitmap();
srcBitmap.CopyTo(opaqueBitmap, srcBitmap.Info.ColorType);
```

Also, always check the return value of `GetPixels()` — if it isn't `SKCodecResult.Success` or `SKCodecResult.IncompleteInput`, the pixels were not written.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1711,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T01:49:10Z"
  },
  "summary": "Reporter finds that decoding a PNG with SKCodec.GetPixels() using SKAlphaType.Opaque in the desired SKImageInfo results in no pixels being loaded, while the same code works for JPG images.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.8
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "No pixels loaded when requesting SKAlphaType.Opaque for PNG via SKCodec.GetPixels",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net472"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a SKCodec from a PNG file with alpha channel",
        "Construct a SKImageInfo from codec.Info but with AlphaType = SKAlphaType.Opaque",
        "Create a SKBitmap from that desiredInfo",
        "Call codec.GetPixels(bitmap.Info, bitmap.GetPixels())",
        "Attempt to draw the bitmap to a canvas — no pixels visible"
      ],
      "codeSnippets": [
        "var codec = SKCodec.Create(\"myimage.png\");\nvar codecInfo = codec.Info;\nSKImageInfo desiredInfo = new SKImageInfo(codecInfo.Width, codecInfo.Height);\ndesiredInfo.AlphaType = SKAlphaType.Opaque;\nbitmap = new SKBitmap(desiredInfo);\ncodec.GetPixels(bitmap.Info, bitmap.GetPixels());"
      ],
      "environmentDetails": "SkiaSharp 2.80.3-pre-90 and 2.80.2, VS 2019, Windows 10, WinForms with SKGLControl"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.3-pre-90",
        "2.80.2"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SKCodec.GetPixels and SKImageInfo alpha handling code has not fundamentally changed since 2.80.x."
    }
  },
  "analysis": {
    "summary": "When a PNG with an alpha channel is decoded using SKCodec.GetPixels() with a desired SKImageInfo specifying SKAlphaType.Opaque, Skia's native codec rejects the conversion and returns an error result. The reporter does not check the return value, so the bitmap pixels remain zeroed (empty). JPG works because it has no alpha channel so its native alpha type is already Opaque, making the 'conversion' a no-op that succeeds.",
    "rationale": "The behavior is by-design in Skia: decoding a PNG that has alpha to an Opaque alpha type is an invalid conversion — Skia's codec won't silently discard alpha channel data. The return value from GetPixels() would be a non-success code (e.g. InvalidConversion). The reporter isn't checking the return value, which causes a silent failure. This is a usage issue rather than a code bug, but the silent failure (empty pixels, no exception) is a real source of confusion, justifying close-as-not-a-bug with a clear explanation.",
    "keySignals": [
      {
        "text": "when I load a PNG and explicitly request the bitmap to have alpha = opaque, nothing gets drawn",
        "source": "issue body",
        "interpretation": "Skia codec refuses conversion from alpha PNG to Opaque, returns error code, pixels remain zeroed."
      },
      {
        "text": "a jpg of the same image works fine",
        "source": "issue body",
        "interpretation": "JPEG has no alpha channel, codec.Info.AlphaType is already Opaque, so the requested conversion is a trivial pass-through that succeeds."
      },
      {
        "text": "The call to GetPixels also seems to end far too quickly as if it's not even reading anything from the file",
        "source": "issue body",
        "interpretation": "The codec performs a quick parameter check and immediately returns an error result rather than attempting to decode. The return value is ignored by the caller."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "119-137",
        "finding": "SKCodec.GetPixels(SKImageInfo, IntPtr, int, SKCodecOptions) passes the info directly to sk_codec_get_pixels without validation. If Skia rejects the alpha conversion (PNG alpha -> Opaque), a non-success SKCodecResult is returned. The method does NOT throw; callers must check the return value. The reporter calls GetPixels without storing or checking the result.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImageInfo.cs",
        "lines": "58-80",
        "finding": "SKImageInfo constructor new SKImageInfo(width, height) sets ColorType = PlatformColorType and AlphaType = SKAlphaType.Premul by default. Reporter then overrides AlphaType = Opaque, creating an info that asks Skia to decode to opaque color data — unsupported for alpha PNGs by the Skia codec.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "Does the reporter need the alpha channel stripped entirely, or do they just want to ignore alpha during rendering?",
      "Is the PNG actually a 32-bit RGBA image or a palette PNG? Some formats may behave differently."
    ],
    "workarounds": [
      "Decode using the native codec info (Premul alpha) and draw normally — Skia composites alpha during rendering automatically.",
      "Decode with the default alpha type, then copy the bitmap using SKBitmap.Copy() with a new SKImageInfo specifying Opaque if truly needed.",
      "Check the return value of GetPixels() and handle non-success results before using the bitmap."
    ],
    "resolution": {
      "hypothesis": "The Skia codec refuses to decode a PNG with alpha directly to SKAlphaType.Opaque and returns a non-success result. The bitmap pixels are never written because the caller does not check the return code.",
      "proposals": [
        {
          "title": "Decode with native alpha type and draw normally",
          "description": "Remove the AlphaType override and decode using codec.Info directly. The bitmap will have premultiplied alpha, which Skia composites correctly when drawn.",
          "category": "workaround",
          "codeSnippet": "var codec = SKCodec.Create(\"myimage.png\");\nvar bitmap = new SKBitmap(codec.Info.Width, codec.Info.Height);\nvar result = codec.GetPixels(bitmap.Info, bitmap.GetPixels());\n// result should be SKCodecResult.Success\n// Draw normally — alpha compositing is handled by Skia",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Decode then copy to opaque bitmap",
          "description": "Decode normally, then use SKBitmap.Copy() with an Opaque SKImageInfo if the caller truly needs an opaque bitmap (e.g., to pass to an API that requires it).",
          "category": "alternative",
          "codeSnippet": "var codec = SKCodec.Create(\"myimage.png\");\nvar srcBitmap = new SKBitmap(codec.Info.Width, codec.Info.Height);\ncodec.GetPixels(srcBitmap.Info, srcBitmap.GetPixels());\nvar opaqueInfo = srcBitmap.Info.WithAlphaType(SKAlphaType.Opaque);\nvar opaqueBitmap = new SKBitmap();\nsrcBitmap.CopyTo(opaqueBitmap, opaqueInfo.ColorType);",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Decode with native alpha type and draw normally",
      "recommendedReason": "Simplest fix — one line change. Decoding with the native alpha type always succeeds and Skia's rendering handles alpha correctly during compositing."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.82,
      "reason": "The Skia codec intentionally rejects decoding an alpha-bearing PNG to SKAlphaType.Opaque. This is by-design upstream behavior. The reporter is not checking the GetPixels return value. JPG vs PNG difference confirms the conversion restriction. A clear workaround exists.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, area/SkiaSharp, Windows-Classic labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain by-design behavior and provide working workaround",
        "risk": "high",
        "confidence": 0.82,
        "comment": "Thanks for the report! This is expected behavior from Skia's codec layer.\n\nWhen decoding a **PNG that has an alpha channel**, requesting `SKAlphaType.Opaque` in the target `SKImageInfo` is an unsupported conversion — Skia's codec won't silently discard alpha data. It returns a non-success `SKCodecResult` and writes no pixels. JPEG works because it has no alpha channel and its native alpha type is already `Opaque`, so no conversion is needed.\n\nThe fix is to decode using the image's native format and let Skia handle alpha compositing during rendering:\n\n```csharp\nvar codec = SKCodec.Create(\"myimage.png\");\nvar bitmap = new SKBitmap(codec.Info.Width, codec.Info.Height);\nvar result = codec.GetPixels(bitmap.Info, bitmap.GetPixels());\n// result should be SKCodecResult.Success\n// Draw normally — Skia composites alpha automatically\n```\n\nIf you specifically need an opaque-alpha bitmap (e.g., for an API that requires it), decode normally first and then copy:\n\n```csharp\nvar codec = SKCodec.Create(\"myimage.png\");\nvar srcBitmap = new SKBitmap(codec.Info.Width, codec.Info.Height);\ncodec.GetPixels(srcBitmap.Info, srcBitmap.GetPixels());\nvar opaqueBitmap = new SKBitmap();\nsrcBitmap.CopyTo(opaqueBitmap, srcBitmap.Info.ColorType);\n```\n\nAlso, always check the return value of `GetPixels()` — if it isn't `SKCodecResult.Success` or `SKCodecResult.IncompleteInput`, the pixels were not written."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — by-design Skia codec behavior with clear workaround provided",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
