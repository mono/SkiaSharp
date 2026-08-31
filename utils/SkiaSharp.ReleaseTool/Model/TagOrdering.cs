using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Model
{
	/// <summary>Selects release tags using NuGet's release-version ordering.</summary>
	public static class TagOrdering
	{
		/// <summary>
		/// Returns the immediately preceding NuGet-compatible tag among
		/// <paramref name="tags"/>, or <see langword="null"/> if none
		/// sorts before <paramref name="currentTag"/>. Tags that do not
		/// match the release-tag policy are ignored.
		/// </summary>
		public static string? SelectPreviousTag(string currentTag, IEnumerable<string> tags)
		{
			if (!TryNormalizeTag(currentTag, out var current))
				throw new PlanException($"cannot order previous tag for invalid tag '{currentTag}'");

			SkiaSharpReleaseIdentity? best = null;
			string? bestTag = null;
			var normalizedTags = new Dictionary<string, string>(StringComparer.Ordinal);
			foreach (var tag in tags)
			{
				if (!TryNormalizeTag(tag, out var parsed))
					continue;
				if (normalizedTags.TryGetValue(parsed.Raw, out var existing))
				{
					if (existing != tag)
					{
						throw new PlanException(
							$"release tags '{existing}' and '{tag}' normalize to the same identity '{parsed.Raw}'");
					}
				}
				else
				{
					normalizedTags.Add(parsed.Raw, tag);
				}
				if (VersionComparer.VersionRelease.Equals(parsed.Version, current.Version))
					continue;
				if (VersionComparer.VersionRelease.Compare(parsed.Version, current.Version) >= 0)
					continue;
				if (best is null || VersionComparer.VersionRelease.Compare(parsed.Version, best.Version) > 0)
				{
					best = parsed;
					bestTag = tag;
				}
			}
			return bestTag;
		}

		internal static bool TryNormalizeTag(
			string? tag,
			out SkiaSharpReleaseIdentity identity)
		{
			if (SkiaSharpReleaseIdentity.TryParseTag(tag, out identity))
				return true;
			identity = null!;
			if (tag is null ||
				!tag.StartsWith('v') ||
				!NuGetVersion.TryParse(tag[1..], out var version) ||
				!ReleaseVersionPolicy.TryGetNumericParts(tag[1..], out var numericParts) ||
				numericParts.Length is not (3 or 4) ||
				!version.IsPrerelease)
			{
				return false;
			}
			var labels = version.ReleaseLabels.ToArray();
			if (labels.Length is not (3 or 4) ||
				labels[0] is not ("preview" or "rc") ||
				!int.TryParse(labels[1], out var iteration) ||
				iteration <= 0 ||
				!ReleaseVersionPolicy.IsBuildRevision(string.Join('.', labels.Skip(2))))
			{
				return false;
			}
			var numeric = string.Join('.', numericParts);
			return SkiaSharpReleaseIdentity.TryParse(
				$"{numeric}-{labels[0]}.{labels[1]}",
				out identity);
		}
	}
}
