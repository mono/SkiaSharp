# Issue Triage Report — #2466

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T23:51:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp.Views (0.85 (85%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** SKBitmapImageSource displays incorrect colors when the source SKBitmap has a non-sRGB color space (e.g. linear), because the platform-specific conversion extensions (ToBitmap/ToCGImage/ToWriteableBitmap) ignore the bitmap's color space and copy raw pixels without color-space transformation.

**Analysis:** The platform conversion methods (ToCGImage on Apple, ToBitmap on Android, ToWriteableBitmap on WPF) all copy raw pixel bytes from the SKBitmap without performing color-space conversion to sRGB. On WPF/Android, they create a destination SKImageInfo with null color space (legacy mode), which causes Skia's ReadPixels to skip color-space transformation. On Apple, ToCGImage(SKBitmap) hardcodes CGColorSpace.CreateDeviceRGB() and CGBitmapFlags.PremultipliedLast|ByteOrder32Big regardless of the bitmap's actual color type and color space, causing raw bytes to be misinterpreted.

**Recommendations:** **needs-investigation** — Bug is confirmed by code investigation. Root cause is clear but the fix touches multiple platform extension files. Repro project exists but is external — a minimal inline repro would help confirm the fix.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Android, os/Windows-Classic, os/Windows-WinUI, os/iOS, os/macOS |
| Backends | — |
| Tenets | tenet/reliability, tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create an SKBitmap with a non-sRGB color space (e.g. SKColorSpace.CreateSrgbLinear())
2. Use it as SKBitmapImageSource in a MAUI Image control (or WPF Image)
3. Observe that the displayed image is darker or has wrong colors compared to an sRGB bitmap with the same visual content

**Environment:** SkiaSharp 2.88.3, .NET 7.0.302; tested on MAUI/Windows 11, WPF/Windows 11, MAUI/Android, MAUI/macOS, MAUI/iOS

**Repository links:**
- https://github.com/koszeggy/KGySoft.Drawing/tree/master/Examples/SkiaSharp_(Maui) — External MAUI repro project — must disable the workaround in MainViewModel.cs:362
- https://github.com/mono/SkiaSharp/issues/2354 — Related: SKBitmap.GetPixel also ignores color space (same reporter)

**Screenshots:**
- https://github.com/mono/SkiaSharp/assets/27336165/978e55ed-7f16-4fd2-9fae-439db49d29e5 — Expected result, regardless of color space
- https://github.com/mono/SkiaSharp/assets/27336165/b2fbfdc0-9d06-4618-83e0-1f7973abb86c — Using linear color space - appears much darker
- https://github.com/mono/SkiaSharp/assets/27336165/426814aa-193b-427d-8254-a631283c6e60 — Android specific: Argb4444 color type wrong even with sRGB color space
- https://github.com/mono/SkiaSharp/assets/27336165/0c445b83-e0d6-4aa3-bfb7-c33f389963f1 — macOS/iOS: only Bgra8888 sRGB is displayed correctly

**Code snippets:**

```csharp
var myImageSource = new SKBitmapImageSource { Bitmap = myNonSrgbBitmap };
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Colors appear much darker or wrong when bitmap uses linear or non-sRGB color space |
| Repro quality | partial |
| Target frameworks | net7.0-android, net7.0-ios, net7.0-maccatalyst, net7.0-windows |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The platform extension methods (ToCGImage, ToBitmap, ToWriteableBitmap) still do not perform color-space conversion — code was reviewed and confirms the bug persists. |

## Analysis

### Technical Summary

The platform conversion methods (ToCGImage on Apple, ToBitmap on Android, ToWriteableBitmap on WPF) all copy raw pixel bytes from the SKBitmap without performing color-space conversion to sRGB. On WPF/Android, they create a destination SKImageInfo with null color space (legacy mode), which causes Skia's ReadPixels to skip color-space transformation. On Apple, ToCGImage(SKBitmap) hardcodes CGColorSpace.CreateDeviceRGB() and CGBitmapFlags.PremultipliedLast|ByteOrder32Big regardless of the bitmap's actual color type and color space, causing raw bytes to be misinterpreted.

### Rationale

This is unambiguously a bug: the reporter provides screenshots, an external repro project, and a working workaround. Code investigation confirms the platform extension methods ignore the source bitmap's color space. The related issue #2354 (GetPixel ignores color space) is a separate but related problem by the same reporter.

### Key Signals

- "only sRGB color space is displayed correctly so results with linear actual color space must be converted to sRGB" — **issue body (workaround comment)** (Workaround already exists — confirms the root cause is missing color-space conversion in the conversion pipeline.)
- "Using linear color space the result appears much darker" — **issue body** (Linear-to-sRGB conversion missing; linear values appear darker because the display assumes sRGB gamma.)
- "macOS/iOS: Everything but Bgra8888 sRGB is displayed incorrectly" — **issue body** (ToCGImage hardcodes BGRA format flags, misinterpreting pixels of other color types.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/AppleExtensions.cs` | 165-184 | direct | ToCGImage(SKBitmap) reads raw pixel bytes and hardcodes CGColorSpace.CreateDeviceRGB() with CGBitmapFlags.PremultipliedLast|ByteOrder32Big, ignoring the actual color type and color space of the input bitmap. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/AndroidExtensions.cs` | 132-138 | direct | ToBitmap(SKBitmap) delegates to PeekPixels().ToBitmap() which wraps the pixmap in SKImage.FromPixels then calls ToBitmap(SKImage). The destination SKImageInfo is created with new SKImageInfo(width, height) — no color space — causing Skia to treat the destination as legacy sRGB without performing color-space conversion. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/WPFExtensions.cs` | 66-83 | direct | ToWriteableBitmap(SKImage) creates a destination SKImageInfo with no color space (null), then calls skiaImage.ReadPixels(pixmap). Null color space means Skia uses legacy mode and skips color-space conversion from the source's linear/non-sRGB space. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKImageSourceService/SKImageSourceService.Android.cs` | 14-26 | related | SKImageSourceService for Android calls bmp.Bitmap?.ToBitmap() — delegating to the extension method that ignores color space. |

### Workarounds

- Convert the bitmap to sRGB before assigning to SKBitmapImageSource: create a new SKBitmap with SKImageInfo using SKColorSpace.CreateSrgb(), draw/copy pixels with color space conversion, then use that bitmap as the image source.
- On WPF/Windows and Android: manually ensure the SKBitmap uses SKImageInfo.PlatformColorType and SKColorSpace.CreateSrgb() before passing to SKBitmapImageSource.
- On macOS/iOS: additionally ensure the color type is Bgra8888 (SKImageInfo.PlatformColorType) with Premul alpha type and sRGB color space.

### Next Questions

- Does SKImageSourceService.Tizen.cs have the same issue?
- Should the fix be in the platform extension methods (ToCGImage, ToBitmap, ToWriteableBitmap) to always convert to sRGB/platform format, or should SKBitmapImageSource normalise the bitmap before calling the extension?
- Is there a performance concern with always converting — should conversion be conditional on color space?

### Resolution Proposals

**Hypothesis:** All platform conversion methods need to specify a sRGB destination color space so Skia performs color-space conversion during ReadPixels, and the Apple ToCGImage needs to convert to a supported format first.

1. **Fix platform extensions to specify destination color space** — fix, confidence 0.80 (80%), cost/m, validated=untested
   - In WPF ToWriteableBitmap and Android ToBitmap, set the destination SKImageInfo's ColorSpace to SKColorSpace.CreateSrgb() so Skia performs color-space conversion. For Apple ToCGImage(SKBitmap), first convert the bitmap to an SKImage with a standard format (Bgra8888/Premul/sRGB) using SKBitmap.FromImage or SKImage.FromBitmap with a color-space-aware destination, then create the CGImage.
2. **Normalize bitmap in SKBitmapImageSource** — workaround, confidence 0.70 (70%), cost/s, validated=untested
   - In SKImageSourceService, before delegating to the platform extension, check if the bitmap's color space differs from sRGB and if so convert it to sRGB using SKBitmap pixel copy with color-space conversion.

**Recommended proposal:** Fix platform extensions to specify destination color space

**Why:** Fixing it at the extension layer corrects the issue for all callers, not just SKBitmapImageSource. This is also where the analogous ToBitmap(SKImage) already shows a TODO comment about color type handling.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Bug is confirmed by code investigation. Root cause is clear but the fix touches multiple platform extension files. Repro project exists but is external — a minimal inline repro would help confirm the fix. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/SkiaSharp.Views, platform and tenet labels | labels=type/bug, area/SkiaSharp.Views, os/Android, os/Windows-Classic, os/Windows-WinUI, os/iOS, os/macOS, tenet/reliability, tenet/compatibility |
| add-comment | medium | 0.88 (88%) | Acknowledge the bug and confirm root cause in platform extension methods | — |
| link-related | low | 0.95 (95%) | Cross-reference related color-space issue #2354 | linkedIssue=#2354 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and the external repro project!

After investigating the source code, this is a confirmed bug. The root cause exists in multiple platform extension methods:

- **WPF** (`WPFExtensions.ToWriteableBitmap`): The destination `SKImageInfo` is created without a color space (`new SKImageInfo(width, height)`) — Skia treats `null` color space as legacy mode and skips color-space conversion from the source bitmap's linear/non-sRGB space.
- **Android** (`AndroidExtensions.ToBitmap`): Same issue — the destination image info has no color space, so the linear-to-sRGB gamma conversion is skipped.
- **Apple/iOS/macOS** (`AppleExtensions.ToCGImage(SKBitmap)`): Raw pixel bytes are copied with hardcoded `CGColorSpace.CreateDeviceRGB()` and `CGBitmapFlags.PremultipliedLast | ByteOrder32Big`, ignoring the bitmap's actual color type and color space — hence only Bgra8888/sRGB works correctly.

The related issue #2354 (`SKBitmap.GetPixel` ignoring color space) is a separate but connected gap.

**Workaround** (already identified by you): Convert the bitmap to sRGB with platform color type and premultiplied alpha before assigning to `SKBitmapImageSource`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2466,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T23:51:00Z"
  },
  "summary": "SKBitmapImageSource displays incorrect colors when the source SKBitmap has a non-sRGB color space (e.g. linear), because the platform-specific conversion extensions (ToBitmap/ToCGImage/ToWriteableBitmap) ignore the bitmap's color space and copy raw pixels without color-space transformation.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.85
    },
    "platforms": [
      "os/Android",
      "os/Windows-Classic",
      "os/Windows-WinUI",
      "os/iOS",
      "os/macOS"
    ],
    "tenets": [
      "tenet/reliability",
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "Colors appear much darker or wrong when bitmap uses linear or non-sRGB color space",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net7.0-android",
        "net7.0-ios",
        "net7.0-maccatalyst",
        "net7.0-windows"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKBitmap with a non-sRGB color space (e.g. SKColorSpace.CreateSrgbLinear())",
        "Use it as SKBitmapImageSource in a MAUI Image control (or WPF Image)",
        "Observe that the displayed image is darker or has wrong colors compared to an sRGB bitmap with the same visual content"
      ],
      "codeSnippets": [
        "var myImageSource = new SKBitmapImageSource { Bitmap = myNonSrgbBitmap };"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, .NET 7.0.302; tested on MAUI/Windows 11, WPF/Windows 11, MAUI/Android, MAUI/macOS, MAUI/iOS",
      "screenshots": [
        {
          "url": "https://github.com/mono/SkiaSharp/assets/27336165/978e55ed-7f16-4fd2-9fae-439db49d29e5",
          "description": "Expected result, regardless of color space"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/assets/27336165/b2fbfdc0-9d06-4618-83e0-1f7973abb86c",
          "description": "Using linear color space - appears much darker"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/assets/27336165/426814aa-193b-427d-8254-a631283c6e60",
          "description": "Android specific: Argb4444 color type wrong even with sRGB color space"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/assets/27336165/0c445b83-e0d6-4aa3-bfb7-c33f389963f1",
          "description": "macOS/iOS: only Bgra8888 sRGB is displayed correctly"
        }
      ],
      "repoLinks": [
        {
          "url": "https://github.com/koszeggy/KGySoft.Drawing/tree/master/Examples/SkiaSharp_(Maui)",
          "description": "External MAUI repro project — must disable the workaround in MainViewModel.cs:362"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2354",
          "description": "Related: SKBitmap.GetPixel also ignores color space (same reporter)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The platform extension methods (ToCGImage, ToBitmap, ToWriteableBitmap) still do not perform color-space conversion — code was reviewed and confirms the bug persists."
    }
  },
  "analysis": {
    "summary": "The platform conversion methods (ToCGImage on Apple, ToBitmap on Android, ToWriteableBitmap on WPF) all copy raw pixel bytes from the SKBitmap without performing color-space conversion to sRGB. On WPF/Android, they create a destination SKImageInfo with null color space (legacy mode), which causes Skia's ReadPixels to skip color-space transformation. On Apple, ToCGImage(SKBitmap) hardcodes CGColorSpace.CreateDeviceRGB() and CGBitmapFlags.PremultipliedLast|ByteOrder32Big regardless of the bitmap's actual color type and color space, causing raw bytes to be misinterpreted.",
    "rationale": "This is unambiguously a bug: the reporter provides screenshots, an external repro project, and a working workaround. Code investigation confirms the platform extension methods ignore the source bitmap's color space. The related issue #2354 (GetPixel ignores color space) is a separate but related problem by the same reporter.",
    "keySignals": [
      {
        "text": "only sRGB color space is displayed correctly so results with linear actual color space must be converted to sRGB",
        "source": "issue body (workaround comment)",
        "interpretation": "Workaround already exists — confirms the root cause is missing color-space conversion in the conversion pipeline."
      },
      {
        "text": "Using linear color space the result appears much darker",
        "source": "issue body",
        "interpretation": "Linear-to-sRGB conversion missing; linear values appear darker because the display assumes sRGB gamma."
      },
      {
        "text": "macOS/iOS: Everything but Bgra8888 sRGB is displayed incorrectly",
        "source": "issue body",
        "interpretation": "ToCGImage hardcodes BGRA format flags, misinterpreting pixels of other color types."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/AppleExtensions.cs",
        "lines": "165-184",
        "finding": "ToCGImage(SKBitmap) reads raw pixel bytes and hardcodes CGColorSpace.CreateDeviceRGB() with CGBitmapFlags.PremultipliedLast|ByteOrder32Big, ignoring the actual color type and color space of the input bitmap.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/AndroidExtensions.cs",
        "lines": "132-138",
        "finding": "ToBitmap(SKBitmap) delegates to PeekPixels().ToBitmap() which wraps the pixmap in SKImage.FromPixels then calls ToBitmap(SKImage). The destination SKImageInfo is created with new SKImageInfo(width, height) — no color space — causing Skia to treat the destination as legacy sRGB without performing color-space conversion.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/WPFExtensions.cs",
        "lines": "66-83",
        "finding": "ToWriteableBitmap(SKImage) creates a destination SKImageInfo with no color space (null), then calls skiaImage.ReadPixels(pixmap). Null color space means Skia uses legacy mode and skips color-space conversion from the source's linear/non-sRGB space.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKImageSourceService/SKImageSourceService.Android.cs",
        "lines": "14-26",
        "finding": "SKImageSourceService for Android calls bmp.Bitmap?.ToBitmap() — delegating to the extension method that ignores color space.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Convert the bitmap to sRGB before assigning to SKBitmapImageSource: create a new SKBitmap with SKImageInfo using SKColorSpace.CreateSrgb(), draw/copy pixels with color space conversion, then use that bitmap as the image source.",
      "On WPF/Windows and Android: manually ensure the SKBitmap uses SKImageInfo.PlatformColorType and SKColorSpace.CreateSrgb() before passing to SKBitmapImageSource.",
      "On macOS/iOS: additionally ensure the color type is Bgra8888 (SKImageInfo.PlatformColorType) with Premul alpha type and sRGB color space."
    ],
    "nextQuestions": [
      "Does SKImageSourceService.Tizen.cs have the same issue?",
      "Should the fix be in the platform extension methods (ToCGImage, ToBitmap, ToWriteableBitmap) to always convert to sRGB/platform format, or should SKBitmapImageSource normalise the bitmap before calling the extension?",
      "Is there a performance concern with always converting — should conversion be conditional on color space?"
    ],
    "resolution": {
      "hypothesis": "All platform conversion methods need to specify a sRGB destination color space so Skia performs color-space conversion during ReadPixels, and the Apple ToCGImage needs to convert to a supported format first.",
      "proposals": [
        {
          "title": "Fix platform extensions to specify destination color space",
          "description": "In WPF ToWriteableBitmap and Android ToBitmap, set the destination SKImageInfo's ColorSpace to SKColorSpace.CreateSrgb() so Skia performs color-space conversion. For Apple ToCGImage(SKBitmap), first convert the bitmap to an SKImage with a standard format (Bgra8888/Premul/sRGB) using SKBitmap.FromImage or SKImage.FromBitmap with a color-space-aware destination, then create the CGImage.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Normalize bitmap in SKBitmapImageSource",
          "description": "In SKImageSourceService, before delegating to the platform extension, check if the bitmap's color space differs from sRGB and if so convert it to sRGB using SKBitmap pixel copy with color-space conversion.",
          "category": "workaround",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Fix platform extensions to specify destination color space",
      "recommendedReason": "Fixing it at the extension layer corrects the issue for all callers, not just SKBitmapImageSource. This is also where the analogous ToBitmap(SKImage) already shows a TODO comment about color type handling."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Bug is confirmed by code investigation. Root cause is clear but the fix touches multiple platform extension files. Repro project exists but is external — a minimal inline repro would help confirm the fix.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views, platform and tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Android",
          "os/Windows-Classic",
          "os/Windows-WinUI",
          "os/iOS",
          "os/macOS",
          "tenet/reliability",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the bug and confirm root cause in platform extension methods",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report and the external repro project!\n\nAfter investigating the source code, this is a confirmed bug. The root cause exists in multiple platform extension methods:\n\n- **WPF** (`WPFExtensions.ToWriteableBitmap`): The destination `SKImageInfo` is created without a color space (`new SKImageInfo(width, height)`) — Skia treats `null` color space as legacy mode and skips color-space conversion from the source bitmap's linear/non-sRGB space.\n- **Android** (`AndroidExtensions.ToBitmap`): Same issue — the destination image info has no color space, so the linear-to-sRGB gamma conversion is skipped.\n- **Apple/iOS/macOS** (`AppleExtensions.ToCGImage(SKBitmap)`): Raw pixel bytes are copied with hardcoded `CGColorSpace.CreateDeviceRGB()` and `CGBitmapFlags.PremultipliedLast | ByteOrder32Big`, ignoring the bitmap's actual color type and color space — hence only Bgra8888/sRGB works correctly.\n\nThe related issue #2354 (`SKBitmap.GetPixel` ignoring color space) is a separate but connected gap.\n\n**Workaround** (already identified by you): Convert the bitmap to sRGB with platform color type and premultiplied alpha before assigning to `SKBitmapImageSource`."
      },
      {
        "type": "link-related",
        "description": "Cross-reference related color-space issue #2354",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 2354
      }
    ]
  }
}
```

</details>
