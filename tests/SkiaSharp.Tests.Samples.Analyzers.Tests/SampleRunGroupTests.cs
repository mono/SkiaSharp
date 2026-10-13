using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SkiaSharp.Tests.Samples.Analyzers;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples.Analyzers.Tests;

[RunGroup(SampleRunGroup.Build)]
public class SampleRunGroupTests
{
    [Theory]
    [InlineData("[Fact] public void Test() {}", "SKS001")]
    [InlineData("[Theory] public void Test(int value) {}", "SKS001")]
    [InlineData("[DerivedFact] public void Test() {}", "SKS001")]
    [InlineData("[DerivedTheory] public void Test(int value) {}", "SKS001")]
    [InlineData("[SkippableFact] public void Test() {}", "SKS001")]
    [InlineData("[RunGroup((SampleRunGroup)99)] [Fact] public void Test() {}", "SKS003")]
    [InlineData("[Trait(\"RunGroup\", \"Build\")] [Fact] public void Test() {}", "SKS001,SKS004")]
    [InlineData("[RunGroup(SampleRunGroup.Build)] [Trait(\"rungroup\", \"Host\")] [Fact] public void Test() {}", "SKS004")]
    [InlineData("[RunGroup(SampleRunGroup.Build)] [Fact] public void Test() {}", "")]
    [InlineData("[RunGroup(SampleRunGroup.Host)] [DerivedTheory] public void Test(int value) {}", "")]
    [InlineData("[RunGroup(SampleRunGroup.Build)] [Trait(\"Category\", \"Infrastructure\")] [Fact] public void Test() {}", "")]
    [InlineData("public void Helper() {}", "")]
    public Task MethodContract(string method, string expected) =>
        Check($"public class Tests {{ {method} }}", expected);

    [Theory]
    [InlineData("[RunGroup(SampleRunGroup.Build)] public class Tests { [Fact] public void Test() {} }", "")]
    [InlineData("[RunGroup((SampleRunGroup)99)] public class Tests { [Fact] public void Test() {} }", "SKS003")]
    [InlineData("[RunGroup(SampleRunGroup.Build)] public class Base {} public class Tests : Base { [Fact] public void Test() {} }", "")]
    [InlineData("[RunGroup(SampleRunGroup.Build)] public class Tests { [RunGroup(SampleRunGroup.Host)] [Fact] public void Test() {} }", "SKS002")]
    [InlineData("[RunGroup(SampleRunGroup.Build)] public class Base {} [RunGroup(SampleRunGroup.Host)] public class Tests : Base { [Fact] public void Test() {} }", "")]
    [InlineData("[Trait(\"RunGroup\", \"Host\")] public class Base {} public class Tests : Base { [RunGroup(SampleRunGroup.Build)] [Fact] public void Test() {} }", "SKS004")]
    public Task ClassContract(string source, string expected) => Check(source, expected);

