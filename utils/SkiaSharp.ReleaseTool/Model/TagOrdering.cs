using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Model
{
	/// <summary>
	/// Previous-tag selection for release notes/GitHub release bodies.
	/// Ported from Python's <c>release_github.previous_release_tag</c>
	/// (which itself reuses <c>release_model.RELEASE_TAG_RE</c> via
	/// <c>TagVersion</c>): ordering spans channels, so every preview/rc of
	/// a numeric version sorts before its stable release, and stable
	/// releases sort by numeric version.
	/// </summary>
	public static class TagOrdering
	{
		/// <summary>
		/// Returns the immediately preceding NuGet-compatible tag among
		/// <paramref name="tags"/>, or <see langword="null"/> if none
		/// sorts before <paramref name="currentTag"/>. Tags that do not
		/// match the release-tag grammar are silently ignored, matching
		/// Python's <c>TagVersion.parse</c> returning <see langword="null"/>
		/// for anything that is not a release tag.
		/// </summary>
		public static string? SelectPreviousTag(string currentTag, IEnumerable<string> tags)
		{
			if (!ReleaseGrammar.TryParseReleaseTag(currentTag, out var current) || current is null)
				throw new PlanException($"cannot order previous tag for invalid tag '{currentTag}'");

			ReleaseVersion? best = null;
			string? bestTag = null;
			foreach (var tag in tags)
			{
				if (tag == currentTag)
					continue;
				if (!ReleaseGrammar.TryParseReleaseTag(tag, out var parsed) || parsed is null)
					continue;
				if (parsed.CompareSortKeyTo(current) >= 0)
					continue;
				if (best is null || parsed.CompareSortKeyTo(best) > 0)
				{
					best = parsed;
					bestTag = tag;
				}
			}
			return bestTag;
		}
	}
}
