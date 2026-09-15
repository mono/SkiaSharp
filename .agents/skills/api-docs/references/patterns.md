# C# XML documentation patterns

Use C# `///` comments immediately above public declarations. The compiler
generates XML from these comments during managed builds.

```csharp
/// <summary>Creates an image from the specified encoded data.</summary>
/// <param name="data">The encoded image data.</param>
/// <returns>A new image, or <see langword="null" /> if the data is invalid.</returns>
/// <exception cref="ArgumentNullException"><paramref name="data" /> is <see langword="null" />.</exception>
public static SKImage? FromEncodedData(SKData data)
```

## Conventions

- Start type/member summaries with a concise present-tense description:
  methods use a third-person verb; read-only properties use “Gets”; read/write
  properties use “Gets or sets”.
- Constructor summaries use “Initializes a new instance of the … class” (or
  “struct”), with a short distinction for overloads.
- Use `<paramref>` for parameters and `<see langword="null" />`,
  `<see langword="true" />`, and `<see langword="false" />` for language
  keywords. Boolean parameters use “true to”; boolean returns and values use
  “true if”.
- Use compiler-resolved `cref` values, such as
  `<see cref="SKCanvas" />` or `<see cref="ArgumentNullException" />`;
  do not write ECMA DocId prefixes or DocFX-only `xref` syntax in source
  comments.
- Include `<param>`, `<returns>`, `<value>`, `<typeparam>`, and `<exception>`
  when they clarify public behavior. Do not state an unverified default or
  constraint.

## Remarks and examples

Use `<remarks>` for important lifetime, threading, interoperability, or usage
context. Examples must be self-contained, use current APIs, and dispose only
objects the caller owns. Prefer real patterns from `samples/` after checking
the exact overloads in source.

The external documentation repository controls ECMA-specific formatting and
presentation after it consumes compiler XML; do not embed repository-specific
output conventions in source comments.
