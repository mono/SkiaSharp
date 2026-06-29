# Obsolete API Map (canonical)

The single source of truth for **obsolete members that must never appear in a code example**, and
their modern replacements. Two consumers read this file:

1. The **deterministic checks** in `docs-format-docs` parse the machine table below to flag
   any obsolete member used inside a `csharp` code fence.
2. **Agent prompts** (writer + reviewer-examples) point here instead of restating the list.

> **Authority:** the *ground truth* is always `[Obsolete("...", error: true)]` in the C# source under
> `binding/`. This table is the curated, high-frequency set for fast linting — it is **not** exhaustive.
> When in doubt, grep the member's source for `[Obsolete`. Regenerate the table from source with:
> `grep -rnE '\[Obsolete \(.*error: true' binding/SkiaSharp/`.

## Why this matters

A member marked `[Obsolete("...", error: true)]` is a **compile error** when used, so any example that
calls it never builds. The most common trap in SkiaSharp is text rendering, which moved off `SKPaint`
onto `SKFont`. The `Obsolete` message always names the replacement — use it.

## Machine table

Columns are tab-or-pipe separated: `Type | Member | Replacement`. Every member listed here is **banned
from code examples** — the `docs-format-docs` linter emits a `obsolete-in-example` **warning** for each
one it finds inside a `csharp` fence. The linter reads only the rows inside the fenced block.

```obsolete-map
Type        | Member                         | Replacement
SKPaint     | TextSize                       | SKFont.Size
SKPaint     | TextScaleX                     | SKFont.ScaleX
SKPaint     | TextSkewX                      | SKFont.SkewX
SKPaint     | Typeface                       | SKFont.Typeface
SKPaint     | SubpixelText                   | SKFont.Subpixel
SKPaint     | LcdRenderText                  | SKFont.Edging
SKPaint     | HintingLevel                   | SKFont.Hinting
SKPaint     | FakeBoldText                   | SKFont.Embolden
SKPaint     | ForceAutoHinting               | SKFont.ForceAutoHinting
SKPaint     | TextAlign                      | SKTextAlign draw overloads
SKPaint     | TextEncoding                   | SKTextEncoding draw overloads
SKPaint     | MeasureText                    | SKFont.MeasureText
SKPaint     | BreakText                      | SKFont.BreakText
SKPaint     | GetTextPath                    | SKFont.GetTextPath
SKPaint     | GetFontMetrics                 | SKFont.Metrics
SKCanvas    | DrawText(string,float,float,SKPaint) | DrawText(string,float,float,SKTextAlign,SKFont,SKPaint)
SKCanvas    | DrawText(string,SKPoint,SKPaint)     | DrawText(string,SKPoint,SKTextAlign,SKFont,SKPaint)
SKCanvas    | DrawTextOnPath(string,SKPath,float,float,SKPaint) | DrawTextOnPath(string,SKPath,float,float,SKTextAlign,SKFont,SKPaint)
```

## Correct modern text example

The canonical replacement pattern (verified against `binding/SkiaSharp/`):

```csharp
using var typeface = SKTypeface.FromFamilyName("Arial");
using var font = new SKFont(typeface, 24);
using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
canvas.DrawText("Hello", 10, 40, SKTextAlign.Left, font, paint);
```

## Documenting an obsolete member itself

When the member *being documented* is obsolete (i.e. it is in scope, not just referenced from an
example), still write a factual `summary`/`value`. The `[Obsolete]` attribute already carries the
deprecation warning — the prose should describe what it did, and may point to the replacement, but must
**not** wrap it in a compile-failing example.
