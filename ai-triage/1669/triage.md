# Issue Triage Report — #1669

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T17:57:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/libSkiaSharp.native (0.95 (95%)) |
| Suggested action | needs-investigation (0.87 (87%)) |

**Issue Summary:** Subpixel text rendering (LCD/ClearType AA) is silently ignored on Linux because the bundled freetype2 in mono/skia lacks the harmony subpixel patch or FT_CONFIG_OPTION_SUBPIXEL_RENDERING.

**Analysis:** The bundled freetype2 inside mono/skia does not have the 'harmony subpixel rendering' patch (upstream commit f25787b) and does not have FT_CONFIG_OPTION_SUBPIXEL_RENDERING enabled. When SKFont.Subpixel=true and Edging=SubpixelAntialias are set in C#, the native P/Invoke calls succeed but freetype2 silently falls back to grayscale AA because subpixel mode is not compiled in. Using system freetype2 (skia_use_system_freetype2=true) resolves the issue on the reporter's machine.

**Recommendations:** **needs-investigation** — Root cause is well-identified (missing freetype2 harmony patch in mono/skia) but needs verification against current mono/skia DEPS to confirm if still present before committing to a fix.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Linux |
| Backends | backend/Raster |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1253 — Related: text rendering differences between Chrome/Skia and SkiaSharp
- https://skia.googlesource.com/skia/+/f25787b72c20e97cdeb74e037dc1ff56a88b45c6 — Upstream Skia harmony subpixel patch that mono/skia is missing

**Screenshots:**
- https://user-images.githubusercontent.com/10325838/111989784-83181d80-8b12-11eb-9e11-b64755ee754a.png — Expected output from C++ with subpixel AA enabled
- https://user-images.githubusercontent.com/10325838/111989926-b064cb80-8b12-11eb-8b7e-224a998cd58f.png — Actual output from C# without subpixel AA (grayscale only)

**Code snippets:**

```csharp
var props = new SKSurfaceProperties(SKSurfacePropsFlags.None, SKPixelGeometry.BgrHorizontal);
var surface = SKSurface.Create(imageInfo, props);
var font = new SKFont {Subpixel = true, Edging = SKFontEdging.SubpixelAntialias, Size = 12};
canvas.DrawText("Hello World", 10, 20, font, paint);
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | complete |
| Target frameworks | net5.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.3-preview.40, 1.68.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Issue never had a code fix applied; bundled freetype2 remains unpatched or ClearType flag still disabled. |

## Analysis

### Technical Summary

The bundled freetype2 inside mono/skia does not have the 'harmony subpixel rendering' patch (upstream commit f25787b) and does not have FT_CONFIG_OPTION_SUBPIXEL_RENDERING enabled. When SKFont.Subpixel=true and Edging=SubpixelAntialias are set in C#, the native P/Invoke calls succeed but freetype2 silently falls back to grayscale AA because subpixel mode is not compiled in. Using system freetype2 (skia_use_system_freetype2=true) resolves the issue on the reporter's machine.

### Rationale

This is a native build configuration bug. The C# API correctly passes SKFont.Subpixel and SKFontEdging.SubpixelAntialias to native code, but the bundled freetype2 ignores the flag because it was compiled without subpixel support. The fix requires updating mono/skia: apply the upstream harmony subpixel patch and/or enable FT_CONFIG_OPTION_SUBPIXEL_RENDERING (ClearType, now patent-free as of 2019). Classified as area/libSkiaSharp.native because the issue is in the native library build, not the C# wrapper.

### Key Signals

- "When building skia with skia_use_system_freetype2=true, subpixels are rendered correctly" — **reporter comment #804185309** (Confirms root cause is in the bundled freetype2 build, not the C# API layer.)
- "mono/skia does not yet have the patch for harmony subpixel rendering (https://skia.googlesource.com/skia/+/f25787b72c20e97cdeb74e037dc1ff56a88b45c6)" — **reporter comment #804224273** (Pinpoints the specific missing upstream patch in the skia submodule.)
- "Updating freetype2 should be a one-liner" — **contributor Gillibald, comment #804248258** (A DEPS version bump plus the harmony patch application would fix the issue.)
- "Works correctly on Windows" — **issue body, Basic Information** (Windows uses GDI/DirectWrite font rendering, confirming the Linux-specific freetype2 path is the culprit.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKFont.cs` | — | related | SKFont.Subpixel property maps directly to sk_font_set_subpixel / sk_font_is_subpixel C API calls. The C# binding is correct and passes the flag through. |
| `binding/SkiaSharp/SKSurfaceProperties.cs` | — | related | SKSurfaceProperties constructor accepts SKPixelGeometry and passes it via sk_surfaceprops_new. The C# wrapper correctly propagates BgrHorizontal pixel geometry. |
| `externals/skia/DEPS` | — | direct | DEPS file not initialized in current checkout, preventing direct version verification. Reporter identified mono/skia DEPS L15 freetype2 entry as the dependency to update. |

### Workarounds

