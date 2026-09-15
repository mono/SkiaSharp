# C# XML documentation patterns

Use C# `///` comments immediately above the public declaration they document.
Managed compilation generates compiler XML. Use standard compiler XML elements
for concise documentation. Existing source comments also use the supported rich
form `<format type="text/markdown"><![CDATA[...]]></format>` in `<remarks>`;
preserve that form, its bare DocFX `<xref:...>` links, and `_DocsMedia` image
references unless an equivalent external-rendering migration is validated. For
SkiaSharp/HarfBuzzSharp facts, read
[`skia-patterns.md`](skia-patterns.md).

These patterns follow the [official .NET API documentation
guidelines](https://github.com/dotnet/dotnet-api-docs/wiki).

## Contents

- [Syntax reference](#syntax-reference)
- [Summary and declaration patterns](#summary-and-declaration-patterns)
- [Parameters, returns, values, and exceptions](#parameters-returns-values-and-exceptions)
- [Cross-references and escaping](#cross-references-and-escaping)
- [Punctuation and common mistakes](#punctuation-and-common-mistakes)
- [Extension methods](#extension-methods)
- [Platform view constructors](#platform-view-constructors)
- [Rich remarks and examples](#rich-remarks-and-examples)
- [Type-level documentation](#type-level-documentation)

## Syntax reference

Put the `///` block directly above the declaration, with no unrelated
attribute, directive, or declaration between it and the documented member.

```csharp
/// <summary>Creates an image from the specified encoded data.</summary>
/// <param name="data">The encoded image data.</param>
/// <returns>
/// A new image, or <see langword="null" /> if <paramref name="data" /> is
/// invalid.
/// </returns>
/// <exception cref="ArgumentNullException">
/// <paramref name="data" /> is <see langword="null" />.
/// </exception>
public static SKImage FromEncodedData(SKData data)
```

| Element | Use |
|---|---|
| `<summary>` | Concise description of every public type/member. |
| `<param name="…">` | Description for a method or constructor parameter. |
| `<typeparam name="…">` | Description for a generic type or method parameter. |
| `<returns>` | Meaning and nullable/failure condition of a return value. |
| `<value>` | Value and state of a property or indexer. |
| `<exception cref="…">` | An exception actually thrown for a documented condition. |
| `<remarks>` | Important context, lifetime/threading constraints, or extended usage. |
| `<example>` | A short compilable usage example; `<code>` can contain source. |
| `<see cref="…">`, `<seealso cref="…">` | Compiler-resolved type/member links. |
| `<paramref name="…">`, `<typeparamref name="…">` | A parameter or generic parameter in prose. |
| `<c>…</c>` / `<code>…</code>` | Inline code or a code block. |
| `<inheritdoc />` | Inherit a suitable base/interface contract when it is the same API contract. |

Use a self-closing element when it has no content: `<inheritdoc />` and
`<see langword="null" />`. Do not add empty summary, parameter, return, value,
or exception elements merely to make a list look complete.

## Summary and declaration patterns

Summaries are complete, concise sentences. Methods normally begin with a
present-tense third-person verb; type and enum summaries state what the type
represents. Add real context rather than repeating the identifier.

### Types and enums

```csharp
/// <summary>Represents a two-dimensional point using single-precision values.</summary>
public struct SKPoint

/// <summary>Specifies the blend mode for drawing operations.</summary>
public enum SKBlendMode

/// <summary>Source pixels replace destination pixels.</summary>
Src
```

### Constructors

Use the full constructor form, choosing `class` or `struct` correctly. Explain
what distinguishes overloads.

```csharp
/// <summary>Initializes a new instance of the <see cref="SKPaint" /> class.</summary>
public SKPaint()

/// <summary>
/// Creates a bitmap with the specified dimensions and opacity.
/// </summary>
/// <param name="width">The desired width in pixels.</param>
/// <param name="height">The desired height in pixels.</param>
/// <param name="isOpaque">
/// <see langword="true" /> to create an opaque bitmap; otherwise,
/// <see langword="false" />.
/// </param>
public SKBitmap(int width, int height, bool isOpaque = false)

/// <summary>Initializes a new instance of the <see cref="SKPoint" /> struct.</summary>
/// <param name="x">The horizontal position of the point.</param>
/// <param name="y">The vertical position of the point.</param>
public SKPoint(float x, float y)

/// <summary>
/// Called from derived-class constructors to initialize the
/// <see cref="SKDrawable" /> class.
/// </summary>
protected SKDrawable()
```

### Properties, indexers, fields, events, and methods

Inspect accessors rather than inferring them. Getter-only properties start
“Gets”; writable properties start “Gets or sets.” Boolean properties use
“Gets [or sets] a value indicating whether…”.

```csharp
/// <summary>Gets or sets the color used for drawing.</summary>
/// <value>The drawing color.</value>
public SKColor Color { get; set; }

/// <summary>Gets the width of the bitmap in pixels.</summary>
/// <value>The width in pixels.</value>
public int Width { get; }

/// <summary>Gets a value indicating whether the path is empty.</summary>
/// <value>
/// <see langword="true" /> if the path contains no lines or curves; otherwise,
/// <see langword="false" />.
/// </value>
public bool IsEmpty { get; }

/// <summary>Occurs when the surface needs to be repainted.</summary>
public event EventHandler<SKPaintSurfaceEventArgs>? PaintSurface;

/// <summary>Draws a rectangle using the specified paint.</summary>
/// <param name="rect">The rectangle to draw.</param>
/// <param name="paint">The paint to use for drawing.</param>
public void DrawRect(SKRect rect, SKPaint paint)
```

For an `init` accessor, say it can be set during initialization if that
distinction matters. Do not call it read-only simply because it cannot be
assigned later.

## Parameters, returns, values, and exceptions

Use an article and a noun phrase for ordinary parameter and return
descriptions. Parameter names in `name` attributes must exactly match the
declaration. Use `paramref` when mentioning them in prose.

```csharp
/// <summary>Draws a rectangle using the specified paint.</summary>
/// <param name="rect">The rectangle to draw.</param>
/// <param name="paint">The paint to use for drawing.</param>
public void DrawRect(SKRect rect, SKPaint paint)

/// <summary>Creates an iterator that scans the path's segments.</summary>
/// <param name="forceClose">
/// <see langword="true" /> to close each contour while iterating; otherwise,
/// <see langword="false" />.
/// </param>
/// <returns>An iterator for the path's segments.</returns>
public Iterator CreateIterator(bool forceClose)

/// <summary>Attempts to parse a color from a string.</summary>
/// <param name="hexString">The hexadecimal color string to parse.</param>
/// <param name="color">
/// When this method returns, contains the parsed color if successful;
/// otherwise, <see cref="SKColor.Empty" />.
/// </param>
/// <returns>
/// <see langword="true" /> if parsing succeeded; otherwise,
/// <see langword="false" />.
/// </returns>
public static bool TryParse(string hexString, out SKColor color)

/// <summary>Gets an image from the encoded data.</summary>
/// <param name="data">The encoded image data.</param>
/// <returns>
/// A new image, or <see langword="null" /> if the data is invalid.
/// </returns>
public static SKImage FromEncodedData(SKData data)
```

- Boolean **parameters** say “`true` to…”.
- Boolean **returns** and property **values** say “`true` if…”.
- Use `<see langword="null" />`, `<see langword="true" />`, and
  `<see langword="false" />`, not backticks or bare keywords.
- Describe `out`/`ref` behavior precisely. Do not claim a constraint, default,
  or failure result that source does not establish.
- Use `<value>` for a property/indexer value. Do not document a default based
  on a typical constant; verify its initializer or zero-initialized state.

Document only exceptions that are part of the observed public behavior, on the
same declaration that performs the validation. Never copy an exception element
into an unrelated member merely to demonstrate XML syntax.

For generic APIs, use `typeparam` and `typeparamref`:

```csharp
/// <summary>Provides event data for retrieving a property value from a renderer.</summary>
/// <typeparam name="T">The type of the property value to retrieve.</typeparam>
public class GetPropertyValueEventArgs<T> : EventArgs
{
    /// <summary>Gets or sets the property value.</summary>
    /// <value>The property value of type <typeparamref name="T" />.</value>
    public T Value { get; set; }
}
```

## Cross-references and escaping

Use the repository's compiler-supported DocId convention for standard XML
`cref` values, such as `T:`, `M:`, `P:`, and `F:` prefixes. In a rich
Markdown/CDATA `<remarks>` block, use the external renderer's bare
`<xref:...>` syntax instead. Both forms are already authoritative source
comment formats; do not rewrite one into the other without validating the
external rendering result.

```csharp
/// <summary>Draws to an <see cref="T:SkiaSharp.SKCanvas" />.</summary>
/// <seealso cref="T:SkiaSharp.SKPaint" />
```

For overload disambiguation, use the fully-qualified DocId and build to
confirm it:

```csharp
/// <seealso cref="M:SkiaSharp.SKCanvas.DrawRect(SkiaSharp.SKRect,SkiaSharp.SKPaint)" />
```

Escape XML metacharacters in prose and code:

| Character | Write |
|---|---|
| `<` | `&lt;` |
| `>` | `&gt;` |
| `&` | `&amp;` |

For a code comparison, prefer `<c>value &lt; limit</c>`. Never place
unescaped markup-looking text in a summary. In existing rich Markdown/CDATA
source comments, preserve bare `<xref:...>` cross-references and renderer
syntax because the external API-docs repository consumes that compiler XML.

## Punctuation and common mistakes

Use normal sentence punctuation and no trailing whitespace. Keep punctuation
outside closing XML tags and after inline elements:

```csharp
/// <returns>The result.</returns>
```

```csharp
/// <returns><see langword="true" /> if successful; otherwise, <see langword="false" />.</returns>
```

Avoid a trailing period only when the summary deliberately ends in a bracket
annotation, status parenthetical, or technical notation that is not a
sentence. A clarifying parenthetical in a sentence still takes a period:

```csharp
/// <summary>Gets the Euclidean distance from the origin (0, 0).</summary>
```

```csharp
/// <summary>Swizzles pixels, swapping R and B (RGBA ↔ BGRA)</summary>
```

Common mistakes:

- `/// <summary>Gets the width.</summary>` merely repeats a name; add context
  such as the object and unit.
- “Gets” on `{ get; set; }`, “Gets or sets” on `{ get; }`, or calling an
  `init` accessor immutable.
- A shortened constructor phrase such as “Initializes a new `SKPoint`…”
  instead of “Initializes a new instance of the … struct…”.
- “`true` if” for an input parameter, or “`true` to” for a result/value.
- Backticked `null`, `true`, or `false` instead of `langword`.
- A parameter `name`, `paramref`, `typeparam`, or `cref` that does not resolve.
- Unsupported claims that a method must reject input, has a default, or owns a
  returned object without reading implementation.
- Empty boilerplate tags, an `inheritdoc` used where the inherited contract
  differs, or standard and rich cross-reference formats mixed in the same
  context. Preserve existing valid DocIds and rich Markdown/CDATA comments.

## Extension methods

Document an extension method at its C# declaration, including the `this`
parameter when its purpose is not obvious. The extension container type needs
its own summary but does not duplicate member documentation in generated
artifacts.

```csharp
/// <summary>Draws shaped text on the canvas at the specified point with the specified alignment.</summary>
/// <param name="canvas">The canvas to draw on.</param>
/// <param name="text">The text to draw.</param>
/// <param name="p">The point at which to draw the text.</param>
/// <param name="textAlign">The text alignment to use when drawing the text.</param>
/// <param name="font">The font to use when shaping and drawing the text.</param>
/// <param name="paint">The paint to use when drawing the text.</param>
public static void DrawShapedText(this SKCanvas canvas, string text, SKPoint p, SKTextAlign textAlign, SKFont font, SKPaint paint)
```

Check the extension's receiver, overload, nullability, and ownership against
the implementation just as for an instance method. Do not maintain a second
copy of its prose in compiler XML or external generated reference output.

## Platform view constructors

Platform view constructors have runtime-specific purposes. Document them as
C# source comments beside the actual constructor. Preserve exact type names
and parameter names from source; the following patterns show the intended
remarks.

### Android

```csharp
/// <summary>
/// Initializes a new instance of the <see cref="SKCanvasView" /> class.
/// </summary>
/// <param name="context">The context in which the view runs.</param>
/// <remarks>Use this constructor when creating the view programmatically.</remarks>
public SKCanvasView(Context context)

/// <summary>
/// Initializes a new instance of the <see cref="SKCanvasView" /> class with
/// the specified XML attributes.
/// </summary>
/// <param name="context">The context in which the view runs.</param>
/// <param name="attrs">The XML attributes used to inflate the view.</param>
/// <remarks>
/// This constructor is called when inflating the view from an Android XML
/// layout file.
/// </remarks>
public SKCanvasView(Context context, IAttributeSet attrs)

/// <summary>
/// Initializes a new instance of the <see cref="SKCanvasView" /> class from a
/// JNI object reference.
/// </summary>
/// <param name="javaReference">The JNI object reference.</param>
/// <param name="transfer">The ownership mode for the Java reference.</param>
/// <remarks>
/// This constructor is used by the Android runtime when creating managed
/// representations of JNI objects. It is not intended for direct user code.
/// </remarks>
protected SKCanvasView(IntPtr javaReference, JniHandleOwnership transfer)
```

### iOS, tvOS, and Mac

```csharp
/// <summary>
/// Initializes a new instance of the <see cref="SKCanvasView" /> class with
/// the specified frame.
/// </summary>
/// <param name="frame">The frame used by the view, expressed in points.</param>
public SKCanvasView(CGRect frame)

/// <summary>
/// Initializes a new instance of the <see cref="SKCanvasView" /> class from a
/// native handle.
/// </summary>
/// <param name="p">The pointer to the unmanaged object.</param>
/// <remarks>
/// This constructor is used by the Apple runtime when creating managed
/// representations of unmanaged objects. It is not intended for direct user
/// code.
/// </remarks>
public SKCanvasView(IntPtr p)
```

### Tizen and cross-platform views

```csharp
/// <summary>
/// Initializes a new instance of the <see cref="SKCanvasView" /> class.
/// </summary>
/// <remarks>Use this constructor when creating the view programmatically.</remarks>
public SKCanvasView()
```

For Forms, MAUI, Blazor, WPF, and desktop views, a plain default-constructor
summary is normally enough unless source gives it a platform/runtime role.
Never copy a platform-specific runtime assertion to another view without
checking its source.

## Rich remarks and examples

Use `<remarks>` for non-obvious behavior and `<example>` for an important
usage pattern. Existing rich source comments use a supported Markdown/CDATA
format inside `<remarks>`; preserve that format when updating rich prose,
examples, cross-references, or `_DocsMedia` images.

```csharp
/// <summary>Provides the style and color information for drawing operations.</summary>
/// <remarks><format type="text/markdown"><![CDATA[
/// ## Remarks
///
/// Configure an instance and pass it to drawing operations on
/// <xref:SkiaSharp.SKCanvas>. Dispose caller-owned instances when they are no
/// longer needed.
///
/// ## Examples
///
/// ```csharp
/// using var bitmap = new SKBitmap(256, 256);
/// using var canvas = new SKCanvas(bitmap);
/// using var paint = new SKPaint
/// {
///     Color = SKColors.CornflowerBlue,
///     IsAntialias = true,
///     Style = SKPaintStyle.Fill,
/// };
/// canvas.DrawCircle(128, 128, 80, paint);
/// ```
/// ]]></format></remarks>
public class SKPaint
```

Use rich remarks for types, important factory methods, and operations whose
lifetime, threading, interoperability, or order of use is significant. Simple
properties and overloads rarely need an example. A type-level example should
show the normal creation/configuration/use path, not an edge case.

Before publishing an example:

1. Read the actual C# declaration and implementation for each call and check
   that its exact overload exists.
2. Declare every identifier or state the precondition in the surrounding
   comments. Do not reference an undeclared `bitmap2`, assumed `canvas`, or
   unstated receiver.
3. Handle nullable factory results before dereference when source can return
   `null`.
4. Use `using` only for objects the caller owns. Do not dispose the canvas
   returned from `SKDocument.BeginPage` or `SKSurface.Canvas`; their parent
   owns it.
5. Use current APIs. In particular, text examples use `SKFont` and the
   `SKCanvas` overload that accepts it, never an error-obsolete `SKPaint` text
   API. See [`obsolete-api-map.md`](obsolete-api-map.md).
6. State disposal and threading rules only after verifying the type category
   and ownership in [`skia-patterns.md`](skia-patterns.md) and source.

Prefer real, current patterns from `samples/` only after validating their
signatures against the current declaration. An example is public API guidance:
it must be self-contained, factual, and compilable.

## Type-level documentation

Public types need a meaningful `<summary>` and, when the type has non-obvious
creation, lifetime, threading, or interoperability requirements, a
`<remarks>` section and a short `<example>`. Keep details on the type where
they apply broadly; member comments should focus on the member's own behavior.

For a type wrapping a caller-owned native resource, explain the disposal
contract and show disposal in a type-level example. For a parent-owned
resource, say that its parent controls its lifetime and do not show
`using`/`Dispose`. For mutable Skia objects, document a threading restriction
only after confirming it in source and [`skia-patterns.md`](skia-patterns.md).
Value types normally need neither disposal nor threading guidance.
