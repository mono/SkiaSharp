using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;

namespace ApiDocsMigrator;

internal sealed class DocumentationMigrator(MigrationOptions options)
{
	private static readonly HashSet<string> SupportedDocumentationElements = new(StringComparer.Ordinal)
	{
		"summary", "remarks", "param", "typeparam", "returns", "value", "exception",
		"example", "permission", "seealso", "include", "inheritdoc",
	};

	public async Task<MigrationResult> RunAsync()
	{
		var stopwatch = Stopwatch.StartNew();
		var declarations = await FindDeclarationsAsync();
		var changes = new List<FileChange>();
		var errors = new List<string>();
		var documentation = LoadDocumentation(options.DocumentationRoot, declarations.Keys, errors);

		foreach (var entry in documentation.Values.OrderBy(entry => entry.DocId, StringComparer.Ordinal))
		{
			if (!declarations.TryGetValue(entry.Key, out var candidates))
			{
				errors.Add($"missing DocId: {entry.DocId} ({entry.SourcePath})");
				continue;
			}

			string xml;
			try
			{
				xml = ConvertDocs(entry);
			}
			catch (MigrationException ex)
			{
				errors.Add(ex.Message);
				continue;
			}

			if (candidates.Count == 1)
			{
				AddIfUndocumented(candidates[0], entry.DocId, xml);
			}
			else if (candidates.Count > 1 && entry.Key.Kind == "T")
			{
				var owner = candidates.OrderBy(candidate => IsBaseSource(candidate.Path) ? 0 : 1)
					.ThenBy(candidate => candidate.Path, StringComparer.Ordinal).First();
				AddIfUndocumented(owner, entry.DocId, xml);
			}
			else if (candidates.Count > 1)
			{
				errors.Add($"ambiguous DocId: {entry.DocId} ({FormatCandidates(candidates)})");
			}
		}

		stopwatch.Stop();
		if (errors.Count != 0 && !options.ApplyResolved)
			throw new MigrationException(BuildFailureReport(errors));

		var changedFiles = ApplyChanges(changes, options.DryRun);
		return new MigrationResult(
			changes.Count, changedFiles, errors,
			documentation.Count, declarations.Sum(pair => pair.Value.Count), stopwatch.Elapsed);

		void AddIfUndocumented(Declaration declaration, string docId, string xml)
		{
			if (!options.OnlyUndocumented || !HasDocumentation(declaration.Node))
				changes.Add(new FileChange(declaration, xml));
		}
	}

