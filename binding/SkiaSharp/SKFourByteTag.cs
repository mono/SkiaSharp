using System;

namespace SkiaSharp;

public readonly struct SKFourByteTag : IEquatable<SKFourByteTag>
{
	private readonly uint value;

	public SKFourByteTag (uint value)
	{
		this.value = value;
	}

	public SKFourByteTag (char c1, char c2, char c3, char c4)
	{
		value = (uint)(((byte)c1 << 24) | ((byte)c2 << 16) | ((byte)c3 << 8) | (byte)c4);
	}

	public static SKFourByteTag Parse (string? tag) =>
		Parse (tag.AsSpan ());

	public static SKFourByteTag Parse (ReadOnlySpan<char> tag)
	{
		if (tag.IsEmpty)
			return new SKFourByteTag (0);

		// Take up to the first four characters, padding any missing trailing
		// slots with spaces — matching the original char[4]-scratch behaviour
		// without allocating the scratch array.
		var c1 = tag.Length > 0 ? tag[0] : ' ';
		var c2 = tag.Length > 1 ? tag[1] : ' ';
		var c3 = tag.Length > 2 ? tag[2] : ' ';
		var c4 = tag.Length > 3 ? tag[3] : ' ';

		return new SKFourByteTag (c1, c2, c3, c4);
	}

	public override string ToString () =>
		string.Concat (
			(char)(byte)(value >> 24),
			(char)(byte)(value >> 16),
			(char)(byte)(value >> 8),
			(char)(byte)value);

	public static implicit operator uint (SKFourByteTag tag) => tag.value;

	public static implicit operator SKFourByteTag (uint tag) => new SKFourByteTag (tag);

	public override bool Equals (object? obj) =>
		obj is SKFourByteTag tag && value.Equals (tag.value);

	public bool Equals (SKFourByteTag other) => value == other.value;

	public override int GetHashCode () => (int)value;

	public static bool operator == (SKFourByteTag left, SKFourByteTag right) => left.Equals (right);

	public static bool operator != (SKFourByteTag left, SKFourByteTag right) => !left.Equals (right);
}
