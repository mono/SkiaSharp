# XML Documentation Patterns

.NET XML documentation formatting rules. For SkiaSharp/HarfBuzz domain knowledge, see [skia-patterns.md](skia-patterns.md).

Based on [official .NET API documentation guidelines](https://github.com/dotnet/dotnet-api-docs/wiki).

## Contents

- [File Structure](#file-structure)
- [Syntax Reference](#syntax-reference)
- [Summary Patterns](#summary-patterns)
- [Parameters](#parameters)
- [Return Values](#return-values)
- [Punctuation Exceptions](#punctuation-exceptions)
- [Common Mistakes](#common-mistakes)
- [Extension Methods](#extension-methods)
- [Platform View Constructors](#platform-view-constructors)
- [Rich Remarks and Examples](#rich-remarks-and-examples)
- [Type-Level Documentation](#type-level-documentation)

## File Structure

Each type has its own XML file with this structure:

```xml
<Type Name="SKCanvas" FullName="SkiaSharp.SKCanvas">
  <Docs>
    <summary>Encapsulates all of the state about drawing into a device.</summary>
    <remarks>...</remarks>
  </Docs>
  <Members>
    <Member MemberName="DrawRect">
      <Docs>
        <summary>Draws a rectangle using the specified paint.</summary>
        <param name="rect">The rectangle to draw.</param>
        <param name="paint">The paint to use for drawing.</param>
        <remarks />
      </Docs>
    </Member>
  </Members>
</Type>
```

## Syntax Reference

### XML Tags

| Tag | Usage |
|-----|-------|
| `<summary>` | Brief description (required) |
| `<param name="x">` | Parameter description |
| `<returns>` | Return value description |
| `<value>` | Property value description |
| `<remarks>` | Extended details (use `<remarks />` if empty) |
| `<exception cref="T:...">` | Exception documentation |

### Cross-References

```xml
<see cref="T:SkiaSharp.SKCanvas" />           <!-- Type -->
<see cref="M:SkiaSharp.SKCanvas.DrawRect" />  <!-- Method -->
<see cref="P:SkiaSharp.SKPaint.Color" />      <!-- Property -->
<see cref="F:SkiaSharp.SKColors.Red" />       <!-- Field -->
<paramref name="paint" />                      <!-- Parameter in same method -->
```

### Keywords

```xml
<see langword="true" />
<see langword="false" />
<see langword="null" />
```

### Escaping

| Character | Escape |
|-----------|--------|
| `<` | `&lt;` |
| `>` | `&gt;` |
| `&` | `&amp;` |

## Summary Patterns

**Key rules from Microsoft guidelines:**
- Begin with a present-tense, third-person verb
- Do NOT merely repeat the member name - provide meaningful context
- Use language-neutral text (no C#/VB-specific terms)
- Avoid parameter names or self-referential names in summaries

### Classes

```xml
<summary>Holds the style and color information about how to draw geometries, text and bitmaps.</summary>
```

### Constructors

Always open with the exact .NET phrase **"Initializes a new instance of the `<see cref>` class."** — use "struct" instead of "class" for value types. The type kind (class vs struct) is in the entry's `signature` field (e.g. `public readonly struct HBColor`). A shortened form like "Initializes a new `<see cref>` from a packed value" is a guideline violation.

```xml
<!-- Class constructor -->
<summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKPaint" /> class.</summary>

<!-- With parameters - describe what makes this overload different -->
<summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKBitmap" /> class with the specified dimensions.</summary>
<param name="width">The width of the bitmap.</param>
<param name="height">The height of the bitmap.</param>

<!-- Struct constructor (note "struct", not "class") -->
<summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKPoint" /> struct.</summary>

<!-- Abstract class constructor -->
<summary>Called from constructors in derived classes to initialize the <see cref="T:SkiaSharp.SKObject" /> class.</summary>

<!-- ❌ WRONG — omits "instance of the ... struct" (a real mistake from a past batch) -->
<summary>Initializes a new <see cref="T:HarfBuzzSharp.HBColor" /> from a packed BGRA value.</summary>
<!-- ✅ Right -->
<summary>Initializes a new instance of the <see cref="T:HarfBuzzSharp.HBColor" /> struct from a packed BGRA value.</summary>
```

### Properties

The opening verb is decided by the accessor, which is shown verbatim in the entry's `signature` field — read it, don't infer from the property's purpose. `{ get; set; }` → **"Gets or sets"**; `{ get; }` → **"Gets"**. Struct properties are an easy trap: many look read-only but are actually settable (e.g. every property on `SKFontVariationAxis` is `{ get; set; }`).

```xml
<!-- Read/write — signature: public SKColor Color { get; set; } -->
<summary>Gets or sets the color.</summary>
<value>The color value.</value>

<!-- Read-only — signature: public int Width { get; } (do NOT say "This property is read-only") -->
<summary>Gets the width of the bitmap.</summary>
<value>The width in pixels.</value>

<!-- Boolean read/write -->
<summary>Gets or sets a value indicating whether anti-aliasing is enabled.</summary>
<value><see langword="true" /> if anti-aliasing is enabled; otherwise, <see langword="false" />.</value>

<!-- Boolean read-only -->
<summary>Gets a value indicating whether the path is empty.</summary>
<value><see langword="true" /> if the path contains no lines or curves; otherwise, <see langword="false" />.</value>

<!-- ❌ WRONG — signature is { get; set; } but summary says only "Gets" -->
<summary>Gets the variation axis minimum value.</summary>   <!-- should be "Gets or sets" -->
```

### Methods

```xml
<!-- General method - begin with present-tense third-person verb -->
<summary>Draws a rectangle using the specified paint.</summary>
<param name="rect">The rectangle to draw.</param>
<param name="paint">The paint to use.</param>

<!-- Async method -->
<summary>Asynchronously encodes the image to the specified format.</summary>

<!-- Factory method -->
<summary>Creates a new image from encoded data.</summary>
<param name="data">The encoded image data.</param>
<returns>A new image, or <see langword="null" /> if the data is invalid.</returns>

<!-- Try pattern -->
<summary>Attempts to parse the color from a string.</summary>
<param name="value">The string to parse.</param>
<param name="color">When this method returns, contains the parsed color if successful. This parameter is treated as uninitialized.</param>
<returns><see langword="true" /> if the parsing succeeded; otherwise, <see langword="false" />.</returns>

<!-- Dispose() -->
<summary>Releases the resources used by the current instance of the <see cref="T:SkiaSharp.SKPaint" /> class.</summary>

<!-- Dispose(Boolean) -->
<summary>Called by the <see cref="M:SkiaSharp.SKPaint.Dispose" /> and <see cref="M:System.Object.Finalize" /> methods to release the managed and unmanaged resources used by the current instance of the <see cref="T:SkiaSharp.SKPaint" /> class.</summary>
```

### Events

```xml
<summary>Occurs when the surface needs to be repainted.</summary>
```

### Enums

```xml
<!-- Type -->
<summary>Specifies the blend mode for drawing operations.</summary>

<!-- Members - no verb needed -->
<summary>Source pixels replace destination pixels.</summary>
<summary>Source and destination pixels are blended.</summary>
```

### Parameters

**Key rules:**
- Noun phrase without specifying the data type
- Begin with an article (The, A, An)
- Do NOT use "true if..." for booleans - use "true to..."

```xml
<!-- Class/struct parameter -->
<param name="rect">The rectangle to draw.</param>

<!-- Boolean parameter: "true to...", NOT "true if..." -->
<param name="antialias"><see langword="true" /> to enable anti-aliasing; otherwise, <see langword="false" />.</param>

<!-- Enum parameter -->
<param name="blendMode">One of the enumeration values that specifies the blend mode.</param>

<!-- Flag enum parameter -->
<param name="flags">A bitwise combination of the enumeration values that specifies the options.</param>

<!-- out parameter -->
<param name="result">When this method returns, contains the parsed value if successful. This parameter is treated as uninitialized.</param>
```

### Return Values

**Key rules:**
- Noun phrase without specifying the data type
- For booleans: "true if...", NOT "true to..."

```xml
<!-- Object -->
<returns>A new image.</returns>

<!-- Nullable -->
<returns>A new image, or <see langword="null" /> if the data is invalid.</returns>

<!-- Boolean: "true if...", NOT "true to..." -->
<returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" />.</returns>

<!-- Enum -->
<returns>One of the enumeration values that indicates the result.</returns>
```

### Exceptions

```xml
<exception cref="T:System.ArgumentNullException"><paramref name="paint" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException"><paramref name="width" /> is less than zero.</exception>
```

### Internal APIs

```xml
<summary>This member supports the SkiaSharp infrastructure and is not intended to be used directly from your code.</summary>
```

## Punctuation Exceptions

**Do NOT add trailing period after:**

```xml
<!-- Bracket annotations (category labels) -->
<summary>Darkens the backdrop color to reflect the source color. [Separable Blend Modes]</summary>

<!-- Status parentheticals -->
<summary>Use the Vulkan 3D backend (not yet supported)</summary>

<!-- Technical notation -->
<summary>Swizzles pixels, swapping R and B (RGBA ↔ BGRA)</summary>
```

**DO use period after clarifying parentheticals:**

```xml
<summary>Gets the Euclidean distance from the origin (0, 0).</summary>
<summary>Clamps values to the range [0..1].</summary>
```

## Common Mistakes

```xml
<!-- ❌ Trailing space -->
<returns>The result. </returns>

<!-- ✅ Correct -->
<returns>The result.</returns>

<!-- ❌ Space before period -->
<returns>Returns <see langword="true" /> .</returns>

<!-- ✅ Correct -->
<returns>Returns <see langword="true" />.</returns>

<!-- ❌ Backtick keywords -->
This may return `null` if not found.

<!-- ✅ Use langword -->
This may return <see langword="null" /> if not found.

<!-- ❌ Boolean param with "true if" -->
<param name="enable"><see langword="true" /> if enabling; otherwise, <see langword="false" />.</param>

<!-- ✅ Boolean param with "true to" -->
<param name="enable"><see langword="true" /> to enable; otherwise, <see langword="false" />.</param>

<!-- ❌ Just repeating the name -->
<summary>Gets the width.</summary>

<!-- ✅ Meaningful description -->
<summary>Gets the width of the bitmap in pixels.</summary>
```

## Extension Methods

Extension method docs appear in two places:
1. **Type file** (e.g., `SkiaSharp/SKCanvasExtensions.xml`) - **Edit this one**
2. **index.xml** - Auto-synced by `docs-format-docs`, don't edit manually

## Platform View Constructors

Native platform views have special constructors with specific purposes. Always include a `<remarks>` tag explaining when/why each constructor is called.

### Android Views

```xml
<!-- Default constructor - used when creating programmatically -->
<summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Android.SKCanvasView" /> class.</summary>
<remarks>Use this constructor when creating the view programmatically from code.</remarks>

<!-- XML inflation constructor -->
<summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Android.SKCanvasView" /> class with the specified XML attributes.</summary>
<remarks>This constructor is called when inflating the view from an Android XML layout file.</remarks>

<!-- XML inflation with style constructor -->
<summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Android.SKCanvasView" /> class with the specified XML attributes and style.</summary>
<remarks>This constructor is called when inflating the view from an Android XML layout file and applying a class-specific base style from a theme attribute.</remarks>

<!-- JNI constructor - runtime use only -->
<summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Android.SKCanvasView" /> class from a JNI object reference.</summary>
<remarks>This constructor is used by the Xamarin.Android runtime when creating managed representations of JNI objects. It is not intended to be called directly from user code.</remarks>
```

### iOS/tvOS/Mac Views

```xml
<!-- Default constructor -->
<summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.iOS.SKCanvasView" /> class.</summary>
<remarks />

<!-- Frame constructor -->
<summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.iOS.SKCanvasView" /> class with the specified frame.</summary>
<remarks />

<!-- Native handle constructor - runtime use only -->
<summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.iOS.SKCanvasView" /> class from a native handle.</summary>
<remarks>This constructor is used by the Xamarin.iOS runtime when creating managed representations of unmanaged objects. It is not intended to be called directly from user code.</remarks>
```

### Tizen Views

```xml
<!-- Default constructor -->
<summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Tizen.SKCanvasView" /> class.</summary>
<remarks>Use this constructor when creating the view programmatically from code.</remarks>
```

### Cross-Platform Views (Forms/MAUI/Blazor/WPF/Desktop)

```xml
<!-- Simple default constructor - no special remarks needed -->
<summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Forms.SKCanvasView" /> class.</summary>
<remarks />
```

## Rich Remarks and Examples

The best SkiaSharp docs use markdown-in-CDATA for rich content. This is the convention for type-level remarks and important factory methods.

### When to Write Rich Remarks

| API type | Remarks level |
|----------|--------------|
| Types (classes/structs) | Rich: overview, usage example, disposal notes, threading |
| Factory methods (`Create*`) | Rich: code example showing common usage |
| Important methods (`Draw*`, `Save`/`Restore`) | Brief explanation + link to related concepts |
| Simple properties, getters, overloads | `<remarks />` (empty) is fine |
| Enum members | No remarks needed |

### Rich Remarks Format (Markdown in CDATA)

For types and important methods, wrap markdown content in `<format type="text/markdown"><![CDATA[...]]></format>`. Microsoft Learn renders the CDATA content as full markdown.

Example remarks value for a type:

````xml
<format type="text/markdown"><![CDATA[
## Remarks

`SKPaint` controls how drawing operations render on the canvas, including
color, stroke width, anti-aliasing, blend modes, shaders, and text properties.
Create an instance, configure the desired properties, and pass it to drawing
methods on `SKCanvas`.

This type wraps a native Skia resource and implements `IDisposable`. Always
dispose of it when done, either with a `using` statement or by calling
`Dispose()` directly.

## Examples

```csharp
using var paint = new SKPaint
{
    Color = SKColors.CornflowerBlue,
    IsAntialias = true,
    Style = SKPaintStyle.Fill,
};
canvas.DrawCircle(128, 128, 80, paint);
```
]]></format>
````

### Type-Level Remarks Template

For classes and structs, follow this structure inside the CDATA:

1. `## Remarks` heading
2. One paragraph: what the type does and when to use it
3. Optional: how to create instances — constructor vs factory methods
4. Optional: disposal pattern for `SKObject` subclasses
5. Optional: threading notes — Skia is NOT thread-safe for mutable types
6. `## Examples` heading
7. One ` ```csharp ``` ` block showing the most common usage pattern

### What Makes Good Examples

- **Show the most common use case** — not edge cases
- **Include using/disposal** — SkiaSharp objects are IDisposable (but never `using` a parent-owned object like the canvas from `SKDocument.BeginPage`)
- **Show the full picture** — create + configure + use, not just one call
- **Keep it short** — 5-15 lines, enough to understand the pattern
- **Use realistic values** — not `0, 0, 0, 0` but actual coordinates/colors
- **Be self-contained** — every variable referenced must be declared in the snippet; a stray `bitmap2` that was never created won't compile

Look at `samples/Gallery/Shared/Samples/` for real usage patterns. These samples show how developers actually use the APIs.

### Cross-References in Rich Remarks

Inside CDATA blocks, use `<xref:...>` (NOT `<see cref>`). An `xref` takes the **bare DocFX UID** — the fully-qualified name with **no `T:`/`M:`/`P:`/`F:` prefix**. Those DocId prefixes belong only to `<see cref>`; carrying them into an `xref` produces a broken link.

```
✅ See <xref:SkiaSharp.SKCanvas> for drawing operations.
✅ Use <xref:SkiaSharp.SKPathBuilder> to construct paths incrementally.
✅ Call <xref:SkiaSharp.SKPathBuilder.MoveTo(System.Single,System.Single)> to start a contour.

❌ See <xref:T:SkiaSharp.SKCanvas> ...      (drop the T:)
❌ Use <xref:P:SkiaSharp.SKPaint.Color> ... (drop the P:)
```

Outside CDATA (in summary/param/returns), use `<see cref="T:..." />` **with** the prefix as usual. So the same type is `<see cref="T:SkiaSharp.SKCanvas" />` in a summary but `<xref:SkiaSharp.SKCanvas>` in a remarks CDATA block.

## Type-Level Documentation

Types that wrap native resources (`IDisposable`) should have remarks that cover:
1. What the type does and when to use it
2. How to create instances (constructor vs factory)
3. Disposal pattern — always show `using` in examples
4. Threading constraints if applicable

**remarks** should use the CDATA format shown above with `## Remarks`, disposal note, and `## Examples` with a code block.

### Code Example Best Practices

- **Show disposal** — if the type is `IDisposable`, examples must use `using` or call `Dispose()` — *except* for objects owned by a parent (e.g. the canvas from `SKDocument.BeginPage` or `SKSurface.Canvas`), which the parent disposes
- **Show the full picture** — create + configure + use, not just one call
- **Use realistic values** — not `0, 0, 0, 0` but actual coordinates/colors
- **Keep it short** — 5-15 lines, enough to understand the pattern
- **Be self-contained and compilable** — every variable referenced must be declared in the snippet; a stray identifier (e.g. `bitmap2` when only `bitmap` was created) is a compile error
- **Only use real APIs** — verify every method/overload exists in source before using in an example
- **Never use obsolete APIs** — a member marked `[Obsolete("...", true)]` is a compile error, so an example using it is broken. The extract JSON flags these in an `obsolete` field; the message names the replacement. The classic SkiaSharp trap is legacy text rendering (see skia-patterns.md "Obsolete APIs").
