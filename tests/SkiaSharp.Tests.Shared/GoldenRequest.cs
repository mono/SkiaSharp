#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkiaSharp.Tests.Shared
{
	public sealed class GoldenRequest
	{
		public GoldenRequest(string profile, Version version, string casePath, string? architecture = null,
			string? variant = null, bool allowNeutralVariant = false)
		{
			Profile = GoldenPlatform.Normalize(profile);
			if (version == null) throw new ArgumentNullException(nameof(version));
			Version = version.Build < 0 ? new Version(version.Major, version.Minor, 0) : version;
			GoldenPaths.ValidateCase(casePath);
			if (architecture != null)
			{
				GoldenPaths.ValidateSegment(architecture);
				if (!GoldenPaths.IsArchitecture(architecture))
					throw new ArgumentException("Expected a supported .NET architecture: " + architecture, nameof(architecture));
			}
			if (variant != null)
			{
				GoldenPaths.ValidateSegment(variant);
				if (GoldenPaths.IsArchitecture(variant))
					throw new ArgumentException("Architecture tokens cannot be used as variants: " + variant, nameof(variant));
			}
			CasePath = casePath;
			Architecture = architecture;
			Variant = variant;
			AllowNeutralVariant = allowNeutralVariant;
		}

		public string Profile { get; }
		public Version Version { get; }
		public string CasePath { get; }
		public string? Architecture { get; }
		public string? Variant { get; }
		public bool AllowNeutralVariant { get; }

		internal IEnumerable<string> FileNames()
		{
			var variants = Variant != null && AllowNeutralVariant ? new[] { Variant, null } : new[] { Variant };
			foreach (var variant in variants)
			{
				var name = CasePath + (variant == null ? "" : "." + variant);
				if (Architecture != null) yield return name + "." + Architecture + ".png";
				yield return name + ".png";
			}
		}
	}

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

		internal static Version ValidateKey(string key)
		{
			var parts = key.Split('/');
			if (parts.Length < 3 || !key.EndsWith(".png", StringComparison.Ordinal))
				throw new ArgumentException("Expected profile/version/case[.variant][.architecture].png: " + key);
			ValidateSegment(parts[0]);
			var version = parts[1];
			if (!Regex.IsMatch(version, @"\A(0|[1-9][0-9]*)(\.(0|[1-9][0-9]*)){2,3}\z") ||
				!Version.TryParse(version, out var parsed))
				throw new ArgumentException("Expected a normalized System.Version with three or four components: " + key);
			foreach (var segment in parts.Skip(2).Take(parts.Length - 3))
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
}
