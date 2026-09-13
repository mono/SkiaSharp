using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;

namespace ApiDocsMigrator;

/// <summary>
/// Imports compiler XML documentation from a NuGet package into one source directory.
/// This is intentionally scoped to a single assembly so package XML and source compile
/// inputs can be compared without constructing a synthetic repository-wide project.
/// </summary>
internal sealed class PackageDocumentationMigrator(MigrationOptions options)
{
	public async Task<MigrationResult> RunAsync()
	{
		var stopwatch = Stopwatch.StartNew();
		var documentation = LoadDocumentation(options.PackageXmlPath!);
		var declarations = await FindDeclarationsAsync();
		var changes = new List<FileChange>();
		var errors = new List<string>();

		foreach (var entry in documentation.Values.OrderBy(entry => entry.DocId, StringComparer.Ordinal))
		{
			if (!declarations.TryGetValue(entry.DocId, out var candidates))
			{
				errors.Add($"missing source declaration: {entry.DocId}");
				continue;
			}

			var selected = SelectDeclarations(entry.DocId, candidates, errors);
			foreach (var declaration in selected)
				changes.Add(new FileChange(declaration, entry.DocId, entry.Xml));

			if (entry.DocId.StartsWith("T:", StringComparison.Ordinal) && candidates.Count > 1)
			{
				foreach (var declaration in candidates.Except(selected))
					changes.Add(new FileChange(declaration, entry.DocId, null));
			}
		}

		var duplicateTargets = changes
			.GroupBy(change => (change.Declaration.Path, change.Declaration.Node.SpanStart))
			.Where(group => group.Select(change => change.DocId).Distinct(StringComparer.Ordinal).Skip(1).Any())
			.ToArray();
		foreach (var duplicateTarget in duplicateTargets)
			errors.Add($"multiple package DocIds target one declaration: {FormatCandidates(duplicateTarget.Select(change => change.Declaration))}");

		if (errors.Count != 0 && !options.ApplyResolved)
			throw new MigrationException(BuildFailureReport(errors));

		var safeChanges = duplicateTargets.Length == 0
			? changes
			: changes.Where(change => !duplicateTargets.Any(group =>
				group.Key == (change.Declaration.Path, change.Declaration.Node.SpanStart))).ToList();
		var changedFiles = ApplyChanges(safeChanges, options.DryRun);
		stopwatch.Stop();

		return new MigrationResult(
			safeChanges.Count(change => change.Xml is not null),
			changedFiles,
			errors,
			documentation.Count,
			declarations.Sum(pair => pair.Value.Count),
			stopwatch.Elapsed);
	}

	private static Dictionary<string, PackageDocumentation> LoadDocumentation(string packageXmlPath)
	{
		var document = XDocument.Load(packageXmlPath, LoadOptions.PreserveWhitespace);
		if (document.Root?.Name.LocalName != "doc")
			throw new MigrationException($"Package documentation does not have a <doc> root: {packageXmlPath}");

		var result = new Dictionary<string, PackageDocumentation>(StringComparer.Ordinal);
		foreach (var member in document.Root.Element("members")?.Elements("member") ?? [])
		{
			var packageDocId = (string?)member.Attribute("name");
			if (string.IsNullOrWhiteSpace(packageDocId))
				throw new MigrationException($"Package documentation member is missing a name: {packageXmlPath}");

			var nodes = member.Nodes()
				.Where(node => node is not XText text || !string.IsNullOrWhiteSpace(text.Value))
				.Select(node => Clone(node))
				.ToArray();
			if (nodes.Length == 0)
				continue;

			NormalizePackageMarkup(nodes);
			RemoveInsignificantWhitespace(nodes);
			var xml = string.Join("\n", OrderDocumentationNodes(nodes)
				.Select(node => node.ToString(SaveOptions.DisableFormatting)));
			ValidateDocumentationComment(packageDocId, xml);
			var roslynDocId = NormalizeDocId(packageDocId);
			if (!result.TryAdd(roslynDocId, new PackageDocumentation(packageDocId, roslynDocId, xml)))
				throw new MigrationException($"Duplicate package DocId: {packageDocId}");
		}
		return result;
	}

	private static string NormalizeDocId(string packageDocId)
	{
		var docId = packageDocId.Replace('+', '.');
		if (!docId.StartsWith("C:", StringComparison.Ordinal))
			return docId;

		var openParenthesis = docId.IndexOf('(');
		var typeName = openParenthesis < 0 ? docId[2..] : docId[2..openParenthesis];
		var parameters = openParenthesis < 0 ? string.Empty : docId[openParenthesis..];
		return $"M:{typeName}.#ctor{parameters}";
	}

