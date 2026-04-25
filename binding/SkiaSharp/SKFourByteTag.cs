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

	public static SKFourByteTag Parse (string tag)
	{
		if (string.IsNullOrEmpty (tag))
			return new SKFourByteTag (0);

		var realTag = new char[4];
		var len = Math.Min (4, tag.Length);
		var i = 0;
		for (; i < len; i++)
			realTag[i] = tag[i];
		for (; i < 4; i++)
			realTag[i] = ' ';

		return new SKFourByteTag (realTag[0], realTag[1], realTag[2], realTag[3]);
	}

	public override string ToString () =>
		string.Concat (
			(char)(byte)(value >> 24),
			(char)(byte)(value >> 16),
			(char)(byte)(value >> 8),
			(char)(byte)value);

	public static implicit operator uint (SKFourByteTag tag) => tag.value;

	public static implicit operator SKFourByteTag (uint tag) => new SKFourByteTag (tag);

	public override bool Equals (object obj) =>
		obj is SKFourByteTag tag && value.Equals (tag.value);

	public bool Equals (SKFourByteTag other) => value == other.value;

	public override int GetHashCode () => (int)value;

	public static bool operator == (SKFourByteTag left, SKFourByteTag right) => left.Equals (right);

	public static bool operator != (SKFourByteTag left, SKFourByteTag right) => !left.Equals (right);
}
