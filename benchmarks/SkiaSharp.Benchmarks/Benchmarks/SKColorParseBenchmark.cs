using System;
using System.Globalization;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// SKColor.TryParse turns hex color strings ("#RGB", "#ARGB", "#RRGGBB", "#AARRGGBB", with or
// without a leading '#') into an SKColor. It sits on hot paths for anyone hydrating colors from
// XAML/CSS/JSON/theme files, so the per-call cost and allocations matter when thousands of colors
// are parsed at startup.
//
// This benchmark compares the shipped SKColor.TryParse (a branch-per-length manual hex parser)
// against the previous implementation, which leaned on byte.TryParse/uint.TryParse with
// NumberStyles.HexNumber + CultureInfo.InvariantCulture. The old span-based path is reproduced
// verbatim below as the baseline so both are measured in the same process/TFM.
//
// Inputs cover every supported shape plus an invalid string, since the length switch means each
// shape takes a different branch.
[MemoryDiagnoser]
public class SKColorParseBenchmark
{
	[Params("#FFFFFFFF", "#FFFFFF", "#FFF", "#FFFF", "#A7A8A9A0", "Red")]
	public string Input { get; set; }

	// New: the shipped implementation.
	[Benchmark]
	public uint New()
	{
		SKColor.TryParse(Input, out var color);
		return (uint)color;
	}

	// Baseline: the previous span-based implementation using byte/uint.TryParse.
	[Benchmark(Baseline = true)]
	public uint Old()
	{
		OldTryParse(Input, out var color);
		return (uint)color;
	}

	// Verbatim copy of the previous SKColor.TryParse span path (NETCOREAPP3_1_OR_GREATER branch).
	private static bool OldTryParse(string hexString, out SKColor color)
	{
		if (string.IsNullOrWhiteSpace(hexString))
		{
			color = SKColor.Empty;
			return false;
		}

		var hexSpan = hexString.AsSpan().Trim().TrimStart('#');

		var len = hexSpan.Length;
		if (len == 3 || len == 4)
		{
			byte a;
			if (len == 4)
			{
				if (!byte.TryParse(hexSpan.Slice(0, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out a))
				{
					color = SKColor.Empty;
					return false;
				}
				a = (byte)(a << 4 | a);
			}
			else
			{
				a = 255;
			}

			if (!byte.TryParse(hexSpan.Slice(len - 3, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r) ||
				!byte.TryParse(hexSpan.Slice(len - 2, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g) ||
				!byte.TryParse(hexSpan.Slice(len - 1, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
			{
				color = SKColor.Empty;
				return false;
			}

			color = new SKColor((byte)(r << 4 | r), (byte)(g << 4 | g), (byte)(b << 4 | b), a);
			return true;
		}

		if (len == 6 || len == 8)
		{
			if (!uint.TryParse(hexSpan, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var number))
			{
				color = SKColor.Empty;
				return false;
			}

			color = (SKColor)number;

			if (len == 6)
			{
				color = color.WithAlpha(255);
			}
			return true;
		}

		color = SKColor.Empty;
		return false;
	}
}