    [Theory]
    [InlineData("public abstract class Base { [Fact] public void Test() {} } [RunGroup(SampleRunGroup.Build)] public class Tests : Base {}", "")]
    [InlineData("public abstract class Base { [Fact] public void Test() {} } public class Tests : Base {}", "SKS001")]
    [InlineData("public abstract class Base { [RunGroup(SampleRunGroup.Host)] [Fact] public void Test() {} } [RunGroup(SampleRunGroup.Build)] public class Tests : Base {}", "SKS002")]
    [InlineData("public abstract class Base { [Fact] [Trait(\"RunGroup\", \"Build\")] public void Test() {} } [RunGroup(SampleRunGroup.Build)] public class Tests : Base {}", "SKS004")]
    [InlineData("public abstract class Base { [Fact] public virtual void Test() {} } [RunGroup(SampleRunGroup.Build)] public class Tests : Base { public override void Test() {} }", "")]
    [InlineData("public abstract class Base { [Fact] public virtual void Test() {} } public class Tests : Base { public override void Test() {} }", "SKS001")]
    [InlineData("public abstract class Base { [Fact] [RunGroup(SampleRunGroup.Build)] public virtual void Test() {} } public class Tests : Base { public override void Test() {} }", "")]
    [InlineData("public abstract class Base { [Fact] [RunGroup(SampleRunGroup.Build)] public virtual void Test() {} } public class Tests : Base { [RunGroup(SampleRunGroup.Host)] public override void Test() {} }", "")]
    [InlineData("[RunGroup((SampleRunGroup)99)] public abstract class Base { [Fact] public void Test() {} } [RunGroup(SampleRunGroup.Build)] public class Tests : Base {}", "")]
    [InlineData("public abstract class Base { [NonInheritedFact] public virtual void Test() {} } public class Tests : Base { public override void Test() {} }", "")]
    [InlineData("public abstract class Base { [Fact] [Trait(\"RunGroup\", \"Build\")] public virtual void Test() {} } [RunGroup(SampleRunGroup.Build)] public class Tests : Base { public override void Test() {} }", "SKS004")]
    [InlineData("public abstract class Base { [Fact] private void Test() {} } public class Tests : Base {}", "")]
    public Task InheritanceContract(string source, string expected) => Check(source, expected);

    [Theory]
    [InlineData("public interface ITests { [Fact] public void Test() {} } public class Tests : ITests {}", "SKS001")]
    [InlineData("public interface ITests { [Fact] [RunGroup(SampleRunGroup.Host)] public void Test() {} } public class Tests : ITests {}", "")]
    [InlineData("public interface ITests { [Fact] public void Test() {} } [RunGroup(SampleRunGroup.Build)] public class Tests : ITests {}", "")]
    [InlineData("public interface ITests { [Fact] [Trait(\"RunGroup\", \"Build\")] public void Test() {} } [RunGroup(SampleRunGroup.Build)] public class Tests : ITests {}", "SKS004")]
    public Task InterfaceContract(string source, string expected) => Check(source, expected);

    [Theory]
    [InlineData(SampleRunGroup.Build)]
    [InlineData(SampleRunGroup.Host)]
    [InlineData(SampleRunGroup.Docker)]
    [InlineData(SampleRunGroup.Android)]
    [InlineData(SampleRunGroup.Ios)]
    [InlineData(SampleRunGroup.Mac)]
    [InlineData(SampleRunGroup.Windows)]
    public void EmitsStandardTrait(SampleRunGroup group) =>
        Assert.Equal(new KeyValuePair<string, string>("RunGroup", group.ToString()),
            Assert.Single(new RunGroupAttribute(group).GetTraits()));

    [Fact]
    public void UndefinedValueFails() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new RunGroupAttribute((SampleRunGroup)99));

    private static async Task Check(string tests, string expected)
    {
        const string preamble = """
            using System;
            using Xunit;
            using SkiaSharp.Tests.Samples.Utils;
            public class DerivedFactAttribute : FactAttribute {}
            public class DerivedTheoryAttribute : TheoryAttribute {}
            public class SkippableFactAttribute : FactAttribute {}
            [AttributeUsage(AttributeTargets.Method, Inherited = false)]
            public class NonInheritedFactAttribute : FactAttribute {}
            """;
        var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Append(typeof(FactAttribute).Assembly.Location)
            .Append(typeof(RunGroupAttribute).Assembly.Location)
            .Distinct();
        var compilation = CSharpCompilation.Create("Contract",
            [CSharpSyntaxTree.ParseText(preamble + tests)],
            paths.Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.Empty(compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        var diagnostics = await compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(new SampleRunGroupAnalyzer())).GetAnalyzerDiagnosticsAsync();
        Assert.All(diagnostics, diagnostic => Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity));
        Assert.Equal(expected, string.Join(",", diagnostics.Select(diagnostic => diagnostic.Id).Order()));
    }
}
