using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Model
{
	internal sealed record TextFileLine(string Content, string Terminator);

	internal static class TextFileLines
	{
		public static IReadOnlyList<TextFileLine> Split(string text)
		{
			var lines = new List<TextFileLine>();
			var start = 0;
			for (var index = 0; index < text.Length;)
			{
				if (text[index] is not ('\r' or '\n'))
				{
					index++;
					continue;
				}

				var newlineLength = text[index] == '\r' &&
					index + 1 < text.Length &&
					text[index + 1] == '\n'
						? 2
						: 1;
				lines.Add(new TextFileLine(
					text[start..index],
					text.Substring(index, newlineLength)));
				index += newlineLength;
				start = index;
			}

			if (start < text.Length)
				lines.Add(new TextFileLine(text[start..], ""));
			return lines;
		}

		public static string ReplaceExactlyOnce(
			string text,
			Func<string, bool> predicate,
			Func<string, string> replacement,
			string description)
		{
			var lines = Split(text).ToArray();
			var matches = 0;
			for (var index = 0; index < lines.Length; index++)
			{
				if (!predicate(lines[index].Content))
					continue;
				matches++;
				lines[index] = lines[index] with { Content = replacement(lines[index].Content) };
			}

			if (matches != 1)
				throw new PlanException($"expected exactly one {description} row, found {matches}");
			return string.Concat(lines.Select(static line => line.Content + line.Terminator));
		}

		public static string ReplaceAll(
			string text,
			Func<string, bool> predicate,
			Func<string, string> replacement,
			int expectedMatches,
			string description)
		{
			var lines = Split(text).ToArray();
			var matches = 0;
			for (var index = 0; index < lines.Length; index++)
			{
				if (!predicate(lines[index].Content))
					continue;
				matches++;
				lines[index] = lines[index] with { Content = replacement(lines[index].Content) };
			}

			if (matches != expectedMatches)
				throw new PlanException($"expected {expectedMatches} {description} rows, found {matches}");
			return string.Concat(lines.Select(static line => line.Content + line.Terminator));
		}

		public static string ReplaceLastToken(string line, string value)
		{
			var end = line.Length;
			while (end > 0 && char.IsWhiteSpace(line[end - 1]))
				end--;
			var start = end;
			while (start > 0 && !char.IsWhiteSpace(line[start - 1]))
				start--;
			return line[..start] + value + line[end..];
		}

		public static string[] Columns(string line) =>
			line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
	}
}
