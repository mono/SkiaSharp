using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Finishing
{
	internal static class ManagedReleaseMarkers
	{
		public const string SummaryStart = "<!-- SKIASHARP:RELEASE-SUMMARY:START -->";
		public const string SummaryEnd = "<!-- SKIASHARP:RELEASE-SUMMARY:END -->";
		public const string GeneratedNotesStart = "<!-- SKIASHARP:GITHUB-GENERATED-NOTES:START -->";
		public const string GeneratedNotesEnd = "<!-- SKIASHARP:GITHUB-GENERATED-NOTES:END -->";

		public static Contracts.ManagedMarkerState Inspect(string body)
		{
			var markers = new[]
			{
				SummaryStart,
				SummaryEnd,
				GeneratedNotesStart,
				GeneratedNotesEnd,
			};
			var positions = new int[markers.Length];
			for (var index = 0; index < markers.Length; index++)
			{
				var first = body.IndexOf(markers[index], StringComparison.Ordinal);
				if (first < 0)
				{
					positions[index] = -1;
					continue;
				}
				if (body.IndexOf(markers[index], first + markers[index].Length, StringComparison.Ordinal) >= 0)
					throw new GitHubException("release body has duplicate managed markers");
				positions[index] = first;
			}

			if (positions.All(static position => position < 0))
				return Contracts.ManagedMarkerState.None;
			if (positions.Any(static position => position < 0))
				throw new GitHubException("release body has incomplete managed markers");
			if (!(positions[0] < positions[1] &&
				positions[1] < positions[2] &&
				positions[2] < positions[3]))
			{
				throw new GitHubException("release body managed markers are out of order");
			}
			return Contracts.ManagedMarkerState.Complete;
		}
	}
}
