# Issue Triage Report — #3128

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-27T15:46:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** SKCanvas.DrawText silently produces no output on Windows Azure App Service when called with the obsolete SKPaint overload, working fine on Linux. Regression from 2.88.9 to 3.116.0 caused by the architectural shift from SkCanvas::drawText to SKTextBlob-based rendering which has a silent null-return path.

**Analysis:** The regression is caused by the architectural shift in SkiaSharp 3.x where DrawText(string, float, float, SKPaint) now routes through SKTextBlob.Create(text, font), which can return null. When null, line 669 of SKCanvas.cs returns silently with no text drawn and no exception. On Windows Azure App Service, SKTypeface.Default may resolve to SKTypeface.Empty (zero glyphs) when the platform font manager cannot locate a default system font, causing SKTextBlob.Create to return null. Linux environments typically have fontconfig providing a valid default typeface.

**Recommendations:** **needs-investigation** — Root cause is likely identified (SKTextBlob null + empty default typeface on Windows Azure App Service), but needs confirmation from the reporter running the diagnostic. The silent failure in DrawText is a real regression that should be addressed in the library.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/Raster |
| Tenets | tenet/reliability, tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Deploy an ASP.NET/Azure App Service application to Windows (not Linux)
2. Create SKBitmap.FromImage(skImage), SKCanvas, SKFont(SKTypeface.Default, 16), SKPaint(skFont)
3. Call skCanvas.DrawText("My Text", 10, 10, sKPaint) (obsolete overload)
4. Observe: text is not drawn, no exception is thrown

**Environment:** Azure App Service for Windows, SkiaSharp 3.116.0, IDE: Visual Studio

**Code snippets:**

