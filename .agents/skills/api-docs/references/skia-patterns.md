# SkiaSharp & HarfBuzz Domain Knowledge

Domain-specific facts for documenting SkiaSharp and HarfBuzzSharp APIs. Always verify claims against source code — this file provides key facts but is not exhaustive.

## Verification Rule

Before documenting byte layouts, struct defaults, or API overloads:
1. **Read the source file** in `binding/` for the C# type
2. For native types (color formats, packed values), check the C/C++ header
3. Never guess a byte layout — either verify it or leave it out

## Two Libraries, Different Conventions

SkiaSharp wraps **Skia** (C++) and **HarfBuzz** (C). They have different conventions. Never assume one works like the other because the C# types look similar.

## Color Types

| Type | Format | Packed Layout | Verified From |
|------|--------|---------------|---------------|
| `SKColor` (Skia) | ARGB | `0xAARRGGBB` | `include/core/SkColor.h` |
| `hb_color_t` (HarfBuzz) | BGRA | `0xBBGGRRAA` | `harfbuzz/src/hb-common.h` |

**Proof for `hb_color_t`:** `HB_COLOR(b,g,r,a)` calls `HB_TAG(b,g,r,a)` which expands to
`(b<<24)|(g<<16)|(r<<8)|a`. So blue is bits 31-24 (high byte), alpha is bits 7-0 (low byte).
The accessors confirm: `hb_color_get_alpha(c) = c & 0xFF`, `hb_color_get_blue(c) = c >> 24`.

- `SKColor` has alpha in the high byte. Standard .NET `Color.ToArgb()` order.
- `hb_color_t` has blue in the high byte, alpha in the low byte.
- `SKFontPaletteOverride.Color` is `sk_color_t` = `SKColor` (ARGB).
- `Face.GetPaletteColors()` returns `hb_color_t` values (BGRA).

Do NOT describe both as "RGBA" just because both are uint32.

## Naming Conventions

### Color Type Suffixes

| Suffix | Meaning | Example |
|--------|---------|---------|
| No suffix or `a` at end | Has alpha | `Rgba8888`, `Bgra8888` |
| `x` at end | Padding — NOT alpha | `Rgb888x`, `RgbF16F16F16x` |

The `x` means the fourth component is unused padding for alignment. The type is opaque.

### Channel Order

The letters in the name specify channel order in memory: `Rgba` = R first, A last. `Bgra` = B first, A last. Match the docs to the name.

## Type Categories

When documenting SkiaSharp types, match the disposal and threading guidance to the type's category:

| Category | Examples | Threading | Disposal |
|----------|----------|-----------|----------|
| Mutable native objects | `SKCanvas`, `SKPaint`, `SKPath` | NOT thread-safe | Must dispose |
| Immutable native objects | `SKImage`, `SKShader`, `SKData` | Thread-safe | Must dispose |
| Value types | `SKColor`, `SKPoint`, `SKRect` | Thread-safe | No disposal |

In examples: mutable types need `using`, immutable types need `using`, value types do not.

## Struct Defaults

C# structs zero-initialize. When documenting struct properties:
- Check what 0 means for each field — it may not be a usable default
- Do not assume a "typical" value is the default (e.g., 72 DPI is NOT the default for a zero-initialized struct)
- If the library provides a constant like `SKDocument.DefaultRasterDpi`, document that separately from the struct's zero-initialized state

**This is a recurring error — be deliberate about it.** Writers keep documenting `SKDocumentXpsOptions.Dpi` as "default 72" because 72 is the obvious "typical" DPI. But `SKDocumentXpsOptions` is a struct with no field initializer, so its `Dpi` defaults to **0**. The 72 comes from `SKDocument.DefaultRasterDpi`, which only applies to the scalar `CreateXps(stream, dpi)` overloads — not to the struct. The rule: a struct property's default is whatever zero-init produces (0 / null / false) **unless you can point to a field initializer in the source**. Never copy a "default" from a sibling constant.

## Standard-Based Enums (CICP, Vulkan, OpenType)

For enums whose members reference external standards, the **member name almost always encodes the exact standard identifier** — use it as a second source of truth alongside `MemberValue`:

