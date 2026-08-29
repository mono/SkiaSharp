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
			=> Locate(body) is null
				? Contracts.ManagedMarkerState.None
				: Contracts.ManagedMarkerState.Complete;

		public static string BuildInitialBody(string generatedNotesBody) =>
			$"{SummaryStart}\n\n{SummaryEnd}\n\n" +
			$"{GeneratedNotesStart}\n{generatedNotesBody.Trim()}\n{GeneratedNotesEnd}\n";

		public static bool HasGeneratedNotes(string body)
		{
			var positions = Locate(body);
			if (positions is null)
				return false;
			var start = positions.Value.GeneratedStart + GeneratedNotesStart.Length;
			return !string.IsNullOrWhiteSpace(
				body[start..positions.Value.GeneratedEnd]);
		}

		public static string ReplaceGeneratedNotes(string body, string generatedNotes)
		{
			var positions = Locate(body) ??
				throw new GitHubException("release body has no managed markers");
			var ownedStart = positions.GeneratedStart + GeneratedNotesStart.Length;
			return body[..ownedStart] +
				"\n" +
				generatedNotes.Trim() +
				"\n" +
				body[positions.GeneratedEnd..];
		}

		private static MarkerPositions? Locate(string body)
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
				return null;
			if (positions.Any(static position => position < 0))
				throw new GitHubException("release body has incomplete managed markers");
			if (!(positions[0] < positions[1] &&
				positions[1] < positions[2] &&
				positions[2] < positions[3]))
			{
				throw new GitHubException("release body managed markers are out of order");
			}
			return new MarkerPositions(
				positions[0],
				positions[1],
				positions[2],
				positions[3]);
		}

		private readonly record struct MarkerPositions(
			int SummaryStart,
			int SummaryEnd,
			int GeneratedStart,
			int GeneratedEnd);
	}
}
