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

Columns are tab-or-pipe separated: `Type | Member | Replacement | Severity`.
`Severity` = `error` (compile-fails — CRITICAL) or `warn` (still compiles — IMPORTANT).
The linter reads only the rows inside the fenced block.

```obsolete-map
Type        | Member                         | Replacement                              | Severity
SKPaint     | TextSize                       | SKFont.Size                              | error
SKPaint     | TextScaleX                     | SKFont.ScaleX                            | error
SKPaint     | TextSkewX                      | SKFont.SkewX                             | error
SKPaint     | Typeface                       | SKFont.Typeface                          | error
SKPaint     | SubpixelText                   | SKFont.Subpixel                          | error
SKPaint     | LcdRenderText                  | SKFont.Edging                            | error
SKPaint     | HintingLevel                   | SKFont.Hinting                           | error
SKPaint     | FakeBoldText                   | SKFont.Embolden                          | error
SKPaint     | ForceAutoHinting               | SKFont.ForceAutoHinting                  | error
SKPaint     | TextAlign                      | SKTextAlign draw overloads               | error
SKPaint     | TextEncoding                   | SKTextEncoding draw overloads            | error
SKPaint     | MeasureText                    | SKFont.MeasureText                       | error
SKPaint     | BreakText                      | SKFont.BreakText                         | error
SKPaint     | GetTextPath                    | SKFont.GetTextPath                       | error
SKPaint     | GetFontMetrics                 | SKFont.Metrics                           | error
SKCanvas    | DrawText(string,float,float,SKPaint) | DrawText(string,float,float,SKTextAlign,SKFont,SKPaint) | error
SKCanvas    | DrawText(string,SKPoint,SKPaint)     | DrawText(string,SKPoint,SKTextAlign,SKFont,SKPaint)     | error
SKCanvas    | DrawTextOnPath(string,SKPath,float,float,SKPaint) | DrawTextOnPath(string,SKPath,float,float,SKTextAlign,SKFont,SKPaint) | error
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