| Member name (enum) | Encodes | Correct description |
|--------------------|---------|---------------------|
| `SmpteRp4312` (`SKColorspacePrimariesCicp`) | SMPTE RP **431-2** | DCI-P3 primaries (NOT "RP 432-2") |
| `SmpteEg4321` (`SKColorspacePrimariesCicp`) | SMPTE EG **432-1** | Display P3 / P3-D65 primaries |
| `SmpteSt4281` (`SKColorspacePrimariesCicp`) | SMPTE ST **428-1** | CIE XYZ (D50 white point) primaries for D-Cinema |
| `SmpteSt4281` (`SKColorspaceTransferFnCicp`) | SMPTE ST **428-1** | D-Cinema transfer function — gamma ~2.6 (NOT "linear") |

**The same member name can mean different things in sibling enums.** `SmpteSt4281` appears in *both* `SKColorspacePrimariesCicp` (a set of color primaries) and `SKColorspaceTransferFnCicp` (a transfer function) — the standard (ST 428-1) defines both, but the member describes only the one relevant to *its* enum. Always confirm which enum you are documenting before copying a description; a "transfer function" gloss on a primaries member (or vice versa) is wrong even when the standard number is right.

Two failure modes to check explicitly:
- **Wrong identifier** — the summary's standard number must match the digits in the member name (`SmpteRp4312` → 431-2, not 432-2). Adjacent standards (431 vs 432) are easy to transpose.
- **Wrong behavior** — the *described nature* must be correct, not just the number. ST 428-1 is a gamma-2.6 power transfer; calling it "linear" is wrong even though the number is right. A separate `Linear` member exists for the actual linear transfer. Verify the behavior against the standard, and never let a description match a similar-looking sibling member instead of its own value.

## Caller-Owned vs Parent-Owned Objects

Most SkiaSharp objects are owned by the caller and must be disposed (`using`). But some are **owned by a parent object** and must NOT be disposed by the caller — the parent manages their lifetime:

| Object | Owned by | In examples |
|--------|----------|-------------|
| `SKDocument.BeginPage(...)` return value | the `SKDocument` | do NOT `using`/`Dispose` the canvas |
| `SKSurface.Canvas` | the `SKSurface` | do NOT `using`/`Dispose` the canvas |
| `SKDocument.CreateXps(...)` return value | caller | DO `using` the document |

`BeginPage` wraps its canvas with `owns: false`, so disposing it is the anti-pattern called out in `AGENTS.md`. In a code example, write `var canvas = doc.BeginPage(...);` (no `using`); the parent's `using` cleans everything up. Also note that some factories (e.g. `SKDocument.CreateXps`) return `null` on unsupported platforms even though the signature is non-nullable — examples that immediately dereference the result are misleading.

## Obsolete APIs (Legacy Text Rendering)

Some members are marked `[Obsolete("...", error: true)]`. Because `error: true` makes them a **compile
error**, any code example using them is broken — never use a flagged member in an example or recommend it.

The full obsolete→replacement table is the canonical, machine-parseable list in
[`obsolete-api-map.md`](obsolete-api-map.md) (the deterministic linter and the writer/example agents both
read it). The dominant trap is **text rendering, which moved off `SKPaint` onto `SKFont`** (e.g.
`SKPaint.TextSize` → `SKFont.Size`, and `SKCanvas.DrawText(string,float,float,SKPaint)` → the `SKFont`
overload). See that file for the modern example and the complete row set.

When documenting one of these obsolete members itself, still write a factual summary — the `[Obsolete]`
attribute already carries the deprecation warning; the doc should not invent compile-failing examples
around it.

## API Surface Verification

When writing code examples:
- Verify the exact overload exists by reading the source file
- Do not fabricate overloads that "should" exist (e.g., passing a struct to a method that only accepts scalar parameters)
- If no suitable overload exists for an example, show direct property/field usage instead
- `SKColor` to `uint` requires an **explicit** cast: `(uint)SKColors.Red`

## String Handling

APIs that parse fixed-length strings often pad or truncate silently. Always read the method body to check actual behavior — don't assume "must be exactly N characters" if the source pads/truncates instead of throwing.
