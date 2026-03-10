using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SkiaSharpGenerator;

public sealed class DocumentationStore(string sourceFile)
{
    private readonly string _source = File.ReadAllText(sourceFile);
    private readonly Dictionary<string, string> _docs = new(StringComparer.Ordinal);

    public static string Type(string name) =>
        $"T:{name}";
    public static string Field(string typeName, string fieldName) =>
        $"F:{typeName}.{fieldName}";
    public static string Property(string typeName, string propertyName) =>
        $"P:{typeName}.{propertyName}";
    public static string Method(string typeName, string methodName, params string[] paramTypes) =>
        $"M:{typeName}.{methodName}({string.Join(",", paramTypes)})";

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
            else if (member is EnumDeclarationSyntax ed)
            {
                var enumName = ed.Identifier.Text;
                foreach (var em in ed.Members)
                {
                    ProcessMember(em, enumName);
                }
            }
        }
    }

    private void AddIfAny(MemberDeclarationSyntax member, string? currentType)
    {
        if (GetDocKey(member, currentType) is { } key && Extract(member) is { } xml)
        {
            _docs[key] = xml;
        }
    }

    private static string? GetDocKey(MemberDeclarationSyntax member, string? currentType)
    {
        if (member is ClassDeclarationSyntax c)
            return Type(c.Identifier.Text);
        else if (member is StructDeclarationSyntax s)
            return Type(s.Identifier.Text);
        else if (member is InterfaceDeclarationSyntax i)
            return Type(i.Identifier.Text);
        else if (member is EnumDeclarationSyntax e)
            return Type(e.Identifier.Text);
        else if (member is EnumMemberDeclarationSyntax em && em.Parent is EnumDeclarationSyntax ep)
            return Field(ep.Identifier.Text, em.Identifier.Text);
        else if (member is DelegateDeclarationSyntax d)
            return Type(d.Identifier.Text);
        else if (currentType != null && member is FieldDeclarationSyntax f)
            return Field(currentType, f.Declaration.Variables.First().Identifier.Text);
        else if (currentType != null && member is PropertyDeclarationSyntax p)
            return Property(currentType, p.Identifier.Text);
        else if (currentType != null && member is MethodDeclarationSyntax m)
        {
            var paramTypes = m.ParameterList.Parameters
                .Select(p => NormalizeType(p.Type))
                .ToArray();
            return Method(currentType, m.Identifier.Text, paramTypes);
        }
        else if (currentType != null && member is OperatorDeclarationSyntax op)
        {
            var paramTypes = op.ParameterList.Parameters
                .Select(p => NormalizeType(p.Type))
                .ToArray();
            return Method(currentType, GetOperatorMetadataName(op), paramTypes);
        }
        else if (currentType != null && member is ConversionOperatorDeclarationSyntax cop)
        {
            var paramTypes = cop.ParameterList.Parameters
                .Select(p => NormalizeType(p.Type))
                .ToArray();
            return Method(currentType, cop.ImplicitOrExplicitKeyword.IsKind(SyntaxKind.ImplicitKeyword) ? "op_Implicit" : "op_Explicit", paramTypes);
        }
        else
            return null;
    }

    private static string GetOperatorMetadataName(OperatorDeclarationSyntax op) =>
        op.OperatorToken.Kind() switch
        {
            SyntaxKind.PlusToken => op.ParameterList.Parameters.Count == 1 ? "op_UnaryPlus" : "op_Addition",
            SyntaxKind.MinusToken => op.ParameterList.Parameters.Count == 1 ? "op_UnaryNegation" : "op_Subtraction",
            SyntaxKind.AsteriskToken => "op_Multiply",
            SyntaxKind.SlashToken => "op_Division",
            SyntaxKind.PercentToken => "op_Modulus",
            SyntaxKind.AmpersandToken => "op_BitwiseAnd",
            SyntaxKind.BarToken => "op_BitwiseOr",
            SyntaxKind.CaretToken => "op_ExclusiveOr",
            SyntaxKind.LessThanLessThanToken => "op_LeftShift",
            SyntaxKind.GreaterThanGreaterThanToken => "op_RightShift",
            SyntaxKind.EqualsEqualsToken => "op_Equality",
            SyntaxKind.ExclamationEqualsToken => "op_Inequality",
            SyntaxKind.LessThanToken => "op_LessThan",
            SyntaxKind.GreaterThanToken => "op_GreaterThan",
            SyntaxKind.LessThanEqualsToken => "op_LessThanOrEqual",
            SyntaxKind.GreaterThanEqualsToken => "op_GreaterThanOrEqual",
            SyntaxKind.PlusPlusToken => "op_Increment",
            SyntaxKind.MinusMinusToken => "op_Decrement",
            SyntaxKind.ExclamationToken => "op_LogicalNot",
            SyntaxKind.TildeToken => "op_OnesComplement",
            SyntaxKind.TrueKeyword => "op_True",
            SyntaxKind.FalseKeyword => "op_False",
            _ => "op_Unknown",// fallback; unlikely but ensures a key if docs exist
        };

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

        // For enum declarations: require 'public'
        if (member is EnumDeclarationSyntax enumDecl)
            return enumDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));

        // Enum members (EnumMemberDeclarationSyntax) are always effectively public in a public enum
        if (member is EnumMemberDeclarationSyntax enumMemberDecl)
            return enumMemberDecl.Parent is EnumDeclarationSyntax parentDecl && parentDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));

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
