# Issue Triage Report — #2887

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T23:24:28Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** Feature request to add native HEIC/HEIF image decoding and encoding support to SkiaSharp, primarily for Android and iOS MAUI apps where gallery and camera images use the HEIC format.

**Analysis:** HEIC/HEIF decoding and encoding is not natively supported in SkiaSharp due to licensing constraints. The libheif library required for HEIC codec support is licensed under LGPL 3.0, which the maintainer has explicitly stated is incompatible with SkiaSharp's distribution model. While `SKEncodedImageFormat.Heif` exists as an enum value (reflecting upstream Skia's codec enum), the actual native codec is not compiled in. Platform-native workarounds (Android BitmapFactory, iOS UIImage) are available and demonstrated by the reporter.

**Recommendations:** **keep-open** — Valid and widely-requested feature that is technically blocked by LGPL license constraints on libheif. The maintainer left it open to future resolution if a better-licensed alternative emerges. Keep open as a community tracking issue.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | os/Android, os/iOS |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/feature-request |

## Evidence

### Reproduction

1. Pick an image from the Android or iOS gallery that was taken with a modern iPhone (default HEIC format)
2. Attempt to load the .heic file using SKBitmap.Decode() or SKCodec.Create()
3. Observe that the image fails to decode — only JPEG images work

**Environment:** Android (MAUI), .NET MAUI, SkiaSharp (version not specified)

**Related issues:** #1700

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1700 — Previous HEIF feature request, closed not_planned (license issues)
- https://stackoverflow.com/questions/61232393/open-heic-bitmap-file-in-skiasharp — Stack Overflow confirming HEIC not supported in SkiaSharp

## Analysis

### Technical Summary

HEIC/HEIF decoding and encoding is not natively supported in SkiaSharp due to licensing constraints. The libheif library required for HEIC codec support is licensed under LGPL 3.0, which the maintainer has explicitly stated is incompatible with SkiaSharp's distribution model. While `SKEncodedImageFormat.Heif` exists as an enum value (reflecting upstream Skia's codec enum), the actual native codec is not compiled in. Platform-native workarounds (Android BitmapFactory, iOS UIImage) are available and demonstrated by the reporter.

### Rationale

Classified as type/feature-request because this is requesting new image format support not currently in the library. The request is valid and well-understood but blocked by LGPL license constraints on libheif. Previous issue #1700 was explicitly closed as not_planned for this same reason. The maintainer suggested a potential path via a separate NuGet package or via LibHeifSharp + SKImage creation from raw pixels.

### Key Signals

- "Feature request https://github.com/mono/SkiaSharp/issues/1700 was close because of license issues" — **issue body** (Reporter is aware of the prior request and the license blocker; this is a re-request hoping the situation has changed.)
- "I am going to close this issue for now unless a new library appears that can load HEIF with a better license." — **issue #1700 comment by maintainer mattleibow** (Explicit policy decision: HEIC support is deferred until a permissively-licensed alternative exists.)
- "Encoding does not work either, it would be great if it were supported too." — **comment #2 on issue #2887** (Demand extends beyond decoding to HEIC encoding, broadening the scope.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaApi.generated.cs` | 20845-20846 | direct | SKEncodedImageFormat.Heif = 11 enum value exists — the format identifier is defined in the upstream Skia C API, but the presence of this enum does not imply the native codec is enabled in the SkiaSharp build. |
| `binding/SkiaSharp/SKPixmap.cs` | 235-246 | direct | SKPixmap.Encode(SKWStream, SKEncodedImageFormat, int) only handles JPEG, PNG, and WebP via a switch expression — all other formats (including HEIF) fall through to return false. No HEIF encoder is wired up. |
| `binding/SkiaSharp/SKCodec.cs` | 39-40 | related | SKCodec.EncodedFormat property can detect the format of a stream, but HEIC decoding requires the native Skia build to include the HEIF codec, which depends on libheif being present at compile time. |

### Workarounds

