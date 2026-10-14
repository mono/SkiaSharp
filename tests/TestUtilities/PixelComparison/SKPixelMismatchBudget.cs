// Copyright (c) Microsoft Corporation. Licensed under the MIT license.
#nullable enable

using System;
using System.Globalization;
using System.Numerics;

namespace SkiaSharp.Testing;

/// <summary>An inclusive mismatched-pixel allowance, limited by a count, percentage, or both.</summary>
/// <remarks>The default value is <see cref="Exact"/>. Percentages are percentage points of the compared pixel count, not fractions. Their inclusive pixel limits are floored from the invariant round-trip decimal representation of the percentage to avoid binary rounding at decimal boundaries.</remarks>
public readonly struct SKPixelMismatchBudget : IEquatable<SKPixelMismatchBudget>
{
	private readonly long? maxPixels;
	private readonly double? maxPercent;

	private SKPixelMismatchBudget(long? maxPixels, double? maxPercent)
	{
		this.maxPixels = maxPixels;
		this.maxPercent = maxPercent;
	}

	/// <summary>Allows no mismatched pixels.</summary>
	public static SKPixelMismatchBudget Exact => default;

	/// <summary>Gets the inclusive mismatched-pixel count limit, or null if no count limit was specified.</summary>
	public long? MaxPixels => maxPixels ?? (maxPercent.HasValue ? (long?)null : 0L);

	/// <summary>Gets the percentage-point limit (0 through 100), or null if no percentage limit was specified.</summary>
	public double? MaxPercent => maxPercent;

	/// <summary>Allows at most <paramref name="maximum"/> mismatched pixels.</summary>
	/// <exception cref="ArgumentOutOfRangeException">The maximum is negative.</exception>
	public static SKPixelMismatchBudget Pixels(long maximum)
	{
		if (maximum < 0)
			throw new ArgumentOutOfRangeException(nameof(maximum), "Maximum pixels must be nonnegative.");
		return new SKPixelMismatchBudget(maximum, null);
	}

	/// <summary>Allows at most <paramref name="maximum"/> percentage points of compared pixels to be mismatched.</summary>
	/// <exception cref="ArgumentOutOfRangeException">The maximum is non-finite or outside 0 through 100.</exception>
	public static SKPixelMismatchBudget Percent(double maximum)
	{
		ValidatePercent(maximum, nameof(maximum));
		return new SKPixelMismatchBudget(null, maximum);
	}

	/// <summary>Allows mismatched pixels only when both the count and percentage-point limits pass.</summary>
	/// <exception cref="ArgumentOutOfRangeException">The count is negative or the percentage is non-finite or outside 0 through 100.</exception>
	public static SKPixelMismatchBudget Both(long pixels, double percent)
	{
		if (pixels < 0)
			throw new ArgumentOutOfRangeException(nameof(pixels), "Maximum pixels must be nonnegative.");
		ValidatePercent(percent, nameof(percent));
		return new SKPixelMismatchBudget(pixels, percent);
	}

	/// <summary>Tests whether the mismatched pixel count satisfies every configured inclusive limit.</summary>
	/// <exception cref="ArgumentOutOfRangeException">The compared count is not positive, or the mismatched count is outside zero through the compared count.</exception>
	public bool Allows(long mismatchedPixels, long comparedPixels)
	{
		if (comparedPixels <= 0)
			throw new ArgumentOutOfRangeException(nameof(comparedPixels), "Compared pixels must be positive.");
		if (mismatchedPixels < 0 || mismatchedPixels > comparedPixels)
			throw new ArgumentOutOfRangeException(nameof(mismatchedPixels), "Mismatched pixels must be between zero and the compared pixel count.");

		if (MaxPixels is long countLimit && mismatchedPixels > countLimit)
			return false;
		return !maxPercent.HasValue || mismatchedPixels <= PercentLimit(comparedPixels, maxPercent.Value);
	}

	/// <summary>Determines whether both budgets have the same limits.</summary>
	public bool Equals(SKPixelMismatchBudget other) => MaxPixels == other.MaxPixels && maxPercent == other.maxPercent;

	/// <summary>Determines whether an object is an equal pixel mismatch budget.</summary>
	public override bool Equals(object? obj) => obj is SKPixelMismatchBudget other && Equals(other);

	/// <summary>Returns a hash code for the count and percentage-point limits.</summary>
	public override int GetHashCode()
	{
		unchecked
		{
			return (MaxPixels?.GetHashCode() ?? 0) * 397 ^ (maxPercent?.GetHashCode() ?? 0);
		}
	}

	/// <summary>Tests two pixel mismatch budgets for equality.</summary>
	public static bool operator ==(SKPixelMismatchBudget left, SKPixelMismatchBudget right) => left.Equals(right);

	/// <summary>Tests two pixel mismatch budgets for inequality.</summary>
	public static bool operator !=(SKPixelMismatchBudget left, SKPixelMismatchBudget right) => !left.Equals(right);

	private static void ValidatePercent(double percent, string parameterName)
	{
		if (double.IsNaN(percent) || double.IsInfinity(percent) || percent < 0 || percent > 100)
			throw new ArgumentOutOfRangeException(parameterName, "Percentage points must be finite and between 0 and 100.");
	}

	private static long PercentLimit(long comparedPixels, double percent)
	{
		if (percent == 100)
			return comparedPixels;
		if (percent == 0)
			return 0;

		// Interpret the round-trip decimal digits as a rational: 0.3% of 1000
		// is exactly 3, without binary multiplication or decimal casts rounding
		// a limit close to 100% up to the full long pixel count.
		var text = percent.ToString("R", CultureInfo.InvariantCulture);
		var exponentIndex = text.IndexOf('E');
		var mantissaEnd = exponentIndex < 0 ? text.Length : exponentIndex;
		var exponent = exponentIndex < 0 ? 0 : int.Parse(text.Substring(exponentIndex + 1), CultureInfo.InvariantCulture);
		var digits = BigInteger.Zero;
		var afterDecimalPoint = false;
		for (var i = 0; i < mantissaEnd; i++)
		{
			if (text[i] == '.')
			{
				afterDecimalPoint = true;
				continue;
			}
			digits = digits * 10 + (text[i] - '0');
			if (afterDecimalPoint)
				exponent--;
		}

		var numerator = new BigInteger(comparedPixels) * digits;
		if (exponent > 0)
			numerator *= BigInteger.Pow(10, exponent);
		var denominator = new BigInteger(100);
		if (exponent < 0)
			denominator *= BigInteger.Pow(10, -exponent);
		return (long)(numerator / denominator);
	}
}
