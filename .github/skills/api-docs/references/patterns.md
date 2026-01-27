# XML Documentation Patterns

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

```xml
<!-- Class constructor -->
<summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKPaint" /> class.</summary>

<!-- With parameters - describe what makes this overload different -->
<summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKBitmap" /> class with the specified dimensions.</summary>
<param name="width">The width of the bitmap.</param>
<param name="height">The height of the bitmap.</param>

<!-- Struct constructor -->
<summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKPoint" /> struct.</summary>

<!-- Abstract class constructor -->
<summary>Called from constructors in derived classes to initialize the <see cref="T:SkiaSharp.SKObject" /> class.</summary>
```

### Properties

```xml
<!-- Read/write -->
<summary>Gets or sets the color.</summary>
<value>The color value.</value>

<!-- Read-only (do NOT say "This property is read-only") -->
<summary>Gets the width of the bitmap.</summary>
<value>The width in pixels.</value>

<!-- Boolean read/write -->
<summary>Gets or sets a value indicating whether anti-aliasing is enabled.</summary>
<value><see langword="true" /> if anti-aliasing is enabled; otherwise, <see langword="false" />.</value>

<!-- Boolean read-only -->
<summary>Gets a value indicating whether the path is empty.</summary>
<value><see langword="true" /> if the path contains no lines or curves; otherwise, <see langword="false" />.</value>
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
