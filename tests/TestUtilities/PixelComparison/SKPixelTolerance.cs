// Copyright (c) Microsoft Corporation. Licensed under the MIT license.
#nullable enable

using System;

namespace SkiaSharp.Testing;

/// <summary>An inclusive absolute byte-difference allowance for one color channel.</summary>
/// <remarks>The default value is <see cref="Exact"/>. Configure each channel independently with <see cref="SKPixelComparerOptions"/>; null disables that channel.</remarks>
public readonly struct SKPixelTolerance : IEquatable<SKPixelTolerance>
{
	private readonly double value;

	private SKPixelTolerance(double value) => this.value = value;

	/// <summary>An exact channel comparison with zero byte-difference allowance.</summary>
	public static SKPixelTolerance Exact => default;

	/// <summary>Gets the effective allowance in byte units, including fractional byte values.</summary>
	public double Value => value;

	/// <summary>Allows up to <paramref name="value"/> byte levels of difference on a channel.</summary>
	/// <exception cref="ArgumentOutOfRangeException">The value is outside 0 through 255.</exception>
	public static SKPixelTolerance Absolute(int value)
	{
		if (value < 0 || value > 255)
			throw new ArgumentOutOfRangeException(nameof(value), "Allowance must be between 0 and 255.");
		return new SKPixelTolerance(value);
	}

	/// <summary>Allows <paramref name="value"/> percentage points of the 255 byte range on a channel, without rounding.</summary>
	/// <exception cref="ArgumentOutOfRangeException">The value is non-finite or outside 0 through 100.</exception>
	public static SKPixelTolerance Percent(double value)
	{
		if (double.IsNaN(value) || double.IsInfinity(value) || value < 0 || value > 100)
			throw new ArgumentOutOfRangeException(nameof(value), "Percentage points must be finite and between 0 and 100.");
		return new SKPixelTolerance(value * 255.0 / 100.0);
	}

	/// <summary>Tests whether a finite nonnegative channel difference is within this inclusive byte allowance.</summary>
	/// <exception cref="ArgumentOutOfRangeException">The difference is negative or non-finite.</exception>
	public bool Allows(double absoluteDifference)
	{
		if (double.IsNaN(absoluteDifference) || double.IsInfinity(absoluteDifference) || absoluteDifference < 0)
			throw new ArgumentOutOfRangeException(nameof(absoluteDifference), "Difference must be finite and nonnegative.");
		return absoluteDifference <= value;
	}

	/// <summary>Determines whether the effective byte allowances are equal.</summary>
	public bool Equals(SKPixelTolerance other) => value == other.value;

	/// <summary>Determines whether an object is an equal pixel tolerance.</summary>
	public override bool Equals(object? obj) => obj is SKPixelTolerance other && Equals(other);

	/// <summary>Returns a hash code for the effective byte allowance.</summary>
	public override int GetHashCode() => value.GetHashCode();

	/// <summary>Tests two tolerances for equality.</summary>
	public static bool operator ==(SKPixelTolerance left, SKPixelTolerance right) => left.Equals(right);

	/// <summary>Tests two tolerances for inequality.</summary>
	public static bool operator !=(SKPixelTolerance left, SKPixelTolerance right) => !left.Equals(right);
}
