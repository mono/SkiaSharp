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
			if (!SkiaSharpReleaseIdentity.TryParseTag(currentTag, out var current))
				throw new PlanException($"cannot order previous tag for invalid tag '{currentTag}'");

			SkiaSharpReleaseIdentity? best = null;
			string? bestTag = null;
			foreach (var tag in tags)
			{
				if (tag == currentTag)
					continue;
				if (!SkiaSharpReleaseIdentity.TryParseTag(tag, out var parsed))
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
	}
}
