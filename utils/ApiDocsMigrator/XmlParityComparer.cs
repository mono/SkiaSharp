using System.Xml.Linq;

namespace ApiDocsMigrator;

internal static class XmlParityComparer
{
	public static XmlParityResult Compare(string packageXmlPath, string compiledXmlPath)
	{
		var expected = Load(packageXmlPath, isPackage: true);
		var actual = Load(compiledXmlPath, isPackage: false);
		var missing = expected.Keys.Except(actual.Keys, StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray();
		var unexpected = actual.Keys.Except(expected.Keys, StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray();
		var changed = expected.Keys.Intersect(actual.Keys, StringComparer.Ordinal)
			.Where(key => !string.Equals(expected[key], actual[key], StringComparison.Ordinal))
			.OrderBy(value => value, StringComparer.Ordinal)
			.ToArray();
		return new XmlParityResult(expected.Count, actual.Count, missing, unexpected, changed);
	}

	private static Dictionary<string, string> Load(string path, bool isPackage)
	{
		var document = XDocument.Load(path, LoadOptions.PreserveWhitespace);
		if (document.Root?.Name.LocalName != "doc")
			throw new MigrationException($"Documentation XML does not have a <doc> root: {path}");

		var result = new Dictionary<string, string>(StringComparer.Ordinal);
		foreach (var member in document.Root.Element("members")?.Elements("member") ?? [])
		{
			var id = (string?)member.Attribute("name");
			if (string.IsNullOrWhiteSpace(id))
				throw new MigrationException($"Documentation member is missing a name: {path}");

			var clone = new XElement(member);
			NormalizeParameterReferences(clone);
			RemoveInsignificantWhitespace(clone);
			NormalizeCDataIndentation(clone);
			var normalizedId = isPackage ? NormalizePackageDocId(id) : id.Replace('+', '.');
			var content = string.Join("\n", clone.Nodes()
				.Where(node => node is not XText text || !string.IsNullOrWhiteSpace(text.Value))
				.OrderBy(node => node is XElement element ? DocumentationElementOrder(element.Name.LocalName) : int.MaxValue)
				.Select(node => node.ToString(SaveOptions.DisableFormatting)));
			if (content.Length == 0)
				continue;
			if (!result.TryAdd(normalizedId, content))
				throw new MigrationException($"Duplicate normalized DocId {normalizedId}: {path}");
		}
		return result;
	}

	private static string NormalizePackageDocId(string docId)
	{
		docId = docId.Replace('+', '.');
		if (!docId.StartsWith("C:", StringComparison.Ordinal))
			return docId;

		var parameterStart = docId.IndexOf('(');
		var typeName = parameterStart < 0 ? docId[2..] : docId[2..parameterStart];
		var parameters = parameterStart < 0 ? string.Empty : docId[parameterStart..];
		return $"M:{typeName}.#ctor{parameters}";
	}

	private static void NormalizeParameterReferences(XElement member)
	{
		foreach (var paramRef in member.Descendants("paramref"))
		{
			if (paramRef.Attribute("name") is not { } name)
				continue;
			name.Value = name.Value.Replace("&nbsp;", string.Empty, StringComparison.Ordinal)
				.Replace("\u00a0", string.Empty, StringComparison.Ordinal)
				.Trim();
		}
	}

	private static void RemoveInsignificantWhitespace(XElement member)
	{
		foreach (var element in member.DescendantsAndSelf())
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

	private static void NormalizeCDataIndentation(XElement member)
	{
		foreach (var cdata in member.DescendantNodes().OfType<XCData>())
			cdata.Value = RemoveCommonIndentation(cdata.Value);
	}

	private static string RemoveCommonIndentation(string value)
	{
		var lines = value.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
		var nonEmptyLines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
		if (nonEmptyLines.Length == 0)
			return value;

		var indentation = nonEmptyLines.Min(line => line.TakeWhile(char.IsWhiteSpace).Count());
		if (indentation == 0)
			return string.Join("\n", lines.Select(line => line.TrimEnd()));

		return string.Join("\n", lines.Select(line =>
		{
			var removable = Math.Min(indentation, line.TakeWhile(char.IsWhiteSpace).Count());
			return line[removable..].TrimEnd();
		}));
	}

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
}

internal sealed record XmlParityResult(
	int ExpectedCount,
	int ActualCount,
	IReadOnlyList<string> Missing,
	IReadOnlyList<string> Unexpected,
	IReadOnlyList<string> Changed)
{
	public bool HasDifferences =>
		Missing.Count != 0 || Unexpected.Count != 0 || Changed.Count != 0;

	public string Format()
	{
		var lines = new List<string>
		{
			$"XML parity: expected={ExpectedCount}, actual={ActualCount}, missing={Missing.Count}, unexpected={Unexpected.Count}, changed={Changed.Count}.",
		};
		Add(lines, "missing", Missing);
		Add(lines, "unexpected", Unexpected);
		Add(lines, "changed", Changed);
		return string.Join(Environment.NewLine, lines);
	}

	private static void Add(List<string> lines, string category, IReadOnlyList<string> docIds)
	{
		foreach (var docId in docIds)
			lines.Add($"  {category}: {docId}");
	}
}
