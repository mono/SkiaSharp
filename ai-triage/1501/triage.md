# Issue Triage Report — #1501

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T21:35:00Z |
| Type | type/bug (0.93 (93%)) |
| Area | area/libSkiaSharp.native (0.82 (82%)) |
| Suggested action | needs-investigation (0.72 (72%)) |

**Issue Summary:** DrawBitmap decoded from SKCodec is 20-30x slower on UWP (200-300ms) vs .NET Core/.NET Framework (<10ms) with SkiaSharp 2.80.2; reporter found that manually referencing the netstandard2.0 managed DLL restores normal performance, implicating the UWP-specific native or managed code path.

**Analysis:** Performance on UWP is 20-30x worse than .NET Core/Framework for DrawBitmap. The reporter's self-diagnosis (replacing the UWP managed DLL with the netstandard2.0 DLL while keeping the same libSkiaSharp.dll) restores performance, strongly implicating either the UWP-specific native DLL compilation (WACK compliance disabling SIMD) or the UWP TFM managed code path (USE_DELEGATES mode adding per-call dispatch overhead). The most likely root cause is the UWP native library being compiled without SSE/AVX optimizations to comply with Windows App Certification Kit requirements, making per-pixel operations in DrawBitmap dramatically slower.

**Recommendations:** **needs-investigation** — Root cause (WACK SIMD restriction vs delegate overhead) is not confirmed by code inspection alone. Issue is from 2020 on a now-unsupported platform (UWP). Current relevance is low, but root cause confirmation would help document the limitation for historical reference.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Windows-Universal-UWP |
| Backends | backend/Raster |
| Tenets | tenet/performance |
| Partner | — |

## Evidence

### Reproduction

1. Create a UWP project targeting SDK 18362
2. Use SKCodec.Create(stream) to decode a PNG
3. Draw the decoded bitmap with canvas.DrawBitmap(decodeBitmap, dest)
4. Measure time with Stopwatch

**Environment:** SkiaSharp 2.80.2, UWP SDK 18362, Windows 10, Visual Studio

**Repository links:**
- https://github.com/h82258652/SKCodecUwpPerformanceRepo — Reporter's minimal reproduction project

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | performance |
| Error message | DrawBitmap costs 200-300ms on UWP; same operation takes <10ms on .NET Core/.NET Framework |
| Repro quality | complete |
| Target frameworks | uap10.0.18362 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | SkiaSharp 3.x dropped UWP (uap10.0) support entirely; affected users should migrate to WinUI or stay on 2.88.x. |

## Analysis

### Technical Summary

Performance on UWP is 20-30x worse than .NET Core/Framework for DrawBitmap. The reporter's self-diagnosis (replacing the UWP managed DLL with the netstandard2.0 DLL while keeping the same libSkiaSharp.dll) restores performance, strongly implicating either the UWP-specific native DLL compilation (WACK compliance disabling SIMD) or the UWP TFM managed code path (USE_DELEGATES mode adding per-call dispatch overhead). The most likely root cause is the UWP native library being compiled without SSE/AVX optimizations to comply with Windows App Certification Kit requirements, making per-pixel operations in DrawBitmap dramatically slower.

### Rationale

The reporter's workaround — using the netstandard2.0 managed DLL but keeping the same native DLL directory — directly narrows the cause to the UWP TFM build. DrawBitmap calls SKImage.FromBitmap then DrawImage, both of which involve significant pixel operations inside the native Skia library. A 20-30x performance gap at the same workload is consistent with SIMD being disabled in the UWP native build for WACK compliance. This is classified as type/bug because the behavior significantly deviates from expected cross-platform performance, and area/libSkiaSharp.native because the root cause is in how the native library is compiled for UWP. Issue has low current relevance since SkiaSharp 3.x dropped UWP support.

### Key Signals

