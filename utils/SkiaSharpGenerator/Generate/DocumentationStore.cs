using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SkiaSharpGenerator;

public sealed class DocumentationStore
{
	private string _source;
    private readonly Dictionary<string, string> _docs = new Dictionary<string, string>(StringComparer.Ordinal);

	public static string Type(string name) =>
        $"T:{name}";
    public static string Field(string typeName, string fieldName) =>
        $"F:{typeName}.{fieldName}";
    public static string Property(string typeName, string propertyName) =>
        $"P:{typeName}.{propertyName}";
	public static string Method(string typeName, string methodName, params string[] paramTypes) =>
        $"M:{typeName}.{methodName}({string.Join(",", paramTypes)})";

	public DocumentationStore(string sourceFile)
    {
        _source = File.ReadAllText(sourceFile);
    }

    public void Load()
    {
        var options = new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse);
        var tree = CSharpSyntaxTree.ParseText(_source, options);
        var root = tree.GetCompilationUnitRoot();

        foreach (var member in root.Members)
        {
            ProcessMember(member, null);
        }
    }

    public bool TryGet(string key, out string xml) =>
        _docs.TryGetValue(key, out xml!);

    private void ProcessMember(MemberDeclarationSyntax member, string? currentType)
    {
        if (member is NamespaceDeclarationSyntax nsDecl)
        {
            foreach (var child in nsDecl.Members)
            {
                ProcessMember(child, null);
            }

            return;
        }

        if (IsPublic(member, currentType))
        {
            AddIfAny(member, currentType);

            if (member is TypeDeclarationSyntax td)
            {
                var typeName = td.Identifier.Text;
                foreach (var m in td.Members)
                {
                    ProcessMember(m, typeName);
                }
            }
        }
    }

    private void AddIfAny(MemberDeclarationSyntax member, string? currentType)
    {
        var xml = Extract(member);
        if (xml == null)
            return;

        string? key = null;
        if (member is ClassDeclarationSyntax c)
            key = Type(c.Identifier.Text);
        else if (member is StructDeclarationSyntax s)
            key = Type(s.Identifier.Text);
        else if (member is InterfaceDeclarationSyntax i)
            key = Type(i.Identifier.Text);
        else if (member is EnumDeclarationSyntax e)
            key = Type(e.Identifier.Text);
        else if (member is DelegateDeclarationSyntax d)
            key = Type(d.Identifier.Text);
        else if (currentType != null && member is FieldDeclarationSyntax f)
            key = Field(currentType, f.Declaration.Variables.First().Identifier.Text);
        else if (currentType != null && member is PropertyDeclarationSyntax p)
            key = Property(currentType, p.Identifier.Text);
        else if (currentType != null && member is MethodDeclarationSyntax m)
        {
            var paramTypes = m.ParameterList.Parameters
                .Select(p => NormalizeType(p.Type))
                .ToArray();
            key = Method(currentType, m.Identifier.Text, paramTypes);
        }
        else if (currentType != null && member is OperatorDeclarationSyntax op)
        {
            var paramTypes = op.ParameterList.Parameters
                .Select(p => NormalizeType(p.Type))
                .ToArray();
            key = Method(currentType, GetOperatorMetadataName(op), paramTypes);
        }
        else if (currentType != null && member is ConversionOperatorDeclarationSyntax cop)
        {
            var paramTypes = cop.ParameterList.Parameters
                .Select(p => NormalizeType(p.Type))
                .ToArray();
            key = Method(currentType, cop.ImplicitOrExplicitKeyword.IsKind(SyntaxKind.ImplicitKeyword) ? "op_Implicit" : "op_Explicit", paramTypes);
        }

        if (key != null)
            _docs[key] = xml;
    }

    private static string GetOperatorMetadataName(OperatorDeclarationSyntax op)
    {
        var paramCount = op.ParameterList.Parameters.Count;
        switch (op.OperatorToken.Kind())
        {
            case SyntaxKind.PlusToken: return paramCount == 1 ? "op_UnaryPlus" : "op_Addition";
            case SyntaxKind.MinusToken: return paramCount == 1 ? "op_UnaryNegation" : "op_Subtraction";
            case SyntaxKind.AsteriskToken: return "op_Multiply";
            case SyntaxKind.SlashToken: return "op_Division";
            case SyntaxKind.PercentToken: return "op_Modulus";
            case SyntaxKind.AmpersandToken: return "op_BitwiseAnd";
            case SyntaxKind.BarToken: return "op_BitwiseOr";
            case SyntaxKind.CaretToken: return "op_ExclusiveOr";
            case SyntaxKind.LessThanLessThanToken: return "op_LeftShift";
            case SyntaxKind.GreaterThanGreaterThanToken: return "op_RightShift";
            case SyntaxKind.EqualsEqualsToken: return "op_Equality";
            case SyntaxKind.ExclamationEqualsToken: return "op_Inequality";
            case SyntaxKind.LessThanToken: return "op_LessThan";
            case SyntaxKind.GreaterThanToken: return "op_GreaterThan";
            case SyntaxKind.LessThanEqualsToken: return "op_LessThanOrEqual";
            case SyntaxKind.GreaterThanEqualsToken: return "op_GreaterThanOrEqual";
            case SyntaxKind.PlusPlusToken: return "op_Increment";
            case SyntaxKind.MinusMinusToken: return "op_Decrement";
            case SyntaxKind.ExclamationToken: return "op_LogicalNot";
            case SyntaxKind.TildeToken: return "op_OnesComplement";
            case SyntaxKind.TrueKeyword: return "op_True";
            case SyntaxKind.FalseKeyword: return "op_False";
            default: return "op_Unknown"; // fallback; unlikely but ensures a key if docs exist
        }
    }

    private static string NormalizeType(TypeSyntax? typeSyntax)
    {
        if (typeSyntax == null)
            return "?";
        // Strip trivia and normalize whitespace
        var text = typeSyntax.ToString().Trim();
        // Collapse spaces
        while (text.Contains("  "))
            text = text.Replace("  ", " ");
        return text;
    }

    private static bool IsPublic(MemberDeclarationSyntax member, string? currentType)
    {
        // Namespace always allowed
        if (member is NamespaceDeclarationSyntax)
            return true;

        // Enum members (EnumMemberDeclarationSyntax) are always effectively public in a public enum
        if (member is EnumDeclarationSyntax enumDecl)
            return enumDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));

        // For type declarations: require 'public'
        if (member is TypeDeclarationSyntax typeDecl)
            return typeDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));

        // Inside interfaces everything is public implicitly
        if (currentType != null && member.Parent is InterfaceDeclarationSyntax)
            return true;

        // Methods/fields/properties: must be public
        var mods = member.Modifiers;
        return mods.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
    }

    private static string? Extract(MemberDeclarationSyntax member)
    {
        var comments = member.GetLeadingTrivia()
            .Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
            .ToList();

        if (comments.Count == 0)
            return null;

        var docLines = comments
            .SelectMany(c => c.ToString().Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToList();

        var docs = "/// " + string.Join("\n", docLines);

        return docs;
    }
}