- Build SkiaSharp from source with skia_use_system_freetype2=true to use the system-installed freetype2 which supports subpixel rendering
- Enable FT_CONFIG_OPTION_SUBPIXEL_RENDERING (ClearType) in the freetype2 build configuration — patent issue resolved as of 2019

### Next Questions

- Has the harmony subpixel patch (f25787b) been applied in more recent versions of mono/skia or after a Skia milestone bump?
- What is the current freetype2 version pinned in the active mono/skia DEPS file?
- Is the issue reproducible on the current main branch of SkiaSharp with the latest native binaries?

### Resolution Proposals

**Hypothesis:** Apply the harmony subpixel rendering patch from upstream Skia (commit f25787b) to mono/skia AND/OR update the bundled freetype2 to a version with FT_CONFIG_OPTION_SUBPIXEL_RENDERING enabled.

1. **Update freetype2 in mono/skia DEPS and apply harmony patch** — fix, cost/m, validated=untested
   - Update the freetype2 commit hash in mono/skia DEPS and cherry-pick or apply the upstream harmony subpixel rendering patch (skia.googlesource.com commit f25787b). This fixes subpixel rendering on Linux without any API changes.
2. **Workaround: use system freetype2 build** — workaround, cost/l, validated=yes
   - Build SkiaSharp from source with skia_use_system_freetype2=true (requires system freetype2 >= version with subpixel support). See reporter's skia.patch.txt attachment.

**Recommended proposal:** Update freetype2 in mono/skia DEPS and apply harmony patch

**Why:** Fixes all Linux users without requiring source builds. The reporter confirmed system freetype2 works, validating the root cause.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.87 (87%) |
| Reason | Root cause is well-identified (missing freetype2 harmony patch in mono/skia) but needs verification against current mono/skia DEPS to confirm if still present before committing to a fix. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply classification labels to the issue | labels=type/bug, area/libSkiaSharp.native, os/Linux, backend/Raster, tenet/compatibility |
| add-comment | medium | 0.87 (87%) | Acknowledge root cause and provide workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the thorough investigation!