	private async Task<Dictionary<DeclarationKey, List<Declaration>>> FindDeclarationsAsync()
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
		return CollectCompilationDeclarations(compilation);
	}

	private static Dictionary<DeclarationKey, List<Declaration>> FindDeclarationsFromSource(string sourceRoot)
	{
		var result = new Dictionary<DeclarationKey, List<Declaration>>();
		var paths = Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
			.Where(path => !IsBuildOutput(path)).OrderBy(path => path, StringComparer.Ordinal).ToArray();
		var source = paths.ToDictionary(path => path, File.ReadAllText, StringComparer.Ordinal);
		var symbols = source.Values.SelectMany(text => Regex.Matches(text, @"(?m)^\s*#(?:if|elif)\s+!?(\w+)")
			.Select(match => match.Groups[1].Value)).Distinct(StringComparer.Ordinal).ToArray();
		CSharpParseOptions[] parseOptions = symbols.Length == 0
			? [new CSharpParseOptions()]
			: [new CSharpParseOptions(), new CSharpParseOptions(preprocessorSymbols: symbols)];
		foreach (var path in paths)
		{
			foreach (var options in parseOptions)
				CollectMembers(CSharpSyntaxTree.ParseText(source[path], options, path).GetRoot(), null, null, path, result);
		}
		foreach (var declarations in result.Values)
		{
			var unique = declarations.GroupBy(candidate => (candidate.Path, candidate.Node.SpanStart))
				.Select(group => group.First()).ToArray();
			declarations.Clear();
			declarations.AddRange(unique);
		}
		return result;
	}

	private static Dictionary<DeclarationKey, List<Declaration>> CollectCompilationDeclarations(Compilation compilation)
	{
		var result = new Dictionary<DeclarationKey, List<Declaration>>();
		foreach (var tree in compilation.SyntaxTrees)
		{
			var model = compilation.GetSemanticModel(tree);
			foreach (var node in tree.GetRoot().DescendantNodes().Where(IsDocumentableDeclaration))
			{
				var symbol = model.GetDeclaredSymbol(node);
				var docId = symbol?.GetDocumentationCommentId();
				if (docId is null)
					continue;

				Add(GetDocumentationTarget(node), ParseDocId(docId), tree.FilePath, result);
			}
		}

		foreach (var declarations in result.Values)
		{
			var unique = declarations.GroupBy(candidate => (candidate.Path, candidate.Node.SpanStart))
				.Select(group => group.First()).ToArray();
			declarations.Clear();
			declarations.AddRange(unique);
		}
		return result;
	}

	private static bool IsDocumentableDeclaration(SyntaxNode node) =>
		node is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax or
		BaseMethodDeclarationSyntax or BasePropertyDeclarationSyntax or
		EventDeclarationSyntax or EnumMemberDeclarationSyntax or
		VariableDeclaratorSyntax { Parent.Parent: FieldDeclarationSyntax or EventFieldDeclarationSyntax };

	private static SyntaxNode GetDocumentationTarget(SyntaxNode node) =>
		node is VariableDeclaratorSyntax { Parent.Parent: MemberDeclarationSyntax declaration }
			? declaration
			: node;

	private static void CollectMembers(SyntaxNode node, string? namespaceName, string? containingType,
		string path, Dictionary<DeclarationKey, List<Declaration>> result)
	{
		foreach (var child in node.ChildNodes())
		{
			switch (child)
			{
				case NamespaceDeclarationSyntax ns:
					CollectMembers(ns, Join(namespaceName, ns.Name.ToString()), null, path, result);
					break;
				case FileScopedNamespaceDeclarationSyntax ns:
					CollectMembers(ns, Join(namespaceName, ns.Name.ToString()), null, path, result);
					break;
				case BaseTypeDeclarationSyntax type:
					Add(type, TypeKey(namespaceName, containingType, type.Identifier.Text, TypeArity(type)),
						path, result);
					CollectMembers(type, namespaceName,
						TypeName(namespaceName, containingType, type.Identifier.Text, TypeArity(type)), path, result);
					break;
				case DelegateDeclarationSyntax delegateDeclaration:
					Add(delegateDeclaration, TypeKey(namespaceName, containingType, delegateDeclaration.Identifier.Text,
						delegateDeclaration.TypeParameterList?.Parameters.Count ?? 0), path, result);
					break;
				case ConstructorDeclarationSyntax constructor when containingType is not null:
					Add(constructor, new DeclarationKey("M", containingType, "#ctor", ParameterTypes(constructor.ParameterList)), path, result);
					break;
				case DestructorDeclarationSyntax destructor when containingType is not null:
					Add(destructor, new DeclarationKey("M", containingType, "Finalize", ""), path, result);
					break;
				case MethodDeclarationSyntax method when containingType is not null:
					Add(method, new DeclarationKey("M", containingType,
						method.Identifier.Text + GenericArity(method.TypeParameterList?.Parameters.Count ?? 0),
						ParameterTypes(method.ParameterList)), path, result);
					break;
				case OperatorDeclarationSyntax op when containingType is not null:
					Add(op, new DeclarationKey("M", containingType, OperatorName(op), ParameterTypes(op.ParameterList)), path, result);
					break;
				case ConversionOperatorDeclarationSyntax conversion when containingType is not null:
					Add(conversion, new DeclarationKey("M", containingType,
						conversion.ImplicitOrExplicitKeyword.IsKind(SyntaxKind.ImplicitKeyword) ? "op_Implicit" : "op_Explicit",
						ParameterTypes(conversion.ParameterList)), path, result);
					break;
				case PropertyDeclarationSyntax property when containingType is not null:
					Add(property, new DeclarationKey("P", containingType, property.Identifier.Text, ""), path, result);
					break;
				case IndexerDeclarationSyntax indexer when containingType is not null:
					Add(indexer, new DeclarationKey("P", containingType, "Item", ParameterTypes(indexer.ParameterList)), path, result);
					break;
				case EventDeclarationSyntax @event when containingType is not null:
					Add(@event, new DeclarationKey("E", containingType, @event.Identifier.Text, ""), path, result);
					break;
				case EventFieldDeclarationSyntax eventField when containingType is not null:
					foreach (var variable in eventField.Declaration.Variables)
						Add(variable, new DeclarationKey("E", containingType, variable.Identifier.Text, ""), path, result, eventField);
					break;
				case FieldDeclarationSyntax field when containingType is not null:
					foreach (var variable in field.Declaration.Variables)
						Add(variable, new DeclarationKey("F", containingType, variable.Identifier.Text, ""), path, result, field);
					break;
				case EnumMemberDeclarationSyntax enumMember when containingType is not null:
					Add(enumMember, new DeclarationKey("F", containingType, enumMember.Identifier.Text, ""), path, result);
					break;
			}
		}
	}

	private static void Add(SyntaxNode node, DeclarationKey key, string path,
		Dictionary<DeclarationKey, List<Declaration>> result, SyntaxNode? documentationTarget = null)
	{
		if (!result.TryGetValue(key, out var declarations))
			result.Add(key, declarations = []);
		declarations.Add(new Declaration(path, documentationTarget ?? node, IsGeneratedSource(path)));
	}

	private static Dictionary<string, EcmaDocumentation> LoadDocumentation(string documentationRoot,
		IEnumerable<DeclarationKey> declarationKeys, List<string> errors)
	{
		var hasApiRoot = Directory.Exists(Path.Combine(documentationRoot, "SkiaSharpAPI"));
		var root = hasApiRoot ? Path.Combine(documentationRoot, "SkiaSharpAPI")
			: documentationRoot;
		var paths = declarationKeys.Where(key => key.Kind == "T")
			.SelectMany(key => DocumentationPaths(root, key.TypeName, includeFlatPath: !hasApiRoot))
			.Where(File.Exists).Distinct(StringComparer.Ordinal).OrderBy(path => path, StringComparer.Ordinal);
		var result = new Dictionary<string, EcmaDocumentation>(StringComparer.Ordinal);
		foreach (var path in paths)
		{
			var document = XDocument.Load(path, LoadOptions.PreserveWhitespace);
			if (document.Root?.Name.LocalName != "Type")
				continue;
			Add(TypeDocumentation(document.Root, path));
			foreach (var member in document.Root.Element("Members")?.Elements("Member") ?? [])
				Add(MemberDocumentation(member, path));
		}
		return result;

		void Add(EcmaDocumentation? entry)
		{
			if (entry is null)
				return;
			if (!result.TryAdd(entry.DocId, entry))
				errors.Add($"duplicate DocId: {entry.DocId} ({result[entry.DocId].SourcePath}; {entry.SourcePath})");
		}
	}

	private static IEnumerable<string> DocumentationPaths(string root, string typeName, bool includeFlatPath)
	{
		var separator = typeName.LastIndexOf('.');
		if (separator < 0)
			yield break;

		var type = typeName[(separator + 1)..].Split('`')[0] + ".xml";
		yield return Path.Combine(root, typeName[..separator].Replace('.', Path.DirectorySeparatorChar), type);
		yield return Path.Combine(root, typeName[..separator], type);
		if (includeFlatPath)
			yield return Path.Combine(root, type);
	}

	private static EcmaDocumentation? TypeDocumentation(XElement type, string path) =>
		CreateDocumentation(type.Elements("TypeSignature").FirstOrDefault(e => (string?)e.Attribute("Language") == "DocId")
			?.Attribute("Value")?.Value, type.Element("Docs"), path);

	private static EcmaDocumentation? MemberDocumentation(XElement member, string path) =>
		CreateDocumentation(member.Elements("MemberSignature").FirstOrDefault(e => (string?)e.Attribute("Language") == "DocId")
			?.Attribute("Value")?.Value, member.Element("Docs"), path);

	private static EcmaDocumentation? CreateDocumentation(string? docId, XElement? docs, string path)
	{
		if (docId is null && docs is null)
			return null;
		if (docId is null || docs is null)
			throw new MigrationException($"Malformed ECMA document: {path}");
		return new EcmaDocumentation(docId, ParseDocId(docId), docs, path);
	}

	private static DeclarationKey ParseDocId(string docId)
	{
		var kind = docId[..1];
		var body = docId[2..];
		if (kind == "T")
			return new DeclarationKey(kind, body, string.Empty, "");
		var open = body.IndexOf('(');
		var close = open < 0 ? -1 : body.IndexOf(')', open);
		var head = open < 0 ? body : body[..open];
		var parameters = open < 0 ? "" : string.Join(",", SplitParameters(body[(open + 1)..close]).Select(NormalizeType));
		var split = head.LastIndexOf('.');
		return new DeclarationKey(kind, head[..split], head[(split + 1)..], parameters);
	}

	private static string ConvertDocs(EcmaDocumentation entry)
	{
		var elements = entry.Docs.Elements().ToArray();
		if (elements.Length == 0)
			throw new MigrationException($"Empty <Docs> element for {entry.DocId}");
		var unsupported = elements.Where(element => !SupportedDocumentationElements.Contains(element.Name.LocalName))
			.Select(element => element.Name.LocalName).Distinct(StringComparer.Ordinal).ToArray();
		if (unsupported.Length != 0)
			throw new MigrationException($"Unsupported ECMA documentation elements for {entry.DocId}: {string.Join(", ", unsupported)}");
		if (entry.Docs.Nodes().OfType<XText>().Any(text => !string.IsNullOrWhiteSpace(text.Value)))
			throw new MigrationException($"Unexpected text directly under <Docs> for {entry.DocId}");
		foreach (var element in elements)
			foreach (var paramRef in element.DescendantsAndSelf("paramref"))
				if (paramRef.Attribute("name") is { } name)
					name.Value = name.Value.Replace("&nbsp;", "", StringComparison.Ordinal).Replace("\u00a0", "", StringComparison.Ordinal).Trim();
		return string.Join("\n", elements.Select(element => element.ToString(SaveOptions.None)));
	}

	private static int ApplyChanges(IEnumerable<FileChange> changes, bool dryRun)
	{
		var files = changes.GroupBy(change => change.Declaration.Path, StringComparer.Ordinal)
			.OrderBy(group => group.Key, StringComparer.Ordinal).Select(group =>
			{
				var original = SourceText.From(File.ReadAllText(group.Key), Encoding.UTF8);
				var updated = original.WithChanges(group.GroupBy(change => change.Declaration.Node.SpanStart)
					.Select(declarations => declarations.Last()).Select(change => CreateReplacement(original, change))
					.OrderByDescending(change => change.Span.Start));
				return new PreparedFileChange(group.Key, original, updated);
			}).ToArray();
		var changed = files.Where(file => !file.Updated.ContentEquals(file.Original)).ToArray();
		if (!dryRun)
			foreach (var file in changed)
				File.WriteAllText(file.Path, file.Updated.ToString(), new UTF8Encoding(false));
		return changed.Length;
	}

	private static TextChange CreateReplacement(SourceText text, FileChange change)
	{
		var line = text.Lines.GetLineFromPosition(change.Declaration.Node.SpanStart);
		var indent = text.ToString(TextSpan.FromBounds(line.Start, change.Declaration.Node.SpanStart));
		if (indent.Any(character => character is not ' ' and not '\t'))
			throw new MigrationException($"Cannot determine indentation for {change.Declaration.Path}");
		var existing = change.Declaration.Node.GetLeadingTrivia()
			.FirstOrDefault(trivia => trivia.GetStructure() is DocumentationCommentTriviaSyntax);
		var start = line.Start;
		if (existing != default)
		{
			start = text.Lines.GetLineFromPosition(existing.FullSpan.Start).Start;
			var end = text.Lines.GetLineFromPosition(Math.Max(existing.FullSpan.Start, existing.FullSpan.End - 1)).EndIncludingLineBreak;
			return new TextChange(TextSpan.FromBounds(start, end), FormatDocumentation(change.Xml, indent));
		}
		return new TextChange(new TextSpan(start, 0), FormatDocumentation(change.Xml, indent));
	}

	private static string FormatDocumentation(string xml, string indent) =>
		string.Join("\n", xml.Replace("\r\n", "\n").Split('\n').Select(line =>
			line.Length == 0 ? $"{indent}///" : $"{indent}/// {line.TrimEnd()}")) + "\n";

	private static string ParameterTypes(BaseParameterListSyntax parameters) =>
		string.Join(",", parameters.Parameters.Select(parameter => NormalizeType(parameter.Type?.ToString() ?? "?")));

	private static string NormalizeType(string type)
	{
		type = type.Replace("global::", "").Replace("@", "").Replace("?", "").Replace("ref ", "").Replace("out ", "").Replace("in ", "")
			.Replace('{', '<').Replace('}', '>').Trim();
		foreach (var pair in PrimitiveTypes)
			type = type.Replace(pair.Key, pair.Value, StringComparison.Ordinal);
		return string.Concat(type.Select(character => char.IsWhiteSpace(character) ? '\0' : character))
			.Replace("\0", "").Split('.').Last();
	}

	private static readonly Dictionary<string, string> PrimitiveTypes = new(StringComparer.Ordinal)
	{
		["System.Boolean"] = "bool", ["System.Byte"] = "byte", ["System.SByte"] = "sbyte",
		["System.Char"] = "char", ["System.Decimal"] = "decimal", ["System.Double"] = "double",
		["System.Single"] = "float", ["System.Int32"] = "int", ["System.UInt32"] = "uint",
		["System.Int64"] = "long", ["System.UInt64"] = "ulong", ["System.Int16"] = "short",
		["System.UInt16"] = "ushort", ["System.String"] = "string", ["System.Object"] = "object",
		["System.Void"] = "void",
	};

	private static IEnumerable<string> SplitParameters(string parameters)
	{
		var depth = 0; var start = 0;
		for (var i = 0; i < parameters.Length; i++)
		{
			depth += parameters[i] is '{' or '[' ? 1 : parameters[i] is '}' or ']' ? -1 : 0;
			if (parameters[i] == ',' && depth == 0) { yield return parameters[start..i]; start = i + 1; }
		}
		if (parameters.Length > 0) yield return parameters[start..];
	}

	private static DeclarationKey TypeKey(string? ns, string? containing, string name, int arity) =>
		new("T", TypeName(ns, containing, name, arity), string.Empty, "");
	private static int TypeArity(BaseTypeDeclarationSyntax type) => type switch
	{
		TypeDeclarationSyntax declaration => declaration.TypeParameterList?.Parameters.Count ?? 0,
		_ => 0,
	};
	private static string TypeName(string? ns, string? containing, string name, int arity) =>
		Join(containing ?? ns, name + GenericArity(arity));
	private static string GenericArity(int arity) => arity == 0 ? "" : $"`{arity}";
	private static string Join(string? prefix, string name) => string.IsNullOrEmpty(prefix) ? name : $"{prefix}.{name}";
	private static string OperatorName(OperatorDeclarationSyntax op) => op.OperatorToken.Kind() switch
	{
		SyntaxKind.PlusToken => op.ParameterList.Parameters.Count == 1 ? "op_UnaryPlus" : "op_Addition",
		SyntaxKind.MinusToken => op.ParameterList.Parameters.Count == 1 ? "op_UnaryNegation" : "op_Subtraction",
		SyntaxKind.AsteriskToken => "op_Multiply", SyntaxKind.SlashToken => "op_Division", SyntaxKind.EqualsEqualsToken => "op_Equality",
		SyntaxKind.ExclamationEqualsToken => "op_Inequality", _ => $"op_{op.OperatorToken.Text}",
	};
	private static bool IsBuildOutput(string path) => path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
		.Any(segment => segment is "bin" or "obj" or ".git");
	private static bool IsGeneratedSource(string path) => path.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase);
	private static bool HasDocumentation(SyntaxNode node) =>
		node.GetLeadingTrivia().Any(trivia => trivia.GetStructure() is DocumentationCommentTriviaSyntax);
	private static bool IsBaseSource(string path) => !Path.GetFileNameWithoutExtension(path).Contains('.', StringComparison.Ordinal);
	private static string FormatCandidates(IEnumerable<Declaration> candidates) =>
		string.Join(", ", candidates.Select(candidate => candidate.Path).Distinct().OrderBy(path => path, StringComparer.Ordinal));
	private static string BuildFailureReport(IEnumerable<string> errors) =>
		"Documentation migration failed. No source files were modified:\n" +
		string.Join("\n", errors.OrderBy(error => error, StringComparer.Ordinal).Select(error => $"  {error}"));

	private sealed record EcmaDocumentation(string DocId, DeclarationKey Key, XElement Docs, string SourcePath);
	private sealed record Declaration(string Path, SyntaxNode Node, bool IsGenerated);
	private sealed record FileChange(Declaration Declaration, string Xml);
	private sealed record PreparedFileChange(string Path, SourceText Original, SourceText Updated);
}

internal sealed record DeclarationKey(string Kind, string TypeName, string MemberName, string ParameterSignature);
internal sealed record MigrationResult(int UpdatedDocumentCount, int UpdatedFileCount, IReadOnlyList<string> Unresolved,
	int ParsedDocumentationCount, int DeclarationCount, TimeSpan Duration);
internal sealed class MigrationException(string message) : Exception(message);
