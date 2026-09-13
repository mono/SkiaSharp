using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ApiDocsMigrator;

internal static class DocumentationSourceFormatter
{
	public static SourceFormattingResult Format(string sourceRoot)
	{
		var updatedFileCount = 0;
		var documentationCommentCount = 0;

		foreach (var path in Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
			.Where(path => !IsBuildOutput(path))
			.OrderBy(path => path, StringComparer.Ordinal))
		{
			var text = SourceText.From(File.ReadAllText(path), Encoding.UTF8);
			var replacements = GetDocumentationTrivia(text, path)
				.Select(trivia => CreateReplacement(text, trivia))
				.Where(change => change is not null)
				.Select(change => change!.Value)
				.OrderByDescending(change => change.Span.Start)
				.ToArray();
			documentationCommentCount += replacements.Length;

			if (replacements.Length == 0)
				continue;

			var updated = text.WithChanges(replacements);
			if (updated.ContentEquals(text))
				continue;

			File.WriteAllText(path, updated.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			updatedFileCount++;
		}

		return new SourceFormattingResult(documentationCommentCount, updatedFileCount);
	}

	private static IEnumerable<SyntaxTrivia> GetDocumentationTrivia(SourceText text, string path)
	{
		var symbols = Regex.Matches(text.ToString(), @"(?m)^\s*#(?:if|elif)\s+(?:!|\()?(\w+)")
			.Select(match => match.Groups[1].Value)
			.Distinct(StringComparer.Ordinal)
			.ToArray();
		var options = new[] { new CSharpParseOptions(documentationMode: DocumentationMode.Parse) }
			.Concat(symbols.Select(symbol => new CSharpParseOptions(
				documentationMode: DocumentationMode.Parse,
				preprocessorSymbols: [symbol])));

		return options
			.SelectMany(option => CSharpSyntaxTree.ParseText(text, option, path).GetRoot().DescendantTrivia())
			.Where(trivia => trivia.GetStructure() is DocumentationCommentTriviaSyntax)
			.GroupBy(trivia => (trivia.FullSpan.Start, trivia.FullSpan.End))
			.Select(group => group.First());
	}

	private static TextChange? CreateReplacement(SourceText text, SyntaxTrivia trivia)
	{
		var path = trivia.SyntaxTree?.FilePath ?? "<unknown>";
		var line = text.Lines.GetLineFromPosition(trivia.FullSpan.Start);
		var indentation = text.ToString(TextSpan.FromBounds(line.Start, trivia.FullSpan.Start));
		if (indentation.Any(character => character is not ' ' and not '\t'))
			throw new MigrationException($"Cannot determine indentation for documentation comment at {path}");

		var xml = ParseDocumentationTrivia(trivia.ToFullString(), path);
		var formatted = FormatDocumentation(xml, indentation);
		var end = text.Lines.GetLineFromPosition(Math.Max(trivia.FullSpan.Start, trivia.FullSpan.End - 1)).EndIncludingLineBreak;
		var span = TextSpan.FromBounds(line.Start, end);
		return string.Equals(text.ToString(span), formatted, StringComparison.Ordinal)
			? null
			: new TextChange(span, formatted);
	}

	private static string ParseDocumentationTrivia(string documentationTrivia, string path)
	{
		var xml = string.Join("\n", documentationTrivia.Replace("\r\n", "\n", StringComparison.Ordinal)
			.Split('\n')
			.Select(line =>
			{
				var trimmed = line.TrimStart();
				if (trimmed.Length == 0)
					return string.Empty;
				if (!trimmed.StartsWith("///", StringComparison.Ordinal))
					throw new MigrationException($"Unexpected documentation comment syntax in {path}");

				var content = trimmed[3..];
				return content.StartsWith(" ", StringComparison.Ordinal) ? content[1..] : content;
			}));

		XElement root;
		try
		{
			root = XElement.Parse($"<root>{xml}</root>", LoadOptions.PreserveWhitespace);
		}
		catch (Exception ex) when (ex is System.Xml.XmlException)
		{
			throw new MigrationException($"Unable to format documentation comment in {path}: {ex.Message}");
		}

		RemoveInsignificantWhitespace(root);
		var elements = root.Elements().ToArray();
		if (elements.Length == 0)
			return xml.TrimEnd();

		foreach (var text in root.DescendantNodes().OfType<XText>().Where(text => text is not XCData))
		{
			if (!text.Value.Contains('\n', StringComparison.Ordinal))
				continue;

			text.Value = string.Join(" ", text.Value.Replace("\r\n", "\n", StringComparison.Ordinal)
				.Split('\n')
				.Select(line => line.Trim())
				.Where(line => line.Length > 0));
		}

		return string.Join("\n", elements
			.OrderBy(element => DocumentationElementOrder(element.Name.LocalName))
			.Select(element => element.ToString(SaveOptions.DisableFormatting)));
	}

	private static void RemoveInsignificantWhitespace(XElement root)
	{
		foreach (var element in root.DescendantsAndSelf())
		{
			if (!element.Elements().Any())
				continue;

			foreach (var whitespace in element.Nodes()
				.OfType<XText>()
				.Where(text => text is not XCData && string.IsNullOrWhiteSpace(text.Value))
				.ToArray())
			{
				whitespace.Remove();
			}
		}
	}

	private static string FormatDocumentation(string xml, string indentation) =>
		string.Join("\n", xml.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n')
			.Select(line => line.Length == 0 ? $"{indentation}///" : $"{indentation}/// {line.TrimEnd()}")) + "\n";

	private static int DocumentationElementOrder(string name) =>
		name switch
		{
			"summary" => 0,
			"inheritdoc" => 0,
			"include" => 0,
			"typeparam" => 1,
			"param" => 2,
			"returns" or "value" => 3,
			"exception" => 4,
			"remarks" => 5,
			"example" => 6,
			"seealso" => 7,
			"permission" => 8,
			_ => 9,
		};

	private static bool IsBuildOutput(string path) =>
		path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
			.Any(segment => segment is "bin" or "obj" or ".git");
}

internal sealed record SourceFormattingResult(int DocumentationCommentCount, int UpdatedFileCount);
