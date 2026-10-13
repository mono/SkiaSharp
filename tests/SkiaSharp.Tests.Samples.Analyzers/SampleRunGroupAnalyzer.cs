// Based on MAUI's missing/multiple test-category analyzer (55c13777), under the MIT license.
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SkiaSharp.Tests.Samples.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SampleRunGroupAnalyzer : DiagnosticAnalyzer
{
    private const string AttributeName = "SkiaSharp.Tests.Samples.Utils.RunGroupAttribute";
    private const string EnumName = "SkiaSharp.Tests.Samples.Utils.SampleRunGroup";

    private static readonly DiagnosticDescriptor Missing = Rule("SKS001", "Test '{0}' requires one typed RunGroup");
    private static readonly DiagnosticDescriptor Multiple = Rule("SKS002", "Test '{0}' has multiple effective RunGroup attributes");
    private static readonly DiagnosticDescriptor Invalid = Rule("SKS003", "Test '{0}' has an undefined SampleRunGroup value");
    private static readonly DiagnosticDescriptor Raw = Rule("SKS004", "Test '{0}' must use typed RunGroup, not a raw trait");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Missing, Multiple, Invalid, Raw);

    private static DiagnosticDescriptor Rule(string id, string message) =>
        new(id, "Samples require one valid execution group", message, "Testing",
            DiagnosticSeverity.Error, isEnabledByDefault: true);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            var fact = start.Compilation.GetTypeByMetadataName("Xunit.FactAttribute");
            var group = start.Compilation.GetTypeByMetadataName(AttributeName);
            var enumType = start.Compilation.GetTypeByMetadataName(EnumName);
            var trait = start.Compilation.GetTypeByMetadataName("Xunit.TraitAttribute");
            if (fact is null || group is null || enumType is null)
                return;
            var values = enumType.GetMembers().OfType<IFieldSymbol>()
                .Where(field => field.HasConstantValue).Select(field => field.ConstantValue).ToImmutableHashSet();
            start.RegisterSymbolAction(symbol =>
            {
                var type = (INamedTypeSymbol)symbol.Symbol;
                if (type.TypeKind != TypeKind.Class || (type.IsAbstract && !type.IsStatic))
                    return;

                foreach (var method in TestMethods(type))
                {
                    var methodAttributes = InheritedAttributes(MethodAttributeLevels(method)).ToArray();
                    if (!methodAttributes.Any(attribute => DerivesFrom(attribute.AttributeClass, fact)))
                        continue;

                    var attributes = methodAttributes
                        .Concat(InheritedAttributes(TypeAttributeLevels(type)))
                        .Concat(type.ContainingAssembly.GetAttributes()).ToArray();
                    var groups = attributes.Where(attribute =>
                        SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, group)).ToArray();
                    if (groups.Length == 0)
                        Report(symbol, Missing, method, type);
                    else if (groups.Length > 1)
                        Report(symbol, Multiple, method, type);
                    if (groups.Any(attribute => attribute.ConstructorArguments.Length != 1 ||
                        !SymbolEqualityComparer.Default.Equals(attribute.ConstructorArguments[0].Type, enumType) ||
                        !values.Contains(attribute.ConstructorArguments[0].Value)))
                        Report(symbol, Invalid, method, type);
                    if (attributes.Any(attribute => DerivesFrom(attribute.AttributeClass, trait) &&
                        attribute.ConstructorArguments.Length > 0 &&
                        attribute.ConstructorArguments[0].Value is string name &&
                        string.Equals(name, "RunGroup", System.StringComparison.OrdinalIgnoreCase)))
                        Report(symbol, Raw, method, type);
                }
            }, SymbolKind.NamedType);
        });
    }

    private static IEnumerable<IMethodSymbol> TestMethods(INamedTypeSymbol type)
    {
        var overrides = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        for (var current = type; current is not null; current = current.BaseType)
        {
            foreach (var method in current.GetMembers().OfType<IMethodSymbol>())
            {
                if (method.MethodKind != MethodKind.Ordinary)
                    continue;
                for (var parent = method.OverriddenMethod; parent is not null; parent = parent.OverriddenMethod)
                    overrides.Add(parent);
                if (overrides.Contains(method) ||
                    (!SymbolEqualityComparer.Default.Equals(current, type) &&
                        (method.DeclaredAccessibility == Accessibility.Private ||
                         (method.IsStatic && method.DeclaredAccessibility != Accessibility.Public &&
                          method.DeclaredAccessibility != Accessibility.Protected &&
                          method.DeclaredAccessibility != Accessibility.ProtectedOrInternal))))
                    continue;
                yield return method;
            }
        }
        foreach (var method in type.AllInterfaces.SelectMany(@interface => @interface.GetMembers().OfType<IMethodSymbol>()))
            if (method.MethodKind == MethodKind.Ordinary)
                yield return method;
    }

    private static IEnumerable<ImmutableArray<AttributeData>> MethodAttributeLevels(IMethodSymbol method)
    {
        for (var current = method; current is not null; current = current.OverriddenMethod)
            yield return current.GetAttributes();
    }

    private static IEnumerable<ImmutableArray<AttributeData>> TypeAttributeLevels(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.BaseType)
            yield return current.GetAttributes();
    }

    private static IEnumerable<AttributeData> InheritedAttributes(
        IEnumerable<ImmutableArray<AttributeData>> levels)
    {
        // xUnit respects AttributeUsage separately on the method and class inheritance chains.
        var seen = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        var topLevel = true;
        foreach (var attributes in levels)
        {
            foreach (var attribute in attributes)
            {
                if (attribute.AttributeClass is not { } attributeType)
                    continue;
                var usage = AttributeUsage(attributeType);
                if ((!topLevel && !usage.Inherited) || (!seen.Add(attributeType) && !usage.AllowMultiple))
                    continue;
                yield return attribute;
            }
            topLevel = false;
        }
    }

    private static (bool Inherited, bool AllowMultiple) AttributeUsage(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            var usage = current.GetAttributes().FirstOrDefault(attribute =>
                attribute.AttributeClass?.ToDisplayString() == "System.AttributeUsageAttribute");
            if (usage is null)
                continue;
            var inherited = true;
            var multiple = false;
            foreach (var argument in usage.NamedArguments)
            {
                if (argument.Key == "Inherited" && argument.Value.Value is bool inherit)
                    inherited = inherit;
                if (argument.Key == "AllowMultiple" && argument.Value.Value is bool allow)
                    multiple = allow;
            }
            return (inherited, multiple);
        }
        return (true, false);
    }

    private static bool DerivesFrom(INamedTypeSymbol? type, INamedTypeSymbol? expected)
    {
        if (expected is null)
            return false;
        for (; type is not null; type = type.BaseType)
            if (SymbolEqualityComparer.Default.Equals(type, expected))
                return true;
        return false;
    }

    private static void Report(SymbolAnalysisContext context, DiagnosticDescriptor rule,
        IMethodSymbol method, INamedTypeSymbol type) =>
        context.ReportDiagnostic(Diagnostic.Create(rule,
            SymbolEqualityComparer.Default.Equals(method.ContainingType, type) ? method.Locations[0] : type.Locations[0],
            method.Name));
}