- On Android: Use Android.Graphics.BitmapFactory.DecodeFile() to decode .heic to an Android Bitmap, then compress to JPEG, save to cache, and load via SKBitmap.Decode().
- On iOS: Use UIKit.UIImage.LoadFromData() to load the HEIC file, then call AsJPEG() to get JPEG data, save to cache, and load via SKBitmap.Decode().
- Cross-platform: Use the LibHeifSharp NuGet package (https://www.nuget.org/packages/LibHeifSharp) to decode HEIC to raw pixel data, then construct an SKImage from the raw pixel pointer using SKImage.FromPixels().

### Next Questions

- Has the LGPL 3.0 licensing situation for libheif changed or become more acceptable since 2023?
- Is there a mechanism in Skia/SkiaSharp for registering external codecs at runtime that could enable opt-in HEIC support?
- Could platform-native HEIC decoding be exposed as a first-class SkiaSharp API (e.g., via platform-specific SKCodecExtensions) without requiring libheif?

### Resolution Proposals

**Hypothesis:** The HEIC codec is blocked by libheif's LGPL 3.0 license. A path forward exists via: (1) a separate opt-in NuGet package that accepts the LGPL license, or (2) exposing a codec registration API so third-party packages like LibHeifSharp can plug in. Platform-native decoding paths already work on Android and iOS.

1. **Platform-native HEIC to JPEG conversion (workaround)** — workaround, confidence 0.90 (90%), cost/s, validated=yes
   - Convert HEIC to JPEG using native platform APIs before passing to SkiaSharp. Works on Android and iOS today with no extra dependencies.

```csharp
// Android: decode HEIC via BitmapFactory, compress to JPEG, load with SkiaSharp
using var bmp = await BitmapFactory.DecodeFileAsync(heicFilePath);
using var ms = new MemoryStream();
bmp.Compress(Bitmap.CompressFormat.Jpeg, 95, ms);
ms.Position = 0;
var skBitmap = SKBitmap.Decode(ms);
```
2. **Use LibHeifSharp for cross-platform HEIC decode** — workaround, confidence 0.80 (80%), cost/m, validated=untested
   - Use the LibHeifSharp NuGet package to decode HEIC to raw pixel data, then wrap in SKImage. Works cross-platform but adds an LGPL dependency.
3. **Add HEIC codec support via separate NuGet package** — fix, confidence 0.70 (70%), cost/xl, validated=untested
   - Create a SkiaSharp.Codec.Heif NuGet package that bundles libheif and registers the codec with SkiaSharp. Users opt in and accept the LGPL license. This was hinted at by the maintainer in #1700.

**Recommended proposal:** Platform-native HEIC to JPEG conversion (workaround)

**Why:** The platform-native workaround is validated, requires no extra dependencies, and works today on both Android and iOS. The reporter already confirmed it works. A proper fix requires licensing decisions that must be made by the maintainer.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | Valid and widely-requested feature that is technically blocked by LGPL license constraints on libheif. The maintainer left it open to future resolution if a better-licensed alternative emerges. Keep open as a community tracking issue. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply feature-request, SkiaSharp area, and platform labels | labels=type/feature-request, area/SkiaSharp, os/Android, os/iOS |
| link-related | low | 0.95 (95%) | Cross-reference the prior HEIC feature request that was closed for license reasons | linkedIssue=#1700 |
| add-comment | medium | 0.88 (88%) | Explain the license blocker and provide platform-native workarounds | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the feature request! HEIC/HEIF support in SkiaSharp has been requested before in #1700, but was closed because the required library (libheif) is licensed under **LGPL 3.0**, which creates distribution complications for SkiaSharp.

While `SKEncodedImageFormat.Heif` exists in the API (reflecting Skia's codec enum), the actual native decoder isn't compiled into the SkiaSharp native libraries.

**Workarounds available today:**

**Android (via BitmapFactory):**
```csharp
using var bmp = await Android.Graphics.BitmapFactory.DecodeFileAsync(heicFilePath);
using var ms = new MemoryStream();
bmp.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg, 95, ms);
ms.Position = 0;
var skBitmap = SKBitmap.Decode(ms);
```

**iOS (via UIImage):**
```csharp
using var data = Foundation.NSData.FromFile(heicFilePath);
using var image = UIKit.UIImage.LoadFromData(data);
using var jpegData = image.AsJPEG();
// Save to temp file or stream, then decode with SkiaSharp
```

**Cross-platform alternative:** [LibHeifSharp](https://www.nuget.org/packages/LibHeifSharp) can decode HEIC to raw pixels, which you can then wrap in `SKImage.FromPixels()`.

We'll keep this open as a tracking issue. A path forward could be a separate opt-in NuGet package that bundles libheif for users who explicitly accept the LGPL license.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2887,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T23:24:28Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "Feature request to add native HEIC/HEIF image decoding and encoding support to SkiaSharp, primarily for Android and iOS MAUI apps where gallery and camera images use the HEIC format.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    },
    "platforms": [
      "os/Android",
      "os/iOS"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Pick an image from the Android or iOS gallery that was taken with a modern iPhone (default HEIC format)",
        "Attempt to load the .heic file using SKBitmap.Decode() or SKCodec.Create()",
        "Observe that the image fails to decode — only JPEG images work"
      ],
      "environmentDetails": "Android (MAUI), .NET MAUI, SkiaSharp (version not specified)",
      "relatedIssues": [
        1700
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1700",
          "description": "Previous HEIF feature request, closed not_planned (license issues)"
        },
        {
          "url": "https://stackoverflow.com/questions/61232393/open-heic-bitmap-file-in-skiasharp",
          "description": "Stack Overflow confirming HEIC not supported in SkiaSharp"
        }
      ]
    }
  },
  "analysis": {
    "summary": "HEIC/HEIF decoding and encoding is not natively supported in SkiaSharp due to licensing constraints. The libheif library required for HEIC codec support is licensed under LGPL 3.0, which the maintainer has explicitly stated is incompatible with SkiaSharp's distribution model. While `SKEncodedImageFormat.Heif` exists as an enum value (reflecting upstream Skia's codec enum), the actual native codec is not compiled in. Platform-native workarounds (Android BitmapFactory, iOS UIImage) are available and demonstrated by the reporter.",
    "rationale": "Classified as type/feature-request because this is requesting new image format support not currently in the library. The request is valid and well-understood but blocked by LGPL license constraints on libheif. Previous issue #1700 was explicitly closed as not_planned for this same reason. The maintainer suggested a potential path via a separate NuGet package or via LibHeifSharp + SKImage creation from raw pixels.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "20845-20846",
        "finding": "SKEncodedImageFormat.Heif = 11 enum value exists — the format identifier is defined in the upstream Skia C API, but the presence of this enum does not imply the native codec is enabled in the SkiaSharp build.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPixmap.cs",
        "lines": "235-246",
        "finding": "SKPixmap.Encode(SKWStream, SKEncodedImageFormat, int) only handles JPEG, PNG, and WebP via a switch expression — all other formats (including HEIF) fall through to return false. No HEIF encoder is wired up.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "39-40",
        "finding": "SKCodec.EncodedFormat property can detect the format of a stream, but HEIC decoding requires the native Skia build to include the HEIF codec, which depends on libheif being present at compile time.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Feature request https://github.com/mono/SkiaSharp/issues/1700 was close because of license issues",
        "source": "issue body",
        "interpretation": "Reporter is aware of the prior request and the license blocker; this is a re-request hoping the situation has changed."
      },
      {
        "text": "I am going to close this issue for now unless a new library appears that can load HEIF with a better license.",
        "source": "issue #1700 comment by maintainer mattleibow",
        "interpretation": "Explicit policy decision: HEIC support is deferred until a permissively-licensed alternative exists."
      },
      {
        "text": "Encoding does not work either, it would be great if it were supported too.",
        "source": "comment #2 on issue #2887",
        "interpretation": "Demand extends beyond decoding to HEIC encoding, broadening the scope."
      }
    ],
    "workarounds": [
      "On Android: Use Android.Graphics.BitmapFactory.DecodeFile() to decode .heic to an Android Bitmap, then compress to JPEG, save to cache, and load via SKBitmap.Decode().",
      "On iOS: Use UIKit.UIImage.LoadFromData() to load the HEIC file, then call AsJPEG() to get JPEG data, save to cache, and load via SKBitmap.Decode().",
      "Cross-platform: Use the LibHeifSharp NuGet package (https://www.nuget.org/packages/LibHeifSharp) to decode HEIC to raw pixel data, then construct an SKImage from the raw pixel pointer using SKImage.FromPixels()."
    ],
    "resolution": {
      "hypothesis": "The HEIC codec is blocked by libheif's LGPL 3.0 license. A path forward exists via: (1) a separate opt-in NuGet package that accepts the LGPL license, or (2) exposing a codec registration API so third-party packages like LibHeifSharp can plug in. Platform-native decoding paths already work on Android and iOS.",
      "proposals": [
        {
          "title": "Platform-native HEIC to JPEG conversion (workaround)",
          "description": "Convert HEIC to JPEG using native platform APIs before passing to SkiaSharp. Works on Android and iOS today with no extra dependencies.",
          "codeSnippet": "// Android: decode HEIC via BitmapFactory, compress to JPEG, load with SkiaSharp\nusing var bmp = await BitmapFactory.DecodeFileAsync(heicFilePath);\nusing var ms = new MemoryStream();\nbmp.Compress(Bitmap.CompressFormat.Jpeg, 95, ms);\nms.Position = 0;\nvar skBitmap = SKBitmap.Decode(ms);",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "yes"
        },
        {
          "title": "Use LibHeifSharp for cross-platform HEIC decode",
          "description": "Use the LibHeifSharp NuGet package to decode HEIC to raw pixel data, then wrap in SKImage. Works cross-platform but adds an LGPL dependency.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Add HEIC codec support via separate NuGet package",
          "description": "Create a SkiaSharp.Codec.Heif NuGet package that bundles libheif and registers the codec with SkiaSharp. Users opt in and accept the LGPL license. This was hinted at by the maintainer in #1700.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/xl",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Platform-native HEIC to JPEG conversion (workaround)",
      "recommendedReason": "The platform-native workaround is validated, requires no extra dependencies, and works today on both Android and iOS. The reporter already confirmed it works. A proper fix requires licensing decisions that must be made by the maintainer."
    },
    "nextQuestions": [
      "Has the LGPL 3.0 licensing situation for libheif changed or become more acceptable since 2023?",
      "Is there a mechanism in Skia/SkiaSharp for registering external codecs at runtime that could enable opt-in HEIC support?",
      "Could platform-native HEIC decoding be exposed as a first-class SkiaSharp API (e.g., via platform-specific SKCodecExtensions) without requiring libheif?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "Valid and widely-requested feature that is technically blocked by LGPL license constraints on libheif. The maintainer left it open to future resolution if a better-licensed alternative emerges. Keep open as a community tracking issue.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, SkiaSharp area, and platform labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "os/Android",
          "os/iOS"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference the prior HEIC feature request that was closed for license reasons",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 1700
      },
      {
        "type": "add-comment",
        "description": "Explain the license blocker and provide platform-native workarounds",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the feature request! HEIC/HEIF support in SkiaSharp has been requested before in #1700, but was closed because the required library (libheif) is licensed under **LGPL 3.0**, which creates distribution complications for SkiaSharp.\n\nWhile `SKEncodedImageFormat.Heif` exists in the API (reflecting Skia's codec enum), the actual native decoder isn't compiled into the SkiaSharp native libraries.\n\n**Workarounds available today:**\n\n**Android (via BitmapFactory):**\n```csharp\nusing var bmp = await Android.Graphics.BitmapFactory.DecodeFileAsync(heicFilePath);\nusing var ms = new MemoryStream();\nbmp.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg, 95, ms);\nms.Position = 0;\nvar skBitmap = SKBitmap.Decode(ms);\n```\n\n**iOS (via UIImage):**\n```csharp\nusing var data = Foundation.NSData.FromFile(heicFilePath);\nusing var image = UIKit.UIImage.LoadFromData(data);\nusing var jpegData = image.AsJPEG();\n// Save to temp file or stream, then decode with SkiaSharp\n```\n\n**Cross-platform alternative:** [LibHeifSharp](https://www.nuget.org/packages/LibHeifSharp) can decode HEIC to raw pixels, which you can then wrap in `SKImage.FromPixels()`.\n\nWe'll keep this open as a tracking issue. A path forward could be a separate opt-in NuGet package that bundles libheif for users who explicitly accept the LGPL license."
      }
    ]
  }
}
```

</details>
