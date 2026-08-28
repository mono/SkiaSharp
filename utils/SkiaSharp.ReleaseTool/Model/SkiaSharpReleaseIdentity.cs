using System.Text.Json.Serialization;
using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Model
{
	public enum ReleaseKind
	{
		[JsonStringEnumMemberName("stable")]
		Stable,

		[JsonStringEnumMemberName("preview")]
		Preview,

		[JsonStringEnumMemberName("rc")]
		ReleaseCandidate,

		[JsonStringEnumMemberName("hotfix-stable")]
		HotfixStable,

		[JsonStringEnumMemberName("hotfix-preview")]
		HotfixPreview,

		[JsonStringEnumMemberName("hotfix-rc")]
		HotfixReleaseCandidate,
	}

	/// <summary>SkiaSharp's narrow release policy layered on NuGet's version model.</summary>
	public sealed class SkiaSharpReleaseIdentity : IComparable<SkiaSharpReleaseIdentity>, IEquatable<SkiaSharpReleaseIdentity>
	{
		private static readonly IVersionComparer Comparer = VersionComparer.VersionRelease;

		private SkiaSharpReleaseIdentity(
			NuGetVersion version,
			int componentCount,
			string numeric,
			string raw)
		{
			Version = version;
			ComponentCount = componentCount;
			Numeric = numeric;
			Raw = raw;

			var labels = version.ReleaseLabels.ToArray();
			if (labels.Length == 0)
			{
				Channel = null;
				Iteration = null;
			}
			else
			{
				Channel = labels[0] switch
				{
					"preview" => ReleaseKind.Preview,
					"rc" => ReleaseKind.ReleaseCandidate,
					_ => throw new InvalidOperationException("Release labels were not validated."),
				};
				Iteration = int.Parse(labels[1], System.Globalization.CultureInfo.InvariantCulture);
			}
		}

		public NuGetVersion Version { get; }

		public int ComponentCount { get; }

		public string Numeric { get; }

		public ReleaseKind? Channel { get; }

		public int? Iteration { get; }

		public bool IsHotfix => ComponentCount == 4;

		public bool Stable => !Version.IsPrerelease;

		public string Raw { get; }

		public string Label => Stable
			? "stable"
			: string.Join('.', Version.ReleaseLabels);

		public ReleaseKind ReleaseType => (IsHotfix, Channel) switch
		{
			(false, null) => ReleaseKind.Stable,
			(false, ReleaseKind.Preview) => ReleaseKind.Preview,
			(false, ReleaseKind.ReleaseCandidate) => ReleaseKind.ReleaseCandidate,
			(true, null) => ReleaseKind.HotfixStable,
			(true, ReleaseKind.Preview) => ReleaseKind.HotfixPreview,
			(true, ReleaseKind.ReleaseCandidate) => ReleaseKind.HotfixReleaseCandidate,
			_ => throw new InvalidOperationException("Unsupported release identity."),
		};

		public string Line => $"{Version.Major}.{Version.Minor}";

		public string IntegrationBranch => $"release/{Line}.x";

		public string ReleaseBranch => $"release/{Raw}";

		public string Tag => $"v{Raw}";

		public string Title => Channel switch
		{
			ReleaseKind.Preview => $"Version {Numeric} (Preview {Iteration})",
			ReleaseKind.ReleaseCandidate => $"Version {Numeric} (RC {Iteration})",
			_ => $"Version {Numeric}",
		};

		public static SkiaSharpReleaseIdentity Parse(string value)
		{
			if (!TryParse(value, out var identity))
				throw new PlanException(
					$"invalid release version '{value}': expected X.Y.Z[.F][-preview.N|-rc.N], with N greater than zero");
			return identity;
		}

		public static bool TryParse(string? value, out SkiaSharpReleaseIdentity identity)
		{
			identity = null!;
			if (string.IsNullOrEmpty(value) ||
				!string.Equals(value, value.Trim(), StringComparison.Ordinal) ||
				!NuGetVersion.TryParse(value, out var parsed) ||
				parsed.HasMetadata ||
				!string.Equals(value, parsed.ToNormalizedString(), StringComparison.Ordinal))
			{
				return false;
			}

			if (!ReleaseVersionPolicy.TryGetNumericParts(value, out var parts) ||
				parts.Length is not (3 or 4))
			{
				return false;
			}

			var labels = parsed.ReleaseLabels.ToArray();
			if (labels.Length != 0)
			{
				if (labels.Length != 2 ||
					(labels[0] != "preview" && labels[0] != "rc") ||
					!int.TryParse(labels[1], out var iteration) ||
					iteration <= 0)
				{
					return false;
				}
			}

			var numeric = string.Join('.', parts);
			identity = new SkiaSharpReleaseIdentity(parsed, parts.Length, numeric, value);
			return true;
		}

		public static SkiaSharpReleaseIdentity ParseBranch(string branch)
		{
			const string prefix = "release/";
			if (!branch.StartsWith(prefix, StringComparison.Ordinal))
				throw new PlanException($"invalid release branch '{branch}'");
			return Parse(branch[prefix.Length..]);
		}

		public static SkiaSharpReleaseIdentity ParseTag(string tag)
		{
			const string prefix = "v";
			if (!tag.StartsWith(prefix, StringComparison.Ordinal))
				throw new PlanException($"invalid release tag '{tag}'");
			return Parse(tag[prefix.Length..]);
		}

		public static bool TryParseTag(string? tag, out SkiaSharpReleaseIdentity identity)
		{
			identity = null!;
			return tag is not null &&
				tag.StartsWith('v') &&
				TryParse(tag[1..], out identity);
		}

		public (string Base, string? BuildRevision) ValidatePublicVersion(string value)
		{
			if (!ReleaseVersionPolicy.TryParseNuGetVersion(value, out var publicVersion, out var componentCount) ||
				componentCount != ComponentCount)
			{
				throw new PlanException($"public version '{value}' is not a valid SkiaSharp package version");
			}

			if (Stable)
			{
				if (!Comparer.Equals(Version, publicVersion))
					throw new PlanException($"public version '{value}' does not match release '{Raw}'");
				return (Numeric, null);
			}

			if (!Equals(Version.Version, publicVersion.Version))
				throw new PlanException($"public version '{value}' has the wrong numeric version");

			var identityLabels = Version.ReleaseLabels.ToArray();
			var publicLabels = publicVersion.ReleaseLabels.ToArray();
			if (publicLabels.Length <= identityLabels.Length ||
				!publicLabels.Take(identityLabels.Length).SequenceEqual(identityLabels, StringComparer.Ordinal))
			{
				throw new PlanException($"public version '{value}' has the wrong release label");
			}

			var buildRevision = string.Join('.', publicLabels.Skip(identityLabels.Length));
			if (!ReleaseVersionPolicy.IsBuildRevision(buildRevision))
				throw new PlanException($"public version '{value}' has an invalid build revision '{buildRevision}'");

			return (Numeric, buildRevision);
		}

		public string ComposePublicVersion(string buildRevision)
		{
			if (Stable)
				throw new PlanException("a stable public version has no build revision");
			if (!ReleaseVersionPolicy.IsBuildRevision(buildRevision))
				throw new PlanException($"invalid build revision '{buildRevision}'");

			var value = $"{Raw}.{buildRevision}";
			if (!NuGetVersion.TryParse(value, out var parsed))
				throw new PlanException($"invalid public version '{value}'");
			return parsed.ToNormalizedString();
		}

		public int CompareTo(SkiaSharpReleaseIdentity? other) =>
			other is null ? 1 : Comparer.Compare(Version, other.Version);

		public bool Equals(SkiaSharpReleaseIdentity? other) =>
			other is not null &&
			ComponentCount == other.ComponentCount &&
			Comparer.Equals(Version, other.Version);

		public override bool Equals(object? obj) => Equals(obj as SkiaSharpReleaseIdentity);

		public override int GetHashCode() => HashCode.Combine(
			ComponentCount,
			StringComparer.OrdinalIgnoreCase.GetHashCode(Raw));

		public override string ToString() => Raw;
	}
}