The root cause you identified — the bundled freetype2 in mono/skia not having the [harmony subpixel rendering patch](https://skia.googlesource.com/skia/+/f25787b72c20e97cdeb74e037dc1ff56a88b45c6) nor `FT_CONFIG_OPTION_SUBPIXEL_RENDERING` enabled — explains why `SKFont.Subpixel = true` + `SKFontEdging.SubpixelAntialias` silently falls back to grayscale AA on Linux. The C# API layer is correct; the issue is in the native build configuration.

**Workaround:** Build SkiaSharp from source with `skia_use_system_freetype2=true` to use your system freetype2, which supports subpixel rendering.

We need to verify whether this has been addressed in a more recent Skia milestone update before determining the fix effort.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1669,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T17:57:00Z"
  },
  "summary": "Subpixel text rendering (LCD/ClearType AA) is silently ignored on Linux because the bundled freetype2 in mono/skia lacks the harmony subpixel patch or FT_CONFIG_OPTION_SUBPIXEL_RENDERING.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.95
    },
    "platforms": [
      "os/Linux"
    ],
    "backends": [
      "backend/Raster"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net5.0"
      ]
    },
    "reproEvidence": {
      "codeSnippets": [
        "var props = new SKSurfaceProperties(SKSurfacePropsFlags.None, SKPixelGeometry.BgrHorizontal);\nvar surface = SKSurface.Create(imageInfo, props);\nvar font = new SKFont {Subpixel = true, Edging = SKFontEdging.SubpixelAntialias, Size = 12};\ncanvas.DrawText(\"Hello World\", 10, 20, font, paint);"
      ],
      "screenshots": [
        {
          "url": "https://user-images.githubusercontent.com/10325838/111989784-83181d80-8b12-11eb-9e11-b64755ee754a.png",
          "description": "Expected output from C++ with subpixel AA enabled"
        },
        {
          "url": "https://user-images.githubusercontent.com/10325838/111989926-b064cb80-8b12-11eb-8b7e-224a998cd58f.png",
          "description": "Actual output from C# without subpixel AA (grayscale only)"
        }
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1253",
          "description": "Related: text rendering differences between Chrome/Skia and SkiaSharp"
        },
        {
          "url": "https://skia.googlesource.com/skia/+/f25787b72c20e97cdeb74e037dc1ff56a88b45c6",
          "description": "Upstream Skia harmony subpixel patch that mono/skia is missing"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.3-preview.40",
        "1.68.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Issue never had a code fix applied; bundled freetype2 remains unpatched or ClearType flag still disabled."
    }
  },
  "analysis": {
    "summary": "The bundled freetype2 inside mono/skia does not have the 'harmony subpixel rendering' patch (upstream commit f25787b) and does not have FT_CONFIG_OPTION_SUBPIXEL_RENDERING enabled. When SKFont.Subpixel=true and Edging=SubpixelAntialias are set in C#, the native P/Invoke calls succeed but freetype2 silently falls back to grayscale AA because subpixel mode is not compiled in. Using system freetype2 (skia_use_system_freetype2=true) resolves the issue on the reporter's machine.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKFont.cs",
        "finding": "SKFont.Subpixel property maps directly to sk_font_set_subpixel / sk_font_is_subpixel C API calls. The C# binding is correct and passes the flag through.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKSurfaceProperties.cs",
        "finding": "SKSurfaceProperties constructor accepts SKPixelGeometry and passes it via sk_surfaceprops_new. The C# wrapper correctly propagates BgrHorizontal pixel geometry.",
        "relevance": "related"
      },
      {
        "file": "externals/skia/DEPS",
        "finding": "DEPS file not initialized in current checkout, preventing direct version verification. Reporter identified mono/skia DEPS L15 freetype2 entry as the dependency to update.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "When building skia with skia_use_system_freetype2=true, subpixels are rendered correctly",
        "source": "reporter comment #804185309",
        "interpretation": "Confirms root cause is in the bundled freetype2 build, not the C# API layer."
      },
      {
        "text": "mono/skia does not yet have the patch for harmony subpixel rendering (https://skia.googlesource.com/skia/+/f25787b72c20e97cdeb74e037dc1ff56a88b45c6)",
        "source": "reporter comment #804224273",
        "interpretation": "Pinpoints the specific missing upstream patch in the skia submodule."
      },
      {
        "text": "Updating freetype2 should be a one-liner",
        "source": "contributor Gillibald, comment #804248258",
        "interpretation": "A DEPS version bump plus the harmony patch application would fix the issue."
      },
      {
        "text": "Works correctly on Windows",
        "source": "issue body, Basic Information",
        "interpretation": "Windows uses GDI/DirectWrite font rendering, confirming the Linux-specific freetype2 path is the culprit."
      }
    ],
    "rationale": "This is a native build configuration bug. The C# API correctly passes SKFont.Subpixel and SKFontEdging.SubpixelAntialias to native code, but the bundled freetype2 ignores the flag because it was compiled without subpixel support. The fix requires updating mono/skia: apply the upstream harmony subpixel patch and/or enable FT_CONFIG_OPTION_SUBPIXEL_RENDERING (ClearType, now patent-free as of 2019). Classified as area/libSkiaSharp.native because the issue is in the native library build, not the C# wrapper.",
    "workarounds": [
      "Build SkiaSharp from source with skia_use_system_freetype2=true to use the system-installed freetype2 which supports subpixel rendering",
      "Enable FT_CONFIG_OPTION_SUBPIXEL_RENDERING (ClearType) in the freetype2 build configuration — patent issue resolved as of 2019"
    ],
    "nextQuestions": [
      "Has the harmony subpixel patch (f25787b) been applied in more recent versions of mono/skia or after a Skia milestone bump?",
      "What is the current freetype2 version pinned in the active mono/skia DEPS file?",
      "Is the issue reproducible on the current main branch of SkiaSharp with the latest native binaries?"
    ],
    "resolution": {
      "hypothesis": "Apply the harmony subpixel rendering patch from upstream Skia (commit f25787b) to mono/skia AND/OR update the bundled freetype2 to a version with FT_CONFIG_OPTION_SUBPIXEL_RENDERING enabled.",
      "proposals": [
        {
          "title": "Update freetype2 in mono/skia DEPS and apply harmony patch",
          "description": "Update the freetype2 commit hash in mono/skia DEPS and cherry-pick or apply the upstream harmony subpixel rendering patch (skia.googlesource.com commit f25787b). This fixes subpixel rendering on Linux without any API changes.",
          "category": "fix",
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Workaround: use system freetype2 build",
          "description": "Build SkiaSharp from source with skia_use_system_freetype2=true (requires system freetype2 >= version with subpixel support). See reporter's skia.patch.txt attachment.",
          "category": "workaround",
          "effort": "cost/l",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Update freetype2 in mono/skia DEPS and apply harmony patch",
      "recommendedReason": "Fixes all Linux users without requiring source builds. The reporter confirmed system freetype2 works, validating the root cause."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.87,
      "reason": "Root cause is well-identified (missing freetype2 harmony patch in mono/skia) but needs verification against current mono/skia DEPS to confirm if still present before committing to a fix.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply classification labels to the issue",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Linux",
          "backend/Raster",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge root cause and provide workaround",
        "risk": "medium",
        "confidence": 0.87,
        "comment": "Thanks for the thorough investigation!\n\nThe root cause you identified — the bundled freetype2 in mono/skia not having the [harmony subpixel rendering patch](https://skia.googlesource.com/skia/+/f25787b72c20e97cdeb74e037dc1ff56a88b45c6) nor `FT_CONFIG_OPTION_SUBPIXEL_RENDERING` enabled — explains why `SKFont.Subpixel = true` + `SKFontEdging.SubpixelAntialias` silently falls back to grayscale AA on Linux. The C# API layer is correct; the issue is in the native build configuration.\n\n**Workaround:** Build SkiaSharp from source with `skia_use_system_freetype2=true` to use your system freetype2, which supports subpixel rendering.\n\nWe need to verify whether this has been addressed in a more recent Skia milestone update before determining the fix effort."
      }
    ]
  }
}
```

</details>
