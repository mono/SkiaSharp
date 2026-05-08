# Issue Triage Report — #528

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T20:48:35Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.HarfBuzz (0.90 (90%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** On Android, SKShaper.Shape() returns results where all Codepoints are 0 when the SKTypeface was created via SKTypeface.FromStream(); the issue does not occur on iOS or WPF. A workaround using SKTypeface.FromData() resolves the problem.

**Analysis:** SKShaper constructor calls Typeface.OpenStream().ToHarfBuzzBlob() to pass font data to HarfBuzz. When the typeface was created via SKTypeface.FromStream() on Android, the underlying stream-based typeface does not properly expose font data through OpenStream(), causing HarfBuzz to produce zero codepoints. SKTypeface.FromData() works because data-backed typefaces reliably serve their contents via OpenStream().

**Recommendations:** **needs-investigation** — Real Android-specific bug with confirmed workaround and demo project. Needs deeper investigation into whether current codebase still exhibits the issue (HarfBuzz has been updated since 2018). The workaround should be communicated to users.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.HarfBuzz |
| Platforms | os/Android |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | status/help-wanted, os/Android |

## Evidence

### Reproduction

1. Create a Xamarin.Android (or Xamarin.Forms) project with SkiaSharp.HarfBuzz
2. Load a font using SKTypeface.FromStream(stream)
3. Create an SKShaper with the loaded typeface
4. Call shaper.Shape("Hello world!", x, y, paint)
5. Inspect result.Codepoints — all values are 0 on Android

**Environment:** Android (emulator x86 and LG G4 ARMv8), tested with Arial, SF-UI-Display-Bold.otf, Roboto-Medium.ttf; works correctly on WPF and iOS

**Related issues:** #989

**Repository links:**
- https://github.com/mono/SkiaSharp/files/2035982/HarfBuzzDemo.zip — Demo project reproducing the issue on Android

**Attachments:**
- HarfBuzzDemo.zip — https://github.com/mono/SkiaSharp/files/2035982/HarfBuzzDemo.zip

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | SKShaper.Result.Codepoints are all 0 on Android |
| Repro quality | complete |
| Target frameworks | monoandroid |

## Analysis

### Technical Summary

SKShaper constructor calls Typeface.OpenStream().ToHarfBuzzBlob() to pass font data to HarfBuzz. When the typeface was created via SKTypeface.FromStream() on Android, the underlying stream-based typeface does not properly expose font data through OpenStream(), causing HarfBuzz to produce zero codepoints. SKTypeface.FromData() works because data-backed typefaces reliably serve their contents via OpenStream().

### Rationale

Reporter provides a demo project and tested multiple fonts, emulator, and physical device. Works on iOS and WPF, only fails on Android. Root cause is confirmed by reporter to be in the C API call: after hb_shape(), all buffer.GlyphInfo.CodePoints become 0. The known workaround (SKTypeface.FromData) is confirmed by multiple community members. This is a platform-specific bug in how managed stream-backed typefaces interact with HarfBuzz font loading on Android.

### Key Signals

- "shaper.Shape result CodePoints are all 0 on Android. When testing on WPF or iOS, everything seems ok." — **issue body** (Platform-specific wrong-output bug, not a crash.)
- "HarfBuzzApi.hb_shape(Handle, buffer.Handle, IntPtr.Zero, 0); set all buffer.GlipInfo.CodePoints from utf8 to 0" — **comment #398522676** (HarfBuzz received a font that has no glyph information — likely because OpenStream() returned unusable data on Android.)
- "Using SKData.Create(stream) + SKTypeface.FromData(data) instead of SKTypeface.FromStream(stream) — and it works!" — **comment #400660319** (Confirmed workaround. The difference is that FromData stores a direct blob reference while FromStream creates a stream-wrapped typeface that may not properly serve data back on Android.)
- "SKTypeface.FromFamilyName("Times New Roman") works... may be font related" — **comment #399927864 (maintainer)** (System fonts loaded by name work fine, suggesting the issue is specific to stream-loaded custom fonts.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/SKShaper.cs` | 19-22 | direct | SKShaper constructor calls Typeface.OpenStream(out index).ToHarfBuzzBlob() to feed font data to HarfBuzz. If OpenStream() does not return valid font bytes (e.g., because the underlying stream is consumed or unavailable), HarfBuzz creates a font with no glyph table and all shaped codepoints become 0. |
| `binding/SkiaSharp/SKTypeface.cs` | 100-113 | direct | SKTypeface.FromStream(SKStreamAsset) converts SKManagedStream to a memory stream via ToMemoryStream() before passing to native sk_typeface_create_from_stream. The typeface then owns the stream via RevokeOwnership. Whether the native typeface properly serves this back through sk_typeface_open_stream on Android is the suspected failure point. |
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/BlobExtensions.cs` | 10-36 | related | ToHarfBuzzBlob() tries asset.GetMemoryBase() first; if zero, it copies bytes from the stream via asset.Read(ptr, size). If the stream from OpenStream() is invalid or returns garbage bytes on Android, HarfBuzz receives malformed font data. |

### Workarounds

- Load font bytes into an SKData object first, then use SKTypeface.FromData(data) instead of SKTypeface.FromStream(stream). Example: using (var data = SKData.Create(stream)) { typeface = SKTypeface.FromData(data); }
- Use SKTypeface.FromFamilyName() for system fonts — this works correctly on Android.

### Next Questions

- Does this still reproduce with the current version of SkiaSharp.HarfBuzz (which updated to a newer HarfBuzz)?
- Does the SKTypeface.FromStream path that now converts to memory stream (via ToMemoryStream) fix the OpenStream() round-trip issue?
- Does this affect SKTypeface.FromFile() as well, or only FromStream()?

### Resolution Proposals

**Hypothesis:** Stream-backed typefaces created via SKTypeface.FromStream() on Android do not properly expose their font data when OpenStream() is subsequently called, causing HarfBuzz to receive no glyph tables and produce all-zero codepoints.

1. **Use SKTypeface.FromData() instead of FromStream()** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Load font bytes into an SKData first, then create the typeface from data. This ensures the font bytes are stored as a blob that Android can reliably pass back to HarfBuzz.

```csharp
using (var data = SKData.Create(stream))
{
    typeface = SKTypeface.FromData(data);
}
```
2. **Investigate SKTypeface.FromStream native round-trip on Android** — investigation, confidence 0.70 (70%), cost/m, validated=untested
   - Investigate whether the native sk_typeface_open_stream call correctly returns bytes for stream-backed typefaces on Android. The fix may involve ensuring the C API stores font bytes as a blob (like FromData does) rather than keeping a stream reference.

**Recommended proposal:** Use SKTypeface.FromData() instead of FromStream()

**Why:** Confirmed working by reporter and community. Zero code change required in SkiaSharp; the user can apply it immediately.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | Real Android-specific bug with confirmed workaround and demo project. Needs deeper investigation into whether current codebase still exhibits the issue (HarfBuzz has been updated since 2018). The workaround should be communicated to users. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/SkiaSharp.HarfBuzz, os/Android, tenet/reliability labels | labels=type/bug, area/SkiaSharp.HarfBuzz, os/Android, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Share known workaround and ask for version verification | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and the demo project! A workaround has been confirmed in the comments: use `SKTypeface.FromData()` instead of `SKTypeface.FromStream()` when loading custom fonts on Android:

```csharp
using (var data = SKData.Create(stream))
{
    typeface = SKTypeface.FromData(data);
}
```

This issue appears to be caused by stream-backed typefaces not properly serving font data back through `OpenStream()` on Android, which is what `SKShaper` uses internally to pass the font bytes to HarfBuzz.

Could someone confirm whether this is still reproducible with the latest version of SkiaSharp.HarfBuzz? The HarfBuzz library has been updated significantly since this issue was filed.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 528,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T20:48:35Z",
    "currentLabels": [
      "status/help-wanted",
      "os/Android"
    ]
  },
  "summary": "On Android, SKShaper.Shape() returns results where all Codepoints are 0 when the SKTypeface was created via SKTypeface.FromStream(); the issue does not occur on iOS or WPF. A workaround using SKTypeface.FromData() resolves the problem.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.HarfBuzz",
      "confidence": 0.9
    },
    "platforms": [
      "os/Android"
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
      "errorMessage": "SKShaper.Result.Codepoints are all 0 on Android",
      "reproQuality": "complete",
      "targetFrameworks": [
        "monoandroid"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Android (or Xamarin.Forms) project with SkiaSharp.HarfBuzz",
        "Load a font using SKTypeface.FromStream(stream)",
        "Create an SKShaper with the loaded typeface",
        "Call shaper.Shape(\"Hello world!\", x, y, paint)",
        "Inspect result.Codepoints — all values are 0 on Android"
      ],
      "environmentDetails": "Android (emulator x86 and LG G4 ARMv8), tested with Arial, SF-UI-Display-Bold.otf, Roboto-Medium.ttf; works correctly on WPF and iOS",
      "attachments": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/2035982/HarfBuzzDemo.zip",
          "filename": "HarfBuzzDemo.zip"
        }
      ],
      "relatedIssues": [
        989
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/2035982/HarfBuzzDemo.zip",
          "description": "Demo project reproducing the issue on Android"
        }
      ]
    }
  },
  "analysis": {
    "summary": "SKShaper constructor calls Typeface.OpenStream().ToHarfBuzzBlob() to pass font data to HarfBuzz. When the typeface was created via SKTypeface.FromStream() on Android, the underlying stream-based typeface does not properly expose font data through OpenStream(), causing HarfBuzz to produce zero codepoints. SKTypeface.FromData() works because data-backed typefaces reliably serve their contents via OpenStream().",
    "rationale": "Reporter provides a demo project and tested multiple fonts, emulator, and physical device. Works on iOS and WPF, only fails on Android. Root cause is confirmed by reporter to be in the C API call: after hb_shape(), all buffer.GlyphInfo.CodePoints become 0. The known workaround (SKTypeface.FromData) is confirmed by multiple community members. This is a platform-specific bug in how managed stream-backed typefaces interact with HarfBuzz font loading on Android.",
    "keySignals": [
      {
        "text": "shaper.Shape result CodePoints are all 0 on Android. When testing on WPF or iOS, everything seems ok.",
        "source": "issue body",
        "interpretation": "Platform-specific wrong-output bug, not a crash."
      },
      {
        "text": "HarfBuzzApi.hb_shape(Handle, buffer.Handle, IntPtr.Zero, 0); set all buffer.GlipInfo.CodePoints from utf8 to 0",
        "source": "comment #398522676",
        "interpretation": "HarfBuzz received a font that has no glyph information — likely because OpenStream() returned unusable data on Android."
      },
      {
        "text": "Using SKData.Create(stream) + SKTypeface.FromData(data) instead of SKTypeface.FromStream(stream) — and it works!",
        "source": "comment #400660319",
        "interpretation": "Confirmed workaround. The difference is that FromData stores a direct blob reference while FromStream creates a stream-wrapped typeface that may not properly serve data back on Android."
      },
      {
        "text": "SKTypeface.FromFamilyName(\"Times New Roman\") works... may be font related",
        "source": "comment #399927864 (maintainer)",
        "interpretation": "System fonts loaded by name work fine, suggesting the issue is specific to stream-loaded custom fonts."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/SKShaper.cs",
        "lines": "19-22",
        "finding": "SKShaper constructor calls Typeface.OpenStream(out index).ToHarfBuzzBlob() to feed font data to HarfBuzz. If OpenStream() does not return valid font bytes (e.g., because the underlying stream is consumed or unavailable), HarfBuzz creates a font with no glyph table and all shaped codepoints become 0.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "100-113",
        "finding": "SKTypeface.FromStream(SKStreamAsset) converts SKManagedStream to a memory stream via ToMemoryStream() before passing to native sk_typeface_create_from_stream. The typeface then owns the stream via RevokeOwnership. Whether the native typeface properly serves this back through sk_typeface_open_stream on Android is the suspected failure point.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/BlobExtensions.cs",
        "lines": "10-36",
        "finding": "ToHarfBuzzBlob() tries asset.GetMemoryBase() first; if zero, it copies bytes from the stream via asset.Read(ptr, size). If the stream from OpenStream() is invalid or returns garbage bytes on Android, HarfBuzz receives malformed font data.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Load font bytes into an SKData object first, then use SKTypeface.FromData(data) instead of SKTypeface.FromStream(stream). Example: using (var data = SKData.Create(stream)) { typeface = SKTypeface.FromData(data); }",
      "Use SKTypeface.FromFamilyName() for system fonts — this works correctly on Android."
    ],
    "nextQuestions": [
      "Does this still reproduce with the current version of SkiaSharp.HarfBuzz (which updated to a newer HarfBuzz)?",
      "Does the SKTypeface.FromStream path that now converts to memory stream (via ToMemoryStream) fix the OpenStream() round-trip issue?",
      "Does this affect SKTypeface.FromFile() as well, or only FromStream()?"
    ],
    "resolution": {
      "hypothesis": "Stream-backed typefaces created via SKTypeface.FromStream() on Android do not properly expose their font data when OpenStream() is subsequently called, causing HarfBuzz to receive no glyph tables and produce all-zero codepoints.",
      "proposals": [
        {
          "title": "Use SKTypeface.FromData() instead of FromStream()",
          "description": "Load font bytes into an SKData first, then create the typeface from data. This ensures the font bytes are stored as a blob that Android can reliably pass back to HarfBuzz.",
          "category": "workaround",
          "codeSnippet": "using (var data = SKData.Create(stream))\n{\n    typeface = SKTypeface.FromData(data);\n}",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Investigate SKTypeface.FromStream native round-trip on Android",
          "description": "Investigate whether the native sk_typeface_open_stream call correctly returns bytes for stream-backed typefaces on Android. The fix may involve ensuring the C API stores font bytes as a blob (like FromData does) rather than keeping a stream reference.",
          "category": "investigation",
          "confidence": 0.7,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use SKTypeface.FromData() instead of FromStream()",
      "recommendedReason": "Confirmed working by reporter and community. Zero code change required in SkiaSharp; the user can apply it immediately."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "Real Android-specific bug with confirmed workaround and demo project. Needs deeper investigation into whether current codebase still exhibits the issue (HarfBuzz has been updated since 2018). The workaround should be communicated to users.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.HarfBuzz, os/Android, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.HarfBuzz",
          "os/Android",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Share known workaround and ask for version verification",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed report and the demo project! A workaround has been confirmed in the comments: use `SKTypeface.FromData()` instead of `SKTypeface.FromStream()` when loading custom fonts on Android:\n\n```csharp\nusing (var data = SKData.Create(stream))\n{\n    typeface = SKTypeface.FromData(data);\n}\n```\n\nThis issue appears to be caused by stream-backed typefaces not properly serving font data back through `OpenStream()` on Android, which is what `SKShaper` uses internally to pass the font bytes to HarfBuzz.\n\nCould someone confirm whether this is still reproducible with the latest version of SkiaSharp.HarfBuzz? The HarfBuzz library has been updated significantly since this issue was filed."
      }
    ]
  }
}
```

</details>
