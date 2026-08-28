using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Model
{
	/// <summary>
	/// An exact release version such as <c>3.119.0-preview.1</c> or
	/// <c>3.119.0.1</c>. Ported field-for-field from Python's
	/// <c>release_model.ReleaseVersion</c> frozen dataclass; construct via
	/// <see cref="ReleaseGrammar"/>, never directly.
	/// </summary>
	public sealed class ReleaseVersion
	{
		internal ReleaseVersion(string raw, string numeric, int[] parts, string? channel, int? iteration)
		{
			Raw = raw;
			Numeric = numeric;
			Parts = parts;
			Channel = channel;
			Iteration = iteration;
		}

		public string Raw { get; }

		public string Numeric { get; }

		public IReadOnlyList<int> Parts { get; }

		/// <summary><see langword="null"/> (stable), <c>"preview"</c>, or <c>"rc"</c>.</summary>
		public string? Channel { get; }

		public int? Iteration { get; }

		public bool IsHotfix => Parts.Count == 4;

		public bool Stable => Channel is null;

		public string Label => Channel is null ? "stable" : $"{Channel}.{Iteration}";

		public string ReleaseType => (IsHotfix ? "hotfix " : "") + (Channel ?? "stable");

		public string Line => $"{Parts[0]}.{Parts[1]}";

		public string IntegrationBranch => $"release/{Line}.x";

		public string ReleaseBranch => $"release/{Raw}";

		public string Tag => $"v{Raw}";

		public string Title => Channel switch
		{
			"preview" => $"Version {Numeric} (Preview {Iteration})",
			"rc" => $"Version {Numeric} (RC {Iteration})",
			_ => $"Version {Numeric}",
		};

		/// <summary>
		/// Channel ordering: preview (0) &lt; rc (1) &lt; stable/no channel (2).
		/// Matches Python's <c>_CHANNEL_RANK</c>.
		/// </summary>
		private static int RankOf(string? channel) => channel switch
		{
			"preview" => 0,
			"rc" => 1,
			_ => 2,
		};

		/// <summary>
		/// Compares two versions the same way Python's
		/// <c>sort_key</c> tuple does: <c>Parts</c> lexicographically
		/// (a shorter prefix -- e.g. the 3-part base of a hotfix -- sorts
		/// before its longer 4-part hotfix, matching Python tuple
		/// comparison), then channel (preview &lt; rc &lt; stable), then
		/// iteration.
		/// </summary>
		public int CompareSortKeyTo(ReleaseVersion other)
		{
			var partsComparison = CompareParts(Parts, other.Parts);
			if (partsComparison != 0)
				return partsComparison;
			var channelComparison = RankOf(Channel).CompareTo(RankOf(other.Channel));
			if (channelComparison != 0)
				return channelComparison;
			return (Iteration ?? 0).CompareTo(other.Iteration ?? 0);
		}

		private static int CompareParts(IReadOnlyList<int> a, IReadOnlyList<int> b)
		{
			var shorterLength = Math.Min(a.Count, b.Count);
			for (var i = 0; i < shorterLength; i++)
			{
				var comparison = a[i].CompareTo(b[i]);
				if (comparison != 0)
					return comparison;
			}
			return a.Count.CompareTo(b.Count);
		}

		/// <summary>
		/// Validates that <paramref name="version"/> was composed from
		/// this release: base + label + build. Returns
		/// <c>(base, buildRevision)</c> where <c>base</c> is always the
		/// bare numeric version (matching
		/// <c>SKIASHARP_VERSION</c>/<c>VERSIONS.txt</c>, never including
		/// the <c>-preview.N</c>/<c>-rc.N</c> channel), and
		/// <c>buildRevision</c> is <see langword="null"/> for a stable
		/// release (a bare public version) and the exact matched
		/// build-revision string otherwise. Composition, not equality, is
		/// used for preview/rc so that the CI build-revision suffix is
		/// accepted.
		/// </summary>
		public (string Base, string? BuildRevision) ValidatePublicVersion(string version)
		{
			if (Stable)
			{
				if (version != Numeric)
					throw new PlanException(
						$"public version '{version}' does not equal the stable base '{Numeric}'");
				return (Numeric, null);
			}
			var prefix = $"{Raw}.";
			if (!version.StartsWith(prefix, StringComparison.Ordinal))
				throw new PlanException($"public version '{version}' does not start with '{prefix}'");
			var buildRevision = version[prefix.Length..];
			var revisionMatch = ReleaseGrammar.BuildRevisionPattern().Match(buildRevision);
			if (!revisionMatch.Success || revisionMatch.Length != buildRevision.Length)
				throw new PlanException(
					$"public version '{version}' has an invalid build revision '{buildRevision}'");
			return (Numeric, buildRevision);
		}

		public static string ComposePublicVersion(string @base, string label, string buildRevision)
		{
			if (label == "stable")
				throw new PlanException("a stable public version has no build revision");
			var match = ReleaseGrammar.BuildRevisionPattern().Match(buildRevision);
			if (!match.Success || match.Length != buildRevision.Length)
				throw new PlanException($"invalid build revision '{buildRevision}'");
			return $"{@base}-{label}.{buildRevision}";
		}
	}
}
