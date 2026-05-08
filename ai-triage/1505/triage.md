# Issue Triage Report — #1505

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T15:50:00Z |
| Type | type/feature-request (0.90 (90%)) |
| Area | area/libSkiaSharp.native (0.92 (92%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** Feature request to support FreeType font rendering on Windows in SkiaSharp's native library, allowing cross-platform consistent text rendering instead of the hardcoded DirectWrite backend.

**Analysis:** The Windows native build of SkiaSharp uses Skia's DirectWrite font manager by default and does not expose a way to switch to FreeType at runtime. The request is to either expose a runtime switch or ship a separate Windows FreeType variant. The maintainer was receptive in 2020 and suggested the separate-package approach (similar to nanoserver), but no PR was ever submitted.

**Recommendations:** **keep-open** — Valid feature request with maintainer buy-in since 2020 and a clear implementation path (new build variant following nanoserver pattern). No PR was submitted despite offers, so it remains open for a willing contributor.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/libSkiaSharp.native |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | os/Windows-Classic, area/libSkiaSharp.native, type/feature-request, triage/triaged |

## Evidence

### Reproduction

**Environment:** Windows, SkiaSharp (version unspecified, circa 2020), DirectWrite is the default Windows font backend in Skia

**Repository links:**
- https://github.com/kyamagu/skia-python/blob/e0b030c14e33f70880cfcc502eaeed898a77fc3c/src/skia/Font.cpp#L709 — skia-python exposes SkFontMgr_New_Custom_Empty as an alternative to the DirectWrite default, referenced by commenter

## Analysis

### Technical Summary

The Windows native build of SkiaSharp uses Skia's DirectWrite font manager by default and does not expose a way to switch to FreeType at runtime. The request is to either expose a runtime switch or ship a separate Windows FreeType variant. The maintainer was receptive in 2020 and suggested the separate-package approach (similar to nanoserver), but no PR was ever submitted.

### Rationale

Title says [QUESTION] but body is clearly a feature request — reporter wants a new capability (FreeType on Windows). Existing labels already correctly classify as type/feature-request. Code investigation confirms no FreeType support is compiled into the Windows build and no C API exposes SkFontMgr_New_Custom_Empty. The nanoserver variant shows a precedent for alternate Windows build variants that could be adapted. Maintainer buy-in exists but implementation never materialised.

### Key Signals

- "I would like to be able to configure it (at runtime) to use Freetype instead. Would a PR be accepted for doing so?" — **issue body** (Clear feature request with contributor offering to implement; not a usage question.)
- "A separate maintained package is perfect! Yes, I can provide a PR." — **comment #8 (ziriax)** (Reporter confirmed willingness to contribute; maintainer preferred option 1 (separate package).)
- "If you can make the or add a variant or some args in the cake, then that should do it very well. Exactly like we do with Linux. In fact, nano server just calls into the main windows cake." — **comment #9 (mattleibow)** (Maintainer approved the nanoserver-style variant approach as the implementation strategy.)
- "Being able to choose the font host implementation (DirectWrite, Freetype, X) at runtime would give us the flexibility we need." — **comment #16 (Gillibald)** (Avalonia team has a concrete use case for runtime font backend selection.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `native/windows/build.cake` | 14-32 | direct | Windows Build() function sets skia_enable_fontmgr_win_gdi=false but does NOT set skia_use_freetype=true. This means the Windows native build uses DirectWrite font manager exclusively — confirming the feature is absent. |
| `native/linux/build.cake` | 100-121 | direct | Linux build sets skia_use_system_freetype2=false (uses bundled FreeType, not system) — FreeType is already compiled in on Linux. The Windows build lacks an equivalent flag, confirming the platform gap. |
| `native/nanoserver/build.cake` | 7-17 | related | NanoServer variant delegates to windows/build.cake via RunCake() with extra GN args (supportDirect3D=false, extra_cflags). This is the exact pattern the maintainer proposed for a Windows-FreeType variant. |
| `native/wasm/build.cake` | 46 | related | WASM build explicitly sets skia_use_freetype=true, confirming FreeType can be enabled as a GN flag in SkiaSharp's build system. |

### Workarounds

- Build a custom libSkiaSharp.dll with FreeType enabled using the GN flags: skia_use_freetype=true plus appropriate font manager flags, and substitute it into the NuGet package manually.
- On Windows, load typefaces explicitly via SKTypeface.FromFile() or SKTypeface.FromData() to avoid the system font discovery path entirely — partial workaround that avoids DirectWrite for font loading but not rasterization.

### Next Questions

- Has anyone attempted the separate Windows-FreeType variant since 2020 and hit blockers?
- What GN flags exactly are needed to disable DirectWrite and enable FreeType on Windows (skia_enable_fontmgr_win, skia_use_freetype)?
- Is SkFontMgr_New_Custom_Empty already available via the C API or would it need a new wrapper in externals/skia/src/c/?
- Should the feature ship as a new NuGet package variant (e.g. SkiaSharp.NativeAssets.Win32.FreeType) or as a runtime build argument?

### Resolution Proposals

**Hypothesis:** A new native Windows build variant using FreeType instead of DirectWrite can be added as a separate libSkiaSharp package, following the nanoserver pattern, requiring only GN flag changes and a new build.cake variant.

1. **New Windows-FreeType native build variant** — fix, confidence 0.80 (80%), cost/m, validated=untested
   - Add a native/windows-freetype/build.cake that calls ../windows/build.cake via RunCake() with skia_use_freetype=true and skia_enable_fontmgr_win=false GN args. Publish as a separate NuGet package (e.g. SkiaSharp.NativeAssets.Win32.FreeType) following the nanoserver precedent.
2. **Expose SkFontMgr_New_Custom_Empty via C API** — alternative, confidence 0.65 (65%), cost/l, validated=untested
   - Add a C API wrapper sk_fontmgr_new_custom_empty() in externals/skia/src/c/ and expose it in C# as SKFontManager.CreateEmpty(). This allows callers to opt into a FreeType-based empty manager at runtime without a separate package, but requires FreeType to be compiled into the Windows binary.

**Recommended proposal:** New Windows-FreeType native build variant

**Why:** Matches the pattern already established by nanoserver, requires no C API changes, doesn't break existing Windows behavior, and ships as an opt-in package.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | Valid feature request with maintainer buy-in since 2020 and a clear implementation path (new build variant following nanoserver pattern). No PR was submitted despite offers, so it remains open for a willing contributor. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply feature-request, libSkiaSharp.native, and Windows-Classic labels | labels=type/feature-request, area/libSkiaSharp.native, os/Windows-Classic |
| add-comment | medium | 0.85 (85%) | Summarize the implementation path and invite a contributor to open a PR | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed discussion on this! The implementation path agreed back in 2020 is still valid:

**Approach: New Windows-FreeType build variant (following the nanoserver pattern)**

1. Add `native/windows-freetype/build.cake` that calls `../windows/build.cake` via `RunCake()` with extra GN args:
   - `skia_use_freetype=true`
   - `skia_enable_fontmgr_win=false` (disable DirectWrite)
2. Publish as a separate NuGet package so existing Windows behavior is unaffected.

The WASM build already uses `skia_use_freetype=true` and the nanoserver variant shows the delegate-to-windows-cake pattern.

If anyone is willing to open a PR, this is a good first step to get the conversation going again!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1505,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T15:50:00Z",
    "currentLabels": [
      "os/Windows-Classic",
      "area/libSkiaSharp.native",
      "type/feature-request",
      "triage/triaged"
    ]
  },
  "summary": "Feature request to support FreeType font rendering on Windows in SkiaSharp's native library, allowing cross-platform consistent text rendering instead of the hardcoded DirectWrite backend.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.9
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.92
    },
    "platforms": [
      "os/Windows-Classic"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Windows, SkiaSharp (version unspecified, circa 2020), DirectWrite is the default Windows font backend in Skia",
      "repoLinks": [
        {
          "url": "https://github.com/kyamagu/skia-python/blob/e0b030c14e33f70880cfcc502eaeed898a77fc3c/src/skia/Font.cpp#L709",
          "description": "skia-python exposes SkFontMgr_New_Custom_Empty as an alternative to the DirectWrite default, referenced by commenter"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The Windows native build of SkiaSharp uses Skia's DirectWrite font manager by default and does not expose a way to switch to FreeType at runtime. The request is to either expose a runtime switch or ship a separate Windows FreeType variant. The maintainer was receptive in 2020 and suggested the separate-package approach (similar to nanoserver), but no PR was ever submitted.",
    "rationale": "Title says [QUESTION] but body is clearly a feature request — reporter wants a new capability (FreeType on Windows). Existing labels already correctly classify as type/feature-request. Code investigation confirms no FreeType support is compiled into the Windows build and no C API exposes SkFontMgr_New_Custom_Empty. The nanoserver variant shows a precedent for alternate Windows build variants that could be adapted. Maintainer buy-in exists but implementation never materialised.",
    "keySignals": [
      {
        "text": "I would like to be able to configure it (at runtime) to use Freetype instead. Would a PR be accepted for doing so?",
        "source": "issue body",
        "interpretation": "Clear feature request with contributor offering to implement; not a usage question."
      },
      {
        "text": "A separate maintained package is perfect! Yes, I can provide a PR.",
        "source": "comment #8 (ziriax)",
        "interpretation": "Reporter confirmed willingness to contribute; maintainer preferred option 1 (separate package)."
      },
      {
        "text": "If you can make the or add a variant or some args in the cake, then that should do it very well. Exactly like we do with Linux. In fact, nano server just calls into the main windows cake.",
        "source": "comment #9 (mattleibow)",
        "interpretation": "Maintainer approved the nanoserver-style variant approach as the implementation strategy."
      },
      {
        "text": "Being able to choose the font host implementation (DirectWrite, Freetype, X) at runtime would give us the flexibility we need.",
        "source": "comment #16 (Gillibald)",
        "interpretation": "Avalonia team has a concrete use case for runtime font backend selection."
      }
    ],
    "codeInvestigation": [
      {
        "file": "native/windows/build.cake",
        "lines": "14-32",
        "finding": "Windows Build() function sets skia_enable_fontmgr_win_gdi=false but does NOT set skia_use_freetype=true. This means the Windows native build uses DirectWrite font manager exclusively — confirming the feature is absent.",
        "relevance": "direct"
      },
      {
        "file": "native/linux/build.cake",
        "lines": "100-121",
        "finding": "Linux build sets skia_use_system_freetype2=false (uses bundled FreeType, not system) — FreeType is already compiled in on Linux. The Windows build lacks an equivalent flag, confirming the platform gap.",
        "relevance": "direct"
      },
      {
        "file": "native/nanoserver/build.cake",
        "lines": "7-17",
        "finding": "NanoServer variant delegates to windows/build.cake via RunCake() with extra GN args (supportDirect3D=false, extra_cflags). This is the exact pattern the maintainer proposed for a Windows-FreeType variant.",
        "relevance": "related"
      },
      {
        "file": "native/wasm/build.cake",
        "lines": "46",
        "finding": "WASM build explicitly sets skia_use_freetype=true, confirming FreeType can be enabled as a GN flag in SkiaSharp's build system.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Build a custom libSkiaSharp.dll with FreeType enabled using the GN flags: skia_use_freetype=true plus appropriate font manager flags, and substitute it into the NuGet package manually.",
      "On Windows, load typefaces explicitly via SKTypeface.FromFile() or SKTypeface.FromData() to avoid the system font discovery path entirely — partial workaround that avoids DirectWrite for font loading but not rasterization."
    ],
    "nextQuestions": [
      "Has anyone attempted the separate Windows-FreeType variant since 2020 and hit blockers?",
      "What GN flags exactly are needed to disable DirectWrite and enable FreeType on Windows (skia_enable_fontmgr_win, skia_use_freetype)?",
      "Is SkFontMgr_New_Custom_Empty already available via the C API or would it need a new wrapper in externals/skia/src/c/?",
      "Should the feature ship as a new NuGet package variant (e.g. SkiaSharp.NativeAssets.Win32.FreeType) or as a runtime build argument?"
    ],
    "resolution": {
      "hypothesis": "A new native Windows build variant using FreeType instead of DirectWrite can be added as a separate libSkiaSharp package, following the nanoserver pattern, requiring only GN flag changes and a new build.cake variant.",
      "proposals": [
        {
          "title": "New Windows-FreeType native build variant",
          "description": "Add a native/windows-freetype/build.cake that calls ../windows/build.cake via RunCake() with skia_use_freetype=true and skia_enable_fontmgr_win=false GN args. Publish as a separate NuGet package (e.g. SkiaSharp.NativeAssets.Win32.FreeType) following the nanoserver precedent.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Expose SkFontMgr_New_Custom_Empty via C API",
          "description": "Add a C API wrapper sk_fontmgr_new_custom_empty() in externals/skia/src/c/ and expose it in C# as SKFontManager.CreateEmpty(). This allows callers to opt into a FreeType-based empty manager at runtime without a separate package, but requires FreeType to be compiled into the Windows binary.",
          "category": "alternative",
          "confidence": 0.65,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "New Windows-FreeType native build variant",
      "recommendedReason": "Matches the pattern already established by nanoserver, requires no C API changes, doesn't break existing Windows behavior, and ships as an opt-in package."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "Valid feature request with maintainer buy-in since 2020 and a clear implementation path (new build variant following nanoserver pattern). No PR was submitted despite offers, so it remains open for a willing contributor.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, libSkiaSharp.native, and Windows-Classic labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/feature-request",
          "area/libSkiaSharp.native",
          "os/Windows-Classic"
        ]
      },
      {
        "type": "add-comment",
        "description": "Summarize the implementation path and invite a contributor to open a PR",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed discussion on this! The implementation path agreed back in 2020 is still valid:\n\n**Approach: New Windows-FreeType build variant (following the nanoserver pattern)**\n\n1. Add `native/windows-freetype/build.cake` that calls `../windows/build.cake` via `RunCake()` with extra GN args:\n   - `skia_use_freetype=true`\n   - `skia_enable_fontmgr_win=false` (disable DirectWrite)\n2. Publish as a separate NuGet package so existing Windows behavior is unaffected.\n\nThe WASM build already uses `skia_use_freetype=true` and the nanoserver variant shows the delegate-to-windows-cake pattern.\n\nIf anyone is willing to open a PR, this is a good first step to get the conversation going again!"
      }
    ]
  }
}
```

</details>
