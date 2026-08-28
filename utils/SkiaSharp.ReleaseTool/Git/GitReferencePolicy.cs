namespace SkiaSharp.ReleaseTool.Git
{
	public static class GitReferencePolicy
	{
		public static bool IsFullyQualified(string? reference)
		{
			if (string.IsNullOrEmpty(reference))
				return false;

			var parts = reference.Split('/');
			return reference.StartsWith("refs/", StringComparison.Ordinal) &&
				reference.Length > "refs/".Length &&
				!reference.EndsWith("/", StringComparison.Ordinal) &&
				reference != "@" &&
				!reference.Contains("..", StringComparison.Ordinal) &&
				!reference.Contains("@{", StringComparison.Ordinal) &&
				!reference.Any(static character =>
					char.IsControl(character) ||
					char.IsWhiteSpace(character) ||
					character is '~' or '^' or ':' or '?' or '*' or '[' or '\\') &&
				!parts.Any(static part =>
					part.Length == 0 ||
					part == "." ||
					part.StartsWith(".", StringComparison.Ordinal) ||
					part.EndsWith(".", StringComparison.Ordinal) ||
					part.EndsWith(".lock", StringComparison.OrdinalIgnoreCase));
		}
	}
}
