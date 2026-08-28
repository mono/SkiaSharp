using System.Text.RegularExpressions;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Model
{
	/// <summary>
	/// Release, tag, and public-version grammars shared by every release
	/// command. Ported field-for-field from Python's
	/// <c>release_model.py</c>: every regex here is deliberately narrow
	/// (a real X.Y.Z[.F][-preview.N|-rc.N] release version, never an
	/// arbitrary SemVer string) so prepare and finish can never silently
	/// drift apart on what counts as a valid release.
	/// </summary>
	public static partial class ReleaseGrammar
	{
		// "main" or an existing maintenance line. Never an arbitrary ref or a PR ref.
		[GeneratedRegex(@"^(?:main|release/\d+\.\d+\.x)$")]
		private static partial Regex IntegrationBranchPattern();

		// An exact release version: X.Y.Z, the hotfix form X.Y.Z.F, and the
		// optional preview/rc channel suffix. Never a range, wildcard, or
		// partial version. Repeated (rather than shared as one pattern) in
		// the branch/tag grammars below, matching Python's `_NUMERIC`
		// f-string interpolation into RELEASE_BRANCH_RE/RELEASE_TAG_RE.
		private const string NumericGroup = @"\d+\.\d+\.\d+(?:\.\d+)?";
		private const string ChannelSuffixGroup = @"(?:-(?<channel>preview|rc)\.(?<iteration>\d+))?";

		[GeneratedRegex("^(?<numeric>" + NumericGroup + ")" + ChannelSuffixGroup + "$")]
		private static partial Regex ReleaseVersionPattern();

		// The exact release branch grammar (`release/` + the version grammar above).
		[GeneratedRegex("^release/(?<numeric>" + NumericGroup + ")" + ChannelSuffixGroup + "$")]
		private static partial Regex ReleaseBranchPattern();

		// The exact tag grammar used by GitHub releases: `v` + the version grammar.
		[GeneratedRegex("^v(?<numeric>" + NumericGroup + ")" + ChannelSuffixGroup + "$")]
		private static partial Regex ReleaseTagPattern();

		// The CI build-revision grammar already accepted by set-build-variables.ps1:
		// either a bare build number, or a 5- or 8-digit date-ish prefix + '.' + number.
		[GeneratedRegex(@"^(?:(?:\d{5}|\d{8})\.)?\d+$")]
		internal static partial Regex BuildRevisionPattern();

		public static ReleaseVersion ParseReleaseVersion(string value)
		{
			var match = ReleaseVersionPattern().Match(value);
			if (!match.Success || match.Length != value.Length)
				throw new PlanException(
					$"invalid release version '{value}': expected X.Y.Z[.F][-preview.N|-rc.N]");
			return FromMatch(value, match, validateIteration: true);
		}

		public static bool TryParseReleaseVersion(string value, out ReleaseVersion? version)
		{
			var match = ReleaseVersionPattern().Match(value);
			if (!match.Success || match.Length != value.Length || HasZeroIteration(match))
			{
				version = null;
				return false;
			}
			version = FromMatch(value, match, validateIteration: false);
			return true;
		}

		public static ReleaseVersion ParseReleaseBranch(string value)
		{
			var match = ReleaseBranchPattern().Match(value);
			if (!match.Success || match.Length != value.Length)
				throw new PlanException(
					$"invalid release branch '{value}': expected release/X.Y.Z[.F][-preview.N|-rc.N]");
			return ParseReleaseVersion(value["release/".Length..]);
		}

		public static ReleaseVersion ParseReleaseTag(string value)
		{
			var match = ReleaseTagPattern().Match(value);
			if (!match.Success || match.Length != value.Length)
				throw new PlanException(
					$"invalid release tag '{value}': expected vX.Y.Z[.F][-preview.N|-rc.N]");
			return ParseReleaseVersion(value[1..]);
		}

		/// <summary>
		/// Parses a <c>vX.Y.Z[...]</c> tag for previous-tag ordering,
		/// returning <see langword="null"/> instead of throwing for a tag
		/// that does not match the grammar. Mirrors Python's
		/// <c>release_github.TagVersion.parse</c>, which a caller
		/// iterating over arbitrary remote tag names uses to silently
		/// skip anything that is not a release tag.
		///
		/// Deliberately does not apply the "iteration must be >= 1" rule
		/// <see cref="ParseReleaseVersion"/> enforces: like
		/// <c>TagVersion.parse</c>, this only orders tags that already
		/// exist on the remote, so it must never throw while doing so --
		/// a stray non-conforming tag should be tolerated for ordering
		/// purposes, not treated as a fatal error.
		/// </summary>
		public static bool TryParseReleaseTag(string value, out ReleaseVersion? version)
		{
			var match = ReleaseTagPattern().Match(value);
			if (!match.Success || match.Length != value.Length)
			{
				version = null;
				return false;
			}
			var raw = value[1..];
			var numericMatch = ReleaseVersionPattern().Match(raw);
			version = FromMatch(raw, numericMatch, validateIteration: false);
			return true;
		}

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
			var match = IntegrationBranchPattern().Match(value);
			if (!match.Success || match.Length != value.Length)
				throw new PlanException(
					$"invalid integration target '{value}': expected 'main' or 'release/X.Y.x'");
			return value;
		}

		private static readonly string[] KnownRefPrefixes = ["refs/remotes/origin/", "refs/heads/", "origin/"];

		private static bool HasZeroIteration(Match match) =>
			match.Groups["channel"].Success && match.Groups["iteration"].Value == "0";

		private static ReleaseVersion FromMatch(string raw, Match match, bool validateIteration)
		{
			if (validateIteration && HasZeroIteration(match))
				throw new PlanException($"invalid release version '{raw}': iteration must be >= 1");
			var channel = match.Groups["channel"].Success ? match.Groups["channel"].Value : null;
			var iteration = channel is null ? (int?)null : int.Parse(match.Groups["iteration"].Value);
			var numeric = match.Groups["numeric"].Value;
			var parts = Array.ConvertAll(numeric.Split('.'), int.Parse);
			return new ReleaseVersion(raw, numeric, parts, channel, iteration);
		}
	}
}
