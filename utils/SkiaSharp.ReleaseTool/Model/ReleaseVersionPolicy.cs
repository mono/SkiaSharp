using System.Text.RegularExpressions;
using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Model
{
	public static partial class ReleaseVersionPolicy
	{
		private static readonly string[] KnownRefPrefixes =
			["refs/remotes/origin/", "refs/heads/", "origin/"];

		[GeneratedRegex(@"^(?:main|release/\d+\.\d+\.x)$", RegexOptions.CultureInvariant)]
		private static partial Regex IntegrationBranchPattern();

		public static string NormalizeIntegrationBranch(string value)
		{
			foreach (var prefix in KnownRefPrefixes)
			{
				if (value.StartsWith(prefix, StringComparison.Ordinal))
				{
					value = value[prefix.Length..];
					break;
				}
			}

			if (!IntegrationBranchPattern().IsMatch(value))
				throw new PlanException(
					$"invalid integration target '{value}': expected 'main' or 'release/X.Y.x'");
			return value;
		}

		public static bool IsBuildRevision(string value)
		{
			var parts = value.Split('.');
			if (parts.Length == 1)
				return IsDigits(parts[0]);
			if (parts.Length == 2)
				return parts[0].Length is 5 or 8 && IsDigits(parts[0]) && IsDigits(parts[1]);
			return false;
		}

		internal static NuGetVersion ParseStableVersion(
			string value,
			string description,
			params int[] allowedComponentCounts)
		{
			if (!TryParseNuGetVersion(value, out var version, out var componentCount) ||
				version.IsPrerelease ||
				!allowedComponentCounts.Contains(componentCount))
			{
				throw new PlanException($"{description} '{value}' must be a stable {FormatCounts(allowedComponentCounts)}-part version");
			}
			return version;
		}

		internal static bool TryParseNuGetVersion(
			string? value,
			out NuGetVersion version,
			out int componentCount)
		{
			version = null!;
			componentCount = 0;
			if (string.IsNullOrEmpty(value) ||
				!string.Equals(value, value.Trim(), StringComparison.Ordinal) ||
				!NuGetVersion.TryParse(value, out var parsed) ||
				parsed.HasMetadata ||
				!TryGetNumericParts(value, out var parts))
			{
				return false;
			}

			version = parsed;
			componentCount = parts.Length;
			return true;
		}

		internal static bool TryGetNumericParts(string value, out string[] parts)
		{
			var suffix = value.IndexOfAny(['-', '+']);
			var numeric = suffix < 0 ? value : value[..suffix];
			parts = numeric.Split('.');
			return parts.All(IsDigits);
		}

		internal static string FormatNumeric(NuGetVersion version, int componentCount) =>
			version.Version.ToString(componentCount);

		private static bool IsDigits(string value) =>
			value.Length > 0 && value.All(char.IsAsciiDigit);

		private static string FormatCounts(int[] counts) =>
			counts.Length == 1 ? counts[0].ToString() : string.Join("-or-", counts);
	}
}