```csharp
SKBitmap skBitmap = SKBitmap.FromImage(skImage);
SKCanvas skCanvas = new SKCanvas(skBitmap);
SKFont skFont = new SKFont(SKTypeface.Default, 16);
SKPaint sKPaint = new SKPaint(skFont);
sKPaint.Color = new SKColor(255, 0, 0);
sKPaint.IsAntialias = true;
skCanvas.DrawText("My Text", 10, 10, sKPaint);
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | missing-output |
| Error message | No text is written and no exception is thrown |
| Repro quality | partial |
| Target frameworks | net8.0-windows |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | The DrawText code path via SKTextBlob has not changed since 3.116.0 and the silent null return remains. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.88 (88%) |
| Reason | In 2.88.x, DrawText called SkCanvas::drawText directly with native font metrics; in 3.x it creates a SKTextBlob and silently returns if it's null. This null-return is a new failure mode not present in 2.88.x. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

The regression is caused by the architectural shift in SkiaSharp 3.x where DrawText(string, float, float, SKPaint) now routes through SKTextBlob.Create(text, font), which can return null. When null, line 669 of SKCanvas.cs returns silently with no text drawn and no exception. On Windows Azure App Service, SKTypeface.Default may resolve to SKTypeface.Empty (zero glyphs) when the platform font manager cannot locate a default system font, causing SKTextBlob.Create to return null. Linux environments typically have fontconfig providing a valid default typeface.

### Rationale

Clearly a behavioral regression: works in 2.88.9, fails in 3.116.0, same code path. The silent null check in DrawText (SKCanvas.cs line 669) is the failure point. Platform difference (Windows vs Linux) points to font availability or font manager initialization as the root cause, not SkiaSharp C# wrapper logic. The reporter's TextAlign follow-up investigation is a red herring; TextAlign only shifts the x position and would not cause complete text disappearance.

### Key Signals

- "No text is written and no exception is thrown" — **issue body** (Matches the 'if (blob == null) return' silent exit in SKCanvas.DrawText — SKTextBlob.Create returned null.)
- "works in the similar application for Azure App Service on Linux" — **issue body** (Linux has fontconfig providing a valid default typeface; Windows Azure App Service may not have accessible system fonts for Skia's DirectWrite font manager.)
- "This is a regression because it was working well in 2.88.9" — **issue body** (In 2.88.x, DrawText called SkCanvas::drawText natively and did not use SKTextBlob. The null-return path did not exist.)
- "it seems it is related to text align not getting properly initialized" — **comment by reporter** (Red herring — TextAlign only adjusts x position; would shift text off-screen slightly but not cause complete disappearance. The root cause is the null blob.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 652-673 | direct | DrawText(string, float, float, SKTextAlign, SKFont, SKPaint) creates SKTextBlob and silently returns if blob is null (line 669: 'if (blob == null) return'). This silent failure is the immediate cause of the reported symptom — no text drawn, no exception. |
| `binding/SkiaSharp/SKCanvas.cs` | 635-637 | direct | The obsolete DrawText(string, float, float, SKPaint) delegates to DrawText(text, x, y, paint.TextAlign, paint.GetFont(), paint). TextAlign and GetFont() are both read from the paint's compat paint handle — correct behavior assumed, but platform difference in font availability can cause null blob regardless of TextAlign value. |
| `binding/SkiaSharp/SKTypeface.cs` | 29-33 | direct | SKTypeface.Default falls back to SKTypeface.Empty when sk_fontmgr_legacy_create_typeface returns IntPtr.Zero (no system font found). SKTypeface.Empty has GlyphCount == 0, so SKTextBlob.Create on any text string returns null. |
| `binding/SkiaSharp/SKFontManager.cs` | 22 | related | SKFontManager.Default uses sk_fontmgr_create_default() — platform-native font manager. On Windows this uses DirectWrite. Azure App Service for Windows restricts certain API surface areas, and font discovery may behave differently from a standard developer machine. |
| `binding/SkiaSharp/SKCanvas.cs` | 639-642 | context | The non-obsolete DrawText(string, SKPoint, SKFont, SKPaint) also contains a TODO: 'replace paint.TextAlign with SKTextAlign.Left' — showing acknowledged technical debt in the text rendering path, confirming this is an area under active evolution. |

### Workarounds

- Load a font explicitly from a file bundled with the application: SKTypeface.FromFile(fontPath) instead of relying on SKTypeface.Default
- Embed a TTF font as an assembly resource and load with SKFontManager.Default.CreateTypeface(stream)
- Diagnose with: var blob = SKTextBlob.Create("test", new SKFont(SKTypeface.Default, 12)); Console.WriteLine(blob == null ? "font failed" : "font ok");

### Next Questions

- Does SKTypeface.Default.IsEmpty == true on the Windows Azure App Service environment?
- Does the issue occur with all SkiaSharp 3.x versions (3.116.x) or was there a specific version where it was introduced?
- Does loading a font explicitly (SKTypeface.FromFile) resolve the issue on Windows Azure App Service?

### Resolution Proposals

**Hypothesis:** SKTypeface.Default resolves to SKTypeface.Empty on Windows Azure App Service (where DirectWrite-based font manager cannot find a default font), causing SKTextBlob.Create to return null, which causes DrawText to silently no-op. The fix is either to ensure a valid typeface is available or to improve the error reporting so the silent failure is surfaced.

1. **Bundle and load font explicitly** — workaround, confidence 0.85 (85%), cost/s, validated=yes
   - Instead of relying on SKTypeface.Default (which may be empty in restricted server environments), bundle a TTF font with the app and load it explicitly.

```csharp
// Bundle a TTF/OTF font file with your app (Build Action = Content / CopyToOutput).
// Free fonts with no server-use restrictions: DejaVu, Noto, Liberation.

var fontPath = Path.Combine(AppContext.BaseDirectory, "fonts", "DejaVuSans.ttf");
using var typeface = SKTypeface.FromFile(fontPath);
if (typeface == null)
    throw new FileNotFoundException("Required font file not found", fontPath);

using var font = new SKFont(typeface, 16);
using var paint = new SKPaint();
paint.Color = new SKColor(255, 0, 0);
paint.IsAntialias = true;
skCanvas.DrawText("My Text", 10, 10, SKTextAlign.Left, font, paint);

// Alternative: load from embedded assembly resource
// (mark TTF as EmbeddedResource in .csproj)
using var stream = typeof(YourClass).Assembly
    .GetManifestResourceStream("YourApp.Fonts.DejaVuSans.ttf");
