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

| Type | Format | Packed Layout |
|------|--------|---------------|
| `SKColor` (Skia) | ARGB | `0xAARRGGBB` |
| `hb_color_t` (HarfBuzz) | BGRA | `0xBBGGRRAA` |

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

## API Surface Verification

When writing code examples:
- Verify the exact overload exists by reading the source file
- Do not fabricate overloads that "should" exist (e.g., passing a struct to a method that only accepts scalar parameters)
- If no suitable overload exists for an example, show direct property/field usage instead
- `SKColor` to `uint` requires an **explicit** cast: `(uint)SKColors.Red`

## String Handling

APIs that parse fixed-length strings often pad or truncate silently. Always read the method body to check actual behavior — don't assume "must be exactly N characters" if the source pads/truncates instead of throwing.