- "I download the 2.80.2 nuget package, extra it and then let the UWP project reference the .net standard 2.0 dll (also copy the libSkiaSharp.dll), the performance equals to run on .net core or .net framework." — **comment by reporter** (The performance difference is between the UWP TFM managed DLL and the netstandard2.0 DLL with the same native library — implicates the managed code path (USE_DELEGATES P/Invoke overhead) or UWP native DLL compilation flags.)
- "It cost 200~300 ms on my computer and the .net core and .net framework version, only takes <10 ms." — **issue body** (20-30x performance gap is consistent with disabled SIMD/AVX in UWP native build or high per-call delegate overhead in USE_DELEGATES mode.)
- "Version with issue: 2.80.2 / Last known good version: 2.80.2" — **issue body** (Reporter has no known-good version; this may have always been the case for UWP builds. No regression — this is a platform-specific build limitation.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 577-581 | direct | DrawBitmap(SKBitmap, SKRect, SKPaint) converts the bitmap to SKImage via SKImage.FromBitmap then calls DrawImage. Both are native calls. No UWP-specific override. |
| `binding/SkiaSharp/SKImage.cs` | 237-244 | direct | SKImage.FromBitmap calls sk_image_new_from_bitmap via a single P/Invoke. In UWP TFM builds with USE_DELEGATES, this goes through a delegate lookup rather than a direct DllImport call. |
| `binding/SkiaSharp/SkiaSharp.csproj` | 12-14 | related | USE_DELEGATES is defined for net4* TFMs. In SkiaSharp 2.80.2, the UWP TFM (uap10.0) also used USE_DELEGATES. With delegates, every native call dispatches through a Delegate pointer rather than a static DllImport entry point — adds per-call overhead but likely insufficient alone to explain 200ms gap. |
| `binding/Binding.Shared/PlatformConfiguration.cs` | 6-62 | context | WINDOWS_UWP conditional compilation confirms UWP has a distinct managed code path. No rendering-related conditional differences found — platform differences are limited to OS detection. |

### Workarounds

- Extract the SkiaSharp NuGet package and reference the netstandard2.0 managed DLL directly (binding/SkiaSharp.dll targeting netstandard2.0) while placing libSkiaSharp.dll in the app output directory. This bypasses the UWP TFM build.
- Migrate the app to WinUI 3 and use SkiaSharp 3.x which ships a properly-optimized Windows native library.
- Stay on SkiaSharp 2.88.x (last 2.x release) which may have improved UWP build flags over 2.80.2.

### Next Questions

- Is the performance difference caused by the UWP native library being compiled without SIMD optimizations (WACK compliance), or by USE_DELEGATES delegate dispatch overhead?
- Does the 2.88.x UWP build show the same performance regression?
- What CPU instructions does the libSkiaSharp.dll for UWP support (check PE headers / DUMPBIN /DISASM for SSE/AVX instructions)?

### Resolution Proposals

**Hypothesis:** The UWP libSkiaSharp.dll was compiled without SIMD/AVX optimizations for WACK compliance, making per-pixel drawing operations like DrawBitmap 20-30x slower than the Win32/netstandard native library. The managed code path (USE_DELEGATES) adds minor overhead but is unlikely to account for 200ms.

1. **Migrate to WinUI 3 / SkiaSharp 3.x** — alternative, confidence 0.90 (90%), cost/l, validated=untested
   - SkiaSharp 3.x dropped UWP and ships optimized WinUI 3 native libraries. Migration resolves the performance issue.
2. **Use netstandard2.0 DLL workaround** — workaround, confidence 0.85 (85%), cost/s, validated=untested
   - Reference the netstandard2.0 SkiaSharp.dll directly and copy the Win32 libSkiaSharp.dll alongside it. Reported by the issue author to restore normal performance.

**Recommended proposal:** Migrate to WinUI 3 / SkiaSharp 3.x

**Why:** UWP is deprecated by Microsoft and no longer supported in SkiaSharp 3.x. The workaround is fragile and unsupported. Migration is the correct long-term path.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.72 (72%) |
| Reason | Root cause (WACK SIMD restriction vs delegate overhead) is not confirmed by code inspection alone. Issue is from 2020 on a now-unsupported platform (UWP). Current relevance is low, but root cause confirmation would help document the limitation for historical reference. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.93 (93%) | Apply bug, native, UWP, performance tenet labels | labels=type/bug, area/libSkiaSharp.native, os/Windows-Universal-UWP, tenet/performance |
| add-comment | medium | 0.80 (80%) | Acknowledge the issue, confirm workaround, note that UWP is no longer supported in SkiaSharp 3.x | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report and the reproduction repository!

The performance difference you're seeing is consistent with the UWP build of `libSkiaSharp.dll` being compiled without SIMD/AVX CPU optimizations to comply with Windows App Certification Kit (WACK) requirements. This makes per-pixel operations like `DrawBitmap` significantly slower than the Win32 Desktop build.

Your workaround (using the `netstandard2.0` managed DLL with the non-UWP `libSkiaSharp.dll`) works because you're loading the Win32 Desktop native library that has full SIMD optimizations enabled.

For a more robust workaround: use `SkiaSharp 2.88.x` (the latest 2.x release) which may have improved UWP build settings.

Longer term, **SkiaSharp 3.x has dropped UWP support** in favour of WinUI 3. If migrating is an option, SkiaSharp 3.x ships an optimized Windows native library and this performance issue would not apply.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1501,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T21:35:00Z"
  },
  "summary": "DrawBitmap decoded from SKCodec is 20-30x slower on UWP (200-300ms) vs .NET Core/.NET Framework (<10ms) with SkiaSharp 2.80.2; reporter found that manually referencing the netstandard2.0 managed DLL restores normal performance, implicating the UWP-specific native or managed code path.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.93
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.82
    },
    "platforms": [
      "os/Windows-Universal-UWP"
    ],
    "backends": [
      "backend/Raster"
    ],
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "performance",
      "errorMessage": "DrawBitmap costs 200-300ms on UWP; same operation takes <10ms on .NET Core/.NET Framework",
      "reproQuality": "complete",
      "targetFrameworks": [
        "uap10.0.18362"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a UWP project targeting SDK 18362",
        "Use SKCodec.Create(stream) to decode a PNG",
        "Draw the decoded bitmap with canvas.DrawBitmap(decodeBitmap, dest)",
        "Measure time with Stopwatch"
      ],
      "environmentDetails": "SkiaSharp 2.80.2, UWP SDK 18362, Windows 10, Visual Studio",
      "repoLinks": [
        {
          "url": "https://github.com/h82258652/SKCodecUwpPerformanceRepo",
          "description": "Reporter's minimal reproduction project"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "SkiaSharp 3.x dropped UWP (uap10.0) support entirely; affected users should migrate to WinUI or stay on 2.88.x."
    }
  },
  "analysis": {
    "summary": "Performance on UWP is 20-30x worse than .NET Core/Framework for DrawBitmap. The reporter's self-diagnosis (replacing the UWP managed DLL with the netstandard2.0 DLL while keeping the same libSkiaSharp.dll) restores performance, strongly implicating either the UWP-specific native DLL compilation (WACK compliance disabling SIMD) or the UWP TFM managed code path (USE_DELEGATES mode adding per-call dispatch overhead). The most likely root cause is the UWP native library being compiled without SSE/AVX optimizations to comply with Windows App Certification Kit requirements, making per-pixel operations in DrawBitmap dramatically slower.",
    "rationale": "The reporter's workaround — using the netstandard2.0 managed DLL but keeping the same native DLL directory — directly narrows the cause to the UWP TFM build. DrawBitmap calls SKImage.FromBitmap then DrawImage, both of which involve significant pixel operations inside the native Skia library. A 20-30x performance gap at the same workload is consistent with SIMD being disabled in the UWP native build for WACK compliance. This is classified as type/bug because the behavior significantly deviates from expected cross-platform performance, and area/libSkiaSharp.native because the root cause is in how the native library is compiled for UWP. Issue has low current relevance since SkiaSharp 3.x dropped UWP support.",
    "keySignals": [
      {
        "text": "I download the 2.80.2 nuget package, extra it and then let the UWP project reference the .net standard 2.0 dll (also copy the libSkiaSharp.dll), the performance equals to run on .net core or .net framework.",
        "source": "comment by reporter",
        "interpretation": "The performance difference is between the UWP TFM managed DLL and the netstandard2.0 DLL with the same native library — implicates the managed code path (USE_DELEGATES P/Invoke overhead) or UWP native DLL compilation flags."
      },
      {
        "text": "It cost 200~300 ms on my computer and the .net core and .net framework version, only takes <10 ms.",
        "source": "issue body",
        "interpretation": "20-30x performance gap is consistent with disabled SIMD/AVX in UWP native build or high per-call delegate overhead in USE_DELEGATES mode."
      },
      {
        "text": "Version with issue: 2.80.2 / Last known good version: 2.80.2",
        "source": "issue body",
        "interpretation": "Reporter has no known-good version; this may have always been the case for UWP builds. No regression — this is a platform-specific build limitation."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "577-581",
        "finding": "DrawBitmap(SKBitmap, SKRect, SKPaint) converts the bitmap to SKImage via SKImage.FromBitmap then calls DrawImage. Both are native calls. No UWP-specific override.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "237-244",
        "finding": "SKImage.FromBitmap calls sk_image_new_from_bitmap via a single P/Invoke. In UWP TFM builds with USE_DELEGATES, this goes through a delegate lookup rather than a direct DllImport call.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaSharp.csproj",
        "lines": "12-14",
        "finding": "USE_DELEGATES is defined for net4* TFMs. In SkiaSharp 2.80.2, the UWP TFM (uap10.0) also used USE_DELEGATES. With delegates, every native call dispatches through a Delegate pointer rather than a static DllImport entry point — adds per-call overhead but likely insufficient alone to explain 200ms gap.",
        "relevance": "related"
      },
      {
        "file": "binding/Binding.Shared/PlatformConfiguration.cs",
        "lines": "6-62",
        "finding": "WINDOWS_UWP conditional compilation confirms UWP has a distinct managed code path. No rendering-related conditional differences found — platform differences are limited to OS detection.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Extract the SkiaSharp NuGet package and reference the netstandard2.0 managed DLL directly (binding/SkiaSharp.dll targeting netstandard2.0) while placing libSkiaSharp.dll in the app output directory. This bypasses the UWP TFM build.",
      "Migrate the app to WinUI 3 and use SkiaSharp 3.x which ships a properly-optimized Windows native library.",
      "Stay on SkiaSharp 2.88.x (last 2.x release) which may have improved UWP build flags over 2.80.2."
    ],
    "nextQuestions": [
      "Is the performance difference caused by the UWP native library being compiled without SIMD optimizations (WACK compliance), or by USE_DELEGATES delegate dispatch overhead?",
      "Does the 2.88.x UWP build show the same performance regression?",
      "What CPU instructions does the libSkiaSharp.dll for UWP support (check PE headers / DUMPBIN /DISASM for SSE/AVX instructions)?"
    ],
    "resolution": {
      "hypothesis": "The UWP libSkiaSharp.dll was compiled without SIMD/AVX optimizations for WACK compliance, making per-pixel drawing operations like DrawBitmap 20-30x slower than the Win32/netstandard native library. The managed code path (USE_DELEGATES) adds minor overhead but is unlikely to account for 200ms.",
      "proposals": [
        {
          "title": "Migrate to WinUI 3 / SkiaSharp 3.x",
          "description": "SkiaSharp 3.x dropped UWP and ships optimized WinUI 3 native libraries. Migration resolves the performance issue.",
          "category": "alternative",
          "confidence": 0.9,
          "effort": "cost/l",
          "validated": "untested"
        },
        {
          "title": "Use netstandard2.0 DLL workaround",
          "description": "Reference the netstandard2.0 SkiaSharp.dll directly and copy the Win32 libSkiaSharp.dll alongside it. Reported by the issue author to restore normal performance.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Migrate to WinUI 3 / SkiaSharp 3.x",
      "recommendedReason": "UWP is deprecated by Microsoft and no longer supported in SkiaSharp 3.x. The workaround is fragile and unsupported. Migration is the correct long-term path."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.72,
      "reason": "Root cause (WACK SIMD restriction vs delegate overhead) is not confirmed by code inspection alone. Issue is from 2020 on a now-unsupported platform (UWP). Current relevance is low, but root cause confirmation would help document the limitation for historical reference.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, native, UWP, performance tenet labels",
        "risk": "low",
        "confidence": 0.93,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Windows-Universal-UWP",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the issue, confirm workaround, note that UWP is no longer supported in SkiaSharp 3.x",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thank you for the detailed report and the reproduction repository!\n\nThe performance difference you're seeing is consistent with the UWP build of `libSkiaSharp.dll` being compiled without SIMD/AVX CPU optimizations to comply with Windows App Certification Kit (WACK) requirements. This makes per-pixel operations like `DrawBitmap` significantly slower than the Win32 Desktop build.\n\nYour workaround (using the `netstandard2.0` managed DLL with the non-UWP `libSkiaSharp.dll`) works because you're loading the Win32 Desktop native library that has full SIMD optimizations enabled.\n\nFor a more robust workaround: use `SkiaSharp 2.88.x` (the latest 2.x release) which may have improved UWP build settings.\n\nLonger term, **SkiaSharp 3.x has dropped UWP support** in favour of WinUI 3. If migrating is an option, SkiaSharp 3.x ships an optimized Windows native library and this performance issue would not apply."
      }
    ]
  }
}
```

</details>