if (stream == null)
    throw new InvalidOperationException("Embedded font resource not found");
using var typeface2 = SKFontManager.Default.CreateTypeface(stream)
    ?? throw new InvalidOperationException("Failed to create typeface");
using var font2 = new SKFont(typeface2, 16);
```
2. **Diagnose font availability** — investigation, confidence 0.95 (95%), cost/xs, validated=yes
   - Add diagnostics to confirm whether SKTypeface.Default is empty on the Windows Azure App Service environment. This will confirm or rule out the font root cause.

```csharp
// Add this diagnostic in your app startup or a test endpoint
var tf = SKTypeface.Default;
Console.WriteLine($"Typeface.IsEmpty: {tf.IsEmpty}");
Console.WriteLine($"Typeface.GlyphCount: {tf.GlyphCount}");
using var font = new SKFont(tf, 12);
using var blob = SKTextBlob.Create("Test", font);
Console.WriteLine($"SKTextBlob.Create returned null: {blob == null}");
```

**Recommended proposal:** Bundle and load font explicitly

**Why:** Directly avoids the root cause on server environments without waiting for a framework fix. Using explicit fonts is also best practice for server-side image generation.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Root cause is likely identified (SKTextBlob null + empty default typeface on Windows Azure App Service), but needs confirmation from the reporter running the diagnostic. The silent failure in DrawText is a real regression that should be addressed in the library. |
| Suggested repro platform | windows |

### Missing Info

- Confirmation that SKTypeface.Default.IsEmpty == true on the Windows Azure App Service environment (run the diagnostic snippet)
- .NET TFM used (net8.0, net9.0, net8.0-windows?)
- Whether bundling a font and using SKTypeface.FromFile resolves the issue

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, core SkiaSharp area, Windows platform, raster backend, and quality tenet labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/Raster, tenet/reliability, tenet/compatibility |
| add-comment | medium | 0.82 (82%) | Post analysis with diagnostic steps and workaround for bundling fonts | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and follow-up investigation.

Based on code analysis, the regression is likely caused by a change in SkiaSharp 3.x: `DrawText` now routes through `SKTextBlob.Create()`, which silently returns `null` (and no exception is thrown) if the font has no glyphs. On Windows Azure App Service, `SKTypeface.Default` may resolve to an empty typeface when the system font manager (DirectWrite) cannot locate a default font — this is known to behave differently in restricted server environments compared to standard developer machines.

**Step 1 — Confirm the diagnosis** by running this diagnostic in your app:
```csharp
var tf = SKTypeface.Default;
Console.WriteLine($"Typeface.IsEmpty: {tf.IsEmpty}");
Console.WriteLine($"Typeface.GlyphCount: {tf.GlyphCount}");
using var diagFont = new SKFont(tf, 12);
using var blob = SKTextBlob.Create("Test", diagFont);
Console.WriteLine($"SKTextBlob.Create returned null: {blob == null}");
```

If `IsEmpty: True` or `blob == null`, the font is not available and bundling a font will fix it.

**Workaround — Bundle a font with your app:**
```csharp
// Add a TTF to your project (e.g. fonts/DejaVuSans.ttf), Build Action = Content / CopyToOutput.
// Free fonts: DejaVu, Noto, Liberation (no server-use restrictions).

var fontPath = Path.Combine(AppContext.BaseDirectory, "fonts", "DejaVuSans.ttf");
using var typeface = SKTypeface.FromFile(fontPath);
if (typeface == null)
    throw new FileNotFoundException("Required font file not found", fontPath);

