// Copyright (c) Microsoft Corporation. Licensed under the MIT license.
#nullable enable

using System;

namespace SkiaSharp.Testing;

/// <summary>Per-channel absolute byte-difference allowances. A null channel is excluded from comparisons and statistics.</summary>
/// <remarks>The default value is <see cref="Exact"/>: all four RGBA channels are enabled with zero allowance.</remarks>
public readonly struct SKPixelTolerance : IEquatable<SKPixelTolerance>
{
	private readonly double red;
	private readonly double green;
	private readonly double blue;
	private readonly double alpha;
	private readonly byte enabledChannels;

	private SKPixelTolerance(double red, double green, double blue, double alpha, byte enabledChannels)
	{
		this.red = red;
		this.green = green;
		this.blue = blue;
		this.alpha = alpha;
		this.enabledChannels = enabledChannels == 15 ? (byte)0 : enabledChannels;
	}

	private byte Channels => enabledChannels == 0 ? (byte)15 : enabledChannels;

	/// <summary>Exact comparison of all four RGBA channels.</summary>
	public static SKPixelTolerance Exact => default;

	/// <summary>Effective red-channel allowance in byte units, or null if red is excluded.</summary>
	public double? Red => (Channels & 1) != 0 ? red : (double?)null;

	/// <summary>Effective green-channel allowance in byte units, or null if green is excluded.</summary>
	public double? Green => (Channels & 2) != 0 ? green : (double?)null;

	/// <summary>Effective blue-channel allowance in byte units, or null if blue is excluded.</summary>
	public double? Blue => (Channels & 4) != 0 ? blue : (double?)null;

	/// <summary>Effective alpha-channel allowance in byte units, or null if alpha is excluded.</summary>
	public double? Alpha => (Channels & 8) != 0 ? alpha : (double?)null;

	/// <summary>Number of included channels (one to four).</summary>
	public int ChannelCount =>
		((Channels & 1) != 0 ? 1 : 0) + ((Channels & 2) != 0 ? 1 : 0) +
		((Channels & 4) != 0 ? 1 : 0) + ((Channels & 8) != 0 ? 1 : 0);

	/// <summary>Allows up to <paramref name="value"/> byte levels of difference on every RGBA channel.</summary>
	/// <exception cref="ArgumentOutOfRangeException">The value is outside 0 through 255.</exception>
	public static SKPixelTolerance Absolute(int value)
	{
		ValidateAbsolute(value, nameof(value));
		return Create(value, value, value, value);
	}

	/// <summary>Sets independent byte allowances; null excludes a channel entirely.</summary>
	/// <exception cref="ArgumentOutOfRangeException">An enabled value is outside 0 through 255.</exception>
	/// <exception cref="ArgumentException">All channels are excluded.</exception>
	public static SKPixelTolerance Absolute(int? red, int? green, int? blue, int? alpha)
	{
		ValidateAbsolute(red, nameof(red));
		ValidateAbsolute(green, nameof(green));
		ValidateAbsolute(blue, nameof(blue));
		ValidateAbsolute(alpha, nameof(alpha));
		return Create(red, green, blue, alpha);
	}

	/// <summary>Allows <paramref name="value"/> percentage points of the 255 byte range on every RGBA channel, without rounding.</summary>
	/// <exception cref="ArgumentOutOfRangeException">The value is non-finite or outside 0 through 100.</exception>
	public static SKPixelTolerance Percent(double value)
	{
		ValidatePercent(value, nameof(value));
		var bytes = value * 255.0 / 100.0;
		return Create(bytes, bytes, bytes, bytes);
	}

	/// <summary>Sets independent percentage-point allowances of the 255 byte range without rounding; null excludes a channel.</summary>
	/// <exception cref="ArgumentOutOfRangeException">An enabled value is non-finite or outside 0 through 100.</exception>
	/// <exception cref="ArgumentException">All channels are excluded.</exception>
	public static SKPixelTolerance Percent(double? red, double? green, double? blue, double? alpha)
	{
		ValidatePercent(red, nameof(red));
		ValidatePercent(green, nameof(green));
		ValidatePercent(blue, nameof(blue));
		ValidatePercent(alpha, nameof(alpha));
		return Create(red * 255.0 / 100.0, green * 255.0 / 100.0,
			blue * 255.0 / 100.0, alpha * 255.0 / 100.0);
	}

	/// <summary>Determines whether the enabled channels and their effective byte allowances are equal.</summary>
	public bool Equals(SKPixelTolerance other) =>
		Channels == other.Channels && red == other.red && green == other.green &&
		blue == other.blue && alpha == other.alpha;

	/// <summary>Determines whether an object is an equal pixel tolerance.</summary>
	public override bool Equals(object? obj) => obj is SKPixelTolerance other && Equals(other);

	/// <summary>Returns a hash code for the enabled channels and their allowances.</summary>
	public override int GetHashCode()
	{
		unchecked
		{
			var hash = Channels.GetHashCode();
			hash = hash * 31 + red.GetHashCode();
			hash = hash * 31 + green.GetHashCode();
			hash = hash * 31 + blue.GetHashCode();
			return hash * 31 + alpha.GetHashCode();
		}
	}

	/// <summary>Tests two tolerances for equality.</summary>
	public static bool operator ==(SKPixelTolerance left, SKPixelTolerance right) => left.Equals(right);

	/// <summary>Tests two tolerances for inequality.</summary>
	public static bool operator !=(SKPixelTolerance left, SKPixelTolerance right) => !left.Equals(right);

	private static SKPixelTolerance Create(double? red, double? green, double? blue, double? alpha)
	{
		var channels = (byte)((red.HasValue ? 1 : 0) | (green.HasValue ? 2 : 0) |
			(blue.HasValue ? 4 : 0) | (alpha.HasValue ? 8 : 0));
		if (channels == 0)
			throw new ArgumentException("At least one channel must be enabled.");
		return new SKPixelTolerance(red ?? 0, green ?? 0, blue ?? 0, alpha ?? 0, channels);
	}

	private static void ValidateAbsolute(int? value, string name)
	{
		if (value < 0 || value > 255)
			throw new ArgumentOutOfRangeException(name, "Allowance must be between 0 and 255.");
	}

	private static void ValidatePercent(double? value, string name)
	{
		if (value.HasValue && (double.IsNaN(value.Value) || double.IsInfinity(value.Value) || value < 0 || value > 100))
			throw new ArgumentOutOfRangeException(name, "Percentage points must be finite and between 0 and 100.");
	}
}