	private static void NormalizePackageMarkup(IEnumerable<XNode> nodes)
	{
		foreach (var paramRef in nodes.OfType<XElement>().SelectMany(element => element.DescendantsAndSelf("paramref")))
		{
			if (paramRef.Attribute("name") is not { } name)
				continue;

			// Historical mdoc package XML contains a literal "&nbsp;" suffix in a
			// few parameter names. The compiler rejects it as an invalid identifier.
			name.Value = name.Value.Replace("&nbsp;", string.Empty, StringComparison.Ordinal)
				.Replace("\u00a0", string.Empty, StringComparison.Ordinal)
				.Trim();
		}
	}

	private static void RemoveInsignificantWhitespace(IEnumerable<XNode> nodes)
	{
		foreach (var element in nodes.OfType<XElement>().SelectMany(element => element.DescendantsAndSelf()))
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

	private static IEnumerable<XNode> OrderDocumentationNodes(IEnumerable<XNode> nodes) =>
		nodes.OrderBy(node => node is XElement element ? DocumentationElementOrder(element.Name.LocalName) : int.MaxValue);

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

	private static XNode Clone(XNode node) =>
		node switch
		{
			XElement element => new XElement(element),
			XComment comment => new XComment(comment.Value),
			XCData cdata => new XCData(cdata.Value),
			XText text => new XText(text.Value),
			_ => throw new MigrationException($"Unsupported package XML node: {node.NodeType}"),
		};

	private static void ValidateDocumentationComment(string docId, string xml)
	{
		var source = $"/// {xml.Replace("\n", "\n/// ", StringComparison.Ordinal)}\ninternal class DocumentationValidation {{ }}";
		var diagnostics = CSharpSyntaxTree.ParseText(
				source,
				new CSharpParseOptions(documentationMode: DocumentationMode.Diagnose))
			.GetDiagnostics()
			.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
			.ToArray();
		if (diagnostics.Length != 0)
			throw new MigrationException($"Invalid package documentation for {docId}: {diagnostics[0].GetMessage()}");
	}

	private async Task<Dictionary<string, List<Declaration>>> FindDeclarationsAsync()
	{
		if (options.ProjectPath is null)
			return FindDeclarationsFromSource(options.SourceRoot);

		if (!MSBuildLocator.IsRegistered)
			MSBuildLocator.RegisterDefaults();

		var properties = new Dictionary<string, string>(StringComparer.Ordinal);
		if (options.Framework is not null)
		{
			properties["TargetFramework"] = options.Framework;
			properties["TargetFrameworks"] = options.Framework;
		}

		using var workspace = MSBuildWorkspace.Create(properties);
		var project = await workspace.OpenProjectAsync(options.ProjectPath);
		var compilation = await project.GetCompilationAsync() ??
			throw new MigrationException($"Unable to create a compilation for {options.ProjectPath}.");
		return CollectDeclarations(compilation);
	}

	private static Dictionary<string, List<Declaration>> FindDeclarationsFromSource(string sourceRoot)
	{
		var paths = Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
			.Where(path => !IsBuildOutput(path))
			.OrderBy(path => path, StringComparer.Ordinal)
			.ToArray();
		var trees = paths.Select(path =>
			CSharpSyntaxTree.ParseText(
				File.ReadAllText(path),
				new CSharpParseOptions(documentationMode: DocumentationMode.Parse),
				path)).ToArray();
		var compilation = CSharpCompilation.Create(
			"ApiDocsMigration",
			trees,
			PlatformReferences,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
		return CollectDeclarations(compilation);
	}

	private static Dictionary<string, List<Declaration>> CollectDeclarations(Compilation compilation)
	{
		var result = new Dictionary<string, List<Declaration>>(StringComparer.Ordinal);

		foreach (var tree in compilation.SyntaxTrees)
		{
			var model = compilation.GetSemanticModel(tree);
			foreach (var node in tree.GetRoot().DescendantNodes().Where(IsDocumentableDeclaration))
			{
				var symbol = model.GetDeclaredSymbol(node);
				var docId = symbol?.GetDocumentationCommentId();
				if (docId is null)
					continue;

				if (!result.TryGetValue(docId, out var declarations))
					result.Add(docId, declarations = []);
				declarations.Add(new Declaration(tree.FilePath, DocumentationTarget(node), node is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax));
			}
		}

		foreach (var declarations in result.Values)
		{
			var unique = declarations
				.GroupBy(declaration => (declaration.Path, declaration.Node.SpanStart))
				.Select(group => group.First())
				.ToArray();
			declarations.Clear();
			declarations.AddRange(unique);
		}
		return result;
	}

	private static ImmutableArray<MetadataReference> PlatformReferences { get; } =
		((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? string.Empty)
			.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
			.Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
			.ToImmutableArray();

	private static bool IsDocumentableDeclaration(SyntaxNode node) =>
		node is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax or
		BaseMethodDeclarationSyntax or BasePropertyDeclarationSyntax or
		EventDeclarationSyntax or EnumMemberDeclarationSyntax or
		VariableDeclaratorSyntax { Parent.Parent: FieldDeclarationSyntax or EventFieldDeclarationSyntax };

	private static SyntaxNode DocumentationTarget(SyntaxNode node) =>
		node is VariableDeclaratorSyntax { Parent.Parent: MemberDeclarationSyntax declaration }
			? declaration
			: node;

	private IEnumerable<Declaration> SelectDeclarations(
		string docId,
		IReadOnlyList<Declaration> candidates,
		ICollection<string> errors)
	{
		if (candidates.Count == 1)
			return candidates;
		if (docId.StartsWith("T:", StringComparison.Ordinal) && candidates.All(candidate => candidate.IsType))
		{
			// Put type docs on the shared/base partial declaration when one exists.
			return [candidates
				.OrderBy(candidate => IsBaseSource(candidate.Path) ? 0 : 1)
				.ThenBy(candidate => candidate.Path, StringComparer.Ordinal)
				.First()];
		}

		errors.Add($"ambiguous source declaration: {docId} ({FormatCandidates(candidates)})");
		return options.ApplyResolved ? candidates : [];
	}

	private static int ApplyChanges(IEnumerable<FileChange> changes, bool dryRun)
	{
		var files = changes.GroupBy(change => change.Declaration.Path, StringComparer.Ordinal)
			.OrderBy(group => group.Key, StringComparer.Ordinal)
			.Select(group =>
			{
				var original = SourceText.From(File.ReadAllText(group.Key), Encoding.UTF8);
				var replacements = group
					.GroupBy(change => change.Declaration.Node.SpanStart)
					.Select(group => group.Single())
					.Select(change => CreateReplacement(original, change))
					.Where(change => change is not null)
					.Select(change => change!.Value)
					.OrderByDescending(change => change.Span.Start);
				return new PreparedFileChange(group.Key, original, original.WithChanges(replacements));
			})
			.ToArray();
		var changedFiles = files.Where(file => !file.Updated.ContentEquals(file.Original)).ToArray();
		if (!dryRun)
			foreach (var file in changedFiles)
				File.WriteAllText(file.Path, file.Updated.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
		return changedFiles.Length;
	}

	private static TextChange? CreateReplacement(SourceText text, FileChange change)
	{
		var line = text.Lines.GetLineFromPosition(change.Declaration.Node.SpanStart);
		var indentation = text.ToString(TextSpan.FromBounds(line.Start, change.Declaration.Node.SpanStart));
		if (indentation.Any(character => character is not ' ' and not '\t'))
			throw new MigrationException($"Cannot determine indentation for {change.Declaration.Path}");

		var existing = change.Declaration.Node.GetLeadingTrivia()
			.FirstOrDefault(trivia => trivia.GetStructure() is DocumentationCommentTriviaSyntax);
		if (existing == default)
			return change.Xml is null
				? null
				: new TextChange(new TextSpan(line.Start, 0), FormatDocumentation(change.Xml, indentation));

		var start = text.Lines.GetLineFromPosition(existing.FullSpan.Start).Start;
		var end = text.Lines.GetLineFromPosition(Math.Max(existing.FullSpan.Start, existing.FullSpan.End - 1)).EndIncludingLineBreak;
		return new TextChange(
			TextSpan.FromBounds(start, end),
			change.Xml is null ? string.Empty : FormatDocumentation(change.Xml, indentation));
	}

	private static string FormatDocumentation(string xml, string indentation) =>
		string.Join("\n", xml.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n')
			.Select(line => line.Length == 0 ? $"{indentation}///" : $"{indentation}/// {line.TrimEnd()}")) + "\n";

	private static bool IsBuildOutput(string path) =>
		path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
			.Any(segment => segment is "bin" or "obj" or ".git");

	private static bool IsBaseSource(string path) =>
		!Path.GetFileNameWithoutExtension(path).Contains('.', StringComparison.Ordinal);

	private static string FormatCandidates(IEnumerable<Declaration> candidates) =>
		string.Join(", ", candidates.Select(candidate => candidate.Path).Distinct().OrderBy(path => path, StringComparer.Ordinal));

	private static string BuildFailureReport(IEnumerable<string> errors) =>
		"Package XML migration failed. No source files were modified:\n" +
		string.Join("\n", errors.OrderBy(error => error, StringComparer.Ordinal).Select(error => $"  {error}"));

	private sealed record PackageDocumentation(string PackageDocId, string DocId, string Xml);

	private sealed record Declaration(string Path, SyntaxNode Node, bool IsType);

	private sealed record FileChange(Declaration Declaration, string DocId, string? Xml);

	private sealed record PreparedFileChange(string Path, SourceText Original, SourceText Updated);
}
