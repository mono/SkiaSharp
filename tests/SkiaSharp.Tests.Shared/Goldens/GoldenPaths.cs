using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkiaSharp.Tests.Shared;

// Validates canonical keys and safe relative case paths.
internal static class GoldenPaths
{
	internal static void ValidateSegment(string value, bool caseSegment = false)
	{
		var pattern = caseSegment ? @"\A[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*\z" : @"\A[a-z0-9]+(?:-[a-z0-9]+)*\z";
		if (string.IsNullOrEmpty(value) || !Regex.IsMatch(value, pattern) ||
			Regex.IsMatch(value, @"\A(con|prn|aux|nul|com[1-9]|lpt[1-9])\z", RegexOptions.IgnoreCase))
			throw new ArgumentException("Expected a safe golden path segment: " + value);
	}

	internal static void ValidateCase(string value)
	{
		if (value == null) throw new ArgumentNullException(nameof(value));
		foreach (var segment in value.Split('/')) ValidateSegment(segment, caseSegment: true);
	}

	internal static bool IsArchitecture(string value)
	{
		switch (value)
		{
			case "x86": case "x64": case "arm": case "arm64": case "armv6":
			case "wasm": case "s390x": case "loongarch64": case "ppc64le": case "riscv64":
				return true;
			default:
				return false;
		}
	}

	internal static Version? ValidateKey(string key)
	{
		var parts = key.Split('/');
		if (parts.Length < 2 || !key.EndsWith(".png", StringComparison.Ordinal))
			throw new ArgumentException("Expected platform/[version/]case[.variant][.architecture].png: " + key);
		ValidateSegment(parts[0]);
		var caseStart = 1;
		Version? parsed = null;
		if (parts.Length >= 3 &&
			Regex.IsMatch(parts[1], @"\A(0|[1-9][0-9]*)(\.(0|[1-9][0-9]*)){2,3}\z"))
		{
			if (!Version.TryParse(parts[1], out parsed))
				throw new ArgumentException("Invalid System.Version: " + key);
			caseStart = 2;
		}
		else if (parts.Length >= 3 && parts[1].Contains(".") && char.IsDigit(parts[1][0]))
		{
			throw new ArgumentException("Expected a normalized System.Version with three or four components: " + key);
		}
		foreach (var segment in parts.Skip(caseStart).Take(parts.Length - caseStart - 1))
			ValidateSegment(segment, caseSegment: true);
		var names = parts[parts.Length - 1].Substring(0, parts[parts.Length - 1].Length - 4).Split('.');
		if (names.Length > 3) throw new ArgumentException("Too many golden filename suffixes: " + key);
		ValidateSegment(names[0], caseSegment: true);
		foreach (var suffix in names.Skip(1)) ValidateSegment(suffix);
		if (names.Length == 3 && (IsArchitecture(names[1]) || !IsArchitecture(names[2])))
			throw new ArgumentException("Variant must precede a supported .NET architecture: " + key);
		return parsed;
	}
}