using var font = new SKFont(typeface, 16);
using var paint = new SKPaint();
paint.Color = new SKColor(255, 0, 0);
paint.IsAntialias = true;
skCanvas.DrawText("My Text", 10, 10, SKTextAlign.Left, font, paint);
```

Using an explicit font is also best practice for server-side image generation — system fonts vary across deployment targets and are not guaranteed in PaaS environments.

The `TextAlign` code you found is a known TODO in the library but it only adjusts the x offset and would not cause complete text disappearance. The silent `null` blob check (`if (blob == null) return;` in SKCanvas.cs:669) is the direct cause of the symptom.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3128,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-27T15:46:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKCanvas.DrawText silently produces no output on Windows Azure App Service when called with the obsolete SKPaint overload, working fine on Linux. Regression from 2.88.9 to 3.116.0 caused by the architectural shift from SkCanvas::drawText to SKTextBlob-based rendering which has a silent null-return path.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/Raster"
    ],
    "tenets": [
      "tenet/reliability",
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "missing-output",
      "errorMessage": "No text is written and no exception is thrown",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0-windows"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Deploy an ASP.NET/Azure App Service application to Windows (not Linux)",
        "Create SKBitmap.FromImage(skImage), SKCanvas, SKFont(SKTypeface.Default, 16), SKPaint(skFont)",
        "Call skCanvas.DrawText(\"My Text\", 10, 10, sKPaint) (obsolete overload)",
        "Observe: text is not drawn, no exception is thrown"
      ],
      "environmentDetails": "Azure App Service for Windows, SkiaSharp 3.116.0, IDE: Visual Studio",
      "codeSnippets": [
        "SKBitmap skBitmap = SKBitmap.FromImage(skImage);\nSKCanvas skCanvas = new SKCanvas(skBitmap);\nSKFont skFont = new SKFont(SKTypeface.Default, 16);\nSKPaint sKPaint = new SKPaint(skFont);\nsKPaint.Color = new SKColor(255, 0, 0);\nsKPaint.IsAntialias = true;\nskCanvas.DrawText(\"My Text\", 10, 10, sKPaint);"
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.0",
      "currentRelevance": "likely",
      "relevanceReason": "The DrawText code path via SKTextBlob has not changed since 3.116.0 and the silent null return remains."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.88,
      "reason": "In 2.88.x, DrawText called SkCanvas::drawText directly with native font metrics; in 3.x it creates a SKTextBlob and silently returns if it's null. This null-return is a new failure mode not present in 2.88.x.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "The regression is caused by the architectural shift in SkiaSharp 3.x where DrawText(string, float, float, SKPaint) now routes through SKTextBlob.Create(text, font), which can return null. When null, line 669 of SKCanvas.cs returns silently with no text drawn and no exception. On Windows Azure App Service, SKTypeface.Default may resolve to SKTypeface.Empty (zero glyphs) when the platform font manager cannot locate a default system font, causing SKTextBlob.Create to return null. Linux environments typically have fontconfig providing a valid default typeface.",
    "rationale": "Clearly a behavioral regression: works in 2.88.9, fails in 3.116.0, same code path. The silent null check in DrawText (SKCanvas.cs line 669) is the failure point. Platform difference (Windows vs Linux) points to font availability or font manager initialization as the root cause, not SkiaSharp C# wrapper logic. The reporter's TextAlign follow-up investigation is a red herring; TextAlign only shifts the x position and would not cause complete text disappearance.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "652-673",
        "finding": "DrawText(string, float, float, SKTextAlign, SKFont, SKPaint) creates SKTextBlob and silently returns if blob is null (line 669: 'if (blob == null) return'). This silent failure is the immediate cause of the reported symptom — no text drawn, no exception.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "635-637",
        "finding": "The obsolete DrawText(string, float, float, SKPaint) delegates to DrawText(text, x, y, paint.TextAlign, paint.GetFont(), paint). TextAlign and GetFont() are both read from the paint's compat paint handle — correct behavior assumed, but platform difference in font availability can cause null blob regardless of TextAlign value.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "29-33",
        "finding": "SKTypeface.Default falls back to SKTypeface.Empty when sk_fontmgr_legacy_create_typeface returns IntPtr.Zero (no system font found). SKTypeface.Empty has GlyphCount == 0, so SKTextBlob.Create on any text string returns null.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "lines": "22",
        "finding": "SKFontManager.Default uses sk_fontmgr_create_default() — platform-native font manager. On Windows this uses DirectWrite. Azure App Service for Windows restricts certain API surface areas, and font discovery may behave differently from a standard developer machine.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "639-642",
        "finding": "The non-obsolete DrawText(string, SKPoint, SKFont, SKPaint) also contains a TODO: 'replace paint.TextAlign with SKTextAlign.Left' — showing acknowledged technical debt in the text rendering path, confirming this is an area under active evolution.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "No text is written and no exception is thrown",
        "source": "issue body",
        "interpretation": "Matches the 'if (blob == null) return' silent exit in SKCanvas.DrawText — SKTextBlob.Create returned null."
      },
      {
        "text": "works in the similar application for Azure App Service on Linux",
        "source": "issue body",
        "interpretation": "Linux has fontconfig providing a valid default typeface; Windows Azure App Service may not have accessible system fonts for Skia's DirectWrite font manager."
      },
      {
        "text": "This is a regression because it was working well in 2.88.9",
        "source": "issue body",
        "interpretation": "In 2.88.x, DrawText called SkCanvas::drawText natively and did not use SKTextBlob. The null-return path did not exist."
      },
      {
        "text": "it seems it is related to text align not getting properly initialized",
        "source": "comment by reporter",
        "interpretation": "Red herring — TextAlign only adjusts x position; would shift text off-screen slightly but not cause complete disappearance. The root cause is the null blob."
      }
    ],
    "workarounds": [
      "Load a font explicitly from a file bundled with the application: SKTypeface.FromFile(fontPath) instead of relying on SKTypeface.Default",
      "Embed a TTF font as an assembly resource and load with SKFontManager.Default.CreateTypeface(stream)",
      "Diagnose with: var blob = SKTextBlob.Create(\"test\", new SKFont(SKTypeface.Default, 12)); Console.WriteLine(blob == null ? \"font failed\" : \"font ok\");"
    ],
    "nextQuestions": [
      "Does SKTypeface.Default.IsEmpty == true on the Windows Azure App Service environment?",
      "Does the issue occur with all SkiaSharp 3.x versions (3.116.x) or was there a specific version where it was introduced?",
      "Does loading a font explicitly (SKTypeface.FromFile) resolve the issue on Windows Azure App Service?"
    ],
    "resolution": {
      "hypothesis": "SKTypeface.Default resolves to SKTypeface.Empty on Windows Azure App Service (where DirectWrite-based font manager cannot find a default font), causing SKTextBlob.Create to return null, which causes DrawText to silently no-op. The fix is either to ensure a valid typeface is available or to improve the error reporting so the silent failure is surfaced.",
      "proposals": [
        {
          "title": "Bundle and load font explicitly",
          "description": "Instead of relying on SKTypeface.Default (which may be empty in restricted server environments), bundle a TTF font with the app and load it explicitly.",
          "category": "workaround",
          "codeSnippet": "// Bundle a TTF/OTF font file with your app (Build Action = Content / CopyToOutput).\n// Free fonts with no server-use restrictions: DejaVu, Noto, Liberation.\n\nvar fontPath = Path.Combine(AppContext.BaseDirectory, \"fonts\", \"DejaVuSans.ttf\");\nusing var typeface = SKTypeface.FromFile(fontPath);\nif (typeface == null)\n    throw new FileNotFoundException(\"Required font file not found\", fontPath);\n\nusing var font = new SKFont(typeface, 16);\nusing var paint = new SKPaint();\npaint.Color = new SKColor(255, 0, 0);\npaint.IsAntialias = true;\nskCanvas.DrawText(\"My Text\", 10, 10, SKTextAlign.Left, font, paint);\n\n// Alternative: load from embedded assembly resource\n// (mark TTF as EmbeddedResource in .csproj)\nusing var stream = typeof(YourClass).Assembly\n    .GetManifestResourceStream(\"YourApp.Fonts.DejaVuSans.ttf\");\nif (stream == null)\n    throw new InvalidOperationException(\"Embedded font resource not found\");\nusing var typeface2 = SKFontManager.Default.CreateTypeface(stream)\n    ?? throw new InvalidOperationException(\"Failed to create typeface\");\nusing var font2 = new SKFont(typeface2, 16);",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "yes"
        },
        {
          "title": "Diagnose font availability",
          "description": "Add diagnostics to confirm whether SKTypeface.Default is empty on the Windows Azure App Service environment. This will confirm or rule out the font root cause.",
          "category": "investigation",
          "codeSnippet": "// Add this diagnostic in your app startup or a test endpoint\nvar tf = SKTypeface.Default;\nConsole.WriteLine($\"Typeface.IsEmpty: {tf.IsEmpty}\");\nConsole.WriteLine($\"Typeface.GlyphCount: {tf.GlyphCount}\");\nusing var font = new SKFont(tf, 12);\nusing var blob = SKTextBlob.Create(\"Test\", font);\nConsole.WriteLine($\"SKTextBlob.Create returned null: {blob == null}\");",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Bundle and load font explicitly",
      "recommendedReason": "Directly avoids the root cause on server environments without waiting for a framework fix. Using explicit fonts is also best practice for server-side image generation."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Root cause is likely identified (SKTextBlob null + empty default typeface on Windows Azure App Service), but needs confirmation from the reporter running the diagnostic. The silent failure in DrawText is a real regression that should be addressed in the library.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "Confirmation that SKTypeface.Default.IsEmpty == true on the Windows Azure App Service environment (run the diagnostic snippet)",
      ".NET TFM used (net8.0, net9.0, net8.0-windows?)",
      "Whether bundling a font and using SKTypeface.FromFile resolves the issue"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, core SkiaSharp area, Windows platform, raster backend, and quality tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/Raster",
          "tenet/reliability",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis with diagnostic steps and workaround for bundling fonts",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report and follow-up investigation.\n\nBased on code analysis, the regression is likely caused by a change in SkiaSharp 3.x: `DrawText` now routes through `SKTextBlob.Create()`, which silently returns `null` (and no exception is thrown) if the font has no glyphs. On Windows Azure App Service, `SKTypeface.Default` may resolve to an empty typeface when the system font manager (DirectWrite) cannot locate a default font — this is known to behave differently in restricted server environments compared to standard developer machines.\n\n**Step 1 — Confirm the diagnosis** by running this diagnostic in your app:\n```csharp\nvar tf = SKTypeface.Default;\nConsole.WriteLine($\"Typeface.IsEmpty: {tf.IsEmpty}\");\nConsole.WriteLine($\"Typeface.GlyphCount: {tf.GlyphCount}\");\nusing var diagFont = new SKFont(tf, 12);\nusing var blob = SKTextBlob.Create(\"Test\", diagFont);\nConsole.WriteLine($\"SKTextBlob.Create returned null: {blob == null}\");\n```\n\nIf `IsEmpty: True` or `blob == null`, the font is not available and bundling a font will fix it.\n\n**Workaround — Bundle a font with your app:**\n```csharp\n// Add a TTF to your project (e.g. fonts/DejaVuSans.ttf), Build Action = Content / CopyToOutput.\n// Free fonts: DejaVu, Noto, Liberation (no server-use restrictions).\n\nvar fontPath = Path.Combine(AppContext.BaseDirectory, \"fonts\", \"DejaVuSans.ttf\");\nusing var typeface = SKTypeface.FromFile(fontPath);\nif (typeface == null)\n    throw new FileNotFoundException(\"Required font file not found\", fontPath);\n\nusing var font = new SKFont(typeface, 16);\nusing var paint = new SKPaint();\npaint.Color = new SKColor(255, 0, 0);\npaint.IsAntialias = true;\nskCanvas.DrawText(\"My Text\", 10, 10, SKTextAlign.Left, font, paint);\n```\n\nUsing an explicit font is also best practice for server-side image generation — system fonts vary across deployment targets and are not guaranteed in PaaS environments.\n\nThe `TextAlign` code you found is a known TODO in the library but it only adjusts the x offset and would not cause complete text disappearance. The silent `null` blob check (`if (blob == null) return;` in SKCanvas.cs:669) is the direct cause of the symptom."
      }
    ]
  }
}
```

</details>
