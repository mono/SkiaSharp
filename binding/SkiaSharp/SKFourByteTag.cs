using System;

namespace SkiaSharp;

/// <summary>Represents a four-byte OpenType tag used to identify tables, axes, and scripts.</summary>
/// <remarks><![CDATA[
/// ## Remarks
///
/// `SKFourByteTag` is a read-only struct that wraps a 32-bit unsigned integer whose four bytes correspond to four ASCII characters. This matches the `SkFourByteTag` representation used throughout the Skia/OpenType APIs for table identifiers (e.g., `cmap`, `GSUB`) and variation axis tags (e.g., `wght`, `wdth`).
///
/// When constructed from four `char` values, the first character (`c1`) occupies the high byte and the last (`c4`) the low byte: `(c1 << 24) | (c2 << 16) | (c3 << 8) | c4`.
///
/// `Parse` accepts a string of 1–4 ASCII characters, right-padding with spaces if shorter than 4 characters.
///
/// `ToString` returns the four-character ASCII string representation.
///
/// ## Examples
///
/// Creating a tag from a string and from individual characters:
///
/// ```csharp
/// var wghtFromString = SKFourByteTag.Parse("wght");
/// var wghtFromChars  = new SKFourByteTag('w', 'g', 'h', 't');
/// Console.WriteLine(wghtFromString == wghtFromChars); // True
/// Console.WriteLine(wghtFromString);                  // wght
/// ```
/// ]]></remarks>
public readonly struct SKFourByteTag : IEquatable<SKFourByteTag>
{
	private readonly uint value;

	/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKFourByteTag" /> struct from a raw 32-bit value.</summary>
	/// <param name="value">The raw 32-bit packed tag value.</param>
	/// <remarks />
	public SKFourByteTag (uint value)
	{
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKFourByteTag" /> struct from four ASCII characters.</summary>
	/// <param name="c1">The first (most significant) character of the tag.</param>
	/// <param name="c2">The second character of the tag.</param>
	/// <param name="c3">The third character of the tag.</param>
	/// <param name="c4">The fourth (least significant) character of the tag.</param>
	/// <remarks />
	public SKFourByteTag (char c1, char c2, char c3, char c4)
	{
		value = (uint)(((byte)c1 << 24) | ((byte)c2 << 16) | ((byte)c3 << 8) | (byte)c4);
	}

	/// <summary>Parses a string of up to four ASCII characters into an <see cref="T:SkiaSharp.SKFourByteTag" />.</summary>
	/// <param name="tag">A string of up to four ASCII characters. Shorter strings are padded with spaces on the right; longer strings are truncated.</param>
	/// <returns>An <see cref="T:SkiaSharp.SKFourByteTag" /> representing the four-byte packed tag.</returns>
	/// <remarks />
	public static SKFourByteTag Parse (string? tag) =>
		Parse (tag.AsSpan ());

	/// <summary>Parses a span of up to four ASCII characters into an <see cref="T:SkiaSharp.SKFourByteTag" />.</summary>
	/// <param name="tag">A span of up to four ASCII characters. Shorter spans are padded with spaces on the right; longer spans are truncated.</param>
	/// <returns>An <see cref="T:SkiaSharp.SKFourByteTag" /> representing the four-byte packed tag.</returns>
	/// <remarks />
	public static SKFourByteTag Parse (ReadOnlySpan<char> tag)
	{
		if (tag.IsEmpty)
			return new SKFourByteTag (0);

		// Take up to the first four characters, padding any missing trailing
		// slots with spaces — matching the original char[4]-scratch behaviour
		// without allocating the scratch array. The first character always
		// exists here because empty input was handled by the guard above.
		var c1 = tag[0];
		var c2 = tag.Length > 1 ? tag[1] : ' ';
		var c3 = tag.Length > 2 ? tag[2] : ' ';
		var c4 = tag.Length > 3 ? tag[3] : ' ';

		return new SKFourByteTag (c1, c2, c3, c4);
	}

	/// <summary>Returns the four-character ASCII string representation of this tag.</summary>
	/// <returns>A four-character ASCII string representation of this tag.</returns>
	/// <remarks />
	public override unsafe string ToString ()
	{
		// Build the 4-character string directly from a stack buffer. Passing four
		// chars to string.Concat binds to Concat(object, object, object, object),
		// which boxes every char (four extra allocations per call); writing into a
		// stackalloc'd buffer avoids that while producing the identical string.
		char* chars = stackalloc char[4];
		chars[0] = (char)(byte)(value >> 24);
		chars[1] = (char)(byte)(value >> 16);
		chars[2] = (char)(byte)(value >> 8);
		chars[3] = (char)(byte)value;
		return new string (chars, 0, 4);
	}

	/// <summary>Implicitly converts an <see cref="T:SkiaSharp.SKFourByteTag" /> to its underlying 32-bit unsigned integer value.</summary>
	/// <param name="tag">The <see cref="T:SkiaSharp.SKFourByteTag" /> to convert.</param>
	/// <returns>The underlying 32-bit packed value of the tag.</returns>
	/// <remarks />
	public static implicit operator uint (SKFourByteTag tag) => tag.value;

	/// <summary>Implicitly converts a 32-bit unsigned integer to an <see cref="T:SkiaSharp.SKFourByteTag" />.</summary>
	/// <param name="tag">The raw 32-bit packed value to store.</param>
	/// <returns>An <see cref="T:SkiaSharp.SKFourByteTag" /> wrapping the specified value.</returns>
	/// <remarks />
	public static implicit operator SKFourByteTag (uint tag) => new SKFourByteTag (tag);

	/// <summary>Indicates whether this tag is equal to the specified object.</summary>
	/// <param name="obj">The object to compare with this instance.</param>
	/// <returns><see langword="true" /> if <paramref name="obj" /> is an <see cref="T:SkiaSharp.SKFourByteTag" /> with the same packed value; otherwise, <see langword="false" />.</returns>
	/// <remarks />
	public override bool Equals (object? obj) =>
		obj is SKFourByteTag tag && value.Equals (tag.value);

	/// <summary>Indicates whether this tag is equal to another <see cref="T:SkiaSharp.SKFourByteTag" />.</summary>
	/// <param name="other">The <see cref="T:SkiaSharp.SKFourByteTag" /> to compare with this instance.</param>
	/// <returns><see langword="true" /> if both tags have the same packed value; otherwise, <see langword="false" />.</returns>
	/// <remarks />
	public bool Equals (SKFourByteTag other) => value == other.value;

	/// <summary>Returns a hash code for this tag.</summary>
	/// <returns>A hash code for this <see cref="T:SkiaSharp.SKFourByteTag" /> instance.</returns>
	/// <remarks />
	public override int GetHashCode () => (int)value;

	/// <summary>Determines whether two <see cref="T:SkiaSharp.SKFourByteTag" /> values are equal.</summary>
	/// <param name="left">The first <see cref="T:SkiaSharp.SKFourByteTag" /> to compare.</param>
	/// <param name="right">The second <see cref="T:SkiaSharp.SKFourByteTag" /> to compare.</param>
	/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, <see langword="false" />.</returns>
	/// <remarks />
	public static bool operator == (SKFourByteTag left, SKFourByteTag right) => left.Equals (right);

	/// <summary>Determines whether two <see cref="T:SkiaSharp.SKFourByteTag" /> values are not equal.</summary>
	/// <param name="left">The first <see cref="T:SkiaSharp.SKFourByteTag" /> to compare.</param>
	/// <param name="right">The second <see cref="T:SkiaSharp.SKFourByteTag" /> to compare.</param>
	/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.</returns>
	/// <remarks />
	public static bool operator != (SKFourByteTag left, SKFourByteTag right) => !left.Equals (right);
}
