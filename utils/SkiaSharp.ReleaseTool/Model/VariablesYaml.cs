using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Model
{
	public static class VariablesYaml
	{
		public static NuGetVersion ParseSkiaSharpVersion(string variablesText)
		{
			var value = ParseSkiaSharpVersionText(variablesText);
			return ReleaseVersionPolicy.ParseStableVersion(value, "SKIASHARP_VERSION", 3, 4);
		}

		internal static string ParseSkiaSharpVersionText(string variablesText) =>
			ParseExactlyOne(variablesText, "SKIASHARP_VERSION");

		public static string ParsePreviewLabel(string variablesText)
		{
			var value = ParseExactlyOne(variablesText, "PREVIEW_LABEL");
			ValidatePreviewLabel(value);
			return value;
		}

		internal static bool IsAssignment(string line, string name) =>
			TryParseAssignment(line, name, out _);

		internal static string ReplaceAssignment(string line, string name, string value, bool quote)
		{
			if (!TryGetAssignmentValueRange(line, name, out var start, out var end))
				throw new InvalidOperationException($"Line is not a {name} assignment.");
			var rendered = quote ? $"'{value}'" : value;
			return line[..start] + rendered + line[end..];
		}

		internal static void ValidatePreviewLabel(string value)
		{
			if (value == "stable")
				return;

			var parts = value.Split('.');
			if (parts.Length != 2 ||
				(parts[0] != "preview" && parts[0] != "rc") ||
				!int.TryParse(parts[1], out var iteration) ||
				iteration < 0)
			{
				throw new PlanException(
					$"invalid PREVIEW_LABEL '{value}': expected stable, preview.N, or rc.N");
			}
		}

		private static string ParseExactlyOne(string text, string name)
		{
			var values = TextFileLines.Split(text)
				.Select(line => TryParseAssignment(line.Content, name, out var value) ? value : null)
				.Where(static value => value is not null)
				.ToArray();
			if (values.Length != 1)
				throw new PlanException($"expected exactly one {name} assignment, found {values.Length}");
			return values[0]!;
		}

		private static bool TryParseAssignment(string line, string name, out string value)
		{
			value = "";
			if (!TryGetAssignmentValueRange(line, name, out var start, out var end))
				return false;

			var token = line[start..end];
			if (token.Length >= 2 && token[0] is '\'' or '"')
			{
				if (token[^1] != token[0])
					throw new PlanException($"{name} has an unterminated quoted value");
				token = token[1..^1];
			}
			else if (token.Any(char.IsWhiteSpace))
			{
				throw new PlanException($"{name} has an invalid unquoted value");
			}

			if (token.Length == 0)
				throw new PlanException($"{name} must not be empty");
			value = token;
			return true;
		}

		private static bool TryGetAssignmentValueRange(
			string line,
			string name,
			out int valueStart,
			out int valueEnd)
		{
			valueStart = 0;
			valueEnd = 0;
			var trimmedStart = line.AsSpan().TrimStart();
			if (!trimmedStart.StartsWith(name, StringComparison.Ordinal))
				return false;

			var nameOffset = line.Length - trimmedStart.Length;
			var colon = nameOffset + name.Length;
			if (colon >= line.Length || line[colon] != ':')
				return false;

			valueStart = colon + 1;
			while (valueStart < line.Length && char.IsWhiteSpace(line[valueStart]))
				valueStart++;
			valueEnd = line.Length;
			while (valueEnd > valueStart && char.IsWhiteSpace(line[valueEnd - 1]))
				valueEnd--;
			return true;
		}
	}
}
