# Obsolete API Map (canonical)

The curated reference for **obsolete members and the modern API to use in their place**. The writer
(`adding.md`) and the example reviewer (`reviewing.md` Check B) read this file when judging whether a
code example or a piece of prose uses a deprecated API.

> **Authority:** the *ground truth* is always `[Obsolete("...", error: true)]` in the C# source under
> `binding/`. This file is the curated, high-frequency set — it is **not** exhaustive. When in doubt,
> grep the member's source: `grep -rnE '\[Obsolete \(.*error: true' binding/SkiaSharp/`.

> **No linter parses this file.** Telling an obsolete API from its modern replacement needs
> *signature/receiver* awareness (the two often share a method name — see §2), which a name-only check
> gets wrong. So this is a judgement call for the author and reviewer, guided by the two lists below.

## Why this matters

A member marked `[Obsolete("...", error: true)]` is a **compile error** when used, so any example that
calls it never builds. The most common trap in SkiaSharp is text rendering, which moved off `SKPaint`
onto `SKFont`. The `[Obsolete]` message always names the replacement — use it.

## 1. Always obsolete — never use these

These `SKPaint` text members are deprecated outright. There is no good overload; each is a clean swap to
the `SKFont` equivalent. **Never put the left column in an example or recommend it in prose.**

| Obsolete (`SKPaint`) | Use instead |
|---|---|
| `SKPaint.TextSize` | `SKFont.Size` |
| `SKPaint.TextScaleX` | `SKFont.ScaleX` |
| `SKPaint.TextSkewX` | `SKFont.SkewX` |
| `SKPaint.Typeface` | `SKFont.Typeface` |
| `SKPaint.SubpixelText` | `SKFont.Subpixel` |
| `SKPaint.LcdRenderText` | `SKFont.Edging` |
| `SKPaint.HintingLevel` | `SKFont.Hinting` |
| `SKPaint.FakeBoldText` | `SKFont.Embolden` |
| `SKPaint.ForceAutoHinting` | `SKFont.ForceAutoHinting` |
| `SKPaint.TextAlign` | the `SKTextAlign` parameter on the draw overloads (see §2) |
| `SKPaint.TextEncoding` | the `SKTextEncoding` parameter on the text overloads |

## 2. Same name on the modern API — disambiguate by signature

The methods below are the tricky ones: **the obsolete and the modern version share a method name**, so the
name alone tells you nothing. Look at the *receiver type* and the *parameter list*:

- **The obsolete forms live on `SKPaint`, or are the `SKCanvas` overloads with no `SKFont` argument.**
- **The modern forms live on `SKFont`, or are the `SKCanvas` overloads that take an `SKFont`.**

The mnemonic: **the deprecated overload is the one *without* the `SKFont` (the modern overloads added an
`SKFont`, and usually an `SKTextAlign`, parameter).** When writing — or choosing which overload an example
should show — always pick the `SKFont` form.

| Method | ❌ Obsolete form (no `SKFont`) | ✅ Preferred form (takes `SKFont`) |
|---|---|---|
| Draw text | `SKCanvas.DrawText(string, float, float, SKPaint)` | `SKCanvas.DrawText(string, float, float, SKTextAlign, SKFont, SKPaint)` |
| Draw text at a point | `SKCanvas.DrawText(string, SKPoint, SKPaint)` | `SKCanvas.DrawText(string, SKPoint, SKTextAlign, SKFont, SKPaint)` |
| Draw text on a path | `SKCanvas.DrawTextOnPath(string, SKPath, float, float, SKPaint)` | `SKCanvas.DrawTextOnPath(string, SKPath, float, float, SKTextAlign, SKFont, SKPaint)` |
| Measure text | `SKPaint.MeasureText(...)` | `SKFont.MeasureText(...)` |
| Break text | `SKPaint.BreakText(...)` | `SKFont.BreakText(...)` |
| Text → path | `SKPaint.GetTextPath(...)` | `SKFont.GetTextPath(...)` |
| Font metrics | `SKPaint.GetFontMetrics(...)` / `SKPaint.FontMetrics` | `SKFont.Metrics` |
| Glyph widths/positions | `SKPaint.GetGlyphWidths(...)` / `GetGlyphPositions(...)` | the matching `SKFont` method |

So `font.MeasureText("hi")` and `canvas.DrawText("hi", 10, 40, SKTextAlign.Left, font, paint)` are
**correct, modern** — do not "fix" them. `paint.MeasureText("hi")` and
`canvas.DrawText("hi", 10, 40, paint)` are the deprecated forms — replace them.

## Correct modern text example

The canonical replacement pattern (verified against `binding/SkiaSharp/`):

```csharp
using var typeface = SKTypeface.FromFamilyName("Arial");
using var font = new SKFont(typeface, 24);
using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
canvas.DrawText("Hello", 10, 40, SKTextAlign.Left, font, paint);
float width = font.MeasureText("Hello");
```

## Documenting an obsolete member itself

When the member *being documented* is obsolete (i.e. it is in scope, not just referenced from an
example), still write a factual `summary`/`value`. The `[Obsolete]` attribute already carries the
deprecation warning — the prose should describe what it did, and may point to the replacement, but must
**not** wrap it in a compile-failing example.
